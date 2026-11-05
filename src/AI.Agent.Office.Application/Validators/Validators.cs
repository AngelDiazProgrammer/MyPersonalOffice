using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Orchestration;

namespace AI.Agent.Office.Application.Validators;

public static class ChatRequestValidator
{
    public const int MaxDocuments = 3;

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

public static class OrchestratorDecisionValidator
{
    /// <summary>
    /// Validates (and normalizes ids of) a decision. Returns the list of problems; empty means valid.
    /// </summary>
    public static IReadOnlyList<string> Validate(
        OrchestratorDecision? decision,
        IReadOnlyCollection<string> delegableAgentIds,
        int maxTasks)
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

    private static string Truncate(string? text, int max) =>
        string.IsNullOrWhiteSpace(text) ? "Tarea" : text.Length <= max ? text.Trim() : text[..max].Trim() + "…";
}
