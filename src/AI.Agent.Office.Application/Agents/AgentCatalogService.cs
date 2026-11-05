using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AI.Agent.Office.Application.Agents;

/// <summary>Loads Markdown definitions into the runtime registry (at startup and on demand).</summary>
public sealed class AgentCatalogService : IAgentCatalogService
{
    private readonly IAgentConfigurationService _configuration;
    private readonly IAgentRegistry _registry;
    private readonly IOfficeEventService _events;
    private readonly ILogger<AgentCatalogService> _logger;

    public AgentCatalogService(
        IAgentConfigurationService configuration,
        IAgentRegistry registry,
        IOfficeEventService events,
        ILogger<AgentCatalogService> logger)
    {
        _configuration = configuration;
        _registry = registry;
        _events = events;
        _logger = logger;
    }

    public async Task<ConfigurationReport> ReloadAsync(CancellationToken cancellationToken = default)
    {
        var report = await _configuration.ReloadAsync(cancellationToken).ConfigureAwait(false);
        _registry.Load(report.Agents);

        foreach (var issue in report.Issues)
        {
            if (issue.IsError) _logger.LogError("Configuración de agente {AgentId}: {Message}", issue.AgentId, issue.Message);
            else _logger.LogWarning("Configuración de agente {AgentId}: {Message}", issue.AgentId, issue.Message);
        }

        await _events.EmitAsync(OfficeEventType.OfficeActivityUpdated,
            $"Oficina cargada con {report.Agents.Count} agentes.").ConfigureAwait(false);

        return report;
    }
}
