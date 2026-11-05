namespace AI.Agent.Office.Domain.ValueObjects;

/// <summary>Per-agent model configuration. Null values mean "use the global default".</summary>
public sealed record AgentModelSettings(string? Model = null, int? MaxTokens = null, double? Temperature = null);
