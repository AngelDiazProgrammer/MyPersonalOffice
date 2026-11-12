using System.Text.Json.Serialization;
using ATLAS.Api.Controllers;
using ATLAS.Api.Health;
using ATLAS.Api.Hubs;
using ATLAS.Api.Middleware;
using ATLAS.Application;
using ATLAS.Application.Interfaces;
using ATLAS.Domain.Interfaces;
using ATLAS.Infrastructure.DependencyInjection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http.Features;

namespace ATLAS.Api.Extensions;

/// <summary>Registro y configuración del backend (servicios, CORS, controladores, SignalR y manejo de errores).</summary>
public static class OfficeApiExtensions
{
    public const string CorsPolicy = "LocalOnly";

    /// <summary>Registra todo el backend (aplicación, infraestructura, controladores y SignalR).</summary>
    public static IServiceCollection AddOfficeApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOfficeApplication(configuration);
        services.AddOfficeInfrastructure(configuration);

        services.AddSingleton<IOfficeEventPublisher, SignalROfficeEventPublisher>();
        services.AddHostedService<OfficeBootstrapper>();

        services.AddControllers()
            .AddApplicationPart(typeof(ChatController).Assembly)
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddSignalR()
            .AddJsonProtocol(o => o.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 12 * 1024 * 1024);

        var origins = GetAllowedOrigins(configuration);
        services.AddCors(o => o.AddPolicy(CorsPolicy, p => p
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));

        AddOfficeRateLimiting(services, configuration);
        services.AddOfficeHealth();

        // Mensajes de error de validación en español y sin detalles internos del framework.
        services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(o => o.InvalidModelStateResponseFactory = context =>
        {
            var tooLarge = context.ModelState.Values.SelectMany(v => v.Errors)
                .Any(e => e.ErrorMessage.Contains("too large", StringComparison.OrdinalIgnoreCase));
            var detail = tooLarge ? "El archivo o el contenido supera el tamaño permitido." : "La solicitud no tiene el formato esperado.";
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new { title = "Solicitud no válida", status = 400, detail, errors = new[] { detail } });
        });

        return services;
    }

    /// <summary>
    /// Límite de peticiones al chat (cada una puede gastar cupo o dinero de los modelos): por defecto 30 por minuto por origen de red
    /// (configurable con <c>Office:ChatRequestsPerMinute</c>). Las excedentes reciben 429.
    /// </summary>
    public static IServiceCollection AddOfficeRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var chatPerMinute = configuration.GetValue("Office:ChatRequestsPerMinute", 30);
        return services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.AddPolicy(ChatRateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "local",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = chatPerMinute, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
        });
    }

    /// <summary>Registra las comprobaciones de «listo»: agentes, base de conocimiento y proveedores de modelos (spec 005, RF-004).</summary>
    public static IServiceCollection AddOfficeHealth(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<AgentsReadyCheck>("agentes", tags: new[] { ReadinessTags.Ready })
            .AddCheck<KnowledgeReadyCheck>("base-conocimiento", tags: new[] { ReadinessTags.Ready })
            .AddCheck<ProvidersReadyCheck>("proveedores-modelos", tags: new[] { ReadinessTags.Ready });
        return services;
    }

    /// <summary>Publica <c>/health/ready</c> (200 listo, 503 no listo) con el estado de cada componente y sin datos sensibles.</summary>
    public static IEndpointRouteBuilder MapOfficeHealth(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(ReadinessTags.Ready),
            ResponseWriter = ReadinessResponse.WriteAsync
        }).AllowAnonymous();
        return endpoints;
    }

    /// <summary>Nombre de la política de límite de peticiones del chat.</summary>
    public const string ChatRateLimitPolicy = "chat";

    /// <summary>Orígenes permitidos para CORS, WebSocket y peticiones que cambian algo (configurables con Office:AllowedOrigins).</summary>
    public static string[] GetAllowedOrigins(IConfiguration configuration) =>
        configuration.GetSection("Office:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:5080", "https://localhost:7080", "http://localhost:5081" };

    /// <summary>
    /// Seguridad común a los dos hosts: cabeceras y bloqueo de peticiones de otros sitios (incluido el canal de Blazor).
    /// Debe ir antes de CORS y de los endpoints.
    /// </summary>
    public static WebApplication UseOfficeSecurity(this WebApplication app)
    {
        var origins = GetAllowedOrigins(app.Configuration);
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseMiddleware<RequestGuardMiddleware>((IEnumerable<string>)origins);
        return app;
    }

    /// <summary>
    /// Configura el pipeline HTTP: manejo central de errores, CORS, controladores, el canal en tiempo
    /// real y /health.
    /// </summary>
    public static WebApplication UseOfficeApi(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseCors(CorsPolicy);
        app.UseRateLimiter();

        app.MapControllers();
        app.MapHub<OfficeHub>(OfficeHub.Route);
        app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();      // «vivo»: la aplicación responde
        app.MapOfficeHealth();                                                  // «listo»: sus componentes necesarios están cargados
        return app;
    }
}

/// <summary>
/// Al arrancar la aplicación carga los Markdown de los agentes y la base de conocimiento y avisa si
/// falta la clave de Claude.
/// </summary>
public sealed class OfficeBootstrapper : IHostedService
{
    private readonly IAgentCatalogService _catalog;
    private readonly IKnowledgeBaseService _knowledge;
    private readonly IAnthropicService _anthropic;
    private readonly ILogger<OfficeBootstrapper> _logger;

    public OfficeBootstrapper(IAgentCatalogService catalog, IKnowledgeBaseService knowledge, IAnthropicService anthropic, ILogger<OfficeBootstrapper> logger)
    {
        _catalog = catalog;
        _knowledge = knowledge;
        _anthropic = anthropic;
        _logger = logger;
    }

    /// <summary>Carga agentes y base de conocimiento al iniciar (y avisa en el registro si falta la clave de Claude).</summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _catalog.ReloadAsync(cancellationToken);
        await _knowledge.ReloadAsync(cancellationToken);
        if (!_anthropic.IsConfigured)
            _logger.LogWarning("ANTHROPIC_API_KEY no está configurada: la oficina arrancará pero no podrá llamar a Claude.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
