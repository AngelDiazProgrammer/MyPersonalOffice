namespace ATLAS.Domain.ValueObjects;

/// <summary>"claude" usa siempre Claude para este agente; null o "auto" sigue el modo global.</summary>
public sealed record AgentModelSettings(
    string? Model = null, int? MaxTokens = null, double? Temperature = null,
    string? Provider = null, string? FreeModel = null);
