namespace ATLAS.Application.Options;

/// <summary>Límites de los documentos subidos y de los exportados (sección "Documents").</summary>
public sealed class DocumentOptions
{
    public const string SectionName = "Documents";

    /// <summary>Tamaño máximo de un PDF, en bytes.</summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    /// <summary>Máximo de páginas que se leen.</summary>
    public int MaxPages { get; set; } = 200;
    /// <summary>Máximo de caracteres de texto que se extraen.</summary>
    public int MaxExtractedChars { get; set; } = 200_000;
    /// <summary>Extensiones de archivo permitidas.</summary>
    public string[] AllowedExtensions { get; set; } = { ".pdf" };
    /// <summary>Tipos de contenido (MIME) permitidos.</summary>
    public string[] AllowedContentTypes { get; set; } = { "application/pdf", "application/x-pdf", "application/octet-stream" };
    /// <summary>Máximo de documentos guardados en memoria.</summary>
    public int MaxDocumentsInMemory { get; set; } = 20;

    /// <summary>Texto Markdown más largo que se acepta al exportar a Word o PDF.</summary>
    public int MaxWordMarkdownChars { get; set; } = 400_000;
}
