using ATLAS.Domain.Enums;

namespace ATLAS.Application.DTOs;

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

/// <summary>Resultado de probar un proveedor gratuito con una petición mínima.</summary>
public sealed record LlmProviderTestDto(string Provider, string Model, bool Ok, long LatencyMs, string? Reply, string? Error);

/// <summary>Solicitud para probar un proveedor gratuito por su nombre.</summary>
public sealed record LlmProviderTestRequestDto(string Provider, string? Model = null);

/// <summary>Agente de una oficina, para mostrarlo en la pantalla de selección.</summary>
public sealed record WorkspaceAgentDto(string Id, string Name, string Icon, string Role);

/// <summary>Una oficina disponible con sus agentes.</summary>
public sealed record WorkspaceDto(string Id, string Name, string Icon, string Description, IReadOnlyList<WorkspaceAgentDto> Agents, string Kind = "");

/// <summary>Required = la persona debe elegir una oficina antes de cualquier otra cosa.</summary>
public sealed record WorkspacesDto(IReadOnlyList<WorkspaceDto> Available, string? ActiveId, bool Required);

/// <summary>Solicitud para abrir una oficina.</summary>
public sealed record WorkspaceSelectRequestDto(string Id);

/// <summary>Foto completa del estado de la oficina.</summary>
public sealed record OfficeStateDto(
    IReadOnlyList<AgentDto> Agents, IReadOnlyList<AgentTaskDto> Tasks,
    IReadOnlyList<OfficeActivityDto> Activity, TokenUsageDto Usage, bool AnthropicConfigured, string? ActiveWorkspaceId = null);

/// <summary><paramref name="Target"/>: «bot:&lt;id&gt;» o «agent:&lt;id&gt;» para los entornos de Camila (spec 012); vacío = la oficina de agentes abierta.</summary>
public sealed record ChatRequestDto(string Message, Guid? ConversationId = null, IReadOnlyList<Guid>? DocumentIds = null, string? ChoiceOptionId = null, string? Target = null);

/// <summary>Una solución de la base que la persona puede elegir.</summary>
public sealed record SolutionOptionDto(string Id, string Title, string Summary, bool Recommended);

/// <summary>
/// Opciones pendientes de elegir para un caso (la interfaz muestra botones y un campo para escribir otra solución).
/// <paramref name="MenuOnly"/> = solo botones (los de una página del bot Camila): sin campo de solución propia ni «Cancelar caso».
/// </summary>
public sealed record KnowledgeChoiceDto(string Code, string Title, string? RecommendedId, IReadOnlyList<SolutionOptionDto> Options, bool MenuOnly = false);

/// <summary>Respuesta del chat: texto, tareas, documentos generados y, si corresponde, las opciones de solución.</summary>
public sealed record ChatResponseDto(
    Guid ConversationId, string Reply, bool Delegated, bool IsError,
    string? PlanMessage, IReadOnlyList<AgentTaskDto> Tasks,
    IReadOnlyList<GeneratedDocumentDto>? Documents = null,
    KnowledgeChoiceDto? Choice = null,
    BrowserApprovalDto? BrowserApproval = null);

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

#region Conectividad a navegadores (spec 007)

/// <summary>Un sitio configurado para la conectividad a navegadores.</summary>
public sealed record BrowserSiteDto(string Id, string Name, string Url, IReadOnlyList<string> AllowedDomains, bool? Headless, bool Enabled, bool HasProposal, bool HasCredentials = false, string? Username = null);

/// <summary>Solicitud para crear o actualizar un sitio.</summary>
public sealed record BrowserSiteRequestDto(string Id, string Name, string Url, IReadOnlyList<string>? AllowedDomains = null, bool? Headless = null, bool Enabled = true, string? Username = null, string? Password = null);

/// <summary>Entrada de un playbook para mostrarla en el front.</summary>
public sealed record PlaybookInputDto(string Name, string Question, bool Draft);

/// <summary>Playbook de un sitio: texto, problemas de validación, entradas y propuesta aprendida (si hay).</summary>
public sealed record PlaybookDto(string SiteId, string Markdown, IReadOnlyList<string> Errors, IReadOnlyList<PlaybookInputDto> Inputs, string? Proposal);

/// <summary>Solicitud para guardar el texto de un playbook o de su propuesta.</summary>
public sealed record PlaybookTextRequestDto(string Markdown);

/// <summary>Credenciales opcionales de una ejecución (en memoria; nunca se guardan).</summary>
public sealed record BrowserCredentialsDto(string? Username, string? Password);

/// <summary>Solicitud de ejecución directa: valores de las entradas y credenciales opcionales.</summary>
public sealed record BrowserRunRequestDto(IReadOnlyDictionary<string, string>? Values = null, BrowserCredentialsDto? Credentials = null);

/// <summary>Resultado de una ejecución de navegador.</summary>
public sealed record BrowserRunResultDto(bool Success, string Message, long DurationMs, int StepsExecuted, string Mode, int ModelCalls, bool ProposalCreated, IReadOnlyList<string> Log, IReadOnlyList<ExtractedRowDto>? Data = null);

/// <summary>Fila leída de la página (por ejemplo un correo).</summary>
public sealed record ExtractedRowDto(string Text, string? Sender, string? Email, string? Subject, string? Snippet, string? Date);

/// <summary>Campo del borrador que la persona debe aprobar.</summary>
public sealed record BrowserFieldDto(string Name, string Value);

/// <summary>Borrador pendiente de aprobación en el chat: la persona aprueba (con credenciales opcionales) o cancela.</summary>
public sealed record BrowserApprovalDto(Guid ConversationId, string SiteId, string SiteName, string Title, IReadOnlyList<BrowserFieldDto> Fields, bool Irreversible);

/// <summary>Respuesta a una aprobación: el mensaje para el chat y el resultado de la ejecución.</summary>
public sealed record BrowserApproveResponseDto(string Reply, BrowserRunResultDto? Result);

#endregion

#region Registros en vivo (spec 010)

/// <summary>Una entrada del registro en vivo. No contiene datos personales ni cuerpos de peticiones (spec 010, RF-013).</summary>
public sealed record LogEntryDto(long Seq, DateTimeOffset Time, string Level, string Source, string Message, string? ConversationId);

/// <summary>Entradas nuevas desde <c>after</c> y el último número de secuencia (para pedir solo lo nuevo).</summary>
public sealed record LogsDto(IReadOnlyList<LogEntryDto> Entries, long Last);

#endregion
