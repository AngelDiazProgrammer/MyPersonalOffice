using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Domain.Entities;
using AI.Agent.Office.Domain.Enums;
using AI.Agent.Office.Infrastructure.Configuration;
using AI.Agent.Office.Infrastructure.Documents;
using AI.Agent.Office.Infrastructure.Knowledge;
using AI.Agent.Office.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using TaskStatus = AI.Agent.Office.Domain.Enums.TaskStatus;

namespace AI.Agent.Office.Tests;

public class KnowledgeBaseTests
{
    private static MarkdownKnowledgeBaseService LoadShipped()
    {
        var service = new MarkdownKnowledgeBaseService(
            Microsoft.Extensions.Options.Options.Create(new KnowledgeOptions()), NullLogger<MarkdownKnowledgeBaseService>.Instance);
        Assert.True(service.ReloadAsync().GetAwaiter().GetResult() >= 5, "No se cargó la base de conocimiento del repositorio.");
        return service;
    }

    // The five test situations given to the user, with the code each one must resolve to.
    public static IEnumerable<object[]> Cases => new[]
    {
        new object[] { "SEG-001", "Ayer le presté mi usuario y contraseña de la VPN a un compañero de otra área porque no tenía acceso y necesitaba terminar un informe. ¿Qué debo hacer ahora?" },
        new object[] { "SEG-002", "Encontré un pendrive en el parqueadero y lo conecté a mi computador del trabajo para ver de quién era; se abrió una carpeta con archivos raros. ¿Cómo procedo?" },
        new object[] { "SEG-003", "Me llegó un correo diciendo que era del banco corporativo, di clic en el enlace e ingresé mi usuario y clave en una página que ahora me parece falsa." },
        new object[] { "SEG-004", "Reenvié a mi Gmail personal la base de datos de clientes para trabajar desde casa el fin de semana. ¿Eso es un problema?" },
        new object[] { "SEG-005", "Me robaron el celular de la empresa en el bus y tenía el correo corporativo y el chat de la compañía configurados. ¿Qué hago?" }
    };

    [Fact]
    public void The_shipped_knowledge_base_has_five_well_formed_situations()
    {
        var kb = LoadShipped();

        Assert.Equal(new[] { "SEG-001", "SEG-002", "SEG-003", "SEG-004", "SEG-005" }, kb.GetAll().Select(e => e.Code));
        Assert.All(kb.GetAll(), e =>
        {
            Assert.NotEmpty(e.Title);
            Assert.NotEmpty(e.Severity);
            Assert.True(e.Keywords.Count >= 5, e.Code);
            Assert.Contains("### Acciones inmediatas", e.Body);
            Assert.Contains("### Qué NO hacer", e.Body);
            Assert.Contains("### Plazo de reporte", e.Body);
        });
        Assert.Contains("SEG-CRED", kb.GetByCode("seg-001")!.Body);
        Assert.Contains("SEG-FUGA", kb.GetByCode("SEG-004")!.Body);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Each_test_situation_resolves_to_its_own_code_with_a_clear_margin(string expected, string message)
    {
        var hits = LoadShipped().Search(message, 3);

        Assert.Equal(expected, hits[0].Entry.Code);
        Assert.True(hits[0].Score >= 4, $"score {hits[0].Score}");
        if (hits.Count > 1) Assert.True(hits[0].Score >= hits[1].Score * 1.5, $"{hits[0].Score} vs {hits[1].Score} ({hits[1].Entry.Code})");
    }

    [Theory]
    [InlineData("Dame la formula para llegar al numero pi")]
    [InlineData("hola, ¿cómo estás?")]
    [InlineData("Escribe en C# una función que ordene una lista")]
    [InlineData("¿Qué es la inyección de dependencias?")]
    public void Unrelated_questions_do_not_enter_the_knowledge_workflow(string message)
    {
        var hits = LoadShipped().Search(message, 3);

        Assert.True(hits.Count == 0 || hits[0].Score < new KnowledgeOptions().MinScore, string.Join(", ", hits.Select(h => $"{h.Entry.Code}={h.Score}")));
    }

    [Theory]
    [InlineData("¿Qué debo hacer ante el incidente SEG-003?", "SEG-003")]
    [InlineData("necesito el procedimiento seg 5", "SEG-005")]
    [InlineData("explícame seg004 por favor", "SEG-004")]
    public void An_explicit_code_always_wins(string message, string expected)
    {
        var hit = LoadShipped().Search(message, 3)[0];

        Assert.Equal(expected, hit.Entry.Code);
        Assert.True(hit.Score >= 1000);
    }

    [Fact]
    public void Conjugations_and_accents_still_match_the_same_situation()
    {
        var kb = LoadShipped();

        Assert.Equal("SEG-001", kb.Search("estoy compartiendo mi contraseña con un compañero", 1)[0].Entry.Code);
        Assert.Equal("SEG-001", kb.Search("PRESTE MI USUARIO Y CONTRASENA A ALGUIEN", 1)[0].Entry.Code);
    }

    [Fact]
    public void Parser_reads_front_matter_flags_and_ignores_text_outside_entries()
    {
        const string md = "---\nconfidential: true\n---\n# Título\nIntro suelta\n\n## ABC-1 · Primera\n**Severidad:** Baja\n**Palabras clave:** uno, dos tres\ntexto\n\n## Otra sección\nno es entrada\n\n## ABC-2 — Segunda\ncuerpo";

        var entries = MarkdownKnowledgeBaseService.Parse(md, "x.md");

        Assert.Equal(new[] { "ABC-1", "ABC-2" }, entries.Select(e => e.Code));
        Assert.All(entries, e => Assert.True(e.Confidential));
        Assert.Equal("Baja", entries[0].Severity);
        Assert.Equal(new[] { "uno", "dos tres" }, entries[0].Keywords);
        Assert.DoesNotContain("no es entrada", entries[0].Body);
    }

    [Fact]
    public void Duplicated_codes_are_ignored_and_reload_picks_up_new_files()
    {
        var dir = Path.Combine(Path.GetTempPath(), "kb-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "a.md"), "## ZZ-1 · Uno\n**Palabras clave:** alfa\ncuerpo");
            File.WriteAllText(Path.Combine(dir, "b.md"), "## ZZ-1 · Duplicado\n## ZZ-2 · Dos\n**Palabras clave:** beta");
            var kb = new MarkdownKnowledgeBaseService(
                Microsoft.Extensions.Options.Options.Create(new KnowledgeOptions { Directory = dir }), NullLogger<MarkdownKnowledgeBaseService>.Instance);

            Assert.Equal(2, kb.ReloadAsync().GetAwaiter().GetResult());
            Assert.Equal("Uno", kb.GetByCode("ZZ-1")!.Title);

            File.WriteAllText(Path.Combine(dir, "c.md"), "## ZZ-3 · Tres\n**Palabras clave:** gamma");
            Assert.Equal(3, kb.ReloadAsync().GetAwaiter().GetResult());
        }
        finally { Directory.Delete(dir, true); }
    }
}

public class DeliveryFormatTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "fmt-" + Guid.NewGuid().ToString("N"));

    public DeliveryFormatTests() => Directory.CreateDirectory(_dir);

    private DeliveryFormatService Create() =>
        new(Microsoft.Extensions.Options.Options.Create(new KnowledgeOptions { FormatDirectory = _dir }), NullLogger<DeliveryFormatService>.Instance);

    [Fact]
    public async Task The_newest_file_is_the_active_format()
    {
        File.WriteAllText(Path.Combine(_dir, "viejo.md"), "# Formato viejo");
        File.SetLastWriteTimeUtc(Path.Combine(_dir, "viejo.md"), DateTime.UtcNow.AddHours(-2));
        File.WriteAllText(Path.Combine(_dir, "nuevo.txt"), "Formato nuevo APA");

        var format = await Create().GetActiveFormatAsync();

        Assert.Equal("nuevo.txt", format!.FileName);
        Assert.Contains("APA", format.Text);
    }

    [Fact]
    public async Task A_word_template_is_read_as_text_with_headings_and_fields()
    {
        var docx = new MarkdownWordGenerator().Generate("Plantilla", "# Plantilla de respuesta\n\n## Resumen\n\n{{RESUMEN}}\n\n| Campo | Valor |\n|---|---|\n| Código | {{CODIGO}} |");
        await File.WriteAllBytesAsync(Path.Combine(_dir, "plantilla.docx"), docx);

        var format = await Create().GetActiveFormatAsync();

        Assert.Equal("plantilla.docx", format!.FileName);
        Assert.Contains("## Resumen", format.Text);
        Assert.Contains("{{RESUMEN}}", format.Text);
        Assert.Contains("{{CODIGO}}", format.Text);
    }

    [Fact]
    public async Task Missing_or_empty_folder_means_no_format_and_other_extensions_are_ignored()
    {
        Assert.Null(await Create().GetActiveFormatAsync());
        File.WriteAllText(Path.Combine(_dir, "notas.exe"), "x");
        Assert.Null(await Create().GetActiveFormatAsync());

        var missing = new DeliveryFormatService(
            Microsoft.Extensions.Options.Options.Create(new KnowledgeOptions { FormatDirectory = "no-existe-" + Guid.NewGuid() }), NullLogger<DeliveryFormatService>.Instance);
        Assert.Null(await missing.GetActiveFormatAsync());
    }

    [Fact]
    public void The_shipped_example_format_is_found_and_readable()
    {
        var service = new DeliveryFormatService(Microsoft.Extensions.Options.Options.Create(new KnowledgeOptions()), NullLogger<DeliveryFormatService>.Instance);

        var format = service.GetActiveFormatAsync().GetAwaiter().GetResult();

        Assert.NotNull(format);
        Assert.Contains("Resumen", format!.Text);
        Assert.Contains("Referencias", format.Text);
    }

    public void Dispose() { try { Directory.Delete(_dir, true); } catch (IOException) { } }
}

public class KnowledgeWorkflowTests
{
    private static OfficeHarness Create(bool confidential = false, bool allowFreeWithDocuments = true)
    {
        var h = new OfficeHarness(configureKnowledge: k => k.AllowFreeApisWithDocuments = allowFreeWithDocuments);
        h.Configuration.Definitions.AddRange(new[]
        {
            Workflow("kb-researcher", "Investigador KB", AgentStatus.Reading),
            Workflow("incident-responder", "Respondedor de Incidentes", AgentStatus.Working),
            Workflow("compliance-auditor", "Auditor de Cumplimiento", AgentStatus.Reviewing)
        });
        h.Registry.Load(h.Configuration.Definitions);
        h.Knowledge.Entries.Add(new KnowledgeEntry
        {
            Code = "SEG-003", Title = "Phishing", Severity = "Crítica", Keywords = new[] { "phishing", "enlace falso" },
            Body = "## SEG-003 · Phishing\n### Acciones inmediatas\n1. Cambiar la contraseña.\n2. Ticket SEG-PHISH.", SourceFile = "kb.md", Confidential = confidential
        });
        h.Knowledge.Entries.Add(new KnowledgeEntry
        {
            Code = "SEG-005", Title = "Robo de dispositivo", Keywords = new[] { "robaron" }, Body = "## SEG-005 · Robo\n### Acciones\n1. Llamar.", SourceFile = "kb.md"
        });
        return h;
    }

    private static AI.Agent.Office.Application.Agents.AgentDefinition Workflow(string id, string name, AgentStatus status) =>
        FakeAgentConfiguration.Make(id, name, status) with { WorkflowOnly = true };

    private static void ScriptWorkflow(OfficeHarness h, string audit = "VEREDICTO: APROBADO\nHALLAZGOS: Sin hallazgos", string research = "CÓDIGO: SEG-003\nCONFIANZA: alta")
    {
        h.Anthropic.Handler = (request, _) => Task.FromResult(FakeAnthropicService.Text(request.Purpose switch
        {
            "agent:pdf-reader" => "RESUMEN DEL LECTOR: el usuario hizo clic en un enlace falso",
            "agent:kb-researcher" => research,
            "agent:incident-responder" => "BORRADOR: paso 1 cambiar contraseña; ticket SEG-PHISH",
            "agent:word-writer" => "# Respuesta al incidente SEG-003\n## Resumen\nTexto formateado",
            "agent:compliance-auditor" => audit,
            _ => "otro"
        }));
    }

    [Fact]
    public async Task A_matching_question_skips_free_planning_and_runs_four_exclusive_agents_in_order()
    {
        var h = Create();
        ScriptWorkflow(h);

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Caí en un phishing, hice clic"), CancellationToken.None);

        Assert.True(result.Delegated);
        Assert.DoesNotContain(h.Anthropic.Requests, r => r.Purpose == "orchestrator:plan");          // deterministic: no LLM planning
        Assert.DoesNotContain(h.Anthropic.Requests, r => r.Purpose == "orchestrator:consolidate");
        Assert.Equal(new[] { "kb-researcher", "incident-responder", "word-writer", "compliance-auditor" },
            result.Tasks.OrderBy(t => t.Key).Select(t => t.AgentId));
        Assert.All(result.Tasks, t => Assert.Equal(TaskStatus.Completed, t.Status));
        Assert.Equal(new[] { "t1" }, result.Tasks.Single(t => t.Key == "t2").DependsOn);
        Assert.Equal(new[] { "t2" }, result.Tasks.Single(t => t.Key == "t3").DependsOn);
        Assert.Equal(new[] { "t1", "t3" }, result.Tasks.Single(t => t.Key == "t4").DependsOn);
    }

    [Fact]
    public async Task The_answer_is_delivered_as_word_and_pdf_in_apa_style_when_the_format_is_apa()
    {
        var h = Create();
        h.Formats.Format = new DeliveryFormat("formato-respuesta-apa.md", "# Formato APA 7\n## Resumen");
        ScriptWorkflow(h);

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Caí en un phishing, hice clic"), CancellationToken.None);

        Assert.Equal(new[] { "doc.docx", "doc.pdf" }, result.Documents!.Select(d => d.FileName).ToArray());
        Assert.Equal(DocumentStyle.Apa, Assert.Single(h.Word.WordStyles));                       // ONE docx: the writer task did not export its own copy
        var pdf = Assert.Single(h.Word.PdfExports);
        Assert.Equal(DocumentStyle.Apa, pdf.Style);
        Assert.Equal("Respuesta al incidente SEG-003: Phishing", pdf.Title);
        Assert.DoesNotContain("Base de conocimiento:", pdf.Markdown);                              // the source footer is chat-only
    }

    [Theory]
    [InlineData("# Formato de entrega · Respuesta a incidente (estilo APA)\n## Resumen\nTexto")]
    [InlineData("**Respuesta al incidente SEG-003: Phishing** — \"Respuesta al incidente SEG-003: Phishing\"\n**Autor:** Mesa\n## Resumen\nTexto")]
    [InlineData("## Resumen\nTexto sin título")]
    public async Task The_document_always_starts_with_the_canonical_title(string writerOutput)
    {
        var h = Create();
        h.Formats.Format = new DeliveryFormat("formato-apa.md", "# APA\n## Resumen");
        ScriptWorkflow(h);
        var previous = h.Anthropic.Handler;
        h.Anthropic.Handler = (request, ct) => request.Purpose == "agent:word-writer"
            ? Task.FromResult(FakeAnthropicService.Text(writerOutput))
            : previous(request, ct);

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Caí en un phishing, hice clic"), CancellationToken.None);

        var markdown = Assert.Single(h.Word.PdfExports).Markdown;
        Assert.StartsWith("# Respuesta al incidente SEG-003: Phishing", markdown);
        Assert.Equal(1, markdown.Split('\n').Count(l => l.StartsWith("# ")));                      // exactly one H1
        Assert.StartsWith("# Respuesta al incidente SEG-003: Phishing", result.Reply);
        Assert.Contains("## Resumen", markdown);
    }

    [Fact]
    public async Task A_non_apa_format_gives_the_default_style()
    {
        var h = Create();
        h.Formats.Format = new DeliveryFormat("plantilla.md", "# Plantilla\n## Resumen");
        ScriptWorkflow(h);

        await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Caí en un phishing, hice clic"), CancellationToken.None);

        Assert.Equal(DocumentStyle.Default, Assert.Single(h.Word.PdfExports).Style);
    }

    [Fact]
    public async Task Each_process_gets_only_what_it_needs_the_real_entries_the_previous_result_and_the_format()
    {
        var h = Create();
        ScriptWorkflow(h);

        await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Caí en un phishing, hice clic"), CancellationToken.None);

        string PromptOf(string agent) => h.Anthropic.Requests.Single(r => r.Purpose == "agent:" + agent).Messages[0].Content;
        Assert.Contains("Ticket SEG-PHISH", PromptOf("kb-researcher"));                         // the real KB text reaches the researcher
        Assert.Contains("Ticket SEG-PHISH", PromptOf("incident-responder"));
        Assert.Contains("CÓDIGO: SEG-003", PromptOf("incident-responder"));                    // ...together with the researcher's result
        Assert.Contains("formato-apa.md", PromptOf("word-writer"));                          // the delivery format
        Assert.Contains("BORRADOR", PromptOf("word-writer"));                                // the draft
        Assert.Contains("Ticket SEG-PHISH", PromptOf("compliance-auditor"));                   // auditor compares against the source
        Assert.Contains("Texto formateado", PromptOf("compliance-auditor"));
        Assert.DoesNotContain("Llamar.", PromptOf("kb-researcher") + "x".Replace("x", "")) ;   // (only matching candidates are given)
    }

    [Fact]
    public async Task The_reply_is_the_formatted_answer_plus_the_source_line()
    {
        var h = Create();
        ScriptWorkflow(h);

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Caí en un phishing, hice clic"), CancellationToken.None);

        Assert.StartsWith("# Respuesta al incidente SEG-003", result.Reply);
        Assert.Contains("📚 **Base de conocimiento:** SEG-003 — Phishing", result.Reply);
        Assert.Contains("aprobada por el auditor", result.Reply);
        Assert.Contains("formato-apa.md", result.Reply);
        Assert.False(result.IsError);
    }

    [Fact]
    public async Task When_the_auditor_corrects_the_answer_the_corrected_text_is_delivered()
    {
        var h = Create();
        ScriptWorkflow(h, audit: "VEREDICTO: CORREGIDO\nHALLAZGOS: El plazo era 30 minutos\n---TEXTO---\n# Respuesta corregida\nPlazo: 30 minutos");

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Caí en un phishing, hice clic"), CancellationToken.None);

        Assert.Contains("Plazo: 30 minutos", result.Reply);
        Assert.Contains("corregida por el auditor", result.Reply);
        Assert.DoesNotContain("Texto formateado", result.Reply);
    }

    [Fact]
    public async Task A_correction_cut_by_the_token_limit_never_replaces_the_complete_answer()
    {
        var h = Create();
        ScriptWorkflow(h, audit: "VEREDICTO: CORREGIDO\nHALLAZGOS: fecha\n---TEXTO---\n# Corte\nPlazo: 30 mi\n\n> ⚠️ _Respuesta cortada por el límite de tokens del agente. Pídele que continúe._");

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Caí en un phishing, hice clic"), CancellationToken.None);

        Assert.Contains("Texto formateado", result.Reply);               // the complete formatted answer is kept
        Assert.DoesNotContain("Plazo: 30 mi", result.Reply);
        Assert.Contains("se conserva el texto original", result.Reply);
    }

    [Fact]
    public async Task The_researcher_can_pick_another_candidate_or_none()
    {
        var h = Create();
        ScriptWorkflow(h, research: "CÓDIGO: SEG-005\nCONFIANZA: media");
        var picked = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("me robaron y fue un phishing"), CancellationToken.None);
        Assert.Contains("SEG-005 — Robo de dispositivo", picked.Reply);

        var h2 = Create();
        ScriptWorkflow(h2, research: "CÓDIGO: NINGUNO\nCONFIANZA: alta");
        var none = await h2.Orchestration.HandleMessageAsync(new ChatRequestDto("phishing raro"), CancellationToken.None);
        Assert.Contains("ninguna situación coincide", none.Reply);
    }

    [Fact]
    public async Task A_failed_step_still_returns_the_best_available_text_and_never_hangs()
    {
        var h = Create();
        h.Anthropic.Handler = (request, _) => request.Purpose == "agent:compliance-auditor"
            ? throw new LlmException("auditor caído")
            : Task.FromResult(FakeAnthropicService.Text(request.Purpose == "agent:word-writer" ? "# Formateado\n## Cuerpo\nTexto Formateado" : "ok"));

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Caí en un phishing, hice clic"), CancellationToken.None);

        Assert.Equal(TaskStatus.Failed, result.Tasks.Single(t => t.AgentId == "compliance-auditor").Status);
        Assert.Contains("Texto Formateado", result.Reply);
        Assert.Contains("sin auditar", result.Reply);
    }

    [Fact]
    public async Task The_knowledge_base_consultation_is_announced_as_an_event()
    {
        var h = Create();
        ScriptWorkflow(h);

        await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Caí en un phishing, hice clic"), CancellationToken.None);

        var consulted = Assert.Single(h.Publisher.Events, e => e.EventType == OfficeEventType.KnowledgeConsulted);
        Assert.Equal("kb-researcher", consulted.AgentId);
        Assert.Equal("SEG-003", consulted.AdditionalData!["code"]);
    }

    [Fact]
    public async Task Questions_outside_the_knowledge_base_use_the_normal_planner_which_never_sees_workflow_only_agents()
    {
        var h = Create();
        h.Script("""{"action":"respond","message":"respuesta normal","tasks":[]}""");

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("dame la fórmula de pi"), CancellationToken.None);

        Assert.Equal("respuesta normal", result.Reply);
        var plan = h.Anthropic.Requests.Single(r => r.Purpose == "orchestrator:plan");
        Assert.DoesNotContain("kb-researcher", plan.System);
        Assert.DoesNotContain("compliance-auditor", plan.System);
        Assert.DoesNotContain("kb-researcher", plan.StructuredOutput!.InputSchemaJson);
        Assert.Contains("developer", plan.StructuredOutput.InputSchemaJson);
    }

    [Fact]
    public async Task A_case_inside_an_attached_pdf_is_matched_against_the_knowledge_base_first()
    {
        var h = Create();
        var doc = new OfficeDocument { FileName = "caso.pdf", PageCount = 1, Text = "Hice clic en un enlace falso: phishing, ingresé mi clave." };
        h.Documents.Add(doc);
        ScriptWorkflow(h);

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Resuelve este caso", null, new[] { doc.Id }), CancellationToken.None);

        Assert.DoesNotContain(h.Anthropic.Requests, r => r.Purpose == "orchestrator:plan");           // no generic planning
        Assert.Equal(new[] { "pdf-reader", "kb-researcher", "incident-responder", "word-writer", "compliance-auditor" },
            result.Tasks.OrderBy(t => t.Key).Select(t => t.AgentId));                                // the reader goes first
        Assert.Equal(new[] { "t0" }, result.Tasks.Single(t => t.Key == "t1").DependsOn);
        Assert.Contains("SEG-003", result.Reply);
        var reader = h.Anthropic.Requests.Single(r => r.Purpose == "agent:pdf-reader");
        Assert.Contains("ingresé mi clave", reader.Messages[0].Content);                             // only the reader gets the PDF text...
        Assert.False(reader.Sensitive);                                                              // flag true (default): free APIs allowed
        var researcher = h.Anthropic.Requests.Single(r => r.Purpose == "agent:kb-researcher");
        Assert.DoesNotContain("<documento", researcher.Messages[0].Content);                         // ...the researcher gets the reader's summary
        Assert.Contains("RESUMEN DEL LECTOR", researcher.Messages[0].Content);
    }

    [Fact]
    public async Task With_the_flag_off_tasks_carrying_the_pdf_text_go_to_Claude_only()
    {
        var h = Create(allowFreeWithDocuments: false);
        var doc = new OfficeDocument { FileName = "caso.pdf", PageCount = 1, Text = "Caí en un phishing, ingresé mi clave." };
        h.Documents.Add(doc);
        ScriptWorkflow(h);

        await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Resuelve este caso", null, new[] { doc.Id }), CancellationToken.None);

        Assert.True(h.Anthropic.Requests.Single(r => r.Purpose == "agent:pdf-reader").Sensitive);
        Assert.True(h.Anthropic.Requests.Single(r => r.Purpose == "agent:kb-researcher").Sensitive);
        Assert.True(h.Anthropic.Requests.Single(r => r.Purpose == "agent:incident-responder").Sensitive);
    }

    [Fact]
    public async Task A_pdf_that_matches_nothing_in_the_base_falls_back_to_the_normal_flow()
    {
        var h = Create();
        var doc = new OfficeDocument { FileName = "x.pdf", PageCount = 1, Text = "contenido sin relación" };
        h.Documents.Add(doc);
        h.Script("""{"action":"respond","message":"normal con documento","tasks":[]}""");

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("lee este pdf", null, new[] { doc.Id }), CancellationToken.None);

        Assert.Equal("normal con documento", result.Reply);
        Assert.Empty(h.Tasks.GetAll());
    }

    [Fact]
    public async Task Confidential_entries_are_only_ever_sent_to_Claude()
    {
        var h = Create(confidential: true);
        ScriptWorkflow(h);

        await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Caí en un phishing, hice clic"), CancellationToken.None);

        Assert.All(h.Anthropic.Requests, r => Assert.True(r.Sensitive, r.Purpose));
    }

    [Fact]
    public async Task Non_confidential_entries_may_use_free_apis()
    {
        var h = Create();
        ScriptWorkflow(h);

        await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Caí en un phishing, hice clic"), CancellationToken.None);

        Assert.All(h.Anthropic.Requests, r => Assert.False(r.Sensitive, r.Purpose));
    }

    [Fact]
    public async Task If_a_workflow_agent_is_missing_it_falls_back_to_normal_planning()
    {
        var h = Create();
        h.Configuration.Definitions.RemoveAll(d => d.Id == "word-writer");
        h.Registry.Load(h.Configuration.Definitions);
        h.Script("""{"action":"respond","message":"plan normal","tasks":[]}""");

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Caí en un phishing, hice clic"), CancellationToken.None);

        Assert.Equal("plan normal", result.Reply);
    }

    [Fact]
    public async Task With_no_format_loaded_the_workflow_still_answers()
    {
        var h = Create();
        h.Formats.Format = null;
        ScriptWorkflow(h);

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Caí en un phishing, hice clic"), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Contains("No hay un formato de entrega cargado", h.Anthropic.Requests.Single(r => r.Purpose == "agent:word-writer").Messages[0].Content);
    }
}
