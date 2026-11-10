using System.Net;
using System.Text.Json;
using AI.Agent.Office.Api.Extensions;
using AI.Agent.Office.Api.Health;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AI.Agent.Office.Tests;

/// <summary>Spec 005 (RF-004): <c>/health/ready</c> responde 200/503 con el estado por componente y sin datos sensibles.</summary>
public class HealthTests
{
    private const string Secret = @"C:\ruta\secreta\clave-sk-123456";

    private static async Task<(HttpStatusCode Code, JsonElement Body, string Raw)> CallAsync(bool allHealthy)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddHealthChecks()
            .AddCheck("agentes", () => HealthCheckResult.Healthy(), tags: new[] { ReadinessTags.Ready })
            .AddCheck("proveedores-modelos",
                () => allHealthy ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy(Secret, new InvalidOperationException(Secret)),
                tags: new[] { ReadinessTags.Ready })
            .AddCheck("otro", () => HealthCheckResult.Unhealthy("no debe contar"));
        await using var app = builder.Build();
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
        app.MapOfficeHealth();
        await app.StartAsync();
        using var http = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };

        var response = await http.GetAsync("/health/ready");
        var raw = await response.Content.ReadAsStringAsync();
        var live = await http.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, live.StatusCode);                  // «vivo» no cambia
        await app.StopAsync();
        return (response.StatusCode, JsonDocument.Parse(raw).RootElement.Clone(), raw);
    }

    [Fact]
    public async Task Ready_returns_200_with_each_component_when_everything_is_loaded()
    {
        var (code, body, _) = await CallAsync(allHealthy: true);

        Assert.Equal(HttpStatusCode.OK, code);
        Assert.Equal("ready", body.GetProperty("status").GetString());
        var names = body.GetProperty("checks").EnumerateArray().Select(c => c.GetProperty("name").GetString()).ToList();
        Assert.Equal(new[] { "agentes", "proveedores-modelos" }, names.OrderBy(n => n));   // solo las etiquetadas «ready»
    }

    [Fact]
    public async Task Ready_returns_503_without_paths_keys_or_error_messages_when_a_component_fails()
    {
        var (code, body, raw) = await CallAsync(allHealthy: false);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, code);
        Assert.Equal("not-ready", body.GetProperty("status").GetString());
        Assert.Contains(body.GetProperty("checks").EnumerateArray(),
            c => c.GetProperty("name").GetString() == "proveedores-modelos" && c.GetProperty("status").GetString() == "not-ready");
        Assert.DoesNotContain("secreta", raw);
        Assert.DoesNotContain("sk-123456", raw);
    }
}
