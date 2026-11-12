using ATLAS.Api.Extensions;
using ATLAS.Application.DTOs;
using ATLAS.Application.Interfaces;
using ATLAS.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace ATLAS.Tests;

/// <summary>Spec 005: registros correlacionados, con duración, sin contenido y en JSON (RF-001, RF-002, RF-007, RNF-001).</summary>
public class LoggingTests
{
    private const string PrivateText = "TEXTO-PRIVADO-DE-LA-PERSONA-98765";

    private static string Everything(CapturedLog log) =>
        log.Message + " " + string.Join(" ", log.Fields.Select(f => $"{f.Key}={f.Value}")) + " " + string.Join(" ", log.Scope.Select(f => $"{f.Key}={f.Value}"));

    [Fact]
    public async Task All_the_logs_of_one_request_share_a_server_generated_correlation_id_and_tasks_add_their_own_ids()
    {
        var capture = new CapturingLoggerProvider();
        var h = new OfficeHarness(loggers: capture.CreateFactory());
        h.Script("""{"action":"delegate","message":"plan","tasks":[{"id":"t1","agentId":"developer","title":"Hacer","task":"Hazlo"}]}""");

        var first = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Programa algo " + PrivateText), CancellationToken.None);
        var second = await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Otra cosa", first.ConversationId), CancellationToken.None);

        var withCorrelation = capture.Entries.Where(e => e.Scope.ContainsKey("CorrelationId")).ToList();
        Assert.NotEmpty(withCorrelation);
        var ids = withCorrelation.Select(e => (string)e.Scope["CorrelationId"]!).Distinct().ToList();
        Assert.Equal(2, ids.Count);                                                          // una por solicitud
        Assert.All(ids, id => Assert.Equal(12, id.Length));                                  // generado en el servidor (12 caracteres hexadecimales)
        Assert.All(withCorrelation, e => Assert.Equal(first.ConversationId, (Guid)e.Scope["ConversationId"]!));
        var taskLog = capture.Entries.Where(e => e.Category.EndsWith("AgentRunner")).ToList();
        Assert.NotEmpty(taskLog);
        Assert.All(taskLog, e => { Assert.True(e.Scope.ContainsKey("TaskId")); Assert.Equal("developer", e.Scope["AgentId"]); Assert.True(e.Scope.ContainsKey("CorrelationId")); });
        Assert.NotNull(second);
    }

    [Fact]
    public async Task The_logs_never_contain_the_text_of_the_persons_message_or_the_agents_answers()
    {
        var capture = new CapturingLoggerProvider();
        var h = new OfficeHarness(loggers: capture.CreateFactory());
        h.Script("""{"action":"delegate","message":"plan","tasks":[{"id":"t1","agentId":"developer","title":"Hacer","task":"Hazlo"}]}""",
            (_, _) => Task.FromResult(FakeAnthropicService.Text("RESPUESTA-CONFIDENCIAL-4321")));

        await h.Orchestration.HandleMessageAsync(new ChatRequestDto("Mensaje " + PrivateText), CancellationToken.None);

        Assert.NotEmpty(capture.Entries);
        Assert.DoesNotContain(capture.Entries, e => Everything(e).Contains(PrivateText));
        Assert.DoesNotContain(capture.Entries, e => Everything(e).Contains("RESPUESTA-CONFIDENCIAL-4321"));
    }
}

public class JsonLoggingConfigurationTests
{
    [Fact]
    public void The_office_logging_uses_the_json_console_formatter_with_scopes_and_utc_time()
    {
        var provider = new ServiceCollection().AddLogging(b => b.AddOfficeLogging()).BuildServiceProvider();

        var console = provider.GetRequiredService<IOptionsMonitor<ConsoleLoggerOptions>>().CurrentValue;
        var json = provider.GetRequiredService<IOptionsMonitor<JsonConsoleFormatterOptions>>().CurrentValue;

        Assert.Equal("json", console.FormatterName);
        Assert.True(json.IncludeScopes);
        Assert.True(json.UseUtcTimestamp);
        Assert.False(string.IsNullOrWhiteSpace(json.TimestampFormat));
    }
}
