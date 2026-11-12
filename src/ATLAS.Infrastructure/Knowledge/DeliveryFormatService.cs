using System.Text;
using ATLAS.Application.Interfaces;
using ATLAS.Application.Options;
using ATLAS.Infrastructure.Configuration;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UglyToad.PdfPig;

namespace ATLAS.Infrastructure.Knowledge;

/// <summary>
/// El formato de entrega activo es el archivo MÁS RECIENTE de la carpeta de formatos (.md, .txt, .docx
/// o .pdf), así que basta con soltar un archivo nuevo. Los Word y PDF se leen como texto (títulos,
/// instrucciones y {{campos}}).
/// </summary>
public sealed class DeliveryFormatService : IDeliveryFormatService
{
    private static readonly string[] Extensions = { ".md", ".txt", ".docx", ".pdf" };

    private readonly KnowledgeOptions _options;
    private readonly ILogger<DeliveryFormatService> _logger;
    private (string Path, DateTime Stamp, DeliveryFormat Format)? _cache;

    /// <summary>Crea el servicio con la carpeta de formatos configurada.</summary>
    public DeliveryFormatService(IOptions<KnowledgeOptions> options, ILogger<DeliveryFormatService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>Devuelve el formato más reciente; guarda el resultado hasta que el archivo cambie.</summary>
    public async Task<DeliveryFormat?> GetActiveFormatAsync(CancellationToken cancellationToken = default)
    {
        var directory = OfficeConfiguration.ResolveDirectory(_options.FormatDirectory);
        if (directory is null) return null;

        var newest = Directory.EnumerateFiles(directory)
            .Where(f => Extensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase) && !Path.GetFileName(f).StartsWith("~$"))
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .FirstOrDefault();
        if (newest is null) return null;

        if (_cache is { } c && c.Path == newest.FullName && c.Stamp == newest.LastWriteTimeUtc) return c.Format;

        try
        {
            var text = await ReadAsync(newest.FullName, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(text)) return null;

            var format = new DeliveryFormat(newest.Name, text.Trim());
            _cache = (newest.FullName, newest.LastWriteTimeUtc, format);
            return format;
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "No se pudo leer el formato '{File}'", newest.Name);
            return null;
        }
    }

    /// <summary>Lee un archivo de formato como texto según su extensión.</summary>
    private static async Task<string> ReadAsync(string path, CancellationToken ct)
    {
        switch (Path.GetExtension(path).ToLowerInvariant())
        {
            case ".docx":
                return ReadDocx(path);
            case ".pdf":
                using (var pdf = PdfDocument.Open(path))
                    return string.Join("\n\n", pdf.GetPages().Take(30).Select(p => p.Text));
            default:
                return await File.ReadAllTextAsync(path, ct).ConfigureAwait(false);
        }
    }

    /// <summary>Extrae el texto de un .docx (párrafos y tablas).</summary>
    private static string ReadDocx(string path)
    {
        // Se abre una copia para que el archivo pueda seguir abierto en Word.
        var temp = System.IO.Path.GetTempFileName();
        try
        {
            File.Copy(path, temp, overwrite: true);
            using var doc = WordprocessingDocument.Open(temp, false);
            var body = doc.MainDocumentPart?.Document.Body;
            if (body is null) return string.Empty;

            var sb = new StringBuilder();
            foreach (var element in body.ChildElements)
            {
                switch (element)
                {
                    case Paragraph p:
                        var style = p.ParagraphProperties?.ParagraphStyleId?.Val?.Value ?? string.Empty;
                        var text = p.InnerText;
                        if (string.IsNullOrWhiteSpace(text)) { sb.AppendLine(); break; }
                        sb.AppendLine(style.StartsWith("Heading", StringComparison.OrdinalIgnoreCase) || style == "Title"
                            ? "## " + text
                            : text);
                        break;
                    case Table t:
                        foreach (var row in t.Elements<TableRow>())
                            sb.AppendLine("| " + string.Join(" | ", row.Elements<TableCell>().Select(c => c.InnerText.Trim())) + " |");
                        break;
                }
            }
            return sb.ToString();
        }
        finally
        {
            try { File.Delete(temp); } catch (IOException) { }
        }
    }
}
