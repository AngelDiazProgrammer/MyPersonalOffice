using AI.Agent.Office.Domain.Enums;

namespace AI.Agent.Office.Domain.Entities;

/// <summary>Un mensaje del chat, del usuario o del Orquestador.</summary>
public sealed class ChatMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public ChatRole Role { get; init; }
    public required string Content { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
