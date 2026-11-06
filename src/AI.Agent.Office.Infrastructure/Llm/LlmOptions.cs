using AI.Agent.Office.Application.Interfaces;

namespace AI.Agent.Office.Infrastructure.Llm;

/// <summary>Configuración del enrutamiento de modelos (sección "Llm").</summary>
public sealed class LlmOptions
{
    public const string SectionName = "Llm";

    /// <summary>
    /// FreeFirst (predeterminado): APIs gratuitas en el orden configurado y Claude solo cuando ninguna
    /// puede responder.
    /// </summary>
    public LlmMode Mode { get; set; } = LlmMode.FreeFirst;

    /// <summary>
    /// Si no está vacío, solo se usan los proveedores cuya región (<see
    /// cref="FreeProviderOptions.Region"/>) esté en la lista (por ejemplo ["US"]).
    /// </summary>
    public List<string> AllowedRegions { get; set; } = new();

    /// <summary>
    /// Proveedores gratuitos compatibles con OpenAI, probados en este orden. Todos pasan por UN servicio
    /// genérico: agregar uno es solo otra entrada aquí (nombre, URL, variable de la clave, modelo y
    /// región).
    /// </summary>
    public List<FreeProviderOptions> FreeProviders { get; set; } = new();
}

/// <summary>Configuración de un proveedor gratuito.</summary>
public sealed class FreeProviderOptions
{
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Dónde procesa los datos el proveedor (informativo y usado por <see
    /// cref="LlmOptions.AllowedRegions"/>).
    /// </summary>
    public string Region { get; set; } = "Global";

    /// <summary>
    /// Dirección base compatible con OpenAI. Los marcadores <c>{VARIABLE}</c> se completan con variables de
    /// entorno o .env.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Nombre de la variable de entorno que guarda la clave (nunca la clave misma).</summary>
    public string ApiKeyVariable { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Prompt más grande (en tokens estimados) que se envía a este proveedor; los mayores van al siguiente
    /// o a Claude.
    /// </summary>
    public int MaxInputTokens { get; set; } = 6000;

    public int MaxOutputTokens { get; set; } = 2048;
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>Extra top-level request parameters for this provider (e.g. reasoning_effort = low for Groq's gpt-oss models). Values are sent as strings.</summary>
    public Dictionary<string, string> ExtraParameters { get; set; } = new();
}
