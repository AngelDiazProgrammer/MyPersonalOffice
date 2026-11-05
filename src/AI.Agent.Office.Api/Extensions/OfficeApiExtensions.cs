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

public static class OfficeApiExtensions
{
    public const string CorsPolicy = "LocalOnly";

    /// <summary>Registers the whole backend (Application + Infrastructure + controllers + SignalR).</summary>
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

/// <summary>Loads the agent Markdown files into the registry when the application starts.</summary>
public sealed class OfficeBootstrapper : IHostedService
{
    private readonly IAgentCatalogService _catalog;
    private readonly IAnthropicService _anthropic;
    private readonly ILogger<OfficeBootstrapper> _logger;

    public OfficeBootstrapper(IAgentCatalogService catalog, IAnthropicService anthropic, ILogger<OfficeBootstrapper> logger)
    {
        _catalog = catalog;
        _anthropic = anthropic;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _catalog.ReloadAsync(cancellationToken);
        if (!_anthropic.IsConfigured)
            _logger.LogWarning("ANTHROPIC_API_KEY no está configurada: la oficina arrancará pero no podrá llamar a Claude.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
