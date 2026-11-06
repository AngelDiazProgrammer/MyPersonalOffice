namespace AI.Agent.Office.Domain.Enums;

/// <summary>Tipos de evento de la oficina; cada uno llega a la interfaz como un método del canal en tiempo real.</summary>
public enum OfficeEventType
{
    AgentStarted,
    AgentThinking,
    AgentTaskAssigned,
    AgentProgress,
    AgentCompleted,
    AgentError,
    TaskCreated,
    TaskCompleted,
    OrchestrationStarted,
    OrchestrationCompleted,
    OfficeActivityUpdated,
    WorkspaceChanged,
    KnowledgeConsulted
}
