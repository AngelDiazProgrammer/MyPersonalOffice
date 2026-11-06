using System.Text.Json.Serialization;
using AI.Agent.Office.Api.Controllers;
using AI.Agent.Office.Api.Hubs;
using AI.Agent.Office.Api.Middleware;
using AI.Agent.Office.Application;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Domain.Interfaces;
using AI.Agent.Office.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Http.Features;

namespace AI.Agent.Office.Api.Extensions;

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

        var origins = configuration.GetSection("Office:AllowedOrigins").Get<string[]>()
                      ?? new[] { "http://localhost:5080", "https://localhost:7080", "http://localhost:5081" };
        services.AddCors(o => o.AddPolicy(CorsPolicy, p => p
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));

        return services;
    }

    /// <summary>
    /// Configura el pipeline HTTP: manejo central de errores, CORS, controladores, el canal en tiempo
    /// real y /health.
    /// </summary>
    public static WebApplication UseOfficeApi(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseCors(CorsPolicy);

        app.MapControllers();
        app.MapHub<OfficeHub>(OfficeHub.Route);
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
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
