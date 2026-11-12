using ATLAS.Domain.Events;

namespace ATLAS.Domain.Interfaces;

/// <summary>Publica los eventos de la oficina a quien esté escuchando (adaptador de SignalR en la capa de API).</summary>
public interface IOfficeEventPublisher
{
    Task PublishAsync(OfficeEvent officeEvent, CancellationToken cancellationToken = default);
}
