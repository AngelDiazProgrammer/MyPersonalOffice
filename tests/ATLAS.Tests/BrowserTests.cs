using ATLAS.Application.Browser;
using ATLAS.Application.DTOs;
using ATLAS.Application.Interfaces;
using ATLAS.Infrastructure.Browser;
using ATLAS.Tests.TestSupport;
using Microsoft.Extensions.Options;

namespace ATLAS.Tests;

/// <summary>Playbook de ejemplo del caso guía (correo) apuntando a un sitio de prueba.</summary>
internal static class BrowserSamples
{
    public const string Mail = """
        ---
        nombre: Enviar correo
        disparadores: enviar correo, mandar correo, envíalo por correo
        ---
        # Enviar correo

        ## Entradas
        - destinatario | ¿A quién va dirigido el correo? | correo
        - motivo | ¿Para qué es el correo?
        - asunto | redactar | Asunto breve y claro
        - cuerpo | redactar | Cuerpo formal del correo según el motivo y el contexto

        ## Pasos
        1. ir | https://mail.example.com/inbox
        2. clic | button.nuevo | | Abrir la redacción
        3. escribir | input.para | {{destinatario}}
        4. escribir | input.asunto | {{asunto}}
        5. escribir | div.cuerpo | {{cuerpo}}
        6. clic | button.enviar | | Enviar el correo | irreversible
        7. verificar | Mensaje enviado
        """;

    public static BrowserSite Site() => new("mail", "Correo de prueba", "https://mail.example.com/inbox", new[] { "example.com" });

    public static Playbook Parse(string markdown = Mail) => PlaybookParser.Parse("mail", markdown).Playbook!;

    public static LlmResponse Json(string json) => new(string.Empty, json, 5, 5, "tool_use", "fake");

    public static Dictionary<string, string> Values() => new()
    {
        ["destinatario"] = "jefe@empresa.com", ["motivo"] = "informe", ["asunto"] = "Asunto X", ["cuerpo"] = "Cuerpo largo"
    };
}

public class PlaybookTests
{
    [Fact]
    public void Parses_inputs_steps_triggers_and_irreversible_marker()
    {
        var result = PlaybookParser.Parse("mail", BrowserSamples.Mail);

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
        var p = result.Playbook!;
        Assert.Equal("Enviar correo", p.Title);
        Assert.Contains("enviar correo", p.Triggers);
        Assert.Equal(4, p.Inputs.Count);
        Assert.True(p.Inputs.Single(i => i.Name == "asunto").Draft);
        Assert.Equal("correo", p.Inputs.Single(i => i.Name == "destinatario").Kind);
        Assert.Equal(7, p.Steps.Count);
        Assert.True(p.Steps[5].Irreversible);
        Assert.Equal("Abrir la redacción", p.Steps[1].Note);
    }

    [Fact]
    public void Reports_errors_with_line_numbers_and_unknown_variables()
    {
        var result = PlaybookParser.Parse("x", "## Entradas\n- a | ¿A?\n## Pasos\n1. volar | algo\n2. escribir | #x | {{nada}}\n3. ir | ftp://sitio");

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("acción desconocida"));
        Assert.Contains(result.Errors, e => e.Contains("{{nada}}"));
        Assert.Contains(result.Errors, e => e.Contains("URL http"));
    }

    [Fact]
    public void A_step_without_pipes_is_an_exploration_step()
    {
        var p = PlaybookParser.Parse("x", "## Pasos\n1. ir | https://a.com\n2. Comprueba que apareció la confirmación").Playbook!;

        Assert.Equal(StepAction.Explore, p.Steps[1].Action);
    }

    [Fact]
    public void A_playbook_without_steps_is_invalid()
    {
        Assert.False(PlaybookParser.Parse("x", "# Vacío\n## Entradas\n").IsValid);
    }

    [Fact]
    public void Render_then_parse_keeps_the_playbook()
    {
        var original = BrowserSamples.Parse();

        var again = PlaybookParser.Parse("mail", PlaybookParser.Render(original, "propuesta"));

        Assert.True(again.IsValid, string.Join("; ", again.Errors));
        Assert.Equal(original.Steps.Count, again.Playbook!.Steps.Count);
        Assert.Equal(original.Inputs.Count, again.Playbook.Inputs.Count);
        Assert.True(again.Playbook.Steps[5].Irreversible);
    }

    [Fact]
    public void Every_playbook_shipped_in_the_repository_is_valid()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ATLAS.sln"))) dir = dir.Parent;
        var files = Directory.GetFiles(Path.Combine(dir!.FullName, "playbooks"), "*.md").Where(f => !f.EndsWith(".propuesta.md")).ToList();

        Assert.NotEmpty(files);
        foreach (var file in files)
        {
            var result = PlaybookParser.Parse(Path.GetFileNameWithoutExtension(file), File.ReadAllText(file));
            Assert.True(result.IsValid, $"{Path.GetFileName(file)}: {string.Join("; ", result.Errors)}");
            Assert.DoesNotContain(PlaybookParser.ReviewMarker, File.ReadAllText(file));
        }
    }

    [Fact]
    public void The_default_template_is_valid()
    {
        Assert.True(PlaybookParser.Parse("x", PlaybookParser.Template("Sitio", "https://a.com")).IsValid);
    }
}

public class BrowserPolicyTests
{
    [Theory]
    [InlineData("https://mail.example.com/x", true)]
    [InlineData("https://example.com", true)]
    [InlineData("https://evilexample.com", false)]
    [InlineData("https://example.com.evil.com", false)]
    [InlineData("ftp://example.com", false)]
    [InlineData("about:blank", true)]
    [InlineData("", false)]
    public void Allowed_domains_match_the_domain_and_its_subdomains_only(string url, bool allowed) =>
        Assert.Equal(allowed, BrowserPolicy.IsUrlAllowed(url, new[] { "example.com" }));

    [Theory]
    [InlineData("localhost", true)]
    [InlineData("127.0.0.1", true)]
    [InlineData("10.1.2.3", true)]
    [InlineData("192.168.0.5", true)]
    [InlineData("172.20.0.1", true)]
    [InlineData("169.254.169.254", true)]
    [InlineData("[::1]", true)]
    [InlineData("mi.servidor.local", true)]
    [InlineData("example.com", false)]
    [InlineData("8.8.8.8", false)]
    public void Private_hosts_are_detected(string host, bool isPrivate) => Assert.Equal(isPrivate, BrowserPolicy.IsPrivateHost(host));

    [Fact]
    public void Site_validation_rejects_bad_ids_private_hosts_credentials_in_url_and_foreign_domains()
    {
        Assert.NotEmpty(BrowserPolicy.ValidateSite(new BrowserSite("Mal Id", "n", "https://a.com", new[] { "a.com" }), false));
        Assert.NotEmpty(BrowserPolicy.ValidateSite(new BrowserSite("ok", "n", "http://localhost:5000", new[] { "localhost" }), false));
        Assert.Empty(BrowserPolicy.ValidateSite(new BrowserSite("ok", "n", "http://localhost:5000", new[] { "localhost" }), true));
        Assert.NotEmpty(BrowserPolicy.ValidateSite(new BrowserSite("ok", "n", "https://user:pw@a.com", new[] { "a.com" }), false));
        Assert.NotEmpty(BrowserPolicy.ValidateSite(new BrowserSite("ok", "n", "https://a.com", new[] { "b.com" }), false));
        Assert.Empty(BrowserPolicy.ValidateSite(new BrowserSite("ok", "Nombre", "https://mail.google.com/mail/u/0/#inbox", new[] { "google.com" }), false));
    }

    [Fact]
    public void Normalize_adds_the_host_as_default_domain()
    {
        var site = BrowserPolicy.Normalize(new BrowserSite("GMAIL", " Gmail ", "https://mail.google.com/", Array.Empty<string>()));

        Assert.Equal("gmail", site.Id);
        Assert.Equal(new[] { "mail.google.com" }, site.AllowedDomains);
    }
}

public class BrowserRunnerTests
{
    private const string Secret = "S3creto-NoLoguear";

    private static OfficeHarness NewHarness() => new();

    private static Task<BrowserRunResult> Run(OfficeHarness h, string markdown = BrowserSamples.Mail, BrowserCredentials? creds = null, Dictionary<string, string>? values = null) =>
        h.BrowserRunner.RunAsync(BrowserSamples.Site(), BrowserSamples.Parse(markdown), values ?? BrowserSamples.Values(), creds, CancellationToken.None);

    [Fact]
    public async Task Replay_runs_every_step_in_order_with_inputs_and_without_calling_the_model()
    {
        var h = NewHarness();

        var result = await Run(h);

        Assert.True(result.Success, result.Message);
        Assert.Equal("reproducción", result.Mode);
        Assert.Equal(0, result.ModelCalls);
        Assert.Empty(h.Anthropic.Requests);
        Assert.Equal(new[]
        {
            "goto https://mail.example.com/inbox", "click button.nuevo", "fill input.para", "value:jefe@empresa.com",
            "fill input.asunto", "value:Asunto X", "fill div.cuerpo", "value:Cuerpo largo", "click button.enviar"
        }, h.BrowserSessions.Session.Actions);
        Assert.False(result.ProposalCreated);
        Assert.Empty(h.BrowserPlaybooks.Proposals);
    }

    [Fact]
    public async Task The_run_log_never_contains_the_values_typed()
    {
        var h = NewHarness();

        var result = await Run(h);

        Assert.DoesNotContain(result.Log, l => l.Contains("jefe@empresa.com") || l.Contains("Cuerpo largo"));
    }

    [Fact]
    public async Task Headless_follows_the_site_setting_or_the_global_default()
    {
        var h = NewHarness();
        h.BrowserConfig.Headless = true;
        await Run(h);
        Assert.True(h.BrowserSessions.LastHeadless);

        await h.BrowserRunner.RunAsync(BrowserSamples.Site() with { Headless = false }, BrowserSamples.Parse(), BrowserSamples.Values(), null, CancellationToken.None);
        Assert.False(h.BrowserSessions.LastHeadless);
    }

    [Fact]
    public async Task Navigating_outside_the_allowed_domains_is_blocked()
    {
        var h = NewHarness();

        var result = await Run(h, "## Pasos\n1. ir | https://evil.com/robo");

        Assert.False(result.Success);
        Assert.Contains("seguridad", result.Message);
        Assert.DoesNotContain(h.BrowserSessions.Session.Actions, a => a.Contains("evil.com"));
    }

    [Fact]
    public async Task A_click_that_leaves_the_allowed_domains_aborts_the_run()
    {
        var h = NewHarness();
        h.BrowserSessions.Session.RedirectAfterClickTo = "https://evil.com/phishing";

        var result = await Run(h);

        Assert.False(result.Success);
        Assert.Contains("seguridad", result.Message);
    }

    [Fact]
    public async Task A_missing_credential_aborts_with_a_clear_message_and_is_not_repaired()
    {
        var h = NewHarness();
        const string login = "## Pasos\n1. ir | https://mail.example.com\n2. escribir | input.clave | {{credencial.clave}}";

        var result = await Run(h, login);

        Assert.False(result.Success);
        Assert.Contains("credencial", result.Message);
        Assert.Equal(0, result.ModelCalls);
    }

    [Fact]
    public async Task Credentials_are_typed_but_never_logged_returned_or_sent_to_the_model()
    {
        var h = NewHarness();
        const string login = "## Pasos\n1. ir | https://mail.example.com\n2. escribir | input.clave | {{credencial.clave}}\n3. Pulsa el botón de entrar";
        h.Anthropic.Handler = (_, _) => Task.FromResult(BrowserSamples.Json("""{"action":"done"}"""));
        var creds = new BrowserCredentials("yo", Secret);

        var result = await Run(h, login, creds);

        Assert.True(result.Success, result.Message);
        Assert.Contains("value:" + Secret, h.BrowserSessions.Session.Actions);        // se escribió en la página
        Assert.DoesNotContain(result.Log, l => l.Contains(Secret));
        Assert.DoesNotContain(result.Message, Secret);
        Assert.All(h.Anthropic.Requests, r => Assert.DoesNotContain(Secret, r.Messages.Single().Content));
        Assert.DoesNotContain(Secret, creds.ToString());
    }

    [Fact]
    public async Task An_exploration_step_asks_the_model_marked_sensitive_and_proposes_a_structured_playbook()
    {
        var h = NewHarness();
        var script = new Queue<string>(new[]
        {
            """{"action":"click","target":"button.nuevo"}""",
            """{"action":"fill","target":"input.para","value":"{{destinatario}}"}""",
            """{"action":"done"}"""
        });
        h.Anthropic.Handler = (_, _) => Task.FromResult(BrowserSamples.Json(script.Dequeue()));
        const string free = "## Entradas\n- destinatario | ¿A quién? | correo\n## Pasos\n1. ir | https://mail.example.com/inbox\n2. Abre la redacción y escribe el destinatario";

        var result = await Run(h, free);

        Assert.True(result.Success, result.Message);
        Assert.Equal("exploración", result.Mode);
        Assert.Equal(3, result.ModelCalls);
        Assert.All(h.Anthropic.Requests, r => Assert.True(r.Sensitive));
        Assert.True(result.ProposalCreated);
        var proposal = h.BrowserPlaybooks.Proposals["mail"];
        Assert.Contains("clic | button.nuevo", proposal);
        Assert.Contains("{{destinatario}}", proposal);
        Assert.True(PlaybookParser.Parse("mail", proposal).IsValid);
        Assert.DoesNotContain(StepAction.Explore, PlaybookParser.Parse("mail", proposal).Playbook!.Steps.Select(s => s.Action));
    }

    [Fact]
    public async Task What_the_model_types_literally_is_never_learned()
    {
        var h = NewHarness();
        var script = new Queue<string>(new[]
        {
            """{"action":"fill","target":"input.para","value":"dato-privado-123"}""",
            """{"action":"done"}"""
        });
        h.Anthropic.Handler = (_, _) => Task.FromResult(BrowserSamples.Json(script.Dequeue()));

        await Run(h, "## Pasos\n1. ir | https://mail.example.com/inbox\n2. Escribe algo en el campo para");

        var proposal = h.BrowserPlaybooks.Proposals["mail"];
        Assert.DoesNotContain("dato-privado-123", proposal);
        Assert.Contains(PlaybookParser.ReviewMarker, proposal);
    }

    [Fact]
    public async Task A_failing_step_is_repaired_by_the_model_and_the_updated_playbook_is_proposed()
    {
        var h = NewHarness();
        h.BrowserSessions.Session.BrokenSelectors.Add("button.nuevo");
        var script = new Queue<string>(new[] { """{"action":"click","target":"button.redactar-nuevo"}""", """{"action":"done"}""" });
        h.Anthropic.Handler = (_, _) => Task.FromResult(BrowserSamples.Json(script.Dequeue()));

        var result = await Run(h);

        Assert.True(result.Success, result.Message);
        Assert.Equal("reparación", result.Mode);
        Assert.Equal(2, result.ModelCalls);
        var proposal = h.BrowserPlaybooks.Proposals["mail"];
        Assert.Contains("button.redactar-nuevo", proposal);
        Assert.DoesNotContain("button.nuevo |", proposal);
        Assert.True(PlaybookParser.Parse("mail", proposal).IsValid);
    }

    [Fact]
    public async Task An_irreversible_step_that_fails_is_never_repaired()
    {
        var h = NewHarness();
        h.BrowserSessions.Session.BrokenSelectors.Add("button.enviar");

        var result = await Run(h);

        Assert.False(result.Success);
        Assert.Equal(0, result.ModelCalls);
    }

    [Fact]
    public async Task A_destructive_looking_action_proposed_by_the_model_is_blocked()
    {
        var h = NewHarness();
        h.BrowserSessions.Session.Snapshot = () => new PageSnapshot("https://mail.example.com/inbox", "Bandeja", new[] { new PageElement("button.borrar", "button", "Eliminar cuenta") });
        var script = new Queue<string>(new[] { """{"action":"click","target":"button.borrar"}""", """{"action":"done"}""" });
        h.Anthropic.Handler = (_, _) => Task.FromResult(BrowserSamples.Json(script.Dequeue()));

        var result = await Run(h, "## Pasos\n1. ir | https://mail.example.com/inbox\n2. Abre el menú de ayuda");

        Assert.DoesNotContain("click button.borrar", h.BrowserSessions.Session.Actions);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task Exploration_stops_at_the_action_limit()
    {
        var h = NewHarness();
        h.Anthropic.Handler = (_, _) => Task.FromResult(BrowserSamples.Json("""{"action":"click","target":"button.nuevo"}"""));

        var result = await Run(h, "## Pasos\n1. ir | https://mail.example.com/inbox\n2. Haz algo imposible");

        Assert.False(result.Success);
        Assert.Equal(h.BrowserConfig.MaxExploreActions, result.ModelCalls);
    }

    [Fact]
    public async Task Approving_the_proposal_makes_the_next_run_deterministic_without_the_model()
    {
        var h = NewHarness();
        await h.BrowserSites.SaveAsync(BrowserSamples.Site(), default);
        await h.BrowserPlaybooks.WriteAsync("mail", "## Entradas\n- destinatario | ¿A quién? | correo\n## Pasos\n1. ir | https://mail.example.com/inbox\n2. Abre la redacción", default);
        var script = new Queue<string>(new[] { """{"action":"click","target":"button.nuevo"}""", """{"action":"done"}""" });
        h.Anthropic.Handler = (_, _) => Task.FromResult(BrowserSamples.Json(script.Dequeue()));
        var manager = new BrowserManager(h.BrowserSites, h.BrowserPlaybooks, h.BrowserSessions, h.BrowserRunner, h.BrowserSecrets, Options.Create(h.BrowserConfig));
        var values = new Dictionary<string, string> { ["destinatario"] = "a@b.com" };

        var first = await manager.RunAsync("mail", values, null, default);
        await manager.ApproveProposalAsync("mail", default);
        h.Anthropic.Requests.Clear();
        h.BrowserSessions.Session.Actions.Clear();
        var second = await manager.RunAsync("mail", values, null, default);

        Assert.Equal("exploración", first.Mode);
        Assert.True(first.ProposalCreated);
        Assert.Equal("reproducción", second.Mode);
        Assert.Equal(0, second.ModelCalls);
        Assert.Empty(h.Anthropic.Requests);
        Assert.Contains("click button.nuevo", h.BrowserSessions.Session.Actions);
        Assert.Empty(h.BrowserPlaybooks.Proposals);          // la propuesta se consumió
    }

    [Fact]
    public async Task A_proposal_with_values_to_review_cannot_be_approved()
    {
        var h = NewHarness();
        await h.BrowserSites.SaveAsync(BrowserSamples.Site(), default);
        await h.BrowserPlaybooks.WriteProposalAsync("mail", "## Pasos\n1. escribir | #x | {{REVISAR}}", default);
        var manager = new BrowserManager(h.BrowserSites, h.BrowserPlaybooks, h.BrowserSessions, h.BrowserRunner, h.BrowserSecrets, Options.Create(h.BrowserConfig));

        var ex = await Assert.ThrowsAsync<OfficeValidationException>(() => manager.ApproveProposalAsync("mail", default));

        Assert.Contains("revisar", ex.Errors[0], StringComparison.OrdinalIgnoreCase);
    }
}

public class BrowserTargetCleaningTests
{
    private static readonly PageSnapshot Page = new("https://mail.example.com", "t", new[] { new PageElement("text=\"Redactar\"", "button", "Redactar"), new PageElement("#nuevo", "button", "Nuevo") });

    [Theory]
    [InlineData("[text=\"Redactar\"]", "text=\"Redactar\"")]
    [InlineData("text=\"Redactar\"", "text=\"Redactar\"")]
    [InlineData("`#nuevo`", "#nuevo")]
    [InlineData("[#nuevo]", "#nuevo")]
    [InlineData("#otro", "#otro")]
    public void Selectors_copied_with_brackets_are_cleaned_when_they_match_the_page(string given, string expected) =>
        Assert.Equal(expected, BrowserRunner.CleanTarget(given, Page));
}

public class BrowserInvalidActionTests
{
    [Fact]
    public async Task A_key_given_as_target_is_accepted_and_an_invalid_action_is_explained_to_the_model()
    {
        var h = new OfficeHarness();
        var script = new Queue<string>(new[]
        {
            """{"action":"press","target":"Enter"}""",       // tolerado
            """{"action":"fill","target":"#x"}""",           // falta value: se le explica
            """{"action":"done"}"""
        });
        h.Anthropic.Handler = (_, _) => Task.FromResult(BrowserSamples.Json(script.Dequeue()));

        var result = await h.BrowserRunner.RunAsync(BrowserSamples.Site(), BrowserSamples.Parse("## Pasos\n1. ir | https://mail.example.com/inbox\n2. Confirma la dirección"),
            new Dictionary<string, string>(), null, CancellationToken.None);

        Assert.True(result.Success, result.Message);
        Assert.Contains("press Enter", h.BrowserSessions.Session.Actions);
        Assert.Contains("falta value", h.Anthropic.Requests.Last().Messages.Single().Content);
    }
}

public class BrowserReadingTests
{
    private const string ReadPlaybook = """
        ---
        nombre: Leer correos
        disparadores: leer correos, mis correos
        ---
        ## Pasos
        1. ir | https://mail.example.com/inbox
        2. extraer | tr.zA | 20 | Últimos correos
        """;

    private static ExtractedRow Row(int i) => new($"texto {i}", $"Remitente {i}", $"r{i}@example.com", $"Asunto secreto {i}", $"Vista previa {i}", "10 nov");

    private static async Task<(OfficeHarness H, Guid Id)> Setup(int rows = 25)
    {
        var h = new OfficeHarness();
        await h.BrowserSites.SaveAsync(BrowserSamples.Site(), default);
        await h.BrowserPlaybooks.WriteAsync("mail", ReadPlaybook, default);
        h.BrowserSessions.Session.ExtractRows.AddRange(Enumerable.Range(1, rows).Select(Row));
        return (h, h.Conversations.GetOrCreate(null).Id);
    }

    [Fact]
    public async Task The_extract_step_reads_at_most_the_requested_rows_and_logs_only_the_count()
    {
        var (h, _) = await Setup();

        var result = await h.BrowserRunner.RunAsync(BrowserSamples.Site(), PlaybookParser.Parse("mail", ReadPlaybook).Playbook!, new Dictionary<string, string>(), null, CancellationToken.None);

        Assert.True(result.Success, result.Message);
        Assert.Equal(20, result.Data!.Count);
        Assert.Contains("extract tr.zA 20", h.BrowserSessions.Session.Actions);
        Assert.DoesNotContain(result.Log, l => l.Contains("secreto") || l.Contains("example.com/r"));
        Assert.Empty(h.Anthropic.Requests);
    }

    [Fact]
    public async Task Extracting_zero_rows_fails_with_a_clear_message()
    {
        var (h, _) = await Setup(rows: 0);

        var result = await h.BrowserRunner.RunAsync(BrowserSamples.Site(), PlaybookParser.Parse("mail", ReadPlaybook).Playbook!, new Dictionary<string, string>(), null, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("No encontré filas", result.Message);
    }

    [Fact]
    public async Task A_read_only_playbook_runs_at_once_without_approval_and_the_content_stays_out_of_the_history()
    {
        var (h, id) = await Setup();

        var response = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("lee mis correos", id), default);

        Assert.Null(response.BrowserApproval);
        Assert.Equal(1, h.BrowserSessions.Opened);
        Assert.Contains("| 1 | Remitente 1 (r1@example.com) | Asunto secreto 1 |", response.Reply);
        Assert.Contains("Asunto secreto 20", response.Reply);
        Assert.DoesNotContain("Asunto secreto 21", response.Reply);
        var history = h.Conversations.Get(id)!.Messages.Last().Content;
        Assert.DoesNotContain("secreto", history);                        // lo que ven los modelos no incluye los correos
        Assert.Contains("20 filas", history);
        Assert.Null(h.BrowserPending.Get(id));
        Assert.Empty(h.Anthropic.Requests);                               // leer no gasta modelo
    }

    [Fact]
    public async Task A_failed_read_clears_the_pending_state_so_the_person_can_just_ask_again()
    {
        var (h, id) = await Setup();
        h.BrowserSessions.Session.ExtractRows.Clear();

        var turn = await h.Browser.HandleAsync(id, "leer mis correos", null, true, default);

        Assert.Contains("❌", turn!.Reply);
        Assert.Contains("pídemelo de nuevo", turn.Reply);
        Assert.Null(h.BrowserPending.Get(id));
    }

    [Fact]
    public void Rows_are_rendered_as_a_markdown_table_with_pipes_escaped()
    {
        var table = BrowserWorkflow.RenderRows(new[] { new ExtractedRow("t", "Ana", "ana@x.com", "A | B", "línea1\nlínea2", "hoy") });

        Assert.Contains("| 1 | Ana (ana@x.com) | A \\| B | hoy | línea1 línea2 |", table);
    }

    [Fact]
    public void The_extract_step_survives_render_and_parse()
    {
        var p = PlaybookParser.Parse("mail", ReadPlaybook).Playbook!;

        var again = PlaybookParser.Parse("mail", PlaybookParser.Render(p)).Playbook!;

        Assert.Equal(StepAction.Extract, again.Steps[1].Action);
        Assert.Equal("20", again.Steps[1].Value);
    }
}

public class BrowserSiteCredentialsTests
{
    private const string Secret = "Cl4ve-Solo-Memoria";

    private static BrowserManager Manager(OfficeHarness h) =>
        new(h.BrowserSites, h.BrowserPlaybooks, h.BrowserSessions, h.BrowserRunner, h.BrowserSecrets, Microsoft.Extensions.Options.Options.Create(h.BrowserConfig));

    [Fact]
    public async Task Site_credentials_are_kept_in_memory_only_and_used_when_a_run_gives_none()
    {
        var h = new OfficeHarness();
        var manager = Manager(h);

        var dto = await manager.SaveSiteAsync(BrowserSamples.Site(), default, new BrowserCredentials("yo", Secret));
        await manager.SavePlaybookAsync("mail", "## Pasos\n1. ir | https://mail.example.com/inbox\n2. escribir | #clave | {{credencial.clave}}", default);
        var result = await manager.RunAsync("mail", null, null, default);        // sin credenciales en la ejecución

        Assert.True(dto.HasCredentials);
        Assert.True(result.Success, result.Message);
        Assert.Contains("value:" + Secret, h.BrowserSessions.Session.Actions);
        Assert.DoesNotContain(result.Log, l => l.Contains(Secret));
        Assert.DoesNotContain(Secret, System.Text.Json.JsonSerializer.Serialize(dto));
    }

    [Fact]
    public async Task Updating_a_site_without_credentials_keeps_them_and_they_can_be_forgotten_or_removed_with_the_site()
    {
        var h = new OfficeHarness();
        var manager = Manager(h);
        await manager.SaveSiteAsync(BrowserSamples.Site(), default, new BrowserCredentials("yo", Secret));

        var again = await manager.SaveSiteAsync(BrowserSamples.Site() with { Name = "Otro nombre" }, default);
        Assert.True(again.HasCredentials);

        manager.ClearCredentials("mail");
        Assert.Null(h.BrowserSecrets.Get("mail"));

        await manager.SaveSiteAsync(BrowserSamples.Site(), default, new BrowserCredentials("yo", Secret));
        await manager.DeleteSiteAsync("mail", default);
        Assert.Null(h.BrowserSecrets.Get("mail"));
    }

    [Fact]
    public void Credentials_given_in_a_run_win_over_the_stored_ones_and_nothing_is_written_to_disk()
    {
        var store = new BrowserCredentialStore();
        store.Set("mail", new BrowserCredentials("a", "guardada"));

        Assert.Equal("guardada", store.Get("mail")!.Password);
        Assert.Equal("***", store.Get("mail")!.ToString());
        Assert.Null(new BrowserCredentialStore().Get("mail"));              // otra instancia (otro arranque) no la tiene
    }
}

public class BrowserCredentialFileTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "office-cred-" + Guid.NewGuid().ToString("N"));

    private MarkdownCredentialStore NewStore() =>
        new(Microsoft.Extensions.Options.Options.Create(new BrowserOptions { DataDirectory = Path.Combine(_dir, "data") }));

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch (IOException) { }
    }

    [Fact]
    public void Credentials_are_saved_in_a_markdown_file_per_connection_and_survive_a_restart()
    {
        NewStore().Set("fine360", new BrowserCredentials("addiazf", "Cl@ve con espacios 123"));

        var file = Path.Combine(_dir, "data", "credentials", "fine360.md");
        Assert.True(File.Exists(file));
        Assert.Contains("usuario: addiazf", File.ReadAllText(file));

        var afterRestart = NewStore().Get("fine360");               // otra instancia = otro arranque de la app
        Assert.Equal("addiazf", afterRestart!.Username);
        Assert.Equal("Cl@ve con espacios 123", afterRestart.Password);
        Assert.Null(NewStore().Get("otra"));
    }

    [Fact]
    public void Clearing_deletes_the_file_and_ids_cannot_escape_the_folder()
    {
        var store = NewStore();
        store.Set("mail", new BrowserCredentials("a", "b"));

        store.Clear("mail");

        Assert.False(File.Exists(Path.Combine(_dir, "data", "credentials", "mail.md")));
        Assert.Null(NewStore().Get("mail"));
        Assert.Throws<ArgumentException>(() => store.Set("..\\fuga", new BrowserCredentials("a", "b")));
        Assert.Throws<ArgumentException>(() => store.Get("../fuga"));
    }

    [Fact]
    public void Parse_reads_user_and_password_and_ignores_everything_else()
    {
        var parsed = MarkdownCredentialStore.Parse("# Credenciales\n> nota usuario: no\nusuario: ana\nclave: x:y z\n");

        Assert.Equal("ana", parsed!.Username);
        Assert.Equal("x:y z", parsed.Password);
        Assert.Null(MarkdownCredentialStore.Parse("# vacío\n"));
    }

    [Fact]
    public void The_credentials_folder_is_ignored_by_git()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ATLAS.sln"))) dir = dir.Parent;
        var ignore = File.ReadAllLines(Path.Combine(dir!.FullName, ".gitignore")).Select(l => l.Trim()).ToList();

        Assert.Contains("browser-data/", ignore);
    }

    [Fact]
    public async Task Editing_a_connection_updates_its_fields_and_merges_partial_credentials_without_exposing_the_password()
    {
        var h = new OfficeHarness();
        var store = NewStore();
        var manager = new BrowserManager(h.BrowserSites, h.BrowserPlaybooks, h.BrowserSessions, h.BrowserRunner, store, Microsoft.Extensions.Options.Options.Create(h.BrowserConfig));
        await manager.SaveSiteAsync(BrowserSamples.Site(), default, new BrowserCredentials("ana", "clave-1"));

        // Edición completa: nombre, URL, dominios, modo, activa/inactiva y solo el usuario (la clave se conserva).
        var edited = await manager.SaveSiteAsync(
            BrowserSamples.Site() with { Name = "Nuevo nombre", Url = "https://otro.example.com/", AllowedDomains = new[] { "example.com", "otro.example.com" }, Headless = true, Enabled = false },
            default, new BrowserCredentials("beto", null));

        Assert.Equal("Nuevo nombre", edited.Name);
        Assert.Equal("https://otro.example.com/", edited.Url);
        Assert.True(edited.Headless);
        Assert.False(edited.Enabled);
        Assert.Equal("beto", edited.Username);
        Assert.True(edited.HasCredentials);
        Assert.Equal("clave-1", store.Get("mail")!.Password);                               // conservada
        Assert.DoesNotContain("clave-1", System.Text.Json.JsonSerializer.Serialize(edited)); // nunca sale por la API
        await Assert.ThrowsAsync<OfficeValidationException>(() => manager.SaveSiteAsync(BrowserSamples.Site() with { Url = "http://127.0.0.1" }, default));
    }
}

public class BrowserWorkflowTests
{
    private static async Task<(OfficeHarness H, Guid Id)> Setup()
    {
        var h = new OfficeHarness();
        await h.BrowserSites.SaveAsync(BrowserSamples.Site(), default);
        await h.BrowserPlaybooks.WriteAsync("mail", BrowserSamples.Mail, default);
        h.Anthropic.Handler = (req, _) => Task.FromResult(req.Purpose == "browser-draft"
            ? BrowserSamples.Json("""{"asunto":"Asunto del correo","cuerpo":"Estimado,\nAdjunto la solución del caso."}""")
            : BrowserSamples.Json("""{"action":"done"}"""));
        var conversation = h.Conversations.GetOrCreate(null);
        conversation.Add(new ATLAS.Domain.Entities.ChatMessage { Role = ATLAS.Domain.Enums.ChatRole.Assistant, Content = "Solución del caso INC-1: aislar el equipo." });
        return (h, conversation.Id);
    }

    private static Task<BrowserTurn?> Say(OfficeHarness h, Guid id, string text, string? choice = null, bool allowStart = true) =>
        h.Browser.HandleAsync(id, text, choice, allowStart, CancellationToken.None);

    [Fact]
    public async Task Asks_drafts_shows_the_draft_and_only_runs_after_approval()
    {
        var (h, id) = await Setup();

        var q1 = await Say(h, id, "quiero enviar correo con la solución");
        Assert.Contains("¿A quién", q1!.Reply);
        Assert.Null(q1.Approval);

        var q2 = await Say(h, id, "a mi jefe: jefe@empresa.com");
        Assert.Contains("¿Para qué", q2!.Reply);

        var draft = await Say(h, id, "informar la solución del incidente");
        Assert.NotNull(draft!.Approval);
        Assert.True(draft.Approval!.Irreversible);
        Assert.Contains("Asunto del correo", draft.Reply);
        Assert.Contains(draft.Approval.Fields, f => f.Name == "destinatario" && f.Value == "jefe@empresa.com");
        Assert.Equal(0, h.BrowserSessions.Opened);                                   // nada se ejecutó sin aprobar
        Assert.All(h.Anthropic.Requests, r => Assert.True(r.Sensitive));

        var done = await Say(h, id, "aprobar");
        Assert.Contains("✅", done!.Reply);
        Assert.Equal(1, h.BrowserSessions.Opened);
        Assert.Contains("value:Asunto del correo", h.BrowserSessions.Session.Actions);
        Assert.Contains("click button.enviar", h.BrowserSessions.Session.Actions);
        Assert.Null(h.BrowserPending.Get(id));
    }

    [Fact]
    public async Task The_draft_prompt_carries_the_conversation_context_as_data()
    {
        var (h, id) = await Setup();
        await Say(h, id, "mandar correo a jefe@empresa.com");
        await Say(h, id, "informar");

        var draftRequest = h.Anthropic.Requests.Single(r => r.Purpose == "browser-draft");
        var content = draftRequest.Messages.Single().Content;
        Assert.Contains("<contexto>", content);
        Assert.Contains("aislar el equipo", content);
        Assert.Contains("no instrucciones", draftRequest.System);
    }

    [Fact]
    public async Task An_email_in_the_first_message_skips_that_question()
    {
        var (h, id) = await Setup();

        var turn = await Say(h, id, "enviar correo a jefe@empresa.com");

        Assert.Contains("¿Para qué", turn!.Reply);
    }

    [Fact]
    public async Task An_invalid_email_is_asked_again()
    {
        var (h, id) = await Setup();
        await Say(h, id, "enviar correo");

        var turn = await Say(h, id, "mi jefe");

        Assert.Contains("dirección de correo", turn!.Reply);
        var next = await Say(h, id, "ana@empresa.com");
        Assert.Contains("¿Para qué", next!.Reply);                                   // con uno válido sigue avanzando
    }

    [Fact]
    public async Task Cancelling_never_runs_anything()
    {
        var (h, id) = await Setup();
        await Say(h, id, "enviar correo a a@b.com");
        await Say(h, id, "motivo");

        var cancel = await Say(h, id, "no");

        Assert.Contains("cancelé", cancel!.Reply);
        Assert.Equal(0, h.BrowserSessions.Opened);
        Assert.Null(h.BrowserPending.Get(id));
    }

    [Fact]
    public async Task Cancel_by_button_choice_works_while_asking()
    {
        var (h, id) = await Setup();
        await Say(h, id, "enviar correo");

        var cancel = await Say(h, id, "Cancelar", BrowserWorkflow.CancelChoice);

        Assert.Contains("cancelé", cancel!.Reply);
        Assert.Null(h.BrowserPending.Get(id));
    }

    [Fact]
    public async Task Asking_for_a_change_redrafts_and_asks_for_approval_again()
    {
        var (h, id) = await Setup();
        await Say(h, id, "enviar correo a a@b.com");
        await Say(h, id, "motivo");

        var again = await Say(h, id, "hazlo más corto y menciona el ticket");

        Assert.NotNull(again!.Approval);
        Assert.Equal(0, h.BrowserSessions.Opened);
        Assert.Contains("CAMBIO PEDIDO", h.Anthropic.Requests.Last(r => r.Purpose == "browser-draft").Messages.Single().Content);
    }

    [Fact]
    public async Task A_message_without_trigger_or_with_another_flow_waiting_is_not_handled()
    {
        var (h, id) = await Setup();

        Assert.Null(await Say(h, id, "explícame la inyección de dependencias"));
        Assert.Null(await Say(h, id, "enviar correo", allowStart: false));
    }

    [Fact]
    public async Task A_failed_run_keeps_the_draft_so_the_person_can_log_in_and_approve_again()
    {
        var (h, id) = await Setup();
        h.BrowserSessions.Session.BrokenSelectors.Add("button.enviar");   // irreversible: no se repara
        await Say(h, id, "enviar correo a a@b.com");
        await Say(h, id, "motivo");

        var failed = await Say(h, id, "aprobar");

        Assert.Contains("❌", failed!.Reply);
        Assert.NotNull(h.BrowserPending.Get(id));

        h.BrowserSessions.Session.BrokenSelectors.Clear();
        var retry = await h.Browser.ApproveAsync(id, new BrowserCredentials("u", "p"), default);

        Assert.True(retry.Result!.Success);
        Assert.Contains("✅", retry.Reply);
        Assert.Contains(h.Conversations.Get(id)!.Messages, m => m.Content.Contains("✅"));
    }

    [Fact]
    public async Task Approving_without_a_draft_is_rejected()
    {
        var (h, id) = await Setup();

        await Assert.ThrowsAsync<OfficeValidationException>(() => h.Browser.ApproveAsync(id, null, default));
    }

    [Fact]
    public async Task The_chat_returns_the_approval_card_in_the_response_contract()
    {
        var (h, id) = await Setup();

        var first = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("enviar correo a jefe@empresa.com", id), default);
        Assert.Contains("¿Para qué", first.Reply);
        Assert.Null(first.BrowserApproval);

        var second = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("informar la solución", id), default);
        Assert.NotNull(second.BrowserApproval);
        Assert.Equal(id, second.BrowserApproval!.ConversationId);
        Assert.False(second.Delegated);
    }
}

public class BrowserStoreAndManagerTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "office-browser-" + Guid.NewGuid().ToString("N"));

    private BrowserOptions Options() => new() { DataDirectory = Path.Combine(_dir, "data"), PlaybooksDirectory = Path.Combine(_dir, "playbooks") };

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch (IOException) { }
    }

    [Fact]
    public async Task Sites_persist_to_disk_and_survive_a_new_instance()
    {
        var options = Microsoft.Extensions.Options.Options.Create(Options());
        await new JsonBrowserSiteStore(options).SaveAsync(BrowserSamples.Site(), default);

        var reloaded = new JsonBrowserSiteStore(options);

        Assert.Equal("Correo de prueba", reloaded.Get("mail")!.Name);
        Assert.True(await reloaded.DeleteAsync("mail", default));
        Assert.Empty(new JsonBrowserSiteStore(options).GetAll());
    }

    [Fact]
    public async Task The_sites_file_never_contains_credentials()
    {
        var options = Microsoft.Extensions.Options.Options.Create(Options());
        await new JsonBrowserSiteStore(options).SaveAsync(BrowserSamples.Site(), default);

        var text = File.ReadAllText(Path.Combine(_dir, "data", "sites.json")).ToLowerInvariant();

        Assert.DoesNotContain("password", text);
        Assert.DoesNotContain("clave", text);
    }

    [Fact]
    public async Task Playbooks_and_proposals_are_markdown_files_and_ids_cannot_escape_the_folder()
    {
        var store = new MarkdownPlaybookStore(Microsoft.Extensions.Options.Options.Create(Options()));
        await store.WriteAsync("mail", "# a", default);
        await store.WriteProposalAsync("mail", "# b", default);

        Assert.True(File.Exists(Path.Combine(_dir, "playbooks", "mail.md")));
        Assert.True(File.Exists(Path.Combine(_dir, "playbooks", "mail.propuesta.md")));
        await store.DeleteProposalAsync("mail", default);
        Assert.Null(await store.ReadProposalAsync("mail", default));
        await Assert.ThrowsAsync<ArgumentException>(() => store.ReadAsync("..\\secreto", default));
        await Assert.ThrowsAsync<ArgumentException>(() => store.WriteAsync("../x", "x", default));
    }

    private BrowserManager NewManager(OfficeHarness h) =>
        new(h.BrowserSites, h.BrowserPlaybooks, h.BrowserSessions, h.BrowserRunner, h.BrowserSecrets, Microsoft.Extensions.Options.Options.Create(h.BrowserConfig));

    [Fact]
    public async Task A_new_site_gets_a_valid_template_playbook_and_invalid_sites_are_rejected()
    {
        var h = new OfficeHarness();
        var manager = NewManager(h);

        var dto = await manager.SaveSiteAsync(new BrowserSite("Gmail", "Gmail", "https://mail.google.com/mail/u/0/#inbox", Array.Empty<string>()), default);

        Assert.Equal("gmail", dto.Id);
        Assert.Contains("mail.google.com", dto.AllowedDomains);
        Assert.True(PlaybookParser.Parse("gmail", h.BrowserPlaybooks.Playbooks["gmail"]).IsValid);
        await Assert.ThrowsAsync<OfficeValidationException>(() => manager.SaveSiteAsync(new BrowserSite("x", "x", "http://127.0.0.1", new[] { "127.0.0.1" }), default));
    }

    [Fact]
    public async Task Editing_a_playbook_validates_it_and_deleting_a_site_clears_everything()
    {
        var h = new OfficeHarness();
        var manager = NewManager(h);
        await manager.SaveSiteAsync(BrowserSamples.Site(), default);

        await Assert.ThrowsAsync<OfficeValidationException>(() => manager.SavePlaybookAsync("mail", "## Pasos\n1. volar | x", default));
        var saved = await manager.SavePlaybookAsync("mail", BrowserSamples.Mail, default);
        Assert.Equal(4, saved.Inputs.Count);

        Assert.True(await manager.DeleteSiteAsync("mail", default));
        Assert.Empty(h.BrowserPlaybooks.Playbooks);
        Assert.Contains("mail", h.BrowserSessions.Cleared);
        Assert.False(await manager.DeleteSiteAsync("mail", default));
    }

    [Fact]
    public async Task A_direct_run_requires_every_input_and_manual_login_opens_the_browser()
    {
        var h = new OfficeHarness();
        var manager = NewManager(h);
        await manager.SaveSiteAsync(BrowserSamples.Site(), default);
        await manager.SavePlaybookAsync("mail", BrowserSamples.Mail, default);

        var ex = await Assert.ThrowsAsync<OfficeValidationException>(() => manager.RunAsync("mail", new Dictionary<string, string> { ["destinatario"] = "a@b.com" }, null, default));
        Assert.Contains("Faltan valores", ex.Errors[0]);

        var ok = await manager.RunAsync("mail", BrowserSamples.Values(), null, default);
        Assert.True(ok.Success, ok.Message);

        await manager.LoginAsync("mail", default);
        Assert.Equal(1, h.BrowserSessions.Logins);
    }
}
