using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Domain.Entities;

namespace AI.Agent.Office.Application.Orchestration;

#region Estado pendiente de la conversación

/// <summary>Caso ya investigado que espera la decisión de la persona (qué solución aplicar).</summary>
/// <param name="Plan">Plan de la fase 1 (formato de entrega, candidatas, sensibilidad).</param>
/// <param name="Entry">Situación de la base elegida por el investigador.</param>
/// <param name="Research">Resultado completo del investigador (hechos del caso).</param>
/// <param name="UserText">Mensaje original de la persona.</param>
public sealed record PendingKnowledgeCase(KnowledgePlan Plan, KnowledgeEntry Entry, string Research, string UserText);

/// <summary>Guarda, por conversación, el caso que espera una elección (solo en memoria).</summary>
public interface IPendingChoiceStore
{
    void Set(Guid conversationId, PendingKnowledgeCase pending);
    PendingKnowledgeCase? Get(Guid conversationId);
    void Clear(Guid conversationId);
}

/// <summary>Implementación en memoria de <see cref="IPendingChoiceStore"/>.</summary>
public sealed class PendingChoiceStore : IPendingChoiceStore
{
    /// <summary>Máximo de casos pendientes en memoria y tiempo que se conservan.</summary>
    public const int MaxPending = 200;
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(6);

    private readonly ConcurrentDictionary<Guid, (PendingKnowledgeCase Case, DateTimeOffset Created)> _items = new();

    public void Set(Guid conversationId, PendingKnowledgeCase pending)
    {
        _items[conversationId] = (pending, DateTimeOffset.UtcNow);
        if (_items.Count <= MaxPending) return;

        // Limpieza: vencidos primero y, si aún sobran, los más antiguos.
        foreach (var old in _items.OrderBy(i => i.Value.Created).Take(_items.Count - MaxPending).ToList()) _items.TryRemove(old.Key, out _);
    }

    public PendingKnowledgeCase? Get(Guid conversationId)
    {
        if (!_items.TryGetValue(conversationId, out var entry)) return null;
        if (DateTimeOffset.UtcNow - entry.Created <= Lifetime) return entry.Case;
        _items.TryRemove(conversationId, out _);
        return null;
    }

    public void Clear(Guid conversationId) => _items.TryRemove(conversationId, out _);
}

#endregion

#region Interpretación de la respuesta de la persona

public enum ChoiceKind
{
    /// <summary>Eligió una de las soluciones de la base.</summary>
    Option,
    /// <summary>Escribió su propia solución.</summary>
    Custom,
    /// <summary>Quiere cerrar el caso sin aplicar nada.</summary>
    Cancel,
    /// <summary>No se entendió: hay que volver a preguntar.</summary>
    Ask,
    /// <summary>El mensaje trata de otro caso distinto del pendiente.</summary>
    NewCase
}

public sealed record ChoiceResolution(ChoiceKind Kind, KnowledgeSolution? Solution = null, string? CustomText = null);

#endregion

public sealed partial class KnowledgeWorkflow
{
    #region Presentar las opciones (fase 1)

    /// <summary>True si la situación tiene suficientes soluciones y la opción de elegir está activada.</summary>
    public bool OffersChoice(KnowledgeEntry entry) =>
        _options.OfferSolutionChoice && entry.Solutions.Count >= Math.Max(2, _options.MinSolutionsForChoice);

    public PendingKnowledgeCase PendingFrom(KnowledgePlan plan, KnowledgeAnswer answer, string research, string userText) =>
        new(plan, answer.Entry, research, userText);

    /// <summary>Opciones para la interfaz (botones).</summary>
    public KnowledgeChoiceDto BuildChoice(KnowledgeEntry entry)
    {
        var options = entry.Solutions.Select(s => new SolutionOptionDto(s.Id, s.Title, s.Summary, s.Recommended)).ToArray();
        var recommended = entry.Solutions.FirstOrDefault(s => s.Recommended)?.Id ?? entry.Solutions[0].Id;
        return new KnowledgeChoiceDto(entry.Code, entry.Title, recommended, options);
    }

    /// <summary>Mensaje del chat: caso identificado, lo entendido y las opciones. Lo arma el sistema (no un modelo) para no inventar datos.</summary>
    public string RenderChoice(KnowledgeEntry entry, string research)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"📚 **Caso identificado: {entry.Code} — {entry.Title}**");
        if (!string.IsNullOrWhiteSpace(entry.Severity)) sb.AppendLine($"Severidad: {entry.Severity}");

        var facts = ExtractSection(research, "HECHOS DEL USUARIO");
        if (!string.IsNullOrWhiteSpace(facts))
            sb.AppendLine().AppendLine("**Lo que entendí de tu caso:**").AppendLine(facts);

        sb.AppendLine().AppendLine("**Opciones para resolverlo (según la base de conocimiento):**");
        foreach (var s in entry.Solutions)
            sb.AppendLine($"- **{s.Id} · {s.Title}**{(s.Recommended ? " ⭐ recomendada" : string.Empty)} — {s.Summary}");

        sb.AppendLine().Append("¿Cómo quieres manejarlo? Elige una opción, o cuéntame tu propia solución y la reviso contra la base.");
        return sb.ToString();
    }

    /// <summary>Texto de una sección del resultado del investigador (por ejemplo "HECHOS DEL USUARIO:") hasta la siguiente.</summary>
    private static string ExtractSection(string text, string title)
    {
        var match = Regex.Match(text,
            @"\*{0,2}" + Regex.Escape(title) + @":?\*{0,2}\s*(.*?)(?=\n\s*\*{0,2}[A-ZÁÉÍÓÚÑ ]{4,}:|\z)",
            RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    public string AskAgainMessage(PendingKnowledgeCase pending) =>
        $"No entendí cuál opción quieres. Elige una letra ({string.Join(", ", pending.Entry.Solutions.Select(s => s.Id))}), " +
        "escribe tu propia solución con algo más de detalle, o di \"cancelar\" para cerrar el caso.";

    #endregion

    #region Interpretar la elección (fase 2)

    /// <summary>Mensaje que es solo una letra de opción ("B", "opción b", "solución C.").</summary>
    private static readonly Regex LetterOnly =
        new(@"^\s*(?:la\s+)?(?:opci[oó]n|soluci[oó]n)?\s*([A-Za-z])\s*[.)]?\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Frases que significan "quiero la solución recomendada".</summary>
    private static readonly Regex RecommendedWords =
        new(@"\b(la\s+recomendada|recomendada|la\s+primera|la\s+est[aá]ndar|la\s+de\s+siempre|como\s+dices|de\s+acuerdo|s[ií],?\s*dale)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Frases que significan "cierra el caso".</summary>
    private static readonly Regex CancelWords =
        new(@"\b(cancel\w*|ninguna|no\s+gracias|d[eé]jalo\s+as[ií]|olv[ií]dalo)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Decide qué quiso la persona: un botón (choiceId), una letra, "la recomendada", cancelar, otro caso distinto o una solución propia.
    /// Es determinista: no se pregunta a un modelo cuál paso sigue.
    /// </summary>
    public async Task<ChoiceResolution> ResolveChoiceAsync(
        string text, string? choiceId, int attachedDocuments, PendingKnowledgeCase pending, CancellationToken cancellationToken)
    {
        var solutions = pending.Entry.Solutions;
        KnowledgeSolution? ById(string id) => solutions.FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));
        KnowledgeSolution Recommended() => solutions.FirstOrDefault(s => s.Recommended) ?? solutions[0];

        if (!string.IsNullOrWhiteSpace(choiceId))
        {
            if (string.Equals(choiceId, "cancel", StringComparison.OrdinalIgnoreCase)) return new ChoiceResolution(ChoiceKind.Cancel);
            return ById(choiceId) is { } byButton ? new ChoiceResolution(ChoiceKind.Option, byButton) : new ChoiceResolution(ChoiceKind.Ask);
        }

        if (attachedDocuments > 0) return new ChoiceResolution(ChoiceKind.NewCase);

        var letter = LetterOnly.Match(text);
        if (letter.Success && ById(letter.Groups[1].Value) is { } byLetter) return new ChoiceResolution(ChoiceKind.Option, byLetter);
        if (CancelWords.IsMatch(text) && text.Length < 40) return new ChoiceResolution(ChoiceKind.Cancel);
        if (RecommendedWords.IsMatch(text) && text.Length < 40) return new ChoiceResolution(ChoiceKind.Option, Recommended());

        // ¿El mensaje trata de otra situación de la base? Entonces es un caso nuevo.
        var other = await TryPlanAsync(text, Array.Empty<OfficeDocument>(), cancellationToken).ConfigureAwait(false);
        if (other is not null && !string.Equals(other.Hits[0].Entry.Code, pending.Entry.Code, StringComparison.OrdinalIgnoreCase))
            return new ChoiceResolution(ChoiceKind.NewCase);

        return text.Trim().Length >= 20 ? new ChoiceResolution(ChoiceKind.Custom, CustomText: text.Trim()) : new ChoiceResolution(ChoiceKind.Ask);
    }

    #endregion

    #region Plan de entrega (fase 2)

    /// <summary>
    /// Tareas que faltan tras la elección: el respondedor redacta SOLO esa solución (o evalúa la propia de la persona),
    /// el redactor la entrega en el formato y el auditor la verifica contra la base.
    /// </summary>
    public KnowledgePlan BuildDeliveryPlan(PendingKnowledgeCase pending, KnowledgeSolution? solution, string? customText)
    {
        var entry = pending.Entry;
        var hits = new List<KnowledgeHit> { new(entry, 100) };
        // A los agentes se les muestra el procedimiento base SIN las demás soluciones: solo la elegida (así no se mezclan).
        var baseBody = entry.Solutions.Count > 0 ? entry.Body[..entry.Body.IndexOf("### Solución", StringComparison.OrdinalIgnoreCase)].TrimEnd() : entry.Body;
        var visible = new KnowledgeEntry { Code = entry.Code, Title = entry.Title, Severity = entry.Severity, Keywords = entry.Keywords, Body = baseBody, SourceFile = entry.SourceFile, Confidential = entry.Confidential };
        var candidates = RenderCandidates(new List<KnowledgeHit> { new(visible, 100) });
        var formatText = pending.Plan.FormatText;
        var sensitive = pending.Plan.Sensitive;

        var chosen = new StringBuilder();
        chosen.AppendLine("## Resumen del caso (investigador)").AppendLine(pending.Research.Trim()).AppendLine();
        string task, message;
        if (solution is not null)
        {
            chosen.AppendLine($"## Solución elegida por la persona: {solution.Id} · {solution.Title}").AppendLine(solution.Body);
            var allowed = ExtractFacts(solution.Body).Values.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(f => f, StringComparer.OrdinalIgnoreCase);
            chosen.AppendLine().AppendLine($"**Identificadores, contactos y plazos PERMITIDOS en esta solución:** {string.Join(", ", allowed)}.");
            task = $"La persona eligió la solución {solution.Id} ({solution.Title}). Redacta la respuesta siguiendo SOLO esa solución.";
            message = $"Preparo la solución elegida: {solution.Id} · {solution.Title}.";
        }
        else if (customText is not null)
        {
            chosen.AppendLine("## Solución propuesta por la persona").AppendLine(customText);
            task = "La persona propone su propia solución. Redáctala como respuesta, dejando claro qué partes cumplen la norma de la base y qué debe ajustarse; no inventes datos.";
            message = "Reviso tu solución contra la base de conocimiento.";
        }
        else
        {
            task = "Con la situación identificada, redacta la respuesta siguiendo SOLO el procedimiento de la base.";
            message = $"Preparo la respuesta para {entry.Code}.";
        }

        var tasks = new List<PlannedTask>
        {
            new()
            {
                Id = "t1", AgentId = _options.ResponderAgentId, Title = solution is null && customText is null ? "Redactar la respuesta según el procedimiento" : "Redactar la solución elegida",
                Task = task, ExtraContext = candidates + "\n\n" + chosen, Sensitive = sensitive
            },
            new()
            {
                Id = "t2", AgentId = _options.FormatterAgentId, Title = "Entregar en el formato requerido",
                Task = "Entrega la respuesta redactada bajo el formato de entrega indicado, sin cambiar su contenido ni agregar datos.",
                DependsOn = { "t1" }, ExtraContext = candidates + "\n\n" + formatText, Sensitive = sensitive, SkipDocument = true
            },
            new()
            {
                Id = "t3", AgentId = _options.AuditorAgentId, Title = "Auditar concordancia con la base de conocimiento",
                Task = customText is null
                    ? "Verifica que la respuesta formateada concuerde con la solución de la base de conocimiento y con el formato."
                    : "La solución es propia de la persona: verifica que NO contradiga ninguna norma de la base; si contradice, corrígela hacia la solución más cercana de la base y lista los hallazgos.",
                DependsOn = { "t2" }, ExtraContext = candidates + "\n\n" + chosen + "\n\n" + formatText, Sensitive = sensitive
            }
        };

        return new KnowledgePlan(
            new OrchestratorDecision { Action = DecisionActions.Delegate, Message = message, Tasks = tasks },
            hits, pending.Plan.FormatName, candidates, formatText, sensitive,
            AwaitsChoice: false, ExtraAllowed: customText ?? string.Empty);
    }

    #endregion
}
