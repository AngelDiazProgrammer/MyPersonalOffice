using AI.Agent.Office.Application.Agents;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Application.Orchestration;
using AI.Agent.Office.Application.Services;
using AI.Agent.Office.Application.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AI.Agent.Office.Application;

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
        services.AddSingleton<IOfficeQueryService, OfficeQueryService>();

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
