using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Application.Orchestration;
using AI.Agent.Office.Domain.Entities;
using AI.Agent.Office.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Application.Agents;

/// <summary>
/// Ejecutor genérico compartido por todos los especialistas: su personalidad viene de las instrucciones
/// Markdown del agente, así agregar un agente nunca requiere una clase nueva.
/// </summary>
public sealed class AgentRunner : IAgentRunner
{
    private readonly IAgentConfigurationService _configuration;
    private readonly IAnthropicService _anthropic;
    private readonly IAgentStateService _state;
    private readonly OrchestrationOptions _options;
    private readonly ILogger<AgentRunner> _logger;

    /// <summary>Crea el ejecutor con la configuración de agentes, el cliente de modelos y el servicio de estados.</summary>
    public AgentRunner(
        IAgentConfigurationService configuration,
        IAnthropicService anthropic,
        IAgentStateService state,
        IOptions<OrchestrationOptions> options,
        ILogger<AgentRunner> logger)
    {
        _configuration = configuration;
        _anthropic = anthropic;
        _state = state;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta una tarea de un agente: marca su estado, llama al modelo con sus instrucciones y devuelve el
    /// texto y el proveedor que respondió.
    /// </summary>
    public async Task<AgentRunResult> RunAsync(AgentTask task, string prompt, bool containsDocuments, CancellationToken cancellationToken)
    {
        var definition = _configuration.GetDefinition(task.AgentId)
            ?? throw new InvalidOperationException($"El agente '{task.AgentId}' no está configurado.");

        await _state.SetAsync(task.AgentId, AgentStatus.Thinking, OfficeEventType.AgentThinking,
            $"{definition.Name} está analizando la tarea: {task.Title}", task.Id, task.Title).ConfigureAwait(false);

        if (_options.ThinkingDisplayMs > 0)
            await Task.Delay(_options.ThinkingDisplayMs, cancellationToken).ConfigureAwait(false);

        await _state.SetAsync(task.AgentId, definition.WorkingStatus, OfficeEventType.AgentProgress,
            $"{definition.Name} está trabajando: {task.Title}", task.Id, task.Title).ConfigureAwait(false);

        var request = new LlmRequest(
            System: PromptBuilder.BuildAgentSystem(definition),
            Messages: new[] { new LlmMessage("user", prompt) },
            MaxTokens: definition.ModelSettings.MaxTokens ?? _options.DefaultAgentMaxTokens,
            Model: definition.ModelSettings.Model,
            Temperature: definition.ModelSettings.Temperature,
            Purpose: $"agent:{definition.Id}",
            ProviderPreference: definition.ModelSettings.Provider,
            FreeModel: definition.ModelSettings.FreeModel,
            Sensitive: containsDocuments);

        var response = await _anthropic.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(response.Text))
        {
            // A veces los modelos no devuelven texto; un reintento con un empujón lo resuelve casi siempre.
            _logger.LogWarning("Agente {AgentId} devolvió una respuesta vacía (stop_reason: {Stop}); se reintenta una vez.",
                task.AgentId, response.StopReason);
            var retry = request with
            {
                Messages = new[] { new LlmMessage("user", prompt + "\n\nEntrega ahora tu resultado completo en texto.") },
                Purpose = request.Purpose + ":retry",
                ProviderPreference = LlmProviders.Claude // a weak/empty free answer escalates to Claude
            };
            response = await _anthropic.SendAsync(retry, cancellationToken).ConfigureAwait(false);
        }

        if (string.IsNullOrWhiteSpace(response.Text))
            throw new LlmException($"El agente devolvió una respuesta vacía (motivo: {response.StopReason ?? "desconocido"}).");

        _logger.LogInformation("Agente {AgentId} completó la tarea {TaskId} ({In}/{Out} tokens, stop: {Stop})",
            task.AgentId, task.Id, response.InputTokens, response.OutputTokens, response.StopReason);

        var text = response.Text.Trim();
        if (response.StopReason == "max_tokens")
            text += "\n\n> ⚠️ _Respuesta cortada por el límite de tokens del agente. Pídele que continúe o sube `maxTokens` en su archivo Markdown._";
        return new AgentRunResult(text, response.Provider);
    }
}
