using System.Net;
using System.Text;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Services;
using AI.Agent.Office.Infrastructure.Anthropic;
using AI.Agent.Office.Infrastructure.Configuration;
using AI.Agent.Office.Infrastructure.Documents;
using Microsoft.Extensions.Logging.Abstractions;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace AI.Agent.Office.Tests;

public class AnthropicServiceTests
{
    private const string SecretKey = "sk-ant-test-SECRET-123";

    private sealed class ScriptedHandler : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();
        public List<(string Body, HttpRequestMessage Request)> Calls { get; } = new();

        public ScriptedHandler Enqueue(HttpStatusCode status, string body, TimeSpan? retryAfter = null)
        {
            _responses.Enqueue(_ =>
            {
                var response = new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
                if (retryAfter is { } delay) response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(delay);
                return response;
            });
            return this;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            Calls.Add((body, request));
            return _responses.Dequeue()(request);
        }
    }

    private sealed class Factory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;
        public Factory(HttpMessageHandler handler) => _handler = handler;
        public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
    }

    private static (AnthropicService Service, ScriptedHandler Handler, TokenUsageTracker Usage) Create(Action<AnthropicOptions>? configure = null)
    {
        var handler = new ScriptedHandler();
        var options = new AnthropicOptions { ApiKey = SecretKey, Model = "claude-test", MaxRetries = 2 };
        configure?.Invoke(options);
        var usage = new TokenUsageTracker();
        return (new AnthropicService(new Factory(handler), Microsoft.Extensions.Options.Options.Create(options), usage,
            NullLogger<AnthropicService>.Instance), handler, usage);
    }

    private static LlmRequest Request(StructuredOutputSpec? spec = null) =>
        new("sistema", new[] { new LlmMessage("user", "hola") }, 500, StructuredOutput: spec, Purpose: "test");

    private const string TextOk = """{"model":"claude-test","stop_reason":"end_turn","content":[{"type":"text","text":"Hola "},{"type":"text","text":"mundo"}],"usage":{"input_tokens":12,"output_tokens":7}}""";

    [Fact]
    public async Task Sends_key_in_header_only_and_parses_text_and_usage()
    {
        var (service, handler, usage) = Create();
        handler.Enqueue(HttpStatusCode.OK, TextOk);

        var response = await service.SendAsync(Request());

        Assert.Equal("Hola mundo", response.Text);
        Assert.Equal(12, response.InputTokens);
        Assert.Equal(7, response.OutputTokens);
        var (body, message) = handler.Calls.Single();
        Assert.Equal(SecretKey, message.Headers.GetValues("x-api-key").Single());
        Assert.Equal("2023-06-01", message.Headers.GetValues("anthropic-version").Single());
        Assert.DoesNotContain(SecretKey, body);
        Assert.Contains("\"model\":\"claude-test\"", body);
        Assert.Contains("\"system\":\"sistema\"", body);
        Assert.Equal(1, usage.GetSnapshot().Requests);
        Assert.Equal(19, usage.GetSnapshot().InputTokens + usage.GetSnapshot().OutputTokens);
    }

    [Fact]
    public async Task Structured_output_forces_the_tool_and_returns_its_json_input()
    {
        var (service, handler, _) = Create();
        handler.Enqueue(HttpStatusCode.OK, """{"content":[{"type":"tool_use","name":"submit_decision","input":{"action":"respond","message":"hola"}}],"usage":{"input_tokens":1,"output_tokens":1}}""");

        var response = await service.SendAsync(Request(new StructuredOutputSpec("submit_decision", "d", """{"type":"object","properties":{}}""")));

        Assert.Contains("\"action\":\"respond\"", response.StructuredJson);
        Assert.Contains("\"tool_choice\":{\"type\":\"tool\",\"name\":\"submit_decision\"}", handler.Calls.Single().Body);
    }

    [Fact]
    public async Task Rate_limits_are_retried_honoring_Retry_After()
    {
        var (service, handler, _) = Create();
        handler.Enqueue(HttpStatusCode.TooManyRequests, """{"error":{"message":"slow down"}}""", TimeSpan.FromMilliseconds(10))
               .Enqueue(HttpStatusCode.OK, TextOk);

        var response = await service.SendAsync(Request());

        Assert.Equal("Hola mundo", response.Text);
        Assert.Equal(2, handler.Calls.Count);
    }

    [Fact]
    public async Task Exhausted_retries_surface_a_rate_limit_error()
    {
        var (service, handler, _) = Create(o => o.MaxRetries = 1);
        handler.Enqueue((HttpStatusCode)429, "{}", TimeSpan.Zero).Enqueue((HttpStatusCode)429, "{}", TimeSpan.Zero);

        var ex = await Assert.ThrowsAsync<LlmException>(() => service.SendAsync(Request()));

        Assert.True(ex.IsRateLimit);
        Assert.Equal(2, handler.Calls.Count);
    }

    [Theory]
    [InlineData(401, "no es válida")]
    [InlineData(404, "ANTHROPIC_MODEL")]
    [InlineData(400, "rechazó")]
    public async Task Http_errors_map_to_safe_messages_without_leaking_the_key(int status, string expected)
    {
        var (service, handler, _) = Create();
        handler.Enqueue((HttpStatusCode)status, """{"error":{"message":"detalle"}}""");

        var ex = await Assert.ThrowsAsync<LlmException>(() => service.SendAsync(Request()));

        Assert.Contains(expected, ex.Message);
        Assert.DoesNotContain(SecretKey, ex.ToString());
        Assert.Single(handler.Calls); // 4xx are not retried
    }

    [Fact]
    public async Task Missing_key_fails_fast_without_any_http_call()
    {
        var (service, handler, _) = Create(o => o.ApiKey = "");

        Assert.False(service.IsConfigured);
        var ex = await Assert.ThrowsAsync<LlmException>(() => service.SendAsync(Request()));

        Assert.Contains("ANTHROPIC_API_KEY", ex.Message);
        Assert.Empty(handler.Calls);
    }

    [Fact]
    public async Task Oversized_inputs_are_rejected_before_calling_the_api()
    {
        var (service, handler, _) = Create(o => o.MaxInputTokensEstimate = 100);

        await Assert.ThrowsAsync<LlmException>(() => service.SendAsync(
            new LlmRequest("s", new[] { new LlmMessage("user", new string('x', 5000)) }, 100)));
        Assert.Empty(handler.Calls);
    }

    [Fact]
    public async Task Cancellation_is_propagated()
    {
        var (service, handler, _) = Create();
        handler.Enqueue(HttpStatusCode.OK, TextOk);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.SendAsync(Request(), cts.Token));
    }

    [Fact]
    public async Task Max_tokens_is_clamped_to_the_configured_ceiling()
    {
        var (service, handler, _) = Create(o => o.MaxOutputTokens = 256);
        handler.Enqueue(HttpStatusCode.OK, TextOk);

        await service.SendAsync(Request());

        Assert.Contains("\"max_tokens\":256", handler.Calls.Single().Body);
    }
}

public class PdfExtractionTests
{
    private static byte[] BuildPdf(params string[] pageTexts)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        foreach (var text in pageTexts)
        {
            var page = builder.AddPage(PageSize.A4);
            if (text.Length > 0) page.AddText(text, 12, new PdfPoint(40, 700), font);
        }
        return builder.Build();
    }

    private static PdfPigTextExtractor Extractor() => new(NullLogger<PdfPigTextExtractor>.Instance);

    [Fact]
    public async Task Extracts_text_page_by_page()
    {
        var pdf = BuildPdf("Este documento habla de arquitectura limpia y capas bien separadas.",
                           "La segunda pagina describe el modulo de pagos con detalle suficiente.");

        var result = await Extractor().ExtractAsync(new MemoryStream(pdf), "x.pdf", 50, 100_000, default);

        Assert.False(result.RequiresOcr);
        Assert.Equal(2, result.PageCount);
        Assert.Contains("arquitectura limpia", result.Text);
        Assert.Contains("--- Página 2 ---", result.Text);
        Assert.Contains("modulo de pagos", result.Text);
    }

    [Fact]
    public async Task Pdf_without_text_layer_is_flagged_as_requiring_ocr()
    {
        var result = await Extractor().ExtractAsync(new MemoryStream(BuildPdf("", "")), "scan.pdf", 50, 100_000, default);

        Assert.True(result.RequiresOcr);
        Assert.Equal(string.Empty, result.Text);
    }

    [Fact]
    public async Task Output_is_truncated_to_the_character_limit()
    {
        var pdf = BuildPdf(string.Join(" ", Enumerable.Repeat("palabra", 40)), string.Join(" ", Enumerable.Repeat("otra", 40)));

        var result = await Extractor().ExtractAsync(new MemoryStream(pdf), "x.pdf", 50, 120, default);

        Assert.True(result.Truncated);
        Assert.True(result.Text.Length <= 120);
    }

    [Fact]
    public async Task Non_pdf_content_is_rejected_and_temp_files_are_cleaned_up()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "ai-agent-office");
        var before = Directory.Exists(tempDir) ? Directory.GetFiles(tempDir).Length : 0;

        await Assert.ThrowsAsync<OfficeValidationException>(() =>
            Extractor().ExtractAsync(new MemoryStream(Encoding.UTF8.GetBytes("MZ esto no es un pdf")), "x.pdf", 50, 1000, default));

        var after = Directory.Exists(tempDir) ? Directory.GetFiles(tempDir).Length : 0;
        Assert.Equal(before, after);
    }
}

/// <summary>Runs only when ANTHROPIC_API_KEY is available; otherwise it is reported as skipped.</summary>
public sealed class LiveAnthropicFactAttribute : FactAttribute
{
    public LiveAnthropicFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")))
            Skip = "Define ANTHROPIC_API_KEY para ejecutar las pruebas de integración con Anthropic.";
    }
}

public class AnthropicLiveIntegrationTests
{
    private static AnthropicService Create()
    {
        var options = new AnthropicOptions
        {
            ApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")!,
            Model = Environment.GetEnvironmentVariable("ANTHROPIC_MODEL") is { Length: > 0 } m ? m : "claude-sonnet-5-5"
        };
        return new AnthropicService(new DefaultFactory(), Microsoft.Extensions.Options.Options.Create(options),
            new TokenUsageTracker(), NullLogger<AnthropicService>.Instance);
    }

    private sealed class DefaultFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new() { Timeout = TimeSpan.FromSeconds(60) };
    }

    [LiveAnthropicFact]
    public async Task Claude_answers_a_plain_request()
    {
        var response = await Create().SendAsync(new LlmRequest("Responde en una palabra.",
            new[] { new LlmMessage("user", "Di 'hola'.") }, 50, Purpose: "live"));

        Assert.False(string.IsNullOrWhiteSpace(response.Text));
        Assert.True(response.OutputTokens > 0);
    }

    [LiveAnthropicFact]
    public async Task Claude_returns_a_structured_orchestrator_decision()
    {
        var spec = AI.Agent.Office.Application.Orchestration.DecisionSchema.Build(new[] { "developer", "architect" }, 3);
        var response = await Create().SendAsync(new LlmRequest(
            "Eres un orquestador. Responde llamando a la herramienta.",
            new[] { new LlmMessage("user", "Diseña la arquitectura de una API y revisa su código.") }, 1024,
            StructuredOutput: spec, Purpose: "live"));

        var decision = AI.Agent.Office.Application.Orchestration.OrchestratorDecision.Parse(response.StructuredJson!);
        Assert.NotNull(decision);
        Assert.Empty(AI.Agent.Office.Application.Validators.OrchestratorDecisionValidator.Validate(decision, new[] { "developer", "architect" }, 3));
    }
}
