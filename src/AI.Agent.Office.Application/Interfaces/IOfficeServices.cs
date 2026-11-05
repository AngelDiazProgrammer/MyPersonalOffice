using AI.Agent.Office.Application.Agents;
using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Domain.Entities;
using AI.Agent.Office.Domain.Enums;
using AI.Agent.Office.Domain.Events;

namespace AI.Agent.Office.Application.Interfaces;

public interface IOfficeEventService
{
    Task<OfficeEvent> EmitAsync(
        OfficeEventType type,
        string description,
        string? agentId = null,
        Guid? taskId = null,
        IDictionary<string, string?>? data = null);
}

public interface IAgentStateService
{
    /// <summary>Updates an agent (null status = leave unchanged) and emits the matching event carrying the new state.</summary>
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

public interface IAgentCatalogService
{
    Task<ConfigurationReport> ReloadAsync(CancellationToken cancellationToken = default);
}

public interface IAgentRunner
{
    Task<string> RunAsync(AgentTask task, string prompt, CancellationToken cancellationToken);
}

public interface IExecutionLimiter
{
    /// <summary>Acquires the agent's exclusive lock and one global concurrency slot.</summary>
    Task<IAsyncDisposable> EnterAsync(string agentId, CancellationToken cancellationToken);
}

public interface IDocumentTextExtractor
{
    Task<DocumentExtractionResult> ExtractAsync(Stream content, string fileName, int maxPages, int maxChars, CancellationToken cancellationToken);
}

public sealed record DocumentExtractionResult(string Text, int PageCount, bool Truncated, bool RequiresOcr);

public interface IDocumentService
{
    Task<DocumentDto> UploadAsync(Stream content, string fileName, string? contentType, long length, CancellationToken cancellationToken);
}

public interface IOrchestrationService
{
    Task<ChatResponseDto> HandleMessageAsync(ChatRequestDto request, CancellationToken cancellationToken);
    bool CancelConversationRun(Guid conversationId);
    bool CancelTask(Guid taskId);
}

public interface IOfficeQueryService
{
    TokenUsageDto GetUsage();
    OfficeStateDto GetState(int activityCount = 60);
    IReadOnlyList<AgentDto> GetAgents();
    AgentDto? GetAgent(string agentId);
    IReadOnlyList<AgentTaskDto> GetTasks();
    IReadOnlyList<OfficeActivityDto> GetActivity(int count, string? agentId = null);
    ConversationDto? GetConversation(Guid id);
}

public class OfficeValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public OfficeValidationException(IEnumerable<string> errors) : base("Solicitud no válida.")
    {
        Errors = errors.ToArray();
    }
}
