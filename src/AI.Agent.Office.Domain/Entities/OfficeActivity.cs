using AI.Agent.Office.Domain.Enums;

namespace AI.Agent.Office.Domain.Entities;

/// <summary>Entrada de la bitácora de la oficina: qué pasó, con qué agente o tarea y cuándo.</summary>
public sealed class OfficeActivity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string? AgentId { get; init; }
    public Guid? TaskId { get; init; }
    public OfficeEventType EventType { get; init; }
    public required string Description { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
