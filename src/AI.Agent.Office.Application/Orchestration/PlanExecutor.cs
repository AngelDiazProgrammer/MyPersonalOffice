using AI.Agent.Office.Application.Events;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Application.Validators;
using AI.Agent.Office.Domain.Entities;
using AI.Agent.Office.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Application.Orchestration;

using Agent = AI.Agent.Office.Domain.Entities.Agent;

public sealed record PlanContext(Guid ConversationId, string UserRequest, IReadOnlyList<OfficeDocument> Documents);

/// <summary>
/// Runs a validated task plan: independent tasks in parallel, dependent tasks after their prerequisites,
/// bounded by the shared <see cref="IExecutionLimiter"/>, with per-task timeout, error isolation and cancellation.
/// </summary>
public sealed class PlanExecutor
{
    private readonly ITaskStore _tasks;
    private readonly IAgentRunner _runner;
    private readonly IExecutionLimiter _limiter;
    private readonly IAgentStateService _state;
    private readonly IOfficeEventService _events;
    private readonly IAgentRegistry _registry;
    private readonly OrchestrationOptions _options;
    private readonly ILogger<PlanExecutor> _logger;

    public PlanExecutor(
        ITaskStore tasks,
        IAgentRunner runner,
        IExecutionLimiter limiter,
        IAgentStateService state,
        IOfficeEventService events,
        IAgentRegistry registry,
        IOptions<OrchestrationOptions> options,
        ILogger<PlanExecutor> logger)
    {
        _tasks = tasks;
        _runner = runner;
        _limiter = limiter;
        _state = state;
        _events = events;
        _registry = registry;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AgentTask>> ExecuteAsync(
        PlanContext context, IReadOnlyList<AgentTask> plan, CancellationToken cancellationToken)
    {
        var ordered = PlanGraph.TopologicalOrder(plan, t => t.Key, t => t.DependsOn)
            ?? throw new InvalidOperationException("El plan contiene dependencias inválidas.");

        var keyToId = plan.ToDictionary(t => t.Key, t => t.Id, StringComparer.OrdinalIgnoreCase);
        var running = new Dictionary<string, Task>(StringComparer.OrdinalIgnoreCase);
        foreach (var task in ordered)
        {
            var dependencies = task.DependsOn.Select(k => running[k]).ToArray();
            running[task.Key] = RunOneAsync(context, task, keyToId, dependencies, cancellationToken);
        }

        await Task.WhenAll(running.Values).ConfigureAwait(false);
        return plan.Select(t => _tasks.Get(t.Id) ?? t).ToList();
    }

    private async Task RunOneAsync(PlanContext context, AgentTask planned, IReadOnlyDictionary<string, Guid> keyToId,
        Task[] dependencies, CancellationToken requestToken)
    {
        var taskId = planned.Id;
        var agent = _registry.Get(planned.AgentId);
        var agentName = agent?.Name ?? planned.AgentId;

        using var taskCts = CancellationTokenSource.CreateLinkedTokenSource(requestToken);
        _tasks.RegisterCancellation(taskId, taskCts);
        IAsyncDisposable? lease = null;
        var finalStatus = TaskStatus.Failed;

        try
        {
            _tasks.Update(taskId, t => t.Status = TaskStatus.Assigned);
            var busy = agent is { CurrentTaskId: not null } && agent.CurrentTaskId != taskId
                       && agent.Status is not (AgentStatus.Idle or AgentStatus.Completed or AgentStatus.Error);
            await _state.SetAsync(planned.AgentId,
                busy ? null : (dependencies.Length > 0 ? AgentStatus.Waiting : AgentStatus.Thinking),
                OfficeEventType.AgentTaskAssigned,
                $"Tarea asignada a {agentName}: {planned.Title}",
                busy ? null : taskId, busy ? null : planned.Title,
                data: TaskData(TaskStatus.Assigned)).ConfigureAwait(false);

            await Task.WhenAll(dependencies).ConfigureAwait(false); // dependency tasks never throw

            var dependencyTasks = planned.DependsOn.Select(k => _tasks.Get(keyToId[k])).ToList();
            var failedDependency = dependencyTasks.FirstOrDefault(d => d is null || d.Status != TaskStatus.Completed);
            if (failedDependency is not null || taskCts.IsCancellationRequested)
            {
                var reason = taskCts.IsCancellationRequested
                    ? "Cancelada antes de iniciar."
                    : $"No se ejecutó porque la dependencia '{failedDependency?.Key}' no se completó.";
                finalStatus = TaskStatus.Cancelled;
                _tasks.Update(taskId, t => t.ErrorMessage = reason);
                return;
            }

            lease = await _limiter.EnterAsync(planned.AgentId, taskCts.Token).ConfigureAwait(false);
            taskCts.CancelAfter(TimeSpan.FromSeconds(_options.TaskTimeoutSeconds));

            _tasks.Update(taskId, t => { t.Status = TaskStatus.Running; t.StartedAt = DateTimeOffset.UtcNow; });

            var current = _tasks.Get(taskId)!;
            var dependencyResults = dependencyTasks
                .Select(d => (d!, _registry.Get(d!.AgentId)?.Name ?? d.AgentId))
                .ToList();
                        var prompt = PromptBuilder.BuildAgentPrompt(
                context.UserRequest, current, dependencyResults,
                planned.IncludeDocuments ? context.Documents : Array.Empty<OfficeDocument>(),
                _options.MaxDocumentCharsPerTask);

            var result = await _runner.RunAsync(current, prompt, taskCts.Token).ConfigureAwait(false);

            _tasks.Update(taskId, t => { t.Result = result; });
            finalStatus = TaskStatus.Completed;
            await _state.SetAsync(planned.AgentId, AgentStatus.Completed, OfficeEventType.AgentCompleted,
                $"{agentName} terminó: {planned.Title}", taskId, planned.Title,
                data: TaskData(TaskStatus.Completed)).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            if (requestToken.IsCancellationRequested)
            {
                finalStatus = TaskStatus.Cancelled;
                _tasks.Update(taskId, t => t.ErrorMessage = "La solicitud fue cancelada.");
            }
            else if (_tasks.WasCancelledByUser(taskId))
            {
                finalStatus = TaskStatus.Cancelled;
                _tasks.Update(taskId, t => t.ErrorMessage = "Tarea cancelada por el usuario.");
            }
            else
            {
                finalStatus = TaskStatus.Failed;
                var message = $"La tarea superó el tiempo máximo de {_options.TaskTimeoutSeconds} s.";
                _tasks.Update(taskId, t => t.ErrorMessage = message);
                await _state.SetAsync(planned.AgentId, AgentStatus.Error, OfficeEventType.AgentError,
                    $"{agentName}: {message}", taskId, planned.Title, data: TaskData(TaskStatus.Failed)).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            finalStatus = TaskStatus.Failed;
            var message = ex is LlmException ? ex.Message : "Ocurrió un error inesperado al ejecutar la tarea.";
            if (ex is not LlmException) _logger.LogError(ex, "Error ejecutando la tarea {TaskId} del agente {AgentId}", taskId, planned.AgentId);
            _tasks.Update(taskId, t => t.ErrorMessage = message);
            await _state.SetAsync(planned.AgentId, AgentStatus.Error, OfficeEventType.AgentError,
                $"{agentName} falló: {message}", taskId, planned.Title, data: TaskData(TaskStatus.Failed)).ConfigureAwait(false);
        }
        finally
        {
            if (lease is not null) await lease.DisposeAsync().ConfigureAwait(false);
            _tasks.UnregisterCancellation(taskId);
            _tasks.Update(taskId, t => { t.Status = finalStatus; t.CompletedAt = DateTimeOffset.UtcNow; });

            if (finalStatus == TaskStatus.Cancelled && _registry.Get(planned.AgentId)?.CurrentTaskId == taskId)
            {
                await _state.SetAsync(planned.AgentId, AgentStatus.Idle, OfficeEventType.AgentProgress,
                    $"{agentName}: tarea cancelada ({planned.Title}).", taskId, clearTask: true,
                    data: TaskData(TaskStatus.Cancelled)).ConfigureAwait(false);
            }

            await _events.EmitAsync(OfficeEventType.TaskCompleted,
                $"Tarea {StatusText(finalStatus)}: {planned.Title}", planned.AgentId, taskId,
                TaskData(finalStatus)).ConfigureAwait(false);

            _state.ReturnToIdleAfter(planned.AgentId, TimeSpan.FromSeconds(_options.CompletedDisplaySeconds));
        }
    }

    private static Dictionary<string, string?> TaskData(TaskStatus status) =>
        new() { [EventDataKeys.TaskStatus] = status.ToString() };

    private static string StatusText(TaskStatus status) => status switch
    {
        TaskStatus.Completed => "completada",
        TaskStatus.Failed => "fallida",
        TaskStatus.Cancelled => "cancelada",
        _ => status.ToString().ToLowerInvariant()
    };
}
