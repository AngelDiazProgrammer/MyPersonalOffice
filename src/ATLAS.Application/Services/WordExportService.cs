using System.Text;
using System.Text.RegularExpressions;
using ATLAS.Application.DTOs;
using ATLAS.Application.Interfaces;
using ATLAS.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATLAS.Application.Services;

/// <summary>Exporta Markdown a Word o PDF: valida el contenido, genera el archivo y lo guarda para su descarga.</summary>
public sealed partial class WordExportService : IWordExportService, IPdfExportService
{
    private readonly IPdfDocumentGenerator _pdf;
    private readonly IWordDocumentGenerator _generator;
    private readonly IGeneratedDocumentStore _store;
    private readonly DocumentOptions _options;
    private readonly ILogger<WordExportService> _logger;

    public WordExportService(
        IWordDocumentGenerator generator, IPdfDocumentGenerator pdf, IGeneratedDocumentStore store,
        IOptions<DocumentOptions> options, ILogger<WordExportService> logger)
    {
        _generator = generator;
        _pdf = pdf;
        _store = store;
        _options = options.Value;
        _logger = logger;
    }

    public Task<GeneratedDocumentDto> ExportAsync(string title, string markdown, DocumentStyle style = DocumentStyle.Default, CancellationToken cancellationToken = default) =>
        ExportCoreAsync(title, markdown, style, pdf: false, cancellationToken);

    Task<GeneratedDocumentDto> IPdfExportService.ExportAsync(string title, string markdown, DocumentStyle style, CancellationToken cancellationToken) =>
        ExportCoreAsync(title, markdown, style, pdf: true, cancellationToken);

    private async Task<GeneratedDocumentDto> ExportCoreAsync(string title, string markdown, DocumentStyle style, bool pdf, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(markdown)) errors.Add("No hay contenido para exportar.");
        else if (markdown.Length > _options.MaxWordMarkdownChars) errors.Add($"El contenido supera el máximo de {_options.MaxWordMarkdownChars} caracteres.");
        if (errors.Count > 0) throw new OfficeValidationException(errors);

        var cleanTitle = string.IsNullOrWhiteSpace(title) ? "Documento" : title.Trim();
        var bytes = pdf ? _pdf.Generate(cleanTitle, markdown, style) : _generator.Generate(cleanTitle, markdown, style);
        var saved = await _store.SaveAsync(BuildFileName(cleanTitle, pdf ? ".pdf" : ".docx"), bytes, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Documento {Kind} generado ({Style}): {File} ({Size} bytes)", pdf ? "PDF" : "Word", style, saved.FileName, saved.SizeBytes);
        return new GeneratedDocumentDto(saved.Id, saved.FileName, saved.SizeBytes);
    }

    /// <summary>Nombre de archivo seguro: solo ASCII, corto, sin caracteres de ruta y con la extensión indicada.</summary>
    internal static string BuildFileName(string title, string extension = ".docx")
    {
        var normalized = title.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in normalized)
        {
            var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == System.Globalization.UnicodeCategory.NonSpacingMark) continue;
            sb.Append(char.IsLetterOrDigit(ch) && ch < 128 ? ch : '_');
        }

        var name = Collapse().Replace(sb.ToString(), "_").Trim('_');
        if (name.Length == 0) name = "Documento";
        if (name.Length > 60) name = name[..60].TrimEnd('_');
        return $"{name}_{DateTime.Now:yyyyMMdd_HHmm}{extension}";
    }

    [GeneratedRegex("_{2,}")]
    private static partial Regex Collapse();
}
