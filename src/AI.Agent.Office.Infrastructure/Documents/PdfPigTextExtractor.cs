using System.Text;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Infrastructure.FileSystem;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace AI.Agent.Office.Infrastructure.Documents;

/// <summary>Extracts text locally with PdfPig. Scanned PDFs (no text layer) are flagged as requiring OCR.</summary>
public sealed class PdfPigTextExtractor : IDocumentTextExtractor
{
    private const int MinCharsPerPageForText = 20;
    private static readonly byte[] PdfMagic = "%PDF-"u8.ToArray();

    private readonly ILogger<PdfPigTextExtractor> _logger;

    public PdfPigTextExtractor(ILogger<PdfPigTextExtractor> logger) => _logger = logger;

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

    private static bool HasPdfSignature(string path)
    {
        using var stream = File.OpenRead(path);
        var buffer = new byte[PdfMagic.Length];
        return stream.Read(buffer, 0, buffer.Length) == buffer.Length && buffer.AsSpan().SequenceEqual(PdfMagic);
    }
}
