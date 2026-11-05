using AI.Agent.Office.Domain.Enums;
using TaskStatus = AI.Agent.Office.Domain.Enums.TaskStatus;

namespace AI.Agent.Office.Domain.Entities;

public sealed class AgentTask
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ConversationId { get; init; }
    public required string AgentId { get; init; }
    /// <summary>Plan-local key used to express dependencies between tasks of one orchestration.</summary>
    public string Key { get; init; } = string.Empty;
    public required string Title { get; init; }
    public required string Description { get; init; }
    public IReadOnlyList<string> DependsOn { get; init; } = Array.Empty<string>();
    public bool IncludeDocuments { get; init; }
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }

    public AgentTask Clone() => (AgentTask)MemberwiseClone();
}
