using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Orchestration;

namespace AI.Agent.Office.Application.Validators;

/// <summary>
/// Valida el mensaje del chat: que no esté vacío, que no exceda el máximo y que los documentos adjuntos
/// sean válidos.
/// </summary>
public static class ChatRequestValidator
{
    public const int MaxDocuments = 3;

    /// <summary>Devuelve la lista de problemas del mensaje; vacía significa válido.</summary>
    public static IReadOnlyList<string> Validate(ChatRequestDto? request, int maxMessageChars)
    {
        var errors = new List<string>();
        if (request is null)
        {
            errors.Add("La solicitud está vacía.");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(request.Message)) errors.Add("El mensaje no puede estar vacío.");
        else if (request.Message.Length > maxMessageChars) errors.Add($"El mensaje supera el máximo de {maxMessageChars} caracteres.");

        if (request.DocumentIds is { Count: > MaxDocuments }) errors.Add($"Se permiten como máximo {MaxDocuments} documentos por mensaje.");
        return errors;
    }
}

/// <summary>
/// Valida la decisión del Orquestador: acción permitida, agentes existentes, tareas completas,
/// dependencias sin ciclos.
/// </summary>
public static class OrchestratorDecisionValidator
{
    /// <summary>Minúsculas, sin acentos y solo letras y dígitos: "Analista QA" → "analistaqa".</summary>
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var sb = new System.Text.StringBuilder();
        foreach (var ch in value.Normalize(System.Text.NormalizationForm.FormD))
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
        }
        return sb.ToString();
    }

    /// <summary>
    /// Los modelos gratuitos suelen responder con un nombre en vez del id ("Profesor", "agente teacher").
    /// Lo traduce al id real cuando no hay ambigüedad; si no, devuelve el valor tal cual para que la
    /// validación lo reporte.
    /// </summary>
    public static string ResolveAgentId(string? value, IReadOnlyDictionary<string, string> aliases)
    {
        var key = Normalize(value);
        if (key.Length == 0) return value ?? string.Empty;
        if (aliases.TryGetValue(key, out var exact)) return exact;

        var partial = aliases.Where(a => a.Key.Length >= 4 && (key.Contains(a.Key) || a.Key.Contains(key) && key.Length >= 4))
            .Select(a => a.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        return partial.Count == 1 ? partial[0] : value ?? string.Empty;
    }

    /// <summary>Valida (y normaliza los ids de) una decisión. Devuelve la lista de problemas; vacía significa válida.</summary>
    public static IReadOnlyList<string> Validate(
        OrchestratorDecision? decision,
        IReadOnlyCollection<string> delegableAgentIds,
        int maxTasks,
        IReadOnlyDictionary<string, string>? aliases = null)
    {
        var errors = new List<string>();
        if (decision is null)
        {
            errors.Add("La decisión no es un JSON válido.");
            return errors;
        }

        var action = decision.Action?.Trim().ToLowerInvariant();
        if (action != DecisionActions.Respond && action != DecisionActions.Delegate)
        {
            errors.Add($"action debe ser '{DecisionActions.Respond}' o '{DecisionActions.Delegate}'.");
            return errors;
        }

        decision.Action = action;
        decision.Tasks ??= new();

        if (action == DecisionActions.Respond)
        {
            if (string.IsNullOrWhiteSpace(decision.Message)) errors.Add("Con action=respond, message no puede estar vacío.");
            return errors;
        }

        if (decision.Tasks.Count == 0) errors.Add("Con action=delegate debe existir al menos una tarea.");
        if (decision.Tasks.Count > maxTasks) errors.Add($"Se permiten como máximo {maxTasks} tareas por solicitud.");

        var agentSet = new HashSet<string>(delegableAgentIds, StringComparer.OrdinalIgnoreCase);
        var index = 1;
        foreach (var t in decision.Tasks)
        {
            if (string.IsNullOrWhiteSpace(t.Id)) t.Id = $"t{index}";
            t.Id = t.Id.Trim();
            t.DependsOn ??= new();
            if (aliases is not null) t.AgentId = ResolveAgentId(t.AgentId, aliases);
            index++;

            if (!agentSet.Contains(t.AgentId ?? string.Empty))
                errors.Add($"La tarea '{t.Id}' usa un agente no válido: '{t.AgentId}'. Válidos: {string.Join(", ", agentSet)}.");
            if (string.IsNullOrWhiteSpace(t.Task)) errors.Add($"La tarea '{t.Id}' no tiene instrucciones.");
            if (string.IsNullOrWhiteSpace(t.Title)) t.Title = Truncate(t.Task, 60);
            if (t.DependsOn.Any(d => string.Equals(d, t.Id, StringComparison.OrdinalIgnoreCase)))
                errors.Add($"La tarea '{t.Id}' depende de sí misma.");
        }

        if (errors.Count == 0 &&
            PlanGraph.TopologicalOrder(decision.Tasks, t => t.Id, t => t.DependsOn) is null)
        {
            errors.Add("Los ids de las tareas deben ser únicos y sus dependencias deben existir y no formar ciclos.");
        }

        return errors;
    }

    /// <summary>Recorta un texto para incluirlo en un mensaje de error.</summary>
    private static string Truncate(string? text, int max) =>
        string.IsNullOrWhiteSpace(text) ? "Tarea" : text.Length <= max ? text.Trim() : text[..max].Trim() + "…";
}
