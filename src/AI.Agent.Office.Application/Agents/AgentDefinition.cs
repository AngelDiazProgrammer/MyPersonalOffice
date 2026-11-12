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
    bool WorkflowOnly = false,
    string Purpose = "",
    string Kind = AgentKinds.Llm,
    string Environment = AgentEnvironments.Office,
    IReadOnlyList<string>? Tools = null,
    string Knowledge = "",
    bool Sensitive = false,
    bool IsSystem = false)
{
    /// <summary>Herramientas declaradas (nunca null).</summary>
    public IReadOnlyList<string> ToolList => Tools ?? Array.Empty<string>();

    /// <summary>El agente puede usar la herramienta (spec 011, RF-010).</summary>
    public bool Allows(string tool) => ToolList.Contains(tool, StringComparer.OrdinalIgnoreCase);
}

/// <summary>Quién ejecuta a un agente (spec 011, RF-003).</summary>
public static class AgentKinds
{
    /// <summary>Lo ejecuta un modelo de lenguaje.</summary>
    public const string Llm = "llm";
    /// <summary>Lo ejecuta código determinista, sin modelo.</summary>
    public const string Executor = "executor";
    public static readonly string[] All = { Llm, Executor };
}

/// <summary>Dónde se prueba o usa un agente.</summary>
public static class AgentEnvironments
{
    public const string Office = "office";
    public const string Chatbot = "chatbot";
    public const string Conversational = "conversational";
    public static readonly string[] All = { Office, Chatbot, Conversational };
}

/// <summary>Herramientas que un agente puede declarar; el código impone que solo use las suyas.</summary>
public static class AgentTools
{
    public const string SearchKnowledge = "buscar_conocimiento";
    public const string ReadService = "consultar_servicio";
    public const string WriteService = "escribir_servicio";
    public const string ReadPlaybook = "leer_playbook";
    public static readonly string[] All = { SearchKnowledge, ReadService, WriteService, ReadPlaybook };
}

/// <summary>Agentes del sistema: se pueden editar pero no eliminar ni cambiar de id o tipo.</summary>
public static class SystemAgents
{
    public const string Orchestrator = "orchestrator";
    public const string CamilaIa = "camila-ia";
    public const string LectorUml = "lector-uml";
    public static readonly string[] Ids = { Orchestrator, CamilaIa, LectorUml };
    public static bool IsSystem(string id) => Ids.Contains(id, StringComparer.OrdinalIgnoreCase);
}

/// <summary>Problema encontrado al validar la configuración de un agente (error o advertencia).</summary>
public sealed record ConfigurationIssue(string AgentId, string Message, bool IsError);

/// <summary>Resultado de cargar y validar los agentes.</summary>
public sealed record ConfigurationReport(IReadOnlyList<AgentDefinition> Agents, IReadOnlyList<ConfigurationIssue> Issues)
{
    public bool IsValid => Issues.All(i => !i.IsError);
}
