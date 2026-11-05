using AI.Agent.Office.Application.Agents;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Application.Orchestration;
using AI.Agent.Office.Application.Services;
using AI.Agent.Office.Application.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AI.Agent.Office.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddOfficeApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OrchestrationOptions>(configuration.GetSection(OrchestrationOptions.SectionName));
        services.Configure<PricingOptions>(configuration.GetSection(PricingOptions.SectionName));
        services.Configure<DocumentOptions>(configuration.GetSection(DocumentOptions.SectionName));

        // Shared in-memory state (singletons, internally synchronized).
        services.AddSingleton<IAgentRegistry, AgentRegistry>();
        services.AddSingleton<ITaskStore, TaskStore>();
        services.AddSingleton<IConversationStore, ConversationStore>();
        services.AddSingleton<IActivityLog, ActivityLog>();
        services.AddSingleton<IDocumentStore, DocumentStore>();
        services.AddSingleton<ITokenUsageTracker, TokenUsageTracker>();
        services.AddSingleton<IExecutionLimiter, ExecutionLimiter>();
        services.AddSingleton<IOfficeEventService, OfficeEventService>();
        services.AddSingleton<IAgentStateService, AgentStateService>();
        services.AddSingleton<IAgentCatalogService, AgentCatalogService>();
        services.AddSingleton<IOfficeQueryService, OfficeQueryService>();

        // Per-request services (they depend on the HttpClient-based Anthropic client).
        services.AddScoped<IAgentRunner, AgentRunner>();
        services.AddScoped<PlanExecutor>();
        services.AddScoped<IOrchestrationService, OrchestrationService>();
        services.AddScoped<IDocumentService, DocumentService>();

        return services;
    }
}
