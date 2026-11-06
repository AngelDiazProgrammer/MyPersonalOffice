using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Domain.Entities;
using AI.Agent.Office.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Infrastructure.Knowledge;

/// <summary>
/// Lee la base de conocimiento desde archivos Markdown. Cada situación es un encabezado de nivel 2 con
/// un código: <c>## SEG-003 · Título</c>, seguido de su procedimiento. Líneas opcionales dentro de la
/// situación: <c>**Severidad:** Alta</c> y <c>**Palabras clave:** a, b, c</c>. Un archivo puede empezar
/// con un bloque de metadatos con <c>confidential: true</c>.
/// </summary>
public sealed partial class MarkdownKnowledgeBaseService : IKnowledgeBaseService
{
    /// <summary>Palabras vacías que se ignoran al buscar (artículos, preposiciones...).</summary>
    private static readonly HashSet<string> StopWords = new(StringComparer.Ordinal)
    {
        "de", "la", "el", "los", "las", "un", "una", "unos", "unas", "y", "o", "a", "en", "que", "con", "por", "para", "al", "del", "se", "me", "mi",
        "mis", "su", "sus", "es", "lo", "le", "les", "ya", "no", "si", "como", "pero", "mas", "muy", "ese", "esa", "esto", "eso", "hay", "fue",
        "era", "he", "ha", "han", "tengo", "tenia", "hacer", "debo", "puedo", "ahora", "ayer", "hoy", "esta", "estos", "estas", "ser", "son", "uno"
    };

    private readonly KnowledgeOptions _options;
    private readonly ILogger<MarkdownKnowledgeBaseService> _logger;
    private volatile IReadOnlyList<KnowledgeEntry> _entries = Array.Empty<KnowledgeEntry>();

    #region Carga de archivos

    /// <summary>Crea el servicio con la configuración de la base.</summary>
    public MarkdownKnowledgeBaseService(IOptions<KnowledgeOptions> options, ILogger<MarkdownKnowledgeBaseService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>Todas las situaciones cargadas.</summary>
    public IReadOnlyList<KnowledgeEntry> GetAll() => _entries;

    /// <summary>Busca una situación por su código (SEG-003).</summary>
    public KnowledgeEntry? GetByCode(string code) =>
        _entries.FirstOrDefault(e => string.Equals(e.Code, code?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Relee todos los archivos .md de la carpeta e ignora los códigos duplicados. Devuelve cuántas
    /// situaciones cargó.
    /// </summary>
    public async Task<int> ReloadAsync(CancellationToken cancellationToken = default)
    {
        var directory = OfficeConfiguration.ResolveDirectory(_options.Directory);
        if (directory is null)
        {
            _logger.LogWarning("No se encontró la carpeta de la base de conocimiento '{Dir}'.", _options.Directory);
            _entries = Array.Empty<KnowledgeEntry>();
            return 0;
        }

        var entries = new List<KnowledgeEntry>();
        foreach (var file in Directory.EnumerateFiles(directory, "*.md").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            var text = await File.ReadAllTextAsync(file, cancellationToken).ConfigureAwait(false);
            foreach (var entry in Parse(text, Path.GetFileName(file)))
            {
                if (entries.Any(e => string.Equals(e.Code, entry.Code, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogWarning("Código duplicado {Code} en {File}: se ignora.", entry.Code, entry.SourceFile);
                    continue;
                }
                entries.Add(entry);
            }
        }

        _entries = entries;
        _logger.LogInformation("Base de conocimiento cargada: {Count} situaciones ({Codes})", entries.Count, string.Join(", ", entries.Select(e => e.Code)));
        return entries.Count;
    }

    #endregion

    #region Lectura del Markdown


    /// <summary>
    /// Convierte el texto de un archivo en situaciones (código, título, severidad, palabras clave y cuerpo
    /// completo).
    /// </summary>
    public static IReadOnlyList<KnowledgeEntry> Parse(string markdown, string sourceFile)
    {
        var text = markdown.TrimStart('﻿').Replace("\r\n", "\n");
        var confidential = false;

        if (text.StartsWith("---\n", StringComparison.Ordinal) && text.IndexOf("\n---", 4, StringComparison.Ordinal) is var end and > 0)
        {
            foreach (var line in text[4..end].Split('\n'))
                if (line.Split(':', 2) is { Length: 2 } kv && kv[0].Trim().Equals("confidential", StringComparison.OrdinalIgnoreCase))
                    confidential = bool.TryParse(kv[1].Trim(), out var b) && b;
            text = text[(text.IndexOf('\n', end + 1) + 1)..];
        }

        var entries = new List<KnowledgeEntry>();
        string? code = null, title = null;
        var body = new StringBuilder();

        void Flush()
        {
            if (code is null) return;
            var content = body.ToString().Trim();
            entries.Add(new KnowledgeEntry
            {
                Code = code!,
                Title = title!,
                Severity = Field(content, "Severidad"),
                Keywords = Field(content, "Palabras clave").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                Body = content,
                SourceFile = sourceFile,
                Confidential = confidential
            });
            body.Clear();
        }

        foreach (var line in text.Split('\n'))
        {
            if (Header().Match(line) is { Success: true } m)
            {
                Flush();
                code = m.Groups[1].Value.ToUpperInvariant();
                title = m.Groups[2].Value.Trim();
                body.AppendLine(line);
            }
            else if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                Flush();               // a non-entry level-2 heading ends the previous entry
                code = null;
            }
            else if (code is not null)
            {
                body.AppendLine(line);
            }
        }
        Flush();
        return entries;
    }

    /// <summary>Valor de una línea como "**Severidad:** Alta" dentro de una situación.</summary>
    private static string Field(string content, string name)
    {
        var m = Regex.Match(content, $@"^\s*\*\*{Regex.Escape(name)}:?\*\*:?\s*(.+)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[1].Value.Trim() : string.Empty;
    }

    /// <summary>Encabezado de una situación: "## SEG-003 · Título".</summary>
    [GeneratedRegex(@"^##\s+([A-Za-z]{2,6}-\d{1,4})\s*[·\-—:|]\s*(.+?)\s*$")]
    private static partial Regex Header();

    #endregion

    #region Búsqueda por palabras clave


    /// <summary>
    /// Busca las situaciones que mejor coinciden con un texto. Un código en el texto gana de inmediato; si
    /// no, puntúa palabras clave y raíces de palabras.
    /// </summary>
    public IReadOnlyList<KnowledgeHit> Search(string query, int maxResults)
    {
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<KnowledgeHit>();

        var queryStems = Stems(query);
        var queryWords = new HashSet<string>(queryStems);
        var explicitCodes = CodeInText().Matches(query).Select(m => NormalizeCode(m.Value)).ToHashSet();

        var hits = new List<KnowledgeHit>();
        foreach (var entry in _entries)
        {
            double score = 0;

            if (explicitCodes.Contains(NormalizeCode(entry.Code))) score += 1000;

            foreach (var keyword in entry.Keywords)
            {
                var stems = Stems(keyword);
                if (stems.Count == 0) continue;
                if (stems.Count == 1 ? queryWords.Contains(stems[0]) : ContainsSequence(queryStems, stems))
                    score += stems.Count == 1 ? 2 : 4 + stems.Count;     // multi-word phrases are far more specific
            }

            score += 1.5 * Stems(entry.Title).Distinct().Count(queryWords.Contains);

            var bodyOverlap = Stems(entry.Body).Distinct().Count(queryWords.Contains);
            score += Math.Min(3.0, 0.25 * bodyOverlap);

            if (score > 0) hits.Add(new KnowledgeHit(entry, Math.Round(score, 2)));
        }

        return hits.OrderByDescending(h => h.Score).ThenBy(h => h.Entry.Code, StringComparer.Ordinal).Take(Math.Max(1, maxResults)).ToList();
    }

    /// <summary>True si las palabras de la frase aparecen seguidas en el texto.</summary>
    private static bool ContainsSequence(IReadOnlyList<string> haystack, IReadOnlyList<string> needle)
    {
        for (var i = 0; i + needle.Count <= haystack.Count; i++)
        {
            var ok = true;
            for (var j = 0; j < needle.Count && ok; j++) ok = haystack[i + j] == needle[j];
            if (ok) return true;
        }
        return false;
    }

    /// <summary>"SEG-005", "seg 5" y "seg005" se convierten todos en "SEG5".</summary>
    private static string NormalizeCode(string code)
    {
        var compact = Regex.Replace(code.ToUpperInvariant(), @"[^A-Z0-9]", string.Empty);
        var m = Regex.Match(compact, @"^([A-Z]+)0*(\d+)$");
        return m.Success ? m.Groups[1].Value + m.Groups[2].Value : compact;
    }

    [GeneratedRegex(@"\b[A-Za-z]{2,6}[-\s]?\d{1,4}\b")]
    private static partial Regex CodeInText();

    /// <summary>
    /// Minúsculas, sin acentos ni puntuación; cada palabra se reduce a una raíz de 5 letras para que
    /// "compartí", "compartir" y "compartiendo" coincidan.
    /// </summary>
    private static List<string> Stems(string text)
    {
        var normalized = new StringBuilder();
        foreach (var ch in text.Normalize(NormalizationForm.FormD))
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (cat == UnicodeCategory.NonSpacingMark) continue;
            normalized.Append(char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : ' ');
        }

        return normalized.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length >= 2 && !StopWords.Contains(w))
            .Select(w => w.Length > 5 ? w[..5] : w)
            .ToList();
    }
    #endregion

}
