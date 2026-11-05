namespace AI.Agent.Office.Application.Interfaces;

public sealed record LlmMessage(string Role, string Content);

/// <summary>Forces the model to answer by calling a tool whose input matches <see cref="InputSchemaJson"/>.</summary>
public sealed record StructuredOutputSpec(string Name, string Description, string InputSchemaJson);

public sealed record LlmRequest(
    string System,
    IReadOnlyList<LlmMessage> Messages,
    int MaxTokens,
    string? Model = null,
    double? Temperature = null,
    StructuredOutputSpec? StructuredOutput = null,
    string? Purpose = null);

public sealed record LlmResponse(
    string Text,
    string? StructuredJson,
    int InputTokens,
    int OutputTokens,
    string? StopReason,
    string Model);

public class LlmException : Exception
{
    public int? StatusCode { get; }
    public bool IsRateLimit { get; }

    public LlmException(string message, int? statusCode = null, bool isRateLimit = false, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        IsRateLimit = isRateLimit;
    }
}
