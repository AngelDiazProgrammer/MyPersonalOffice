using System.Text.Json;
using System.Text.Json.Serialization;

namespace ATLAS.Application.Orchestration;

/// <summary>Acciones posibles de una decisión del Orquestador.</summary>
public static class DecisionActions
{
    public const string Respond = "respond";
    public const string Delegate = "delegate";
}

/// <summary>Decisión estructurada que devuelve el Orquestador mediante una llamada a herramienta forzada.</summary>
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

    /// <summary>Lee el JSON de una decisión; devuelve null si no es válido.</summary>
    public static OrchestratorDecision? Parse(string json)
    {
        try { return JsonSerializer.Deserialize<OrchestratorDecision>(json, JsonOptions); }
        catch (JsonException) { return null; }
    }
}

/// <summary>Una tarea del plan: qué agente la hace, qué debe hacer y de qué otras tareas depende.</summary>
public sealed class PlannedTask
{
    public string Id { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("task")]
    public string Task { get; set; } = string.Empty;

    public List<string> DependsOn { get; set; } = new();
    public bool? IncludeDocuments { get; set; }

    /// <summary>La fija el sistema (no el modelo) en los flujos fijos.</summary>
    [JsonIgnore] public string? ExtraContext { get; set; }

    /// <summary>El flujo genera los documentos finales por sí mismo: se omite el Word automático de esta tarea.</summary>
    [JsonIgnore] public bool SkipDocument { get; set; }

    /// <summary>La fija el flujo: ante un fallo, la tarea se reintenta una vez con Claude.</summary>
    [JsonIgnore] public bool RetryWithClaude { get; set; }

    [JsonIgnore] public bool AllowFreeDocuments { get; set; }
    [JsonIgnore] public bool Sensitive { get; set; }
}
