using System.Text;
using ATLAS.Application.Interfaces;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ATLAS.Infrastructure.Documents;

/// <summary>
/// Convierte Markdown en un PDF real con QuestPDF (licencia Community: gratis para particulares y
/// empresas con menos de 1 M USD de ingresos). El estilo APA replica el del Word: carta, Times New
/// Roman 12, doble espacio, portada, número de página arriba a la derecha y referencias en página
/// aparte (la sangría francesa solo existe en el Word).
/// </summary>
public sealed class MarkdownPdfGenerator : IPdfDocumentGenerator
{
    #region Tipos auxiliares

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    /// <summary>Formato de texto en línea que se acumula al recorrer el Markdown.</summary>
    private sealed record Fmt(bool Bold = false, bool Italic = false, bool Code = false, bool Strike = false, bool Link = false, bool HardBreaks = false);

    /// <summary>Estado de la generación: banderas del estilo APA (referencias y resumen).</summary>
    private sealed class Context
    {
        public bool Apa;
        public bool InReferences;
        public bool InAbstract;
    }

    #endregion

    #region Generación del documento

    static MarkdownPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.EnableDebugging = false;
        // Las fuentes salen del sistema (Times New Roman o Calibri en Windows, Liberation en Linux); si falta una se usa
        // la siguiente de la lista.
        QuestPDF.Settings.UseSystemFonts = true;
        QuestPDF.Settings.ThrowOnMissingFontFamilies = false;   // fall back to the first available family of the list
    }

    /// <summary>Genera el PDF. Con el estilo APA arma portada, número de página y referencias en página aparte.</summary>
    public byte[] Generate(string title, string markdown, DocumentStyle style = DocumentStyle.Default)
    {
        var apa = style == DocumentStyle.Apa;
        var blocks = Markdown.Parse(markdown ?? string.Empty, Pipeline).ToList();

        string? documentTitle = null;
        if (blocks.FirstOrDefault() is HeadingBlock { Level: 1 } first)
        {
            documentTitle = PlainText(first.Inline);
            blocks.RemoveAt(0);
        }
        documentTitle = string.IsNullOrWhiteSpace(documentTitle) ? title : documentTitle;

        var ctx = new Context { Apa = apa };
        var font = apa ? new[] { "Times New Roman", "Liberation Serif", "Times" } : new[] { "Calibri", "Carlito", "Arial", "Liberation Sans" };

        // Líneas de la portada APA: los párrafos que siguen al título (autor, fecha...).
        var titleLines = new List<ParagraphBlock>();
        if (apa)
        {
            while (blocks.Count > 0 && blocks[0] is ParagraphBlock meta && titleLines.Count < 6)
            {
                titleLines.Add(meta);
                blocks.RemoveAt(0);
            }
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                if (apa)
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Inch);
                }
                else
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                }

                page.DefaultTextStyle(x => x.FontFamily(font).FontSize(apa ? 12 : 11).LineHeight(apa ? 2f : 1.3f));

                if (apa)
                {
                    page.Header().AlignRight().Text(t => t.CurrentPageNumber());
                }
                else
                {
                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Medium));
                        t.Span("ATLAS · Página ");
                        t.CurrentPageNumber();
                    });
                }

                page.Content().Column(col =>
                {
                    if (apa)
                    {
                        col.Item().PaddingTop(144).AlignCenter().Text(documentTitle!).Bold();
                        col.Item().Height(24);
                        if (titleLines.Count == 0)
                            col.Item().AlignCenter().Text($"{DateTime.Now:d 'de' MMMM 'de' yyyy}");
                        foreach (var line in titleLines)
                            col.Item().AlignCenter().Text(t => AddInlines(t, line.Inline, new Fmt(HardBreaks: true)));
                        col.Item().PageBreak();
                        col.Item().AlignCenter().Text(documentTitle!).Bold();
                    }
                    else
                    {
                        col.Item().PaddingBottom(4).Text(documentTitle!).Bold().FontSize(24).FontColor("#1F3864");
                        col.Item().PaddingBottom(12).Text($"Generado por ATLAS · {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(10).FontColor(Colors.Grey.Medium);
                    }

                    foreach (var block in blocks) Render(col, block, ctx, 0, false);
                });
            });
        });

        document.WithMetadata(new DocumentMetadata { Title = documentTitle, Creator = "ATLAS", CreationDate = DateTimeOffset.Now });
        return document.GeneratePdf();
    }

    #endregion

    #region Bloques (títulos, párrafos, listas, tablas)


    /// <summary>Convierte un bloque de Markdown en elementos del PDF.</summary>
    private void Render(ColumnDescriptor col, Block block, Context ctx, int level, bool quote)
    {
        switch (block)
        {
            case HeadingBlock heading when ctx.Apa:
            {
                var text = PlainText(heading.Inline);
                var isRefs = IsReferencesTitle(text);
                var isAbstract = text.Equals("Resumen", StringComparison.OrdinalIgnoreCase) || text.Equals("Abstract", StringComparison.OrdinalIgnoreCase);
                ctx.InReferences = isRefs;
                ctx.InAbstract = isAbstract;
                if (isRefs) col.Item().PageBreak();

                var apaLevel = Math.Clamp(heading.Level - 1, 1, 3);
                var fmt = new Fmt(Bold: true, Italic: apaLevel >= 3);
                var item = col.Item();
                if (apaLevel == 1) item = item.AlignCenter();
                item.Text(t => AddInlines(t, heading.Inline, fmt));
                break;
            }

            case HeadingBlock heading:
            {
                var size = heading.Level switch { 1 => 18f, 2 => 15f, 3 => 13f, _ => 11.5f };
                col.Item().PaddingTop(10).PaddingBottom(3).Text(t =>
                {
                    t.DefaultTextStyle(x => x.FontSize(size).FontColor(heading.Level <= 2 ? "#1F3864" : "#2F5496"));
                    AddInlines(t, heading.Inline, new Fmt(Bold: true));
                });
                break;
            }

            case ParagraphBlock paragraph:
            {
                if (quote)
                {
                    col.Item().Text(t => { t.DefaultTextStyle(x => x.Italic().FontColor(Colors.Grey.Darken2)); AddInlines(t, paragraph.Inline, new Fmt()); });
                    break;
                }

                var first = PlainText(paragraph.Inline);
                var item = col.Item();
                if (ctx.Apa && ctx.InReferences)
                {
                    // QuestPDF no puede aplicar sangría francesa (sangrar todas las líneas menos la primera), por eso las
                    // referencias van alineadas a la izquierda; el Word sí la tiene.
                    item.Text(t => AddInlines(t, paragraph.Inline, new Fmt()));
                }
                else
                {
                    var indent = ctx.Apa && (!ctx.InAbstract || first.StartsWith("Palabras clave", StringComparison.OrdinalIgnoreCase) || first.StartsWith("Keywords", StringComparison.OrdinalIgnoreCase));
                    item.PaddingBottom(ctx.Apa ? 0 : 6).Text(t =>
                    {
                        if (indent) t.ParagraphFirstLineIndentation(36);
                        AddInlines(t, paragraph.Inline, new Fmt());
                    });
                }
                break;
            }

            case ListBlock list:
                RenderList(col, list, ctx, level);
                break;

            case CodeBlock code:
            {
                var sb = new StringBuilder();
                for (var i = 0; i < code.Lines.Count; i++) sb.AppendLine(code.Lines.Lines[i].ToString());
                col.Item().PaddingVertical(4).Background(Colors.Grey.Lighten3).Padding(6)
                    .Text(sb.ToString().TrimEnd()).FontFamily("Consolas", "Courier New", "Liberation Mono").FontSize(9).LineHeight(1.2f);
                break;
            }

            case QuoteBlock q:
                col.Item().PaddingLeft(ctx.Apa ? 36 : 4).BorderLeft(ctx.Apa ? 0 : 2).BorderColor("#A78BFA").PaddingLeft(ctx.Apa ? 0 : 10).Column(inner =>
                {
                    foreach (var child in q) Render(inner, child, ctx, level, true);
                });
                break;

            case ThematicBreakBlock:
                col.Item().PaddingVertical(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                break;

            case Markdig.Extensions.Tables.Table table:
                RenderTable(col, table, ctx);
                break;

            case HtmlBlock html:
                col.Item().Text(string.Join("\n", html.Lines.Lines.Take(html.Lines.Count).Select(l => l.ToString())));
                break;

            case ContainerBlock container:
                foreach (var child in container) Render(col, child, ctx, level, quote);
                break;
        }
    }

    /// <summary>Convierte una lista con viñetas o numerada, con niveles anidados.</summary>
    private void RenderList(ColumnDescriptor col, ListBlock list, Context ctx, int level)
    {
        var number = 1;
        var bullets = new[] { "•", "◦", "▪", "–" };
        foreach (var item in list.OfType<ListItemBlock>())
        {
            var marker = list.IsOrdered ? $"{number++}." : bullets[Math.Min(level, bullets.Length - 1)];
            var firstParagraph = true;
            foreach (var sub in item)
            {
                switch (sub)
                {
                    case ParagraphBlock p:
                        var showMarker = firstParagraph;
                        firstParagraph = false;
                        col.Item().PaddingLeft(18 + level * 18).Row(row =>
                        {
                            row.ConstantItem(20).Text(showMarker ? marker : string.Empty);
                            row.RelativeItem().Text(t => AddInlines(t, p.Inline, new Fmt()));
                        });
                        break;
                    case ListBlock nested:
                        RenderList(col, nested, ctx, level + 1);
                        break;
                    default:
                        Render(col, sub, ctx, level + 1, false);
                        break;
                }
            }
        }
    }

    /// <summary>Convierte una tabla de Markdown en una tabla del PDF.</summary>
    private void RenderTable(ColumnDescriptor col, Markdig.Extensions.Tables.Table mdTable, Context ctx)
    {
        var rows = mdTable.OfType<Markdig.Extensions.Tables.TableRow>().ToList();
        var columns = rows.Select(r => r.Count).DefaultIfEmpty(1).Max();

        col.Item().PaddingVertical(6).Table(table =>
        {
            table.ColumnsDefinition(c => { for (var i = 0; i < columns; i++) c.RelativeColumn(); });

            for (var r = 0; r < rows.Count; r++)
            {
                var mdRow = rows[r];
                var cells = mdRow.OfType<Markdig.Extensions.Tables.TableCell>().ToList();
                for (var c = 0; c < columns; c++)
                {
                    var cell = c < cells.Count ? cells[c] : null;
                    var element = table.Cell().Row((uint)(r + 1)).Column((uint)(c + 1))
                        .Border(0.5f).BorderColor(Colors.Grey.Medium)
                        .Background(mdRow.IsHeader ? "#D9E2F3" : Colors.White).Padding(4);

                    element.Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontSize(ctx.Apa ? 11 : 10).LineHeight(1.15f));
                        if (cell is null) return;
                        var firstCellBlock = true;
                        foreach (var cellBlock in cell.OfType<ParagraphBlock>())
                        {
                            if (!firstCellBlock) t.Span("\n");
                            firstCellBlock = false;
                            AddInlines(t, cellBlock.Inline, new Fmt(Bold: mdRow.IsHeader));
                        }
                    });
                }
            }
        });
    }

    #endregion

    #region Texto en línea y utilidades


    /// <summary>Agrega el texto en línea de un párrafo con su formato (negrita, cursiva, código, enlaces).</summary>
    private static void AddInlines(TextDescriptor t, ContainerInline? container, Fmt fmt)
    {
        if (container is null) return;

        foreach (var inline in container)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    Span(t, literal.Content.ToString(), fmt);
                    break;
                case HtmlEntityInline entity:
                    Span(t, entity.Transcoded.ToString(), fmt);
                    break;
                case CodeInline code:
                    Span(t, code.Content, fmt with { Code = true });
                    break;
                case LineBreakInline lineBreak:
                    Span(t, lineBreak.IsHard || fmt.HardBreaks ? "\n" : " ", fmt);
                    break;
                case Markdig.Extensions.TaskLists.TaskList task:
                    Span(t, task.Checked ? "☑ " : "☐ ", fmt);
                    break;
                case EmphasisInline emphasis:
                    var inner = emphasis.DelimiterChar == '~'
                        ? fmt with { Strike = true }
                        : emphasis.DelimiterCount >= 2 ? fmt with { Bold = true } : fmt with { Italic = true };
                    AddInlines(t, emphasis, inner);
                    break;
                case LinkInline link when link.IsImage:
                    Span(t, $"[imagen: {PlainText(link)}]", fmt with { Italic = true });
                    break;
                case LinkInline link:
                    AddInlines(t, link, fmt with { Link = true });
                    break;
                case AutolinkInline auto:
                    Span(t, auto.Url, fmt with { Link = true });
                    break;
                case ContainerInline nested:
                    AddInlines(t, nested, fmt);
                    break;
            }
        }
    }

    /// <summary>Agrega un fragmento de texto aplicando el formato indicado.</summary>
    private static void Span(TextDescriptor t, string text, Fmt fmt)
    {
        if (text.Length == 0) return;
        var span = t.Span(text.Replace("\r\n", "\n"));
        if (fmt.Bold) span = span.Bold();
        if (fmt.Italic) span = span.Italic();
        if (fmt.Strike) span = span.Strikethrough();
        if (fmt.Code) span = span.FontFamily("Consolas", "Courier New", "Liberation Mono").BackgroundColor(Colors.Grey.Lighten3);
        if (fmt.Link) span.FontColor("#0563C1").Underline();
    }

    /// <summary>Texto plano de un contenido en línea.</summary>
    private static string PlainText(ContainerInline? container)
    {
        if (container is null) return string.Empty;
        var sb = new StringBuilder();
        foreach (var inline in container.Descendants())
        {
            switch (inline)
            {
                case LiteralInline literal: sb.Append(literal.Content.ToString()); break;
                case CodeInline code: sb.Append(code.Content); break;
                case LineBreakInline: sb.Append(' '); break;
            }
        }
        return sb.ToString().Trim();
    }

    /// <summary>True si el título corresponde a la sección de referencias o bibliografía.</summary>
    private static bool IsReferencesTitle(string text) =>
        text.StartsWith("Referencias", StringComparison.OrdinalIgnoreCase) || text.StartsWith("References", StringComparison.OrdinalIgnoreCase) ||
        text.StartsWith("Bibliograf", StringComparison.OrdinalIgnoreCase);
    #endregion

}
