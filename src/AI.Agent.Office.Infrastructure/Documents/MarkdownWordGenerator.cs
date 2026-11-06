using System.Text;
using AI.Agent.Office.Application.Interfaces;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace AI.Agent.Office.Infrastructure.Documents;

/// <summary>
/// Convierte Markdown (lo que ya escriben todos los agentes) en un .docx real con Open XML: estilos,
/// títulos, listas numeradas, tablas, bloques de código, citas, hipervínculos y número de página. No se
/// ejecuta nada: el resultado es un documento estático.
/// </summary>
public sealed class MarkdownWordGenerator : IWordDocumentGenerator
{
    #region Tipos auxiliares

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    /// <summary>
    /// Formato de texto en línea que se va acumulando al recorrer el Markdown (negrita, cursiva,
    /// código...).
    /// </summary>
    private sealed record Fmt(bool Bold = false, bool Italic = false, bool Code = false, bool Strike = false, bool Link = false, bool HardBreaks = false);

    /// <summary>Estado de la generación: parte principal del documento, numeración y banderas del estilo APA.</summary>
    private sealed class Context
    {
        public required MainDocumentPart Main { get; init; }
        public required Numbering Numbering { get; init; }
        public int NextNumberId = 10;
        public bool Apa;
        public bool InReferences;
        public bool InAbstract;
    }

    #endregion

    #region Generación del documento

    /// <summary>
    /// Genera el .docx. Con el estilo APA arma portada, encabezado con número de página, niveles de título
    /// y referencias en página aparte.
    /// </summary>
    public byte[] Generate(string title, string markdown, DocumentStyle style = DocumentStyle.Default)
    {
        var apa = style == DocumentStyle.Apa;
        var parsed = Markdown.Parse(markdown ?? string.Empty, Pipeline);
        var blocks = parsed.ToList();

        string? documentTitle = null;
        if (blocks.FirstOrDefault() is HeadingBlock { Level: 1 } first)
        {
            documentTitle = PlainText(first.Inline);
            blocks.RemoveAt(0);
        }
        documentTitle = string.IsNullOrWhiteSpace(documentTitle) ? title : documentTitle;

        using var stream = new MemoryStream();
        using (var word = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, autoSave: true))
        {
            var main = word.AddMainDocumentPart();
            main.Document = new Document(new Body());
            var body = main.Document.Body!;

            if (apa) AddApaStyles(main); else AddStyles(main);
            var ctx = new Context { Main = main, Numbering = AddNumbering(main), Apa = apa };

            if (apa)
            {
                // Portada: título (negrita, centrado, a un tercio de la página) y las primeras líneas (autor, fecha...),
                // seguidas de un salto de página.
                body.Append(new Paragraph(
                    new ParagraphProperties { ParagraphStyleId = new ParagraphStyleId { Val = "Title" }, SpacingBetweenLines = new SpacingBetweenLines { Before = "2880", After = "480" } },
                    MakeRun(documentTitle!, new Fmt())));

                var meta = 0;
                while (blocks.Count > 0 && blocks[0] is ParagraphBlock metaBlock && meta < 6)
                {
                    body.Append(Para(new ParagraphProperties { ParagraphStyleId = new ParagraphStyleId { Val = "ApaTitlePage" } },
                        InlineRuns(metaBlock.Inline, new Fmt(HardBreaks: true), ctx)));
                    blocks.RemoveAt(0);
                    meta++;
                }
                if (meta == 0)
                    body.Append(new Paragraph(new ParagraphProperties { ParagraphStyleId = new ParagraphStyleId { Val = "ApaTitlePage" } },
                        MakeRun($"{DateTime.Now:d 'de' MMMM 'de' yyyy}", new Fmt())));

                body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
                body.Append(new Paragraph(new ParagraphProperties { ParagraphStyleId = new ParagraphStyleId { Val = "Title" } },
                    MakeRun(documentTitle!, new Fmt())));
            }
            else
            {
                body.Append(new Paragraph(new ParagraphProperties { ParagraphStyleId = new ParagraphStyleId { Val = "Title" } },
                    MakeRun(documentTitle!, new Fmt())));
                body.Append(new Paragraph(new ParagraphProperties { ParagraphStyleId = new ParagraphStyleId { Val = "Subtitle" } },
                    MakeRun($"Generado por AI Agent Office · {DateTime.Now:dd/MM/yyyy HH:mm}", new Fmt())));
            }

            foreach (var block in blocks) RenderBlock(block, ctx, body, 0, false);

            body.Append(apa ? BuildApaSection(main) : BuildSection(main));

            word.PackageProperties.Title = documentTitle;
            word.PackageProperties.Creator = "AI Agent Office";
            word.PackageProperties.Created = DateTime.UtcNow;
        }

        return stream.ToArray();
    }

    /// <summary>True si el título corresponde a la sección de referencias o bibliografía.</summary>
    private static bool IsReferencesTitle(string text) =>
        text.StartsWith("Referencias", StringComparison.OrdinalIgnoreCase) || text.StartsWith("References", StringComparison.OrdinalIgnoreCase) ||
        text.StartsWith("Bibliograf", StringComparison.OrdinalIgnoreCase);

    /// <summary>Crea un párrafo con sus propiedades y su contenido.</summary>
    private static Paragraph Para(ParagraphProperties properties, IEnumerable<OpenXmlElement> children)
    {
        var paragraph = new Paragraph(properties);
        paragraph.Append(children);
        return paragraph;
    }

    #endregion

    #region Bloques (títulos, párrafos, listas, tablas)


    /// <summary>
    /// Convierte un bloque de Markdown (título, párrafo, lista, código, cita, tabla...) en elementos de
    /// Word.
    /// </summary>
    private void RenderBlock(Block block, Context ctx, OpenXmlElement target, int level, bool quote)
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
                var apaLevel = Math.Clamp(heading.Level - 1, 1, 3);
                var props = new ParagraphProperties { ParagraphStyleId = new ParagraphStyleId { Val = $"Heading{apaLevel}" } };
                if (isRefs) props.PageBreakBefore = new PageBreakBefore();
                target.Append(Para(props, InlineRuns(heading.Inline, new Fmt(), ctx)));
                break;
            }

            case HeadingBlock heading:
                target.Append(Para(
                    new ParagraphProperties { ParagraphStyleId = new ParagraphStyleId { Val = $"Heading{Math.Min(heading.Level, 4)}" } },
                    InlineRuns(heading.Inline, new Fmt(), ctx)));
                break;

            case ParagraphBlock paragraph:
            {
                var styleId = quote ? "Quote" : "Normal";
                if (ctx.Apa && !quote)
                {
                    var first = PlainText(paragraph.Inline);
                    styleId = ctx.InReferences ? "Reference"
                        : ctx.InAbstract && !first.StartsWith("Palabras clave", StringComparison.OrdinalIgnoreCase) && !first.StartsWith("Keywords", StringComparison.OrdinalIgnoreCase) ? "Normal"
                        : "ApaBody";
                }
                target.Append(Para(
                    new ParagraphProperties { ParagraphStyleId = new ParagraphStyleId { Val = styleId } },
                    InlineRuns(paragraph.Inline, new Fmt(), ctx)));
                break;
            }

            case ListBlock list:
                RenderList(list, ctx, target, level);
                break;

            case CodeBlock code:
                RenderCode(code, target);
                break;

            case QuoteBlock q:
                foreach (var child in q) RenderBlock(child, ctx, target, level, true);
                break;

            case ThematicBreakBlock:
                target.Append(new Paragraph(new ParagraphProperties
                {
                    ParagraphBorders = new ParagraphBorders(new BottomBorder { Val = BorderValues.Single, Size = 6, Space = 1, Color = "A6A6A6" })
                }));
                break;

            case Markdig.Extensions.Tables.Table table:
                RenderTable(table, ctx, target);
                break;

            case HtmlBlock html:
                target.Append(new Paragraph(MakeRun(string.Join("\n", html.Lines.Lines.Take(html.Lines.Count).Select(l => l.ToString())), new Fmt())));
                break;

            case ContainerBlock container:
                foreach (var child in container) RenderBlock(child, ctx, target, level, quote);
                break;
        }
    }

    /// <summary>
    /// Convierte una lista (con viñetas o numerada, con niveles anidados) usando las definiciones de
    /// numeración.
    /// </summary>
    private void RenderList(ListBlock list, Context ctx, OpenXmlElement target, int level)
    {
        var ilvl = Math.Min(level, 3);
        int numId = 1; // bullets share one definition
        if (list.IsOrdered)
        {
            numId = ctx.NextNumberId++;
            ctx.Numbering.Append(new NumberingInstance(
                new AbstractNumId { Val = 2 },
                new LevelOverride(new StartOverrideNumberingValue { Val = 1 }) { LevelIndex = 0 }) { NumberID = numId });
        }

        foreach (var item in list.OfType<ListItemBlock>())
        {
            foreach (var sub in item)
            {
                switch (sub)
                {
                    case ParagraphBlock p:
                        target.Append(Para(
                            new ParagraphProperties
                            {
                                ParagraphStyleId = new ParagraphStyleId { Val = "ListParagraph" },
                                NumberingProperties = new NumberingProperties(
                                    new NumberingLevelReference { Val = ilvl }, new NumberingId { Val = numId })
                            },
                            InlineRuns(p.Inline, new Fmt(), ctx)));
                        break;
                    case ListBlock nested:
                        RenderList(nested, ctx, target, level + 1);
                        break;
                    default:
                        RenderBlock(sub, ctx, target, level + 1, false);
                        break;
                }
            }
        }
    }

    /// <summary>Convierte un bloque de código en párrafos con fuente monoespaciada.</summary>
    private static void RenderCode(CodeBlock code, OpenXmlElement target)
    {
        var lines = code.Lines.Lines;
        for (var i = 0; i < code.Lines.Count; i++)
        {
            var text = lines[i].ToString();
            target.Append(new Paragraph(
                new ParagraphProperties { ParagraphStyleId = new ParagraphStyleId { Val = "Code" } },
                MakeRun(text.Length == 0 ? " " : text, new Fmt())));
        }
        target.Append(new Paragraph(new ParagraphProperties { SpacingBetweenLines = new SpacingBetweenLines { After = "60" } }));
    }

    /// <summary>Convierte una tabla de Markdown en una tabla de Word con encabezado sombreado.</summary>
    private void RenderTable(Markdig.Extensions.Tables.Table mdTable, Context ctx, OpenXmlElement target)
    {
        var columns = mdTable.OfType<Markdig.Extensions.Tables.TableRow>().Select(r => r.Count).DefaultIfEmpty(1).Max();
        var table = new Table(new TableProperties
        {
            TableWidth = new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
            TableBorders = new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4, Color = "BFBFBF" },
                new LeftBorder { Val = BorderValues.Single, Size = 4, Color = "BFBFBF" },
                new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "BFBFBF" },
                new RightBorder { Val = BorderValues.Single, Size = 4, Color = "BFBFBF" },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4, Color = "BFBFBF" },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4, Color = "BFBFBF" }),
            TableCellMarginDefault = new TableCellMarginDefault(
                new TopMargin { Width = "40", Type = TableWidthUnitValues.Dxa },
                new TableCellLeftMargin { Width = 90, Type = TableWidthValues.Dxa },
                new BottomMargin { Width = "40", Type = TableWidthUnitValues.Dxa },
                new TableCellRightMargin { Width = 90, Type = TableWidthValues.Dxa })
        });
        var grid = new TableGrid();
        for (var i = 0; i < columns; i++) grid.Append(new GridColumn { Width = (9000 / columns).ToString() });
        table.Append(grid);

        foreach (var mdRow in mdTable.OfType<Markdig.Extensions.Tables.TableRow>())
        {
            var row = new TableRow();
            if (mdRow.IsHeader) row.Append(new TableRowProperties(new TableHeader()));

            var cellCount = 0;
            foreach (var mdCell in mdRow.OfType<Markdig.Extensions.Tables.TableCell>())
            {
                var cell = new TableCell(new TableCellProperties
                {
                    TableCellWidth = new TableCellWidth { Width = (5000 / columns).ToString(), Type = TableWidthUnitValues.Pct },
                    Shading = mdRow.IsHeader ? new Shading { Val = ShadingPatternValues.Clear, Fill = "D9E2F3" } : null
                });

                var wrote = false;
                foreach (var cellBlock in mdCell)
                {
                    if (cellBlock is ParagraphBlock p)
                    {
                        cell.Append(Para(
                            new ParagraphProperties { SpacingBetweenLines = new SpacingBetweenLines { After = "0" } },
                            InlineRuns(p.Inline, new Fmt(Bold: mdRow.IsHeader), ctx)));
                        wrote = true;
                    }
                }
                if (!wrote) cell.Append(new Paragraph());
                row.Append(cell);
                cellCount++;
            }

            for (; cellCount < columns; cellCount++) row.Append(new TableCell(new Paragraph()));
            table.Append(row);
        }

        target.Append(table);
        target.Append(new Paragraph(new ParagraphProperties { SpacingBetweenLines = new SpacingBetweenLines { After = "80" } }));
    }

    #endregion

    #region Texto en línea (negrita, cursiva, enlaces)


    /// <summary>Convierte el texto en línea de un párrafo en fragmentos de Word con su formato.</summary>
    private IEnumerable<OpenXmlElement> InlineRuns(ContainerInline? container, Fmt fmt, Context ctx)
    {
        if (container is null) yield break;

        foreach (var inline in container)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    yield return MakeRun(literal.Content.ToString(), fmt);
                    break;

                case HtmlEntityInline entity:
                    yield return MakeRun(entity.Transcoded.ToString(), fmt);
                    break;

                case CodeInline code:
                    yield return MakeRun(code.Content, fmt with { Code = true });
                    break;

                case LineBreakInline lineBreak:
                    yield return lineBreak.IsHard || fmt.HardBreaks ? new Run(new Break()) : MakeRun(" ", fmt);
                    break;

                case Markdig.Extensions.TaskLists.TaskList task:
                    yield return MakeRun(task.Checked ? "☑ " : "☐ ", fmt);
                    break;

                case EmphasisInline emphasis:
                    var inner = emphasis.DelimiterChar == '~'
                        ? fmt with { Strike = true }
                        : emphasis.DelimiterCount >= 2 ? fmt with { Bold = true } : fmt with { Italic = true };
                    foreach (var run in InlineRuns(emphasis, inner, ctx)) yield return run;
                    break;

                case LinkInline link when link.IsImage:
                    yield return MakeRun($"[imagen: {PlainText(link)}]", fmt with { Italic = true });
                    break;

                case LinkInline link:
                    foreach (var element in LinkRuns(link.Url, InlineRuns(link, fmt with { Link = true }, ctx), ctx)) yield return element;
                    break;

                case AutolinkInline auto:
                    foreach (var element in LinkRuns(auto.Url, new[] { MakeRun(auto.Url, fmt with { Link = true }) }, ctx)) yield return element;
                    break;

                case ContainerInline nested:
                    foreach (var run in InlineRuns(nested, fmt, ctx)) yield return run;
                    break;
            }
        }
    }

    /// <summary>Envuelve el texto en un hipervínculo real si la dirección es segura.</summary>
    private static IEnumerable<OpenXmlElement> LinkRuns(string? url, IEnumerable<OpenXmlElement> runs, Context ctx)
    {
        // Solo los esquemas seguros (http, https y mailto) se vuelven hipervínculos reales.
        if (url is not null && Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            (uri.Scheme is "http" or "https" or "mailto"))
        {
            var relationship = ctx.Main.AddHyperlinkRelationship(uri, true);
            yield return new Hyperlink(runs.ToList()) { Id = relationship.Id };
        }
        else
        {
            foreach (var run in runs) yield return run;
        }
    }

    /// <summary>Crea un fragmento de texto con su formato (negrita, cursiva, código, enlace).</summary>
    private static Run MakeRun(string text, Fmt fmt)
    {
        var run = new Run();
        var props = new RunProperties
        {
            RunFonts = fmt.Code ? new RunFonts { Ascii = "Consolas", HighAnsi = "Consolas", ComplexScript = "Consolas" } : null,
            Bold = fmt.Bold ? new Bold() : null,
            Italic = fmt.Italic ? new Italic() : null,
            Strike = fmt.Strike ? new Strike() : null,
            Color = fmt.Link ? new Color { Val = "0563C1" } : null,
            Underline = fmt.Link ? new Underline { Val = UnderlineValues.Single } : null,
            Shading = fmt.Code ? new Shading { Val = ShadingPatternValues.Clear, Fill = "EDEDED" } : null
        };
        if (props.HasChildren) run.Append(props);

        // El texto con saltos de línea se separa en líneas (Word ignora los \n dentro de w:t).
        var parts = text.Replace("\r\n", "\n").Split('\n');
        for (var i = 0; i < parts.Length; i++)
        {
            if (i > 0) run.Append(new Break());
            run.Append(new Text(parts[i]) { Space = SpaceProcessingModeValues.Preserve });
        }
        return run;
    }

    /// <summary>Texto plano de un contenido en línea (sin formato).</summary>
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

    #endregion

    #region Partes del paquete: secciones, estilos y numeración


    /// <summary>Sección estándar: A4 con número de página en el pie.</summary>
    private static SectionProperties BuildSection(MainDocumentPart main)
    {
        var footerPart = main.AddNewPart<FooterPart>();
        footerPart.Footer = new Footer(new Paragraph(
            new ParagraphProperties { Justification = new Justification { Val = JustificationValues.Center } },
            new Run(new RunProperties { Color = new Color { Val = "7F7F7F" }, FontSize = new FontSize { Val = "18" } },
                new Text("AI Agent Office · Página ") { Space = SpaceProcessingModeValues.Preserve }),
            new SimpleField(new Run(new RunProperties { Color = new Color { Val = "7F7F7F" }, FontSize = new FontSize { Val = "18" } }, new Text("1")))
            { Instruction = " PAGE " }));

        return new SectionProperties(
            new FooterReference { Type = HeaderFooterValues.Default, Id = main.GetIdOfPart(footerPart) },
            new PageSize { Width = 11906, Height = 16838 },            // A4
            new PageMargin { Top = 1304, Right = 1304, Bottom = 1304, Left = 1304, Header = 708, Footer = 708, Gutter = 0 });
    }

    /// <summary>Sección APA 7: carta, márgenes de 1", número de página arriba a la derecha, Times New Roman 12.</summary>
    private static SectionProperties BuildApaSection(MainDocumentPart main)
    {
        var headerPart = main.AddNewPart<HeaderPart>();
        headerPart.Header = new Header(new Paragraph(
            new ParagraphProperties { Justification = new Justification { Val = JustificationValues.Right } },
            new SimpleField(new Run(new Text("1"))) { Instruction = " PAGE " }));

        return new SectionProperties(
            new HeaderReference { Type = HeaderFooterValues.Default, Id = main.GetIdOfPart(headerPart) },
            new PageSize { Width = 12240, Height = 15840 },
            new PageMargin { Top = 1440, Right = 1440, Bottom = 1440, Left = 1440, Header = 720, Footer = 720, Gutter = 0 });
    }

    /// <summary>
    /// Estilos APA 7: doble espacio, sangría de primera línea, niveles de título y referencias con sangría
    /// francesa.
    /// </summary>
    private static void AddApaStyles(MainDocumentPart main)
    {
        var part = main.AddNewPart<StyleDefinitionsPart>();

        static Style Para(string id, string name, OpenXmlElement[] pPr, OpenXmlElement[] rPr, bool heading = false)
        {
            var style = new Style(new StyleName { Val = name }, new BasedOn { Val = "Normal" }, new NextParagraphStyle { Val = "Normal" })
            { Type = StyleValues.Paragraph, StyleId = id };
            if (heading) style.Append(new UIPriority { Val = 9 }, new PrimaryStyle());
            if (pPr.Length > 0) style.Append(new StyleParagraphProperties(pPr));
            if (rPr.Length > 0) style.Append(new StyleRunProperties(rPr));
            return style;
        }

        part.Styles = new Styles(
            new DocDefaults(
                new RunPropertiesDefault(new RunPropertiesBaseStyle(
                    new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman", ComplexScript = "Times New Roman" },
                    new FontSize { Val = "24" }, new FontSizeComplexScript { Val = "24" },
                    new Languages { Val = "es-ES" })),
                new ParagraphPropertiesDefault(new ParagraphPropertiesBaseStyle(
                    new SpacingBetweenLines { Before = "0", After = "0", Line = "480", LineRule = LineSpacingRuleValues.Auto }))),
            new Style(new StyleName { Val = "Normal" }, new PrimaryStyle()) { Type = StyleValues.Paragraph, Default = true, StyleId = "Normal" },
            Para("Title", "Title",
                new OpenXmlElement[] { new KeepNext(), new Justification { Val = JustificationValues.Center } },
                new OpenXmlElement[] { new Bold() }, heading: true),
            Para("ApaTitlePage", "APA Title Page", new OpenXmlElement[] { new Justification { Val = JustificationValues.Center } }, Array.Empty<OpenXmlElement>()),
            Para("ApaBody", "APA Body", new OpenXmlElement[] { new Indentation { FirstLine = "720" } }, Array.Empty<OpenXmlElement>()),
            Para("Reference", "APA Reference", new OpenXmlElement[] { new Indentation { Left = "720", Hanging = "720" } }, Array.Empty<OpenXmlElement>()),
            Para("Heading1", "heading 1",
                new OpenXmlElement[] { new KeepNext(), new Justification { Val = JustificationValues.Center }, new OutlineLevel { Val = 0 } },
                new OpenXmlElement[] { new Bold() }, heading: true),
            Para("Heading2", "heading 2",
                new OpenXmlElement[] { new KeepNext(), new OutlineLevel { Val = 1 } },
                new OpenXmlElement[] { new Bold() }, heading: true),
            Para("Heading3", "heading 3",
                new OpenXmlElement[] { new KeepNext(), new OutlineLevel { Val = 2 } },
                new OpenXmlElement[] { new Bold(), new Italic() }, heading: true),
            Para("Heading4", "heading 4",
                new OpenXmlElement[] { new KeepNext(), new OutlineLevel { Val = 3 } },
                new OpenXmlElement[] { new Bold(), new Italic() }, heading: true),
            Para("ListParagraph", "List Paragraph", Array.Empty<OpenXmlElement>(), Array.Empty<OpenXmlElement>()),
            Para("Quote", "Quote", new OpenXmlElement[] { new Indentation { Left = "720" } }, Array.Empty<OpenXmlElement>()),
            Para("Code", "Code",
                new OpenXmlElement[]
                {
                    new Shading { Val = ShadingPatternValues.Clear, Fill = "F2F2F2" },
                    new SpacingBetweenLines { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto }
                },
                new OpenXmlElement[] { new RunFonts { Ascii = "Consolas", HighAnsi = "Consolas", ComplexScript = "Consolas" }, new FontSize { Val = "19" } }));
    }

    /// <summary>Estilos estándar: Calibri, títulos de colores, citas, código y listas.</summary>
    private static void AddStyles(MainDocumentPart main)
    {
        var part = main.AddNewPart<StyleDefinitionsPart>();

        static Style Para(string id, string name, OpenXmlElement[] pPr, OpenXmlElement[] rPr, bool heading = false)
        {
            var style = new Style(new StyleName { Val = name }, new BasedOn { Val = "Normal" }, new NextParagraphStyle { Val = "Normal" })
            { Type = StyleValues.Paragraph, StyleId = id };
            if (heading) style.Append(new UIPriority { Val = 9 }, new PrimaryStyle());
            if (pPr.Length > 0) style.Append(new StyleParagraphProperties(pPr));
            if (rPr.Length > 0) style.Append(new StyleRunProperties(rPr));
            return style;
        }

        static Style Heading(string id, string name, int level, string size, string color) => Para(id, name,
            new OpenXmlElement[] { new KeepNext(), new SpacingBetweenLines { Before = level == 1 ? "320" : "220", After = "80" }, new OutlineLevel { Val = level - 1 } },
            new OpenXmlElement[] { new Bold(), new Color { Val = color }, new FontSize { Val = size } }, heading: true);

        part.Styles = new Styles(
            new DocDefaults(
                new RunPropertiesDefault(new RunPropertiesBaseStyle(
                    new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri", EastAsia = "Calibri", ComplexScript = "Calibri" },
                    new FontSize { Val = "22" }, new FontSizeComplexScript { Val = "22" },
                    new Languages { Val = "es-ES" })),
                new ParagraphPropertiesDefault(new ParagraphPropertiesBaseStyle(
                    new SpacingBetweenLines { After = "120", Line = "276", LineRule = LineSpacingRuleValues.Auto }))),
            new Style(new StyleName { Val = "Normal" }, new PrimaryStyle()) { Type = StyleValues.Paragraph, Default = true, StyleId = "Normal" },
            Para("Title", "Title",
                new OpenXmlElement[]
                {
                    new ParagraphBorders(new BottomBorder { Val = BorderValues.Single, Size = 12, Space = 4, Color = "7C3AED" }),
                    new SpacingBetweenLines { Before = "0", After = "60" }
                },
                new OpenXmlElement[] { new Bold(), new Color { Val = "1F3864" }, new FontSize { Val = "52" } }, heading: true),
            Para("Subtitle", "Subtitle",
                new OpenXmlElement[] { new SpacingBetweenLines { After = "240" } },
                new OpenXmlElement[] { new Color { Val = "7F7F7F" }, new FontSize { Val = "20" } }),
            Heading("Heading1", "heading 1", 1, "34", "1F3864"),
            Heading("Heading2", "heading 2", 2, "28", "2F5496"),
            Heading("Heading3", "heading 3", 3, "24", "2F5496"),
            Heading("Heading4", "heading 4", 4, "22", "404040"),
            Para("ListParagraph", "List Paragraph",
                new OpenXmlElement[] { new SpacingBetweenLines { After = "40" } },
                Array.Empty<OpenXmlElement>()),
            Para("Quote", "Quote",
                new OpenXmlElement[]
                {
                    new ParagraphBorders(new LeftBorder { Val = BorderValues.Single, Size = 18, Space = 8, Color = "A78BFA" }),
                    new Indentation { Left = "480" }
                },
                new OpenXmlElement[] { new Italic(), new Color { Val = "595959" } }),
            Para("Code", "Code",
                new OpenXmlElement[]
                {
                    new Shading { Val = ShadingPatternValues.Clear, Fill = "F2F2F2" },
                    new SpacingBetweenLines { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto }
                },
                new OpenXmlElement[] { new RunFonts { Ascii = "Consolas", HighAnsi = "Consolas", ComplexScript = "Consolas" }, new FontSize { Val = "19" } }));
    }

    /// <summary>Definiciones de numeración: viñetas y listas numeradas con cuatro niveles.</summary>
    private static Numbering AddNumbering(MainDocumentPart main)
    {
        var part = main.AddNewPart<NumberingDefinitionsPart>();

        static AbstractNum Define(int id, bool ordered)
        {
            var abstractNum = new AbstractNum(new MultiLevelType { Val = MultiLevelValues.HybridMultilevel }) { AbstractNumberId = id };
            string[] bullets = { "•", "◦", "▪", "–" };
            NumberFormatValues[] formats = { NumberFormatValues.Decimal, NumberFormatValues.LowerLetter, NumberFormatValues.LowerRoman, NumberFormatValues.Decimal };
            for (var i = 0; i < 4; i++)
            {
                abstractNum.Append(new Level(
                    new StartNumberingValue { Val = 1 },
                    new NumberingFormat { Val = ordered ? formats[i] : NumberFormatValues.Bullet },
                    new LevelText { Val = ordered ? $"%{i + 1}." : bullets[i] },
                    new LevelJustification { Val = LevelJustificationValues.Left },
                    new PreviousParagraphProperties(new Indentation { Left = (720 + i * 360).ToString(), Hanging = "360" }))
                { LevelIndex = i });
            }
            return abstractNum;
        }

        part.Numbering = new Numbering(
            Define(1, ordered: false),
            Define(2, ordered: true),
            new NumberingInstance(new AbstractNumId { Val = 1 }) { NumberID = 1 });
        return part.Numbering;
    }
    #endregion

}
