using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Domain.Entities;
using AI.Agent.Office.Domain.Enums;
using AI.Agent.Office.Domain.Events;
using AI.Agent.Office.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AI.Agent.Office.Application.Services;

/// <summary>Records every office event in the activity log and forwards it to the realtime publisher.</summary>
public sealed class OfficeEventService : IOfficeEventService
{
    private readonly IActivityLog _activityLog;
    private readonly IOfficeEventPublisher _publisher;
    private readonly ILogger<OfficeEventService> _logger;

    public OfficeEventService(IActivityLog activityLog, IOfficeEventPublisher publisher, ILogger<OfficeEventService> logger)
    {
        _activityLog = activityLog;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<OfficeEvent> EmitAsync(
        OfficeEventType type,
        string description,
        string? agentId = null,
        Guid? taskId = null,
        IDictionary<string, string?>? data = null)
    {
        var now = DateTimeOffset.UtcNow;
        var officeEvent = new OfficeEvent(
            Guid.NewGuid(), agentId, taskId, type, description, now,
            data is null ? null : new Dictionary<string, string?>(data));

        _activityLog.Add(new OfficeActivity
        {
            Id = officeEvent.EventId,
            AgentId = agentId,
            TaskId = taskId,
            EventType = type,
            Description = description,
            Timestamp = now
        });

        try
        {
            await _publisher.PublishAsync(officeEvent, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // A failing realtime channel must never break the orchestration itself.
            _logger.LogWarning(ex, "No se pudo publicar el evento {EventType}", type);
        }

        return officeEvent;
    }
}
