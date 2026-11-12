using ATLAS.Application.Interfaces;
using ATLAS.Application.Options;
using ATLAS.Application.Services;
using ATLAS.Domain.Entities;
using ATLAS.Infrastructure.Documents;
using ATLAS.Tests.TestSupport;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;
using TaskStatus = ATLAS.Domain.Enums.TaskStatus;

namespace ATLAS.Tests;

public class ApaDocumentTests
{
    private static readonly string NL = Environment.NewLine;

    private static readonly string Sample = string.Join(NL,
        "# Respuesta al incidente SEG-003: Phishing", "",
        "**Autor:** Mesa de Seguridad", "**Fecha:** 5 de noviembre de 2026", "",
        "## Resumen", "", "Texto del resumen.", "",
        "**Palabras clave:** phishing, correo", "",
        "## Acciones inmediatas", "", "1. No abra el enlace", "2. Reporte el correo", "",
        "### Detalle", "", "Un párrafo de desarrollo con **negrita**.", "",
        "## Referencias", "", "Nexora S.A. (2026). *Base de conocimiento* [Documento interno].");

    private static WordprocessingDocument Open(byte[] bytes) => WordprocessingDocument.Open(new MemoryStream(bytes), false);

    [Fact]
    public void Apa_word_is_valid_and_uses_letter_times_new_roman_double_spacing()
    {
        var bytes = new MarkdownWordGenerator().Generate("x", Sample, DocumentStyle.Apa);
        using var doc = Open(bytes);

        Assert.Empty(new OpenXmlValidator().Validate(doc).Where(e => e.ErrorType == ValidationErrorType.Schema));

        var styles = doc.MainDocumentPart!.StyleDefinitionsPart!.Styles!;
        var defaults = styles.DocDefaults!.RunPropertiesDefault!.RunPropertiesBaseStyle!;
        Assert.Equal("Times New Roman", defaults.RunFonts!.Ascii!.Value);
        Assert.Equal("24", defaults.FontSize!.Val!.Value);
        Assert.Equal("480", styles.DocDefaults!.ParagraphPropertiesDefault!.ParagraphPropertiesBaseStyle!.SpacingBetweenLines!.Line!.Value);

        var section = doc.MainDocumentPart.Document!.Body!.Elements<SectionProperties>().Single();
        Assert.Equal(12240u, section.GetFirstChild<PageSize>()!.Width!.Value);   // US Letter
        Assert.NotNull(section.GetFirstChild<HeaderReference>());               // page number top right
        Assert.Empty(section.Elements<FooterReference>());
    }

    [Fact]
    public void Apa_word_has_title_page_hanging_references_and_apa_headings()
    {
        using var doc = Open(new MarkdownWordGenerator().Generate("x", Sample, DocumentStyle.Apa));
        var body = doc.MainDocumentPart!.Document!.Body!;
        var paragraphs = body.Elements<Paragraph>().ToList();

        // title page: title, author/date lines, then a page break
        Assert.Equal("Title", paragraphs[0].ParagraphProperties!.ParagraphStyleId!.Val!.Value);
        Assert.Contains("Respuesta al incidente SEG-003", paragraphs[0].InnerText);
        Assert.Contains(paragraphs.Take(5), p => p.InnerText.Contains("Mesa de Seguridad") && p.ParagraphProperties?.ParagraphStyleId?.Val == "ApaTitlePage");
        Assert.Contains(paragraphs.Take(5), p => p.Descendants<Break>().Any(b => b.Type?.Value == BreakValues.Page));

        // references: own page, hanging indent style; level-1 headings are the markdown H2
        var refsHeading = paragraphs.Single(p => p.InnerText == "Referencias");
        Assert.NotNull(refsHeading.ParagraphProperties!.PageBreakBefore);
        Assert.Equal("Heading1", refsHeading.ParagraphProperties.ParagraphStyleId!.Val!.Value);
        var reference = paragraphs.Single(p => p.InnerText.StartsWith("Nexora"));
        Assert.Equal("Reference", reference.ParagraphProperties!.ParagraphStyleId!.Val!.Value);

        var styles = doc.MainDocumentPart.StyleDefinitionsPart!.Styles!.Elements<Style>().ToDictionary(s => s.StyleId!.Value);
        Assert.Equal("720", styles["Reference"].StyleParagraphProperties!.Indentation!.Hanging!.Value);
        Assert.Equal("720", styles["ApaBody"].StyleParagraphProperties!.Indentation!.FirstLine!.Value);

        // H3 → APA level 2
        Assert.Equal("Heading2", paragraphs.Single(p => p.InnerText == "Detalle").ParagraphProperties!.ParagraphStyleId!.Val!.Value);
    }

    [Fact]
    public void Default_word_style_is_unchanged()
    {
        using var doc = Open(new MarkdownWordGenerator().Generate("x", Sample));
        var defaults = doc.MainDocumentPart!.StyleDefinitionsPart!.Styles!.DocDefaults!.RunPropertiesDefault!.RunPropertiesBaseStyle!;
        Assert.Equal("Calibri", defaults.RunFonts!.Ascii!.Value);
        Assert.Equal(11906u, doc.MainDocumentPart.Document!.Body!.Elements<SectionProperties>().Single().GetFirstChild<PageSize>()!.Width!.Value); // A4
    }

    [Fact]
    public void Pdf_apa_is_a_real_letter_pdf_with_title_page_and_references_on_their_own_page()
    {
        var bytes = new MarkdownPdfGenerator().Generate("x", Sample, DocumentStyle.Apa);
        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));

        using var pdf = PdfDocument.Open(bytes);
        Assert.True(pdf.NumberOfPages >= 3);                                       // title page + body + references
        Assert.Equal(612, pdf.GetPage(1).Width, 1);                                // US Letter
        Assert.Contains("Respuesta al incidente SEG-003", pdf.GetPage(1).Text);
        Assert.Contains("Mesa de Seguridad", pdf.GetPage(1).Text);
        Assert.DoesNotContain("Texto del resumen", pdf.GetPage(1).Text);
        Assert.Contains("Referencias", pdf.GetPage(pdf.NumberOfPages).Text);
        Assert.Contains("Nexora", pdf.GetPage(pdf.NumberOfPages).Text);
    }

    [Fact]
    public void Pdf_default_handles_lists_tables_code_and_quotes()
    {
        var md = string.Join(NL, "# Informe", "", "- uno", "- dos", "", "| a | b |", "|---|---|", "| 1 | 2 |", "", "> cita", "", "```", "var x = 1;", "```");
        using var pdf = PdfDocument.Open(new MarkdownPdfGenerator().Generate("x", md));
        var text = string.Join(" ", pdf.GetPages().Select(p => p.Text));
        Assert.Contains("Informe", text);
        Assert.Contains("uno", text);
        Assert.Contains("cita", text);
        Assert.Contains("var x = 1;", text);
    }
}

public class WorkspaceTests
{
    private static WorkspaceService Create() => new(Microsoft.Extensions.Options.Options.Create(new WorkspacesOptions
    {
        Items =
        {
            new WorkspaceDefinition { Id = "dev", Name = "Desarrollo", Agents = { "orchestrator", "developer" } },
            new WorkspaceDefinition { Id = "kb", Name = "KB", Agents = { "orchestrator", "pdf-reader", "word-writer" } }
        }
    }));

    [Fact]
    public void Without_workspaces_everything_is_available_and_no_selection_is_required()
    {
        var service = new WorkspaceService(Microsoft.Extensions.Options.Options.Create(new WorkspacesOptions()));
        Assert.False(service.RequiresSelection);
        Assert.True(service.IsAgentActive("anything"));
    }

    [Fact]
    public void Selection_is_required_until_one_is_chosen_and_it_limits_the_agents()
    {
        var service = Create();
        Assert.True(service.RequiresSelection);
        Assert.False(service.IsAgentActive("developer"));

        Assert.False(service.Select("nope"));
        Assert.True(service.Select("KB"));                    // case-insensitive
        Assert.False(service.RequiresSelection);
        Assert.True(service.IsAgentActive("word-writer"));
        Assert.False(service.IsAgentActive("developer"));
        Assert.Equal(2, service.OrderOf("word-writer"));

        service.Clear();
        Assert.True(service.RequiresSelection);
    }

    [Fact]
    public async Task Chat_is_blocked_until_an_office_is_chosen_and_the_planner_only_sees_its_agents()
    {
        var h = new OfficeHarness();
        var workspaces = Create();
        var orchestration = h.CreateOrchestration(workspaces);

        await Assert.ThrowsAsync<OfficeValidationException>(() =>
            orchestration.HandleMessageAsync(new ATLAS.Application.DTOs.ChatRequestDto("hola"), CancellationToken.None));

        workspaces.Select("dev");
        h.Script("""{"action":"respond","message":"hola","tasks":[]}""", (r, _) => Task.FromResult(FakeAnthropicService.Text("ok")));
        await orchestration.HandleMessageAsync(new ATLAS.Application.DTOs.ChatRequestDto("hola"), CancellationToken.None);

        var planning = h.Anthropic.Requests.First(r => r.StructuredOutput is not null);
        var prompt = planning.System + string.Join(" ", planning.Messages.Select(m => m.Content));
        Assert.Contains("developer", prompt);
        Assert.DoesNotContain("pdf-reader", prompt);
        Assert.DoesNotContain("word-writer", prompt);
    }

    [Fact]
    public void Query_service_lists_only_the_active_office_agents_in_seating_order()
    {
        var h = new OfficeHarness();
        var workspaces = Create();
        workspaces.Select("kb");
        var query = new OfficeQueryService(h.Registry, h.Tasks, h.Activity, h.Conversations, new TokenUsageTracker(),
            h.Anthropic, Microsoft.Extensions.Options.Options.Create(new PricingOptions()), workspaces, h.Configuration);

        Assert.Equal(new[] { "orchestrator", "pdf-reader", "word-writer" }, query.GetAgents().Select(a => a.Id).ToArray());
        var offices = query.GetWorkspaces();
        Assert.Equal("kb", offices.ActiveId);
        Assert.False(offices.Required);
        Assert.Equal(2, offices.Available.Count);
    }
}
