using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Api.Auth;

/// <summary>Token con el que la interfaz (que corre en el servidor) llama a su propia API; nunca llega al navegador (spec 010, D-4).</summary>
public sealed class InternalApiToken
{
    public const string HeaderName = "X-Office-Internal";

    public InternalApiToken(string value) => Value = value;

    public string Value { get; }
}

/// <summary>Autenticación y autorización de la plataforma (spec 010).</summary>
public static class OfficeAuthExtensions
{
    public const string CookieScheme = "OfficeCookie";
    public const string InternalScheme = "OfficeInternal";

    #region Servicios

    /// <summary>
    /// Registra la cookie de sesión, el esquema interno y una política general que exige sesión en TODO lo que no se marque como público.
    /// El token interno viene de <c>Office:InternalToken</c> o, si no hay, es aleatorio por proceso.
    /// </summary>
    public static IServiceCollection AddOfficeAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var configured = configuration["Office:InternalToken"];
        services.AddSingleton(new InternalApiToken(string.IsNullOrWhiteSpace(configured) ? Convert.ToHexString(RandomNumberGenerator.GetBytes(32)) : configured));
        services.AddSingleton<IUserStore>(new MarkdownUserStore());
        services.AddSingleton(new LoginThrottle());
        services.AddAntiforgery();

        services.AddAuthentication(CookieScheme)
            .AddCookie(CookieScheme, o =>
            {
                o.Cookie.Name = "office.auth";
                o.Cookie.HttpOnly = true;
                o.Cookie.SameSite = SameSiteMode.Strict;
                o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                o.ExpireTimeSpan = TimeSpan.FromHours(8);
                o.SlidingExpiration = true;
                o.LoginPath = "/login";
                o.Events.OnRedirectToLogin = ctx => Reject(ctx.HttpContext, ctx.RedirectUri, StatusCodes.Status401Unauthorized);
                o.Events.OnRedirectToAccessDenied = ctx => Reject(ctx.HttpContext, null, StatusCodes.Status403Forbidden);
            })
            .AddScheme<AuthenticationSchemeOptions, InternalTokenHandler>(InternalScheme, _ => { });

        services.AddAuthorization(o =>
            o.FallbackPolicy = new AuthorizationPolicyBuilder(CookieScheme, InternalScheme).RequireAuthenticatedUser().Build());
        return services;
    }

    /// <summary>Las llamadas a la API o a los canales en tiempo real reciben 401; las de páginas, la redirección al acceso.</summary>
    private static Task Reject(HttpContext context, string? redirect, int status)
    {
        var path = context.Request.Path;
        var isApi = path.StartsWithSegments("/api") || path.StartsWithSegments("/hubs") || path.StartsWithSegments("/_blazor");
        if (redirect is not null && !isApi) context.Response.Redirect(redirect);
        else context.Response.StatusCode = status;
        return Task.CompletedTask;
    }

    #endregion

    #region Pipeline y endpoints

    /// <summary>Agrega autenticación y autorización. Va después de los archivos estáticos y antes de los endpoints y del antifalsificación.</summary>
    public static WebApplication UseOfficeAuth(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    /// <summary>Pantalla de acceso (<c>/login</c>), envío (<c>/auth/login</c>) y salida (<c>/auth/logout</c>); son lo único público junto con /health.</summary>
    public static IEndpointRouteBuilder MapOfficeAuth(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/login", (HttpContext ctx, IAntiforgery antiforgery) =>
        {
            if (ctx.User.Identity?.IsAuthenticated == true) return Results.Redirect("/");
            var tokens = antiforgery.GetAndStoreTokens(ctx);
            var page = LoginPage.Render(tokens.FormFieldName, tokens.RequestToken ?? string.Empty,
                ctx.Request.Query.ContainsKey("error"), ctx.Request.Query.ContainsKey("bloqueado"), SafeReturn(ctx.Request.Query["returnUrl"]));
            return Results.Content(page, "text/html; charset=utf-8");
        }).AllowAnonymous();

        endpoints.MapPost("/auth/login", async (HttpContext ctx, IAntiforgery antiforgery, IUserStore users, LoginThrottle throttle, ILoggerFactory loggers) =>
        {
            var log = loggers.CreateLogger("AI.Agent.Office.Api.Auth.Login");
            await antiforgery.ValidateRequestAsync(ctx);

            var form = await ctx.Request.ReadFormAsync();
            var user = form["usuario"].ToString().Trim();
            var password = form["clave"].ToString();
            var returnUrl = SafeReturn(form["returnUrl"]);
            var origin = ctx.Connection.RemoteIpAddress?.ToString() ?? "local";

            if (throttle.IsLocked(user, origin))
            {
                log.LogWarning("Acceso bloqueado temporalmente por intentos fallidos (origen {Origin})", origin);
                return Results.Redirect("/login?bloqueado=1");
            }

            var found = users.Validate(user, password);
            if (found is null)
            {
                throttle.Fail(user, origin);
                log.LogWarning("Intento de acceso fallido (origen {Origin})", origin);
                return Results.Redirect("/login?error=1" + (returnUrl is null ? string.Empty : "&returnUrl=" + Uri.EscapeDataString(returnUrl)));
            }

            throttle.Succeed(user, origin);
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, found.Name), new Claim(ClaimTypes.Role, found.Role) }, CookieScheme);
            await ctx.SignInAsync(CookieScheme, new ClaimsPrincipal(identity));
            log.LogInformation("Sesión iniciada por {User} ({Role})", found.Name, found.Role);
            return Results.Redirect(returnUrl ?? "/");
        }).AllowAnonymous();

        endpoints.MapGet("/auth/logout", async (HttpContext ctx, ILoggerFactory loggers) =>
        {
            loggers.CreateLogger("AI.Agent.Office.Api.Auth.Login").LogInformation("Sesión cerrada por {User}", ctx.User.Identity?.Name ?? "(desconocido)");
            await ctx.SignOutAsync(CookieScheme);
            return Results.Redirect("/login");
        }).AllowAnonymous();

        return endpoints;
    }

    /// <summary>Solo se acepta volver a una ruta local (evita redirecciones a otros sitios).</summary>
    private static string? SafeReturn(string? url) =>
        !string.IsNullOrEmpty(url) && url.StartsWith('/') && !url.StartsWith("//") && !url.StartsWith("/\\")
        && !url.StartsWith("/login", StringComparison.Ordinal) && !url.StartsWith("/auth", StringComparison.Ordinal) ? url : null;

    #endregion
}

/// <summary>Autentica las llamadas internas que llevan el token del proceso (y solo esas).</summary>
public sealed class InternalTokenHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly InternalApiToken _token;

    public InternalTokenHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, InternalApiToken token)
        : base(options, logger, encoder) => _token = token;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(InternalApiToken.HeaderName, out var supplied)) return Task.FromResult(AuthenticateResult.NoResult());

        var a = Encoding.UTF8.GetBytes(supplied.ToString());
        var b = Encoding.UTF8.GetBytes(_token.Value);
        if (a.Length != b.Length || !CryptographicOperations.FixedTimeEquals(a, b)) return Task.FromResult(AuthenticateResult.Fail("Token interno no válido."));

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "interno"), new Claim(ClaimTypes.Role, "admin") }, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }

    // Este esquema solo autentica: el desafío (redirección al acceso o 401) lo decide la cookie, no el token interno.
    protected override Task HandleChallengeAsync(AuthenticationProperties properties) => Task.CompletedTask;

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties) => Task.CompletedTask;
}

/// <summary>HTML de la pantalla de acceso (sin Blazor: la cookie de sesión solo puede fijarse en una petición HTTP normal).</summary>
internal static class LoginPage
{
    public static string Render(string field, string token, bool error, bool locked, string? returnUrl)
    {
        static string E(string s) => WebUtility.HtmlEncode(s);
        var message = locked ? "Demasiados intentos. Espera unos minutos e inténtalo de nuevo."
            : error ? "Usuario o contraseña incorrectos." : string.Empty;
        var alert = message.Length > 0 ? "<div class=\"err\" role=\"alert\">" + E(message) + "</div>" : string.Empty;

        return $$"""
            <!DOCTYPE html>
            <html lang="es"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><meta name="color-scheme" content="dark">
            <title>Acceso · Agent Office</title>
            <style>
            :root{--bg-0:#070b19;--panel:rgba(17,25,54,.8);--line:rgba(139,92,246,.28);--text:#e6e9f8;--muted:#8f97bd;--violet:#8b5cf6;--cyan:#38bdf8}
            *{box-sizing:border-box}html,body{height:100%;margin:0}
            body{background:radial-gradient(1200px 700px at 15% -10%,#1d1450 0,transparent 60%),radial-gradient(900px 600px at 100% 110%,#0f2a5a 0,transparent 55%),var(--bg-0);color:var(--text);font-family:"Segoe UI",system-ui,sans-serif;font-size:14px;display:grid;place-items:center}
            main{width:min(380px,92vw);background:var(--panel);border:1px solid var(--line);border-radius:18px;padding:28px;box-shadow:0 20px 60px rgba(3,4,20,.55),0 0 40px rgba(139,92,246,.1)}
            h1{margin:0 0 4px;font-size:1.5rem}p{margin:0 0 18px;color:var(--muted);font-size:12px}
            label{display:block;margin:12px 0 4px;font-size:12px;color:var(--muted)}
            input{width:100%;padding:10px 12px;border-radius:10px;border:1px solid var(--line);background:#0b1226;color:var(--text);font-size:14px}
            input:focus{outline:2px solid var(--violet);outline-offset:1px}
            button{margin-top:18px;width:100%;padding:11px;border:0;border-radius:10px;background:linear-gradient(135deg,#8b5cf6,#38bdf8);color:#fff;font-weight:600;font-size:14px;cursor:pointer}
            .err{margin:14px 0 0;padding:8px 10px;border-radius:8px;background:rgba(248,113,113,.12);border:1px solid rgba(248,113,113,.4);color:#fca5a5;font-size:12px}
            </style></head><body>
            <main>
              <h1>🏢 Agent Office</h1>
              <p>Orquestador de agentes · acceso de administración</p>
              <form method="post" action="/auth/login" autocomplete="off">
                <input type="hidden" name="{{E(field)}}" value="{{E(token)}}">
                <input type="hidden" name="returnUrl" value="{{E(returnUrl ?? string.Empty)}}">
                <label for="u">Usuario</label><input id="u" name="usuario" required autofocus autocomplete="username">
                <label for="c">Contraseña</label><input id="c" name="clave" type="password" required autocomplete="current-password">
                <button type="submit">Entrar</button>
                {{alert}}
              </form>
            </main></body></html>
            """;
    }
}
