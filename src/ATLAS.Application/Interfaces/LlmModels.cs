namespace ATLAS.Application.Interfaces;

/// <summary>Un mensaje de la conversación con el modelo (rol "user" o "assistant").</summary>
public sealed record LlmMessage(string Role, string Content);

/// <summary>
/// Obliga al modelo a responder con un JSON que cumpla <see cref="InputSchemaJson"/> (llamada a
/// herramienta en Claude, modo JSON en las APIs gratuitas).
/// </summary>
public sealed record StructuredOutputSpec(string Name, string Description, string InputSchemaJson);

/// <summary>Solicitud a un modelo de lenguaje: instrucciones, mensajes, límites y preferencias de proveedor.</summary>
/// <param name="ProviderPreference">"claude" fuerza a Claude (se usa para escalar); null o "auto" sigue el modo configurado.</param>
/// <param name="FreeModel">Cambia el modelo por defecto del proveedor gratuito solo para esta solicitud.</param>
/// <param name="Sensitive">La solicitud lleva documentos del usuario: solo se envía a Claude, nunca a las APIs gratuitas.</param>
public sealed record LlmRequest(
    string System,
    IReadOnlyList<LlmMessage> Messages,
    int MaxTokens,
    string? Model = null,
    double? Temperature = null,
    StructuredOutputSpec? StructuredOutput = null,
    string? Purpose = null,
    string? ProviderPreference = null,
    string? FreeModel = null,
    bool Sensitive = false);

/// <summary>Respuesta de un modelo: texto, JSON estructurado (si se pidió), tokens usados y proveedor.</summary>
/// <param name="Provider">"claude" o "free:{proveedor}": quién produjo realmente la respuesta.</param>
public sealed record LlmResponse(
    string Text,
    string? StructuredJson,
    int InputTokens,
    int OutputTokens,
    string? StopReason,
    string Model,
    string? Provider = null);

/// <summary>Error al llamar a un modelo; indica el código HTTP y si fue por límite de uso.</summary>
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

/// <summary>Nombres de los proveedores de modelos.</summary>
public static class LlmProviders
{
    public const string Claude = "claude";
    public const string FreePrefix = "free:";
    public static bool IsFree(string? provider) => provider?.StartsWith(FreePrefix, StringComparison.OrdinalIgnoreCase) == true;
}
