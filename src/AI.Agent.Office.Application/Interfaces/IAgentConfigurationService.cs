using AI.Agent.Office.Application.Agents;

namespace AI.Agent.Office.Application.Interfaces;

/// <summary>Loads agent roles from Markdown files and validates them.</summary>
public interface IAgentConfigurationService
{
    /// <summary>Reads (or re-reads) every configured Markdown file.</summary>
    Task<ConfigurationReport> ReloadAsync(CancellationToken cancellationToken = default);
    IReadOnlyList<AgentDefinition> GetDefinitions();
    AgentDefinition? GetDefinition(string agentId);
    string? GetInstructions(string agentId);
    ConfigurationReport Validate();
}
