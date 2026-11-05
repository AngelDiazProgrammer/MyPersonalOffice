using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Domain.Enums;
using AI.Agent.Office.Web.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace AI.Agent.Office.Web.Hubs;

public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting
}

/// <summary>SignalR client: subscribes to every office event type and forwards them to a single handler.</summary>
public sealed class OfficeHubClient : IAsyncDisposable
{
    private readonly HubConnection _connection;

    public ConnectionStatus Status { get; private set; } = ConnectionStatus.Disconnected;

    public event Action<ConnectionStatus>? StatusChanged;
    public event Func<OfficeEventDto, Task>? EventReceived;
    public event Func<Task>? Reconnected;

    public OfficeHubClient(string hubUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .AddJsonProtocol(o => o.PayloadSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter()))
            .Build();

        foreach (var type in Enum.GetValues<OfficeEventType>())
        {
            _connection.On<OfficeEventDto>(type.ToString(), async e =>
            {
                if (EventReceived is { } handler) await handler(e);
            });
        }

        _connection.Reconnecting += _ => { SetStatus(ConnectionStatus.Reconnecting); return Task.CompletedTask; };
        _connection.Reconnected += async _ =>
        {
            SetStatus(ConnectionStatus.Connected);
            if (Reconnected is { } handler) await handler();
        };
        _connection.Closed += _ => { SetStatus(ConnectionStatus.Disconnected); return Task.CompletedTask; };
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        SetStatus(ConnectionStatus.Connecting);
        try
        {
            await _connection.StartAsync(ct);
            SetStatus(ConnectionStatus.Connected);
        }
        catch
        {
            SetStatus(ConnectionStatus.Disconnected);
            throw;
        }
    }

    private void SetStatus(ConnectionStatus status)
    {
        Status = status;
        StatusChanged?.Invoke(status);
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}
