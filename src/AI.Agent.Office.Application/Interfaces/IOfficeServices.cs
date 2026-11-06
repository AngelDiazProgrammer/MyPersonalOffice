using AI.Agent.Office.Application.Agents;
using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Domain.Entities;
using AI.Agent.Office.Domain.Enums;
using AI.Agent.Office.Domain.Events;

namespace AI.Agent.Office.Application.Interfaces;

/// <summary>Emite eventos de la oficina (los registra y los publica).</summary>
public interface IOfficeEventService
{
    Task<OfficeEvent> EmitAsync(
        OfficeEventType type,
        string description,
        string? agentId = null,
        Guid? taskId = null,
        IDictionary<string, string?>? data = null);
}

/// <summary>Único camino para cambiar el estado de un agente.</summary>
public interface IAgentStateService
{
    /// <summary>
    /// Actualiza un agente (estado null = sin cambio) y emite el evento correspondiente con el nuevo
    /// estado.
    /// </summary>
    Task SetAsync(
        string agentId,
        AgentStatus? status,
        OfficeEventType eventType,
        string description,
        Guid? taskId = null,
        string? taskTitle = null,
        bool clearTask = false,
        IDictionary<string, string?>? data = null);

    void ReturnToIdleAfter(string agentId, TimeSpan delay);
}

/// <summary>Carga los agentes desde sus archivos Markdown al registro.</summary>
public interface IAgentCatalogService
{
    Task<ConfigurationReport> ReloadAsync(CancellationToken cancellationToken = default);
}

/// <summary>Resultado de ejecutar un agente: el texto y el proveedor que respondió.</summary>
public sealed record AgentRunResult(string Text, string? Provider);

/// <summary>Ejecuta una tarea con un agente.</summary>
public interface IAgentRunner
{
    /// <param name="containsDocuments">El prompt incluye documentos del usuario: solo puede ir a Claude.</param>
    Task<AgentRunResult> RunAsync(AgentTask task, string prompt, bool containsDocuments, CancellationToken cancellationToken);
}

/// <summary>Limita la ejecución: un agente a la vez y un máximo de agentes simultáneos.</summary>
public interface IExecutionLimiter
{
    /// <summary>Toma el candado exclusivo del agente y un cupo de concurrencia global.</summary>
    Task<IAsyncDisposable> EnterAsync(string agentId, CancellationToken cancellationToken);
}

/// <summary>Extrae el texto de un documento (PDF).</summary>
public interface IDocumentTextExtractor
{
    Task<DocumentExtractionResult> ExtractAsync(Stream content, string fileName, int maxPages, int maxChars, CancellationToken cancellationToken);
}

/// <summary>Texto extraído de un documento, con su número de páginas y si fue truncado o requiere OCR.</summary>
public sealed record DocumentExtractionResult(string Text, int PageCount, bool Truncated, bool RequiresOcr);

/// <summary>Recibe y valida los documentos subidos.</summary>
public interface IDocumentService
{
    Task<DocumentDto> UploadAsync(Stream content, string fileName, string? contentType, long length, CancellationToken cancellationToken);
}

/// <summary>Atiende los mensajes del chat y coordina a los agentes.</summary>
public interface IOrchestrationService
{
    Task<ChatResponseDto> HandleMessageAsync(ChatRequestDto request, CancellationToken cancellationToken);
    bool CancelConversationRun(Guid conversationId);
    bool CancelTask(Guid taskId);
}

/// <summary>Consultas de solo lectura sobre la oficina.</summary>
public interface IOfficeQueryService
{
    TokenUsageDto GetUsage();
    WorkspacesDto GetWorkspaces();
    OfficeStateDto GetState(int activityCount = 60);
    IReadOnlyList<AgentDto> GetAgents();
    AgentDto? GetAgent(string agentId);
    IReadOnlyList<AgentTaskDto> GetTasks();
    IReadOnlyList<OfficeActivityDto> GetActivity(int count, string? agentId = null);
    ConversationDto? GetConversation(Guid id);
}

/// <summary>Error de validación de una solicitud; lleva la lista de problemas para mostrarlos al usuario.</summary>
public class OfficeValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public OfficeValidationException(IEnumerable<string> errors) : base("Solicitud no válida.")
    {
        Errors = errors.ToArray();
    }
}
