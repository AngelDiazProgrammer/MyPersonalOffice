using ATLAS.Application.Events;
using ATLAS.Application.Interfaces;
using ATLAS.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ATLAS.Application.Agents;

/// <summary>
/// Único componente autorizado a cambiar el estado de un agente. Cada cambio emite un evento con el
/// nuevo estado, así lo que muestra la interfaz siempre es consecuencia de un evento real.
/// </summary>
public sealed class AgentStateService : IAgentStateService
{
    private readonly IAgentRegistry _registry;
    private readonly IOfficeEventService _events;
    private readonly ILogger<AgentStateService> _logger;
    private readonly object _gate = new();
    private readonly Dictionary<string, long> _revisions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Crea el servicio con el registro de agentes y el emisor de eventos.</summary>
    public AgentStateService(IAgentRegistry registry, IOfficeEventService events, ILogger<AgentStateService> logger)
    {
        _registry = registry;
        _events = events;
        _logger = logger;
    }

    /// <summary>Cambia el estado de un agente (null = sin cambio) y emite el evento correspondiente.</summary>
    public async Task SetAsync(
        string agentId,
        AgentStatus? status,
        OfficeEventType eventType,
        string description,
        Guid? taskId = null,
        string? taskTitle = null,
        bool clearTask = false,
        IDictionary<string, string?>? data = null)
    {
        var snapshot = _registry.Update(agentId, agent =>
        {
            if (status is { } s) agent.Status = s;
            if (clearTask)
            {
                agent.CurrentTaskId = null;
                agent.CurrentTaskTitle = null;
            }
            else if (taskId is not null || taskTitle is not null)
            {
                if (taskId is not null) agent.CurrentTaskId = taskId;
                agent.CurrentTaskTitle = taskTitle ?? agent.CurrentTaskTitle;
            }
            agent.LastActivityAt = DateTimeOffset.UtcNow;
        });

        if (snapshot is null)
        {
            _logger.LogWarning("Se intentó actualizar el agente desconocido {AgentId}", agentId);
            return;
        }

        lock (_gate) { _revisions[agentId] = _revisions.GetValueOrDefault(agentId) + 1; }

        var payload = data is null ? new Dictionary<string, string?>() : new Dictionary<string, string?>(data);
        payload[EventDataKeys.AgentStatus] = snapshot.Status.ToString();
        payload[EventDataKeys.TaskTitle] = snapshot.CurrentTaskTitle;

        await _events.EmitAsync(eventType, description, agentId, taskId ?? snapshot.CurrentTaskId, payload).ConfigureAwait(false);
    }

    /// <summary>
    /// Programa que el agente vuelva a "En reposo" tras una pausa, salvo que mientras tanto haya cambiado
    /// de estado.
    /// </summary>
    public void ReturnToIdleAfter(string agentId, TimeSpan delay)
    {
        long revision;
        lock (_gate) { revision = _revisions.GetValueOrDefault(agentId); }

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay).ConfigureAwait(false);

                bool unchanged;
                lock (_gate) { unchanged = _revisions.GetValueOrDefault(agentId) == revision; }
                if (!unchanged) return; // the agent got new work in the meantime

                var agent = _registry.Get(agentId);
                if (agent is null || agent.Status is AgentStatus.Idle) return;

                await SetAsync(agentId, AgentStatus.Idle, OfficeEventType.AgentStarted,
                    $"{agent.Name} está disponible.", clearTask: true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "No se pudo devolver al agente {AgentId} a reposo", agentId);
            }
        });
    }
}
