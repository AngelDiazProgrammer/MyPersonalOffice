using AI.Agent.Office.Application.Browser;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Infrastructure.Agents;
using AI.Agent.Office.Infrastructure.Anthropic;
using AI.Agent.Office.Infrastructure.Configuration;
using AI.Agent.Office.Infrastructure.Documents;
using AI.Agent.Office.Infrastructure.Llm;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AI.Agent.Office.Infrastructure.DependencyInjection;

/// <summary>
/// Registro de los servicios de infraestructura (modelos, documentos, archivos, base de conocimiento)
/// en la inyección de dependencias.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registra el cliente de Claude, el servicio de APIs gratuitas, el enrutador, los generadores de Word
    /// y PDF y los lectores de archivos.
    /// </summary>
    public static IServiceCollection AddOfficeInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AgentsOptions>(configuration.GetSection(AgentsOptions.SectionName));

        services.AddOptions<AnthropicOptions>()
            .Bind(configuration.GetSection(AnthropicOptions.SectionName))
            .PostConfigure(options =>
            {
                // Los secretos y el modelo vienen de variables de entorno (se admite .env), nunca del código fuente.
                var key = configuration["ANTHROPIC_API_KEY"];
                if (!string.IsNullOrWhiteSpace(key)) options.ApiKey = key.Trim();

                var model = configuration["ANTHROPIC_MODEL"];
                if (!string.IsNullOrWhiteSpace(model)) options.Model = model.Trim();
            });

        services.AddHttpClient(AnthropicService.HttpClientName, (sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AnthropicOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(Math.Max(10, options.TimeoutSeconds));
        });

        // Enrutamiento: UN servicio global de APIs gratuitas (todos los proveedores son entradas de configuración) y
        // Claude como proveedor de pago.
        services.Configure<LlmOptions>(configuration.GetSection(LlmOptions.SectionName));
        services.AddHttpClient(FreeApiService.HttpClientName, client => client.Timeout = TimeSpan.FromMinutes(3));
        services.AddSingleton<AnthropicService>();
        services.AddHostedService<FreeProviderCheck>();
        services.AddSingleton<FreeApiService>();
        services.AddSingleton<RoutingLlmService>();
        services.AddSingleton<IAnthropicService>(sp => sp.GetRequiredService<RoutingLlmService>());
        services.AddSingleton<ILlmRouterState>(sp => sp.GetRequiredService<RoutingLlmService>());
        services.AddSingleton<ILlmStatusService>(sp => sp.GetRequiredService<RoutingLlmService>());
        services.AddSingleton<IAgentConfigurationService, MarkdownAgentConfigurationService>();
        services.AddSingleton<AI.Agent.Office.Application.Agents.IAgentFileStore, MarkdownAgentFileStore>();
        services.AddSingleton<IDocumentTextExtractor, PdfPigTextExtractor>();

        services.AddSingleton<IKnowledgeBaseService, AI.Agent.Office.Infrastructure.Knowledge.MarkdownKnowledgeBaseService>();
        services.AddSingleton<IDeliveryFormatService, AI.Agent.Office.Infrastructure.Knowledge.DeliveryFormatService>();

        services.Configure<GeneratedDocumentsOptions>(configuration.GetSection(GeneratedDocumentsOptions.SectionName));
        services.AddSingleton<IWordDocumentGenerator, MarkdownWordGenerator>();
        services.AddSingleton<IPdfDocumentGenerator, MarkdownPdfGenerator>();
        services.AddSingleton<IGeneratedDocumentStore, AI.Agent.Office.Infrastructure.FileSystem.GeneratedDocumentStore>();

        // Conectividad a navegadores (spec 007): Playwright tras interfaces de Application.
        services.AddSingleton<IBrowserSiteStore, AI.Agent.Office.Infrastructure.Browser.JsonBrowserSiteStore>();
        services.AddSingleton<IBrowserCredentialStore, AI.Agent.Office.Infrastructure.Browser.MarkdownCredentialStore>();
        services.AddSingleton<IPlaybookStore, AI.Agent.Office.Infrastructure.Browser.MarkdownPlaybookStore>();
        services.AddSingleton<IBrowserSessionFactory, AI.Agent.Office.Infrastructure.Browser.PlaywrightSessionFactory>();

        // Camila IA (spec 009): la base se lee de CamilaIA.md y los webhooks salen por un cliente HTTP sin redirecciones.
        services.AddSingleton<AI.Agent.Office.Application.Camila.ICamilaBotSource, AI.Agent.Office.Infrastructure.Camila.BotPlaybookSource>();
        services.AddSingleton<AI.Agent.Office.Application.Camila.ICamilaHttpTransport, AI.Agent.Office.Infrastructure.Camila.HttpCamilaTransport>();
        services.AddHttpClient(AI.Agent.Office.Infrastructure.Camila.HttpCamilaTransport.HttpClientName, client => client.Timeout = Timeout.InfiniteTimeSpan)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false });

        return services;
    }
}
