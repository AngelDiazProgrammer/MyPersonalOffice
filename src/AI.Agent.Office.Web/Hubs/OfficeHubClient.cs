using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Domain.Enums;
using AI.Agent.Office.Web.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace AI.Agent.Office.Web.Hubs;

/// <summary>Estado de la conexión en tiempo real.</summary>
public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting
}

/// <summary>Cliente SignalR: se suscribe a cada tipo de evento de la oficina y los reenvía a un único manejador.</summary>
public sealed class OfficeHubClient : IAsyncDisposable
{
    private readonly HubConnection _connection;

    public ConnectionStatus Status { get; private set; } = ConnectionStatus.Disconnected;

    public event Action<ConnectionStatus>? StatusChanged;
    public event Func<OfficeEventDto, Task>? EventReceived;
    public event Func<Task>? Reconnected;

    /// <summary>Crea la conexión con reconexión automática y la suscripción a todos los eventos.</summary>
    public OfficeHubClient(string hubUrl, string? internalToken = null)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, o => { if (!string.IsNullOrEmpty(internalToken)) o.Headers["X-Office-Internal"] = internalToken; })
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

    /// <summary>Abre la conexión.</summary>
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

    /// <summary>Cambia el estado de conexión y avisa a los interesados.</summary>
    private void SetStatus(ConnectionStatus status)
    {
        Status = status;
        StatusChanged?.Invoke(status);
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}
