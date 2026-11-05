using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Application.Services;

public sealed class OfficeQueryService : IOfficeQueryService
{
    private readonly IAgentRegistry _registry;
    private readonly ITaskStore _tasks;
    private readonly IActivityLog _activity;
    private readonly IConversationStore _conversations;
    private readonly ITokenUsageTracker _usage;
    private readonly IAnthropicService _anthropic;
    private readonly PricingOptions _pricing;

    public OfficeQueryService(
        IAgentRegistry registry, ITaskStore tasks, IActivityLog activity,
        IConversationStore conversations, ITokenUsageTracker usage, IAnthropicService anthropic,
        IOptions<PricingOptions> pricing)
    {
        _registry = registry;
        _tasks = tasks;
        _activity = activity;
        _conversations = conversations;
        _usage = usage;
        _anthropic = anthropic;
        _pricing = pricing.Value;
    }

    public OfficeStateDto GetState(int activityCount = 60) => new(
        GetAgents(),
        GetTasks().Take(50).ToArray(),
        GetActivity(activityCount),
        GetUsage(),
        _anthropic.IsConfigured);

    public TokenUsageDto GetUsage()
    {
        var usage = _usage.GetSnapshot();
        decimal? cost = _pricing.InputPerMillionTokens > 0 || _pricing.OutputPerMillionTokens > 0
            ? (usage.InputTokens * _pricing.InputPerMillionTokens + usage.OutputTokens * _pricing.OutputPerMillionTokens) / 1_000_000m
            : null;

        var byAgent = usage.ByAgent
            .Select(kv => new AgentUsageDto(kv.Key, kv.Value.InputTokens, kv.Value.OutputTokens, kv.Value.Requests))
            .OrderByDescending(a => a.InputTokens + a.OutputTokens)
            .ToArray();

        return new TokenUsageDto(usage.InputTokens, usage.OutputTokens, usage.Requests, byAgent, cost);
    }

    public IReadOnlyList<AgentDto> GetAgents() => _registry.GetAll().Select(a => a.ToDto()).ToArray();

    public AgentDto? GetAgent(string agentId) => _registry.Get(agentId)?.ToDto();

    public IReadOnlyList<AgentTaskDto> GetTasks() => _tasks.GetAll().Select(t => t.ToDto()).ToArray();

    public IReadOnlyList<OfficeActivityDto> GetActivity(int count, string? agentId = null) =>
        _activity.GetRecent(Math.Clamp(count, 1, 300), agentId).Select(a => a.ToDto()).ToArray();

    public ConversationDto? GetConversation(Guid id) => _conversations.Get(id)?.ToDto();
}
