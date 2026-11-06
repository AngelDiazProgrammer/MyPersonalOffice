using AI.Agent.Office.Application.Agents;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Application.Orchestration;
using AI.Agent.Office.Application.Services;
using AI.Agent.Office.Application.Tasks;
using AI.Agent.Office.Domain.Enums;
using AI.Agent.Office.Domain.Events;
using AI.Agent.Office.Domain.Interfaces;
using AI.Agent.Office.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Tests.TestSupport;

public sealed class RecordingPublisher : IOfficeEventPublisher
{
    private readonly object _gate = new();
    private readonly List<OfficeEvent> _events = new();

    public IReadOnlyList<OfficeEvent> Events { get { lock (_gate) { return _events.ToArray(); } } }

    public Task PublishAsync(OfficeEvent officeEvent, CancellationToken cancellationToken = default)
    {
        lock (_gate) { _events.Add(officeEvent); }
        return Task.CompletedTask;
    }
}

/// <summary>Scriptable stand-in for Claude: no API key or network needed.</summary>
public sealed class FakeAnthropicService : IAnthropicService
{
    public bool IsConfigured => true;
    public Func<LlmRequest, CancellationToken, Task<LlmResponse>> Handler { get; set; } =
        (_, _) => Task.FromResult(new LlmResponse("ok", null, 1, 1, "end_turn", "fake"));

    public List<LlmRequest> Requests { get; } = new();

    public Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        lock (Requests) { Requests.Add(request); }
        return Handler(request, cancellationToken);
    }

    public static LlmResponse Decision(string json) => new(string.Empty, json, 10, 10, "tool_use", "fake");
    public static LlmResponse Text(string text) => new(text, null, 10, 10, "end_turn", "fake");
}

public sealed class FakeWordExport : IWordExportService, IPdfExportService
{
    public List<(string Title, string Markdown)> Exports { get; } = new();
    public List<(string Title, string Markdown, DocumentStyle Style)> PdfExports { get; } = new();
    public List<DocumentStyle> WordStyles { get; } = new();

    public Task<AI.Agent.Office.Application.DTOs.GeneratedDocumentDto> ExportAsync(string title, string markdown, DocumentStyle style = DocumentStyle.Default, CancellationToken cancellationToken = default)
    {
        Exports.Add((title, markdown));
        WordStyles.Add(style);
        return Task.FromResult(new AI.Agent.Office.Application.DTOs.GeneratedDocumentDto(Guid.NewGuid(), "doc.docx", 123));
    }

    Task<AI.Agent.Office.Application.DTOs.GeneratedDocumentDto> IPdfExportService.ExportAsync(string title, string markdown, DocumentStyle style, CancellationToken cancellationToken)
    {
        PdfExports.Add((title, markdown, style));
        return Task.FromResult(new AI.Agent.Office.Application.DTOs.GeneratedDocumentDto(Guid.NewGuid(), "doc.pdf", 456));
    }
}

public sealed class FakeKnowledgeBase : IKnowledgeBaseService
{
    public List<AI.Agent.Office.Domain.Entities.KnowledgeEntry> Entries { get; } = new();

    public Task<int> ReloadAsync(CancellationToken cancellationToken = default) => Task.FromResult(Entries.Count);
    public IReadOnlyList<AI.Agent.Office.Domain.Entities.KnowledgeEntry> GetAll() => Entries;
    public AI.Agent.Office.Domain.Entities.KnowledgeEntry? GetByCode(string code) => Entries.FirstOrDefault(e => e.Code == code);

    /// <summary>Tiny matcher for tests: score = number of keywords contained in the query (x5).</summary>
    public IReadOnlyList<KnowledgeHit> Search(string query, int maxResults) => Entries
        .Select(e => new KnowledgeHit(e, e.Keywords.Count(k => query.Contains(k, StringComparison.OrdinalIgnoreCase)) * 5.0))
        .Where(h => h.Score > 0).OrderByDescending(h => h.Score).Take(maxResults).ToList();
}

public sealed class FakeFormatService : IDeliveryFormatService
{
    public DeliveryFormat? Format { get; set; } = new("formato-apa.md", "# Título" + Environment.NewLine + "## Resumen" + Environment.NewLine + "## Desarrollo" + Environment.NewLine + "## Referencias");
    public Task<DeliveryFormat?> GetActiveFormatAsync(CancellationToken cancellationToken = default) => Task.FromResult(Format);
}

public sealed class FakeAgentConfiguration : IAgentConfigurationService
{
    public List<AgentDefinition> Definitions { get; } = new()
    {
        Make("orchestrator", "Orquestador", isOrchestrator: true),
        Make("developer", "Desarrollador", AgentStatus.Coding),
        Make("architect", "Arquitecto"),
        Make("qa-analyst", "Analista QA", AgentStatus.Reviewing),
        Make("pdf-reader", "Lector PDF", AgentStatus.Reading, acceptsDocuments: true),
        Make("word-writer", "Redactor Word", producesWord: true)
    };

    public static AgentDefinition Make(string id, string name, AgentStatus working = AgentStatus.Working,
        bool isOrchestrator = false, bool acceptsDocuments = false, bool producesWord = false) =>
        new(id, name, $"Descripción de {name}", name, id + ".md", $"Instrucciones de {name}",
            new AgentModelSettings(), working, isOrchestrator, acceptsDocuments, "#8b5cf6", "🤖", producesWord);

    public Task<ConfigurationReport> ReloadAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Validate());

    public IReadOnlyList<AgentDefinition> GetDefinitions() => Definitions;
    public AgentDefinition? GetDefinition(string agentId) => Definitions.FirstOrDefault(d => d.Id == agentId);
    public string? GetInstructions(string agentId) => GetDefinition(agentId)?.Instructions;
    public ConfigurationReport Validate() => new(Definitions, Array.Empty<ConfigurationIssue>());
}

/// <summary>Wires the real Application services around fakes for Claude and the realtime channel.</summary>
public sealed class OfficeHarness
{
    public FakeAnthropicService Anthropic { get; } = new();
    public FakeAgentConfiguration Configuration { get; } = new();
    public FakeWordExport Word { get; } = new();
    public FakeKnowledgeBase Knowledge { get; } = new();
    public FakeFormatService Formats { get; } = new();
    public WorkspaceService Workspaces { get; }
    public PendingChoiceStore Pending { get; } = new();
    public RecordingPublisher Publisher { get; } = new();
    public AgentRegistry Registry { get; } = new();
    public TaskStore Tasks { get; } = new();
    public ConversationStore Conversations { get; } = new();
    public DocumentStore Documents { get; }
    public ActivityLog Activity { get; }
    public OfficeEventService Events { get; }
    public AgentStateService State { get; }
    public OrchestrationOptions Options { get; }
    public OrchestrationService Orchestration { get; }

    public OfficeHarness(Action<OrchestrationOptions>? configure = null, Action<KnowledgeOptions>? configureKnowledge = null)
    {
        Options = new OrchestrationOptions { ThinkingDisplayMs = 0, CompletedDisplaySeconds = 60, TaskTimeoutSeconds = 5 };
        configure?.Invoke(Options);
        var options = Microsoft.Extensions.Options.Options.Create(Options);

        Documents = new DocumentStore(Microsoft.Extensions.Options.Options.Create(new DocumentOptions()));
        Activity = new ActivityLog(options);
        Events = new OfficeEventService(Activity, Publisher, NullLogger<OfficeEventService>.Instance);
        State = new AgentStateService(Registry, Events, NullLogger<AgentStateService>.Instance);
        Registry.Load(Configuration.Definitions);

        var runner = new AgentRunner(Configuration, Anthropic, State, options, NullLogger<AgentRunner>.Instance);
        var executor = new PlanExecutor(Tasks, runner, new ExecutionLimiter(options), State, Events, Registry, Word, options,
            NullLogger<PlanExecutor>.Instance);

        Workspaces = new WorkspaceService(Microsoft.Extensions.Options.Options.Create(new WorkspacesOptions()));
        var knowledgeOptions = new KnowledgeOptions { MinScore = 4 };
        configureKnowledge?.Invoke(knowledgeOptions);
        var workflow = new KnowledgeWorkflow(Knowledge, Formats, Configuration, Workspaces,
            Microsoft.Extensions.Options.Options.Create(knowledgeOptions), NullLogger<KnowledgeWorkflow>.Instance);
        _executor = executor;
        _workflow = workflow;
        _options = options;
        Orchestration = CreateOrchestration(Workspaces);
    }

    private readonly PlanExecutor _executor;
    private readonly KnowledgeWorkflow _workflow;
    private readonly IOptions<OrchestrationOptions> _options;

    /// <summary>A second orchestration service sharing everything but the workspace selection (the workflow keeps its own).</summary>
    public OrchestrationService CreateOrchestration(IWorkspaceService workspaces) =>
        new(Anthropic, Configuration, Conversations, Tasks, Documents, State, Events,
            _executor, _workflow, workspaces, Word, Word, Pending, _options, NullLogger<OrchestrationService>.Instance);

    /// <summary>Anthropic fake: planning calls get <paramref name="decisionJson"/>, everything else is answered by <paramref name="agentReply"/>.</summary>
    public void Script(string decisionJson, Func<LlmRequest, CancellationToken, Task<LlmResponse>>? agentReply = null)
    {
        Anthropic.Handler = (request, ct) =>
        {
            if (request.StructuredOutput is not null)
                return Task.FromResult(FakeAnthropicService.Decision(decisionJson));
            return agentReply is not null
                ? agentReply(request, ct)
                : Task.FromResult(FakeAnthropicService.Text("resultado de " + request.Purpose));
        };
    }
}
