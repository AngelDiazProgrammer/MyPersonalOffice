using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Infrastructure.Agents;
using AI.Agent.Office.Infrastructure.Anthropic;
using AI.Agent.Office.Infrastructure.Configuration;
using AI.Agent.Office.Infrastructure.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AI.Agent.Office.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddOfficeInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AgentsOptions>(configuration.GetSection(AgentsOptions.SectionName));

        services.AddOptions<AnthropicOptions>()
            .Bind(configuration.GetSection(AnthropicOptions.SectionName))
            .PostConfigure(options =>
            {
                // Secrets and model come from environment variables (.env supported), never from source code.
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

        services.AddSingleton<IAnthropicService, AnthropicService>();
        services.AddSingleton<IAgentConfigurationService, MarkdownAgentConfigurationService>();
        services.AddSingleton<IDocumentTextExtractor, PdfPigTextExtractor>();

        return services;
    }
}
