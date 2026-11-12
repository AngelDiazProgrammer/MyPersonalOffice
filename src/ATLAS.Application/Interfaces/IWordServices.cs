using ATLAS.Application.DTOs;
using ATLAS.Domain.Entities;

namespace ATLAS.Application.Interfaces;

/// <summary>Estilo visual de un documento generado.</summary>
public enum DocumentStyle
{
    /// <summary>Aspecto corporativo sencillo (Calibri, A4).</summary>
    Default,
    /// <summary>
    /// APA 7: Times New Roman 12, doble espacio, portada, número de página en el encabezado y referencias
    /// con sangría francesa.
    /// </summary>
    Apa
}

/// <summary>Convierte Markdown en un .docx (implementado con Open XML en Infraestructura).</summary>
public interface IWordDocumentGenerator
{
    byte[] Generate(string title, string markdown, DocumentStyle style = DocumentStyle.Default);
}

/// <summary>Convierte Markdown en un PDF (implementado con QuestPDF en Infraestructura).</summary>
public interface IPdfDocumentGenerator
{
    byte[] Generate(string title, string markdown, DocumentStyle style = DocumentStyle.Default);
}

/// <summary>Guarda en disco los archivos generados (nombres saneados y retención limitada).</summary>
public interface IGeneratedDocumentStore
{
    Task<GeneratedDocument> SaveAsync(string fileName, byte[] content, CancellationToken cancellationToken = default);
    GeneratedDocument? Get(Guid id);
}

/// <summary>Exporta Markdown a un archivo Word y lo deja listo para descargar.</summary>
public interface IWordExportService
{
    Task<GeneratedDocumentDto> ExportAsync(string title, string markdown, DocumentStyle style = DocumentStyle.Default, CancellationToken cancellationToken = default);
}

/// <summary>Exporta Markdown a un archivo PDF y lo deja listo para descargar.</summary>
public interface IPdfExportService
{
    Task<GeneratedDocumentDto> ExportAsync(string title, string markdown, DocumentStyle style = DocumentStyle.Default, CancellationToken cancellationToken = default);
}
