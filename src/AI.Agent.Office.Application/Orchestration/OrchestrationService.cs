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
/// Núcleo de la oficina: atiende cada mensaje del chat. Primero consulta la base de conocimiento; si no
/// hay coincidencia, le pide al Orquestador una decisión estructurada, la valida, ejecuta el plan con
/// los agentes especialistas y consolida la respuesta final.
/// </summary>
public sealed class OrchestrationService : IOrchestrationService
{
    // Los dos primeros intentos de planificación usan los proveedores gratuitos; solo el último escala a Claude.
    private const int MaxPlanningAttempts = 3;

    #region Dependencias y constructor

    private readonly IAnthropicService _anthropic;
    private readonly IAgentConfigurationService _configuration;
    private readonly IConversationStore _conversations;
    private readonly ITaskStore _tasks;
    private readonly IDocumentStore _documents;
    private readonly IAgentStateService _state;
    private readonly IOfficeEventService _events;
    private readonly PlanExecutor _executor;
    private readonly KnowledgeWorkflow _knowledge;
    private readonly IWorkspaceService _workspaces;
    private readonly IWordExportService _word;
    private readonly IPdfExportService _pdf;
    private readonly IPendingChoiceStore _pending;
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
        KnowledgeWorkflow knowledge,
        IWorkspaceService workspaces,
        IWordExportService word,
        IPdfExportService pdf,
        IPendingChoiceStore pending,
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
        _knowledge = knowledge;
        _workspaces = workspaces;
        _word = word;
        _pdf = pdf;
        _pending = pending;
        _options = options.Value;
        _logger = logger;
    }

    #endregion

    #region Cancelación

    /// <summary>Cancela la solicitud en curso de una conversación.</summary>
    public bool CancelConversationRun(Guid conversationId) => _conversations.CancelRun(conversationId);

    /// <summary>Cancela una tarea concreta (la pide el usuario desde el panel).</summary>
    public bool CancelTask(Guid taskId) => _tasks.RequestCancel(taskId);

    #endregion

    #region Atención de mensajes

    /// <summary>
    /// Atiende un mensaje del chat de principio a fin: valida, consulta la base de conocimiento, planifica,
    /// ejecuta las tareas y arma la respuesta (con las opciones de solución o los documentos Word/PDF
    /// cuando corresponde). Los errores se convierten en un mensaje para el usuario.
    /// </summary>
    public async Task<ChatResponseDto> HandleMessageAsync(ChatRequestDto request, CancellationToken cancellationToken)
    {
        if (_workspaces.RequiresSelection)
            throw new OfficeValidationException(new[] { "Elige primero una oficina (Desarrollo o Pruebas base de conocimiento)." });

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
        var deliverables = new List<GeneratedDocumentDto>();
        KnowledgeChoiceDto? choice = null;

        try
        {
            await _state.SetAsync(orchestrator.Id, AgentStatus.Thinking, OfficeEventType.AgentThinking,
                "El Orquestador analiza la solicitud.", taskTitle: Shorten(userText, 60), data: convData).ConfigureAwait(false);

            var team = _configuration.GetDefinitions().Where(d => !d.IsOrchestrator && _workspaces.IsAgentActive(d.Id)).ToList();

            // La base de conocimiento es SIEMPRE el primer filtro: se busca con el mensaje y con el texto de los PDF
            // adjuntos. Solo si nada coincide pasa al planificador normal.
            KnowledgePlan? knowledgePlan = null;
            OrchestratorDecision decision;

            #region Respuesta a una pregunta pendiente (elegir solución)

            // Si el caso anterior espera una decisión, este mensaje es la respuesta: botón, letra, "la recomendada",
            // cancelar o una solución propia. Un caso distinto o un PDF nuevo reinicia el flujo normal.
            var pending = _pending.Get(conversation.Id);
            ChoiceResolution? resolution = null;
            if (pending is not null)
            {
                resolution = await _knowledge.ResolveChoiceAsync(userText, request.ChoiceOptionId, attached.Count, pending, runCts.Token).ConfigureAwait(false);
                if (resolution.Kind == ChoiceKind.NewCase)
                {
                    _pending.Clear(conversation.Id);
                    pending = null;
                    resolution = null;
                }
            }

            #endregion

            if (pending is null && !string.IsNullOrWhiteSpace(request.ChoiceOptionId))
            {
                // Se eligió una opción (o se canceló) pero no hay caso pendiente: se responde aquí, sin gastar una llamada a un modelo.
                decision = new OrchestratorDecision { Action = DecisionActions.Respond, Message = "No hay ningún caso esperando una elección. Cuéntame tu situación y la busco en la base de conocimiento." };
            }
            else if (resolution is { Kind: ChoiceKind.Cancel })
            {
                _pending.Clear(conversation.Id);
                decision = new OrchestratorDecision { Action = DecisionActions.Respond, Message = "Entendido, cierro este caso sin aplicar ninguna solución. Cuando quieras, cuéntame otro." };
            }
            else if (resolution is { Kind: ChoiceKind.Ask })
            {
                decision = new OrchestratorDecision { Action = DecisionActions.Respond, Message = _knowledge.AskAgainMessage(pending!) };
            }
            else if (resolution is not null)
            {
                // Eligió una solución o escribió la suya: faltan redactar, dar formato y auditar.
                knowledgePlan = _knowledge.BuildDeliveryPlan(pending!, resolution.Solution, resolution.CustomText);
                _pending.Clear(conversation.Id);
                decision = knowledgePlan.Decision;
            }
            else
            {
                knowledgePlan = await _knowledge.TryPlanAsync(userText, attached, runCts.Token).ConfigureAwait(false);
                if (knowledgePlan is not null)
                {
                    var top = knowledgePlan.Hits[0];
                    await _events.EmitAsync(OfficeEventType.KnowledgeConsulted,
                        $"Base de conocimiento consultada: {top.Entry.Code} — {top.Entry.Title}",
                        knowledgePlan.Decision.Tasks[0].AgentId,
                        data: new Dictionary<string, string?>(convData) { ["code"] = top.Entry.Code, ["title"] = top.Entry.Title }).ConfigureAwait(false);
                    decision = knowledgePlan.Decision;
                }
                else
                {
                    // Los agentes "workflowOnly" son internos: el planificador libre nunca los ve.
                    decision = await PlanAsync(orchestrator, team.Where(t => !t.WorkflowOnly).ToList(), conversation, attached, runCts.Token).ConfigureAwait(false);
                }
            }

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

                #region Fase 1 del caso: presentar las opciones y esperar la decisión

                var awaitingChoice = false;
                if (knowledgePlan is { AwaitsChoice: true })
                {
                    var found = _knowledge.Compose(knowledgePlan, executed);
                    var research = executed.FirstOrDefault(t => string.Equals(t.AgentId, _knowledge.ResearcherAgentId, StringComparison.OrdinalIgnoreCase)
                                                                && t.Status == TaskStatus.Completed)?.Result;

                    if (research is not null && !found.NoMatch && _knowledge.OffersChoice(found.Entry))
                    {
                        // El sistema guarda el caso, muestra las opciones y la siguiente respuesta de la persona decide el camino.
                        _pending.Set(conversation.Id, _knowledge.PendingFrom(knowledgePlan, found, research, userText));
                        reply = _knowledge.RenderChoice(found.Entry, research);
                        choice = _knowledge.BuildChoice(found.Entry);
                        awaitingChoice = true;
                    }
                    else if (research is not null)
                    {
                        // La situación elegida no tiene opciones: se entrega directamente el procedimiento de la base.
                        knowledgePlan = _knowledge.BuildDeliveryPlan(_knowledge.PendingFrom(knowledgePlan, found, research, userText), null, null);
                        var delivery = CreateTasks(conversation.Id, knowledgePlan.Decision, team);
                        var more = await _executor.ExecuteAsync(context, delivery, runCts.Token).ConfigureAwait(false);
                        executed = executed.Concat(more).ToList();
                    }
                }

                #endregion

                if (awaitingChoice)
                {
                    // Nada más que hacer en esta solicitud: la respuesta ya es la pregunta con las opciones.
                }
                else if (knowledgePlan is not null)
                {
                    // El auditor revisó la respuesta; luego el SISTEMA verifica los datos contra la base (tickets, formularios,
                    // contactos y plazos). Si algo parece inventado, el auditor recibe UNA tarea de corrección.
                    var answer = _knowledge.Compose(knowledgePlan, executed);
                    var text = answer.Text;
                    var unverified = string.IsNullOrWhiteSpace(text) ? Array.Empty<string>() : _knowledge.FindUnverified(knowledgePlan, text);
                    var repaired = false;

                    if (unverified.Count > 0)
                    {
                        await _events.EmitAsync(OfficeEventType.OfficeActivityUpdated,
                            $"Verificación automática: datos sin respaldo en la base ({string.Join(", ", unverified)}). Se pide una corrección.",
                            _knowledge.AuditorAgentId).ConfigureAwait(false);

                        var repairTasks = CreateTasks(conversation.Id,
                            new OrchestratorDecision { Action = DecisionActions.Delegate, Tasks = { _knowledge.BuildRepairTask(knowledgePlan, text!, unverified) } }, team);
                        var repairRun = await _executor.ExecuteAsync(context, repairTasks, runCts.Token).ConfigureAwait(false);
                        executed = executed.Concat(repairRun).ToList();

                        if (repairRun[0] is { Status: TaskStatus.Completed, Result: { } repairResult } && KnowledgeWorkflow.ExtractCorrectedText(repairResult) is { } fixedText)
                        {
                            text = fixedText;
                            unverified = _knowledge.FindUnverified(knowledgePlan, text);
                            repaired = true;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(text)) text = _knowledge.NormalizeDocument(answer, text);
                    reply = _knowledge.Render(knowledgePlan, answer, text, unverified, repaired);

                    if (!string.IsNullOrWhiteSpace(text))
                        deliverables.AddRange(await BuildDeliverablesAsync(knowledgePlan, text, executed, runCts.Token).ConfigureAwait(false));
                }
                else if (_options.SkipConsolidationForSingleTask && executed.Count == 1
                    && executed[0].Status == TaskStatus.Completed && !string.IsNullOrWhiteSpace(executed[0].Result))
                {
                    // Con una sola tarea exitosa su respuesta ya es la final: se ahorra una llamada completa a Claude.
                    reply = executed[0].Result!;
                }
                else
                {
                    await _state.SetAsync(orchestrator.Id, AgentStatus.Reviewing, OfficeEventType.AgentProgress,
                        "El Orquestador consolida los resultados.", data: convData).ConfigureAwait(false);

                    reply = await ConsolidateAsync(orchestrator, team, conversation, userText, executed, attached.Count > 0, runCts.Token).ConfigureAwait(false);
                }
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
            executed.Select(t => t.ToDto()).ToArray(),
            executed.Where(t => t.DocumentId is not null && deliverables.All(d => d.Id != t.DocumentId))
                .Select(t => new GeneratedDocumentDto(t.DocumentId!.Value, t.DocumentName ?? "documento.docx", 0))
                .Concat(deliverables).ToArray(), choice);
    }

    #endregion

    #region Entregables de la base de conocimiento

    /// <summary>
    /// Genera el Word y el PDF finales de la respuesta, con el estilo del formato de entrega (APA si el
    /// formato lo pide).
    /// </summary>
    private async Task<IReadOnlyList<GeneratedDocumentDto>> BuildDeliverablesAsync(
        KnowledgePlan plan, string text, IReadOnlyList<AgentTask> executed, CancellationToken cancellationToken)
    {
        var style = _knowledge.ResolveStyle(plan);
        var title = FirstHeading(text) ?? "Respuesta a incidente";
        var formatter = executed.FirstOrDefault(t => string.Equals(t.AgentId, _knowledge.FormatterAgentId, StringComparison.OrdinalIgnoreCase));
        try
        {
            await _events.EmitAsync(OfficeEventType.OfficeActivityUpdated,
                $"Generando el entregable (Word y PDF) en estilo {(style == DocumentStyle.Apa ? "APA 7" : "estándar")}.", _knowledge.FormatterAgentId).ConfigureAwait(false);

            var word = await _word.ExportAsync(title, text, style, cancellationToken).ConfigureAwait(false);
            var pdf = await _pdf.ExportAsync(title, text, style, cancellationToken).ConfigureAwait(false);
            if (formatter is not null) _tasks.Update(formatter.Id, t => { t.DocumentId = word.Id; t.DocumentName = word.FileName; });
            return new[] { word, pdf };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "No se pudieron generar los documentos del entregable");
            return Array.Empty<GeneratedDocumentDto>();
        }
    }

    /// <summary>Texto del primer título (# ...) del documento, o null; se usa como título del archivo.</summary>
    private static string? FirstHeading(string text)
    {
        foreach (var raw in text.Split('\n'))
        {
            var line = raw.Trim();
            if (line.StartsWith("# ", StringComparison.Ordinal)) return line[2..].Trim(' ', '*', '#');
            if (line.Length > 0 && !line.StartsWith('#')) break;
        }
        return null;
    }

    #endregion

    #region Planificación del Orquestador

    /// <summary>
    /// Pide al Orquestador la decisión (responder o delegar) mediante una llamada estructurada. Reintenta
    /// con más margen de tokens y, si todo falla, devuelve una respuesta segura en vez de romper la
    /// solicitud.
    /// </summary>
    private async Task<OrchestratorDecision> PlanAsync(
        AgentDefinition orchestrator, IReadOnlyList<AgentDefinition> team, Conversation conversation,
        IReadOnlyList<OfficeDocument> documents, CancellationToken cancellationToken)
    {
        var delegableIds = team.Select(t => t.Id).ToList();
        var aliases = new Dictionary<string, string>();
        foreach (var member in team)
            foreach (var alias in new[] { member.Id, member.Name, member.Role })
            {
                var key = OrchestratorDecisionValidator.Normalize(alias);
                if (key.Length > 0) aliases.TryAdd(key, member.Id);
            }
        var spec = DecisionSchema.Build(delegableIds, _options.MaxTasksPerRequest);
        var system = PromptBuilder.BuildOrchestratorSystem(orchestrator, team, _options.MaxTasksPerRequest, documents.Count > 0);
        var messages = BuildHistory(conversation, documents);

        IReadOnlyList<string> problems = Array.Empty<string>();
        for (var attempt = 1; attempt <= MaxPlanningAttempts; attempt++)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.PlanningTimeoutSeconds));

            var response = await _anthropic.SendAsync(new LlmRequest(
                system, messages, PlanningTokens(attempt), orchestrator.ModelSettings.Model,
                orchestrator.ModelSettings.Temperature, spec, "orchestrator:plan",
                ProviderPreference: attempt >= MaxPlanningAttempts ? LlmProviders.Claude : orchestrator.ModelSettings.Provider,
                FreeModel: orchestrator.ModelSettings.FreeModel,
                Sensitive: documents.Count > 0), timeout.Token).ConfigureAwait(false);

            if (response.StopReason == "max_tokens")
            {
                // Una decisión cortada por el límite de tokens puede parecer un plan válido con MENOS tareas: nunca se acepta.
                problems = new[] { "Tu decisión se cortó por el límite de tokens. Reduce cada descripción de tarea a 1-2 frases." };
            }
            else if (response.StructuredJson is null)
            {
                // Con tool_choice "auto" el modelo puede contestar en texto plano; en el último intento se acepta como respuesta
                // directa.
                if (attempt == MaxPlanningAttempts && !string.IsNullOrWhiteSpace(response.Text))
                    return new OrchestratorDecision { Action = DecisionActions.Respond, Message = response.Text.Trim() };

                problems = new[] { $"No llamaste a la herramienta {DecisionSchema.ToolName}." };
            }
            else
            {
                var decision = OrchestratorDecision.Parse(response.StructuredJson);
                problems = OrchestratorDecisionValidator.Validate(decision, delegableIds, _options.MaxTasksPerRequest, aliases);
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

        // Respuesta segura en lugar de fallar toda la solicitud.
        _logger.LogError("El Orquestador no produjo una decisión válida: {Problems}", string.Join(" | ", problems));
        return new OrchestratorDecision
        {
            Action = DecisionActions.Respond,
            Message = "No pude planificar la solicitud de forma fiable (" + string.Join(" ", problems) + ") ¿Puedes reformularla con más detalle?"
        };
    }

    /// <summary>
    /// El plan es un JSON con una entrada por tarea: cada reintento duplica el margen de tokens (el tope
    /// solo cuesta lo que se genera).
    /// </summary>
    private int PlanningTokens(int attempt) => Math.Min(_options.PlanningMaxTokens * (1 << (attempt - 1)), 8192);

    #endregion

    #region Creación de tareas

    /// <summary>
    /// Convierte las tareas planificadas en tareas registradas (con su agente, dependencias y banderas) y
    /// emite sus eventos de creación.
    /// </summary>
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
                IncludeDocuments = planned.IncludeDocuments ?? definition.AcceptsDocuments,
                ExtraContext = planned.ExtraContext,
                Sensitive = planned.Sensitive,
                SkipDocument = planned.SkipDocument,
                AllowFreeDocuments = planned.AllowFreeDocuments
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

    #endregion

    #region Consolidación de resultados

    /// <summary>
    /// Pide al Orquestador una síntesis corta de lo que hicieron los agentes y le añade los resultados
    /// completos.
    /// </summary>
    private async Task<string> ConsolidateAsync(
        AgentDefinition orchestrator, IReadOnlyList<AgentDefinition> team, Conversation conversation, string userText,
        IReadOnlyList<AgentTask> executed, bool hasDocuments, CancellationToken cancellationToken)
    {
        var results = executed.Select(t => (t, team.FirstOrDefault(a => a.Id == t.AgentId)?.Name ?? t.AgentId)).ToList();
        var prompt = PromptBuilder.BuildConsolidationPrompt(userText, results, _options.MaxResultCharsForConsolidation);

        // Los turnos anteriores sirven de contexto y luego va un turno de consolidación con los resultados.
        var history = BuildHistory(conversation, Array.Empty<OfficeDocument>()).ToList();
        if (history.Count > 0 && history[^1].Role == "user") history.RemoveAt(history.Count - 1);
        history.Add(new LlmMessage("user", prompt));

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.ConsolidationTimeoutSeconds));

            var response = await _anthropic.SendAsync(new LlmRequest(
                PromptBuilder.BuildConsolidationSystem(orchestrator), history, _options.ConsolidationMaxTokens,
                orchestrator.ModelSettings.Model, orchestrator.ModelSettings.Temperature, null, "orchestrator:consolidate",
                orchestrator.ModelSettings.Provider, orchestrator.ModelSettings.FreeModel, hasDocuments),
                timeout.Token).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(response.Text)) return AppendFullResults(response.Text.Trim(), results);
        }
        catch (LlmException ex) when (executed.Any(t => t.Status == TaskStatus.Completed))
        {
            _logger.LogWarning("No se pudo consolidar con Claude ({Message}); se entregan resultados directos.", ex.Message);
        }

        return FallbackConsolidation(results);
    }

    /// <summary>
    /// El Orquestador solo escribe una síntesis corta (barata); las salidas completas de los agentes se
    /// anexan tal cual debajo, para no perder nada ni gastar tokens copiándolas.
    /// </summary>
    private static string AppendFullResults(string synthesis, IReadOnlyList<(AgentTask Task, string AgentName)> results)
    {
        var done = results.Where(r => r.Task.Status == TaskStatus.Completed && !string.IsNullOrWhiteSpace(r.Task.Result)).ToList();
        if (done.Count == 0) return synthesis;

        var sb = new StringBuilder(synthesis).AppendLine().AppendLine().AppendLine("---").AppendLine();
        foreach (var (task, name) in done)
        {
            sb.AppendLine($"## {name} — {task.Title}").AppendLine().AppendLine(task.Result).AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

    /// <summary>Consolidación sin modelo, por si la llamada al Orquestador falla: concatena los resultados.</summary>
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

    #endregion

    #region Historial y utilidades

    /// <summary>
    /// Arma el historial de mensajes para el modelo, acortando los turnos viejos y respetando que los roles
    /// alternen.
    /// </summary>
    private IReadOnlyList<LlmMessage> BuildHistory(Conversation conversation, IReadOnlyList<OfficeDocument> documents)
    {
        var recent = conversation.Messages.TakeLast(Math.Max(1, _options.MaxHistoryMessages)).ToList();
        var result = new List<LlmMessage>();

        for (var i = 0; i < recent.Count; i++)
        {
            var role = recent[i].Role == ChatRole.User ? "user" : "assistant";
            var content = recent[i].Content;

            // Planificar solo necesita lo esencial de los turnos anteriores: se acortan (el último mensaje queda completo).
            var isLast = i == recent.Count - 1;
            if (!isLast && content.Length > _options.HistoryMessageMaxChars)
                content = content[.._options.HistoryMessageMaxChars] + " […]";

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

            // La API exige roles alternados empezando por "user": se fusionan los turnos consecutivos del mismo rol.
            if (result.Count > 0 && result[^1].Role == role)
                result[^1] = result[^1] with { Content = result[^1].Content + "\n\n" + content };
            else
                result.Add(new LlmMessage(role, content));
        }

        while (result.Count > 0 && result[0].Role != "user") result.RemoveAt(0);
        return result;
    }

    /// <summary>Busca en memoria los documentos adjuntos por su id (ignora los que ya no existen).</summary>
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

    /// <summary>Datos del evento con la cantidad de tareas delegadas.</summary>
    private static Dictionary<string, string?> TaskCountData(Dictionary<string, string?> baseData, int count) =>
        new(baseData) { [EventDataKeys.TaskCount] = count.ToString() };

    /// <summary>Recorta un texto a un máximo de caracteres añadiendo puntos suspensivos.</summary>
    private static string Shorten(string text, int max) =>
        text.Length <= max ? text : text[..max].TrimEnd() + "…";
    #endregion

}
