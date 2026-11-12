using ATLAS.Application.Interfaces;
using ATLAS.Domain.Entities;
using ATLAS.Domain.Enums;
using ATLAS.Domain.Events;
using ATLAS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ATLAS.Application.Services;

/// <summary>Registra cada evento de la oficina en la bitácora y lo reenvía al canal en tiempo real.</summary>
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

    /// <summary>Crea el evento, lo guarda en la bitácora y lo publica a los clientes conectados.</summary>
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
            // Un canal en tiempo real caído nunca debe romper la orquestación.
            _logger.LogWarning(ex, "No se pudo publicar el evento {EventType}", type);
        }

        return officeEvent;
    }
}
