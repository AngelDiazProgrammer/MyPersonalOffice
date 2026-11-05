using System.Text;
using AI.Agent.Office.Application.Agents;
using AI.Agent.Office.Domain.Entities;

namespace AI.Agent.Office.Application.Orchestration;

public static class PromptBuilder
{
    public static string BuildOrchestratorSystem(
        AgentDefinition orchestrator,
        IEnumerable<AgentDefinition> team,
        int maxTasks,
        bool hasDocuments)
    {
        var sb = new StringBuilder();
        sb.AppendLine(orchestrator.Instructions.Trim());
        sb.AppendLine();
        sb.AppendLine("## Equipo disponible para delegar");
        foreach (var a in team)
            sb.AppendLine($"- `{a.Id}` — {a.Name} ({a.Role}): {a.Description}");
        sb.AppendLine();
        sb.AppendLine("## Protocolo del sistema (obligatorio)");
        sb.AppendLine($"- Responde SIEMPRE llamando a la herramienta `{DecisionSchema.ToolName}`; no escribas texto fuera de ella.");
        sb.AppendLine("- `action=respond`: contestas tú mismo (saludos, preguntas simples, aclaraciones). `message` es la respuesta final en Markdown.");
        sb.AppendLine("- `action=delegate`: asignas tareas. `message` explica el plan en una o dos frases.");
        sb.AppendLine($"- Máximo {maxTasks} tareas. No delegues si no aporta valor. No inventes agentes.");
        sb.AppendLine("- Tareas independientes se ejecutan en paralelo. Usa `dependsOn` solo si una tarea necesita el resultado de otra.");
        sb.AppendLine("- Cada `task` debe ser autocontenida: el agente solo ve su tarea, el resultado de sus dependencias y la solicitud del usuario.");
        sb.AppendLine("- Los agentes no pueden delegar entre sí ni ejecutar comandos; tú coordinas.");
        if (hasDocuments)
            sb.AppendLine("- Hay documentos adjuntos: asigna su lectura a un agente que acepte documentos y marca `includeDocuments=true`.");
        sb.AppendLine("- Responde en el idioma del usuario.");
        return sb.ToString();
    }

    public static string BuildConsolidationSystem(AgentDefinition orchestrator)
    {
        var sb = new StringBuilder();
        sb.AppendLine(orchestrator.Instructions.Trim());
        sb.AppendLine();
        sb.AppendLine("## Fase actual: consolidación");
        sb.AppendLine("Los agentes ya terminaron. Redacta la respuesta final para el usuario integrando sus resultados.");
        sb.AppendLine("- No menciones detalles internos innecesarios; sí indica quién aportó qué cuando ayude.");
        sb.AppendLine("- Si alguna tarea falló o fue cancelada, dilo con claridad y propone el siguiente paso.");
        sb.AppendLine("- Si detectas inconsistencias entre resultados, señálalas.");
        sb.AppendLine("- Usa Markdown. Responde en el idioma del usuario.");
        return sb.ToString();
    }

    public static string BuildAgentSystem(AgentDefinition agent)
    {
        var sb = new StringBuilder();
        sb.AppendLine(agent.Instructions.Trim());
        sb.AppendLine();
        sb.AppendLine("## Contexto de ejecución");
        sb.AppendLine($"Eres **{agent.Name}** dentro de AI Agent Office. Recibes una tarea del Orquestador y devuelves tu resultado a él.");
        sb.AppendLine("- No puedes ejecutar comandos ni delegar en otros agentes.");
        sb.AppendLine("- Responde en el idioma del usuario, en Markdown, de forma completa y accionable.");
        return sb.ToString();
    }

    public static string BuildAgentPrompt(
        string userRequest,
        AgentTask task,
        IReadOnlyList<(AgentTask Task, string AgentName)> dependencyResults,
        IReadOnlyList<OfficeDocument> documents,
        int maxDocumentChars)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Solicitud original del usuario");
        sb.AppendLine(userRequest);
        sb.AppendLine();
        sb.AppendLine("## Tu tarea");
        sb.AppendLine($"**{task.Title}**");
        sb.AppendLine(task.Description);

        if (dependencyResults.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Resultados de tareas previas");
            foreach (var (dep, name) in dependencyResults)
            {
                sb.AppendLine($"### {dep.Key} — {dep.Title} ({name})");
                sb.AppendLine(dep.Result);
                sb.AppendLine();
            }
        }

        if (documents.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Documentos adjuntos");
            var budget = maxDocumentChars;
            foreach (var doc in documents)
            {
                sb.AppendLine($"### {doc.FileName} ({doc.PageCount} páginas)");
                if (doc.RequiresOcr)
                {
                    sb.AppendLine("[Este PDF no contiene texto extraíble: parece escaneado. Se requiere OCR para leerlo. Informa de ello al usuario.]");
                    continue;
                }

                var text = doc.Text.Length <= budget ? doc.Text : doc.Text[..Math.Max(0, budget)];
                var cut = text.Length < doc.Text.Length || doc.Truncated;
                budget -= text.Length;
                sb.AppendLine("<documento>");
                sb.AppendLine(text);
                sb.AppendLine("</documento>");
                if (cut) sb.AppendLine("[Aviso: el documento se truncó por tamaño; solo se analizó la parte inicial.]");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    public static string BuildConsolidationPrompt(string userRequest, IReadOnlyList<(AgentTask Task, string AgentName)> results, int maxChars)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Solicitud original del usuario");
        sb.AppendLine(userRequest);
        sb.AppendLine();
        sb.AppendLine("## Resultados de los agentes");
        var perTask = Math.Max(1000, maxChars / Math.Max(1, results.Count));
        foreach (var (task, name) in results)
        {
            sb.AppendLine($"### {task.Key} — {task.Title} · {name} · estado: {task.Status}");
            if (task.Status == TaskStatus.Completed)
            {
                var text = task.Result ?? string.Empty;
                sb.AppendLine(text.Length <= perTask ? text : text[..perTask] + "\n[…resultado truncado…]");
            }
            else
            {
                sb.AppendLine($"(Sin resultado: {task.ErrorMessage ?? task.Status.ToString()})");
            }
            sb.AppendLine();
        }
        sb.AppendLine("Redacta ahora la respuesta final para el usuario.");
        return sb.ToString();
    }
}
