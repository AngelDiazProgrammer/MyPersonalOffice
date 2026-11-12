using ATLAS.Application.Interfaces;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace ATLAS.Infrastructure.Documents;

/// <summary>
/// Convierte Markdown a HTML seguro para mostrar respuestas de los modelos: sin HTML crudo, sin imágenes (una imagen externa
/// podría filtrar datos a otro servidor) y con enlaces solo http, https, mailto o internos (reglas de la spec 003).
/// </summary>
public sealed class SafeMarkdownRenderer : IMarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    /// <summary>Devuelve el HTML ya saneado del Markdown indicado.</summary>
    public string ToHtml(string? markdown)
    {
        var document = Markdown.Parse(markdown ?? string.Empty, Pipeline);

        // Las imágenes NUNCA se cargan: se muestra solo su texto alternativo.
        foreach (var image in document.Descendants<LinkInline>().Where(l => l.IsImage).ToList())
        {
            var alt = string.Concat(image.Descendants<LiteralInline>().Select(l => l.Content.ToString()));
            image.ReplaceBy(new LiteralInline("[imagen" + (alt.Length > 0 ? ": " + alt : string.Empty) + "]"));
        }

        foreach (var link in document.Descendants<LinkInline>())
        {
            var url = link.Url ?? string.Empty;
            var safe = url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                       || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                       || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                       || url.StartsWith('#');
            if (!safe) link.Url = "#";
        }

        using var writer = new StringWriter();
        var renderer = new Markdig.Renderers.HtmlRenderer(writer);
        Pipeline.Setup(renderer);
        renderer.Render(document);
        writer.Flush();
        return writer.ToString();
    }
}
