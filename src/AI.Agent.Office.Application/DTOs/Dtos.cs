using AI.Agent.Office.Domain.Enums;

namespace AI.Agent.Office.Application.DTOs;

/// <summary>Datos de un agente para la interfaz.</summary>
public sealed record AgentDto(
    string Id, string Name, string Description, string Role, AgentStatus Status,
    Guid? CurrentTaskId, string? CurrentTaskTitle, string ConfigurationFile,
    DateTimeOffset? LastActivityAt, string Color, string Icon, bool IsOrchestrator, bool AcceptsDocuments, string? Model);

/// <summary>Datos de una tarea para la interfaz.</summary>
public sealed record AgentTaskDto(
    Guid Id, Guid ConversationId, string AgentId, string Key, string Title, string Description,
    TaskStatus Status, DateTimeOffset CreatedAt, DateTimeOffset? StartedAt, DateTimeOffset? CompletedAt,
    string? Result, string? ErrorMessage, IReadOnlyList<string> DependsOn,
    Guid? DocumentId = null, string? DocumentName = null, string? Provider = null);

/// <summary>Un mensaje del chat.</summary>
public sealed record ChatMessageDto(Guid Id, ChatRole Role, string Content, DateTimeOffset CreatedAt);

/// <summary>Una conversación completa.</summary>
public sealed record ConversationDto(Guid Id, DateTimeOffset CreatedAt, IReadOnlyList<ChatMessageDto> Messages);

/// <summary>Una entrada de la bitácora de actividad.</summary>
public sealed record OfficeActivityDto(Guid Id, string? AgentId, Guid? TaskId, OfficeEventType EventType, string Description, DateTimeOffset Timestamp);

/// <summary>Evento en tiempo real que recibe la interfaz.</summary>
public sealed record OfficeEventDto(
    Guid EventId, string? AgentId, Guid? TaskId, OfficeEventType EventType, string Description,
    DateTimeOffset Timestamp, IReadOnlyDictionary<string, string?>? AdditionalData);

/// <summary>Consumo de tokens de un agente.</summary>
public sealed record AgentUsageDto(string AgentId, long InputTokens, long OutputTokens, long Requests);

/// <summary>
/// Los totales incluyen el consumo de Claude y de las APIs gratuitas; los campos Free* son el
/// subconjunto gratuito.
/// </summary>
public sealed record TokenUsageDto(
    long InputTokens, long OutputTokens, long Requests,
    IReadOnlyList<AgentUsageDto> ByAgent, decimal? EstimatedCostUsd,
    long FreeInputTokens = 0, long FreeOutputTokens = 0, long FreeRequests = 0,
    IReadOnlyList<ProviderUsageDto>? ByProvider = null);

/// <summary>Consumo de un proveedor ("claude", "groq", "cloudflare"...).</summary>
public sealed record ProviderUsageDto(string Provider, bool IsFree, long InputTokens, long OutputTokens, long Requests);

/// <summary>Estado de un proveedor gratuito: configurado, en pausa por límite de uso y último error.</summary>
public sealed record FreeProviderStatusDto(string Name, string Region, string Model, bool Configured, bool CoolingDown, string? Error = null);

/// <summary>Modo de enrutamiento y estado de los proveedores.</summary>
public sealed record LlmStatusDto(string Mode, IReadOnlyList<FreeProviderStatusDto> FreeProviders, bool ClaudeConfigured);

/// <summary>Solicitud para cambiar el modo de enrutamiento.</summary>
public sealed record LlmModeRequestDto(string Mode);

/// <summary>Agente de una oficina, para mostrarlo en la pantalla de selección.</summary>
public sealed record WorkspaceAgentDto(string Id, string Name, string Icon, string Role);

/// <summary>Una oficina disponible con sus agentes.</summary>
public sealed record WorkspaceDto(string Id, string Name, string Icon, string Description, IReadOnlyList<WorkspaceAgentDto> Agents);

/// <summary>Required = la persona debe elegir una oficina antes de cualquier otra cosa.</summary>
public sealed record WorkspacesDto(IReadOnlyList<WorkspaceDto> Available, string? ActiveId, bool Required);

/// <summary>Solicitud para abrir una oficina.</summary>
public sealed record WorkspaceSelectRequestDto(string Id);

/// <summary>Foto completa del estado de la oficina.</summary>
public sealed record OfficeStateDto(
    IReadOnlyList<AgentDto> Agents, IReadOnlyList<AgentTaskDto> Tasks,
    IReadOnlyList<OfficeActivityDto> Activity, TokenUsageDto Usage, bool AnthropicConfigured, string? ActiveWorkspaceId = null);

public sealed record ChatRequestDto(string Message, Guid? ConversationId = null, IReadOnlyList<Guid>? DocumentIds = null, string? ChoiceOptionId = null);

/// <summary>Una solución de la base que la persona puede elegir.</summary>
public sealed record SolutionOptionDto(string Id, string Title, string Summary, bool Recommended);

/// <summary>Opciones pendientes de elegir para un caso (la interfaz muestra botones y un campo para escribir otra solución).</summary>
public sealed record KnowledgeChoiceDto(string Code, string Title, string? RecommendedId, IReadOnlyList<SolutionOptionDto> Options);

/// <summary>Respuesta del chat: texto, tareas, documentos generados y, si corresponde, las opciones de solución.</summary>
public sealed record ChatResponseDto(
    Guid ConversationId, string Reply, bool Delegated, bool IsError,
    string? PlanMessage, IReadOnlyList<AgentTaskDto> Tasks,
    IReadOnlyList<GeneratedDocumentDto>? Documents = null,
    KnowledgeChoiceDto? Choice = null);

/// <summary>Documento generado listo para descargar.</summary>
public sealed record GeneratedDocumentDto(Guid Id, string FileName, long SizeBytes);

/// <summary>Solicitud para exportar un Markdown a Word o PDF (estilo "apa" opcional).</summary>
public sealed record WordExportRequestDto(string Title, string Markdown, string? Style = null);

/// <summary>Documento subido: nombre, páginas y si requiere OCR o fue truncado.</summary>
public sealed record DocumentDto(Guid Id, string FileName, int PageCount, int CharacterCount, bool Truncated, bool RequiresOcr);

/// <summary>Resultado de validar los agentes.</summary>
public sealed record ConfigurationReportDto(IReadOnlyList<AgentDto> Agents, IReadOnlyList<ConfigurationIssueDto> Issues, bool IsValid);

/// <summary>Un problema de configuración de un agente.</summary>
public sealed record ConfigurationIssueDto(string AgentId, string Message, bool IsError);
