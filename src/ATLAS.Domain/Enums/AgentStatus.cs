namespace ATLAS.Domain.Enums;

/// <summary>Estados de un agente; la interfaz los refleja con animaciones y colores.</summary>
public enum AgentStatus
{
    Idle,
    Thinking,
    Working,
    Reading,
    Coding,
    Reviewing,
    Waiting,
    Completed,
    Error
}
