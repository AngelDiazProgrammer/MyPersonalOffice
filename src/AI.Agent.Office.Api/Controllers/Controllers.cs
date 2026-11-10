using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AI.Agent.Office.Api.Controllers;

#region Chat (mensajes y cancelación)

/// <summary>Endpoints del chat con el Orquestador.</summary>
[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly IOrchestrationService _orchestration;
    private readonly IOfficeQueryService _query;

    public ChatController(IOrchestrationService orchestration, IOfficeQueryService query)
    {
        _orchestration = orchestration;
        _query = query;
    }

    /// <summary>Envía un mensaje al Orquestador (o una elección de solución) y devuelve la respuesta consolidada.</summary>
    [HttpPost]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting(Extensions.OfficeApiExtensions.ChatRateLimitPolicy)]
    public async Task<ActionResult<ChatResponseDto>> Send([FromBody] ChatRequestDto request, CancellationToken cancellationToken) =>
        Ok(await _orchestration.HandleMessageAsync(request, cancellationToken));

    /// <summary>Devuelve una conversación completa.</summary>
    [HttpGet("{conversationId:guid}")]
    public ActionResult<ConversationDto> Get(Guid conversationId) =>
        _query.GetConversation(conversationId) is { } conversation ? Ok(conversation) : NotFound();

    /// <summary>Cancela la solicitud en curso de la conversación.</summary>
    [HttpPost("{conversationId:guid}/cancel")]
    public IActionResult Cancel(Guid conversationId) =>
        _orchestration.CancelConversationRun(conversationId) ? Accepted() : NotFound();
}

#endregion

#region Agentes

/// <summary>Endpoints de los agentes de la oficina activa.</summary>
[ApiController]
[Route("api/agents")]
public sealed class AgentsController : ControllerBase
{
    private readonly IOfficeQueryService _query;
    private readonly IAgentCatalogService _catalog;

    public AgentsController(IOfficeQueryService query, IAgentCatalogService catalog)
    {
        _query = query;
        _catalog = catalog;
    }

    /// <summary>Lista los agentes de la oficina activa.</summary>
    [HttpGet]
    public ActionResult<IReadOnlyList<AgentDto>> List() => Ok(_query.GetAgents());

    /// <summary>Devuelve un agente por su id.</summary>
    [HttpGet("{agentId}")]
    public ActionResult<AgentDto> Get(string agentId) =>
        _query.GetAgent(agentId) is { } agent ? Ok(agent) : NotFound();

    /// <summary>Actividad reciente de un agente.</summary>
    [HttpGet("{agentId}/activity")]
    public ActionResult<IReadOnlyList<OfficeActivityDto>> Activity(string agentId, [FromQuery] int count = 50) =>
        _query.GetAgent(agentId) is null ? NotFound() : Ok(_query.GetActivity(count, agentId));

    /// <summary>Relee los archivos Markdown de los roles sin reiniciar la aplicación.</summary>
    [HttpPost("reload")]
    public async Task<ActionResult<ConfigurationReportDto>> Reload(CancellationToken cancellationToken)
    {
        var report = await _catalog.ReloadAsync(cancellationToken);
        return Ok(report.ToDto(_query.GetAgents()));
    }
}

#endregion

#region Tareas

/// <summary>Endpoints de las tareas de los agentes.</summary>
[ApiController]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly IOfficeQueryService _query;
    private readonly IOrchestrationService _orchestration;

    public TasksController(IOfficeQueryService query, IOrchestrationService orchestration)
    {
        _query = query;
        _orchestration = orchestration;
    }

    /// <summary>Lista todas las tareas.</summary>
    [HttpGet]
    public ActionResult<IReadOnlyList<AgentTaskDto>> List() => Ok(_query.GetTasks());

    /// <summary>Cancela una tarea en curso.</summary>
    [HttpPost("{taskId:guid}/cancel")]
    public IActionResult Cancel(Guid taskId) => _orchestration.CancelTask(taskId) ? Accepted() : NotFound();
}

#endregion

#region Oficina (estado, consumo y actividad)

/// <summary>Endpoints del estado general de la oficina.</summary>
[ApiController]
[Route("api/office")]
public sealed class OfficeController : ControllerBase
{
    private readonly IOfficeQueryService _query;

    public OfficeController(IOfficeQueryService query) => _query = query;

    /// <summary>Foto completa del estado de la oficina (agentes, tareas, actividad y consumo).</summary>
    [HttpGet("state")]
    public ActionResult<OfficeStateDto> State([FromQuery] int activity = 60) => Ok(_query.GetState(activity));

    /// <summary>Consumo de tokens por agente y proveedor.</summary>
    [HttpGet("usage")]
    public ActionResult<TokenUsageDto> Usage() => Ok(_query.GetUsage());

    /// <summary>Actividad reciente de la oficina.</summary>
    [HttpGet("activity")]
    public ActionResult<IReadOnlyList<OfficeActivityDto>> Activity([FromQuery] int count = 60) => Ok(_query.GetActivity(count));
}

#endregion

#region Base de conocimiento

/// <summary>Endpoints de la base de conocimiento y del formato de entrega.</summary>
[ApiController]
[Route("api/knowledge")]
public sealed class KnowledgeController : ControllerBase
{
    private readonly IKnowledgeBaseService _knowledge;
    private readonly IDeliveryFormatService _formats;

    public KnowledgeController(IKnowledgeBaseService knowledge, IDeliveryFormatService formats)
    {
        _knowledge = knowledge;
        _formats = formats;
    }

    /// <summary>
    /// Situaciones de la base de conocimiento y formato de entrega activo; con "query" muestra qué
    /// situación coincidiría.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? query, CancellationToken cancellationToken)
    {
        var format = await _formats.GetActiveFormatAsync(cancellationToken);
        return Ok(new
        {
            entries = _knowledge.GetAll().Select(e => new { e.Code, e.Title, e.Severity, e.Keywords, e.SourceFile, e.Confidential }),
            activeFormat = format?.FileName,
            matches = string.IsNullOrWhiteSpace(query)
                ? null
                : _knowledge.Search(query, 5).Select(h => new { h.Entry.Code, h.Entry.Title, h.Score })
        });
    }

    /// <summary>Relee los archivos de la base de conocimiento sin reiniciar.</summary>
    [HttpPost("reload")]
    public async Task<IActionResult> Reload(CancellationToken cancellationToken) =>
        Ok(new { entries = await _knowledge.ReloadAsync(cancellationToken) });
}

#endregion

#region Oficinas (selección)

/// <summary>Endpoints de las oficinas (instancias): qué oficinas hay y cuál está abierta.</summary>
[ApiController]
[Route("api/workspaces")]
public sealed class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _workspaces;
    private readonly IOfficeQueryService _query;
    private readonly IOrchestrationService _orchestration;
    private readonly IOfficeEventService _events;

    public WorkspacesController(IWorkspaceService workspaces, IOfficeQueryService query, IOrchestrationService orchestration, IOfficeEventService events)
    {
        _workspaces = workspaces;
        _query = query;
        _orchestration = orchestration;
        _events = events;
    }

    /// <summary>Oficinas disponibles y cuál está abierta (ninguna = la persona debe elegir).</summary>
    [HttpGet]
    public ActionResult<WorkspacesDto> Get() => Ok(_query.GetWorkspaces());

    /// <summary>Abre una oficina y avisa a las interfaces conectadas.</summary>
    [HttpPost("select")]
    public async Task<ActionResult<WorkspacesDto>> Select([FromBody] WorkspaceSelectRequestDto request)
    {
        if (!_workspaces.Select(request.Id))
            throw new OfficeValidationException(new[] { "Oficina desconocida." });
        await _events.EmitAsync(AI.Agent.Office.Domain.Enums.OfficeEventType.WorkspaceChanged, "Oficina abierta: " + _workspaces.Active!.Name);
        return Ok(_query.GetWorkspaces());
    }

    /// <summary>Vuelve a la pantalla de selección de oficina.</summary>
    [HttpPost("reset")]
    public async Task<ActionResult<WorkspacesDto>> Reset()
    {
        _workspaces.Clear();
        await _events.EmitAsync(AI.Agent.Office.Domain.Enums.OfficeEventType.WorkspaceChanged, "Selección de oficina");
        return Ok(_query.GetWorkspaces());
    }
}

#endregion

#region Modelos (enrutamiento)

/// <summary>Endpoints del enrutamiento de modelos.</summary>
[ApiController]
[Route("api/llm")]
public sealed class LlmController : ControllerBase
{
    private readonly ILlmStatusService _status;
    private readonly ILlmRouterState _state;

    public LlmController(ILlmStatusService status, ILlmRouterState state)
    {
        _status = status;
        _state = state;
    }

    /// <summary>Modo de enrutamiento y estado de cada proveedor gratuito.</summary>
    [HttpGet("status")]
    public async Task<ActionResult<LlmStatusDto>> Status(CancellationToken cancellationToken) =>
        Ok(await _status.GetStatusAsync(cancellationToken));

    /// <summary>Prueba un proveedor gratuito (por ejemplo "gemini") con una petición mínima y devuelve si funciona, la latencia o el error.</summary>
    [HttpPost("test")]
    public async Task<ActionResult<LlmProviderTestDto>> TestProvider([FromBody] LlmProviderTestRequestDto request, CancellationToken cancellationToken) =>
        Ok(await _status.TestProviderAsync(request.Provider, request.Model, cancellationToken));

    /// <summary>Cambia en caliente entre FreeFirst, ClaudeOnly y FreeOnly.</summary>
    [HttpPost("mode")]
    public async Task<ActionResult<LlmStatusDto>> SetMode([FromBody] LlmModeRequestDto request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<LlmMode>(request.Mode, ignoreCase: true, out var mode) || !Enum.IsDefined(mode))
            throw new OfficeValidationException(new[] { "Modo no válido. Usa FreeFirst, ClaudeOnly o FreeOnly." });

        _state.Mode = mode;
        return Ok(await _status.GetStatusAsync(cancellationToken));
    }
}

#endregion

#region Documentos (subida, exportación y descarga)

/// <summary>Endpoints de documentos: subir PDF, exportar a Word o PDF y descargar lo generado.</summary>
[ApiController]
[Route("api/documents")]
public sealed class DocumentsController : ControllerBase
{
    private const long RequestLimit = 12 * 1024 * 1024;
    private readonly IDocumentService _documents;

    private readonly IWordExportService _word;
    private readonly IPdfExportService _pdf;
    private readonly IGeneratedDocumentStore _generated;

    public DocumentsController(IDocumentService documents, IWordExportService word, IPdfExportService pdf, IGeneratedDocumentStore generated)
    {
        _pdf = pdf;
        _documents = documents;
        _word = word;
        _generated = generated;
    }

    /// <summary>Exporta cualquier Markdown (por ejemplo el resultado de un agente) a un archivo Word.</summary>
    [HttpPost("word")]
    public async Task<ActionResult<GeneratedDocumentDto>> ExportWord([FromBody] WordExportRequestDto request, CancellationToken cancellationToken) =>
        Ok(await _word.ExportAsync(request.Title, request.Markdown, ParseStyle(request.Style), cancellationToken));

    /// <summary>Exporta Markdown a un PDF (estilo "apa" o estándar).</summary>
    [HttpPost("pdf")]
    public async Task<ActionResult<GeneratedDocumentDto>> ExportPdf([FromBody] WordExportRequestDto request, CancellationToken cancellationToken) =>
        Ok(await _pdf.ExportAsync(request.Title, request.Markdown, ParseStyle(request.Style), cancellationToken));

    /// <summary>Interpreta el estilo pedido: "apa" o el estándar.</summary>
    private static DocumentStyle ParseStyle(string? style) =>
        string.Equals(style, "apa", StringComparison.OrdinalIgnoreCase) ? DocumentStyle.Apa : DocumentStyle.Default;

    /// <summary>Descarga un documento generado (Word o PDF) por su id.</summary>
    [HttpGet("generated/{id:guid}")]
    public IActionResult Download(Guid id)
    {
        var document = _generated.Get(id);
        if (document is null) return NotFound();
        var contentType = document.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        return PhysicalFile(document.FilePath, contentType, document.FileName);
    }

    /// <summary>Valida un PDF y extrae su texto. El archivo en sí nunca se guarda.</summary>
    [HttpPost]
    [RequestSizeLimit(RequestLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = RequestLimit)]
    public async Task<ActionResult<DocumentDto>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null) throw new OfficeValidationException(new[] { "No se recibió ningún archivo." });

        await using var stream = file.OpenReadStream();
        var result = await _documents.UploadAsync(stream, file.FileName, file.ContentType, file.Length, cancellationToken);
        return Ok(result);
    }
#endregion

}
