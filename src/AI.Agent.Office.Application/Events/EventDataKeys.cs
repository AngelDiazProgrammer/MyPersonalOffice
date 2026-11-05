namespace AI.Agent.Office.Application.Events;

/// <summary>Keys used in <c>OfficeEvent.AdditionalData</c>. The UI derives visual state only from these.</summary>
public static class EventDataKeys
{
    public const string AgentStatus = "agentStatus";
    public const string TaskTitle = "taskTitle";
    public const string TaskStatus = "taskStatus";
    public const string ConversationId = "conversationId";
    public const string TaskCount = "taskCount";
    public const string Delegated = "delegated";
}
