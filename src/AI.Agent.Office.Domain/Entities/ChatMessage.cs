using AI.Agent.Office.Domain.Enums;

namespace AI.Agent.Office.Domain.Entities;

public sealed class ChatMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public ChatRole Role { get; init; }
    public required string Content { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
