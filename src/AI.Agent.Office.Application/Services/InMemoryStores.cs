using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Domain.Entities;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Application.Services;

#region Conversaciones

/// <summary>Almacén en memoria de conversaciones y de las ejecuciones en curso (para poder cancelarlas).</summary>
public sealed class ConversationStore : IConversationStore
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, Conversation> _conversations = new();
    private readonly Dictionary<Guid, CancellationTokenSource> _runs = new();
    private readonly Queue<Guid> _order = new();

    /// <summary>Máximo de conversaciones en memoria: al superarlo se descartan las más viejas que no estén en curso.</summary>
    public const int MaxConversations = 300;

    /// <summary>Devuelve la conversación indicada o crea una nueva si no existe.</summary>
    public Conversation GetOrCreate(Guid? conversationId)
    {
        lock (_gate)
        {
            if (conversationId is { } id && _conversations.TryGetValue(id, out var existing)) return existing;
            var conversation = conversationId is { } wanted ? new Conversation { Id = wanted } : new Conversation();
            _conversations[conversation.Id] = conversation;
            _order.Enqueue(conversation.Id);
            TrimOldConversations();
            return conversation;
        }
    }

    /// <summary>Descarta las conversaciones más antiguas (sin ejecución en curso) para que la memoria no crezca sin límite.</summary>
    private void TrimOldConversations()
    {
        var attempts = _order.Count;
        while (_conversations.Count > MaxConversations && attempts-- > 0)
        {
            var oldest = _order.Dequeue();
            if (_runs.ContainsKey(oldest)) _order.Enqueue(oldest);       // sigue en curso: se conserva
            else _conversations.Remove(oldest);
        }
    }

    /// <summary>Busca una conversación por su id.</summary>
    public Conversation? Get(Guid conversationId)
    {
        lock (_gate) { return _conversations.GetValueOrDefault(conversationId); }
    }

    /// <summary>Registra la ejecución en curso de una conversación.</summary>
    public void RegisterRun(Guid conversationId, CancellationTokenSource source)
    {
        lock (_gate) { _runs[conversationId] = source; }
    }

    /// <summary>Quita la ejecución registrada al terminar.</summary>
    public void UnregisterRun(Guid conversationId, CancellationTokenSource source)
    {
        lock (_gate)
        {
            if (_runs.TryGetValue(conversationId, out var current) && ReferenceEquals(current, source))
                _runs.Remove(conversationId);
        }
    }

    /// <summary>Cancela la ejecución en curso; devuelve false si no había ninguna.</summary>
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

#endregion

#region Registro de actividad

/// <summary>Bitácora en memoria con los últimos eventos de la oficina (se descartan los más viejos).</summary>
public sealed class ActivityLog : IActivityLog
{
    private readonly object _gate = new();
    private readonly LinkedList<OfficeActivity> _items = new();
    private readonly int _capacity;

    public ActivityLog(IOptions<OrchestrationOptions> options)
    {
        _capacity = Math.Max(50, options.Value.MaxActivityEntries);
    }

    /// <summary>Agrega un evento a la bitácora.</summary>
    public void Add(OfficeActivity activity)
    {
        lock (_gate)
        {
            _items.AddFirst(activity);
            while (_items.Count > _capacity) _items.RemoveLast();
        }
    }

    /// <summary>Últimos eventos, del más nuevo al más viejo; opcionalmente solo los de un agente.</summary>
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

#endregion

#region Documentos adjuntos

/// <summary>Guarda en memoria el texto extraído de los PDF subidos (nunca el archivo) y descarta los más viejos.</summary>
public sealed class DocumentStore : IDocumentStore
{
    private readonly object _gate = new();
    private readonly List<OfficeDocument> _documents = new();
    private readonly int _capacity;

    public DocumentStore(IOptions<DocumentOptions> options)
    {
        _capacity = Math.Max(1, options.Value.MaxDocumentsInMemory);
    }

    /// <summary>Guarda un documento; si se supera el máximo se elimina el más antiguo.</summary>
    public void Add(OfficeDocument document)
    {
        lock (_gate)
        {
            _documents.Add(document);
            while (_documents.Count > _capacity) _documents.RemoveAt(0);
        }
    }

    /// <summary>Busca un documento por su id.</summary>
    public OfficeDocument? Get(Guid id)
    {
        lock (_gate) { return _documents.FirstOrDefault(d => d.Id == id); }
    }
}

#endregion

#region Consumo de tokens

/// <summary>
/// Contador de tokens y llamadas desde que arrancó la app, total, por agente y por proveedor (Claude o
/// APIs gratuitas).
/// </summary>
public sealed class TokenUsageTracker : ITokenUsageTracker
{
    private readonly object _gate = new();
    private readonly Dictionary<string, AgentUsage> _byAgent = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AgentUsage> _byProvider = new(StringComparer.OrdinalIgnoreCase);
    private long _input;
    private long _output;
    private long _requests;
    private long _freeInput;
    private long _freeOutput;
    private long _freeRequests;

    /// <summary>
    /// Registra un consumo. "purpose" es "agent:{id}" u "orchestrator:{fase}"; el consumo se agrupa por
    /// agente.
    /// </summary>
    public void Record(string purpose, int inputTokens, int outputTokens, bool free = false, string? provider = null)
    {
        var key = purpose.StartsWith("agent:", StringComparison.OrdinalIgnoreCase) ? purpose[6..]
            : purpose.StartsWith("orchestrator", StringComparison.OrdinalIgnoreCase) ? "orchestrator"
            : "other";

        lock (_gate)
        {
            _input += inputTokens;
            _output += outputTokens;
            _requests++;
            var providerKey = string.IsNullOrWhiteSpace(provider) ? (free ? "free" : "claude") : provider;
            var byProvider = _byProvider.GetValueOrDefault(providerKey) ?? new AgentUsage(0, 0, 0);
            _byProvider[providerKey] = new AgentUsage(byProvider.InputTokens + inputTokens, byProvider.OutputTokens + outputTokens, byProvider.Requests + 1);
            if (free) { _freeInput += inputTokens; _freeOutput += outputTokens; _freeRequests++; }
            var current = _byAgent.GetValueOrDefault(key) ?? new AgentUsage(0, 0, 0);
            _byAgent[key] = new AgentUsage(current.InputTokens + inputTokens, current.OutputTokens + outputTokens, current.Requests + 1);
        }
    }

    /// <summary>Foto del consumo acumulado hasta ahora.</summary>
    public TokenUsageSnapshot GetSnapshot()
    {
        lock (_gate) { return new TokenUsageSnapshot(_input, _output, _requests, new Dictionary<string, AgentUsage>(_byAgent), _freeInput, _freeOutput, _freeRequests, new Dictionary<string, AgentUsage>(_byProvider)); }
    }
}

#endregion

#region Límite de concurrencia

/// <summary>
/// Un candado exclusivo por agente más un número máximo de ejecuciones simultáneas globales. El orden
/// es siempre: primero el agente, después el cupo.
/// </summary>
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

    /// <summary>Espera el turno del agente y un cupo global; al liberar el resultado se devuelven ambos.</summary>
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

    /// <summary>Permiso que libera el candado del agente y el cupo al desecharse.</summary>
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
#endregion

}
