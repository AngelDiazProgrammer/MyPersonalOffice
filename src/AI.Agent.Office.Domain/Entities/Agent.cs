using AI.Agent.Office.Domain.Enums;
using AI.Agent.Office.Domain.ValueObjects;

namespace AI.Agent.Office.Domain.Entities;

public sealed class Agent
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public AgentStatus Status { get; set; } = AgentStatus.Idle;
    public Guid? CurrentTaskId { get; set; }
    public string? CurrentTaskTitle { get; set; }
    public required string ConfigurationFile { get; init; }
    public AgentModelSettings ModelSettings { get; set; } = new();
    public DateTimeOffset? LastActivityAt { get; set; }

    /// <summary>Status shown while the agent works on a task (Working, Coding, Reading, Reviewing...).</summary>
    public AgentStatus WorkingStatus { get; set; } = AgentStatus.Working;
    public bool IsOrchestrator { get; set; }
    public bool AcceptsDocuments { get; set; }
    public string Color { get; set; } = "#8b5cf6";
    public string Icon { get; set; } = "🤖";

    public Agent Clone() => (Agent)MemberwiseClone();
}
