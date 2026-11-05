using AI.Agent.Office.Domain.Enums;

namespace AI.Agent.Office.Application.DTOs;

public sealed record AgentDto(
    string Id, string Name, string Description, string Role, AgentStatus Status,
    Guid? CurrentTaskId, string? CurrentTaskTitle, string ConfigurationFile,
    DateTimeOffset? LastActivityAt, string Color, string Icon, bool IsOrchestrator, bool AcceptsDocuments, string? Model);

public sealed record AgentTaskDto(
    Guid Id, Guid ConversationId, string AgentId, string Key, string Title, string Description,
    TaskStatus Status, DateTimeOffset CreatedAt, DateTimeOffset? StartedAt, DateTimeOffset? CompletedAt,
    string? Result, string? ErrorMessage, IReadOnlyList<string> DependsOn);

public sealed record ChatMessageDto(Guid Id, ChatRole Role, string Content, DateTimeOffset CreatedAt);

public sealed record ConversationDto(Guid Id, DateTimeOffset CreatedAt, IReadOnlyList<ChatMessageDto> Messages);

public sealed record OfficeActivityDto(Guid Id, string? AgentId, Guid? TaskId, OfficeEventType EventType, string Description, DateTimeOffset Timestamp);

public sealed record OfficeEventDto(
    Guid EventId, string? AgentId, Guid? TaskId, OfficeEventType EventType, string Description,
    DateTimeOffset Timestamp, IReadOnlyDictionary<string, string?>? AdditionalData);

public sealed record AgentUsageDto(string AgentId, long InputTokens, long OutputTokens, long Requests);

public sealed record TokenUsageDto(
    long InputTokens, long OutputTokens, long Requests,
    IReadOnlyList<AgentUsageDto> ByAgent, decimal? EstimatedCostUsd);

public sealed record OfficeStateDto(
    IReadOnlyList<AgentDto> Agents, IReadOnlyList<AgentTaskDto> Tasks,
    IReadOnlyList<OfficeActivityDto> Activity, TokenUsageDto Usage, bool AnthropicConfigured);

public sealed record ChatRequestDto(string Message, Guid? ConversationId = null, IReadOnlyList<Guid>? DocumentIds = null);

public sealed record ChatResponseDto(
    Guid ConversationId, string Reply, bool Delegated, bool IsError,
    string? PlanMessage, IReadOnlyList<AgentTaskDto> Tasks);

public sealed record DocumentDto(Guid Id, string FileName, int PageCount, int CharacterCount, bool Truncated, bool RequiresOcr);

public sealed record ConfigurationReportDto(IReadOnlyList<AgentDto> Agents, IReadOnlyList<ConfigurationIssueDto> Issues, bool IsValid);

public sealed record ConfigurationIssueDto(string AgentId, string Message, bool IsError);
