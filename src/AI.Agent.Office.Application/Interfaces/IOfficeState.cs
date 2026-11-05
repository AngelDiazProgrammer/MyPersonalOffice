using AI.Agent.Office.Application.Agents;

namespace AI.Agent.Office.Application.Interfaces;

using AI.Agent.Office.Domain.Entities;
using Agent = AI.Agent.Office.Domain.Entities.Agent;

public interface IAgentRegistry
{
    void Load(IEnumerable<AgentDefinition> definitions);
    IReadOnlyList<Agent> GetAll();
    Agent? Get(string agentId);
    /// <summary>Mutates an agent atomically and returns a snapshot, or null when unknown.</summary>
    Agent? Update(string agentId, Action<Agent> mutation);
}

public interface ITaskStore
{
    void Add(AgentTask task);
    AgentTask? Get(Guid taskId);
    IReadOnlyList<AgentTask> GetAll();
    IReadOnlyList<AgentTask> GetByConversation(Guid conversationId);
    AgentTask? Update(Guid taskId, Action<AgentTask> mutation);
    void RegisterCancellation(Guid taskId, CancellationTokenSource source);
    void UnregisterCancellation(Guid taskId);
    /// <summary>Cancels a pending or running task. Returns false when it is not active.</summary>
    bool RequestCancel(Guid taskId);
    bool WasCancelledByUser(Guid taskId);
}

public interface IConversationStore
{
    Conversation GetOrCreate(Guid? conversationId);
    Conversation? Get(Guid conversationId);
    void RegisterRun(Guid conversationId, CancellationTokenSource source);
    void UnregisterRun(Guid conversationId, CancellationTokenSource source);
    bool CancelRun(Guid conversationId);
}

public interface IActivityLog
{
    void Add(OfficeActivity activity);
    IReadOnlyList<OfficeActivity> GetRecent(int count, string? agentId = null);
}

public interface IDocumentStore
{
    void Add(OfficeDocument document);
    OfficeDocument? Get(Guid id);
}

public interface ITokenUsageTracker
{
    void Record(string purpose, int inputTokens, int outputTokens);
    TokenUsageSnapshot GetSnapshot();
}

public sealed record AgentUsage(long InputTokens, long OutputTokens, long Requests);

public sealed record TokenUsageSnapshot(long InputTokens, long OutputTokens, long Requests, IReadOnlyDictionary<string, AgentUsage> ByAgent);
