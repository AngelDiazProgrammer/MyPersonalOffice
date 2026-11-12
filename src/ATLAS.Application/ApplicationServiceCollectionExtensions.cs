using ATLAS.Application.Agents;
using ATLAS.Application.Browser;
using ATLAS.Application.Camila;
using ATLAS.Application.Interfaces;
using ATLAS.Application.Options;
using ATLAS.Application.Orchestration;
using ATLAS.Application.Services;
using ATLAS.Application.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATLAS.Application;

/// <summary>Registro de los servicios de la capa de aplicación en la inyección de dependencias.</summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>Registra almacenes, orquestación, flujo de conocimiento, oficinas y opciones de configuración.</summary>
    public static IServiceCollection AddOfficeApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OrchestrationOptions>(configuration.GetSection(OrchestrationOptions.SectionName));
        services.Configure<WorkspacesOptions>(configuration.GetSection(WorkspacesOptions.SectionName));
        services.AddSingleton<IWorkspaceService, WorkspaceService>();
        services.Configure<KnowledgeOptions>(configuration.GetSection(KnowledgeOptions.SectionName));
        services.Configure<PricingOptions>(configuration.GetSection(PricingOptions.SectionName));
        services.Configure<DocumentOptions>(configuration.GetSection(DocumentOptions.SectionName));
        services.Configure<BrowserOptions>(configuration.GetSection(BrowserOptions.SectionName));

        // Shared in-memory state (singletons, internally synchronized).
        services.AddSingleton<IAgentRegistry, AgentRegistry>();
        services.AddSingleton<ITaskStore, TaskStore>();
        services.AddSingleton<IConversationStore, ConversationStore>();
        services.AddSingleton<IPendingChoiceStore, PendingChoiceStore>();
        services.AddSingleton<IActivityLog, ActivityLog>();
        services.AddSingleton<IDocumentStore, DocumentStore>();
        services.AddSingleton<ITokenUsageTracker, TokenUsageTracker>();
        services.AddSingleton<IExecutionLimiter, ExecutionLimiter>();
        services.AddSingleton<IOfficeEventService, OfficeEventService>();
        services.AddSingleton<IAgentStateService, AgentStateService>();
        services.AddSingleton<IAgentCatalogService, AgentCatalogService>();
        services.AddSingleton<IAgentAdminService, AgentAdminService>();
        services.AddSingleton<IOfficeQueryService, OfficeQueryService>();

        // Conectividad a navegadores (spec 007): el ejecutor, el flujo del chat y la gestión de sitios y playbooks.
        services.AddSingleton<IBrowserPendingStore, BrowserPendingStore>();
        services.AddSingleton<BrowserRunner>();
        services.AddSingleton<BrowserManager>();
        services.AddSingleton<BrowserWorkflow>();

        // Camila IA (spec 009): oficinas UML y Conversacional sobre el bot de Dialogflow. El estado vive solo en memoria.
        services.Configure<CamilaOptions>(configuration.GetSection(CamilaOptions.SectionName));
        services.AddSingleton<CamilaSessionStore>();
        services.AddSingleton<ICamilaWebhookService, CamilaWebhookService>();
        services.AddSingleton<ICamilaGeneratorRunner, CamilaGeneratorRunner>();
        services.AddSingleton<CamilaWorkflow>();

        // Servicios por solicitud (dependen del cliente de Anthropic basado en HttpClient).
        services.AddScoped<IAgentRunner, AgentRunner>();
        services.AddScoped<PlanExecutor>();
        services.AddScoped<IOrchestrationService, OrchestrationService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<KnowledgeWorkflow>();
        services.AddSingleton<WordExportService>();
        services.AddSingleton<IWordExportService>(sp => sp.GetRequiredService<WordExportService>());
        services.AddSingleton<IPdfExportService>(sp => sp.GetRequiredService<WordExportService>());

        return services;
    }
}
