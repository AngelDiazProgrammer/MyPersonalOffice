using AI.Agent.Office.Application.Agents;

namespace AI.Agent.Office.Application.Interfaces;

/// <summary>Carga los roles de los agentes desde archivos Markdown y los valida.</summary>
public interface IAgentConfigurationService
{
    /// <summary>Lee (o relee) todos los archivos Markdown configurados.</summary>
    Task<ConfigurationReport> ReloadAsync(CancellationToken cancellationToken = default);
    IReadOnlyList<AgentDefinition> GetDefinitions();
    AgentDefinition? GetDefinition(string agentId);
    string? GetInstructions(string agentId);
    ConfigurationReport Validate();
}
