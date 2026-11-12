using System.Text.Json;

namespace ATLAS.Api.Middleware;

#region Cabeceras de seguridad

/// <summary>
/// Agrega a toda respuesta las cabeceras que limitan lo que un navegador puede hacer con la aplicación:
/// política de contenido (solo recursos propios), sin adivinar tipos, sin enviar el origen y sin incrustarla en otros sitios.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    // 'unsafe-inline' es necesario para Blazor Server y los estilos en línea de la interfaz; no se permite 'unsafe-eval' ni cargar scripts externos.
    private const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: blob:; " +
        "font-src 'self' data:; " +
        "connect-src 'self' ws: wss:; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'self'";

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Fija las cabeceras antes de que empiece a escribirse la respuesta.</summary>
    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["Content-Security-Policy"] = ContentSecurityPolicy;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Permissions-Policy"] = "camera=(), geolocation=(), payment=()";
            headers["X-Frame-Options"] = "SAMEORIGIN";
            return Task.CompletedTask;
        });
        return _next(context);
    }
}

#endregion

#region Protección contra peticiones de otros sitios

/// <summary>
/// Bloquea las peticiones que un sitio web ajeno puede forzar al navegador de la persona (CSRF y secuestro de WebSocket):
/// <list type="bullet">
/// <item>Toda petición con cabecera <c>Origin</c> a la API o a los canales en tiempo real debe venir de un origen permitido.</item>
/// <item>Las peticiones que cambian algo (POST, PUT, PATCH, DELETE) a <c>/api</c> deben llevar la cabecera <c>X-Office-Client</c>.
/// Un sitio ajeno no puede añadir cabeceras propias sin pasar por CORS, que ya las rechaza.</item>
/// </list>
/// </summary>
public sealed class RequestGuardMiddleware
{
    /// <summary>Nombre de la cabecera que identifica a un cliente propio de la oficina.</summary>
    public const string ClientHeader = "X-Office-Client";

    private static readonly string[] SafeMethods = { "GET", "HEAD", "OPTIONS", "TRACE" };
    private static readonly string[] GuardedPrefixes = { "/api", "/hubs", "/_blazor" };

    private readonly RequestDelegate _next;
    private readonly HashSet<string> _allowedOrigins;
    private readonly ILogger<RequestGuardMiddleware> _logger;

    public RequestGuardMiddleware(RequestDelegate next, IEnumerable<string> allowedOrigins, ILogger<RequestGuardMiddleware> logger)
    {
        _next = next;
        _allowedOrigins = new HashSet<string>(allowedOrigins.Select(o => o.TrimEnd('/')), StringComparer.OrdinalIgnoreCase);
        _logger = logger;
    }

    /// <summary>Rechaza con 403 lo que no cumpla las reglas; lo demás sigue su camino.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        if (GuardedPrefixes.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
        {
            var origin = context.Request.Headers.Origin.ToString();
            if (origin.Length > 0 && !IsAllowedOrigin(origin, context.Request))
            {
                await RejectAsync(context, $"Origen no permitido: {origin}");
                return;
            }

            var unsafeMethod = !SafeMethods.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase);
            if (unsafeMethod && path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
                && !context.Request.Headers.ContainsKey(ClientHeader))
            {
                await RejectAsync(context, "Falta la cabecera " + ClientHeader);
                return;
            }
        }

        await _next(context);
    }

    /// <summary>Un origen es válido si está en la lista configurada o si es el propio sitio (mismo esquema y host).</summary>
    private bool IsAllowedOrigin(string origin, HttpRequest request)
    {
        var normalized = origin.TrimEnd('/');
        return _allowedOrigins.Contains(normalized)
               || string.Equals(normalized, $"{request.Scheme}://{request.Host}", StringComparison.OrdinalIgnoreCase);
    }

    private async Task RejectAsync(HttpContext context, string reason)
    {
        _logger.LogWarning("Petición bloqueada ({Method} {Path}): {Reason}", context.Request.Method, context.Request.Path, reason);
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { title = "Petición no permitida", status = 403, detail = "La petición no proviene de un cliente autorizado." }));
    }
}

#endregion
