using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Domain.Events;
using AI.Agent.Office.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AI.Agent.Office.Api.Hubs;

/// <summary>
/// Realtime channel. The server pushes one client method per event type
/// (AgentStarted, AgentThinking, TaskCreated, ...), each carrying an <see cref="OfficeEventDto"/>.
/// </summary>
public sealed class OfficeHub : Hub
{
    public const string Route = "/hubs/office";
}

/// <summary>Adapter that lets the Application layer publish events without knowing about SignalR.</summary>
public sealed class SignalROfficeEventPublisher : IOfficeEventPublisher
{
    private readonly IHubContext<OfficeHub> _hub;

    public SignalROfficeEventPublisher(IHubContext<OfficeHub> hub) => _hub = hub;

    public Task PublishAsync(OfficeEvent officeEvent, CancellationToken cancellationToken = default) =>
        _hub.Clients.All.SendAsync(officeEvent.EventType.ToString(), officeEvent.ToDto(), cancellationToken);
}
