using ATLAS.Domain.Enums;
using TaskStatus = ATLAS.Domain.Enums.TaskStatus;

namespace ATLAS.Domain.Entities;

/// <summary>Tarea que el Orquestador asigna a un agente, con su estado, resultado y los datos para ejecutarla.</summary>
public sealed class AgentTask
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ConversationId { get; init; }
    public required string AgentId { get; init; }
    /// <summary>Clave local del plan para expresar las dependencias entre las tareas de una misma orquestación.</summary>
    public string Key { get; init; } = string.Empty;
    public required string Title { get; init; }
    public required string Description { get; init; }
    /// <summary>Claves de las tareas cuyo resultado necesita esta tarea antes de empezar.</summary>
    public IReadOnlyList<string> DependsOn { get; init; } = Array.Empty<string>();
    /// <summary>La tarea recibe el texto de los documentos adjuntos.</summary>
    public bool IncludeDocuments { get; init; }

    /// <summary>
    /// Texto extra del prompt de esta tarea (por ejemplo las situaciones candidatas de la base o el formato
    /// de entrega).
    /// </summary>
    public string? ExtraContext { get; init; }

    /// <summary>
    /// True si el contexto extra o el prompt son confidenciales: la tarea solo se envía a Claude, nunca a
    /// las APIs gratuitas.
    /// </summary>
    public bool Sensitive { get; init; }

    /// <summary>El flujo genera los documentos finales por sí mismo: no hay Word automático para esta tarea.</summary>
    public bool SkipDocument { get; init; }

    /// <summary>Si la tarea falla, se reintenta una vez con Claude antes de darla por fallida (lo fija el flujo de conocimiento).</summary>
    public bool RetryWithClaude { get; init; }

    /// <summary>
    /// Los documentos adjuntos de esta tarea pueden enviarse a las APIs gratuitas (lo fija el flujo de
    /// conocimiento).
    /// </summary>
    public bool AllowFreeDocuments { get; init; }
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }

    /// <summary>Documento Word generado con el resultado de esta tarea (agentes con producesDocument: word).</summary>
    public Guid? DocumentId { get; set; }
    public string? DocumentName { get; set; }

    /// <summary>Qué modelo respondió: "claude" o "free:proveedor" (ver el enrutamiento de modelos).</summary>
    public string? Provider { get; set; }

    /// <summary>Copia de la tarea para leerla sin riesgo de que cambie mientras se usa.</summary>
    public AgentTask Clone() => (AgentTask)MemberwiseClone();
}
