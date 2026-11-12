using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Application.Camila;

#region Contratos

/// <summary>Una petición HTTP ya armada (cabeceras con los secretos resueltos).</summary>
public sealed record CamilaHttpRequest(string Method, string Url, IReadOnlyDictionary<string, string> Headers, string? Body, int TimeoutSeconds);

/// <summary>Respuesta HTTP en bruto.</summary>
public sealed record CamilaHttpResponse(int Status, string Body);

/// <summary>Transporte HTTP (lo implementa la capa de infraestructura).</summary>
public interface ICamilaHttpTransport
{
    Task<CamilaHttpResponse> SendAsync(CamilaHttpRequest request, CancellationToken cancellationToken);
}

/// <summary>Dónde está la conversación al llamar al webhook (lo usan los webhooks estándar de Dialogflow).</summary>
public sealed record CamilaWebhookContext(string? Flow, string? Page, string Text);

/// <summary>Resultado de ejecutar un webhook. <c>Detail</c> nunca contiene datos de la petición ni de la respuesta.</summary>
public sealed record CamilaWebhookResult(bool Ok, string Mode, string Detail, IReadOnlyDictionary<string, object?> Params, long ElapsedMs);

/// <summary>Ejecuta los webhooks del bot según su modo (Desactivado, Simulado, Real).</summary>
public interface ICamilaWebhookService
{
    CamilaWebhookMode ModeOf(CamilaWebhook webhook);
    bool IsWrite(CamilaWebhook webhook);
    CamilaPendingConfirm Preview(CamilaWebhook webhook, string tag, IReadOnlyDictionary<string, object?> session, CamilaWebhookContext? context = null);
    Task<CamilaWebhookResult> ExecuteAsync(CamilaWebhook webhook, string tag, IReadOnlyDictionary<string, object?> session, CancellationToken cancellationToken, CamilaWebhookContext? context = null);
}

#endregion

/// <summary>
/// Webhooks de Camila. El modo Real solo llama a hosts de la lista permitida, con los secretos de la configuración
/// (<c>.env</c>); los registros llevan webhook, modo, resultado y tiempo, nunca cuerpos, cédulas ni credenciales (spec 009, RF-018 a RF-023).
/// </summary>
public sealed partial class CamilaWebhookService : ICamilaWebhookService
{
    private static readonly string[] WriteHints = { "guardar", "envio", "envío", "finalizar", "actualizar", "crear", "registrar", "enviar", "certificad" };

    private readonly CamilaOptions _options;
    private readonly IConfiguration _configuration;
    private readonly ICamilaHttpTransport _transport;
    private readonly ILogger<CamilaWebhookService> _logger;

    /// <summary>Crea el servicio.</summary>
    public CamilaWebhookService(IOptions<CamilaOptions> options, IConfiguration configuration, ICamilaHttpTransport transport, ILogger<CamilaWebhookService> logger)
    {
        _options = options.Value;
        _configuration = configuration;
        _transport = transport;
        _logger = logger;
    }

    #region Modo y tipo

    /// <summary>Modo configurado del webhook (el propio o el predeterminado).</summary>
    public CamilaWebhookMode ModeOf(CamilaWebhook webhook) =>
        _options.WebhookModes.TryGetValue(webhook.Name, out var mode) ? mode : _options.DefaultWebhookMode;

    /// <summary>Es de escritura si el generador lo marcó así o si la configuración lo declara.</summary>
    public bool IsWrite(CamilaWebhook webhook) =>
        string.Equals(webhook.Kind, "write", StringComparison.OrdinalIgnoreCase)
        || _options.ExtraWriteWebhooks.Contains(webhook.Name, StringComparer.OrdinalIgnoreCase);

    /// <summary>Heurística por nombre que usa el generador para marcar los webhooks de escritura.</summary>
    public static string GuessKind(string name) =>
        WriteHints.Any(h => name.Contains(h, StringComparison.OrdinalIgnoreCase)) ? "write" : "read";

    #endregion

    #region Vista previa

    /// <summary>Qué se enviaría, para pedir la confirmación (la persona ve sus propios datos; no se registran).</summary>
    public CamilaPendingConfirm Preview(CamilaWebhook webhook, string tag, IReadOnlyDictionary<string, object?> session, CamilaWebhookContext? context = null)
    {
        var uri = Uri.TryCreate(webhook.Uri, UriKind.Absolute, out var u) ? u : null;
        return new CamilaPendingConfirm(webhook.Name, tag, webhook.Method, uri?.Authority ?? webhook.Host, uri?.AbsolutePath ?? webhook.Uri, BuildBody(webhook, tag, session, context) ?? string.Empty);
    }

    #endregion

    #region Ejecución

    /// <summary>Ejecuta el webhook y devuelve los parámetros que su respuesta asigna.</summary>
    public async Task<CamilaWebhookResult> ExecuteAsync(CamilaWebhook webhook, string tag, IReadOnlyDictionary<string, object?> session, CancellationToken cancellationToken, CamilaWebhookContext? context = null)
    {
        var mode = ModeOf(webhook);
        var empty = new Dictionary<string, object?>();

        if (mode == CamilaWebhookMode.Disabled)
            return Log(webhook, mode, new CamilaWebhookResult(false, "Desactivado", "El webhook está desactivado.", empty, 0));
        if (mode == CamilaWebhookMode.Simulated)
            return Log(webhook, mode, new CamilaWebhookResult(true, "Simulado", "Respuesta simulada: no se llamó al servicio y no se asignó ningún dato.", empty, 0));

        var watch = Stopwatch.StartNew();
        try
        {
            if (!Uri.TryCreate(Interpolate(webhook.Uri, session, urlEncode: true), UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                return Log(webhook, mode, Fail("La dirección del webhook no es una URL https válida.", watch));
            if (!_options.AllowedHosts.Contains(uri.Authority, StringComparer.OrdinalIgnoreCase))
                return Log(webhook, mode, Fail($"El host {uri.Authority} no está en la lista de hosts permitidos (Camila:AllowedHosts).", watch));

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (name, value) in webhook.Headers)
            {
                var resolved = SecretRef().Replace(value, m =>
                    _configuration[m.Groups[1].Value] is { Length: > 0 } secret
                        ? secret
                        : throw new InvalidOperationException($"Falta el secreto {m.Groups[1].Value} (defínelo en .env)."));
                headers[name] = Interpolate(resolved, session, urlEncode: false);
            }

            var body = BuildBody(webhook, tag, session, context);
            // Un POST siempre lleva contenido JSON (aunque sea vacío): sin él algunos servicios responden 415.
            if (body is null && webhook.Method is not ("GET" or "DELETE" or "HEAD")) body = string.Empty;
            var timeout = Math.Max(1, Math.Min(webhook.TimeoutSeconds, _options.WebhookTimeoutSeconds));
            var response = await _transport.SendAsync(new CamilaHttpRequest(webhook.Method, uri.ToString(), headers, body, timeout), cancellationToken).ConfigureAwait(false);

            if (response.Status is < 200 or >= 300)
                return Log(webhook, mode, Fail($"El servicio respondió HTTP {response.Status}.", watch));

            var assigned = ReadResponse(webhook, response.Body);
            return Log(webhook, mode, new CamilaWebhookResult(true, "Real", $"OK · {assigned.Count} dato(s) asignado(s).", assigned, watch.ElapsedMilliseconds));
        }
        catch (InvalidOperationException ex)
        {
            return Log(webhook, mode, Fail(ex.Message, watch));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Log(webhook, mode, Fail("El servicio no respondió a tiempo.", watch));
        }
        catch (HttpRequestException ex)
        {
            return Log(webhook, mode, Fail($"No se pudo conectar con el servicio ({ex.GetType().Name}). ¿Estás en la red o VPN correcta?", watch));
        }
    }

    private static CamilaWebhookResult Fail(string detail, Stopwatch watch) =>
        new(false, "Real", detail, new Dictionary<string, object?>(), watch.ElapsedMilliseconds);

    /// <summary>Solo se registra el nombre, el modo, el resultado y el tiempo (RF-023).</summary>
    private CamilaWebhookResult Log(CamilaWebhook webhook, CamilaWebhookMode mode, CamilaWebhookResult result)
    {
        _logger.LogInformation("Camila webhook {Webhook} · modo {Mode} · {Outcome} · {Elapsed} ms", webhook.Name, mode, result.Ok ? "ok" : "error", result.ElapsedMs);
        return result;
    }

    #endregion

    #region Cuerpo, URL y respuesta

    [GeneratedRegex(@"\{\{secret:([A-Za-z0-9_]+)\}\}")]
    private static partial Regex SecretRef();

    [GeneratedRegex(@"\$session\.params\.(?<n>[A-Za-z0-9_]+)")]
    private static partial Regex ParamRef();

    /// <summary>
    /// Cuerpo de la petición. Webhook FLEXIBLE: la plantilla con cada <c>$session.params.x</c> sustituido (escapado según vaya o no entre
    /// comillas). Webhook ESTÁNDAR: el WebhookRequest de Dialogflow (etiqueta, página, parámetros de sesión y texto).
    /// </summary>
    private static string? BuildBody(CamilaWebhook webhook, string tag, IReadOnlyDictionary<string, object?> session, CamilaWebhookContext? context)
    {
        if (string.Equals(webhook.Type, "standard", StringComparison.OrdinalIgnoreCase))
        {
            var request = new Dictionary<string, object?>
            {
                ["detectIntentResponseId"] = Guid.NewGuid().ToString(),
                ["pageInfo"] = new Dictionary<string, object?> { ["displayName"] = context?.Page ?? string.Empty, ["currentPage"] = $"flows/{context?.Flow}/pages/{context?.Page}" },
                ["sessionInfo"] = new Dictionary<string, object?> { ["parameters"] = session.ToDictionary(p => p.Key, p => CamilaExpression.Normalize(p.Value)) },
                ["fulfillmentInfo"] = new Dictionary<string, object?> { ["tag"] = tag },
                ["text"] = context?.Text ?? string.Empty,
                ["languageCode"] = "es"
            };
            return JsonSerializer.Serialize(request);
        }

        if (string.IsNullOrWhiteSpace(webhook.Body)) return null;
        var template = webhook.Body;
        return ParamRef().Replace(template, m =>
        {
            session.TryGetValue(m.Groups["n"].Value, out var value);
            value = CamilaExpression.Normalize(value);
            var quoted = m.Index > 0 && template[m.Index - 1] == '"' && m.Index + m.Length < template.Length && template[m.Index + m.Length] == '"';
            if (quoted) return JsonEncodedText.Encode(CamilaExpression.ToText(value)).ToString();
            return value is null ? "null" : value is string s ? JsonSerializer.Serialize(s) : JsonSerializer.Serialize(value);
        });
    }

    private static string Interpolate(string text, IReadOnlyDictionary<string, object?> session, bool urlEncode) =>
        ParamRef().Replace(text, m =>
        {
            session.TryGetValue(m.Groups["n"].Value, out var value);
            var s = CamilaExpression.ToText(CamilaExpression.Normalize(value));
            return urlEncode ? Uri.EscapeDataString(s) : s;
        });

    /// <summary>Asigna los parámetros que indica el mapeo (ruta JSON → parámetro) y los de <c>sessionInfo.parameters</c> si vienen.</summary>
    private static Dictionary<string, object?> ReadResponse(CamilaWebhook webhook, string body)
    {
        var assigned = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(body)) return assigned;

        JsonElement root;
        try { root = JsonDocument.Parse(body).RootElement; }
        catch (JsonException) { return assigned; }

        foreach (var map in webhook.Mapping)
        {
            var key = map.Parameter.StartsWith("$session.params.", StringComparison.Ordinal) ? map.Parameter["$session.params.".Length..] : map.Parameter;
            assigned[key] = SelectPath(root, map.Path) is { } found ? CamilaExpression.FromJson(found) : null;
        }

        foreach (var name in new[] { "sessionInfo", "session_info" })
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out var info) && info.TryGetProperty("parameters", out var ps) && ps.ValueKind == JsonValueKind.Object)
                foreach (var p in ps.EnumerateObject()) assigned[p.Name] = CamilaExpression.FromJson(p.Value);

        return assigned;
    }

    /// <summary>Ruta JSON mínima: <c>$.a.b[0].c</c>, <c>$['a']</c> y <c>$</c>.</summary>
    public static JsonElement? SelectPath(JsonElement root, string path)
    {
        var current = root;
        var i = path.StartsWith('$') ? 1 : 0;
        while (i < path.Length)
        {
            if (path[i] == '.')
            {
                var start = ++i;
                while (i < path.Length && path[i] != '.' && path[i] != '[') i++;
                var name = path[start..i];
                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(name, out current)) return null;
            }
            else if (path[i] == '[')
            {
                var end = path.IndexOf(']', i);
                if (end < 0) return null;
                var raw = path[(i + 1)..end].Trim();
                var quoted = raw.StartsWith('\'') || raw.StartsWith('"');
                var inner = raw.Trim('\'', '"');
                i = end + 1;
                if (!quoted && int.TryParse(inner, out var index))
                {
                    if (current.ValueKind != JsonValueKind.Array || index < 0 || index >= current.GetArrayLength()) return null;
                    current = current[index];
                }
                else if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(inner, out current)) return null;
            }
            else return null;
        }
        return current;
    }

    #endregion
}
