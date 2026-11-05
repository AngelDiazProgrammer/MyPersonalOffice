namespace AI.Agent.Office.Application.Options;

public sealed class OrchestrationOptions
{
    public const string SectionName = "Orchestration";

    public int MaxTasksPerRequest { get; set; } = 6;
    public int MaxConcurrency { get; set; } = 3;
    public int TaskTimeoutSeconds { get; set; } = 180;
    public int PlanningTimeoutSeconds { get; set; } = 90;
    public int ConsolidationTimeoutSeconds { get; set; } = 120;
    public int MaxHistoryMessages { get; set; } = 20;
    public int MaxMessageChars { get; set; } = 8000;
    public int MaxDocumentCharsPerTask { get; set; } = 60000;
    public int MaxResultCharsForConsolidation { get; set; } = 24000;
    public int PlanningMaxTokens { get; set; } = 2048;
    public int DefaultAgentMaxTokens { get; set; } = 4096;
    public int ThinkingDisplayMs { get; set; } = 700;
    public int CompletedDisplaySeconds { get; set; } = 4;
    public int MaxActivityEntries { get; set; } = 300;
}
