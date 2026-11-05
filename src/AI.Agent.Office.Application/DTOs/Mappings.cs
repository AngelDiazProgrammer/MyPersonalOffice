using AI.Agent.Office.Application.Agents;

namespace AI.Agent.Office.Application.DTOs;

using AI.Agent.Office.Domain.Entities;
using AI.Agent.Office.Domain.Events;
using Agent = AI.Agent.Office.Domain.Entities.Agent;

public static class Mappings
{
    public static AgentDto ToDto(this Agent a) => new(
        a.Id, a.Name, a.Description, a.Role, a.Status, a.CurrentTaskId, a.CurrentTaskTitle, a.ConfigurationFile,
        a.LastActivityAt, a.Color, a.Icon, a.IsOrchestrator, a.AcceptsDocuments, a.ModelSettings.Model);

    public static AgentTaskDto ToDto(this AgentTask t) => new(
        t.Id, t.ConversationId, t.AgentId, t.Key, t.Title, t.Description, t.Status, t.CreatedAt,
        t.StartedAt, t.CompletedAt, t.Result, t.ErrorMessage, t.DependsOn);

    public static ChatMessageDto ToDto(this ChatMessage m) => new(m.Id, m.Role, m.Content, m.CreatedAt);

    public static ConversationDto ToDto(this Conversation c) =>
        new(c.Id, c.CreatedAt, c.Messages.Select(m => m.ToDto()).ToArray());

    public static OfficeActivityDto ToDto(this OfficeActivity a) =>
        new(a.Id, a.AgentId, a.TaskId, a.EventType, a.Description, a.Timestamp);

    public static OfficeEventDto ToDto(this OfficeEvent e) =>
        new(e.EventId, e.AgentId, e.TaskId, e.EventType, e.Description, e.Timestamp, e.AdditionalData);

    public static DocumentDto ToDto(this OfficeDocument d) =>
        new(d.Id, d.FileName, d.PageCount, d.Text.Length, d.Truncated, d.RequiresOcr);

    public static ConfigurationReportDto ToDto(this ConfigurationReport r, IEnumerable<AgentDto> agents) =>
        new(agents.ToArray(), r.Issues.Select(i => new ConfigurationIssueDto(i.AgentId, i.Message, i.IsError)).ToArray(), r.IsValid);
}
