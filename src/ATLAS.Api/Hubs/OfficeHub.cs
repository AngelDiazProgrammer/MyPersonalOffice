using ATLAS.Application.DTOs;
using ATLAS.Domain.Events;
using ATLAS.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ATLAS.Api.Hubs;

/// <summary>
/// Canal en tiempo real. El servidor envía un método de cliente por tipo de evento (AgentStarted,
/// AgentThinking, TaskCreated...), cada uno con un <see cref="OfficeEventDto"/>.
/// </summary>
public sealed class OfficeHub : Hub
{
    public const string Route = "/hubs/office";
}

/// <summary>Adaptador que permite a la capa de aplicación publicar eventos sin conocer SignalR.</summary>
public sealed class SignalROfficeEventPublisher : IOfficeEventPublisher
{
    private readonly IHubContext<OfficeHub> _hub;

    public SignalROfficeEventPublisher(IHubContext<OfficeHub> hub) => _hub = hub;

    /// <summary>Envía el evento a todos los clientes conectados con el método que lleva su nombre.</summary>
    public Task PublishAsync(OfficeEvent officeEvent, CancellationToken cancellationToken = default) =>
        _hub.Clients.All.SendAsync(officeEvent.EventType.ToString(), officeEvent.ToDto(), cancellationToken);
}
