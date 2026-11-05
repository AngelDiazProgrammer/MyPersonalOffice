using AI.Agent.Office.Domain.Enums;
using AI.Agent.Office.Domain.ValueObjects;

namespace AI.Agent.Office.Application.Agents;

public sealed record AgentDefinition(
    string Id,
    string Name,
    string Description,
    string Role,
    string ConfigurationFile,
    string Instructions,
    AgentModelSettings ModelSettings,
    AgentStatus WorkingStatus,
    bool IsOrchestrator,
    bool AcceptsDocuments,
    string Color,
    string Icon);

public sealed record ConfigurationIssue(string AgentId, string Message, bool IsError);

public sealed record ConfigurationReport(IReadOnlyList<AgentDefinition> Agents, IReadOnlyList<ConfigurationIssue> Issues)
{
    public bool IsValid => Issues.All(i => !i.IsError);
}
