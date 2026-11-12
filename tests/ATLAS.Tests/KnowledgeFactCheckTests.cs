using ATLAS.Application.DTOs;
using ATLAS.Application.Interfaces;
using ATLAS.Application.Orchestration;
using ATLAS.Domain.Entities;
using ATLAS.Domain.Enums;
using ATLAS.Tests.TestSupport;
using TaskStatus = ATLAS.Domain.Enums.TaskStatus;

namespace ATLAS.Tests;

public class KnowledgeFactCheckTests
{
    private static OfficeHarness Create()
    {
        var h = new OfficeHarness();
        h.Configuration.Definitions.AddRange(new[]
        {
            FakeAgentConfiguration.Make("kb-researcher", "Investigador KB", AgentStatus.Reading) with { WorkflowOnly = true },
            FakeAgentConfiguration.Make("incident-responder", "Respondedor", AgentStatus.Working) with { WorkflowOnly = true },
            FakeAgentConfiguration.Make("compliance-auditor", "Auditor", AgentStatus.Reviewing) with { WorkflowOnly = true }
        });
        h.Registry.Load(h.Configuration.Definitions);
        h.Knowledge.Entries.Add(new KnowledgeEntry
        {
            Code = "SEG-005", Title = "Robo de dispositivo", Keywords = new[] { "robaron" }, SourceFile = "kb.md",
            Body = "## SEG-005 · Robo\n### Plazo de reporte\nMáximo **1 hora**.\n### Acciones\n1. Llamar a la Mesa (ext. 4400) o al 01-800-NEXORA.\n2. Denuncia en **24 horas**.\n3. Reemplazo en **48 horas**.\nCorreo: seguridad@nexora.example"
        });
        return h;
    }

    private static void Script(OfficeHarness h, string formatted, Func<LlmRequest, string>? repair = null)
    {
        h.Anthropic.Handler = (request, _) => Task.FromResult(FakeAnthropicService.Text(request.Purpose switch
        {
            "agent:kb-researcher" => "CÓDIGO: SEG-005\nCONFIANZA: alta",
            "agent:incident-responder" => "borrador",
            "agent:word-writer" => formatted,
            "agent:compliance-auditor" => request.Messages[0].Content.Contains("Hallazgos automáticos")
                ? (repair?.Invoke(request) ?? "VEREDICTO: CORREGIDO\n---TEXTO---\n# Corregido\n## Cuerpo\nTexto Corregido. Llamar a la ext. 4400. Denuncia en 24 horas.")
                : "VEREDICTO: APROBADO\nHALLAZGOS: Sin hallazgos",
            _ => "otro"
        }));
    }

    // ---- fact extraction ----

    [Fact]
    public void Facts_are_extracted_with_hyphen_variants_and_deadlines_normalized_to_minutes()
    {
        var facts = KnowledgeWorkflow.ExtractFacts(
            "Ticket SEG‑CRED y FOR-ACC-02; llame al 01‑800‑NEXORA (ext. 4400) o a seguridad@Nexora.example. Plazo: 2 horas, luego 48 horas y 30 minutos; una hora.");

        Assert.Contains("SEG-CRED", facts.Keys);
        Assert.Contains("FOR-ACC-02", facts.Keys);
        Assert.Contains("01-800-NEXORA", facts.Keys);
        Assert.Contains("ext4400", facts.Keys);
        Assert.Contains("seguridad@nexora.example", facts.Keys);
        Assert.Contains("t:120", facts.Keys);      // 2 horas
        Assert.Contains("t:2880", facts.Keys);     // 48 horas
        Assert.Contains("t:30", facts.Keys);
        Assert.Contains("t:60", facts.Keys);       // "una hora"
    }

    [Fact]
    public void Ordinary_hyphenated_words_dates_and_lowercase_text_are_not_facts()
    {
        var facts = KnowledgeWorkflow.ExtractFacts("Use el Wi-Fi o el auto-bloqueo el 2026-11-05 tras el post-incidente, sin demora.");

        Assert.Empty(facts);
    }

    [Fact]
    public void One_hour_equals_sixty_minutes()
    {
        var a = KnowledgeWorkflow.ExtractFacts("1 hora").Keys.Single();
        var b = KnowledgeWorkflow.ExtractFacts("60 minutos").Keys.Single();

        Assert.Equal(a, b);
    }

    // ---- workflow behavior ----

    [Fact]
    public async Task A_clean_answer_is_delivered_without_repair()
    {
        var h = Create();
        Script(h, "# Respuesta\nLlame a la ext. 4400 (1 hora). Denuncia en 24 horas, reemplazo en 48 horas. SEG-005. seguridad@nexora.example");

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("me robaron el equipo"), CancellationToken.None);

        Assert.Equal(4, result.Tasks.Count);                                   // no repair task
        Assert.DoesNotContain("corregidos automáticamente", result.Reply);
        Assert.DoesNotContain("sin respaldo", result.Reply);
    }

    [Fact]
    public async Task Invented_tickets_phones_and_deadlines_trigger_one_automatic_repair_by_the_auditor()
    {
        var h = Create();
        Script(h, "# Respuesta\nTicket SEG-005-001 y formulario DEN-2026. Llame al 800-123-4567 ext. 9999. Denuncia en 12 horas.");

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("me robaron el equipo"), CancellationToken.None);

        var repair = Assert.Single(result.Tasks, t => t.Key == "t5");
        Assert.Equal("compliance-auditor", repair.AgentId);
        Assert.Equal(TaskStatus.Completed, repair.Status);
        var prompt = h.Anthropic.Requests.Single(r => r.Messages[0].Content.Contains("Hallazgos automáticos")).Messages[0].Content;
        Assert.Contains("SEG-005-001", prompt);                                // the findings are given explicitly...
        Assert.Contains("DEN-2026", prompt);
        Assert.Contains("ext. 9999", prompt);
        Assert.Contains("12 horas", prompt);
        Assert.Contains("Texto Corregido", result.Reply);                        // ...and the corrected text is what the user gets
        Assert.Contains("🔧 datos corregidos automáticamente", result.Reply);
        Assert.DoesNotContain("sin respaldo", result.Reply);
    }

    [Fact]
    public async Task If_the_repair_does_not_fix_it_the_reply_warns_instead_of_hiding_the_problem()
    {
        var h = Create();
        Script(h, "# Respuesta\nTicket TI-REQ-SEG-005", repair: _ => "VEREDICTO: CORREGIDO\n---TEXTO---\n# Aún mal\nTicket TI-REQ-SEG-005");

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("me robaron el equipo"), CancellationToken.None);

        Assert.Equal(5, result.Tasks.Count);                                   // one repair only, never a loop
        Assert.Contains("⚠️ **Datos sin respaldo en la base de conocimiento", result.Reply);
        Assert.Contains("TI-REQ-SEG-005", result.Reply);
    }

    [Fact]
    public async Task If_the_repair_task_fails_the_original_text_is_kept_with_the_warning()
    {
        var h = Create();
        h.Anthropic.Handler = (request, _) =>
        {
            if (request.Purpose == "agent:compliance-auditor" && request.Messages[0].Content.Contains("Hallazgos automáticos"))
                throw new LlmException("auditor caído");
            return Task.FromResult(FakeAnthropicService.Text(request.Purpose switch
            {
                "agent:kb-researcher" => "CÓDIGO: SEG-005",
                "agent:word-writer" => "# Original\n## Cuerpo\nTexto Original. Ticket XX-999",
                "agent:compliance-auditor" => "VEREDICTO: APROBADO",
                _ => "x"
            }));
        };

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("me robaron el equipo"), CancellationToken.None);

        Assert.Contains("Texto Original", result.Reply);
        Assert.Contains("XX-999", result.Reply);
        Assert.Contains("⚠️", result.Reply);
    }

    [Fact]
    public async Task The_agents_receive_the_whitelist_of_allowed_identifiers_and_the_current_date()
    {
        var h = Create();
        Script(h, "# Respuesta\nSEG-005");

        await h.Orchestration.HandleMessageAsync(new ChatRequestDto("me robaron el equipo"), CancellationToken.None);

        string PromptOf(string agent) => h.Anthropic.Requests.First(r => r.Purpose == "agent:" + agent).Messages[0].Content;
        Assert.Contains("PERMITIDOS para SEG-005", PromptOf("incident-responder"));
        Assert.Contains("01-800-NEXORA", PromptOf("incident-responder"));
        Assert.Contains("NO inventes", PromptOf("word-writer"));
        Assert.Contains(DateTime.Now.Year.ToString(), PromptOf("word-writer"));      // current date for the format
    }
}
