using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Infrastructure.Llm;

/// <summary>
/// UN solo cliente global para todos los proveedores gratuitos compatibles con OpenAI (Groq, Cloudflare
/// Workers AI...). Los proveedores son solo entradas de configuración: esta clase los recorre en orden,
/// maneja los límites de uso con pausas por proveedor y pasa al siguiente. Nunca habla con Claude.
/// </summary>
public sealed partial class FreeApiService
{
    #region Proveedores y configuración

    public const string HttpClientName = "free-llm";

    /// <summary>Un proveedor configurado, con su estado (clave, pausa por límite de uso y último error).</summary>
    private sealed class Provider
    {
        public required FreeProviderOptions Options { get; init; }
        public string? ApiKey { get; init; }
        public string? BaseUrl { get; init; }
        public DateTimeOffset CooldownUntil { get; set; }
        public bool JsonModeUnsupported { get; set; }
        public string? LastError { get; set; }
        public bool Configured => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(BaseUrl);
        public bool CoolingDown => CooldownUntil > DateTimeOffset.UtcNow;
    }

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenUsageTracker _usage;
    private readonly ILogger<FreeApiService> _logger;
    private readonly List<string> _allowedRegions;
    private readonly List<Provider> _providers;

    /// <summary>Crea el cliente leyendo los proveedores de la configuración.</summary>
    public FreeApiService(
        IHttpClientFactory httpClientFactory, IOptions<LlmOptions> options, IConfiguration configuration,
        ITokenUsageTracker usage, ILogger<FreeApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _usage = usage;
        _logger = logger;
        _allowedRegions = options.Value.AllowedRegions;

        _providers = options.Value.FreeProviders
            .Where(p => p.Enabled && !string.IsNullOrWhiteSpace(p.Name))
            .Select(p => new Provider
            {
                Options = p,
                ApiKey = string.IsNullOrWhiteSpace(p.ApiKeyVariable) ? null : configuration[p.ApiKeyVariable]?.Trim(),
                BaseUrl = ResolveBaseUrl(p.BaseUrl, configuration)
            })
            .ToList();
    }

    /// <summary>
    /// Reemplaza los marcadores {VARIABLE} de la dirección; devuelve null si falta alguna (el proveedor
    /// cuenta entonces como no configurado).
    /// </summary>
    private static string? ResolveBaseUrl(string template, IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(template)) return null;
        var missing = false;
        var resolved = Placeholder().Replace(template, m =>
        {
            var value = configuration[m.Groups[1].Value];
            if (string.IsNullOrWhiteSpace(value)) missing = true;
            return value?.Trim() ?? string.Empty;
        });
        return missing ? null : resolved.TrimEnd('/');
    }

    /// <summary>Marcadores {VARIABLE} de las direcciones base.</summary>
    [GeneratedRegex(@"\{([A-Za-z0-9_]+)\}")]
    private static partial Regex Placeholder();

    /// <summary>Hay al menos un proveedor gratuito configurado.</summary>
    public bool AnyConfigured => _providers.Any(p => p.Configured);

    /// <summary>
    /// Pregunta a cada proveedor qué modelos permite su clave (GET /models) y avisa si el modelo
    /// configurado no está. Es informativo: un fallo aquí (algunos proveedores no tienen /models) nunca
    /// desactiva un proveedor.
    /// </summary>
    public async Task ValidateModelsAsync(CancellationToken cancellationToken)
    {
        foreach (var provider in _providers.Where(p => p.Configured))
        {
            try
            {
                using var message = new HttpRequestMessage(HttpMethod.Get, $"{provider.BaseUrl}/models");
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", provider.ApiKey);
                using var response = await _httpClientFactory.CreateClient(HttpClientName).SendAsync(message, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode) continue;

                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array) continue;

                var ids = data.EnumerateArray().Select(m => m.TryGetProperty("id", out var id) ? id.GetString() : null).Where(id => id is not null)
                    .Select(id => id!.StartsWith("models/", StringComparison.OrdinalIgnoreCase) ? id[7..] : id)   // Gemini lista "models/gemini-..."
                    .ToList();
                if (ids.Count == 0 || ids.Contains(provider.Options.Model, StringComparer.OrdinalIgnoreCase)) continue;

                provider.LastError = $"el modelo '{provider.Options.Model}' no está disponible para tu clave";
                provider.CooldownUntil = DateTimeOffset.UtcNow.AddMinutes(10);
                _logger.LogWarning("{Provider}: el modelo configurado '{Model}' NO está disponible para tu clave. Modelos disponibles: {Models}",
                    provider.Options.Name, provider.Options.Model, string.Join(", ", ids));
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException)
            {
                _logger.LogDebug(ex, "No se pudo listar los modelos de {Provider}", provider.Options.Name);
            }
        }
    }

    /// <summary>Estado actual de cada proveedor para la interfaz.</summary>
    public IReadOnlyList<FreeProviderStatusDto> GetStatuses() =>
        _providers.Select(p => new FreeProviderStatusDto(p.Options.Name, p.Options.Region, p.Options.Model, p.Configured, p.CoolingDown, p.LastError)).ToList();

    /// <summary>True si la región del proveedor está permitida por la configuración de privacidad.</summary>
    private bool RegionAllowed(Provider p) =>
        _allowedRegions.Count == 0 || _allowedRegions.Contains(p.Options.Region, StringComparer.OrdinalIgnoreCase);

    /// <summary>Estimación rápida de los tokens de la solicitud (para no enviar a un proveedor que no la soporte).</summary>
    private static double EstimateTokens(LlmRequest request) =>
        (request.System.Length + request.Messages.Sum(m => m.Content.Length) + (request.StructuredOutput?.InputSchemaJson.Length ?? 0)) / 3.5;

    /// <summary>True si el proveedor puede atender esta solicitud ahora mismo.</summary>
    private bool Eligible(Provider p, LlmRequest request, double promptTokens) =>
        p.Configured && !p.CoolingDown && RegionAllowed(p) && promptTokens + 64 <= p.Options.MaxInputTokens;

    #endregion

    #region Envío a los proveedores

    /// <summary>
    /// Prueba UN proveedor por su nombre con una petición mínima, ignorando el orden y las pausas. Si responde bien, le quita la pausa
    /// y el error guardado. Sirve para diagnosticar claves, modelos y conexión.
    /// </summary>
    public async Task<LlmProviderTestDto> TestProviderAsync(string name, string? model, CancellationToken cancellationToken)
    {
        var provider = _providers.FirstOrDefault(p => string.Equals(p.Options.Name, name, StringComparison.OrdinalIgnoreCase));
        if (provider is null) return new LlmProviderTestDto(name, string.Empty, false, 0, null, "Proveedor desconocido.");
        if (!provider.Configured) return new LlmProviderTestDto(provider.Options.Name, provider.Options.Model, false, 0, null, "Sin clave configurada.");

        var request = new LlmRequest("Responde solo con la palabra OK.", new[] { new LlmMessage("user", "Prueba de conexión") }, 20,
            Purpose: "provider-test", FreeModel: model, Sensitive: false);
        var watch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var response = await SendToAsync(provider, request, cancellationToken).ConfigureAwait(false);
            provider.LastError = null;
            provider.CooldownUntil = default;
            return new LlmProviderTestDto(provider.Options.Name, model ?? provider.Options.Model, true, watch.ElapsedMilliseconds, response.Text.Trim(), null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var detail = ex is LlmException llm && llm.InnerException is not null ? $"{llm.Message}: {llm.InnerException.Message}" : ex.Message;
            provider.LastError = detail;
            return new LlmProviderTestDto(provider.Options.Name, model ?? provider.Options.Model, false, watch.ElapsedMilliseconds, null, detail);
        }
    }

    /// <summary>True si algún proveedor podría atender la solicitud ahora mismo.</summary>
    public bool CanHandle(LlmRequest request)
    {
        var tokens = EstimateTokens(request);
        return _providers.Any(p => Eligible(p, request, tokens));
    }

    /// <summary>
    /// Prueba cada proveedor elegible en orden. Devuelve null (con los motivos) si ninguno dio una
    /// respuesta utilizable.
    /// </summary>
    public async Task<LlmResponse?> TrySendAsync(LlmRequest request, List<string> reasons, CancellationToken cancellationToken)
    {
        var promptTokens = EstimateTokens(request);

        foreach (var provider in _providers)
        {
            if (!Eligible(provider, request, promptTokens))
            {
                if (provider.Configured) reasons.Add($"{provider.Options.Name}: " + (provider.CoolingDown ? "en pausa por límite" : "fuera de región o prompt demasiado grande"));
                continue;
            }

            try
            {
                var response = await SendToAsync(provider, request, cancellationToken).ConfigureAwait(false);
                var problem = Unacceptable(request, response);
                if (problem is null)
                {
                    provider.LastError = null;
                    return response;
                }
                provider.LastError = problem;

                reasons.Add($"{provider.Options.Name}: {problem}");
                _logger.LogInformation("{Provider} dio una respuesta inservible ({Problem}); se prueba el siguiente.", provider.Options.Name, problem);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (ex is LlmException or System.Text.Json.JsonException or InvalidOperationException or KeyNotFoundException or FormatException)
            {
                // Un proveedor con una respuesta rara o ilegible nunca debe tumbar la solicitud: se pasa al siguiente.
                provider.LastError = ex.Message;
                reasons.Add($"{provider.Options.Name}: {ex.Message}");
                _logger.LogInformation("{Provider} falló ({Message}); se prueba el siguiente.", provider.Options.Name, ex.Message);
            }
        }

        return null;
    }

    /// <summary>Envía la solicitud a un proveedor; si rechaza el modo JSON reintenta confiando solo en el prompt.</summary>
    private async Task<LlmResponse> SendToAsync(Provider provider, LlmRequest request, CancellationToken cancellationToken)
    {
        var useJsonMode = request.StructuredOutput is not null && !provider.JsonModeUnsupported;
        try
        {
            return await PostAsync(provider, request, useJsonMode, cancellationToken).ConfigureAwait(false);
        }
        catch (LlmException ex) when (useJsonMode && ex.StatusCode == 400)
        {
            // Algunos proveedores o modelos rechazan response_format: se recuerda y se reintenta una vez confiando solo en
            // el prompt.
            provider.JsonModeUnsupported = true;
            return await PostAsync(provider, request, false, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Hace la llamada HTTP al proveedor y lee su respuesta.</summary>
    private async Task<LlmResponse> PostAsync(Provider provider, LlmRequest request, bool jsonMode, CancellationToken cancellationToken)
    {
        var o = provider.Options;
        var model = string.IsNullOrWhiteSpace(request.FreeModel) ? o.Model : request.FreeModel!;

        var system = request.System;
        if (request.StructuredOutput is { } spec)
        {
            system += $"\n\nResponde ÚNICAMENTE con un objeto JSON válido que cumpla este esquema JSON (herramienta `{spec.Name}`), sin texto adicional ni bloques de código:\n{spec.InputSchemaJson}";
        }

        var messages = new JsonArray { new JsonObject { ["role"] = "system", ["content"] = system } };
        foreach (var m in request.Messages) messages.Add(new JsonObject { ["role"] = m.Role, ["content"] = m.Content });

        var body = new JsonObject
        {
            ["model"] = model,
            ["messages"] = messages,
            ["max_tokens"] = Math.Min(request.MaxTokens, o.MaxOutputTokens),
            ["stream"] = false
        };
        if (request.Temperature is { } t) body["temperature"] = t;
        else if (request.StructuredOutput is not null) body["temperature"] = 0.1;
        if (jsonMode) body["response_format"] = new JsonObject { ["type"] = "json_object" };
        foreach (var (key, value) in o.ExtraParameters) body[key] = value;

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(o.TimeoutSeconds));

        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, $"{provider.BaseUrl}/chat/completions")
            {
                Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json")
            };
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", provider.ApiKey); // never logged

            var client = _httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.SendAsync(message, timeout.Token).ConfigureAwait(false);
            var payload = await response.Content.ReadAsStringAsync(timeout.Token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode) throw ToException(provider, response, payload);

            var parsed = Parse(payload, model, request.StructuredOutput is not null, o.Name);
            _usage.Record(request.Purpose ?? "request", parsed.InputTokens, parsed.OutputTokens, free: true, provider: o.Name);
            _logger.LogInformation("Free {Provider} {Purpose} OK ({Model}, {In} in / {Out} out tokens)",
                o.Name, request.Purpose, model, parsed.InputTokens, parsed.OutputTokens);
            return parsed;
        }
        catch (HttpRequestException ex)
        {
            Cool(provider, TimeSpan.FromSeconds(30));
            throw new LlmException("no se pudo conectar", inner: ex);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            Cool(provider, TimeSpan.FromSeconds(30));
            throw new LlmException($"tardó más de {o.TimeoutSeconds} s");
        }
    }

    /// <summary>Pone a un proveedor en pausa durante un tiempo (límite de uso o error persistente).</summary>
    private void Cool(Provider provider, TimeSpan duration)
    {
        var until = DateTimeOffset.UtcNow + duration;
        if (until > provider.CooldownUntil) provider.CooldownUntil = until;
    }

    #endregion

    #region Interpretación de respuestas y errores

    /// <summary>
    /// Convierte una respuesta HTTP de error en una excepción y decide la pausa del proveedor (429, 401,
    /// 404, 5xx).
    /// </summary>
    private LlmException ToException(Provider provider, HttpResponseMessage response, string payload)
    {
        var detail = ErrorText.Clean(ExtractError(payload));
        var status = (int)response.StatusCode;

        switch (response.StatusCode)
        {
            case HttpStatusCode.TooManyRequests:
            {
                var wait = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(60);
                Cool(provider, wait > TimeSpan.FromMinutes(30) ? TimeSpan.FromMinutes(30) : wait);
                return new LlmException($"límite de uso alcanzado ({detail})", status, isRateLimit: true);
            }
            case HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden:
                Cool(provider, TimeSpan.FromMinutes(10));
                _logger.LogWarning("{Provider}: clave rechazada ({Status}). Revisa {Variable}.", provider.Options.Name, status, provider.Options.ApiKeyVariable);
                return new LlmException($"clave rechazada ({status})", status);
            case HttpStatusCode.NotFound:
                // Normalmente "model_not_found": la clave no tiene acceso a este modelo; reintentar en cada solicitud no sirve
                // de nada.
                Cool(provider, TimeSpan.FromMinutes(10));
                _logger.LogWarning("{Provider}: el modelo '{Model}' no está disponible para tu clave ({Detail}). Revisa Llm:FreeProviders:*:Model.", provider.Options.Name, provider.Options.Model, detail);
                return new LlmException($"modelo '{provider.Options.Model}' no disponible para tu clave ({detail})", status);
            case HttpStatusCode.RequestEntityTooLarge:
                return new LlmException("solicitud demasiado grande para el plan gratuito", status);
            default:
                if (status >= 500) Cool(provider, TimeSpan.FromSeconds(30));
                return new LlmException($"error {status}: {detail}", status);
        }
    }

    /// <summary>Lee la respuesta del proveedor (texto o JSON estructurado) y calcula los tokens.</summary>
    private static LlmResponse Parse(string payload, string model, bool structured, string providerName)
    {
        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        var content = string.Empty;
        string? finish = null;
        if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
        {
            var first = choices[0];
            if (first.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var c) && c.ValueKind == JsonValueKind.String)
                content = c.GetString() ?? string.Empty;
            if (first.TryGetProperty("finish_reason", out var f) && f.ValueKind == JsonValueKind.String) finish = f.GetString();
        }
        if (finish == "length") finish = "max_tokens";

        int input = 0, output = 0;
        if (root.TryGetProperty("usage", out var usage))
        {
            if (usage.TryGetProperty("prompt_tokens", out var p) && p.ValueKind == JsonValueKind.Number) input = p.GetInt32();
            if (usage.TryGetProperty("completion_tokens", out var o) && o.ValueKind == JsonValueKind.Number) output = o.GetInt32();
        }

        content = content.Trim();
        return new LlmResponse(
            structured ? string.Empty : content,
            structured ? StripFences(content) : null,
            input, output, finish,
            root.TryGetProperty("model", out var m) && m.ValueKind == JsonValueKind.String ? m.GetString() ?? model : model,
            LlmProviders.FreePrefix + providerName);
    }

    /// <summary>A veces los modelos envuelven el JSON en bloques ```json incluso en modo JSON.</summary>
    private static string StripFences(string text)
    {
        var t = text.Trim();
        if (!t.StartsWith("```")) return t;
        var firstNewline = t.IndexOf('\n');
        if (firstNewline < 0) return t;
        var body = t[(firstNewline + 1)..];
        var end = body.LastIndexOf("```", StringComparison.Ordinal);
        return (end >= 0 ? body[..end] : body).Trim();
    }

    /// <summary>Extrae el mensaje de error de la respuesta del proveedor.</summary>
    private static string ExtractError(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            // Gemini envuelve sus errores en un arreglo: [{"error": {...}}].
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0) root = root[0];
            if (root.ValueKind != JsonValueKind.Object) return "error";

            if (root.TryGetProperty("error", out var e))
            {
                if (e.ValueKind == JsonValueKind.String) return e.GetString() ?? "error";
                if (e.ValueKind == JsonValueKind.Object && e.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String) return m.GetString() ?? "error";
            }
            if (root.TryGetProperty("errors", out var errs) && errs.ValueKind == JsonValueKind.Array && errs.GetArrayLength() > 0
                && errs[0].ValueKind == JsonValueKind.Object && errs[0].TryGetProperty("message", out var em)) return em.GetString() ?? "error";
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException) { }
        return "error";
    }

    /// <summary>
    /// Por qué una respuesta gratuita no es suficientemente buena (null = utilizable); si no lo es, se pasa
    /// al siguiente proveedor o a Claude.
    /// </summary>
    internal static string? Unacceptable(LlmRequest request, LlmResponse response)
    {
        if (request.StructuredOutput is not null)
        {
            if (string.IsNullOrWhiteSpace(response.StructuredJson)) return "no devolvió el JSON esperado";
            try
            {
                using var doc = JsonDocument.Parse(response.StructuredJson);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return "el JSON no es un objeto";
            }
            catch (JsonException)
            {
                return "devolvió JSON inválido";
            }
            return null;
        }

        return string.IsNullOrWhiteSpace(response.Text) ? "devolvió una respuesta vacía" : null;
    }
    #endregion

}
