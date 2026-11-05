using AI.Agent.Office.Domain.Enums;

namespace AI.Agent.Office.Domain.Events;

public sealed record OfficeEvent(
    Guid EventId,
    string? AgentId,
    Guid? TaskId,
    OfficeEventType EventType,
    string Description,
    DateTimeOffset Timestamp,
    IReadOnlyDictionary<string, string?>? AdditionalData = null);
