using System.Text;
using AI.Agent.Office.Application.Agents;
using AI.Agent.Office.Domain.Entities;

namespace AI.Agent.Office.Application.Orchestration;

/// <summary>Construye los prompts. Son deliberadamente compactos: cada carácter se paga en cada llamada.</summary>
public static class PromptBuilder
{
    /// <summary>Instrucciones del Orquestador para planificar: reglas de delegación y el equipo disponible.</summary>
    public static string BuildOrchestratorSystem(
        AgentDefinition orchestrator,
        IEnumerable<AgentDefinition> team,
        int maxTasks,
        bool hasDocuments)
    {
        var sb = new StringBuilder();
        sb.AppendLine(orchestrator.Instructions.Trim());
        sb.AppendLine();
        sb.AppendLine("## Equipo");
        foreach (var a in team)
            sb.AppendLine($"- `{a.Id}`: {a.Role}. {a.Description}");
        sb.AppendLine();
        sb.AppendLine("## Protocolo");
        sb.AppendLine($"- Responde SOLO llamando a `{DecisionSchema.ToolName}`.");
        sb.AppendLine("- respond: contestas tú (message = respuesta final en Markdown). delegate: asignas tareas (message = plan en 1-2 frases).");
        sb.AppendLine("- Si la solicitud encaja con la especialidad de un agente, DEBES delegar.");
        sb.AppendLine($"- Máximo {maxTasks} tareas y solo las necesarias. Independientes = paralelo; usa dependsOn solo si una necesita el resultado de otra.");
        sb.AppendLine("- Cada task es autocontenida pero BREVE (1-3 frases): el agente solo ve su tarea, sus dependencias y la solicitud. Los agentes no delegan.");
        if (hasDocuments)
            sb.AppendLine("- Hay documentos adjuntos: asigna su lectura a un agente que acepte documentos con includeDocuments=true.");
        sb.AppendLine("- Idioma del usuario.");
        return sb.ToString();
    }

    /// <summary>Instrucciones del Orquestador para la síntesis final.</summary>
    public static string BuildConsolidationSystem(AgentDefinition orchestrator)
    {
        var sb = new StringBuilder();
        sb.AppendLine(orchestrator.Instructions.Trim());
        sb.AppendLine();
        sb.AppendLine("## Fase: consolidación");
        sb.AppendLine("Escribe SOLO una síntesis breve (máx. ~250 palabras): qué aportó cada agente en una línea, inconsistencias entre resultados y siguiente paso recomendado.");
        sb.AppendLine("NO copies ni reescribas los resultados: el sistema los adjunta completos debajo de tu síntesis.");
        sb.AppendLine("Si una tarea falló o se canceló, dilo. Si un resultado dice que fue cortado por límite de tokens, indícalo. Markdown, idioma del usuario.");
        return sb.ToString();
    }

    /// <summary>Instrucciones de un especialista (las de su archivo Markdown más reglas comunes).</summary>
    public static string BuildAgentSystem(AgentDefinition agent)
    {
        var sb = new StringBuilder();
        sb.AppendLine(agent.Instructions.Trim());
        sb.AppendLine();
        sb.AppendLine($"Eres {agent.Name} en AI Agent Office; entregas tu resultado al Orquestador. No ejecutas comandos ni delegas.");
        sb.AppendLine("Sé directo y conciso: ve al grano, sin introducciones ni cierres de cortesía, salvo que pidan detalle. Markdown, idioma del usuario.");
        return sb.ToString();
    }

    /// <summary>
    /// Mensaje que recibe un agente: la solicitud, su tarea, el contexto extra, los resultados de sus
    /// dependencias y los documentos.
    /// </summary>
    public static string BuildAgentPrompt(
        string userRequest,
        AgentTask task,
        IReadOnlyList<(AgentTask Task, string AgentName)> dependencyResults,
        IReadOnlyList<OfficeDocument> documents,
        int maxDocumentChars,
        int maxDependencyChars = 6000)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Solicitud del usuario");
        sb.AppendLine(userRequest);
        sb.AppendLine();
        sb.AppendLine($"## Tu tarea: {task.Title}");
        sb.AppendLine(task.Description);

        if (!string.IsNullOrWhiteSpace(task.ExtraContext))
        {
            sb.AppendLine();
            sb.AppendLine(task.ExtraContext);
        }

        if (dependencyResults.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Resultados previos");
            foreach (var (dep, name) in dependencyResults)
            {
                sb.AppendLine($"### {dep.Key} · {name}: {dep.Title}");
                sb.AppendLine(Cut(dep.Result, maxDependencyChars));
                sb.AppendLine();
            }
        }

        if (documents.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Documentos");
            var budget = maxDocumentChars;
            foreach (var doc in documents)
            {
                sb.AppendLine($"### {doc.FileName} ({doc.PageCount} págs.)");
                if (doc.RequiresOcr)
                {
                    sb.AppendLine("[PDF sin texto extraíble: parece escaneado y requiere OCR. Informa de ello al usuario.]");
                    continue;
                }

                var text = doc.Text.Length <= budget ? doc.Text : doc.Text[..Math.Max(0, budget)];
                var cut = text.Length < doc.Text.Length || doc.Truncated;
                budget -= text.Length;
                sb.AppendLine("<documento>");
                sb.AppendLine(text);
                sb.AppendLine("</documento>");
                if (cut) sb.AppendLine("[Documento truncado por tamaño: solo se analizó la parte inicial.]");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>Mensaje de la síntesis final con los resultados de los agentes, recortados al máximo permitido.</summary>
    public static string BuildConsolidationPrompt(string userRequest, IReadOnlyList<(AgentTask Task, string AgentName)> results, int maxChars)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Solicitud del usuario");
        sb.AppendLine(userRequest);
        sb.AppendLine();
        sb.AppendLine("## Resultados");
        var perTask = Math.Max(1000, maxChars / Math.Max(1, results.Count));
        foreach (var (task, name) in results)
        {
            sb.AppendLine($"### {task.Key} · {name}: {task.Title} [{task.Status}]");
            sb.AppendLine(task.Status == TaskStatus.Completed
                ? Cut(task.Result, perTask)
                : $"(Sin resultado: {task.ErrorMessage ?? task.Status.ToString()})");
            sb.AppendLine();
        }
        sb.AppendLine("Redacta tu síntesis.");
        return sb.ToString();
    }

    /// <summary>Recorta un texto a un máximo de caracteres.</summary>
    private static string Cut(string? text, int max)
    {
        text ??= string.Empty;
        return text.Length <= max ? text : text[..max] + "\n[…recortado…]";
    }
}
