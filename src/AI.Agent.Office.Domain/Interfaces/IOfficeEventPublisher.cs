using AI.Agent.Office.Domain.Events;

namespace AI.Agent.Office.Domain.Interfaces;

/// <summary>Publishes office events to whoever is listening (SignalR adapter in the API layer).</summary>
public interface IOfficeEventPublisher
{
    Task PublishAsync(OfficeEvent officeEvent, CancellationToken cancellationToken = default);
}
