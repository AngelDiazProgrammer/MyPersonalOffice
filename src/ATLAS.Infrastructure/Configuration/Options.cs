namespace ATLAS.Infrastructure.Configuration;

/// <summary>Configuración de Claude (sección "Anthropic"): clave, modelo, reintentos y caché de prompt.</summary>
public sealed class AnthropicOptions
{
    public const string SectionName = "Anthropic";

    /// <summary>Viene de ANTHROPIC_API_KEY. Nunca se serializa hacia el frontend.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Viene de ANTHROPIC_MODEL. Es configurable; consulta la lista de modelos en la documentación de
    /// Anthropic.
    /// </summary>
    public string Model { get; set; } = "claude-sonnet-5-5";

    public string BaseUrl { get; set; } = "https://api.anthropic.com";
    public string ApiVersion { get; set; } = "2023-06-01";
    public int TimeoutSeconds { get; set; } = 120;
    public int MaxRetries { get; set; } = 3;
    public int MaxConcurrentRequests { get; set; } = 4;
    public int MaxOutputTokens { get; set; } = 8192;
    public int MaxInputTokensEstimate { get; set; } = 150_000;

    /// <summary>
    /// Marca el prompt del sistema como cacheable (caché de prompts de Anthropic) para bajar el costo de
    /// las llamadas repetidas.
    /// </summary>
    public bool PromptCaching { get; set; } = true;
}

/// <summary>Dónde y cuánto tiempo se guardan los documentos generados (sección "GeneratedDocuments").</summary>
public sealed class GeneratedDocumentsOptions
{
    public const string SectionName = "GeneratedDocuments";

    /// <summary>
    /// Carpeta de los archivos generados (las rutas relativas quedan junto a la carpeta agents/ del
    /// repositorio).
    /// </summary>
    public string Directory { get; set; } = "generated-documents";

    public int RetentionHours { get; set; } = 24;
}

/// <summary>Un agente registrado en la configuración: su id y su archivo Markdown.</summary>
public sealed class AgentRegistration
{
    public string Id { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
}

/// <summary>Configuración de los agentes (sección "Agents").</summary>
public sealed class AgentsOptions
{
    public const string SectionName = "Agents";

    /// <summary>
    /// Carpeta con los archivos Markdown de los roles (las rutas relativas se buscan subiendo desde la
    /// app).
    /// </summary>
    public string Directory { get; set; } = "agents";

    /// <summary>Agentes registrados. Si está vacío se registra cada archivo *.md de la carpeta.</summary>
    public List<AgentRegistration> Definitions { get; set; } = new();
}
