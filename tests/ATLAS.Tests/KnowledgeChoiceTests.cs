using ATLAS.Application.DTOs;
using ATLAS.Application.Interfaces;
using ATLAS.Domain.Entities;
using ATLAS.Domain.Enums;
using ATLAS.Infrastructure.Knowledge;
using ATLAS.Tests.TestSupport;
using TaskStatus = ATLAS.Domain.Enums.TaskStatus;

namespace ATLAS.Tests;

/// <summary>Soluciones alternativas por caso: lectura desde la base real y flujo de elección con la persona.</summary>
public class KnowledgeSolutionsParsingTests
{
    private static string RealKnowledgeBase()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "bases de conocimiento"))) dir = dir.Parent;
        return Path.Combine(dir!.FullName, "bases de conocimiento", "seguridad-incidentes.md");
    }

    [Fact]
    public void Every_real_situation_has_five_distinct_solutions_and_exactly_one_recommended()
    {
        var entries = MarkdownKnowledgeBaseService.Parse(File.ReadAllText(RealKnowledgeBase()), "seguridad-incidentes.md");

        Assert.Equal(5, entries.Count);
        foreach (var entry in entries)
        {
            Assert.Equal(new[] { "A", "B", "C", "D", "E" }, entry.Solutions.Select(s => s.Id).ToArray());
            Assert.Single(entry.Solutions, s => s.Recommended);
            Assert.Equal(5, entry.Solutions.Select(s => s.Title).Distinct().Count());
            Assert.All(entry.Solutions, s => Assert.False(string.IsNullOrWhiteSpace(s.Summary)));
        }
    }

    [Fact]
    public void A_solution_stops_at_the_next_section_and_the_recommended_marker_is_removed_from_the_title()
    {
        var md = "## SEG-009 · Prueba\n\n**Palabras clave:** x\n\n### Solución A · Estándar (recomendada)\n**Cuándo conviene:** siempre\n1. Paso uno\n\n"
               + "### Solución B · Otra\n**Cuándo conviene:** a veces\n1. Paso dos\n\n### Medidas posteriores\nNada.\n";

        var entry = Assert.Single(MarkdownKnowledgeBaseService.Parse(md, "x.md"));

        Assert.Equal(2, entry.Solutions.Count);
        Assert.Equal("Estándar", entry.Solutions[0].Title);
        Assert.True(entry.Solutions[0].Recommended);
        Assert.Equal("siempre", entry.Solutions[0].Summary);
        Assert.DoesNotContain("Medidas posteriores", entry.Solutions[1].Body);
    }
}

public class KnowledgeChoiceFlowTests
{
    private const string Body =
        "## SEG-003 · Phishing\n### Acciones inmediatas\n1. Cambiar la contraseña.\n2. Ticket SEG-PHISH.\n\n" +
        "### Solución A · Contención estándar (recomendada)\n**Cuándo conviene:** siempre\n1. Cambiar la contraseña y abrir el ticket SEG-PHISH en 30 minutos.\n\n" +
        "### Solución B · Bloqueo temporal\n**Cuándo conviene:** sin otro dispositivo\n1. Seguridad bloquea la cuenta 2 horas.\n";

    private const string Research = "CÓDIGO: SEG-003\nCONFIANZA: alta\nHECHOS DEL USUARIO:\n- Hizo clic en un enlace falso\nPLAZO DE REPORTE: 30 minutos";

    private static OfficeHarness Create(bool offerChoice = true)
    {
        var h = new OfficeHarness(configureKnowledge: k => k.OfferSolutionChoice = offerChoice);
        h.Configuration.Definitions.AddRange(new[]
        {
            FakeAgentConfiguration.Make("kb-researcher", "Investigador KB", AgentStatus.Reading) with { WorkflowOnly = true },
            FakeAgentConfiguration.Make("incident-responder", "Respondedor", AgentStatus.Working) with { WorkflowOnly = true },
            FakeAgentConfiguration.Make("compliance-auditor", "Auditor", AgentStatus.Reviewing) with { WorkflowOnly = true }
        });
        h.Registry.Load(h.Configuration.Definitions);
        h.Knowledge.Entries.Add(new KnowledgeEntry { Code = "SEG-003", Title = "Phishing", Severity = "Crítica", Keywords = new[] { "phishing", "enlace falso" }, Body = Body, SourceFile = "kb.md" });
        h.Knowledge.Entries.Add(new KnowledgeEntry { Code = "SEG-005", Title = "Robo", Keywords = new[] { "robaron" }, Body = "## SEG-005 · Robo\n### Acciones\n1. Llamar.", SourceFile = "kb.md" });
        h.Anthropic.Handler = (request, _) => Task.FromResult(FakeAnthropicService.Text(request.Purpose switch
        {
            "agent:kb-researcher" => Research,
            "agent:incident-responder" => "BORRADOR: sigue la solución elegida con el ticket SEG-PHISH",
            "agent:word-writer" => "# Respuesta\n## Resumen\nTexto formateado con SEG-PHISH",
            "agent:compliance-auditor" => "VEREDICTO: APROBADO\nHALLAZGOS: ninguno",
            _ => "otro"
        }));
        return h;
    }

    private const string Case = "Caí en un phishing, hice clic en un enlace falso";

    private static async Task<(ChatResponseDto First, Guid Conversation)> StartAsync(OfficeHarness h)
    {
        var first = await h.Orchestration.HandleMessageAsync(new ChatRequestDto(Case), CancellationToken.None);
        return (first, first.ConversationId);
    }

    #region Fase 1: presentar opciones

    [Fact]
    public async Task The_case_is_investigated_and_the_options_are_offered_without_delivering_documents()
    {
        var h = Create();

        var (first, _) = await StartAsync(h);

        Assert.Equal(new[] { "kb-researcher" }, first.Tasks.Select(t => t.AgentId).ToArray());          // solo investiga
        Assert.Empty(h.Word.WordStyles);                                                                // todavía no hay entregable
        Assert.Empty(h.Word.PdfExports);
        Assert.NotNull(first.Choice);
        Assert.Equal(new[] { "A", "B" }, first.Choice!.Options.Select(o => o.Id).ToArray());
        Assert.Equal("A", first.Choice.RecommendedId);
        Assert.Contains("SEG-003", first.Reply);
        Assert.Contains("Hizo clic en un enlace falso", first.Reply);                                   // lo entendido del caso
        Assert.Contains("B · Bloqueo temporal", first.Reply);
        Assert.DoesNotContain(h.Anthropic.Requests, r => r.Purpose == "agent:incident-responder");
    }

    [Fact]
    public async Task With_the_flag_off_the_full_flow_runs_in_one_go()
    {
        var h = Create(offerChoice: false);

        var (first, _) = await StartAsync(h);

        Assert.Null(first.Choice);
        Assert.Equal(4, first.Tasks.Count);
        Assert.Equal(2, first.Documents!.Count);
    }

    #endregion

    #region Fase 2: elegir

    [Theory]
    [InlineData("B")]
    [InlineData("opción b")]
    [InlineData("Solución B.")]
    public async Task Choosing_by_letter_delivers_only_that_solution(string answer)
    {
        var h = Create();
        var (_, conversation) = await StartAsync(h);

        var second = await h.Orchestration.HandleMessageAsync(new ChatRequestDto(answer, conversation), CancellationToken.None);

        Assert.Null(second.Choice);
        Assert.Equal(new[] { "incident-responder", "word-writer", "compliance-auditor" }, second.Tasks.OrderBy(t => t.Key).Select(t => t.AgentId).ToArray());
        var responder = h.Anthropic.Requests.Single(r => r.Purpose == "agent:incident-responder").Messages[0].Content;
        Assert.Contains("Bloqueo temporal", responder);
        Assert.DoesNotContain("Contención estándar (recomendada)\n**Cuándo conviene:** siempre", responder);   // la A no se entrega
        Assert.Contains("Hizo clic en un enlace falso", responder);                                           // el resumen del investigador viaja
        Assert.Equal(2, second.Documents!.Count);                                                             // ahora sí Word y PDF
    }

    [Fact]
    public async Task A_button_choice_id_is_used_exactly()
    {
        var h = Create();
        var (_, conversation) = await StartAsync(h);

        var second = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Opción A · Contención estándar", conversation, null, "A"), CancellationToken.None);

        Assert.All(second.Tasks, t => Assert.Equal(TaskStatus.Completed, t.Status));
        Assert.Contains("Solución elegida por la persona: A", h.Anthropic.Requests.Single(r => r.Purpose == "agent:incident-responder").Messages[0].Content);
    }

    [Fact]
    public async Task The_recommended_option_can_be_chosen_in_words()
    {
        var h = Create();
        var (_, conversation) = await StartAsync(h);

        await h.Orchestration.HandleMessageAsync(new ChatRequestDto("la recomendada", conversation), CancellationToken.None);

        Assert.Contains("Solución elegida por la persona: A", h.Anthropic.Requests.Single(r => r.Purpose == "agent:incident-responder").Messages[0].Content);
    }

    [Fact]
    public async Task A_custom_solution_is_reviewed_against_the_base_by_the_auditor_and_its_own_data_is_not_flagged()
    {
        var h = Create();
        var (_, conversation) = await StartAsync(h);

        var second = await h.Orchestration.HandleMessageAsync(
            new ChatRequestDto("Prefiero llamar yo mismo al banco al 555-0199 y cambiar la clave desde mi casa", conversation), CancellationToken.None);

        Assert.Equal(3, second.Tasks.Count);
        Assert.Contains("propia de la persona", second.Tasks.Single(t => t.AgentId == "compliance-auditor").Description);
        Assert.Contains("555-0199", h.Anthropic.Requests.Single(r => r.Purpose == "agent:incident-responder").Messages[0].Content);
        Assert.DoesNotContain("Datos sin respaldo", second.Reply);
    }

    [Fact]
    public async Task Cancel_closes_the_case_and_clears_the_pending_choice()
    {
        var h = Create();
        var (_, conversation) = await StartAsync(h);

        var second = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Cancelar", conversation, null, "cancel"), CancellationToken.None);

        Assert.Contains("cierro este caso", second.Reply);
        Assert.Empty(second.Tasks);
        Assert.Null(h.Pending.Get(conversation));
    }

    [Fact]
    public async Task An_unclear_short_answer_asks_again_and_keeps_the_case_pending()
    {
        var h = Create();
        var (_, conversation) = await StartAsync(h);

        var second = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("mmm", conversation), CancellationToken.None);

        Assert.Contains("No entendí", second.Reply);
        Assert.NotNull(h.Pending.Get(conversation));

        var third = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("B", conversation), CancellationToken.None);      // aún se puede elegir
        Assert.Equal(3, third.Tasks.Count);
    }

    [Fact]
    public async Task A_message_about_another_situation_starts_a_new_case_instead_of_answering_the_pending_one()
    {
        var h = Create();
        var (_, conversation) = await StartAsync(h);

        var second = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Me robaron el celular de la empresa", conversation), CancellationToken.None);

        Assert.Contains("SEG-005", second.Reply);
        Assert.Null(h.Pending.Get(conversation));                       // SEG-005 no tiene opciones: se entregó directo
        Assert.Contains(second.Tasks, t => t.AgentId == "kb-researcher");
    }

    #endregion
}

/// <summary>RF-014: si el Lector PDF falla, se reintenta una vez con Claude; el Investigador nunca recibe el documento directamente.</summary>
public class ReaderFallbackTests
{
    private const string Body =
        "## SEG-003 · Phishing\n### Acciones inmediatas\n1. Cambiar la contraseña.\n\n" +
        "### Solución A · Estándar (recomendada)\n**Cuándo conviene:** siempre\n1. Paso A.\n\n### Solución B · Otra\n**Cuándo conviene:** a veces\n1. Paso B.\n";

    private static (OfficeHarness H, OfficeDocument Doc) Create(bool retry, Func<bool, string?> readerBehavior)
    {
        var h = new OfficeHarness(configureKnowledge: k => k.RetryReaderWithClaude = retry);
        h.Configuration.Definitions.AddRange(new[]
        {
            FakeAgentConfiguration.Make("kb-researcher", "Investigador KB", AgentStatus.Reading) with { WorkflowOnly = true },
            FakeAgentConfiguration.Make("incident-responder", "Respondedor", AgentStatus.Working) with { WorkflowOnly = true },
            FakeAgentConfiguration.Make("compliance-auditor", "Auditor", AgentStatus.Reviewing) with { WorkflowOnly = true }
        });
        h.Registry.Load(h.Configuration.Definitions);
        h.Knowledge.Entries.Add(new KnowledgeEntry { Code = "SEG-003", Title = "Phishing", Keywords = new[] { "phishing" }, Body = Body, SourceFile = "kb.md" });
        var doc = new OfficeDocument { FileName = "caso.pdf", PageCount = 1, Text = "Hice clic en un enlace de phishing." };
        h.Documents.Add(doc);

        h.Anthropic.Handler = (request, _) =>
        {
            var isRetry = request.Purpose == "agent:pdf-reader:claude-retry";
            if (request.Purpose is "agent:pdf-reader" or "agent:pdf-reader:claude-retry")
            {
                var text = readerBehavior(isRetry);
                return text is null ? throw new LlmException("proveedores gratuitos agotados") : Task.FromResult(FakeAnthropicService.Text(text));
            }
            return Task.FromResult(FakeAnthropicService.Text(request.Purpose == "agent:kb-researcher"
                ? "CÓDIGO: SEG-003\nHECHOS DEL USUARIO:\n- Hizo clic en un enlace" : "ok"));
        };
        return (h, doc);
    }

    [Fact]
    public async Task When_the_free_apis_fail_the_reader_retries_once_with_Claude_and_the_flow_continues()
    {
        var (h, doc) = Create(retry: true, isRetry => isRetry ? "RESUMEN DEL LECTOR: hizo clic en un enlace" : null);

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Resuelve este caso", null, new[] { doc.Id }), CancellationToken.None);

        Assert.Equal(TaskStatus.Completed, result.Tasks.Single(t => t.AgentId == "pdf-reader").Status);
        var retry = Assert.Single(h.Anthropic.Requests, r => r.Purpose == "agent:pdf-reader:claude-retry");
        Assert.Equal("claude", retry.ProviderPreference);
        var researcher = h.Anthropic.Requests.Single(r => r.Purpose == "agent:kb-researcher").Messages[0].Content;
        Assert.Contains("RESUMEN DEL LECTOR", researcher);                       // el Investigador recibe el resumen...
        Assert.DoesNotContain("<documento", researcher);                          // ...nunca el documento
        Assert.NotNull(result.Choice);                                            // el flujo siguió hasta las opciones
    }

    [Fact]
    public async Task When_Claude_also_fails_the_flow_stops_with_the_usual_message()
    {
        var (h, doc) = Create(retry: true, _ => null);

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Resuelve este caso", null, new[] { doc.Id }), CancellationToken.None);

        Assert.Equal(TaskStatus.Failed, result.Tasks.Single(t => t.AgentId == "pdf-reader").Status);
        Assert.Equal(2, h.Anthropic.Requests.Count(r => r.Purpose is "agent:pdf-reader" or "agent:pdf-reader:claude-retry"));   // un intento + UN reintento
        Assert.Contains("No pude completar", result.Reply);
        Assert.DoesNotContain(h.Anthropic.Requests, r => r.Purpose == "agent:kb-researcher");
    }

    [Fact]
    public async Task With_the_flag_off_there_is_no_retry()
    {
        var (h, doc) = Create(retry: false, _ => null);

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Resuelve este caso", null, new[] { doc.Id }), CancellationToken.None);

        Assert.Equal(TaskStatus.Failed, result.Tasks.Single(t => t.AgentId == "pdf-reader").Status);
        Assert.Single(h.Anthropic.Requests, r => r.Purpose is "agent:pdf-reader" or "agent:pdf-reader:claude-retry");
    }
}
