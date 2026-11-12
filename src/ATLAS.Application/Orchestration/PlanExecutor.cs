using ATLAS.Application.Events;
using ATLAS.Application.Interfaces;
using ATLAS.Application.Options;
using ATLAS.Application.Validators;
using ATLAS.Domain.Entities;
using ATLAS.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATLAS.Application.Orchestration;

using Agent = ATLAS.Domain.Entities.Agent;

/// <summary>Contexto de una ejecución: conversación, mensaje original del usuario y documentos adjuntos.</summary>
public sealed record PlanContext(Guid ConversationId, string UserRequest, IReadOnlyList<OfficeDocument> Documents);

/// <summary>
/// Ejecuta un plan de tareas ya validado: las tareas independientes corren en paralelo y las
/// dependientes esperan a sus requisitos, acotadas por el <see cref="IExecutionLimiter"/> compartido,
/// con tiempo máximo por tarea, aislamiento de errores y cancelación.
/// </summary>
public sealed class PlanExecutor
{
    #region Dependencias y constructor

    private readonly ITaskStore _tasks;
    private readonly IAgentRunner _runner;
    private readonly IExecutionLimiter _limiter;
    private readonly IAgentStateService _state;
    private readonly IOfficeEventService _events;
    private readonly IAgentRegistry _registry;
    private readonly IWordExportService _word;
    private readonly OrchestrationOptions _options;
    private readonly ILogger<PlanExecutor> _logger;

    public PlanExecutor(
        ITaskStore tasks,
        IAgentRunner runner,
        IExecutionLimiter limiter,
        IAgentStateService state,
        IOfficeEventService events,
        IAgentRegistry registry,
        IWordExportService word,
        IOptions<OrchestrationOptions> options,
        ILogger<PlanExecutor> logger)
    {
        _tasks = tasks;
        _runner = runner;
        _limiter = limiter;
        _state = state;
        _events = events;
        _registry = registry;
        _word = word;
        _options = options.Value;
        _logger = logger;
    }

    #endregion

    #region Ejecución del plan

    /// <summary>
    /// Ejecuta todas las tareas del plan respetando sus dependencias y devuelve las tareas ya terminadas
    /// (completadas, fallidas o canceladas).
    /// </summary>
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

    /// <summary>
    /// Ejecuta UNA tarea: espera sus dependencias, toma el turno del agente, llama al modelo, genera el
    /// Word si corresponde y publica los eventos de estado.
    /// </summary>
    private async Task RunOneAsync(PlanContext context, AgentTask planned, IReadOnlyDictionary<string, Guid> keyToId,
        Task[] dependencies, CancellationToken requestToken)
    {
        var taskId = planned.Id;
        var agent = _registry.Get(planned.AgentId);
        var agentName = agent?.Name ?? planned.AgentId;

        // Los registros de esta tarea (y de las llamadas a proveedores que haga) llevan el id de la tarea y del agente.
        using var taskScope = _logger.BeginScope(new Dictionary<string, object> { ["TaskId"] = taskId, ["AgentId"] = planned.AgentId });

        using var taskCts = CancellationTokenSource.CreateLinkedTokenSource(requestToken);
        _tasks.RegisterCancellation(taskId, taskCts);
        IAsyncDisposable? lease = null;
        var finalStatus = TaskStatus.Failed;

        try
        {
            _tasks.Update(taskId, t => t.Status = TaskStatus.Assigned);
            var busy = agent is { CurrentTaskId: not null } && agent.CurrentTaskId != taskId
                       && agent.Status is not (AgentStatus.Idle or AgentStatus.Completed or AgentStatus.Error);
            var assignedData = TaskData(TaskStatus.Assigned);
            assignedData[EventDataKeys.DependsOnAgents] = string.Join(",",
                planned.DependsOn.Select(k => _tasks.Get(keyToId[k])?.AgentId).Where(a => a is not null));
            await _state.SetAsync(planned.AgentId,
                busy ? null : (dependencies.Length > 0 ? AgentStatus.Waiting : AgentStatus.Thinking),
                OfficeEventType.AgentTaskAssigned,
                $"Tarea asignada a {agentName}: {planned.Title}",
                busy ? null : taskId, busy ? null : planned.Title,
                data: assignedData).ConfigureAwait(false);

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
                _options.MaxDocumentCharsPerTask, _options.MaxDependencyResultChars);

            var containsDocuments = (planned.IncludeDocuments && context.Documents.Count > 0 && !planned.AllowFreeDocuments) || planned.Sensitive;
            AgentRunResult run;
            try
            {
                run = await _runner.RunAsync(current, prompt, containsDocuments, taskCts.Token).ConfigureAwait(false);
            }
            catch (Exception ex) when (planned.RetryWithClaude && ex is not OperationCanceledException)
            {
                // Respaldo (RF-014): si la tarea falla, se reintenta UNA sola vez con Claude antes de darla por fallida.
                _logger.LogWarning("La tarea {TaskId} de {Agent} falló ({Error}); se reintenta una vez con Claude.", taskId, agentName, ex.GetType().Name);
                await _events.EmitAsync(OfficeEventType.OfficeActivityUpdated,
                    $"{agentName} no pudo con las APIs gratuitas; se reintenta con Claude.", planned.AgentId, taskId).ConfigureAwait(false);
                run = await _runner.RunAsync(current, prompt, containsDocuments, taskCts.Token, forceClaude: true).ConfigureAwait(false);
            }
            var result = run.Text;

            _tasks.Update(taskId, t => { t.Result = result; t.Provider = run.Provider; });

            if (agent is { ProducesWord: true } && !planned.SkipDocument)
            {
                try
                {
                    var document = await _word.ExportAsync(planned.Title, result, cancellationToken: taskCts.Token).ConfigureAwait(false);
                    _tasks.Update(taskId, t => { t.DocumentId = document.Id; t.DocumentName = document.FileName; });
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // El resultado en texto se entrega aunque no se haya podido construir el .docx.
                    _logger.LogWarning(ex, "No se pudo generar el Word de la tarea {TaskId}", taskId);
                }
            }
            finalStatus = TaskStatus.Completed;
            var completedData = TaskData(TaskStatus.Completed);
            if (_tasks.Get(taskId)?.DocumentName is { } documentName) completedData[EventDataKeys.Document] = documentName; // drives the printer animation
            await _state.SetAsync(planned.AgentId, AgentStatus.Completed, OfficeEventType.AgentCompleted,
                $"{agentName} terminó: {planned.Title}", taskId, planned.Title,
                data: completedData).ConfigureAwait(false);
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

    #endregion

    #region Datos para eventos

    /// <summary>Datos adicionales de los eventos de una tarea (su estado).</summary>
    private static Dictionary<string, string?> TaskData(TaskStatus status) =>
        new() { [EventDataKeys.TaskStatus] = status.ToString() };

    /// <summary>Texto legible del estado de una tarea.</summary>
    private static string StatusText(TaskStatus status) => status switch
    {
        TaskStatus.Completed => "completada",
        TaskStatus.Failed => "fallida",
        TaskStatus.Cancelled => "cancelada",
        _ => status.ToString().ToLowerInvariant()
    };
    #endregion

}
