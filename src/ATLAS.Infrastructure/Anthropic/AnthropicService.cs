using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ATLAS.Application.Interfaces;
using ATLAS.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATLAS.Infrastructure.Anthropic;

/// <summary>
/// Puerta única a la API de mensajes de Anthropic (Claude). La clave se queda en esta clase: solo viaja
/// en el encabezado <c>x-api-key</c> y nunca se registra ni llega al frontend.
/// </summary>
public sealed class AnthropicService : IAnthropicService
{
    public const string HttpClientName = "anthropic";

    /// <summary>Códigos HTTP que se reintentan (límite de uso y errores temporales del servidor).</summary>
    private static readonly HashSet<HttpStatusCode> RetryableStatusCodes = new()
    {
        HttpStatusCode.TooManyRequests,
        HttpStatusCode.InternalServerError,
        HttpStatusCode.BadGateway,
        HttpStatusCode.ServiceUnavailable,
        HttpStatusCode.GatewayTimeout,
        (HttpStatusCode)529 // Anthropic "overloaded"
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AnthropicOptions _options;
    private readonly ITokenUsageTracker _usage;
    private readonly ILogger<AnthropicService> _logger;
    private readonly SemaphoreSlim _concurrency;

    /// <summary>Crea el servicio con la configuración y el cliente HTTP.</summary>
    public AnthropicService(
        IHttpClientFactory httpClientFactory,
        IOptions<AnthropicOptions> options,
        ITokenUsageTracker usage,
        ILogger<AnthropicService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _usage = usage;
        _logger = logger;
        _concurrency = new SemaphoreSlim(Math.Max(1, _options.MaxConcurrentRequests));
    }

    #region Envío y reintentos

    /// <summary>Hay una clave de API configurada.</summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    /// <summary>
    /// Envía la solicitud a Claude. Si el modelo rechaza tool_choice forzado o el parámetro temperature,
    /// reintenta sin ellos y lo recuerda.
    /// </summary>
    public async Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            throw new LlmException("Falta la variable de entorno ANTHROPIC_API_KEY. Configúrala y reinicia la aplicación.");

        var model = string.IsNullOrWhiteSpace(request.Model) ? _options.Model : request.Model!;
        var maxTokens = Math.Clamp(request.MaxTokens, 1, _options.MaxOutputTokens);
        var forceTool = !_forcedToolChoiceUnsupported.ContainsKey(model);
        var sendTemperature = !_temperatureUnsupported.ContainsKey(model);
        var body = BuildBody(request, model, maxTokens, forceTool, _options.PromptCaching, sendTemperature);

        var estimatedInput = (request.System.Length + request.Messages.Sum(m => m.Content.Length)) / 3;
        if (estimatedInput > _options.MaxInputTokensEstimate)
            throw new LlmException("El contenido es demasiado grande para enviarlo a Claude. Reduce el tamaño de los documentos o del mensaje.");

        await _concurrency.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            try
            {
                return await SendWithRetriesAsync(body, request.Purpose ?? "request", model, cancellationToken).ConfigureAwait(false);
            }
            catch (LlmException ex) when (forceTool && request.StructuredOutput is not null
                                          && ex.StatusCode == 400 && ex.Message.Contains("tool_choice", StringComparison.OrdinalIgnoreCase))
            {
                // Algunos modelos (por ejemplo con razonamiento extendido) rechazan tool_choice forzado: se pasa a "auto".
                _forcedToolChoiceUnsupported[model] = true;
                _logger.LogInformation("El modelo {Model} no admite tool_choice forzado; se usará 'auto'.", model);
                var autoBody = BuildBody(request, model, maxTokens, forceTool: false, _options.PromptCaching, sendTemperature);
                return await SendWithRetriesAsync(autoBody, request.Purpose ?? "request", model, cancellationToken).ConfigureAwait(false);
            }
            catch (LlmException ex) when (sendTemperature && request.Temperature is not null
                                          && ex.StatusCode == 400 && ex.Message.Contains("temperature", StringComparison.OrdinalIgnoreCase))
            {
                // Los modelos nuevos descartan el parámetro temperature: se recuerda y se reintenta sin él.
                _temperatureUnsupported[model] = true;
                _logger.LogInformation("El modelo {Model} no admite 'temperature'; se enviará sin ese parámetro.", model);
                var plainBody = BuildBody(request, model, maxTokens, forceTool, _options.PromptCaching, sendTemperature: false);
                return await SendWithRetriesAsync(plainBody, request.Purpose ?? "request", model, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            _concurrency.Release();
        }
    }

    /// <summary>Envía el cuerpo con reintentos y espera creciente ante límites de uso y errores temporales.</summary>
    private async Task<LlmResponse> SendWithRetriesAsync(string body, string purpose, string model, CancellationToken ct)
    {
        var maxAttempts = Math.Max(1, _options.MaxRetries + 1);

        for (var attempt = 1; ; attempt++)
        {
            ct.ThrowIfCancellationRequested();
            HttpResponseMessage? response = null;
            try
            {
                using var message = CreateMessage(body);
                var client = _httpClientFactory.CreateClient(HttpClientName);
                response = await client.SendAsync(message, HttpCompletionOption.ResponseContentRead, ct).ConfigureAwait(false);

                var payload = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var parsed = Parse(payload, model);
                    _usage.Record(purpose, parsed.InputTokens, parsed.OutputTokens, free: false, provider: "claude");
                    _logger.LogInformation("Anthropic {Purpose} OK ({Model}, {In} in / {Out} out tokens)",
                        purpose, parsed.Model, parsed.InputTokens, parsed.OutputTokens);
                    return parsed;
                }

                var apiMessage = Llm.ErrorText.Clean(ExtractErrorMessage(payload));
                _logger.LogWarning("Anthropic {Purpose} devolvió {Status}: {Error}", purpose, (int)response.StatusCode, apiMessage);

                if (RetryableStatusCodes.Contains(response.StatusCode) && attempt < maxAttempts)
                {
                    var delay = RetryDelay(response, attempt);
                    _logger.LogInformation("Reintento {Attempt}/{Max} en {Delay:0.0}s", attempt, maxAttempts - 1, delay.TotalSeconds);
                    await Task.Delay(delay, ct).ConfigureAwait(false);
                    continue;
                }

                throw ToException(response.StatusCode, apiMessage);
            }
            catch (HttpRequestException ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(ex, "Error de red hacia Anthropic (intento {Attempt})", attempt);
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(2 * attempt, 10)), ct).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                throw new LlmException("No se pudo conectar con la API de Anthropic. Verifica tu conexión.", inner: ex);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                throw new LlmException("La API de Anthropic no respondió a tiempo.", inner: ex);
            }
            finally
            {
                response?.Dispose();
            }
        }
    }

    /// <summary>Arma la petición HTTP con la clave y las cabeceras requeridas.</summary>
    private HttpRequestMessage CreateMessage(string body)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl.TrimEnd('/')}/v1/messages")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        message.Headers.Add("x-api-key", _options.ApiKey);
        message.Headers.Add("anthropic-version", _options.ApiVersion);
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return message;
    }

    #endregion

    #region Construcción del cuerpo de la petición

    /// <summary>Modelos que rechazaron tool_choice forzado; desde entonces usan "auto".</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, bool> _forcedToolChoiceUnsupported = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Modelos que rechazaron el parámetro temperature.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, bool> _temperatureUnsupported = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Arma el JSON de la petición: modelo, instrucciones (con caché de prompt), mensajes y herramienta
    /// estructurada.
    /// </summary>
    private static string BuildBody(LlmRequest request, string model, int maxTokens, bool forceTool, bool promptCaching, bool sendTemperature = true)
    {
        var root = new JsonObject
        {
            ["model"] = model,
            ["max_tokens"] = maxTokens,
            // Se cachea el prompt del sistema (estable): las llamadas repetidas con el mismo prompt se cobran mucho más
            // barato.
            ["system"] = promptCaching
                ? new JsonArray(new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = request.System,
                    ["cache_control"] = new JsonObject { ["type"] = "ephemeral" }
                })
                : request.System,
            ["messages"] = new JsonArray(request.Messages
                .Select(m => (JsonNode)new JsonObject { ["role"] = m.Role, ["content"] = m.Content })
                .ToArray())
        };

        if (sendTemperature && request.Temperature is { } temperature) root["temperature"] = temperature;

        if (request.StructuredOutput is { } spec)
        {
            root["tools"] = new JsonArray(new JsonObject
            {
                ["name"] = spec.Name,
                ["description"] = spec.Description,
                ["input_schema"] = JsonNode.Parse(spec.InputSchemaJson)
            });
            root["tool_choice"] = forceTool
                ? new JsonObject { ["type"] = "tool", ["name"] = spec.Name }
                : new JsonObject { ["type"] = "auto" };
        }

        return root.ToJsonString();
    }

    #endregion

    #region Lectura de respuestas y errores

    /// <summary>Lee la respuesta de Claude: texto, JSON de la herramienta y tokens (incluida la caché).</summary>
    private static LlmResponse Parse(string payload, string requestedModel)
    {
        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        var text = new StringBuilder();
        string? structured = null;

        if (root.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
        {
            foreach (var block in content.EnumerateArray())
            {
                var type = block.TryGetProperty("type", out var t) ? t.GetString() : null;
                if (type == "text" && block.TryGetProperty("text", out var tx)) text.Append(tx.GetString());
                else if (type == "tool_use" && block.TryGetProperty("input", out var input)) structured = input.GetRawText();
            }
        }

        var inputTokens = 0;
        var outputTokens = 0;
        if (root.TryGetProperty("usage", out var usage))
        {
            if (usage.TryGetProperty("input_tokens", out var i)) inputTokens = i.GetInt32();
            if (usage.TryGetProperty("cache_creation_input_tokens", out var cc) && cc.ValueKind == JsonValueKind.Number) inputTokens += cc.GetInt32();
            if (usage.TryGetProperty("cache_read_input_tokens", out var cr) && cr.ValueKind == JsonValueKind.Number) inputTokens += cr.GetInt32();
            if (usage.TryGetProperty("output_tokens", out var o)) outputTokens = o.GetInt32();
        }

        return new LlmResponse(
            text.ToString(),
            structured,
            inputTokens,
            outputTokens,
            root.TryGetProperty("stop_reason", out var stop) ? stop.GetString() : null,
            root.TryGetProperty("model", out var m) ? m.GetString() ?? requestedModel : requestedModel,
            LlmProviders.Claude);
    }

    /// <summary>Espera antes de reintentar: la que indica el servidor o una creciente.</summary>
    private static TimeSpan RetryDelay(HttpResponseMessage response, int attempt)
    {
        if (response.Headers.RetryAfter?.Delta is { } delta)
            return delta > TimeSpan.FromSeconds(30) ? TimeSpan.FromSeconds(30) : delta;

        return TimeSpan.FromSeconds(Math.Min(Math.Pow(2, attempt), 20));
    }

    /// <summary>Extrae el mensaje de error del cuerpo de la respuesta.</summary>
    private static string ExtractErrorMessage(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("error", out var error) && error.TryGetProperty("message", out var message))
                return message.GetString() ?? "Error desconocido";
        }
        catch (JsonException) { }

        return "Error desconocido";
    }

    /// <summary>Traduce el código HTTP de error en un mensaje claro para la persona (sin exponer la clave).</summary>
    private static LlmException ToException(HttpStatusCode status, string apiMessage) => status switch
    {
        HttpStatusCode.Unauthorized => new LlmException("La API key de Anthropic no es válida o fue revocada.", 401),
        HttpStatusCode.Forbidden => new LlmException("La API key no tiene permisos para este recurso o modelo.", 403),
        HttpStatusCode.NotFound => new LlmException($"Modelo o recurso no encontrado en Anthropic: {apiMessage}. Revisa ANTHROPIC_MODEL.", 404),
        HttpStatusCode.TooManyRequests => new LlmException("Se alcanzó el límite de solicitudes de Anthropic. Inténtalo de nuevo en unos instantes.", 429, isRateLimit: true),
        (HttpStatusCode)529 => new LlmException("Anthropic está sobrecargado en este momento. Inténtalo de nuevo más tarde.", 529),
        HttpStatusCode.BadRequest => new LlmException($"Anthropic rechazó la solicitud: {apiMessage}", 400),
        _ => new LlmException($"Anthropic devolvió un error ({(int)status}).", (int)status)
    };
    #endregion

}
