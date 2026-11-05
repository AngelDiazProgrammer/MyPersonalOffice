using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Domain.Entities;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Application.Services;

public sealed class ConversationStore : IConversationStore
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, Conversation> _conversations = new();
    private readonly Dictionary<Guid, CancellationTokenSource> _runs = new();

    public Conversation GetOrCreate(Guid? conversationId)
    {
        lock (_gate)
        {
            if (conversationId is { } id && _conversations.TryGetValue(id, out var existing)) return existing;
            var conversation = conversationId is { } wanted ? new Conversation { Id = wanted } : new Conversation();
            _conversations[conversation.Id] = conversation;
            return conversation;
        }
    }

    public Conversation? Get(Guid conversationId)
    {
        lock (_gate) { return _conversations.GetValueOrDefault(conversationId); }
    }

    public void RegisterRun(Guid conversationId, CancellationTokenSource source)
    {
        lock (_gate) { _runs[conversationId] = source; }
    }

    public void UnregisterRun(Guid conversationId, CancellationTokenSource source)
    {
        lock (_gate)
        {
            if (_runs.TryGetValue(conversationId, out var current) && ReferenceEquals(current, source))
                _runs.Remove(conversationId);
        }
    }

    public bool CancelRun(Guid conversationId)
    {
        CancellationTokenSource? source;
        lock (_gate) { _runs.TryGetValue(conversationId, out source); }
        if (source is null) return false;
        try { source.Cancel(); }
        catch (ObjectDisposedException) { return false; }
        return true;
    }
}

public sealed class ActivityLog : IActivityLog
{
    private readonly object _gate = new();
    private readonly LinkedList<OfficeActivity> _items = new();
    private readonly int _capacity;

    public ActivityLog(IOptions<OrchestrationOptions> options)
    {
        _capacity = Math.Max(50, options.Value.MaxActivityEntries);
    }

    public void Add(OfficeActivity activity)
    {
        lock (_gate)
        {
            _items.AddFirst(activity);
            while (_items.Count > _capacity) _items.RemoveLast();
        }
    }

    public IReadOnlyList<OfficeActivity> GetRecent(int count, string? agentId = null)
    {
        lock (_gate)
        {
            IEnumerable<OfficeActivity> query = _items;
            if (agentId is not null)
                query = query.Where(a => string.Equals(a.AgentId, agentId, StringComparison.OrdinalIgnoreCase));
            return query.Take(Math.Max(1, count)).ToArray();
        }
    }
}

public sealed class DocumentStore : IDocumentStore
{
    private readonly object _gate = new();
    private readonly List<OfficeDocument> _documents = new();
    private readonly int _capacity;

    public DocumentStore(IOptions<DocumentOptions> options)
    {
        _capacity = Math.Max(1, options.Value.MaxDocumentsInMemory);
    }

    public void Add(OfficeDocument document)
    {
        lock (_gate)
        {
            _documents.Add(document);
            while (_documents.Count > _capacity) _documents.RemoveAt(0);
        }
    }

    public OfficeDocument? Get(Guid id)
    {
        lock (_gate) { return _documents.FirstOrDefault(d => d.Id == id); }
    }
}

public sealed class TokenUsageTracker : ITokenUsageTracker
{
    private readonly object _gate = new();
    private readonly Dictionary<string, AgentUsage> _byAgent = new(StringComparer.OrdinalIgnoreCase);
    private long _input;
    private long _output;
    private long _requests;

    /// <summary>purpose is "agent:{id}" or "orchestrator:{phase}"; usage is grouped per agent.</summary>
    public void Record(string purpose, int inputTokens, int outputTokens)
    {
        var key = purpose.StartsWith("agent:", StringComparison.OrdinalIgnoreCase) ? purpose[6..]
            : purpose.StartsWith("orchestrator", StringComparison.OrdinalIgnoreCase) ? "orchestrator"
            : "other";

        lock (_gate)
        {
            _input += inputTokens;
            _output += outputTokens;
            _requests++;
            var current = _byAgent.GetValueOrDefault(key) ?? new AgentUsage(0, 0, 0);
            _byAgent[key] = new AgentUsage(current.InputTokens + inputTokens, current.OutputTokens + outputTokens, current.Requests + 1);
        }
    }

    public TokenUsageSnapshot GetSnapshot()
    {
        lock (_gate) { return new TokenUsageSnapshot(_input, _output, _requests, new Dictionary<string, AgentUsage>(_byAgent)); }
    }
}

/// <summary>Per-agent exclusive lock + global concurrency slots. Lock order is always agent first, slot second.</summary>
public sealed class ExecutionLimiter : IExecutionLimiter
{
    private readonly SemaphoreSlim _slots;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, SemaphoreSlim> _agentLocks =
        new(StringComparer.OrdinalIgnoreCase);

    public ExecutionLimiter(IOptions<OrchestrationOptions> options)
    {
        var max = Math.Max(1, options.Value.MaxConcurrency);
        _slots = new SemaphoreSlim(max, max);
    }

    public async Task<IAsyncDisposable> EnterAsync(string agentId, CancellationToken cancellationToken)
    {
        var agentLock = _agentLocks.GetOrAdd(agentId, _ => new SemaphoreSlim(1, 1));
        await agentLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await _slots.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            agentLock.Release();
            throw;
        }

        return new Lease(_slots, agentLock);
    }

    private sealed class Lease : IAsyncDisposable
    {
        private SemaphoreSlim? _slots;
        private SemaphoreSlim? _agentLock;

        public Lease(SemaphoreSlim slots, SemaphoreSlim agentLock)
        {
            _slots = slots;
            _agentLock = agentLock;
        }

        public ValueTask DisposeAsync()
        {
            Interlocked.Exchange(ref _slots, null)?.Release();
            Interlocked.Exchange(ref _agentLock, null)?.Release();
            return ValueTask.CompletedTask;
        }
    }
}
