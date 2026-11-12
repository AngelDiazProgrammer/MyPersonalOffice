using System.Text.Json;
using AI.Agent.Office.Application.Camila;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Application.Options;
using AI.Agent.Office.Infrastructure.Camila;
using AI.Agent.Office.Tests.TestSupport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Tests;

/// <summary>Spec 009 · Camila IA: expresiones, webhooks, motor de flujos, conversión del export y las dos oficinas.</summary>
public class CamilaTests
{
    #region Apoyo

    private sealed class FakeTransport : ICamilaHttpTransport
    {
        public List<CamilaHttpRequest> Calls { get; } = new();
        public Func<CamilaHttpRequest, CamilaHttpResponse> Respond { get; set; } = _ => new CamilaHttpResponse(200, "{\"data\":{\"nota\":4.5}}");

        public Task<CamilaHttpResponse> SendAsync(CamilaHttpRequest request, CancellationToken cancellationToken)
        {
            Calls.Add(request);
            return Task.FromResult(Respond(request));
        }
    }

    private sealed class FakeGenerators : ICamilaGeneratorRunner
    {
        public Task<string> RunAsync(CamilaGeneratorDef generator, IReadOnlyDictionary<string, string> inputs, CancellationToken cancellationToken) => Task.FromResult("generado");
    }

    private const string UmlKind = "uml";
    private const string ConversationalKind = "conversacional";

    /// <summary>Una sola fuente de bots con el bot «test» (o ninguno si no se le asigna).</summary>
    private sealed class FakeSource : ICamilaBotSource
    {
        public CamilaBot? Bot { get; set; }
        public string? NotesText { get; set; }
        public Dictionary<string, string>? NarrativeSections { get; set; }
        public IReadOnlyList<BotInfo> Bots => Bot is null ? Array.Empty<BotInfo>()
            : new[] { new BotInfo("test", "Bot de prueba", "Bot para pruebas", "🧩", "lector-uml", "camila-ia", Bot.Flows.Count, Bot.Flows.Sum(f => f.Pages.Count), Bot.Webhooks.Count) };
        public CamilaBot? Get(string botId) => Bot;
        public string? Notes(string botId) => NotesText;
        public IReadOnlyDictionary<string, string>? Narrative(string botId) => NarrativeSections;
        public IReadOnlyList<string> Problems(string botId) => Array.Empty<string>();
        public Task<int> ReloadAsync(CancellationToken cancellationToken = default) => Task.FromResult(Bot is null ? 0 : 1);
    }

    private static IOptions<CamilaOptions> Options(Action<CamilaOptions>? tweak = null)
    {
        var options = new CamilaOptions { AllowedHosts = { "api.example.com:9876" } };
        tweak?.Invoke(options);
        return Microsoft.Extensions.Options.Options.Create(options);
    }

    private static CamilaWebhookService Hooks(FakeTransport transport, IOptions<CamilaOptions> options, Dictionary<string, string?>? secrets = null) =>
        new(options, new ConfigurationBuilder().AddInMemoryCollection(secrets ?? new()).Build(), transport, NullLogger<CamilaWebhookService>.Instance);

    private static CamilaFulfillment Say(string text) => new() { Messages = { new CamilaMessage { Texts = { text } } } };

    private static CamilaWebhook Hook(string name, string kind = "read") => new()
    {
        Name = name,
        Uri = "https://api.example.com:9876/api/consulta",
        Kind = kind,
        Headers = { ["x-api-key"] = "{{secret:CAMILA_WH_TEST}}" },
        Body = "{\"cedula\":\"$session.params.numero_documento\"}",
        Mapping = { new CamilaMapping { Parameter = "$session.params.nota", Path = "$.data.nota" } }
    };

    /// <summary>Bot mínimo: bienvenida → botón → pide documento → consulta (lectura) → guardar (escritura).</summary>
    private static CamilaBot TestBot()
    {
        var bot = new CamilaBot { StartFlow = "Inicio" };
        bot.Intents.Add(new CamilaIntent { Name = "Default Welcome Intent", Phrases = { "hola", "buenas" } });
        bot.Intents.Add(new CamilaIntent { Name = "Menu_Principal", Phrases = { "volver al menú principal" } });
        bot.Entities.Add(new CamilaEntityType { Name = "Numero_Documento", Kind = "KIND_REGEXP", Values = { @"\d{5,15}" } });
        bot.Webhooks.Add(Hook("Consulta"));
        bot.Webhooks.Add(Hook("Guardar nota", "write"));

        var inicio = new CamilaFlow { Name = "Inicio" };
        inicio.Routes.Add(new CamilaRoute { Intent = "Default Welcome Intent", TargetPage = "Bienvenida" });
        inicio.Events.Add(new CamilaRoute { Event = "sys.no-match-default", Fulfillment = Say("No te entendí") });

        var bienvenida = new CamilaPage { Name = "Bienvenida" };
        bienvenida.Entry.Messages.Add(new CamilaMessage { Texts = { "Hola, soy Camila" } });
        bienvenida.Entry.Messages.Add(new CamilaMessage { Type = "buttons", Buttons = { new CamilaButton { Text = "Ver mi nota", Event = "ver_nota" }, new CamilaButton { Text = "Guardar", Event = "guardar" } } });
        bienvenida.Events.Add(new CamilaRoute { Event = "ver_nota", TargetPage = "Pedir documento" });
        bienvenida.Events.Add(new CamilaRoute { Event = "guardar", TargetPage = "Guardar" });
        bienvenida.Routes.Add(new CamilaRoute { Intent = "Menu_Principal", TargetPage = "Bienvenida" });

        var pedir = new CamilaPage { Name = "Pedir documento" };
        pedir.Form.Add(new CamilaFormParam { Name = "numero_documento", Entity = "@Numero_Documento", Required = true, Prompt = Say("¿Cuál es tu documento?") });
        pedir.Routes.Add(new CamilaRoute { Condition = "$page.params.status = \"FINAL\"", TargetPage = "Resultado" });

        var resultado = new CamilaPage { Name = "Resultado" };
        resultado.Entry.Webhook = "Consulta";
        resultado.Entry.Messages.Add(new CamilaMessage { Texts = { "Tu nota es $session.params.nota" } });

        var guardar = new CamilaPage { Name = "Guardar" };
        guardar.Entry.SetParams.Add(new CamilaSet { Parameter = "numero_documento", Value = "12345678" });
        guardar.Entry.Webhook = "Guardar nota";
        guardar.Entry.Messages.Add(new CamilaMessage { Texts = { "Guardado" } });

        inicio.Pages.AddRange(new[] { bienvenida, pedir, resultado, guardar });
        bot.Flows.Add(inicio);
        return bot;
    }

    private static (CamilaEngine Engine, CamilaSession Session, FakeTransport Transport) NewEngine(Action<CamilaOptions>? tweak = null, CamilaMode mode = CamilaMode.Uml)
    {
        var transport = new FakeTransport();
        var options = Options(tweak);
        var hooks = Hooks(transport, options, new() { ["CAMILA_WH_TEST"] = "valor-secreto" });
        return (new CamilaEngine(TestBot(), hooks, new FakeGenerators(), options), new CamilaSession(Guid.NewGuid(), mode), transport);
    }

    #endregion

    #region Expresiones

    private static CamilaExpression Expr(Dictionary<string, object?> session, Dictionary<string, object?>? page = null) =>
        new(session, page, "texto", () => new DateTime(2026, 11, 11));

    [Fact]
    public void Conditions_compare_combine_and_handle_nulls()
    {
        var s = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["existe"] = true, ["activo"] = false, ["nombre"] = "Ana", ["nota"] = 4.5, ["vacio"] = null };
        Assert.True(Expr(s).Condition("$session.params.existe = true AND $session.params.activo = false"));
        Assert.False(Expr(s).Condition("$session.params.existe = false OR $session.params.activo = true"));
        Assert.True(Expr(s).Condition("$session.params.vacio = null"));
        Assert.True(Expr(s).Condition("$session.params.nombre != null"));
        Assert.True(Expr(s).Condition("$session.params.nota > 4 AND ($session.params.nombre = \"ana\" OR false)"));
        Assert.True(Expr(s).Condition("$session.params.nombre CONTAINS \"n\""));
        Assert.True(Expr(s, new() { ["status"] = "FINAL" }).Condition("$page.params.status = \"FINAL\""));
        Assert.True(Expr(s).Condition("true"));
        Assert.False(Expr(s).Condition("esto no es ( una expresión"));          // un error de sintaxis cuenta como falso
    }

    [Fact]
    public void Functions_and_interpolation_follow_the_bot_usage()
    {
        var s = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["a"] = "Hola", ["n"] = "2", ["lista"] = "[{\"x\":1},{\"x\":2}]", ["obj"] = "{\"material\":\"guia\"}", ["url"] = "https://x/", ["usr"] = "ana"
        };
        var e = Expr(s);
        Assert.Equal("hola", e.Value("$sys.func.LOWER($session.params.a)"));
        Assert.Equal("3", e.Value("$sys.func.TO_TEXT($sys.func.ADD($sys.func.TO_NUMBER($session.params.n), 1))"));
        Assert.Equal("2", e.Value("$sys.func.TO_TEXT($sys.func.COUNT($sys.func.TO_OBJECT($session.params.lista)))"));
        Assert.Equal("guia", e.Value("$sys.func.TO_TEXT($sys.func.GET_FIELD($sys.func.TO_OBJECT($session.params.obj), \"material\"))"));
        Assert.Equal("2026-11-12", e.Value("$sys.func.FORMAT_DATE($sys.func.ADD_DATE($sys.func.NOW(), 1, \"DAYS\"), \"yyyy-MM-dd\")"));
        Assert.Equal("dos", e.Value("$if($session.params.a CONTAINS \"ol\", \"dos\", \"tres\")"));
        Assert.Equal("https://x/ana", e.Value("$session.params.url$session.params.usr"));            // referencias seguidas
        Assert.Equal("Hola, ana", e.Interpolate("$session.params.a, $session.params.usr"));
    }

    #endregion

    #region Webhooks

    [Fact]
    public async Task Webhook_real_builds_the_body_uses_the_secret_and_maps_the_response()
    {
        var transport = new FakeTransport();
        var hooks = Hooks(transport, Options(), new() { ["CAMILA_WH_TEST"] = "valor-secreto" });

        var result = await hooks.ExecuteAsync(Hook("Consulta"), "", new Dictionary<string, object?> { ["numero_documento"] = "12345678" }, CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Equal(4.5, result.Params["nota"]);
        var call = Assert.Single(transport.Calls);
        Assert.Equal("valor-secreto", call.Headers["x-api-key"]);
        Assert.Equal("12345678", JsonDocument.Parse(call.Body!).RootElement.GetProperty("cedula").GetString());
        Assert.DoesNotContain("valor-secreto", result.Detail);
    }

    [Fact]
    public async Task Webhook_to_a_host_outside_the_allow_list_is_rejected_without_calling()
    {
        var transport = new FakeTransport();
        var hooks = Hooks(transport, Options(o => o.AllowedHosts.Clear()), new() { ["CAMILA_WH_TEST"] = "x" });

        var result = await hooks.ExecuteAsync(Hook("Consulta"), "", new Dictionary<string, object?>(), CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains("no está en la lista", result.Detail);
        Assert.Empty(transport.Calls);
    }

    [Fact]
    public async Task Webhook_with_a_missing_secret_fails_without_leaking_values()
    {
        var transport = new FakeTransport();
        var result = await Hooks(transport, Options()).ExecuteAsync(Hook("Consulta"), "", new Dictionary<string, object?>(), CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains("CAMILA_WH_TEST", result.Detail);
        Assert.Empty(transport.Calls);
    }

    [Theory]
    [InlineData(CamilaWebhookMode.Disabled)]
    [InlineData(CamilaWebhookMode.Simulated)]
    public async Task Disabled_and_simulated_webhooks_never_call_the_service(CamilaWebhookMode mode)
    {
        var transport = new FakeTransport();
        var hooks = Hooks(transport, Options(o => o.DefaultWebhookMode = mode));

        var result = await hooks.ExecuteAsync(Hook("Consulta"), "", new Dictionary<string, object?>(), CancellationToken.None);

        Assert.Equal(mode == CamilaWebhookMode.Simulated, result.Ok);
        Assert.Empty(transport.Calls);
    }

    [Fact]
    public async Task Webhook_http_errors_are_reported_without_the_response_body()
    {
        var transport = new FakeTransport { Respond = _ => new CamilaHttpResponse(500, "datos-personales-del-error") };
        var hooks = Hooks(transport, Options(), new() { ["CAMILA_WH_TEST"] = "x" });

        var result = await hooks.ExecuteAsync(Hook("Consulta"), "", new Dictionary<string, object?>(), CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains("HTTP 500", result.Detail);
        Assert.DoesNotContain("datos-personales", result.Detail);
    }

    [Fact]
    public async Task Standard_webhooks_receive_the_dialogflow_request_and_a_post_without_body_still_sends_json()
    {
        var transport = new FakeTransport { Respond = _ => new CamilaHttpResponse(200, "{\"sessionInfo\":{\"parameters\":{\"idSessionWatson\":\"abc\"}}}") };
        var hooks = Hooks(transport, Options(), new() { ["CAMILA_WH_TEST"] = "x" });
        var standard = Hook("Estándar");
        standard.Type = "standard";

        var result = await hooks.ExecuteAsync(standard, "session-watson", new Dictionary<string, object?> { ["numero_documento"] = "123" }, CancellationToken.None, new CamilaWebhookContext("Inicio", "Bienvenida", "hola"));

        Assert.Equal("abc", result.Params["idSessionWatson"]);
        var body = JsonDocument.Parse(transport.Calls[0].Body!).RootElement;
        Assert.Equal("session-watson", body.GetProperty("fulfillmentInfo").GetProperty("tag").GetString());
        Assert.Equal("123", body.GetProperty("sessionInfo").GetProperty("parameters").GetProperty("numero_documento").GetString());
        Assert.Equal("Bienvenida", body.GetProperty("pageInfo").GetProperty("displayName").GetString());

        var empty = Hook("Sin cuerpo");
        empty.Body = null;
        await hooks.ExecuteAsync(empty, "", new Dictionary<string, object?>(), CancellationToken.None);
        Assert.Equal(string.Empty, transport.Calls[1].Body);                          // POST sin cuerpo: contenido vacío, no ausente (evita HTTP 415)
    }

    [Fact]
    public void Json_path_supports_indexes_and_quoted_names()
    {
        var root = JsonDocument.Parse("{\"data\":[{\"a\":{\"b c\":7}}]}").RootElement;
        Assert.Equal(7, CamilaWebhookService.SelectPath(root, "$.data[0].a['b c']")!.Value.GetInt32());
        Assert.Null(CamilaWebhookService.SelectPath(root, "$.data[3]"));
    }

    #endregion

    #region Motor de flujos

    [Fact]
    public async Task Engine_walks_buttons_forms_and_runs_the_read_webhook_automatically()
    {
        var (engine, session, transport) = NewEngine();

        var start = await engine.StartAsync(session, default);
        Assert.Equal("Hola, soy Camila", start.Messages[0].Text);
        Assert.Equal(new[] { "ver_nota", "guardar" }, start.Options.Select(o => o.Event));

        var ask = await engine.UserEventAsync(session, "ver_nota", default);
        Assert.Equal("numero_documento", ask.WaitingParam);
        Assert.Equal("¿Cuál es tu documento?", ask.Messages[0].Text);
        Assert.Empty(transport.Calls);

        var invalid = await engine.UserTextAsync(session, "abc", default);          // no cumple la entidad: sigue esperando
        Assert.Equal("numero_documento", invalid.WaitingParam);
        Assert.Contains("No te entendí", invalid.Messages.Select(m => m.Text));

        var done = await engine.UserTextAsync(session, "mi cédula es 1020304050", default);
        Assert.Null(done.WaitingParam);
        Assert.Equal("Tu nota es 4.5", done.Messages.Last().Text);
        Assert.Equal("1020304050", session.Params["numero_documento"]);
        Assert.Single(transport.Calls);
    }

    [Fact]
    public async Task Write_webhooks_pause_for_confirmation_and_run_only_after_yes()
    {
        var (engine, session, transport) = NewEngine();
        await engine.StartAsync(session, default);

        var pause = await engine.UserEventAsync(session, "guardar", default);
        Assert.NotNull(pause.Confirm);
        Assert.Equal("Guardar nota", pause.Confirm!.Webhook);
        Assert.Contains("12345678", pause.Confirm.Body);
        Assert.Empty(transport.Calls);
        Assert.DoesNotContain(pause.Messages, m => m.Text == "Guardado");

        var done = await engine.ConfirmAsync(session, yes: true, default);
        Assert.Single(transport.Calls);
        Assert.Equal("Guardado", done.Messages.Last().Text);
        Assert.Null(done.Confirm);
    }

    [Fact]
    public async Task Declining_a_write_webhook_sends_nothing()
    {
        var (engine, session, transport) = NewEngine();
        await engine.StartAsync(session, default);
        await engine.UserEventAsync(session, "guardar", default);

        var cancelled = await engine.ConfirmAsync(session, yes: false, default);

        Assert.Empty(transport.Calls);
        Assert.Contains("no envié nada", cancelled.Messages[0].Text);
    }

    [Fact]
    public async Task Text_matching_an_intent_takes_that_route_and_unknown_text_hits_no_match()
    {
        var (engine, session, _) = NewEngine();
        await engine.StartAsync(session, default);

        var menu = await engine.UserTextAsync(session, "Volver al menú principal", default);
        Assert.Equal("Hola, soy Camila", menu.Messages[0].Text);

        var nonsense = await engine.UserTextAsync(session, "qwerty zxcv", default);
        Assert.Equal("No te entendí", nonsense.Messages[0].Text);
    }

    [Fact]
    public async Task Auto_routes_that_loop_are_stopped()
    {
        var (engine, session, _) = NewEngine(o => o.MaxAutoSteps = 10);
        var bot = engine.Bot;
        var a = new CamilaPage { Name = "A" };
        a.Routes.Add(new CamilaRoute { Condition = "true", TargetPage = "B" });
        var b = new CamilaPage { Name = "B" };
        b.Routes.Add(new CamilaRoute { Condition = "true", TargetPage = "A" });
        bot.Flows[0].Pages.AddRange(new[] { a, b });

        var turn = await engine.GoToAsync(session, "Inicio", "A", default);

        Assert.Contains(turn.Trace, t => t.Text.Contains("bucle"));
    }

    [Fact]
    public async Task Parameters_live_only_in_the_session_and_a_new_session_starts_empty()
    {
        var (engine, session, _) = NewEngine();
        await engine.StartAsync(session, default);
        await engine.UserEventAsync(session, "ver_nota", default);
        await engine.UserTextAsync(session, "1020304050", default);

        var other = new CamilaSession(Guid.NewGuid(), CamilaMode.Uml);
        await engine.StartAsync(other, default);

        Assert.False(other.Params.ContainsKey("numero_documento"));
        Assert.False(other.Params.ContainsKey("nota"));
    }

    #endregion

    #region Conversión del export y CamilaIA.md

    private static string WriteExport(string root)
    {
        void Json(string relative, string json)
        {
            var path = Path.Combine(root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, json);
        }

        Json("agent.json", "{\"displayName\":\"Bot\",\"startFlow\":\"Default Start Flow\"}");
        Json("flows/Default Start Flow/Default Start Flow.json",
            "{\"displayName\":\"Default Start Flow\",\"transitionRoutes\":[{\"intent\":\"Hola\",\"triggerFulfillment\":{},\"targetPage\":\"Menu\"}],\"eventHandlers\":[]}");
        Json("flows/Default Start Flow/pages/Menu.json",
            "{\"displayName\":\"Menu\",\"entryFulfillment\":{\"messages\":[{\"text\":{\"text\":[\"Bienvenido\"]}},{\"payload\":{\"richContent\":[[{\"type\":\"button\",\"text\":\"Ver\",\"event\":{\"event\":\"ver\"}}]]}}],\"webhook\":\"Mi Webhook\",\"tag\":\"t1\"},\"transitionRoutes\":[],\"eventHandlers\":[{\"event\":\"ver\",\"triggerFulfillment\":{},\"targetPage\":\"END_SESSION\"}]}");
        Json("intents/Hola/Hola.json", "{\"displayName\":\"Hola\"}");
        Json("intents/Hola/trainingPhrases/es.json", "{\"trainingPhrases\":[{\"parts\":[{\"text\":\"hola \"},{\"text\":\"camila\"}]}]}");
        Json("webhooks/Mi Webhook.json",
            "{\"displayName\":\"Mi Webhook\",\"genericWebService\":{\"uri\":\"https://api.example.com:9876/x\",\"requestHeaders\":{\"x-api-key\":\"CLAVE-SECRETA-123\",\"Content-Type\":\"application/json\",\"X-Token\":\"Bearer $session.params.token\"},\"username\":\"usuario\",\"password\":\"CLAVE-BASICA-456\",\"httpMethod\":\"POST\",\"requestBody\":\"{}\",\"parameterMapping\":[{\"key\":\"nota\",\"value\":\"$.n\"}]},\"timeout\":{\"seconds\":20}}");
        Json("webhooks/Guardar Cosa.json", "{\"displayName\":\"Guardar Cosa\",\"genericWebService\":{\"uri\":\"https://api.example.com:9876/g\",\"requestHeaders\":[{\"key\":\"x\",\"value\":\"y\"}]}}");
        return root;
    }

    [Fact]
    public void Conversion_keeps_the_structure_and_moves_credentials_out_of_the_markdown()
    {
        var dir = Path.Combine(Path.GetTempPath(), "camila-" + Guid.NewGuid().ToString("N"));
        try
        {
            var conversion = CamilaBotConverter.Convert(WriteExport(dir));
            var markdown = CamilaMarkdown.Write(conversion.Bot);

            // Estructura
            var page = conversion.Bot.FindFlow("Default Start Flow")!.Pages.Single();
            Assert.Equal("Bienvenido", page.Entry.Messages[0].Texts[0]);
            Assert.Equal("ver", page.Entry.Messages[1].Buttons[0].Event);
            Assert.Equal("Mi Webhook", page.Entry.Webhook);
            Assert.Equal("hola camila", conversion.Bot.FindIntent("Hola")!.Phrases.Single());

            // Credenciales: nunca en el Markdown, siempre como referencia
            Assert.DoesNotContain("CLAVE-SECRETA-123", markdown);
            Assert.DoesNotContain("CLAVE-BASICA-456", markdown);
            Assert.Equal(2, conversion.Secrets.Count);
            Assert.Contains("CLAVE-SECRETA-123", conversion.Secrets.Values);
            Assert.Contains("{{secret:CAMILA_WH_MI_WEBHOOK_X_API_KEY}}", markdown);
            Assert.Contains("Bearer $session.params.token", markdown);          // un parámetro de sesión no es un secreto

            // Tipos de webhook y hosts
            Assert.Equal("write", conversion.Bot.FindWebhook("Guardar Cosa")!.Kind);
            Assert.Equal("read", conversion.Bot.FindWebhook("Mi Webhook")!.Kind);
            Assert.Contains("api.example.com:9876", conversion.Hosts);
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Fact]
    public void Markdown_is_deterministic_and_reads_back_to_the_same_bot()
    {
        var dir = Path.Combine(Path.GetTempPath(), "camila-" + Guid.NewGuid().ToString("N"));
        try
        {
            WriteExport(dir);
            var first = CamilaMarkdown.Write(CamilaBotConverter.Convert(dir).Bot);
            var second = CamilaMarkdown.Write(CamilaBotConverter.Convert(dir).Bot);
            Assert.Equal(first, second);                                          // CA-001

            var bot = CamilaMarkdown.Read(first)!;
            Assert.Equal("Default Start Flow", bot.StartFlow);
            Assert.Equal("Menu", bot.FindFlow("Default Start Flow")!.Pages.Single().Name);
            Assert.Equal(2, bot.Webhooks.Count);
            Assert.Equal(first, CamilaMarkdown.Write(bot));                       // lo leído se vuelve a escribir igual
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Fact]
    public void Markdown_explains_in_order_where_webhooks_run_what_is_asked_and_where_each_route_goes()
    {
        var markdown = CamilaMarkdown.Write(TestBot());

        // Panorama general
        Assert.Contains("## Cómo leer este documento", markdown);
        Assert.Contains("## ✍️ Webhooks de escritura (piden confirmación)", markdown);
        Assert.Contains("**Guardar nota** — POST `api.example.com:9876`", markdown);
        Assert.Contains("`numero_documento` (@Numero_Documento) — Inicio › Pedir documento", markdown);

        // Página con webhook de escritura: orden de ejecución y confirmación
        Assert.Contains("✍️ Llama al webhook **Guardar nota** — **escritura, pide confirmación**", markdown);
        Assert.Contains("✍️ **escritura** — modifica datos reales", markdown);

        // Página que pide un dato, ruta automática y ruta por botón
        Assert.Contains("**⌨️ Pide al usuario**", markdown);
        Assert.Contains("pregunta: «¿Cuál es tu documento?»", markdown);
        Assert.Contains("🔀 *Automáticas", markdown);
        Assert.Contains("«Ver mi nota» (`ver_nota`) → página «Pedir documento»", markdown);

        // Webhook de lectura: qué envía y qué devuelve
        Assert.Contains("⚡ lectura", markdown);
        Assert.Contains("`cedula` ← `numero_documento`", markdown);
        Assert.Contains("`nota` ← `$.data.nota`", markdown);

        // La plataforma sigue pudiendo leerlo de vuelta
        Assert.Equal(markdown, CamilaMarkdown.Write(CamilaMarkdown.Read(markdown)!));
    }

    [Fact]
    public void Env_writer_adds_only_missing_secrets_and_never_overwrites()
    {
        var env = Path.Combine(Path.GetTempPath(), "env-" + Guid.NewGuid().ToString("N"));
        try
        {
            File.WriteAllText(env, "OTRA=1\nCAMILA_WH_A=ya-definido\n");
            var (added, kept) = CamilaGeneratorCommand.AppendMissing(env, new Dictionary<string, string> { ["CAMILA_WH_A"] = "nuevo", ["CAMILA_WH_B"] = "b" });

            Assert.Equal((1, 1), (added, kept));
            var text = File.ReadAllText(env);
            Assert.Contains("CAMILA_WH_A=ya-definido", text);
            Assert.DoesNotContain("nuevo", text);
            Assert.Contains("CAMILA_WH_B=b", text);
        }
        finally { File.Delete(env); }
    }

    #endregion

    #region Oficinas

    private static (CamilaWorkflow Workflow, FakeTransport Transport, FakeAnthropicService Llm) NewWorkflow(Microsoft.Extensions.Logging.ILogger<CamilaWorkflow>? logger = null)
    {
        var transport = new FakeTransport();
        var options = Options();
        var hooks = Hooks(transport, options, new() { ["CAMILA_WH_TEST"] = "x" });
        var llm = new FakeAnthropicService();
        var workflow = new CamilaWorkflow(new FakeSource { Bot = TestBot() }, new CamilaSessionStore(), hooks, new FakeGenerators(), llm, options, logger ?? NullLogger<CamilaWorkflow>.Instance);
        return (workflow, transport, llm);
    }

    /// <summary>El destino del chat: el bot «test» (UML) o el agente «camila-ia» (conversacional).</summary>
    private static CamilaTarget Office(string kind) => kind == UmlKind ? new CamilaTarget(CamilaMode.Uml, "test") : new CamilaTarget(CamilaMode.Conversational, "camila-ia");

    [Fact]
    public async Task Uml_console_shows_only_what_the_bot_shows_and_the_technical_detail_goes_to_the_log()
    {
        var store = new AI.Agent.Office.Api.Logging.InMemoryLogStore();
        using var factory = Microsoft.Extensions.Logging.LoggerFactory.Create(b => b.AddProvider(new AI.Agent.Office.Api.Logging.InMemoryLoggerProvider(store)));
        var (workflow, transport, llm) = NewWorkflow(factory.CreateLogger<CamilaWorkflow>());
        var id = Guid.NewGuid();
        var office = Office(UmlKind);

        var first = await workflow.HandleAsync(office, id, "hola", null, default);
        Assert.Equal("Hola, soy Camila", first.Reply.Trim());                         // solo el texto del bot
        Assert.True(first.Choice!.MenuOnly);
        Assert.Equal(new[] { "Ver mi nota", "Guardar" }, first.Choice.Options.Select(o => o.Title));

        var asked = await workflow.HandleAsync(office, id, "Opción 1", "1", default);
        Assert.Equal("¿Cuál es tu documento?", asked.Reply.Trim());

        var answered = await workflow.HandleAsync(office, id, "1020304050", null, default);
        Assert.Equal("Tu nota es 4.5", answered.Reply.Trim());
        Assert.Single(transport.Calls);
        Assert.Empty(llm.Requests);                                                   // el UML no usa el modelo (RF-009)

        // Nada técnico en el chat…
        foreach (var reply in new[] { first, asked, answered })
            foreach (var technical in new[] { "Ficha", "Qué hizo el bot", "📍", "Webhook", "ver_nota", "numero_documento", "Default", "ruta", "espera que escribas" })
                Assert.DoesNotContain(technical, reply.Reply);

        // …todo en el registro de la izquierda: página, dato esperado, ruta tomada, webhook y botones.
        var log = string.Join("\n", store.Read().Entries.Select(e => e.Source + " · " + e.Message));
        Assert.Contains("página · Inicio › Bienvenida", log);
        Assert.Contains("Espera el dato «numero_documento»", log);
        Assert.Contains("Ruta automática", log);
        Assert.Contains("el bot ofrece 2 botón(es)", log);
        Assert.DoesNotContain("1020304050", log);                                     // y sin datos personales
    }

    [Fact]
    public async Task The_ficha_command_still_shows_the_technical_card_when_asked_for_explicitly()
    {
        var (workflow, _, _) = NewWorkflow();
        var id = Guid.NewGuid();
        var office = Office(UmlKind);
        await workflow.HandleAsync(office, id, "hola", null, default);

        var ficha = await workflow.HandleAsync(office, id, "/ficha", null, default);

        Assert.Contains("Ficha", ficha.Reply);
        Assert.Contains("ver_nota", ficha.Reply);
    }

    [Fact]
    public async Task Uml_commands_show_the_hierarchy_variables_and_can_jump_to_a_page()
    {
        var (workflow, _, _) = NewWorkflow();
        var id = Guid.NewGuid();
        var office = Office(UmlKind);
        await workflow.HandleAsync(office, id, "hola", null, default);

        var tree = await workflow.HandleAsync(office, id, "/jerarquia", null, default);
        Assert.Contains("Inicio", tree.Reply);
        Assert.Contains("Pedir documento", tree.Reply);

        var jump = await workflow.HandleAsync(office, id, "/ir inicio › pedir", null, default);
        Assert.Contains("¿Cuál es tu documento?", jump.Reply);

        var vars = await workflow.HandleAsync(office, id, "/vars", null, default);
        Assert.Contains("idSessionWatson", vars.Reply);
    }

    [Fact]
    public async Task Uml_office_asks_for_confirmation_with_the_exact_payload_before_writing()
    {
        var (workflow, transport, _) = NewWorkflow();
        var id = Guid.NewGuid();
        var office = Office(UmlKind);
        await workflow.HandleAsync(office, id, "hola", null, default);

        var ask = await workflow.HandleAsync(office, id, "Guardar", "2", default);
        Assert.Contains("Confirmación necesaria", ask.Reply);
        Assert.Contains("12345678", ask.Reply);
        Assert.Equal(new[] { "yes", "no" }, ask.Choice!.Options.Select(o => o.Id));
        Assert.Empty(transport.Calls);

        var unclear = await workflow.HandleAsync(office, id, "mmm", null, default);       // sin sí o no, no se ejecuta nada
        Assert.Contains("sí** o **no", unclear.Reply);
        Assert.Empty(transport.Calls);

        var yes = await workflow.HandleAsync(office, id, "Sí, enviar", "yes", default);
        Assert.Contains("Guardado", yes.Reply);
        Assert.Single(transport.Calls);
    }

    #region Camila Conversacional: agente libre

    /// <summary>El modelo responde, en orden, cada decisión de la lista (y repite la última).</summary>
    private static void Script(FakeAnthropicService llm, params string[] decisions)
    {
        var i = 0;
        llm.Handler = (_, _) => Task.FromResult(FakeAnthropicService.Decision(decisions[Math.Min(i++, decisions.Length - 1)]));
    }

    private static string Reply(string text) => "{\"action\":\"responder\",\"reply\":\"" + text + "\"}";

    [Fact]
    public async Task Conversational_agent_chats_freely_without_following_any_flow()
    {
        var (workflow, transport, llm) = NewWorkflow();
        Script(llm, Reply("¡Hola! ¿En qué te ayudo?"));
        var office = Office(ConversationalKind);
        var id = Guid.NewGuid();

        var first = await workflow.HandleAsync(office, id, "hola, ¿cómo estás?", null, default);
        Script(llm, Reply("Claro, cuéntame."));
        var second = await workflow.HandleAsync(office, id, "primero una duda general", null, default);

        Assert.Equal("¡Hola! ¿En qué te ayudo?", first.Reply);
        Assert.Equal("Claro, cuéntame.", second.Reply);
        Assert.Null(first.Choice);                                              // sin botones ni ficha de página
        Assert.Empty(transport.Calls);                                          // no corrió ningún flujo ni webhook
        Assert.All(llm.Requests, r => Assert.True(r.Sensitive));                // datos de colaboradores: solo Claude
        Assert.Contains("Hola, soy Camila", llm.Requests[0].Messages[0].Content);  // la base va en el contexto como conocimiento
        Assert.Contains("«Consulta»", llm.Requests[0].Messages[0].Content);        // y el catálogo de servicios
        Assert.Contains("Persona: hola, ¿cómo estás?", llm.Requests[1].Messages[0].Content);   // recuerda la conversación
    }

    [Fact]
    public async Task Conversational_agent_searches_the_knowledge_base_on_demand()
    {
        var (workflow, transport, llm) = NewWorkflow();
        Script(llm, "{\"action\":\"buscar\",\"query\":\"mi nota documento\"}", Reply("Te explico lo que sé."));

        var reply = await workflow.HandleAsync(Office(ConversationalKind), Guid.NewGuid(), "¿cómo veo mi nota?", null, default);

        Assert.Equal("Te explico lo que sé.", reply.Reply);
        Assert.Equal(2, llm.Requests.Count);
        var second = llm.Requests[1].Messages[0].Content;
        Assert.Contains("[Resultado de buscar en la base]", second);
        Assert.Contains("¿Cuál es tu documento?", second);                          // lo que el bot decía, como información
        Assert.Empty(transport.Calls);
    }

    [Fact]
    public async Task Conversational_agent_calls_a_read_service_with_the_data_the_person_gave_and_uses_the_result()
    {
        var (workflow, transport, llm) = NewWorkflow();
        Script(llm,
            "{\"action\":\"webhook\",\"webhook\":\"Consulta\",\"data\":{\"numero_documento\":\"1020304050\"}}",
            Reply("Tu nota es 4.5"));

        var reply = await workflow.HandleAsync(Office(ConversationalKind), Guid.NewGuid(), "mi cédula es 1020304050, ¿cuál es mi nota?", null, default);

        Assert.Equal("Tu nota es 4.5", reply.Reply);
        var call = Assert.Single(transport.Calls);
        Assert.Equal("1020304050", JsonDocument.Parse(call.Body!).RootElement.GetProperty("cedula").GetString());
        Assert.Equal("x", call.Headers["x-api-key"]);                                // el secreto sale de la configuración, no del modelo
        Assert.Contains("nota = 4.5", llm.Requests[1].Messages[0].Content);          // el modelo ve el dato devuelto
        Assert.DoesNotContain("CAMILA_WH_TEST", llm.Requests[1].Messages[0].Content);   // el modelo nunca ve nombres ni valores de secretos
    }

    [Fact]
    public async Task Conversational_agent_does_not_call_a_service_when_data_is_missing_and_asks_for_it()
    {
        var (workflow, transport, llm) = NewWorkflow();
        Script(llm, "{\"action\":\"webhook\",\"webhook\":\"Consulta\"}", Reply("¿Me das tu número de documento?"));

        var reply = await workflow.HandleAsync(Office(ConversationalKind), Guid.NewGuid(), "quiero ver mi nota", null, default);

        Assert.Equal("¿Me das tu número de documento?", reply.Reply);
        Assert.Empty(transport.Calls);
        Assert.Contains("Faltan datos: numero_documento (lo da la persona)", llm.Requests[1].Messages[0].Content);
    }

    [Fact]
    public async Task Conversational_agent_rejects_unknown_services_and_unsafe_data_keys()
    {
        var (workflow, transport, llm) = NewWorkflow();
        Script(llm, "{\"action\":\"webhook\",\"webhook\":\"Borrar todo\",\"data\":{\"x\":\"1\"}}", Reply("No puedo hacer eso."));

        var reply = await workflow.HandleAsync(Office(ConversationalKind), Guid.NewGuid(), "borra todo", null, default);

        Assert.Equal("No puedo hacer eso.", reply.Reply);
        Assert.Empty(transport.Calls);
        Assert.Contains("No existe un servicio llamado «Borrar todo»", llm.Requests[1].Messages[0].Content);
    }

    [Fact]
    public async Task Conversational_agent_asks_for_confirmation_before_a_write_and_only_a_yes_sends_it()
    {
        var (workflow, transport, llm) = NewWorkflow();
        var id = Guid.NewGuid();
        var office = Office(ConversationalKind);
        Script(llm, "{\"action\":\"webhook\",\"webhook\":\"Guardar nota\",\"data\":{\"numero_documento\":\"12345678\"}}");

        var ask = await workflow.HandleAsync(office, id, "guarda mi nota", null, default);
        Assert.Contains("necesito tu confirmación", ask.Reply);
        Assert.Contains("12345678", ask.Reply);                                       // se muestra exactamente lo que se enviaría
        Assert.Empty(transport.Calls);

        var before = llm.Requests.Count;
        var unclear = await workflow.HandleAsync(office, id, "mmm, quizás", null, default);
        Assert.Contains("necesito tu confirmación", unclear.Reply);
        Assert.Equal(before, llm.Requests.Count);                                     // sin sí ni no, ni siquiera se consulta al modelo
        Assert.Empty(transport.Calls);

        Script(llm, Reply("Listo, quedó guardado."));
        var done = await workflow.HandleAsync(office, id, "sí, envíalo", null, default);
        Assert.Equal("Listo, quedó guardado.", done.Reply);
        Assert.Single(transport.Calls);
        Assert.Contains("[La persona confirmó el envío]", llm.Requests[^1].Messages[0].Content);
    }

    [Fact]
    public async Task Declining_a_write_in_the_conversational_agent_sends_nothing()
    {
        var (workflow, transport, llm) = NewWorkflow();
        var id = Guid.NewGuid();
        var office = Office(ConversationalKind);
        Script(llm, "{\"action\":\"webhook\",\"webhook\":\"Guardar nota\",\"data\":{\"numero_documento\":\"12345678\"}}");
        await workflow.HandleAsync(office, id, "guarda mi nota", null, default);

        Script(llm, Reply("De acuerdo, no envié nada."));
        var reply = await workflow.HandleAsync(office, id, "no, cancela", null, default);

        Assert.Equal("De acuerdo, no envié nada.", reply.Reply);
        Assert.Empty(transport.Calls);
        Assert.Contains("no se envió nada", llm.Requests[^1].Messages[0].Content);
    }

    [Fact]
    public async Task Conversational_agent_stops_after_a_bounded_number_of_steps()
    {
        var (workflow, _, llm) = NewWorkflow();
        Script(llm, "{\"action\":\"buscar\",\"query\":\"algo\"}");                  // el modelo nunca responde

        var reply = await workflow.HandleAsync(Office(ConversationalKind), Guid.NewGuid(), "hola", null, default);

        Assert.Equal(6, llm.Requests.Count);
        Assert.Contains("No logré resolver eso", reply.Reply);
    }

    #endregion

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Free_models_are_used_by_camila_only_when_explicitly_allowed(bool allowFree)
    {
        var options = Options(o => o.AllowFreeModels = allowFree);
        var llm = new FakeAnthropicService { Handler = (_, _) => Task.FromResult(FakeAnthropicService.Decision("{\"action\":\"responder\",\"reply\":\"hola\"}")) };
        var workflow = new CamilaWorkflow(new FakeSource { Bot = TestBot() }, new CamilaSessionStore(), Hooks(new FakeTransport(), options), new FakeGenerators(), llm, options, NullLogger<CamilaWorkflow>.Instance);

        await workflow.HandleAsync(Office(ConversationalKind), Guid.NewGuid(), "hola", null, default);

        Assert.All(llm.Requests, r => Assert.Equal(!allowFree, r.Sensitive));          // Sensitive = solo Claude
    }

    [Fact(Timeout = 15000)]
    public async Task Conversational_office_does_not_hang_when_the_model_is_slow()
    {
        var transport = new FakeTransport();
        var options = Options(o => o.ModelTimeoutSeconds = 1);
        var llm = new FakeAnthropicService { Handler = async (_, ct) => { await Task.Delay(Timeout.Infinite, ct); return FakeAnthropicService.Text("nunca"); } };
        var workflow = new CamilaWorkflow(new FakeSource { Bot = TestBot() }, new CamilaSessionStore(), Hooks(transport, options), new FakeGenerators(), llm, options, NullLogger<CamilaWorkflow>.Instance);

        var reply = await workflow.HandleAsync(Office(ConversationalKind), Guid.NewGuid(), "hola", null, default);

        Assert.True(reply.IsError);
        Assert.Contains("tardó demasiado", reply.Reply);
    }

    [Fact]
    public async Task Missing_knowledge_file_gives_clear_instructions()
    {
        var options = Options();
        var workflow = new CamilaWorkflow(new FakeSource(), new CamilaSessionStore(), Hooks(new FakeTransport(), options), new FakeGenerators(), new FakeAnthropicService(), options, NullLogger<CamilaWorkflow>.Instance);

        var reply = await workflow.HandleAsync(Office(UmlKind), Guid.NewGuid(), "hola", null, default);

        Assert.True(reply.IsError);
        Assert.Contains("--bot-generar", reply.Reply);
    }

    [Theory]
    [InlineData("bot:camila", CamilaMode.Uml, "camila")]
    [InlineData("agent:camila-ia", CamilaMode.Conversational, "camila-ia")]
    [InlineData(" BOT:otro-bot ", CamilaMode.Uml, "otro-bot")]
    public void Chat_targets_are_parsed_for_bots_and_agents(string text, CamilaMode mode, string id)
    {
        var target = CamilaTarget.Parse(text)!;
        Assert.Equal((mode, id), (target.Mode, target.Id));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("camila")]
    [InlineData("office:dev")]
    [InlineData("bot:")]
    [InlineData("bot:../etc")]
    [InlineData("agent:Mayusculas")]
    public void Invalid_chat_targets_are_rejected(string? text) => Assert.Null(CamilaTarget.Parse(text));

    #endregion
}

/// <summary>Comprobaciones sobre el playbook real de <c>playbooks bots/camila</c> (se omiten si todavía no se generó).</summary>
public class CamilaRealBaseTests
{
    private static CamilaBot? Load()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "AI.Agent.Office.sln"))) dir = dir.Parent;
        var botDir = dir is null ? null : Path.Combine(dir.FullName, "playbooks bots", "camila");
        return botDir is not null && Directory.Exists(botDir) ? CamilaPlaybooks.Load(botDir)?.Bot : null;
    }

    [Fact]
    public void Every_route_webhook_and_intent_of_the_generated_base_points_to_something_that_exists()
    {
        var bot = Load();
        if (bot is null) return;

        var problems = new List<string>();
        foreach (var flow in bot.Flows)
        {
            var routes = flow.Routes.Concat(flow.Events).Select(r => (Where: flow.Name, Route: r))
                .Concat(flow.Pages.SelectMany(p => p.Routes.Concat(p.Events).Select(r => (Where: flow.Name + " › " + p.Name, Route: r))))
                .Concat(flow.Pages.SelectMany(p => p.Form.Select(f => (Where: flow.Name + " › " + p.Name, Route: new CamilaRoute { Fulfillment = f.Prompt }))))
                .Concat(flow.Pages.Select(p => (Where: flow.Name + " › " + p.Name, Route: new CamilaRoute { Fulfillment = p.Entry })));

            foreach (var (where, route) in routes)
            {
                if (route.TargetFlow is not null && bot.FindFlow(route.TargetFlow) is null) problems.Add($"{where}: flujo inexistente «{route.TargetFlow}»");
                if (route.TargetPage is not null && !CamilaEngine.IsSpecialPage(route.TargetPage))
                {
                    var targetFlow = route.TargetFlow is not null ? bot.FindFlow(route.TargetFlow) : flow;
                    if (targetFlow is not null && bot.FindPage(targetFlow, route.TargetPage) is null) problems.Add($"{where}: página inexistente «{route.TargetPage}»");
                }
                if (route.Intent is not null && bot.FindIntent(route.Intent) is null) problems.Add($"{where}: intent inexistente «{route.Intent}»");
                if (route.Fulfillment.Webhook is not null && bot.FindWebhook(route.Fulfillment.Webhook) is null) problems.Add($"{where}: webhook inexistente «{route.Fulfillment.Webhook}»");
                foreach (var g in route.Fulfillment.Generators)
                    if (bot.FindGenerator(g.Generator) is null) problems.Add($"{where}: generador inexistente «{g.Generator}»");
            }
        }
        Assert.True(problems.Count == 0, string.Join("\n", problems.Distinct().Take(25)));
    }

    [Fact]
    public async Task The_real_bot_welcomes_like_a_person_and_offers_its_menu()
    {
        var bot = Load();
        if (bot is null) return;

        var options = Microsoft.Extensions.Options.Options.Create(new CamilaOptions { DefaultWebhookMode = CamilaWebhookMode.Simulated });
        var hooks = new CamilaWebhookService(options, new ConfigurationBuilder().Build(), new NoNetwork(), NullLogger<CamilaWebhookService>.Instance);
        var engine = new CamilaEngine(bot, hooks, new NoGenerators(), options);
        var session = new CamilaSession(Guid.NewGuid(), CamilaMode.Uml);

        var turn = await engine.StartAsync(session, default);

        Assert.NotEmpty(turn.Messages);
        Assert.DoesNotContain(turn.Trace, t => t.Kind == "aviso");
        Assert.False(string.IsNullOrEmpty(session.Flow));
    }

    [Fact]
    public async Task Every_page_and_button_of_the_real_bot_runs_without_errors_or_jumps_to_missing_pages()
    {
        var bot = Load();
        if (bot is null) return;

        var options = Microsoft.Extensions.Options.Options.Create(new CamilaOptions { DefaultWebhookMode = CamilaWebhookMode.Simulated });
        var hooks = new CamilaWebhookService(options, new ConfigurationBuilder().Build(), new NoNetwork(), NullLogger<CamilaWebhookService>.Instance);
        var engine = new CamilaEngine(bot, hooks, new NoGenerators(), options);

        var pages = 0;
        var buttons = 0;
        var problems = new List<string>();
        foreach (var flow in bot.Flows)
            foreach (var page in flow.Pages)
            {
                var session = new CamilaSession(Guid.NewGuid(), CamilaMode.Uml);
                var turn = await engine.GoToAsync(session, flow.Name, page.Name, default);
                pages++;
                problems.AddRange(turn.Trace.Where(t => t.Kind == "aviso" && t.Text.Contains("no existe")).Select(t => $"{flow.Name} › {page.Name}: {t.Text}"));

                foreach (var option in turn.Options.Where(o => o.Event is not null))
                {
                    buttons++;
                    var again = new CamilaSession(Guid.NewGuid(), CamilaMode.Uml);
                    await engine.GoToAsync(again, flow.Name, page.Name, default);
                    var next = await engine.UserEventAsync(again, option.Event!, default);
                    // Un botón sin manejador es un hueco del bot original (p. ej. «mejor_oferta» en la encuesta de retiro), no un error del motor;
                    // aquí solo importa que pulsarlo no rompa nada ni salte a algo que no existe.
                    problems.AddRange(next.Trace.Where(t => t.Kind == "aviso" && t.Text.Contains("no existe")).Select(t => $"{flow.Name} › {page.Name} › botón «{option.Label}»: {t.Text}"));
                }
            }

        Assert.True(pages > 150, $"Páginas recorridas: {pages}");
        Assert.True(buttons > 100, $"Botones pulsados: {buttons}");
        Assert.True(problems.Count == 0, $"{pages} páginas y {buttons} botones.\n" + string.Join("\n", problems.Distinct().Take(25)));
    }

    private sealed class NoNetwork : ICamilaHttpTransport
    {
        public Task<CamilaHttpResponse> SendAsync(CamilaHttpRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException("Sin red en las pruebas.");
    }

    private sealed class NoGenerators : ICamilaGeneratorRunner
    {
        public Task<string> RunAsync(CamilaGeneratorDef generator, IReadOnlyDictionary<string, string> inputs, CancellationToken cancellationToken) => throw new InvalidOperationException("Sin modelo en las pruebas.");
    }
}

public class CamilaOnboardingTests
{
    [Fact(Timeout = 20000)]
    public async Task The_real_bot_onboarding_walks_welcome_name_role_and_asks_for_the_document()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "AI.Agent.Office.sln"))) dir = dir.Parent;
        var botDir = Path.Combine(dir!.FullName, "playbooks bots", "camila");
        if (!Directory.Exists(botDir)) return;

        var bot = CamilaPlaybooks.Load(botDir)!.Bot;
        await Task.CompletedTask;
        var options = Microsoft.Extensions.Options.Options.Create(new CamilaOptions { DefaultWebhookMode = CamilaWebhookMode.Simulated });
        var hooks = new CamilaWebhookService(options, new ConfigurationBuilder().Build(), new Offline(), NullLogger<CamilaWebhookService>.Instance);
        var engine = new CamilaEngine(bot, hooks, new Offline(), options);
        var session = new CamilaSession(Guid.NewGuid(), CamilaMode.Conversational);

        await engine.StartAsync(session, default);
        Assert.Equal("nombreProvisional", session.WaitingParam);

        var role = await engine.ProvideValueAsync(session, "Ana", default);
        Assert.Contains(role.Options, o => o.Event == "colaborador");

        var document = await engine.UserEventAsync(session, "colaborador", default);
        Assert.Equal("Numero_Documento", session.WaitingParam);
        Assert.NotEmpty(document.Messages);
    }

    private sealed class Offline : ICamilaHttpTransport, ICamilaGeneratorRunner
    {
        public Task<CamilaHttpResponse> SendAsync(CamilaHttpRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException();
        public Task<string> RunAsync(CamilaGeneratorDef generator, IReadOnlyDictionary<string, string> inputs, CancellationToken cancellationToken) => throw new InvalidOperationException();
    }
}
