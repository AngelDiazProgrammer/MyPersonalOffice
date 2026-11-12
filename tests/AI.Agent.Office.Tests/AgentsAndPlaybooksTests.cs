using AI.Agent.Office.Application.Agents;
using AI.Agent.Office.Application.Camila;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Application.Services;
using AI.Agent.Office.Infrastructure.Agents;
using AI.Agent.Office.Infrastructure.Camila;
using AI.Agent.Office.Infrastructure.Configuration;
using AI.Agent.Office.Tests.TestSupport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Tests;

/// <summary>Spec 011 · catálogo y gestión de agentes.</summary>
public class AgentAdminTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "agentes-" + Guid.NewGuid().ToString("N"));
    private readonly AgentRegistry _registry = new();
    private readonly MarkdownAgentConfigurationService _definitions;
    private readonly AgentAdminService _admin;

    public AgentAdminTests()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path.Combine(_dir, "orchestrator.md"), "---\nname: Orquestador\nrole: Coordinador\ndescription: Coordina\nisOrchestrator: true\nworkingStatus: Thinking\n---\nCoordina a los demás.\n");
        File.WriteAllText(Path.Combine(_dir, "teacher.md"), "---\nname: Profesor\nrole: Tutor\ndescription: Enseña\nacceptsDocuments: true\nworkingStatus: Working\nmaxTokens: 2048\n---\nEnseña con ejemplos.\n");
        File.WriteAllText(Path.Combine(_dir, "camila-ia.md"), "---\nname: CamilaIA\nrole: Asistente\ndescription: Conversa\npurpose: Llevar la conversación y la base de conocimiento de Camila.\nkind: llm\nenvironment: conversational\ntools: buscar_conocimiento, consultar_servicio\nsensitive: true\nsystem: true\n---\nEres Camila.\n");

        _definitions = new MarkdownAgentConfigurationService(Options.Create(new AgentsOptions { Directory = _dir }), NullLogger<MarkdownAgentConfigurationService>.Instance);
        var events = new OfficeEventService(new ActivityLog(Options.Create(new OrchestrationOptions())), new RecordingPublisher(), NullLogger<OfficeEventService>.Instance);
        var catalog = new ReloadingCatalog(_definitions, _registry);
        catalog.ReloadAsync().GetAwaiter().GetResult();
        var workspaces = Options.Create(new WorkspacesOptions { Items = { new WorkspaceDefinition { Id = "dev", Name = "Desarrollo", Agents = { "orchestrator", "teacher" } } } });
        _admin = new AgentAdminService(_definitions, catalog, _registry, new MarkdownAgentFileStore(Options.Create(new AgentsOptions { Directory = _dir })), new TokenUsageTracker(), workspaces, events);
    }

    public void Dispose() { if (Directory.Exists(_dir)) Directory.Delete(_dir, true); }

    private sealed class ReloadingCatalog : IAgentCatalogService
    {
        private readonly IAgentConfigurationService _config;
        private readonly IAgentRegistry _registry;
        public ReloadingCatalog(IAgentConfigurationService config, IAgentRegistry registry) { _config = config; _registry = registry; }
        public async Task<ConfigurationReport> ReloadAsync(CancellationToken cancellationToken = default)
        {
            var report = await _config.ReloadAsync(cancellationToken);
            _registry.Load(report.Agents);
            return report;
        }
    }

    private static AgentUpsertDto New(string id = "analista-rh", string purpose = "Responder dudas de recursos humanos con la política vigente.") =>
        new(id, "Analista RH", "Analista", purpose, "", "llm", "office", new[] { "buscar_conocimiento" }, "", false, "#22d3ee", "📋", 1500, "# Rol\nAyudas con dudas de RH.");

    [Fact]
    public void Validator_requires_a_specific_purpose_a_valid_id_known_tools_and_unique_ids()
    {
        var ids = new[] { "teacher" };

        Assert.Empty(AgentValidator.Validate(New(), true, ids, null));
        Assert.Contains(AgentValidator.Validate(New(purpose: "corto"), true, ids, null), e => e.Contains("uso específico"));
        Assert.Contains(AgentValidator.Validate(New(id: "Mal Id"), true, ids, null), e => e.Contains("id debe"));
        Assert.Contains(AgentValidator.Validate(New(id: "../x"), true, ids, null), e => e.Contains("id debe"));
        Assert.Contains(AgentValidator.Validate(New(id: "teacher"), true, ids, null), e => e.Contains("Ya existe"));
        Assert.Contains(AgentValidator.Validate(New(id: "orchestrator"), true, ids, null), e => e.Contains("orquestador"));
        Assert.Contains(AgentValidator.Validate(New() with { Tools = new[] { "borrar_todo" } }, true, ids, null), e => e.Contains("Herramienta no válida"));
        Assert.Contains(AgentValidator.Validate(New() with { Kind = "magico" }, true, ids, null), e => e.Contains("tipo"));
        Assert.Contains(AgentValidator.Validate(New() with { Instructions = new string('x', AgentValidator.MaxInstructionsChars + 1) }, true, ids, null), e => e.Contains("superan"));
    }

    [Fact]
    public async Task Creating_an_agent_writes_its_file_reloads_the_registry_and_lists_it()
    {
        var result = await _admin.CreateAsync(New());

        Assert.True(result.Ok, string.Join("; ", result.Errors));
        Assert.True(File.Exists(Path.Combine(_dir, "analista-rh.md")));
        var item = Assert.Single(_admin.GetCatalog(), a => a.Id == "analista-rh");
        Assert.Equal("Responder dudas de recursos humanos con la política vigente.", item.Purpose);
        Assert.Contains("buscar_conocimiento", item.Tools);
        Assert.NotNull(_registry.Get("analista-rh"));                                      // usable sin reiniciar

        var again = await _admin.CreateAsync(New());
        Assert.False(again.Ok);                                                            // id repetido
    }

    [Fact]
    public async Task Invalid_agents_are_rejected_without_writing_anything()
    {
        var result = await _admin.CreateAsync(New(purpose: ""));

        Assert.False(result.Ok);
        Assert.False(File.Exists(Path.Combine(_dir, "analista-rh.md")));
    }

    [Fact]
    public async Task Editing_changes_the_instructions_and_keeps_the_keys_the_form_does_not_manage()
    {
        var edit = _admin.GetForEdit("teacher")! with { Instructions = "# Nuevo prompt\nExplica con calma.", Purpose = "Enseñar con ejemplos y ejercicios graduales." };

        var result = await _admin.UpdateAsync("teacher", edit);

        Assert.True(result.Ok, string.Join("; ", result.Errors));
        var text = File.ReadAllText(Path.Combine(_dir, "teacher.md"));
        Assert.Contains("Explica con calma.", text);
        Assert.Contains("acceptsDocuments: true", text);                                   // clave interna conservada
        Assert.Contains("workingStatus: Working", text);
        Assert.True(_definitions.GetDefinition("teacher")!.AcceptsDocuments);
        Assert.Contains("Explica con calma.", _definitions.GetInstructions("teacher"));
    }

    [Fact]
    public async Task The_id_cannot_change_and_a_system_agent_keeps_its_kind()
    {
        var renamed = await _admin.UpdateAsync("teacher", _admin.GetForEdit("teacher")! with { Id = "otro", Purpose = "Enseñar con ejemplos y ejercicios." });
        Assert.True(renamed.Ok);                                                           // el id de la ruta manda: sigue siendo «teacher»
        Assert.True(File.Exists(Path.Combine(_dir, "teacher.md")));
        Assert.False(File.Exists(Path.Combine(_dir, "otro.md")));

        var changed = await _admin.UpdateAsync("camila-ia", _admin.GetForEdit("camila-ia")! with { Kind = "executor" });
        Assert.False(changed.Ok);
        Assert.Contains(changed.Errors, e => e.Contains("del sistema"));
    }

    [Fact]
    public async Task System_agents_and_agents_in_an_office_cannot_be_deleted_but_others_can()
    {
        var system = await _admin.DeleteAsync("camila-ia");
        Assert.False(system.Ok);
        Assert.Contains(system.Errors, e => e.Contains("sistema"));
        Assert.False((await _admin.DeleteAsync("orchestrator")).Ok);

        var inOffice = await _admin.DeleteAsync("teacher");
        Assert.False(inOffice.Ok);
        Assert.Contains(inOffice.Errors, e => e.Contains("Desarrollo"));                  // se dice en qué oficina está
        Assert.True(File.Exists(Path.Combine(_dir, "teacher.md")));

        await _admin.CreateAsync(New());
        var removed = await _admin.DeleteAsync("analista-rh");
        Assert.True(removed.Ok);
        Assert.False(File.Exists(Path.Combine(_dir, "analista-rh.md")));
        Assert.Null(_registry.Get("analista-rh"));
    }

    [Fact]
    public void The_catalog_lists_the_use_environment_tools_and_where_each_agent_is_used()
    {
        var catalog = _admin.GetCatalog();

        Assert.Equal("orchestrator", catalog[0].Id);                                       // primero el orquestador
        var teacher = catalog.Single(a => a.Id == "teacher");
        Assert.Equal("Enseña", teacher.Purpose);                                           // sin `purpose`, el uso sale de la descripción
        Assert.Contains("Oficina: Desarrollo", teacher.UsedBy);
        var camila = catalog.Single(a => a.Id == "camila-ia");
        Assert.True(camila.IsSystem && camila.Sensitive);
        Assert.Equal(new[] { "buscar_conocimiento", "consultar_servicio" }, camila.Tools);
        Assert.Contains(camila.UsedBy, u => u.Contains("conversacionales"));
    }

    [Fact]
    public async Task A_path_in_the_id_never_reaches_the_file_system()
    {
        var result = await _admin.CreateAsync(New(id: "../../escapar"));

        Assert.False(result.Ok);
        Assert.Empty(Directory.GetFiles(Path.GetTempPath(), "escapar*"));
    }
}

/// <summary>Spec 012 · playbooks maestros de bots y herramientas impuestas a los agentes que los usan.</summary>
public class BotPlaybookTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "playbooks-" + Guid.NewGuid().ToString("N"));

    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }

    internal static CamilaBot Bot()
    {
        var bot = new CamilaBot { StartFlow = "Inicio" };
        bot.Intents.Add(new CamilaIntent { Name = "Default Welcome Intent", Phrases = { "hola" } });
        bot.Webhooks.Add(new CamilaWebhook { Name = "Consulta", Uri = "https://api.example.com:9876/c", Kind = "read", Body = "{\"cedula\":\"$session.params.doc\"}", Headers = { ["x-api-key"] = "{{secret:CAMILA_WH_X}}" } });
        bot.Webhooks.Add(new CamilaWebhook { Name = "Guardar nota", Uri = "https://api.example.com:9876/g", Kind = "write" });

        var inicio = new CamilaFlow { Name = "Inicio" };
        inicio.Routes.Add(new CamilaRoute { Intent = "Default Welcome Intent", TargetPage = "Bienvenida" });
        var bienvenida = new CamilaPage { Name = "Bienvenida" };
        bienvenida.Entry.Messages.Add(new CamilaMessage { Texts = { "Hola" } });
        bienvenida.Entry.Messages.Add(new CamilaMessage { Type = "buttons", Buttons = { new CamilaButton { Text = "Guardar", Event = "guardar" } } });
        bienvenida.Events.Add(new CamilaRoute { Event = "guardar", TargetPage = "Guardar" });
        var guardar = new CamilaPage { Name = "Guardar" };
        guardar.Entry.Webhook = "Guardar nota";
        guardar.Entry.Messages.Add(new CamilaMessage { Texts = { "Guardado" } });
        inicio.Pages.AddRange(new[] { bienvenida, guardar });

        var otro = new CamilaFlow { Name = "Convenios y beneficios" };
        otro.Pages.Add(new CamilaPage { Name = "Info" });
        bot.Flows.AddRange(new[] { inicio, otro });
        return bot;
    }

    private static BotPlaybookMeta Meta() => new("mi-bot", "Mi bot", "Bot de prueba.", "🧩", "lector-uml", "camila-ia", "bases de conocimiento/MiBot.md", "Inicio");

    [Fact]
    public void A_playbook_has_a_master_a_file_per_flow_catalogs_and_notes_and_reads_back_to_the_same_bot()
    {
        var files = CamilaPlaybooks.Write(_root, Meta(), Bot());

        Assert.Contains("bot.md", files);
        Assert.Contains("notas.md", files);
        Assert.Contains(files, f => f.StartsWith("flujos/01-inicio"));
        Assert.Contains(files, f => f.StartsWith("flujos/02-convenios-y-beneficios"));
        Assert.Contains("catalogos/webhooks.md", files);

        var loaded = CamilaPlaybooks.Load(Path.Combine(_root, "mi-bot"))!;
        Assert.Equal(("mi-bot", "Mi bot", "lector-uml", "camila-ia"), (loaded.Info.Id, loaded.Info.Name, loaded.Info.Executor, loaded.Info.Agent));
        Assert.Equal((2, 3, 2), (loaded.Info.Flows, loaded.Info.Pages, loaded.Info.Webhooks));
        Assert.Equal("Inicio", loaded.Bot.StartFlow);
        Assert.Equal("write", loaded.Bot.FindWebhook("Guardar nota")!.Kind);
        Assert.Equal("Guardado", loaded.Bot.FindPage(loaded.Bot.FindFlow("Inicio")!, "Guardar")!.Entry.Messages[0].Texts[0]);
        Assert.Empty(loaded.Problems);
    }

    [Fact]
    public void The_master_explains_the_playbook_in_steps_and_lists_every_flow_file()
    {
        CamilaPlaybooks.Write(_root, Meta(), Bot());
        var master = File.ReadAllText(Path.Combine(_root, "mi-bot", "bot.md"));
        var flow = File.ReadAllText(Path.Combine(_root, "mi-bot", "flujos", "01-inicio.md"));

        Assert.Contains("executor: lector-uml", master);
        Assert.Contains("[Inicio](flujos/01-inicio.md)", master);
        Assert.Contains("ejecuta el agente `lector-uml`", master);
        Assert.Contains("**Al entrar, en este orden:**", flow);                             // los pasos en orden que ejecuta LectorUML
        Assert.Contains("✍️ Llama al webhook **Guardar nota**", flow);
        Assert.Contains("```json camila:page", flow);
    }

    [Fact]
    public void Regenerating_is_deterministic_and_never_overwrites_the_hand_written_notes()
    {
        CamilaPlaybooks.Write(_root, Meta(), Bot());
        var dir = Path.Combine(_root, "mi-bot");
        var first = Directory.GetFiles(dir, "*.md", SearchOption.AllDirectories).Where(f => !f.EndsWith("notas.md")).ToDictionary(f => f, File.ReadAllText);
        File.WriteAllText(Path.Combine(dir, "notas.md"), "# Notas\nTutea siempre a la persona.");
        File.WriteAllText(Path.Combine(dir, "flujos", "99-vieja.md"), "viejo");              // un flujo que ya no existe se limpia

        CamilaPlaybooks.Write(_root, Meta(), Bot());

        Assert.Equal("# Notas\nTutea siempre a la persona.", File.ReadAllText(Path.Combine(dir, "notas.md")));
        Assert.False(File.Exists(Path.Combine(dir, "flujos", "99-vieja.md")));
        var second = Directory.GetFiles(dir, "*.md", SearchOption.AllDirectories).Where(f => !f.EndsWith("notas.md")).ToDictionary(f => f, File.ReadAllText);
        Assert.Equal(first, second);
        Assert.Contains("Tutea siempre a la persona.", CamilaPlaybooks.Load(dir)!.Notes);
    }

    [Fact]
    public void Playbooks_never_contain_credentials_only_secret_references()
    {
        CamilaPlaybooks.Write(_root, Meta(), Bot());
        var all = string.Concat(Directory.GetFiles(_root, "*.md", SearchOption.AllDirectories).Select(File.ReadAllText));

        Assert.Contains("{{secret:CAMILA_WH_X}}", all);
        Assert.DoesNotContain("Bearer ", all);
    }

    [Fact]
    public void An_untouched_notes_template_counts_as_no_notes()
    {
        CamilaPlaybooks.Write(_root, Meta(), Bot());

        Assert.Null(CamilaPlaybooks.Load(Path.Combine(_root, "mi-bot"))!.Notes);
    }

    [Fact]
    public void Loading_reports_broken_references_without_failing()
    {
        CamilaPlaybooks.Write(_root, Meta(), Bot());
        var flow = Path.Combine(_root, "mi-bot", "flujos", "01-inicio.md");
        File.WriteAllText(flow, File.ReadAllText(flow).Replace("\"targetPage\":\"Bienvenida\"", "\"targetPage\":\"No existe\""));

        var loaded = CamilaPlaybooks.Load(Path.Combine(_root, "mi-bot"))!;

        Assert.Contains(loaded.Problems, p => p.Contains("página inexistente «No existe»"));
        Assert.Equal(2, loaded.Info.Flows);
    }

    [Fact]
    public void The_narrative_of_the_knowledge_base_has_no_executable_blocks_and_is_split_by_flow()
    {
        var narrative = CamilaMarkdown.Write(Bot(), includeBlocks: false);

        Assert.DoesNotContain("```json camila:", narrative);
        Assert.Contains("la lee el agente CamilaIA", narrative);
        var sections = CamilaPlaybooks.ParseNarrative(narrative);
        Assert.Equal(new[] { "Convenios y beneficios", "Inicio" }, sections.Keys.OrderBy(k => k));
        Assert.Contains("Llama al webhook **Guardar nota**", sections["Inicio"]);
        Assert.DoesNotContain("Convenios", sections["Inicio"]);
    }

    [Fact]
    public async Task The_bot_source_loads_every_bot_folder_with_its_narrative()
    {
        CamilaPlaybooks.Write(_root, Meta() with { Knowledge = "kb-prueba.md" }, Bot());
        var kb = Path.Combine(AppContext.BaseDirectory, "kb-prueba.md");
        File.WriteAllText(kb, CamilaMarkdown.Write(Bot(), includeBlocks: false));
        try
        {
            var source = new BotPlaybookSource(Options.Create(new CamilaOptions { BotsDirectory = _root }), NullLogger<BotPlaybookSource>.Instance);

            Assert.Equal(1, await source.ReloadAsync());
            Assert.Equal("mi-bot", Assert.Single(source.Bots).Id);
            Assert.NotNull(source.Get("mi-bot"));
            Assert.Null(source.Get("otro"));
            Assert.Contains("Inicio", source.Narrative("mi-bot")!.Keys);
        }
        finally { File.Delete(kb); }
    }

    [Fact]
    public async Task The_executor_agent_tools_are_enforced_so_without_write_service_nothing_is_written()
    {
        // LectorUML sin escribir_servicio: el webhook de escritura del playbook no se llama (ni siquiera pide confirmación).
        var calls = 0;
        var transport = new CountingTransport(() => calls++);
        var hooks = new CamilaWebhookService(Options.Create(new CamilaOptions { AllowedHosts = { "api.example.com:9876" } }),
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["CAMILA_WH_X"] = "x" }).Build(), transport, NullLogger<CamilaWebhookService>.Instance);
        var options = Options.Create(new CamilaOptions { AllowedHosts = { "api.example.com:9876" } });
        var engine = new CamilaEngine(Bot(), hooks, new NoGen(), options);
        var session = new CamilaSession(Guid.NewGuid(), CamilaMode.Uml) { AllowWriteServices = false };
        await engine.StartAsync(session, default);

        var turn = await engine.UserEventAsync(session, "guardar", default);

        Assert.Equal(0, calls);
        Assert.Null(turn.Confirm);
        Assert.Contains(turn.Trace, t => t.Text.Contains("no tiene la herramienta escribir_servicio"));
        Assert.Contains(turn.Messages, m => m.Text == "Guardado");                          // el flujo sigue; solo la escritura queda bloqueada
    }

    private sealed class CountingTransport : ICamilaHttpTransport
    {
        private readonly Action _count;
        public CountingTransport(Action count) => _count = count;
        public Task<CamilaHttpResponse> SendAsync(CamilaHttpRequest request, CancellationToken cancellationToken) { _count(); return Task.FromResult(new CamilaHttpResponse(200, "{}")); }
    }

    private sealed class NoGen : ICamilaGeneratorRunner
    {
        public Task<string> RunAsync(CamilaGeneratorDef generator, IReadOnlyDictionary<string, string> inputs, CancellationToken cancellationToken) => Task.FromResult(string.Empty);
    }
}
