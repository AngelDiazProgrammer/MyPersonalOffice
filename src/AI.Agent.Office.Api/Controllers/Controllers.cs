using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AI.Agent.Office.Api.Controllers;

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

    /// <summary>Sends a message to the Orchestrator and returns the consolidated answer.</summary>
    [HttpPost]
    public async Task<ActionResult<ChatResponseDto>> Send([FromBody] ChatRequestDto request, CancellationToken cancellationToken) =>
        Ok(await _orchestration.HandleMessageAsync(request, cancellationToken));

    [HttpGet("{conversationId:guid}")]
    public ActionResult<ConversationDto> Get(Guid conversationId) =>
        _query.GetConversation(conversationId) is { } conversation ? Ok(conversation) : NotFound();

    [HttpPost("{conversationId:guid}/cancel")]
    public IActionResult Cancel(Guid conversationId) =>
        _orchestration.CancelConversationRun(conversationId) ? Accepted() : NotFound();
}

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

    [HttpGet]
    public ActionResult<IReadOnlyList<AgentDto>> List() => Ok(_query.GetAgents());

    [HttpGet("{agentId}")]
    public ActionResult<AgentDto> Get(string agentId) =>
        _query.GetAgent(agentId) is { } agent ? Ok(agent) : NotFound();

    [HttpGet("{agentId}/activity")]
    public ActionResult<IReadOnlyList<OfficeActivityDto>> Activity(string agentId, [FromQuery] int count = 50) =>
        _query.GetAgent(agentId) is null ? NotFound() : Ok(_query.GetActivity(count, agentId));

    /// <summary>Re-reads the Markdown role files without restarting the application.</summary>
    [HttpPost("reload")]
    public async Task<ActionResult<ConfigurationReportDto>> Reload(CancellationToken cancellationToken)
    {
        var report = await _catalog.ReloadAsync(cancellationToken);
        return Ok(report.ToDto(_query.GetAgents()));
    }
}

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

    [HttpGet]
    public ActionResult<IReadOnlyList<AgentTaskDto>> List() => Ok(_query.GetTasks());

    [HttpPost("{taskId:guid}/cancel")]
    public IActionResult Cancel(Guid taskId) => _orchestration.CancelTask(taskId) ? Accepted() : NotFound();
}

[ApiController]
[Route("api/office")]
public sealed class OfficeController : ControllerBase
{
    private readonly IOfficeQueryService _query;

    public OfficeController(IOfficeQueryService query) => _query = query;

    [HttpGet("state")]
    public ActionResult<OfficeStateDto> State([FromQuery] int activity = 60) => Ok(_query.GetState(activity));

    [HttpGet("usage")]
    public ActionResult<TokenUsageDto> Usage() => Ok(_query.GetUsage());

    [HttpGet("activity")]
    public ActionResult<IReadOnlyList<OfficeActivityDto>> Activity([FromQuery] int count = 60) => Ok(_query.GetActivity(count));
}

[ApiController]
[Route("api/documents")]
public sealed class DocumentsController : ControllerBase
{
    private const long RequestLimit = 12 * 1024 * 1024;
    private readonly IDocumentService _documents;

    public DocumentsController(IDocumentService documents) => _documents = documents;

    /// <summary>Validates and extracts the text of a PDF. The file itself is never kept.</summary>
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
}
