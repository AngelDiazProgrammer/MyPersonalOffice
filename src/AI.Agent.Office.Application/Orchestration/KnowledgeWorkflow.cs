using System.Text;
using System.Text.RegularExpressions;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Application.Orchestration;

/// <summary>
/// Lo que el flujo de la base de conocimiento decidió para una solicitud: el plan de tareas, las
/// situaciones candidatas y el formato de entrega.
/// </summary>
public sealed record KnowledgePlan(
    OrchestratorDecision Decision,
    IReadOnlyList<KnowledgeHit> Hits,
    string? FormatName,
    string CandidatesText = "",
    string FormatText = "",
    bool Sensitive = false,
    bool AwaitsChoice = false,
    string ExtraAllowed = "");

/// <summary>
/// Respuesta armada con los resultados del flujo, antes de presentarla (línea de fuente y
/// advertencias).
/// </summary>
public sealed record KnowledgeAnswer(string? Text, KnowledgeEntry Entry, bool NoMatch, string Verdict);

/// <summary>
/// Flujo fijo para las consultas cubiertas por la base de conocimiento. Cada proceso tiene su PROPIO
/// agente exclusivo: 1) el investigador ubica la situación, 2) el respondedor redacta desde ese
/// procedimiento, 3) el redactor la entrega en el formato de entrega y 4) el auditor la contrasta con
/// el procedimiento y el formato. El sistema consulta la base ANTES de responder y verifica por sí
/// mismo el texto final: tickets, formularios, contactos y plazos deben existir en la base.
/// </summary>
public sealed partial class KnowledgeWorkflow
{
    #region Dependencias y estilo del documento

    private readonly IKnowledgeBaseService _knowledge;
    private readonly IDeliveryFormatService _formats;
    private readonly IAgentConfigurationService _agents;
    private readonly IWorkspaceService _workspaces;
    private readonly KnowledgeOptions _options;
    private readonly ILogger<KnowledgeWorkflow> _logger;

    /// <summary>Crea el flujo con la base de conocimiento, el formato de entrega, los agentes y la oficina activa.</summary>
    public KnowledgeWorkflow(
        IKnowledgeBaseService knowledge, IDeliveryFormatService formats, IAgentConfigurationService agents, IWorkspaceService workspaces,
        IOptions<KnowledgeOptions> options, ILogger<KnowledgeWorkflow> logger)
    {
        _knowledge = knowledge;
        _formats = formats;
        _agents = agents;
        _workspaces = workspaces;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>Id del agente auditor (lo usa el orquestador para la tarea de corrección).</summary>
    public string AuditorAgentId => _options.AuditorAgentId;
    /// <summary>Id del agente que entrega la respuesta en el formato (Redactor Word).</summary>
    public string FormatterAgentId => _options.FormatterAgentId;
    /// <summary>Id del agente investigador de la base de conocimiento.</summary>
    public string ResearcherAgentId => _options.ResearcherAgentId;

    /// <summary>
    /// Estilo de los documentos generados: APA si así se configura o, en modo Auto, cuando el formato de
    /// entrega menciona APA.
    /// </summary>
    public DocumentStyle ResolveStyle(KnowledgePlan plan)
    {
        if (string.Equals(_options.DocumentStyle, "Apa", StringComparison.OrdinalIgnoreCase)) return DocumentStyle.Apa;
        if (string.Equals(_options.DocumentStyle, "Default", StringComparison.OrdinalIgnoreCase)) return DocumentStyle.Default;
        return ApaWord().IsMatch(plan.FormatName + " " + plan.FormatText) ? DocumentStyle.Apa : DocumentStyle.Default;
    }

    /// <summary>La palabra APA como término completo.</summary>
    [GeneratedRegex(@"\bAPA\b", RegexOptions.IgnoreCase)]
    private static partial Regex ApaWord();

    #endregion

    #region Planificación del flujo


    /// <summary>
    /// Devuelve el plan fijo cuando el mensaje (o el texto de los PDF adjuntos) coincide con la base de
    /// conocimiento; si no, null y sigue la planificación normal.
    /// </summary>
    public async Task<KnowledgePlan?> TryPlanAsync(string userText, IReadOnlyList<OfficeDocument> documents, CancellationToken cancellationToken)
    {
        if (!_options.Enabled) return null;

        // El caso puede venir en el mensaje, en un documento adjunto (PDF...) o en ambos.
        var docText = string.Join("\n", documents.Select(d => d.Text));
        if (docText.Length > _options.MaxQueryDocumentChars) docText = docText[.._options.MaxQueryDocumentChars];
        var query = documents.Count == 0 ? userText : userText + "\n" + docText;
        var hasDocuments = documents.Count > 0;
        var hits = _knowledge.Search(query, Math.Max(1, _options.MaxCandidates));
        if (hits.Count == 0 || hits[0].Score < _options.MinScore) return null;

        var ids = new[] { _options.ResearcherAgentId, _options.ResponderAgentId, _options.FormatterAgentId, _options.AuditorAgentId };
        var missing = ids.Where(id => _agents.GetDefinition(id) is null || !_workspaces.IsAgentActive(id)).ToList();
        if (missing.Count > 0)
        {
            _logger.LogWarning("La base de conocimiento coincide pero faltan agentes del flujo: {Missing}", string.Join(", ", missing));
            return null;
        }

        var format = await _formats.GetActiveFormatAsync(cancellationToken).ConfigureAwait(false);
        var formatText = format is null
            ? "(No hay un formato de entrega cargado: entrega la respuesta con títulos claros y secciones ordenadas.)"
            : $"## Formato de entrega: {format.FileName}\n{Truncate(format.Text, _options.MaxFormatChars)}";
        formatText += $"\n\nFecha actual (úsala solo si el formato pide fecha): {DateTime.Now:d 'de' MMMM 'de' yyyy}.";

        var candidates = RenderCandidates(hits);
        var sensitive = hits.Any(h => h.Entry.Confidential);
        // El texto del documento va solo a Claude cuando la bandera lo indica (una base confidencial también lo fuerza).
        var docsToClaudeOnly = hasDocuments && !_options.AllowFreeApisWithDocuments;

        // Con documentos adjuntos el Lector PDF va primero: los lee, los analiza y le entrega el caso al investigador.
        var useReader = hasDocuments && _agents.GetDefinition(_options.ReaderAgentId) is not null && _workspaces.IsAgentActive(_options.ReaderAgentId);

        var tasks = new List<PlannedTask>
        {
            new()
            {
                Id = "t1", AgentId = _options.ResearcherAgentId, Title = "Investigar en la base de conocimiento",
                Task = useReader
                    ? "Determina qué situación de la base de conocimiento corresponde al caso resumido por el Lector PDF y extrae su procedimiento."
                    : "Determina qué situación de la base de conocimiento corresponde a la consulta del usuario y extrae su procedimiento.",
                ExtraContext = candidates, Sensitive = sensitive || docsToClaudeOnly,
                IncludeDocuments = hasDocuments && !useReader, AllowFreeDocuments = hasDocuments && !useReader && !docsToClaudeOnly
            },
            new()
            {
                Id = "t2", AgentId = _options.ResponderAgentId, Title = "Redactar la respuesta según el procedimiento",
                Task = "Con la situación identificada por el investigador, redacta la respuesta al usuario siguiendo SOLO ese procedimiento.",
                DependsOn = { "t1" }, ExtraContext = candidates, Sensitive = sensitive || docsToClaudeOnly,
                IncludeDocuments = hasDocuments && !useReader, AllowFreeDocuments = hasDocuments && !useReader && !docsToClaudeOnly
            },
            new()
            {
                Id = "t3", AgentId = _options.FormatterAgentId, Title = "Entregar en el formato requerido",
                Task = "Entrega la respuesta redactada bajo el formato de entrega indicado, sin cambiar su contenido ni agregar datos.",
                DependsOn = { "t2" }, ExtraContext = candidates + "\n\n" + formatText, Sensitive = sensitive, SkipDocument = true
            },
            new()
            {
                Id = "t4", AgentId = _options.AuditorAgentId, Title = "Auditar concordancia con la base de conocimiento",
                Task = "Verifica que la respuesta formateada concuerde con el procedimiento de la base de conocimiento y con el formato.",
                DependsOn = { "t1", "t3" }, ExtraContext = candidates + "\n\n" + formatText, Sensitive = sensitive
            }
        };

        if (useReader)
        {
            tasks.Insert(0, new PlannedTask
            {
                Id = "t0", AgentId = _options.ReaderAgentId, Title = "Leer y analizar el documento del caso",
                Task = "Lee el/los documento(s) adjuntos y entrega al Investigador KB un resumen fiel del caso: qué ocurrió, quién, qué hizo la persona, fechas y datos concretos, y qué se pide. No recomiendes acciones, no inventes y señala lo que el documento no dice.",
                Sensitive = sensitive || docsToClaudeOnly, IncludeDocuments = true, AllowFreeDocuments = !docsToClaudeOnly,
                RetryWithClaude = _options.RetryReaderWithClaude
            });
            tasks.First(t => t.Id == "t1").DependsOn.Add("t0");
        }

        var top = hits[0].Entry;

        // Con opciones: primero solo se investiga el caso; redactar, formatear y auditar espera a la elección de la persona.
        var awaitsChoice = OffersChoice(top);
        if (awaitsChoice) tasks.RemoveAll(t => t.Id is "t2" or "t3" or "t4");
        _logger.LogInformation("Base de conocimiento: {Code} ({Score:0.#}) → {Mode}", top.Code, hits[0].Score,
            awaitsChoice ? "investigación y opciones" : "flujo completo");

        return new KnowledgePlan(
            new OrchestratorDecision
            {
                Action = DecisionActions.Delegate,
                Message = $"Consulté la base de conocimiento: coincide con {top.Code} — {top.Title}. {(useReader ? "El lector PDF analizará el documento y se lo entregará al investigador; luego " : "Lo resolverán, en orden, ")}{(awaitsChoice ? "el investigador te presentará las opciones para resolverlo." : (useReader ? "intervienen " : "") + "el investigador, el respondedor, el redactor Word (formato) y el auditor.")}",
                Tasks = tasks
            },
            hits, format?.FileName, candidates, formatText, sensitive, AwaitsChoice: awaitsChoice);
    }

    #endregion

    #region Composición de la respuesta


    /// <summary>Elige el mejor texto final entre los resultados del flujo (gana el texto corregido por el auditor).</summary>
    public KnowledgeAnswer Compose(KnowledgePlan plan, IReadOnlyList<AgentTask> executed)
    {
        string? ResultOf(string agentId) =>
            executed.FirstOrDefault(t => string.Equals(t.AgentId, agentId, StringComparison.OrdinalIgnoreCase)) is { Status: TaskStatus.Completed, Result: { Length: > 0 } r } ? r : null;

        var research = ResultOf(_options.ResearcherAgentId);
        var draft = ResultOf(_options.ResponderAgentId);
        var formatted = ResultOf(_options.FormatterAgentId);
        var audit = ResultOf(_options.AuditorAgentId);

        // ¿Qué situación eligió el investigador? (si no se entiende, se usa la mejor coincidencia de la búsqueda)
        var code = plan.Hits[0].Entry.Code;
        var none = false;
        if (research is not null && CodeLine().Match(research) is { Success: true } m)
        {
            var found = m.Groups[1].Value.Trim().ToUpperInvariant();
            if (found.StartsWith("NINGUN", StringComparison.Ordinal)) none = true;
            else if (Regex.Match(found, @"[A-Z]{2,6}-\d{1,4}") is { Success: true } c) code = c.Value;
        }
        var entry = plan.Hits.Select(h => h.Entry).FirstOrDefault(e => string.Equals(e.Code, code, StringComparison.OrdinalIgnoreCase)) ?? plan.Hits[0].Entry;

        var verdict = "sin auditar";
        var text = formatted ?? draft;
        if (audit is not null && Verdict().Match(audit) is { Success: true } v)
        {
            var corrected = v.Groups[1].Value.Equals("CORREGIDO", StringComparison.OrdinalIgnoreCase);
            verdict = corrected ? "corregida por el auditor" : "aprobada por el auditor";
            if (corrected)
            {
                if (ExtractCorrectedText(audit) is { } fixedText) text = fixedText;
                else verdict = "con observaciones del auditor (su corrección llegó incompleta: se conserva el texto original)";
            }
        }

        return new KnowledgeAnswer(text, entry, none, verdict);
    }

    /// <summary>Texto que sigue al marcador ---TEXTO--- del resultado del auditor, o null.</summary>
    public static string? ExtractCorrectedText(string auditResult) =>
        // Un texto corregido cortado por el límite de tokens REEMPLAZARÍA una respuesta completa por una truncada: se
        // ignora.
        auditResult.Contains("Respuesta cortada por el límite de tokens", StringComparison.Ordinal) ? null :
        auditResult.Split("---TEXTO---", 2, StringSplitOptions.None) is { Length: 2 } parts && !string.IsNullOrWhiteSpace(parts[1])
            ? parts[1].Trim()
            : null;

    /// <summary>Respuesta final: el texto, una advertencia si algo no se pudo verificar y la línea de fuente.</summary>
    public string Render(KnowledgePlan plan, KnowledgeAnswer answer, string? text, IReadOnlyList<string> unverified, bool repaired)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "No pude completar la respuesta con el procedimiento de la base de conocimiento: el flujo se interrumpió. Revisa el panel de tareas.";

        var sb = new StringBuilder(text.Trim());
        sb.AppendLine().AppendLine().AppendLine("---");
        sb.Append(answer.NoMatch
            ? "> 📚 **Base de conocimiento:** ninguna situación coincide con la consulta."
            : $"> 📚 **Base de conocimiento:** {answer.Entry.Code} — {answer.Entry.Title}");
        sb.Append($" · ✔ Verificación: {answer.Verdict}");
        if (repaired) sb.Append(" · 🔧 datos corregidos automáticamente");
        if (plan.FormatName is not null) sb.Append($" · 🧾 Formato: {plan.FormatName}");

        if (unverified.Count > 0)
        {
            sb.AppendLine().AppendLine();
            sb.Append("> ⚠️ **Datos sin respaldo en la base de conocimiento (revisar antes de usar):** ")
              .Append(string.Join(", ", unverified));
        }
        return sb.ToString();
    }

    #endregion

    #region Normalización del entregable


    /// <summary>
    /// El documento siempre empieza con UNA línea de título (# ...): "Respuesta al incidente {CÓDIGO}:
    /// {título}". Los modelos a veces copian el encabezado del archivo de formato, ponen el título en
    /// negrita o lo repiten; el sistema lo corrige.
    /// </summary>
    public string NormalizeDocument(KnowledgeAnswer answer, string text)
    {
        if (answer.NoMatch || string.IsNullOrWhiteSpace(text)) return text;

        var title = $"Respuesta al incidente {answer.Entry.Code}: {answer.Entry.Title}";
        var lines = text.Replace("\r\n", "\n").Split('\n').ToList();
        var i = 0;
        while (i < lines.Count && string.IsNullOrWhiteSpace(lines[i])) i++;
        if (i >= lines.Count) return text;

        var first = lines[i].Trim();
        var isMeta = first.StartsWith("**Autor", StringComparison.OrdinalIgnoreCase) || first.StartsWith("**Fecha", StringComparison.OrdinalIgnoreCase);
        var isTitleLine = first.StartsWith("# ", StringComparison.Ordinal) || (BoldLine().IsMatch(first) && !isMeta);

        if (isTitleLine) lines[i] = "# " + title;
        else lines.Insert(i, "# " + title + "\n");

        return string.Join("\n", lines).Trim();
    }

    /// <summary>Línea que es solo texto en negrita (posible título mal formateado).</summary>
    [GeneratedRegex(@"^\*\*[^*]+\*\*(\s*[—–-].*)?$")]
    private static partial Regex BoldLine();

    #endregion

    #region Verificación automática de datos


    /// <summary>
    /// Identificadores (tickets, formularios, contactos, extensiones) y plazos del texto que NO existen en
    /// las situaciones candidatas. Los modelos tienden a inventar números de ticket verosímiles; esto los
    /// detecta.
    /// </summary>
    public IReadOnlyList<string> FindUnverified(KnowledgePlan plan, string text)
    {
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var hit in plan.Hits)
            foreach (var key in ExtractFacts(hit.Entry.Body).Keys) allowed.Add(key);
        // Los datos que la propia persona escribió en su solución no cuentan como inventados.
        if (plan.ExtraAllowed.Length > 0)
            foreach (var key in ExtractFacts(plan.ExtraAllowed).Keys) allowed.Add(key);

        return ExtractFacts(text)
            .Where(f => !allowed.Contains(f.Key))
            .Select(f => f.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>Tarea para el auditor que corrige los datos listados y devuelve el documento completo corregido.</summary>
    public PlannedTask BuildRepairTask(KnowledgePlan plan, string currentText, IReadOnlyList<string> unverified)
    {
        var findings = string.Join("\n", unverified.Select(u => $"- {u}"));
        return new PlannedTask
        {
            Id = "t5",
            AgentId = _options.AuditorAgentId,
            Title = "Corregir datos sin respaldo en la base",
            Task = "La verificación automática encontró datos que NO existen en la base de conocimiento. Devuelve la respuesta completa corregida: " +
                   "elimina esos datos o sustitúyelos por los reales de la base, y no agregues ninguno nuevo. Responde SIEMPRE con " +
                   "'VEREDICTO: CORREGIDO', una línea '---TEXTO---' y el documento completo.",
            ExtraContext = plan.CandidatesText + "\n\n" + plan.FormatText +
                           "\n\n## Hallazgos automáticos (obligatorio corregir)\nEstos datos aparecen en la respuesta pero no existen en la base:\n" + findings +
                           "\n\n## Respuesta actual a corregir\n" + currentText,
            Sensitive = plan.Sensitive
        };
    }

    /// <summary>
    /// Hechos verificables de un texto (identificadores, correos, extensiones y plazos): hecho normalizado
    /// → texto original.
    /// </summary>
    public static IReadOnlyDictionary<string, string> ExtractFacts(string text)
    {
        var facts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var t = Dashes().Replace(text, "-");

        // Identificadores: SEG-CRED, FOR-ACC-02, 01-800-NEXORA, TI-REQ-SEG-005 (mayúsculas y dígitos, con alguna letra).
        foreach (Match m in Identifier().Matches(t))
        {
            var token = m.Value;
            if (!token.Any(char.IsLetter) || token != token.ToUpperInvariant()) continue;
            facts[token] = token;
        }

        foreach (Match m in Email().Matches(t)) facts[m.Value.ToLowerInvariant()] = m.Value;
        foreach (Match m in Extension().Matches(t)) facts["ext" + m.Groups[1].Value] = $"ext. {m.Groups[1].Value}";

        // Plazos, comparados en minutos para que "1 hora" equivalga a "60 minutos".
        foreach (Match m in Duration().Matches(t))
        {
            var minutes = Minutes(int.Parse(m.Groups[1].Value), m.Groups[2].Value);
            facts["t:" + minutes] = m.Value.Trim();
        }
        foreach (Match m in WordDuration().Matches(t))
            facts["t:" + Minutes(1, m.Groups[1].Value)] = m.Value.Trim();

        return facts;
    }

    /// <summary>Convierte una duración (minutos, horas o días) a minutos.</summary>
    private static int Minutes(int amount, string unit) =>
        unit.StartsWith("min", StringComparison.OrdinalIgnoreCase) ? amount
        : unit.StartsWith("hor", StringComparison.OrdinalIgnoreCase) ? amount * 60
        : amount * 1440;

    /// <summary>
    /// Texto con las situaciones candidatas y los datos permitidos de cada una, que se entrega a los
    /// agentes como contexto.
    /// </summary>
    private static string RenderCandidates(IReadOnlyList<KnowledgeHit> hits)
    {
        var sb = new StringBuilder("## Base de conocimiento (situaciones candidatas)\n");
        for (var i = 0; i < hits.Count; i++)
        {
            sb.AppendLine().AppendLine($"### Candidato {i + 1} (afinidad {hits[i].Score:0.#})");
            sb.AppendLine(hits[i].Entry.Body.Trim());
            var facts = ExtractFacts(hits[i].Entry.Body).Values.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(f => f, StringComparer.OrdinalIgnoreCase);
            sb.AppendLine().AppendLine($"**Identificadores, contactos y plazos PERMITIDOS para {hits[i].Entry.Code}:** {string.Join(", ", facts)}.");
        }
        sb.AppendLine().AppendLine("REGLA ESTRICTA: usa únicamente los identificadores, contactos y plazos permitidos. Si un paso no tiene ticket, formulario o plazo en la base, NO inventes uno: déjalo sin él.");
        return sb.ToString();
    }

    /// <summary>Recorta el texto del formato de entrega al máximo permitido.</summary>
    private static string Truncate(string text, int max) => text.Length <= max ? text : text[..max] + "\n[…formato recortado…]";

    /// <summary>Guiones tipográficos que se normalizan a "-".</summary>
    [GeneratedRegex("[‐‑‒–—−]")]
    private static partial Regex Dashes();

    /// <summary>Identificadores con guiones (tickets y formularios).</summary>
    [GeneratedRegex(@"\b[A-Za-z0-9]{2,8}(?:-[A-Za-z0-9]{1,8})+\b")]
    private static partial Regex Identifier();

    /// <summary>Direcciones de correo.</summary>
    [GeneratedRegex(@"[A-Za-z0-9._+-]+@[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)+")]
    private static partial Regex Email();

    /// <summary>Extensiones telefónicas ("ext. 4400").</summary>
    [GeneratedRegex(@"\bext\.?\s*(\d{3,5})\b", RegexOptions.IgnoreCase)]
    private static partial Regex Extension();

    /// <summary>Plazos numéricos ("30 minutos", "2 horas").</summary>
    [GeneratedRegex(@"\b(\d{1,3})\s*(minutos?|horas?|d[ií]as?)\b", RegexOptions.IgnoreCase)]
    private static partial Regex Duration();

    /// <summary>Plazos en palabras ("una hora").</summary>
    [GeneratedRegex(@"\b(?:una|un)\s+(hora|d[ií]a|minuto)\b", RegexOptions.IgnoreCase)]
    private static partial Regex WordDuration();

    /// <summary>Línea "CÓDIGO: ..." del resultado del investigador.</summary>
    [GeneratedRegex(@"C[ÓO]DIGO\s*:\s*\**\s*([^\r\n|]+)", RegexOptions.IgnoreCase)]
    private static partial Regex CodeLine();

    /// <summary>Línea "VEREDICTO: APROBADO/CORREGIDO" del resultado del auditor.</summary>
    [GeneratedRegex(@"VEREDICTO\s*:\s*\**\s*(APROBADO|CORREGIDO)", RegexOptions.IgnoreCase)]
    private static partial Regex Verdict();
    #endregion

}
