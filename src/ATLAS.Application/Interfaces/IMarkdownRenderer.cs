namespace ATLAS.Application.Interfaces;

/// <summary>
/// Convierte Markdown (respuestas de los modelos) en HTML seguro para mostrarlo en la interfaz.
/// La implementación vive en Infrastructure; la capa de presentación solo conoce este contrato (spec 005, RF-008).
/// </summary>
public interface IMarkdownRenderer
{
    /// <summary>Devuelve el HTML saneado: sin HTML crudo, sin imágenes y con enlaces solo http, https, mailto o internos.</summary>
    string ToHtml(string? markdown);
}
