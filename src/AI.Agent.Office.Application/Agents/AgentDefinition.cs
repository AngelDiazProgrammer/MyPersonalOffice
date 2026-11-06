using AI.Agent.Office.Domain.Enums;
using AI.Agent.Office.Domain.ValueObjects;

namespace AI.Agent.Office.Application.Agents;

/// <summary>
/// Definición de un agente leída de su archivo Markdown: nombre, rol, instrucciones, modelo y banderas
/// (orquestador, acepta documentos, genera Word, interno).
/// </summary>
public sealed record AgentDefinition(
    string Id,
    string Name,
    string Description,
    string Role,
    string ConfigurationFile,
    string Instructions,
    AgentModelSettings ModelSettings,
    AgentStatus WorkingStatus,
    bool IsOrchestrator,
    bool AcceptsDocuments,
    string Color,
    string Icon,
    bool ProducesWord = false,
    bool WorkflowOnly = false);

/// <summary>Problema encontrado al validar la configuración de un agente (error o advertencia).</summary>
public sealed record ConfigurationIssue(string AgentId, string Message, bool IsError);

/// <summary>Resultado de cargar y validar los agentes.</summary>
public sealed record ConfigurationReport(IReadOnlyList<AgentDefinition> Agents, IReadOnlyList<ConfigurationIssue> Issues)
{
    public bool IsValid => Issues.All(i => !i.IsError);
}
