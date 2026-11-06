using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Application.Orchestration;
using AI.Agent.Office.Application.Services;
using AI.Agent.Office.Application.Validators;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Domain.Enums;
using AI.Agent.Office.Infrastructure.Agents;
using AI.Agent.Office.Infrastructure.Configuration;
using AI.Agent.Office.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;

namespace AI.Agent.Office.Tests;

public sealed class TempAgentsDirectory : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "office-tests-" + Guid.NewGuid().ToString("N"));

    public TempAgentsDirectory() => Directory.CreateDirectory(Path);

    public void Write(string file, string content) => File.WriteAllText(System.IO.Path.Combine(Path, file), content);

    public MarkdownAgentConfigurationService CreateService(params (string Id, string File)[] registrations) =>
        new(Microsoft.Extensions.Options.Options.Create(new AgentsOptions
        {
            Directory = Path,
            Definitions = registrations.Select(r => new AgentRegistration { Id = r.Id, File = r.File }).ToList()
        }), NullLogger<MarkdownAgentConfigurationService>.Instance);

    public void Dispose() { try { Directory.Delete(Path, true); } catch (IOException) { } }
}

public class MarkdownConfigurationTests
{
    private const string Orchestrator = "---\nname: Jefe\nisOrchestrator: true\nrole: Coordinador\n---\n# Instrucciones\nCoordina.";

    [Fact]
    public async Task Loads_front_matter_and_instructions_from_markdown()
    {
        using var dir = new TempAgentsDirectory();
        dir.Write("orchestrator.md", Orchestrator);
        dir.Write("dev.md", "---\nname: Dev\nworkingStatus: Coding\nmaxTokens: 1234\ntemperature: 0.3\nacceptsDocuments: true\n---\nEres un dev.");

        var service = dir.CreateService(("orchestrator", "orchestrator.md"), ("dev", "dev.md"));
        var report = await service.ReloadAsync();

        Assert.True(report.IsValid);
        var dev = service.GetDefinition("dev")!;
        Assert.Equal("Dev", dev.Name);
        Assert.Equal(AgentStatus.Coding, dev.WorkingStatus);
        Assert.Equal(1234, dev.ModelSettings.MaxTokens);
        Assert.Equal(0.3, dev.ModelSettings.Temperature);
        Assert.True(dev.AcceptsDocuments);
        Assert.Equal("Eres un dev.", service.GetInstructions("dev"));
        Assert.True(service.GetDefinition("orchestrator")!.IsOrchestrator);
    }

    [Fact]
    public async Task Missing_markdown_file_is_reported_and_the_agent_is_skipped()
    {
        using var dir = new TempAgentsDirectory();
        dir.Write("orchestrator.md", Orchestrator);

        var service = dir.CreateService(("orchestrator", "orchestrator.md"), ("ghost", "ghost.md"));
        var report = await service.ReloadAsync();

        Assert.False(report.IsValid);
        Assert.Contains(report.Issues, i => i.AgentId == "ghost" && i.IsError && i.Message.Contains("ghost.md"));
        Assert.Null(service.GetDefinition("ghost"));
        Assert.NotNull(service.GetDefinition("orchestrator"));
    }

    [Fact]
    public async Task Missing_orchestrator_and_empty_instructions_are_validation_errors()
    {
        using var dir = new TempAgentsDirectory();
        dir.Write("empty.md", "---\nname: Vacío\n---\n   ");

        var report = await dir.CreateService(("empty", "empty.md")).ReloadAsync();

        Assert.Contains(report.Issues, i => i.Message.Contains("no contiene instrucciones"));
        Assert.Contains(report.Issues, i => i.Message.Contains("orquestador"));
    }

    [Fact]
    public async Task Reload_picks_up_edited_roles_without_restarting()
    {
        using var dir = new TempAgentsDirectory();
        dir.Write("orchestrator.md", Orchestrator);
        dir.Write("dev.md", "Versión 1");
        var service = dir.CreateService(("orchestrator", "orchestrator.md"), ("dev", "dev.md"));
        await service.ReloadAsync();
        Assert.Equal("Versión 1", service.GetInstructions("dev"));

        dir.Write("dev.md", "Versión 2");
        await service.ReloadAsync();

        Assert.Equal("Versión 2", service.GetInstructions("dev"));
    }

    [Fact]
    public async Task New_agent_is_registered_by_configuration_only()
    {
        using var dir = new TempAgentsDirectory();
        dir.Write("orchestrator.md", Orchestrator);
        dir.Write("translator.md", "---\nname: Traductor\nicon: 🌐\n---\nTraduces textos.");

        var service = dir.CreateService(("orchestrator", "orchestrator.md"), ("translator", "translator.md"));
        await service.ReloadAsync();
        var registry = new AI.Agent.Office.Application.Agents.AgentRegistry();
        registry.Load(service.GetDefinitions());

        Assert.Equal("Traductor", registry.Get("translator")!.Name);
        Assert.Equal(2, registry.GetAll().Count);
    }

    [Fact]
    public async Task Auto_discovers_markdown_files_when_nothing_is_registered()
    {
        using var dir = new TempAgentsDirectory();
        dir.Write("orchestrator.md", Orchestrator);
        dir.Write("helper.md", "Ayudas.");

        var service = dir.CreateService();
        await service.ReloadAsync();

        Assert.Equal(2, service.GetDefinitions().Count);
    }

    [Fact]
    public async Task Path_traversal_in_file_names_is_neutralized()
    {
        using var dir = new TempAgentsDirectory();
        dir.Write("orchestrator.md", Orchestrator);

        var service = dir.CreateService(("orchestrator", "orchestrator.md"), ("evil", "../../secret.md"));
        var report = await service.ReloadAsync();

        Assert.Contains(report.Issues, i => i.AgentId == "evil" && i.IsError);
        Assert.Null(service.GetDefinition("evil"));
    }

    [Fact]
    public void The_shipped_agent_files_are_valid()
    {
        var directory = OfficeConfiguration.ResolveDirectory("agents");
        Assert.NotNull(directory);

        var service = new MarkdownAgentConfigurationService(
            Microsoft.Extensions.Options.Options.Create(new AgentsOptions { Directory = directory! }),
            NullLogger<MarkdownAgentConfigurationService>.Instance);
        var report = service.ReloadAsync().GetAwaiter().GetResult();

        Assert.True(report.IsValid, string.Join("; ", report.Issues.Select(i => i.Message)));
        Assert.Equal(10, report.Agents.Count);
        Assert.Single(report.Agents, a => a.IsOrchestrator);
    }
}

public class RegistryTests
{
    [Fact]
    public void Registry_loads_agents_starting_in_Idle_and_updates_atomically()
    {
        var h = new OfficeHarness();

        Assert.Equal(6, h.Registry.GetAll().Count);
        Assert.All(h.Registry.GetAll(), a => Assert.Equal(AgentStatus.Idle, a.Status));

        h.Registry.Update("developer", a => a.Status = AgentStatus.Coding);
        Assert.Equal(AgentStatus.Coding, h.Registry.Get("developer")!.Status);
        Assert.Null(h.Registry.Update("missing", _ => { }));
    }

    [Fact]
    public void Registry_snapshots_cannot_mutate_internal_state()
    {
        var h = new OfficeHarness();
        var snapshot = h.Registry.Get("developer")!;
        snapshot.Status = AgentStatus.Error;

        Assert.Equal(AgentStatus.Idle, h.Registry.Get("developer")!.Status);
    }

    [Fact]
    public void Reloading_keeps_the_runtime_state_of_existing_agents()
    {
        var h = new OfficeHarness();
        h.Registry.Update("developer", a => a.Status = AgentStatus.Coding);

        h.Registry.Load(h.Configuration.Definitions);

        Assert.Equal(AgentStatus.Coding, h.Registry.Get("developer")!.Status);
    }

    [Fact]
    public async Task Registry_is_safe_under_concurrent_updates()
    {
        var h = new OfficeHarness();
        await Task.WhenAll(Enumerable.Range(0, 200).Select(i => Task.Run(() =>
            h.Registry.Update("developer", a => a.Status = i % 2 == 0 ? AgentStatus.Working : AgentStatus.Reading))));

        Assert.Contains(h.Registry.Get("developer")!.Status, new[] { AgentStatus.Working, AgentStatus.Reading });
    }

    [Fact]
    public async Task Agents_return_to_idle_after_the_display_delay_unless_they_got_new_work()
    {
        var h = new OfficeHarness();
        await h.State.SetAsync("developer", AgentStatus.Completed, OfficeEventType.AgentCompleted, "fin");
        h.State.ReturnToIdleAfter("developer", TimeSpan.FromMilliseconds(50));
        await Task.Delay(300);
        Assert.Equal(AgentStatus.Idle, h.Registry.Get("developer")!.Status);

        await h.State.SetAsync("architect", AgentStatus.Completed, OfficeEventType.AgentCompleted, "fin");
        h.State.ReturnToIdleAfter("architect", TimeSpan.FromMilliseconds(100));
        await h.State.SetAsync("architect", AgentStatus.Working, OfficeEventType.AgentProgress, "nuevo trabajo");
        await Task.Delay(400);
        Assert.Equal(AgentStatus.Working, h.Registry.Get("architect")!.Status);
    }
}

public class DecisionValidationTests
{
    private static readonly string[] Agents = { "developer", "architect", "qa-analyst" };

    private static IReadOnlyList<string> Validate(string json, int max = 6) =>
        OrchestratorDecisionValidator.Validate(OrchestratorDecision.Parse(json), Agents, max);

    [Fact]
    public void Valid_delegation_passes_and_ids_are_normalized()
    {
        var decision = OrchestratorDecision.Parse("""{"action":"Delegate","message":"m","tasks":[{"agentId":"developer","task":"haz algo"}]}""");
        var errors = OrchestratorDecisionValidator.Validate(decision, Agents, 6);

        Assert.Empty(errors);
        Assert.Equal("t1", decision!.Tasks[0].Id);
        Assert.Equal("haz algo", decision.Tasks[0].Title);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("""{"action":"explode","message":"x"}""")]
    [InlineData("""{"action":"respond","message":""}""")]
    [InlineData("""{"action":"delegate","message":"x","tasks":[]}""")]
    [InlineData("""{"action":"delegate","message":"x","tasks":[{"id":"a","agentId":"orchestrator","task":"x"}]}""")]
    [InlineData("""{"action":"delegate","message":"x","tasks":[{"id":"a","agentId":"developer","task":""}]}""")]
    [InlineData("""{"action":"delegate","message":"x","tasks":[{"id":"a","agentId":"developer","task":"x","dependsOn":["zzz"]}]}""")]
    [InlineData("""{"action":"delegate","message":"x","tasks":[{"id":"a","agentId":"developer","task":"x","dependsOn":["a"]}]}""")]
    [InlineData("""{"action":"delegate","message":"x","tasks":[{"id":"a","agentId":"developer","task":"x"},{"id":"a","agentId":"architect","task":"y"}]}""")]
    public void Invalid_decisions_are_rejected(string json) => Assert.NotEmpty(Validate(json));

    [Theory]
    [InlineData("teacher", "teacher")]
    [InlineData("Profesor", "teacher")]
    [InlineData("PROFESOR", "teacher")]
    [InlineData("Analista QA", "qa-analyst")]
    [InlineData("qa analyst", "qa-analyst")]
    [InlineData("Tutor técnico", "teacher")]
    [InlineData("teacher agent", "teacher")]
    [InlineData("matematico", "matematico")]       // unknown: left untouched so validation reports it
    public void Agent_names_and_loose_ids_are_resolved_to_the_real_id(string given, string expected)
    {
        var aliases = new Dictionary<string, string>();
        foreach (var (id, name, role) in new[] { ("teacher", "Profesor", "Tutor técnico"), ("qa-analyst", "Analista QA", "Control de calidad"), ("developer", "Desarrollador", "Ingeniero de software") })
            foreach (var alias in new[] { id, name, role })
                aliases.TryAdd(OrchestratorDecisionValidator.Normalize(alias), id);

        Assert.Equal(expected, OrchestratorDecisionValidator.ResolveAgentId(given, aliases));
    }

    [Fact]
    public void Dependency_cycles_are_rejected()
    {
        var errors = Validate("""
            {"action":"delegate","message":"x","tasks":[
              {"id":"a","agentId":"developer","task":"x","dependsOn":["b"]},
              {"id":"b","agentId":"architect","task":"y","dependsOn":["a"]}]}
            """);

        Assert.Contains(errors, e => e.Contains("ciclos"));
    }

    [Fact]
    public void Delegation_count_is_limited()
    {
        var tasks = string.Join(",", Enumerable.Range(1, 4).Select(i => $$"""{"id":"t{{i}}","agentId":"developer","task":"x"}"""));
        Assert.Contains(Validate($$"""{"action":"delegate","message":"x","tasks":[{{tasks}}]}""", max: 3), e => e.Contains("como máximo 3"));
    }

    [Fact]
    public void Topological_order_puts_dependencies_first()
    {
        var items = new[] { ("c", new[] { "a", "b" }), ("b", new[] { "a" }), ("a", Array.Empty<string>()) };
        var order = PlanGraph.TopologicalOrder(items, i => i.Item1, i => i.Item2)!.Select(i => i.Item1).ToList();

        Assert.Equal(new[] { "a", "b", "c" }, order);
    }

    [Fact]
    public void Schema_restricts_agent_ids_to_the_registered_team()
    {
        var spec = DecisionSchema.Build(Agents, 4);

        Assert.Equal(DecisionSchema.ToolName, spec.Name);
        using var doc = System.Text.Json.JsonDocument.Parse(spec.InputSchemaJson);
        var ids = doc.RootElement.GetProperty("properties").GetProperty("tasks").GetProperty("items")
            .GetProperty("properties").GetProperty("agentId").GetProperty("enum").EnumerateArray().Select(e => e.GetString());
        Assert.Equal(Agents, ids);
    }

    [Fact]
    public void Chat_request_validation_checks_message_and_document_count()
    {
        Assert.NotEmpty(ChatRequestValidator.Validate(new AI.Agent.Office.Application.DTOs.ChatRequestDto(""), 100));
        Assert.NotEmpty(ChatRequestValidator.Validate(new AI.Agent.Office.Application.DTOs.ChatRequestDto(new string('a', 101)), 100));
        Assert.NotEmpty(ChatRequestValidator.Validate(
            new AI.Agent.Office.Application.DTOs.ChatRequestDto("ok", null, Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToList()), 100));
        Assert.Empty(ChatRequestValidator.Validate(new AI.Agent.Office.Application.DTOs.ChatRequestDto("ok"), 100));
    }
}

public class DocumentServiceTests
{
    private sealed class FakeExtractor : IDocumentTextExtractor
    {
        public Task<DocumentExtractionResult> ExtractAsync(Stream content, string fileName, int maxPages, int maxChars, CancellationToken ct) =>
            Task.FromResult(new DocumentExtractionResult("texto extraído", 3, false, false));
    }

    private static DocumentService Create(DocumentOptions? options = null) =>
        new(new FakeExtractor(), new DocumentStore(Microsoft.Extensions.Options.Options.Create(options ?? new DocumentOptions())),
            Microsoft.Extensions.Options.Options.Create(options ?? new DocumentOptions()), NullLogger<DocumentService>.Instance);

    [Fact]
    public async Task Valid_pdf_is_processed_and_stored_in_memory()
    {
        var dto = await Create().UploadAsync(new MemoryStream(new byte[10]), "informe.pdf", "application/pdf", 10, default);

        Assert.Equal("informe.pdf", dto.FileName);
        Assert.Equal(3, dto.PageCount);
    }

    [Theory]
    [InlineData("virus.exe", "application/pdf", 10)]
    [InlineData("doc.pdf", "text/html", 10)]
    [InlineData("doc.pdf", "application/pdf", 0)]
    [InlineData("doc.pdf", "application/pdf", 50_000_000)]
    public async Task Invalid_files_are_rejected(string name, string contentType, long length) =>
        await Assert.ThrowsAsync<OfficeValidationException>(() =>
            Create().UploadAsync(new MemoryStream(new byte[1]), name, contentType, length, default));

    [Fact]
    public async Task Directory_components_in_the_file_name_are_stripped()
    {
        var dto = await Create().UploadAsync(new MemoryStream(new byte[10]), @"..\..\secreto\informe.pdf", "application/pdf", 10, default);
        Assert.Equal("informe.pdf", dto.FileName);
    }
}
