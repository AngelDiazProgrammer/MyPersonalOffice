using System.Text.Json;
using System.Text.Json.Serialization;

namespace AI.Agent.Office.Application.Orchestration;

public static class DecisionActions
{
    public const string Respond = "respond";
    public const string Delegate = "delegate";
}

/// <summary>Structured decision returned by the Orchestrator through a forced tool call.</summary>
public sealed class OrchestratorDecision
{
    public string Action { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<PlannedTask> Tasks { get; set; } = new();

    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public static OrchestratorDecision? Parse(string json)
    {
        try { return JsonSerializer.Deserialize<OrchestratorDecision>(json, JsonOptions); }
        catch (JsonException) { return null; }
    }
}

public sealed class PlannedTask
{
    public string Id { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("task")]
    public string Task { get; set; } = string.Empty;

    public List<string> DependsOn { get; set; } = new();
    public bool? IncludeDocuments { get; set; }
}
