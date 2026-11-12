using System.Text.RegularExpressions;

namespace ATLAS.Domain.Entities;

/// <summary>
/// Una situación de la base de conocimiento (por ejemplo "SEG-003 · Phishing"): código, metadatos y el
/// texto completo del procedimiento.
/// </summary>
public sealed class KnowledgeEntry
{
    public required string Code { get; init; }
    public required string Title { get; init; }
    public string Severity { get; init; } = string.Empty;
    public IReadOnlyList<string> Keywords { get; init; } = Array.Empty<string>();

    /// <summary>El Markdown completo de la situación (con su encabezado), tal como está escrito en el archivo.</summary>
    public required string Body { get; init; }

    public required string SourceFile { get; init; }

    /// <summary>Si es true, esta situación solo se envía a Claude (nunca a las APIs gratuitas).</summary>
    public bool Confidential { get; init; }

    #region Soluciones alternativas

    private IReadOnlyList<KnowledgeSolution>? _solutions;

    /// <summary>Soluciones de la situación (<c>### Solución A · Título</c>), en el orden del archivo. Se calculan una sola vez.</summary>
    public IReadOnlyList<KnowledgeSolution> Solutions => _solutions ??= ParseSolutions(Body);

    private static readonly Regex SolutionHeading =
        new(@"^###\s+Soluci[oó]n\s+([A-Za-z])\s*[·:\-–]\s*(.+?)\s*$", RegexOptions.Multiline | RegexOptions.Compiled);

    private static IReadOnlyList<KnowledgeSolution> ParseSolutions(string body)
    {
        var text = body.Replace("\r\n", "\n");
        var headings = SolutionHeading.Matches(text);
        var result = new List<KnowledgeSolution>();

        for (var i = 0; i < headings.Count; i++)
        {
            var start = headings[i].Index;
            var end = i + 1 < headings.Count ? headings[i + 1].Index : text.Length;
            var next = text.IndexOf("\n### ", start + 4, StringComparison.Ordinal);       // otro apartado ### corta la solución
            if (next > 0 && next < end) end = next;

            var block = text[start..end].Trim();
            var title = headings[i].Groups[2].Value;
            var recommended = title.Contains("recomendada", StringComparison.OrdinalIgnoreCase);
            title = Regex.Replace(title, @"\s*\(recomendada\)", string.Empty, RegexOptions.IgnoreCase).Trim();

            var summary = block.Split('\n').Skip(1).FirstOrDefault(l => !string.IsNullOrWhiteSpace(l))?.Trim() ?? title;
            summary = Regex.Replace(summary, @"^\*\*Cuándo conviene:\*\*\s*", string.Empty);
            result.Add(new KnowledgeSolution(headings[i].Groups[1].Value.ToUpperInvariant(), title, recommended, summary, block));
        }
        return result;
    }

    #endregion
}
