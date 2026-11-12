using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Application.Services;

/// <summary>
/// Consultas de solo lectura sobre la oficina (agentes, tareas, actividad, consumo y oficinas) que usan
/// la API y la interfaz.
/// </summary>
public sealed class OfficeQueryService : IOfficeQueryService
{
    private readonly IAgentRegistry _registry;
    private readonly ITaskStore _tasks;
    private readonly IActivityLog _activity;
    private readonly IConversationStore _conversations;
    private readonly ITokenUsageTracker _usage;
    private readonly IAnthropicService _anthropic;
    private readonly PricingOptions _pricing;
    private readonly IWorkspaceService _workspaces;
    private readonly IAgentConfigurationService _agentDefinitions;

    /// <summary>Crea el servicio con los almacenes y la configuración necesarios.</summary>
    public OfficeQueryService(
        IAgentRegistry registry, ITaskStore tasks, IActivityLog activity,
        IConversationStore conversations, ITokenUsageTracker usage, IAnthropicService anthropic,
        IOptions<PricingOptions> pricing, IWorkspaceService workspaces, IAgentConfigurationService agentDefinitions)
    {
        _registry = registry;
        _tasks = tasks;
        _activity = activity;
        _conversations = conversations;
        _usage = usage;
        _anthropic = anthropic;
        _pricing = pricing.Value;
        _workspaces = workspaces;
        _agentDefinitions = agentDefinitions;
    }

    /// <summary>Foto completa del estado de la oficina para la interfaz.</summary>
    public OfficeStateDto GetState(int activityCount = 60) => new(
        GetAgents(),
        GetTasks().Take(50).ToArray(),
        GetActivity(activityCount),
        GetUsage(),
        _anthropic.IsConfigured,
        _workspaces.Active?.Id);

    /// <summary>
    /// Consumo de tokens, separado por agente y proveedor, con el costo estimado si hay precios
    /// configurados.
    /// </summary>
    public TokenUsageDto GetUsage()
    {
        var usage = _usage.GetSnapshot();
        decimal? cost = _pricing.InputPerMillionTokens > 0 || _pricing.OutputPerMillionTokens > 0
            // Solo los tokens de Claude cuestan dinero; los de las APIs gratuitas no.
            ? ((usage.InputTokens - usage.FreeInputTokens) * _pricing.InputPerMillionTokens
               + (usage.OutputTokens - usage.FreeOutputTokens) * _pricing.OutputPerMillionTokens) / 1_000_000m
            : null;

        var byAgent = usage.ByAgent
            .Select(kv => new AgentUsageDto(kv.Key, kv.Value.InputTokens, kv.Value.OutputTokens, kv.Value.Requests))
            .OrderByDescending(a => a.InputTokens + a.OutputTokens)
            .ToArray();

        return new TokenUsageDto(usage.InputTokens, usage.OutputTokens, usage.Requests, byAgent, cost,
            usage.FreeInputTokens, usage.FreeOutputTokens, usage.FreeRequests,
            (usage.ByProvider ?? new Dictionary<string, AgentUsage>())
                .Select(kv => new ProviderUsageDto(kv.Key, !string.Equals(kv.Key, "claude", StringComparison.OrdinalIgnoreCase),
                    kv.Value.InputTokens, kv.Value.OutputTokens, kv.Value.Requests))
                .OrderBy(p => p.IsFree ? 0 : 1).ThenByDescending(p => p.InputTokens + p.OutputTokens)
                .ToArray());
    }

    /// <summary>Solo los agentes de la oficina activa, en el orden de los puestos (el Orquestador primero).</summary>
    public IReadOnlyList<AgentDto> GetAgents() => _registry.GetAll()
        .Where(a => _workspaces.IsAgentActive(a.Id))
        .OrderBy(a => a.IsOrchestrator ? 0 : 1).ThenBy(a => _workspaces.OrderOf(a.Id))
        .Select(a => a.ToDto()).ToArray();

    /// <summary>Oficinas disponibles, cuál está abierta y si hay que elegir una antes de empezar.</summary>
    public WorkspacesDto GetWorkspaces()
    {
        var definitions = _agentDefinitions.GetDefinitions();
        var available = _workspaces.GetAll().Select(w => new WorkspaceDto(w.Id, w.Name, w.Icon, w.Description,
            w.Agents.Select(id => definitions.FirstOrDefault(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase)))
                .Where(d => d is not null)
                .Select(d => new WorkspaceAgentDto(d!.Id, d.Name, d.Icon, d.Role)).ToArray(), w.Kind)).ToArray();
        return new WorkspacesDto(available, _workspaces.Active?.Id, _workspaces.RequiresSelection);
    }

    /// <summary>Un agente por su id.</summary>
    public AgentDto? GetAgent(string agentId) => _registry.Get(agentId)?.ToDto();

    /// <summary>Todas las tareas.</summary>
    public IReadOnlyList<AgentTaskDto> GetTasks() => _tasks.GetAll().Select(t => t.ToDto()).ToArray();

    /// <summary>Actividad reciente, opcionalmente de un solo agente.</summary>
    public IReadOnlyList<OfficeActivityDto> GetActivity(int count, string? agentId = null) =>
        _activity.GetRecent(Math.Clamp(count, 1, 300), agentId).Select(a => a.ToDto()).ToArray();

    /// <summary>Una conversación por su id.</summary>
    public ConversationDto? GetConversation(Guid id) => _conversations.Get(id)?.ToDto();
}
