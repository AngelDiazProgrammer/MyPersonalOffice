namespace AI.Agent.Office.Domain.Enums;

/// <summary>Estados del ciclo de vida de una tarea.</summary>
public enum TaskStatus
{
    Pending,
    Assigned,
    Running,
    Completed,
    Failed,
    Cancelled
}
