using ATLAS.Domain.Enums;
using ATLAS.Domain.ValueObjects;

namespace ATLAS.Domain.Entities;

/// <summary>
/// Agente de la oficina en ejecución: su identidad, su estado actual y la tarea que atiende. Se crea
/// desde su archivo Markdown.
/// </summary>
public sealed class Agent
{
    /// <summary>Identificador único (por ejemplo "developer").</summary>
    public required string Id { get; init; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    /// <summary>Estado que muestra la interfaz (Idle, Thinking, Working...).</summary>
    public AgentStatus Status { get; set; } = AgentStatus.Idle;
    public Guid? CurrentTaskId { get; set; }
    public string? CurrentTaskTitle { get; set; }
    /// <summary>Archivo Markdown del que sale su configuración.</summary>
    public required string ConfigurationFile { get; init; }
    public AgentModelSettings ModelSettings { get; set; } = new();
    public DateTimeOffset? LastActivityAt { get; set; }

    /// <summary>Estado que se muestra mientras trabaja en una tarea (Working, Coding, Reading, Reviewing...).</summary>
    public AgentStatus WorkingStatus { get; set; } = AgentStatus.Working;
    /// <summary>Es el Orquestador: planifica y delega, no es un especialista.</summary>
    public bool IsOrchestrator { get; set; }
    /// <summary>Puede recibir el texto de los PDF adjuntos.</summary>
    public bool AcceptsDocuments { get; set; }
    /// <summary>Su resultado se convierte automáticamente en un documento Word.</summary>
    public bool ProducesWord { get; set; }

    /// <summary>Especialista interno de un flujo fijo: el planificador libre del Orquestador no lo ofrece.</summary>
    public bool WorkflowOnly { get; set; }
    public string Color { get; set; } = "#8b5cf6";
    public string Icon { get; set; } = "🤖";

    /// <summary>Copia del agente (los lectores reciben copias, nunca el objeto compartido).</summary>
    public Agent Clone() => (Agent)MemberwiseClone();
}
