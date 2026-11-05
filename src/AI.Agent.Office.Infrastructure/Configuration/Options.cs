namespace AI.Agent.Office.Infrastructure.Configuration;

public sealed class AnthropicOptions
{
    public const string SectionName = "Anthropic";

    /// <summary>Comes from ANTHROPIC_API_KEY. Never serialized to the frontend.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Comes from ANTHROPIC_MODEL. Configurable; check the model list in the Anthropic documentation.</summary>
    public string Model { get; set; } = "claude-sonnet-5-5";

    public string BaseUrl { get; set; } = "https://api.anthropic.com";
    public string ApiVersion { get; set; } = "2023-06-01";
    public int TimeoutSeconds { get; set; } = 120;
    public int MaxRetries { get; set; } = 3;
    public int MaxConcurrentRequests { get; set; } = 4;
    public int MaxOutputTokens { get; set; } = 8192;
    public int MaxInputTokensEstimate { get; set; } = 150_000;
}

public sealed class AgentRegistration
{
    public string Id { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
}

public sealed class AgentsOptions
{
    public const string SectionName = "Agents";

    /// <summary>Folder with the Markdown role files (relative paths are searched upwards from the app).</summary>
    public string Directory { get; set; } = "agents";

    /// <summary>Registered agents. When empty, every *.md file in the directory is registered.</summary>
    public List<AgentRegistration> Definitions { get; set; } = new();
}
