using System.Net;
using System.Text;
using ATLAS.Application.Interfaces;
using ATLAS.Application.Services;
using ATLAS.Infrastructure.Anthropic;
using ATLAS.Infrastructure.Configuration;
using ATLAS.Infrastructure.Llm;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace ATLAS.Tests;

public class LlmRoutingTests
{
    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, string, HttpResponseMessage> _respond;
        public List<(string Url, string Body, HttpRequestMessage Request)> Calls { get; } = new();

        public FakeHandler(Func<HttpRequestMessage, string, HttpResponseMessage> respond) => _respond = respond;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(ct);
            Calls.Add((request.RequestUri!.ToString(), body, request));
            return _respond(request, body);
        }

        public static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK, TimeSpan? retryAfter = null)
        {
            var r = new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
            if (retryAfter is { } d) r.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(d);
            return r;
        }
    }

    private sealed class NamedFactory : IHttpClientFactory
    {
        private readonly Dictionary<string, HttpMessageHandler> _handlers;
        public NamedFactory(Dictionary<string, HttpMessageHandler> handlers) => _handlers = handlers;
        public HttpClient CreateClient(string name) => new(_handlers[name], disposeHandler: false);
    }

    private static string Reply(string content, string model = "llama-3.3-70b-versatile", string finish = "stop")
    {
        var message = System.Text.Json.JsonSerializer.Serialize(new { role = "assistant", content });
        return "{\"model\":\"" + model + "\",\"choices\":[{\"index\":0,\"message\":" + message +
               ",\"finish_reason\":\"" + finish + "\"}],\"usage\":{\"prompt_tokens\":40,\"completion_tokens\":20}}";
    }

    private const string ClaudeText = """{"model":"claude-test","stop_reason":"end_turn","content":[{"type":"text","text":"respuesta claude"}],"usage":{"input_tokens":10,"output_tokens":5}}""";

    private sealed class Rig
    {
        public required FakeHandler Free { get; init; }
        public required FakeHandler Claude { get; init; }
        public required RoutingLlmService Router { get; init; }
        public required FreeApiService FreeService { get; init; }
        public required TokenUsageTracker Usage { get; init; }
    }

    private static LlmOptions TwoProviders(Action<LlmOptions>? configure = null)
    {
        var o = new LlmOptions
        {
            FreeProviders =
            {
                new FreeProviderOptions { Name = "groq", Region = "US", BaseUrl = "https://groq.test/openai/v1", ApiKeyVariable = "GROQ_API_KEY", Model = "llama-70b", MaxInputTokens = 6000 },
                new FreeProviderOptions { Name = "cloudflare", Region = "Global", BaseUrl = "https://cf.test/accounts/{CLOUDFLARE_ACCOUNT_ID}/ai/v1", ApiKeyVariable = "CLOUDFLARE_API_TOKEN", Model = "@cf/meta/llama", MaxInputTokens = 16000 }
            }
        };
        configure?.Invoke(o);
        return o;
    }

    private static Rig Create(
        Func<HttpRequestMessage, string, HttpResponseMessage>? free = null,
        Func<HttpRequestMessage, string, HttpResponseMessage>? claude = null,
        Action<LlmOptions>? configure = null,
        Dictionary<string, string?>? env = null,
        string anthropicKey = "sk-ant-test",
        Microsoft.Extensions.Logging.ILoggerFactory? loggers = null)
    {
        var freeHandler = new FakeHandler(free ?? ((_, _) => FakeHandler.Json(Reply("respuesta gratis"))));
        var claudeHandler = new FakeHandler(claude ?? ((_, _) => FakeHandler.Json(ClaudeText)));
        var factory = new NamedFactory(new() { [FreeApiService.HttpClientName] = freeHandler, [AnthropicService.HttpClientName] = claudeHandler });

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(env ?? new Dictionary<string, string?>
        {
            ["GROQ_API_KEY"] = "gsk-secret-groq",
            ["CLOUDFLARE_API_TOKEN"] = "cf-secret-token",
            ["CLOUDFLARE_ACCOUNT_ID"] = "acc123"
        }).Build();

        var options = Microsoft.Extensions.Options.Options.Create(TwoProviders(configure));
        var usage = new TokenUsageTracker();
        var freeService = new FreeApiService(factory, options, configuration, usage, NullLogger<FreeApiService>.Instance);
        var anthropic = new AnthropicService(factory,
            Microsoft.Extensions.Options.Options.Create(new AnthropicOptions { ApiKey = anthropicKey, Model = "claude-test", MaxRetries = 0 }),
            usage, NullLogger<AnthropicService>.Instance);
        var router = new RoutingLlmService(anthropic, freeService, options, Microsoft.Extensions.Logging.LoggerFactoryExtensions.CreateLogger<RoutingLlmService>(loggers ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance));
        return new Rig { Free = freeHandler, Claude = claudeHandler, Router = router, FreeService = freeService, Usage = usage };
    }

    private static LlmRequest Plain(string? preference = null, bool sensitive = false, string text = "hola", int maxTokens = 500) =>
        new("sistema", new[] { new LlmMessage("user", text) }, maxTokens, Purpose: "agent:test", ProviderPreference: preference, Sensitive: sensitive);

    private static LlmRequest Structured() =>
        new("sistema", new[] { new LlmMessage("user", "planifica") }, 500,
            StructuredOutput: new StructuredOutputSpec("submit_decision", "d", """{"type":"object","properties":{"action":{"type":"string"}}}"""),
            Purpose: "orchestrator:plan");

    [Fact]
    public async Task Free_first_answers_with_the_first_provider_and_never_calls_Claude()
    {
        var rig = Create();

        var response = await rig.Router.SendAsync(Plain());

        Assert.Equal("respuesta gratis", response.Text);
        Assert.Equal("free:groq", response.Provider);
        Assert.Empty(rig.Claude.Calls);
        var usage = rig.Usage.GetSnapshot();
        Assert.Equal(20, usage.FreeOutputTokens);
        Assert.Equal(1, usage.FreeRequests);
    }

    [Fact]
    public async Task One_generic_service_serves_every_provider_with_its_own_url_key_and_model()
    {
        var rig = Create(free: (req, _) => req.RequestUri!.Host == "groq.test"
            ? FakeHandler.Json("""{"error":{"message":"boom"}}""", HttpStatusCode.InternalServerError)
            : FakeHandler.Json(Reply("desde cloudflare", "@cf/meta/llama")));

        var response = await rig.Router.SendAsync(Plain());

        Assert.Equal("free:cloudflare", response.Provider);
        var (groqUrl, groqBody, groqReq) = rig.Free.Calls[0];
        var (cfUrl, cfBody, cfReq) = rig.Free.Calls[1];
        Assert.Equal("https://groq.test/openai/v1/chat/completions", groqUrl);
        Assert.Equal("https://cf.test/accounts/acc123/ai/v1/chat/completions", cfUrl);          // {CLOUDFLARE_ACCOUNT_ID} resolved
        Assert.Equal("Bearer gsk-secret-groq", groqReq.Headers.Authorization!.ToString());
        Assert.Equal("Bearer cf-secret-token", cfReq.Headers.Authorization!.ToString());
        Assert.Contains("\"model\":\"llama-70b\"", groqBody);
        Assert.Contains("\"model\":\"@cf/meta/llama\"", cfBody);
        Assert.DoesNotContain("secret", groqBody + cfBody);                                      // keys only in headers
        Assert.Empty(rig.Claude.Calls);
    }

    [Fact]
    public async Task A_rate_limited_provider_is_paused_and_the_next_one_answers()
    {
        var rig = Create(free: (req, _) => req.RequestUri!.Host == "groq.test"
            ? FakeHandler.Json("""{"error":{"message":"rate limit"}}""", HttpStatusCode.TooManyRequests, TimeSpan.FromMinutes(5))
            : FakeHandler.Json(Reply("desde cloudflare")));

        Assert.Equal("free:cloudflare", (await rig.Router.SendAsync(Plain())).Provider);
        var callsAfterFirst = rig.Free.Calls.Count;

        // Second request: Groq is still cooling down, so it is not even contacted.
        Assert.Equal("free:cloudflare", (await rig.Router.SendAsync(Plain())).Provider);

        Assert.Equal(callsAfterFirst + 1, rig.Free.Calls.Count);
        Assert.True(rig.FreeService.GetStatuses().Single(s => s.Name == "groq").CoolingDown);
    }

    [Fact]
    public async Task When_every_free_provider_fails_Claude_takes_over()
    {
        var rig = Create(free: (_, _) => throw new HttpRequestException("offline"));

        var response = await rig.Router.SendAsync(Plain());

        Assert.Equal("claude", response.Provider);
        Assert.Equal("respuesta claude", response.Text);
        Assert.Single(rig.Claude.Calls);
    }

    [Fact]
    public async Task Rejected_keys_pause_the_provider_and_fall_through()
    {
        var rig = Create(free: (_, _) => FakeHandler.Json("""{"error":{"message":"invalid api key"}}""", HttpStatusCode.Unauthorized));

        var response = await rig.Router.SendAsync(Plain());

        Assert.Equal("claude", response.Provider);
        Assert.All(rig.FreeService.GetStatuses(), s => Assert.True(s.CoolingDown));
    }

    [Fact]
    public async Task A_model_the_key_cannot_use_pauses_the_provider_and_explains_why()
    {
        var rig = Create(free: (req, _) => req.RequestUri!.Host == "groq.test"
            ? FakeHandler.Json("""{"error":{"message":"The model `llama-70b` does not exist or you do not have access to it.","code":"model_not_found"}}""", HttpStatusCode.NotFound)
            : FakeHandler.Json(Reply("desde cloudflare")));

        Assert.Equal("free:cloudflare", (await rig.Router.SendAsync(Plain())).Provider);
        var groqCalls = rig.Free.Calls.Count(c => c.Url.StartsWith("https://groq.test"));
        Assert.Equal("free:cloudflare", (await rig.Router.SendAsync(Plain())).Provider);

        Assert.Equal(1, groqCalls);
        Assert.Equal(1, rig.Free.Calls.Count(c => c.Url.StartsWith("https://groq.test")));   // not retried on every request
        var groq = rig.FreeService.GetStatuses().Single(s => s.Name == "groq");
        Assert.True(groq.CoolingDown);
        Assert.Contains("no disponible", groq.Error);
        Assert.Null(rig.FreeService.GetStatuses().Single(s => s.Name == "cloudflare").Error);
    }

    [Fact]
    public async Task Extra_provider_parameters_are_sent_only_to_that_provider()
    {
        var rig = Create(free: (req, _) => req.RequestUri!.Host == "groq.test"
            ? FakeHandler.Json("""{"error":{"message":"boom"}}""", HttpStatusCode.InternalServerError)
            : FakeHandler.Json(Reply("ok")),
            configure: o => o.FreeProviders[0].ExtraParameters["reasoning_effort"] = "low");

        await rig.Router.SendAsync(Plain());

        Assert.Contains("\"reasoning_effort\":\"low\"", rig.Free.Calls[0].Body);   // groq
        Assert.DoesNotContain("reasoning_effort", rig.Free.Calls[1].Body);             // cloudflare
    }

    [Fact]
    public async Task Startup_check_flags_a_configured_model_missing_from_the_providers_model_list()
    {
        var rig = Create(free: (req, _) => req.RequestUri!.AbsolutePath.EndsWith("/models") && req.RequestUri.Host == "groq.test"
            ? FakeHandler.Json("""{"data":[{"id":"openai/gpt-oss-120b"},{"id":"qwen/qwen3.8-27b"}]}""")
            : req.RequestUri!.AbsolutePath.EndsWith("/models")
                ? FakeHandler.Json("""{"data":[{"id":"@cf/meta/llama"}]}""")
                : FakeHandler.Json(Reply("ok")));

        await rig.FreeService.ValidateModelsAsync(CancellationToken.None);

        var groq = rig.FreeService.GetStatuses().Single(s => s.Name == "groq");
        Assert.Contains("llama-70b", groq.Error);                                       // configured model is not in the list
        Assert.True(groq.CoolingDown);
        Assert.Null(rig.FreeService.GetStatuses().Single(s => s.Name == "cloudflare").Error);
    }

    [Fact]
    public async Task Startup_check_accepts_model_ids_listed_with_the_models_prefix_like_Gemini_does()
    {
        var rig = Create(free: (req, _) => req.RequestUri!.AbsolutePath.EndsWith("/models") && req.RequestUri.Host == "groq.test"
            ? FakeHandler.Json("""{"data":[{"id":"models/llama-70b"}]}""")
            : FakeHandler.Json(Reply("ok")));

        await rig.FreeService.ValidateModelsAsync(CancellationToken.None);

        Assert.Null(rig.FreeService.GetStatuses().Single(s => s.Name == "groq").Error);   // "models/llama-70b" == "llama-70b"
    }

    [Fact]
    public async Task An_error_wrapped_in_a_json_array_like_Geminis_falls_through_to_the_next_provider_instead_of_crashing()
    {
        var rig = Create(free: (req, _) => req.RequestUri!.Host == "groq.test"
            ? FakeHandler.Json("""[{"error":{"code":503,"message":"alta demanda"}}]""", HttpStatusCode.ServiceUnavailable)
            : FakeHandler.Json(Reply("respuesta del segundo")));

        var response = await rig.Router.SendAsync(Plain());

        Assert.Equal("free:cloudflare", response.Provider);
        Assert.Contains("alta demanda", rig.FreeService.GetStatuses().Single(s => s.Name == "groq").Error);
    }

    [Fact]
    public async Task The_provider_test_reports_success_or_the_real_error_without_touching_the_order()
    {
        var rig = Create(free: (req, _) => req.RequestUri!.Host == "groq.test"
            ? FakeHandler.Json("""[{"error":{"message":"modelo saturado"}}]""", HttpStatusCode.ServiceUnavailable)
            : FakeHandler.Json(Reply("OK")));

        var bad = await rig.FreeService.TestProviderAsync("groq", null, CancellationToken.None);
        var good = await rig.FreeService.TestProviderAsync("cloudflare", null, CancellationToken.None);
        var unknown = await rig.FreeService.TestProviderAsync("nope", null, CancellationToken.None);

        Assert.False(bad.Ok);
        Assert.Contains("modelo saturado", bad.Error);
        Assert.True(good.Ok);
        Assert.Equal("OK", good.Reply);
        Assert.False(unknown.Ok);
    }

    [Fact]
    public async Task Every_provider_call_logs_provider_model_duration_and_tokens_without_the_message_content()
    {
        var capture = new ATLAS.Tests.TestSupport.CapturingLoggerProvider();
        var rig = Create(loggers: capture.CreateFactory());

        await rig.Router.SendAsync(new LlmRequest("sistema", new[] { new LlmMessage("user", "CONTENIDO-SECRETO-555") }, 200, Purpose: "agent:developer"));

        var call = Assert.Single(capture.Entries, e => e.Message.StartsWith("Llamada a modelo"));
        Assert.Equal("agent:developer", call.Fields["Purpose"]);
        Assert.True(call.Fields.ContainsKey("Provider") && call.Fields.ContainsKey("Model"));
        Assert.True(Convert.ToInt64(call.Fields["DurationMs"]) >= 0);
        Assert.True(call.Fields.ContainsKey("InputTokens") && call.Fields.ContainsKey("OutputTokens"));
        Assert.DoesNotContain(capture.Entries, e => e.Message.Contains("CONTENIDO-SECRETO-555") || e.Fields.Values.Any(v => v?.ToString()?.Contains("CONTENIDO-SECRETO-555") == true));
    }

    [Fact]
    public async Task A_failed_provider_call_logs_its_duration_and_error_type_but_not_the_content()
    {
        var capture = new ATLAS.Tests.TestSupport.CapturingLoggerProvider();
        var rig = Create(claude: (_, _) => FakeHandler.Json("""{"error":{"message":"fallo"}}""", HttpStatusCode.BadRequest), loggers: capture.CreateFactory());

        await Assert.ThrowsAnyAsync<Exception>(() =>
            rig.Router.SendAsync(new LlmRequest("sistema", new[] { new LlmMessage("user", "OTRO-SECRETO-777") }, 200, Purpose: "agent:qa", ProviderPreference: "claude")));

        var failure = Assert.Single(capture.Entries, e => e.Message.StartsWith("Llamada a modelo") && e.Level == Microsoft.Extensions.Logging.LogLevel.Warning);
        Assert.True(failure.Fields.ContainsKey("DurationMs") && failure.Fields.ContainsKey("ErrorType"));
        Assert.DoesNotContain(capture.Entries, e => e.Message.Contains("OTRO-SECRETO-777"));
    }

    [Fact]
    public async Task Empty_free_answers_are_skipped_in_favor_of_the_next_provider()
    {
        var rig = Create(free: (req, _) => FakeHandler.Json(Reply(req.RequestUri!.Host == "groq.test" ? "" : "ahora sí")));

        Assert.Equal("free:cloudflare", (await rig.Router.SendAsync(Plain())).Provider);
    }

    [Fact]
    public async Task Structured_requests_use_json_mode_and_put_the_schema_in_the_prompt()
    {
        var rig = Create(free: (_, _) => FakeHandler.Json(Reply("```json\n{\"action\":\"respond\"}\n```")));

        var response = await rig.Router.SendAsync(Structured());

        Assert.Equal("{\"action\":\"respond\"}", response.StructuredJson);   // fences stripped
        var body = rig.Free.Calls.Single().Body;
        Assert.Contains("\"response_format\":{\"type\":\"json_object\"}", body);
        Assert.Contains("submit_decision", body);
        Assert.Contains("properties", body);                                  // schema text reached the model
    }

    [Fact]
    public async Task Providers_that_reject_json_mode_are_retried_once_without_it_and_remembered()
    {
        var rig = Create(free: (_, body) => body.Contains("response_format")
            ? FakeHandler.Json("""{"error":{"message":"response_format not supported"}}""", HttpStatusCode.BadRequest)
            : FakeHandler.Json(Reply("{\"action\":\"respond\"}")));

        var first = await rig.Router.SendAsync(Structured());
        await rig.Router.SendAsync(Structured());

        Assert.Equal("free:groq", first.Provider);
        Assert.Equal(3, rig.Free.Calls.Count);                                // json try, plain retry, then plain directly
        Assert.DoesNotContain("response_format", rig.Free.Calls[2].Body);
    }

    [Fact]
    public async Task Invalid_structured_json_from_every_free_provider_escalates_to_Claude()
    {
        var rig = Create(
            free: (_, _) => FakeHandler.Json(Reply("esto no es json")),
            claude: (_, _) => FakeHandler.Json("""{"content":[{"type":"tool_use","name":"submit_decision","input":{"action":"respond"}}],"usage":{"input_tokens":1,"output_tokens":1}}"""));

        var response = await rig.Router.SendAsync(Structured());

        Assert.Equal("claude", response.Provider);
    }

    [Fact]
    public async Task Requests_with_documents_never_reach_free_APIs()
    {
        var rig = Create();

        var response = await rig.Router.SendAsync(Plain(sensitive: true, text: "contenido confidencial del PDF"));

        Assert.Equal("claude", response.Provider);
        Assert.Empty(rig.Free.Calls);
    }

    [Fact]
    public async Task Sensitive_requests_in_free_only_mode_are_refused_instead_of_leaking()
    {
        var rig = Create();
        rig.Router.Mode = LlmMode.FreeOnly;

        var ex = await Assert.ThrowsAsync<LlmException>(() => rig.Router.SendAsync(Plain(sensitive: true)));

        Assert.Contains("documentos", ex.Message);
        Assert.Empty(rig.Free.Calls);
        Assert.Empty(rig.Claude.Calls);
    }

    [Fact]
    public async Task Claude_only_mode_skips_free_APIs()
    {
        var rig = Create(configure: o => o.Mode = LlmMode.ClaudeOnly);

        Assert.Equal("claude", (await rig.Router.SendAsync(Plain())).Provider);
        Assert.Empty(rig.Free.Calls);
    }

    [Fact]
    public async Task Per_request_preference_forces_Claude()
    {
        var rig = Create();

        Assert.Equal("claude", (await rig.Router.SendAsync(Plain(preference: "claude"))).Provider);
        Assert.Empty(rig.Free.Calls);
    }

    [Fact]
    public async Task Free_only_mode_never_uses_Claude_even_when_free_fails()
    {
        var rig = Create(free: (_, _) => throw new HttpRequestException("down"));
        rig.Router.Mode = LlmMode.FreeOnly;

        var ex = await Assert.ThrowsAsync<LlmException>(() => rig.Router.SendAsync(Plain()));

        Assert.Contains("gratuita", ex.Message);
        Assert.Empty(rig.Claude.Calls);
    }

    [Fact]
    public async Task Without_free_keys_it_goes_straight_to_Claude()
    {
        var rig = Create(env: new Dictionary<string, string?>());

        Assert.False(rig.FreeService.AnyConfigured);
        Assert.Equal("claude", (await rig.Router.SendAsync(Plain())).Provider);
        Assert.Empty(rig.Free.Calls);
    }

    [Fact]
    public async Task A_provider_with_a_missing_placeholder_variable_is_not_configured()
    {
        var rig = Create(env: new Dictionary<string, string?> { ["CLOUDFLARE_API_TOKEN"] = "t" }); // no account id, no groq key

        Assert.All(rig.FreeService.GetStatuses(), s => Assert.False(s.Configured));
    }

    [Fact]
    public async Task No_keys_at_all_explains_what_to_configure()
    {
        var rig = Create(env: new Dictionary<string, string?>(), anthropicKey: "");

        var ex = await Assert.ThrowsAsync<LlmException>(() => rig.Router.SendAsync(Plain()));

        Assert.Contains("ANTHROPIC_API_KEY", ex.Message);
    }

    [Fact]
    public async Task Prompts_larger_than_a_provider_limit_skip_it()
    {
        // ~30k chars ≈ 8.6k tokens: over groq's 6000 but within cloudflare's 16000.
        var rig = Create();

        var response = await rig.Router.SendAsync(Plain(text: new string('x', 30_000)));

        Assert.Equal("free:cloudflare", response.Provider);
        Assert.All(rig.Free.Calls, c => Assert.StartsWith("https://cf.test", c.Url));
    }

    [Fact]
    public async Task Region_filter_excludes_providers_outside_the_allowed_regions()
    {
        var rig = Create(configure: o => o.AllowedRegions.Add("Global"));

        var response = await rig.Router.SendAsync(Plain());

        Assert.Equal("free:cloudflare", response.Provider);        // groq is US-only
        Assert.All(rig.Free.Calls, c => Assert.StartsWith("https://cf.test", c.Url));
    }

    [Fact]
    public async Task Output_tokens_are_capped_per_provider_and_truncation_is_reported()
    {
        var rig = Create(free: (_, _) => FakeHandler.Json(Reply("texto cortado", finish: "length")),
            configure: o => o.FreeProviders[0].MaxOutputTokens = 300);

        var response = await rig.Router.SendAsync(Plain(maxTokens: 4000));

        Assert.Contains("\"max_tokens\":300", rig.Free.Calls[0].Body);
        Assert.Equal("max_tokens", response.StopReason);
    }

    [Fact]
    public async Task The_mode_can_be_switched_at_runtime()
    {
        var rig = Create();
        Assert.Equal("free:groq", (await rig.Router.SendAsync(Plain())).Provider);

        rig.Router.Mode = LlmMode.ClaudeOnly;

        Assert.Equal("claude", (await rig.Router.SendAsync(Plain())).Provider);
    }

    [Fact]
    public async Task User_cancellation_is_not_treated_as_a_provider_failure()
    {
        var rig = Create();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => rig.Router.SendAsync(Plain(), cts.Token));
        Assert.Empty(rig.Claude.Calls);
    }

    [Fact]
    public async Task Status_lists_each_free_provider_and_claude()
    {
        var rig = Create();

        var status = await rig.Router.GetStatusAsync();

        Assert.Equal("FreeFirst", status.Mode);
        Assert.Equal(new[] { "groq", "cloudflare" }, status.FreeProviders.Select(p => p.Name));
        Assert.All(status.FreeProviders, p => Assert.True(p.Configured));
        Assert.True(status.ClaudeConfigured);
        Assert.Equal("US", status.FreeProviders[0].Region);
    }
}
