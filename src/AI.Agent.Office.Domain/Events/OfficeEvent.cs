using AI.Agent.Office.Domain.Enums;

namespace AI.Agent.Office.Domain.Events;

/// <summary>
/// Evento de la oficina: qué ocurrió, a quién y cuándo, con datos adicionales (estado del agente,
/// título de la tarea...).
/// </summary>
public sealed record OfficeEvent(
    Guid EventId,
    string? AgentId,
    Guid? TaskId,
    OfficeEventType EventType,
    string Description,
    DateTimeOffset Timestamp,
    IReadOnlyDictionary<string, string?>? AdditionalData = null);
