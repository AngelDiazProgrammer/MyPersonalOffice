using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Application.Orchestration;
using AI.Agent.Office.Domain.Entities;
using AI.Agent.Office.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Application.Agents;

/// <summary>
/// Generic runner shared by every specialist: its personality comes from the agent's Markdown instructions,
/// so adding an agent never requires a new class.
/// </summary>
public sealed class AgentRunner : IAgentRunner
{
    private readonly IAgentConfigurationService _configuration;
    private readonly IAnthropicService _anthropic;
    private readonly IAgentStateService _state;
    private readonly OrchestrationOptions _options;
    private readonly ILogger<AgentRunner> _logger;

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

    public async Task<string> RunAsync(AgentTask task, string prompt, CancellationToken cancellationToken)
    {
        var definition = _configuration.GetDefinition(task.AgentId)
            ?? throw new InvalidOperationException($"El agente '{task.AgentId}' no está configurado.");

        await _state.SetAsync(task.AgentId, AgentStatus.Thinking, OfficeEventType.AgentThinking,
            $"{definition.Name} está analizando la tarea: {task.Title}", task.Id, task.Title).ConfigureAwait(false);

        if (_options.ThinkingDisplayMs > 0)
            await Task.Delay(_options.ThinkingDisplayMs, cancellationToken).ConfigureAwait(false);

        await _state.SetAsync(task.AgentId, definition.WorkingStatus, OfficeEventType.AgentProgress,
            $"{definition.Name} está trabajando con Claude: {task.Title}", task.Id, task.Title).ConfigureAwait(false);

        var request = new LlmRequest(
            System: PromptBuilder.BuildAgentSystem(definition),
            Messages: new[] { new LlmMessage("user", prompt) },
            MaxTokens: definition.ModelSettings.MaxTokens ?? _options.DefaultAgentMaxTokens,
            Model: definition.ModelSettings.Model,
            Temperature: definition.ModelSettings.Temperature,
            Purpose: $"agent:{definition.Id}");

        var response = await _anthropic.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(response.Text))
            throw new LlmException("El agente devolvió una respuesta vacía.");

        _logger.LogInformation("Agente {AgentId} completó la tarea {TaskId} ({In}/{Out} tokens)",
            task.AgentId, task.Id, response.InputTokens, response.OutputTokens);

        return response.Text.Trim();
    }
}
