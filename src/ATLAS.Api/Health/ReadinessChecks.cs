using System.Text.Json;
using ATLAS.Application.Interfaces;
using ATLAS.Application.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace ATLAS.Api.Health;

#region Comprobaciones de «listo» (spec 005, RF-004)

/// <summary>Etiqueta de las comprobaciones que forman el estado «listo» (<c>/health/ready</c>).</summary>
public static class ReadinessTags
{
    public const string Ready = "ready";
}

/// <summary>Los agentes se cargaron y su configuración es válida.</summary>
public sealed class AgentsReadyCheck : IHealthCheck
{
    private readonly IAgentConfigurationService _agents;

    public AgentsReadyCheck(IAgentConfigurationService agents) => _agents = agents;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var report = _agents.Validate();
        return Task.FromResult(report.Agents.Count > 0 && report.IsValid
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("No hay agentes válidos cargados."));
    }
}

/// <summary>La base de conocimiento tiene situaciones cargadas (solo si el flujo de conocimiento está activado).</summary>
public sealed class KnowledgeReadyCheck : IHealthCheck
{
    private readonly IKnowledgeBaseService _knowledge;
    private readonly KnowledgeOptions _options;

    public KnowledgeReadyCheck(IKnowledgeBaseService knowledge, IOptions<KnowledgeOptions> options)
    {
        _knowledge = knowledge;
        _options = options.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(!_options.Enabled || _knowledge.GetAll().Count > 0
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("La base de conocimiento no tiene situaciones cargadas."));
}

/// <summary>Hay al menos un proveedor de modelos configurado (una API gratuita o Claude).</summary>
public sealed class ProvidersReadyCheck : IHealthCheck
{
    private readonly ILlmStatusService _status;

    public ProvidersReadyCheck(ILlmStatusService status) => _status = status;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var status = await _status.GetStatusAsync(cancellationToken).ConfigureAwait(false);
        return status.ClaudeConfigured || status.FreeProviders.Any(p => p.Configured)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("No hay ningún proveedor de modelos configurado.");
    }
}

#endregion

/// <summary>Escribe el informe de «listo» solo con estado y nombre de cada componente: sin rutas, claves ni mensajes técnicos.</summary>
public static class ReadinessResponse
{
    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        var body = new
        {
            status = report.Status == HealthStatus.Healthy ? "ready" : "not-ready",
            checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status == HealthStatus.Healthy ? "ready" : "not-ready" })
        };
        return context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
