using ATLAS.Application.Agents;

namespace ATLAS.Application.Interfaces;

using ATLAS.Domain.Entities;
using Agent = ATLAS.Domain.Entities.Agent;

/// <summary>Registro de los agentes y su estado en ejecución.</summary>
public interface IAgentRegistry
{
    void Load(IEnumerable<AgentDefinition> definitions);
    IReadOnlyList<Agent> GetAll();
    Agent? Get(string agentId);
    /// <summary>Cambia un agente de forma atómica y devuelve una copia, o null si no existe.</summary>
    Agent? Update(string agentId, Action<Agent> mutation);
}

/// <summary>Almacén de tareas de los agentes y de su cancelación.</summary>
public interface ITaskStore
{
    void Add(AgentTask task);
    AgentTask? Get(Guid taskId);
    IReadOnlyList<AgentTask> GetAll();
    IReadOnlyList<AgentTask> GetByConversation(Guid conversationId);
    AgentTask? Update(Guid taskId, Action<AgentTask> mutation);
    void RegisterCancellation(Guid taskId, CancellationTokenSource source);
    void UnregisterCancellation(Guid taskId);
    /// <summary>Cancela una tarea pendiente o en curso. Devuelve false si no estaba activa.</summary>
    bool RequestCancel(Guid taskId);
    bool WasCancelledByUser(Guid taskId);
}

/// <summary>Almacén de conversaciones y de las ejecuciones en curso.</summary>
public interface IConversationStore
{
    Conversation GetOrCreate(Guid? conversationId);
    Conversation? Get(Guid conversationId);
    void RegisterRun(Guid conversationId, CancellationTokenSource source);
    void UnregisterRun(Guid conversationId, CancellationTokenSource source);
    bool CancelRun(Guid conversationId);
}

/// <summary>Bitácora de eventos de la oficina.</summary>
public interface IActivityLog
{
    void Add(OfficeActivity activity);
    IReadOnlyList<OfficeActivity> GetRecent(int count, string? agentId = null);
}

/// <summary>Almacén en memoria del texto de los documentos subidos.</summary>
public interface IDocumentStore
{
    void Add(OfficeDocument document);
    OfficeDocument? Get(Guid id);
}

/// <summary>Contador de tokens consumidos.</summary>
public interface ITokenUsageTracker
{
    /// <summary>Quién respondió ("claude", "groq", "cloudflare"...), para el desglose por proveedor.</summary>
    void Record(string purpose, int inputTokens, int outputTokens, bool free = false, string? provider = null);
    TokenUsageSnapshot GetSnapshot();
}

/// <summary>Consumo de un agente o proveedor: tokens de entrada y salida y número de llamadas.</summary>
public sealed record AgentUsage(long InputTokens, long OutputTokens, long Requests);

/// <summary>Foto del consumo acumulado (total, gratuito, por agente y por proveedor).</summary>
public sealed record TokenUsageSnapshot(
    long InputTokens, long OutputTokens, long Requests, IReadOnlyDictionary<string, AgentUsage> ByAgent,
    long FreeInputTokens = 0, long FreeOutputTokens = 0, long FreeRequests = 0,
    IReadOnlyDictionary<string, AgentUsage>? ByProvider = null);
