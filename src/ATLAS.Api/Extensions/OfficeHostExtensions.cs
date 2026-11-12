using ATLAS.Application.Interfaces;
using ATLAS.Infrastructure.Configuration;
using ATLAS.Infrastructure.Documents;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ATLAS.Api.Extensions;

/// <summary>
/// Raíz de composición compartida por los hosts: expone las capacidades de Infrastructure que necesitan los hosts
/// (configuración de arranque y renderizado seguro de Markdown) sin que la capa de presentación (Web) vea Infrastructure.
/// </summary>
public static class OfficeHostExtensions
{
    /// <summary>Carga <c>.env</c> y la configuración compartida del repositorio (la prioridad sigue siendo de las variables de entorno).</summary>
    public static IConfigurationBuilder AddOfficeConfiguration(this IConfigurationBuilder builder) =>
        OfficeConfiguration.AddOfficeConfiguration(builder);

    /// <summary>
    /// Registros en JSON estructurado, una línea por evento, con hora UTC, nivel, mensaje y los identificadores de correlación
    /// (CorrelationId, ConversationId, TaskId, AgentId) como campos (spec 005, RF-007).
    /// </summary>
    public static ILoggingBuilder AddOfficeLogging(this ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddJsonConsole(o =>
        {
            o.IncludeScopes = true;
            o.UseUtcTimestamp = true;
            o.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        });

        // Registro en vivo en memoria para la sección «Logs» (spec 010).
        var store = new Logging.InMemoryLogStore();
        logging.Services.AddSingleton(store);
        logging.AddProvider(new Logging.InMemoryLoggerProvider(store));
        return logging;
    }

    /// <summary>Registra <see cref="IMarkdownRenderer"/>; lo necesita la interfaz aunque la API se consuma desde otro proceso.</summary>
    public static IServiceCollection AddOfficeRendering(this IServiceCollection services)
    {
        services.TryAddSingleton<IMarkdownRenderer, SafeMarkdownRenderer>();
        return services;
    }
}
