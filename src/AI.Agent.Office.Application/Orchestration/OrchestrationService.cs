using System.Text;
using AI.Agent.Office.Application.Agents;
using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Events;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Application.Validators;
using AI.Agent.Office.Domain.Entities;
using AI.Agent.Office.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Application.Orchestration;

/// <summary>
/// Core of the office: asks the Orchestrator (Claude) for a structured decision, validates it,
/// runs the plan through the specialist agents and consolidates the final answer.
/// </summary>
public sealed class OrchestrationService : IOrchestrationService
{
    private const int MaxPlanningAttempts = 2;

    private readonly IAnthropicService _anthropic;
    private readonly IAgentConfigurationService _configuration;
    private readonly IConversationStore _conversations;
    private readonly ITaskStore _tasks;
    private readonly IDocumentStore _documents;
    private readonly IAgentStateService _state;
    private readonly IOfficeEventService _events;
    private readonly PlanExecutor _executor;
    private readonly OrchestrationOptions _options;
    private readonly ILogger<OrchestrationService> _logger;

    public OrchestrationService(
        IAnthropicService anthropic,
        IAgentConfigurationService configuration,
        IConversationStore conversations,
        ITaskStore tasks,
        IDocumentStore documents,
        IAgentStateService state,
        IOfficeEventService events,
        PlanExecutor executor,
        IOptions<OrchestrationOptions> options,
        ILogger<OrchestrationService> logger)
    {
        _anthropic = anthropic;
        _configuration = configuration;
        _conversations = conversations;
        _tasks = tasks;
        _documents = documents;
        _state = state;
        _events = events;
        _executor = executor;
        _options = options.Value;
        _logger = logger;
    }

    public bool CancelConversationRun(Guid conversationId) => _conversations.CancelRun(conversationId);

    public bool CancelTask(Guid taskId) => _tasks.RequestCancel(taskId);

    public async Task<ChatResponseDto> HandleMessageAsync(ChatRequestDto request, CancellationToken cancellationToken)
    {
        var errors = ChatRequestValidator.Validate(request, _options.MaxMessageChars);
        if (errors.Count > 0) throw new OfficeValidationException(errors);

        var orchestrator = _configuration.GetDefinitions().FirstOrDefault(d => d.IsOrchestrator)
            ?? throw new InvalidOperationException("No hay un agente orquestador configurado.");

        var attached = ResolveDocuments(request.DocumentIds);
        var conversation = _conversations.GetOrCreate(request.ConversationId);
        var userText = request.Message.Trim();

        using var runCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _conversations.RegisterRun(conversation.Id, runCts);

        conversation.Add(new ChatMessage { Role = ChatRole.User, Content = userText });
        var convData = new Dictionary<string, string?> { [EventDataKeys.ConversationId] = conversation.Id.ToString() };

        await _events.EmitAsync(OfficeEventType.OrchestrationStarted,
            $"Nueva solicitud: {Shorten(userText, 80)}", orchestrator.Id, data: convData).ConfigureAwait(false);

        var reply = string.Empty;
        string? planMessage = null;
        var delegated = false;
        var isError = false;
        IReadOnlyList<AgentTask> executed = Array.Empty<AgentTask>();

        try
        {
            await _state.SetAsync(orchestrator.Id, AgentStatus.Thinking, OfficeEventType.AgentThinking,
                "El Orquestador analiza la solicitud.", taskTitle: Shorten(userText, 60), data: convData).ConfigureAwait(false);

            var team = _configuration.GetDefinitions().Where(d => !d.IsOrchestrator).ToList();
            var decision = await PlanAsync(orchestrator, team, conversation, attached, runCts.Token).ConfigureAwait(false);

            if (decision.Action == DecisionActions.Respond)
            {
                reply = decision.Message.Trim();
            }
            else
            {
                delegated = true;
                planMessage = decision.Message.Trim();
                var plan = CreateTasks(conversation.Id, decision, team);

                await _state.SetAsync(orchestrator.Id, AgentStatus.Waiting, OfficeEventType.AgentProgress,
                    $"El Orquestador delega {plan.Count} tarea(s): {planMessage}", data: TaskCountData(convData, plan.Count)).ConfigureAwait(false);

                var context = new PlanContext(conversation.Id, userText, attached);
                executed = await _executor.ExecuteAsync(context, plan, runCts.Token).ConfigureAwait(false);

                await _state.SetAsync(orchestrator.Id, AgentStatus.Reviewing, OfficeEventType.AgentProgress,
                    "El Orquestador consolida los resultados.", data: convData).ConfigureAwait(false);

                reply = await ConsolidateAsync(orchestrator, team, conversation, userText, executed, runCts.Token).ConfigureAwait(false);
            }

            await _state.SetAsync(orchestrator.Id, AgentStatus.Completed, OfficeEventType.AgentCompleted,
                "El Orquestador entregó la respuesta.", clearTask: true, data: convData).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested || runCts.IsCancellationRequested)
        {
            isError = true;
            reply = "La solicitud fue cancelada.";
            await _state.SetAsync(orchestrator.Id, AgentStatus.Idle, OfficeEventType.AgentProgress,
                "Solicitud cancelada.", clearTask: true, data: convData).ConfigureAwait(false);
        }
        catch (LlmException ex)
        {
            isError = true;
            reply = $"No pude completar la solicitud: {ex.Message}";
            _logger.LogWarning("Falla de Anthropic durante la orquestación: {Message}", ex.Message);
            await _state.SetAsync(orchestrator.Id, AgentStatus.Error, OfficeEventType.AgentError,
                $"Error del Orquestador: {ex.Message}", clearTask: true, data: convData).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            isError = true;
            reply = "Ocurrió un error inesperado al procesar la solicitud. Revisa los registros del servidor.";
            _logger.LogError(ex, "Error inesperado en la orquestación de la conversación {ConversationId}", conversation.Id);
            await _state.SetAsync(orchestrator.Id, AgentStatus.Error, OfficeEventType.AgentError,
                "Error inesperado del Orquestador.", clearTask: true, data: convData).ConfigureAwait(false);
        }
        finally
        {
            _conversations.UnregisterRun(conversation.Id, runCts);
        }

        conversation.Add(new ChatMessage { Role = ChatRole.Assistant, Content = reply });
        await _events.EmitAsync(OfficeEventType.OrchestrationCompleted,
            isError ? "Solicitud finalizada con errores." : "Solicitud completada.", orchestrator.Id,
            data: new Dictionary<string, string?>(convData) { [EventDataKeys.Delegated] = delegated.ToString() }).ConfigureAwait(false);

        _state.ReturnToIdleAfter(orchestrator.Id, TimeSpan.FromSeconds(_options.CompletedDisplaySeconds));

        return new ChatResponseDto(conversation.Id, reply, delegated, isError, planMessage,
            executed.Select(t => t.ToDto()).ToArray());
    }

    private async Task<OrchestratorDecision> PlanAsync(
        AgentDefinition orchestrator, IReadOnlyList<AgentDefinition> team, Conversation conversation,
        IReadOnlyList<OfficeDocument> documents, CancellationToken cancellationToken)
    {
        var delegableIds = team.Select(t => t.Id).ToList();
        var spec = DecisionSchema.Build(delegableIds, _options.MaxTasksPerRequest);
        var system = PromptBuilder.BuildOrchestratorSystem(orchestrator, team, _options.MaxTasksPerRequest, documents.Count > 0);
        var messages = BuildHistory(conversation, documents);

        IReadOnlyList<string> problems = Array.Empty<string>();
        for (var attempt = 1; attempt <= MaxPlanningAttempts; attempt++)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.PlanningTimeoutSeconds));

            var response = await _anthropic.SendAsync(new LlmRequest(
                system, messages, _options.PlanningMaxTokens, orchestrator.ModelSettings.Model,
                orchestrator.ModelSettings.Temperature, spec, "orchestrator:plan"), timeout.Token).ConfigureAwait(false);

            if (response.StructuredJson is null)
            {
                problems = new[] { "No se recibió una decisión estructurada." };
            }
            else
            {
                var decision = OrchestratorDecision.Parse(response.StructuredJson);
                problems = OrchestratorDecisionValidator.Validate(decision, delegableIds, _options.MaxTasksPerRequest);
                if (problems.Count == 0) return decision!;
            }

            _logger.LogWarning("Decisión del Orquestador inválida (intento {Attempt}): {Problems}", attempt, string.Join(" | ", problems));
            messages = messages
                .Concat(new[]
                {
                    new LlmMessage("assistant", response.StructuredJson ?? "(sin decisión)"),
                    new LlmMessage("user", "Tu decisión no es válida: " + string.Join(" ", problems) + " Corrígela y vuelve a llamar a la herramienta.")
                })
                .ToList();
        }

        // Safe fallback instead of failing the whole request.
        _logger.LogError("El Orquestador no produjo una decisión válida: {Problems}", string.Join(" | ", problems));
        return new OrchestratorDecision
        {
            Action = DecisionActions.Respond,
            Message = "No pude planificar la solicitud de forma fiable. ¿Puedes reformularla con más detalle?"
        };
    }

    private IReadOnlyList<AgentTask> CreateTasks(Guid conversationId, OrchestratorDecision decision, IReadOnlyList<AgentDefinition> team)
    {
        var created = new List<AgentTask>();
        foreach (var planned in decision.Tasks)
        {
            var definition = team.First(t => string.Equals(t.Id, planned.AgentId, StringComparison.OrdinalIgnoreCase));
            var task = new AgentTask
            {
                ConversationId = conversationId,
                AgentId = definition.Id,
                Key = planned.Id,
                Title = Shorten(planned.Title, 80),
                Description = planned.Task.Trim(),
                DependsOn = planned.DependsOn.Select(d => d.Trim()).ToArray(),
                IncludeDocuments = planned.IncludeDocuments ?? definition.AcceptsDocuments
            };
            _tasks.Add(task);
            created.Add(task);
        }

        foreach (var task in created)
        {
            _ = _events.EmitAsync(OfficeEventType.TaskCreated, $"Tarea creada: {task.Title}", task.AgentId, task.Id,
                new Dictionary<string, string?> { [EventDataKeys.TaskStatus] = TaskStatus.Pending.ToString() });
        }

        return created;
    }

    private async Task<string> ConsolidateAsync(
        AgentDefinition orchestrator, IReadOnlyList<AgentDefinition> team, Conversation conversation, string userText,
        IReadOnlyList<AgentTask> executed, CancellationToken cancellationToken)
    {
        var results = executed.Select(t => (t, team.FirstOrDefault(a => a.Id == t.AgentId)?.Name ?? t.AgentId)).ToList();
        var prompt = PromptBuilder.BuildConsolidationPrompt(userText, results, _options.MaxResultCharsForConsolidation);

        // Earlier turns as context, then one consolidation turn that carries the results.
        var history = BuildHistory(conversation, Array.Empty<OfficeDocument>()).ToList();
        if (history.Count > 0 && history[^1].Role == "user") history.RemoveAt(history.Count - 1);
        history.Add(new LlmMessage("user", prompt));

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.ConsolidationTimeoutSeconds));

            var response = await _anthropic.SendAsync(new LlmRequest(
                PromptBuilder.BuildConsolidationSystem(orchestrator), history, _options.DefaultAgentMaxTokens,
                orchestrator.ModelSettings.Model, orchestrator.ModelSettings.Temperature, null, "orchestrator:consolidate"),
                timeout.Token).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(response.Text)) return response.Text.Trim();
        }
        catch (LlmException ex) when (executed.Any(t => t.Status == TaskStatus.Completed))
        {
            _logger.LogWarning("No se pudo consolidar con Claude ({Message}); se entregan resultados directos.", ex.Message);
        }

        return FallbackConsolidation(results);
    }

    private static string FallbackConsolidation(IReadOnlyList<(AgentTask Task, string AgentName)> results)
    {
        var sb = new StringBuilder("No pude consolidar la respuesta automáticamente; estos son los resultados de cada agente:\n\n");
        foreach (var (task, name) in results)
        {
            sb.AppendLine($"### {name} — {task.Title}");
            sb.AppendLine(task.Status == TaskStatus.Completed ? task.Result : $"_No disponible: {task.ErrorMessage ?? task.Status.ToString()}_");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private IReadOnlyList<LlmMessage> BuildHistory(Conversation conversation, IReadOnlyList<OfficeDocument> documents)
    {
        var recent = conversation.Messages.TakeLast(Math.Max(1, _options.MaxHistoryMessages)).ToList();
        var result = new List<LlmMessage>();

        for (var i = 0; i < recent.Count; i++)
        {
            var role = recent[i].Role == ChatRole.User ? "user" : "assistant";
            var content = recent[i].Content;

            if (i == recent.Count - 1 && role == "user" && documents.Count > 0)
            {
                var sb = new StringBuilder(content).AppendLine().AppendLine().AppendLine("[Documentos adjuntos por el usuario]");
                foreach (var d in documents)
                {
                    sb.AppendLine($"- {d.FileName}: {d.PageCount} páginas, {d.Text.Length} caracteres" +
                                  (d.RequiresOcr ? " — SIN TEXTO EXTRAÍBLE (escaneado, requiere OCR)" : string.Empty));
                }
                content = sb.ToString();
            }

            // The API requires alternating roles starting with "user": merge consecutive same-role turns.
            if (result.Count > 0 && result[^1].Role == role)
                result[^1] = result[^1] with { Content = result[^1].Content + "\n\n" + content };
            else
                result.Add(new LlmMessage(role, content));
        }

        while (result.Count > 0 && result[0].Role != "user") result.RemoveAt(0);
        return result;
    }

    private IReadOnlyList<OfficeDocument> ResolveDocuments(IReadOnlyList<Guid>? ids)
    {
        if (ids is null || ids.Count == 0) return Array.Empty<OfficeDocument>();
        var docs = new List<OfficeDocument>();
        foreach (var id in ids.Distinct())
        {
            var doc = _documents.Get(id)
                ?? throw new OfficeValidationException(new[] { $"El documento {id} no existe o ya no está disponible. Adjúntalo de nuevo." });
            docs.Add(doc);
        }
        return docs;
    }

    private static Dictionary<string, string?> TaskCountData(Dictionary<string, string?> baseData, int count) =>
        new(baseData) { [EventDataKeys.TaskCount] = count.ToString() };

    private static string Shorten(string text, int max) =>
        text.Length <= max ? text : text[..max].TrimEnd() + "…";
}
