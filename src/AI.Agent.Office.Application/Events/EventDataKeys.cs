namespace AI.Agent.Office.Application.Events;

/// <summary>Claves de <c>OfficeEvent.AdditionalData</c>. La interfaz deriva su estado visual solo de ellas.</summary>
public static class EventDataKeys
{
    public const string AgentStatus = "agentStatus";
    public const string TaskTitle = "taskTitle";
    public const string TaskStatus = "taskStatus";
    public const string ConversationId = "conversationId";
    public const string TaskCount = "taskCount";
    public const string Delegated = "delegated";
    /// <summary>
    /// Ids de los agentes (separados por coma) cuyos resultados necesita la tarea; activa las animaciones
    /// de entrega.
    /// </summary>
    public const string DependsOnAgents = "dependsOnAgents";
    /// <summary>Nombre del Word generado por la tarea (viene en AgentCompleted cuando hay uno).</summary>
    public const string Document = "document";
}
