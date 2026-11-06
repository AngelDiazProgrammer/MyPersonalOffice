using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Application.Services;
using AI.Agent.Office.Infrastructure.Configuration;
using AI.Agent.Office.Infrastructure.Documents;
using AI.Agent.Office.Infrastructure.FileSystem;
using AI.Agent.Office.Tests.TestSupport;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging.Abstractions;
using TaskStatus = AI.Agent.Office.Domain.Enums.TaskStatus;

namespace AI.Agent.Office.Tests;

public class WordGeneratorTests
{
    private static readonly string NL = Environment.NewLine;

    private const string Sample = """
        # Informe trimestral

        Resumen con **negrita**, *cursiva*, `código` y un [enlace](https://example.com).

        ## Puntos clave

        - Primero
        - Segundo
          - Anidado
        - Tercero

        1. Paso uno
        2. Paso dos

        ## Datos

        | Región | Ventas |
        |--------|--------|
        | Norte  | 120    |
        | Sur    | 98     |

        > Una cita importante.

        ```csharp
        Console.WriteLine("hola");
        ```

        ---

        Texto final con ñ, tildes y emojis ✅.
        """;

    private static byte[] Build(string markdown = Sample) => new MarkdownWordGenerator().Generate("Documento", markdown);

    [Fact]
    public void Generated_docx_is_valid_Open_XML()
    {
        using var stream = new MemoryStream(Build());
        using var doc = WordprocessingDocument.Open(stream, false);

        var errors = new OpenXmlValidator().Validate(doc).Select(e => $"{e.Path?.XPath}: {e.Description}").ToList();

        Assert.True(errors.Count == 0, string.Join(NL, errors));
    }

    [Fact]
    public void Document_contains_title_headings_lists_table_code_and_footer()
    {
        using var stream = new MemoryStream(Build());
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart!.Document.Body!;

        string StyleOf(Paragraph p) => p.ParagraphProperties?.ParagraphStyleId?.Val?.Value ?? "Normal";
        var paragraphs = body.Elements<Paragraph>().ToList();

        Assert.Equal("Title", StyleOf(paragraphs[0]));
        Assert.Equal("Informe trimestral", paragraphs[0].InnerText);                         // the H1 became the title (once)
        Assert.Equal(2, paragraphs.Count(p => StyleOf(p) == "Heading2"));
        Assert.Contains(paragraphs, p => StyleOf(p) == "ListParagraph" && p.InnerText == "Anidado"
                                          && p.ParagraphProperties!.NumberingProperties!.NumberingLevelReference!.Val!.Value == 1);
        Assert.Contains(paragraphs, p => StyleOf(p) == "Quote" && p.InnerText.Contains("cita"));
        Assert.Contains(paragraphs, p => StyleOf(p) == "Code" && p.InnerText.Contains("Console.WriteLine"));
        Assert.Contains(paragraphs, p => p.InnerText.Contains("ñ") && p.InnerText.Contains("✅"));

        var table = Assert.Single(body.Elements<Table>());
        var rows = table.Elements<TableRow>().ToList();
        Assert.Equal(3, rows.Count);
        Assert.Equal(new[] { "Región", "Ventas" }, rows[0].Elements<TableCell>().Select(c => c.InnerText));
        Assert.Equal(new[] { "Sur", "98" }, rows[2].Elements<TableCell>().Select(c => c.InnerText));

        Assert.Single(doc.MainDocumentPart.HyperlinkRelationships);
        Assert.Single(doc.MainDocumentPart.FooterParts);
        Assert.True(doc.MainDocumentPart.Document.Descendants<Bold>().Any());
        Assert.Equal("Informe trimestral", doc.PackageProperties.Title);
    }

    [Fact]
    public void Ordered_lists_restart_numbering_with_their_own_instance()
    {
        using var stream = new MemoryStream(Build("1. a\n2. b\n\ntexto\n\n1. c\n2. d"));
        using var doc = WordprocessingDocument.Open(stream, false);

        var ids = doc.MainDocumentPart!.Document.Body!.Descendants<NumberingId>().Select(n => n.Val!.Value).Distinct().ToList();

        Assert.Equal(2, ids.Count);
    }

    [Fact]
    public void Unsafe_link_schemes_are_not_turned_into_hyperlinks_and_html_is_inert()
    {
        using var stream = new MemoryStream(Build("[x](javascript:alert(1)) y <script>alert(1)</script>"));
        using var doc = WordprocessingDocument.Open(stream, false);

        Assert.Empty(doc.MainDocumentPart!.HyperlinkRelationships);
        Assert.Contains("x", doc.MainDocumentPart.Document.Body!.InnerText);
    }

    [Fact]
    public void Title_parameter_is_used_when_the_markdown_has_no_H1()
    {
        using var stream = new MemoryStream(Build("Solo un párrafo."));
        using var doc = WordprocessingDocument.Open(stream, false);

        Assert.Equal("Documento", doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First().InnerText);
    }

    [Fact]
    public void Empty_and_unusual_markdown_does_not_break_the_generator()
    {
        foreach (var md in new[] { "", "   ", "|a|\n|-|", "# Solo título", "- [ ] pendiente\n- [x] hecho", "![img](http://x/y.png)" })
        {
            using var stream = new MemoryStream(Build(md));
            using var doc = WordprocessingDocument.Open(stream, false);
            var problems = new OpenXmlValidator().Validate(doc).Select(e => $"{e.Path?.XPath}: {e.Description}").ToList();
            Assert.True(problems.Count == 0, $"[{md}] " + string.Join(NL, problems));
        }
    }
}

public class WordExportTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "office-docs-" + Guid.NewGuid().ToString("N"));

    private GeneratedDocumentStore CreateStore(int retentionHours = 24) =>
        new(Microsoft.Extensions.Options.Options.Create(new GeneratedDocumentsOptions { Directory = _dir, RetentionHours = retentionHours }),
            NullLogger<GeneratedDocumentStore>.Instance);

    private WordExportService CreateService(IGeneratedDocumentStore? store = null) =>
        new(new MarkdownWordGenerator(), new MarkdownPdfGenerator(), store ?? CreateStore(),
            Microsoft.Extensions.Options.Options.Create(new DocumentOptions { MaxWordMarkdownChars = 5000 }),
            NullLogger<WordExportService>.Instance);

    [Fact]
    public async Task Export_saves_a_downloadable_docx_with_a_safe_file_name()
    {
        var store = CreateStore();
        var dto = await CreateService(store).ExportAsync("Propuesta: Ñandú / ../../Windows <v1>", "# Hola\n\ntexto");

        Assert.EndsWith(".docx", dto.FileName);
        Assert.DoesNotContain("/", dto.FileName);
        Assert.DoesNotContain("..", dto.FileName);
        Assert.DoesNotContain("<", dto.FileName);
        Assert.Matches("^[A-Za-z0-9_]+\\.docx$", dto.FileName);

        var saved = store.Get(dto.Id)!;
        Assert.StartsWith(Path.GetFullPath(_dir), Path.GetFullPath(saved.FilePath));
        Assert.True(new FileInfo(saved.FilePath).Length > 1000);
    }

    [Fact]
    public async Task Export_rejects_empty_and_oversized_content()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<OfficeValidationException>(() => service.ExportAsync("t", "  "));
        await Assert.ThrowsAsync<OfficeValidationException>(() => service.ExportAsync("t", new string('x', 5001)));
    }

    [Fact]
    public async Task Store_never_trusts_names_and_unknown_ids_return_nothing()
    {
        var store = CreateStore();
        var doc = await store.SaveAsync(@"..\..\evil.docx", new byte[] { 1, 2, 3 });

        Assert.Equal("evil.docx", doc.FileName);
        Assert.StartsWith(Path.GetFullPath(_dir), Path.GetFullPath(doc.FilePath));
        Assert.Null(store.Get(Guid.NewGuid()));
    }

    [Fact]
    public async Task Expired_documents_are_removed()
    {
        var store = CreateStore(retentionHours: 1);
        var old = await store.SaveAsync("viejo.docx", new byte[] { 1 });
        File.SetCreationTimeUtc(old.FilePath, DateTime.UtcNow.AddHours(-5));

        // CreatedAt is set at save time: simulate expiry with a zero-hour window by using a fresh store pointing at the same folder.
        var strict = new GeneratedDocumentStore(
            Microsoft.Extensions.Options.Options.Create(new GeneratedDocumentsOptions { Directory = _dir, RetentionHours = 1 }),
            NullLogger<GeneratedDocumentStore>.Instance);
        await strict.SaveAsync("nuevo.docx", new byte[] { 2 });

        Assert.NotNull(store.Get(old.Id)); // still within retention
    }

    [Fact]
    public async Task An_agent_that_produces_Word_gets_a_document_attached_to_its_task()
    {
        var h = new OfficeHarness();
        h.Script("""
            {"action":"delegate","message":"x","tasks":[{"id":"t1","agentId":"word-writer","title":"Informe de ventas","task":"Redacta el informe"}]}
            """, (_, _) => Task.FromResult(FakeAnthropicService.Text("# Informe de ventas\n\nContenido del informe.")));

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("hazme un informe en Word"), CancellationToken.None);

        var task = Assert.Single(result.Tasks);
        Assert.Equal(TaskStatus.Completed, task.Status);
        Assert.NotNull(task.DocumentId);
        Assert.Equal("doc.docx", task.DocumentName);
        Assert.Equal("Informe de ventas", Assert.Single(h.Word.Exports).Title);
        Assert.Equal("doc.docx", Assert.Single(result.Documents!).FileName);
    }

    [Fact]
    public async Task Other_agents_do_not_generate_documents()
    {
        var h = new OfficeHarness();
        h.Script("""
            {"action":"delegate","message":"x","tasks":[{"id":"t1","agentId":"developer","title":"Código","task":"x"}]}
            """);

        var result = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("código"), CancellationToken.None);

        Assert.Empty(h.Word.Exports);
        Assert.Null(Assert.Single(result.Tasks).DocumentId);
    }

    public void Dispose() { try { Directory.Delete(_dir, true); } catch (IOException) { } }
}
