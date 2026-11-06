using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Application.Services;

/// <summary>Recibe un PDF subido: lo valida, extrae su texto y lo guarda en memoria (el archivo no se conserva).</summary>
public sealed class DocumentService : IDocumentService
{
    private readonly IDocumentTextExtractor _extractor;
    private readonly IDocumentStore _store;
    private readonly DocumentOptions _options;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentTextExtractor extractor, IDocumentStore store, IOptions<DocumentOptions> options, ILogger<DocumentService> logger)
    {
        _extractor = extractor;
        _store = store;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>Valida el archivo (extensión, tipo, firma y tamaño), extrae el texto y devuelve sus datos.</summary>
    public async Task<DocumentDto> UploadAsync(
        Stream content, string fileName, string? contentType, long length, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var safeName = Path.GetFileName(fileName ?? string.Empty);
        var extension = Path.GetExtension(safeName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(safeName)) errors.Add("El archivo no tiene nombre.");
        if (!_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            errors.Add($"Extensión no permitida. Solo: {string.Join(", ", _options.AllowedExtensions.Distinct(StringComparer.OrdinalIgnoreCase))}.");
        if (!string.IsNullOrWhiteSpace(contentType) &&
            !_options.AllowedContentTypes.Contains(contentType.Split(';')[0].Trim(), StringComparer.OrdinalIgnoreCase))
            errors.Add($"Tipo de contenido no permitido: {contentType}.");
        if (length <= 0) errors.Add("El archivo está vacío.");
        if (length > _options.MaxFileSizeBytes)
            errors.Add($"El archivo supera el máximo de {_options.MaxFileSizeBytes / (1024 * 1024)} MB.");
        if (errors.Count > 0) throw new OfficeValidationException(errors);

        var result = await _extractor.ExtractAsync(content, safeName, _options.MaxPages, _options.MaxExtractedChars, cancellationToken)
            .ConfigureAwait(false);

        var document = new OfficeDocument
        {
            FileName = safeName,
            PageCount = result.PageCount,
            Text = result.Text,
            Truncated = result.Truncated,
            RequiresOcr = result.RequiresOcr
        };
        _store.Add(document);

        _logger.LogInformation("Documento procesado: {Pages} páginas, {Chars} caracteres, OCR requerido: {Ocr}",
            document.PageCount, document.Text.Length, document.RequiresOcr);
        return document.ToDto();
    }
}
