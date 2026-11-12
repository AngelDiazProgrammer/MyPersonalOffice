using System.Text;
using ATLAS.Application.Interfaces;
using ATLAS.Infrastructure.FileSystem;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace ATLAS.Infrastructure.Documents;

/// <summary>
/// Extrae el texto localmente con PdfPig. Los PDF escaneados (sin capa de texto) se marcan como que
/// requieren OCR.
/// </summary>
public sealed class PdfPigTextExtractor : IDocumentTextExtractor
{
    /// <summary>Caracteres mínimos por página para considerar que el PDF tiene texto.</summary>
    private const int MinCharsPerPageForText = 20;
    private static readonly byte[] PdfMagic = "%PDF-"u8.ToArray();

    private readonly ILogger<PdfPigTextExtractor> _logger;

    public PdfPigTextExtractor(ILogger<PdfPigTextExtractor> logger) => _logger = logger;

    /// <summary>
    /// Copia el PDF a un archivo temporal, valida su firma y extrae el texto con límites de páginas y
    /// caracteres.
    /// </summary>
    public async Task<DocumentExtractionResult> ExtractAsync(
        Stream content, string fileName, int maxPages, int maxChars, CancellationToken cancellationToken)
    {
        await using var temp = await TempFile.CreateAsync(content, ".pdf", cancellationToken).ConfigureAwait(false);

        if (!HasPdfSignature(temp.Path))
            throw new OfficeValidationException(new[] { "El archivo no es un PDF válido." });

        try
        {
            return await Task.Run(() => Extract(temp.Path, maxPages, maxChars, cancellationToken), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (OfficeValidationException) { throw; }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo leer el PDF");
            throw new OfficeValidationException(new[] { "No se pudo leer el PDF (¿está dañado o protegido con contraseña?)." });
        }
    }

    /// <summary>Lee las páginas y arma el texto con marcas "--- Página N ---".</summary>
    private static DocumentExtractionResult Extract(string path, int maxPages, int maxChars, CancellationToken ct)
    {
        using var document = PdfDocument.Open(path);
        var totalPages = document.NumberOfPages;
        var sb = new StringBuilder();
        var truncated = false;
        var textChars = 0;
        var processed = 0;

        foreach (var page in document.GetPages())
        {
            ct.ThrowIfCancellationRequested();
            if (processed >= maxPages) { truncated = true; break; }
            processed++;

            var text = page.Text.Trim();
            textChars += text.Count(c => !char.IsWhiteSpace(c));

            sb.Append("--- Página ").Append(page.Number).AppendLine(" ---");
            sb.AppendLine(text);
            sb.AppendLine();

            if (sb.Length >= maxChars) { truncated = true; break; }
        }

        var result = sb.ToString();
        if (result.Length > maxChars) { result = result[..maxChars]; truncated = true; }

        var requiresOcr = processed > 0 && textChars < processed * MinCharsPerPageForText;
        return new DocumentExtractionResult(requiresOcr ? string.Empty : result, totalPages, truncated, requiresOcr);
    }

    /// <summary>True si el archivo empieza con la firma de un PDF (%PDF-).</summary>
    private static bool HasPdfSignature(string path)
    {
        using var stream = File.OpenRead(path);
        var buffer = new byte[PdfMagic.Length];
        return stream.Read(buffer, 0, buffer.Length) == buffer.Length && buffer.AsSpan().SequenceEqual(PdfMagic);
    }
}
