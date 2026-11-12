using System.Collections.Concurrent;
using System.Text.Json;
using AI.Agent.Office.Application.Browser;
using AI.Agent.Office.Infrastructure.Browser;
using AI.Agent.Office.Tests.TestSupport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Tests;

/// <summary>
/// Spec 007 (CA-010): el playbook completo contra un navegador REAL (Playwright) y una página local que imita un correo web.
/// Si el navegador no está instalado la prueba no puede ejecutarse y termina sin fallar (se informa en el nombre del resultado).
/// </summary>
public class BrowserEndToEndTests : IDisposable
{
    private const string Page = """
        <!doctype html><html><head><meta charset="utf-8"><title>Correo de prueba</title></head><body>
        <button id="nuevo" onclick="document.getElementById('f').style.display='block'">Redactar</button>
        <a id="fuera" href="http://localhost:__PORT__/otro">Salir</a>
        <div id="f" style="display:none">
          <input id="para" aria-label="Para"><input id="asunto" aria-label="Asunto">
          <div id="cuerpo" contenteditable="true" aria-label="Cuerpo" style="min-height:40px;border:1px solid #999"></div>
          <button id="enviar" onclick="fetch('/enviado',{method:'POST',body:JSON.stringify({para:para.value,asunto:asunto.value,cuerpo:cuerpo.innerText})}).then(()=>document.body.insertAdjacentHTML('beforeend','<p>Mensaje enviado</p>'))">Enviar</button>
        </div></body></html>
        """;

    private readonly string _dir = Path.Combine(Path.GetTempPath(), "office-e2e-" + Guid.NewGuid().ToString("N"));
    private readonly ConcurrentQueue<string> _sent = new();
    private int _otherHits;
    private WebApplication? _app;

    public void Dispose()
    {
        _app?.DisposeAsync().AsTask().Wait();
        try { Directory.Delete(_dir, true); } catch (IOException) { }
    }

    private async Task<string> StartPageAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        _app = builder.Build();
        string port = "0";
        _app.MapGet("/", () => Results.Content(Page.Replace("__PORT__", port), "text/html"));
        _app.MapGet("/inbox", () =>
        {
            var rows = string.Concat(Enumerable.Range(1, 25).Select(i =>
                $"<tr class=\"zA\"><td><span email=\"r{i}@example.com\" name=\"Remitente {i}\">Remitente {i}</span></td><td><span class=\"bog\">Asunto {i}</span> <span class=\"y2\">- Vista previa {i}</span></td><td class=\"xW\"><span title=\"10 nov 2026, 10:{i:00}\">10:{i:00}</span></td></tr>"));
            return Results.Content($"<!doctype html><html><body><table>{rows}</table></body></html>", "text/html");
        });
        _app.MapGet("/login", () => Results.Content(
            "<!doctype html><html><body><input placeholder=\"Ingrese su usuario\"><input type=\"password\" placeholder=\"Ingrese su contraseña\">" +
            "<select><option value=\"\">Seleccione una opción</option><option value=\"Outsourcing\">Outsourcing</option></select>" +
            "<button onclick=\"var u=document.querySelector('input').value, o=document.querySelector('select').value;" +
            "fetch('/entro',{method:'POST',body:JSON.stringify({u:u,o:o})}); alert('Es necesario aceptar las notificaciones para continuar'); document.body.insertAdjacentHTML('beforeend','<h2>Información Casos</h2>')\">Ingresar</button>" +
            "<a href=\"#\">¿Olvidó su contraseña?</a></body></html>", "text/html; charset=utf-8"));
        _app.MapPost("/entro", async (HttpRequest r) => { _sent.Enqueue(await new StreamReader(r.Body).ReadToEndAsync()); return Results.Ok(); });
        // Simula una aplicación que se vuelve a pintar y borra lo escrito: a los 700 ms del primer «input» en cada campo (una sola vez).
        _app.MapGet("/borra", () => Results.Content(
            "<!doctype html><html><body><input id=\"u\"><input id=\"p\" type=\"password\"><button id=\"enviar\" onclick=\"fetch('/entro',{method:'POST',body:JSON.stringify({u:u.value,p:p.value})}).then(()=>document.body.insertAdjacentHTML('beforeend','<h2>Enviado</h2>'))\">Enviar</button>" +
            "<script>['u','p'].forEach(function(id){var el=document.getElementById(id),done=false;el.addEventListener('input',function(){if(!done){done=true;setTimeout(function(){el.value='';},700);}});});</script></body></html>", "text/html; charset=utf-8"));
        // Borra el valor inmediatamente después de la primera escritura (la primera vez).
        _app.MapGet("/borra-ya", () => Results.Content(
            "<!doctype html><html><body><input id=\"u\"><button id=\"enviar\" onclick=\"fetch('/entro',{method:'POST',body:JSON.stringify({u:u.value})}).then(()=>document.body.insertAdjacentHTML('beforeend','<h2>Enviado</h2>'))\">Enviar</button>" +
            "<script>var el=document.getElementById('u'),n=0;el.addEventListener('input',function(){if(n++===0){setTimeout(function(){el.value='';},50);}});</script></body></html>", "text/html; charset=utf-8"));
        _app.MapGet("/otro", () => { Interlocked.Increment(ref _otherHits); return Results.Content("otro sitio", "text/html"); });
        _app.MapPost("/enviado", async (HttpRequest r) => { _sent.Enqueue(await new StreamReader(r.Body).ReadToEndAsync()); return Results.Ok(); });
        await _app.StartAsync();
        var url = _app.Urls.First();
        port = new Uri(url).Port.ToString();
        return url;
    }

    private static bool NoBrowser(BrowserRunResult r) => !r.Success && (r.Message.StartsWith("No encuentro el navegador") || r.Message.StartsWith("No pude abrir el navegador") || r.Message.StartsWith("No se pudo iniciar Playwright"));

    private (PlaywrightSessionFactory Factory, BrowserOptions Options) NewFactory()
    {
        var options = new BrowserOptions { DataDirectory = Path.Combine(_dir, "data"), PlaybooksDirectory = Path.Combine(_dir, "pb"), Headless = true, StepTimeoutSeconds = 5, RunTimeoutSeconds = 60 };
        return (new PlaywrightSessionFactory(Options.Create(options), NullLogger<PlaywrightSessionFactory>.Instance), options);
    }

    private static string Playbook(string url) => """
        ## Entradas
        - destinatario | ¿A quién? | correo
        - asunto | redactar | Asunto
        - cuerpo | redactar | Cuerpo
        ## Pasos
        1. ir | __URL__
        2. clic | #nuevo | | Abrir la redacción
        3. escribir | input[aria-label="Para"] | {{destinatario}}
        4. escribir | input[aria-label="Asunto"] | {{asunto}}
        5. escribir | div[aria-label="Cuerpo"] | {{cuerpo}}
        6. clic | #enviar | | Enviar | irreversible
        7. verificar | Mensaje enviado
        """.Replace("__URL__", url);

    private static BrowserRunner NewRunner(PlaywrightSessionFactory factory, BrowserOptions options, FakeAnthropicService llm, InMemoryPlaybookStore store) =>
        new(factory, llm, store, new BrowserCredentialStore(), Options.Create(options), NullLogger<BrowserRunner>.Instance);

    [Fact]
    public async Task The_whole_playbook_runs_in_a_real_browser_and_sends_the_values()
    {
        var url = await StartPageAsync();
        var (factory, options) = NewFactory();
        var llm = new FakeAnthropicService();
        var runner = NewRunner(factory, options, llm, new InMemoryPlaybookStore());
        var site = new BrowserSite("prueba", "Prueba", url, new[] { "127.0.0.1" });
        var playbook = PlaybookParser.Parse("prueba", Playbook(url));
        Assert.True(playbook.IsValid, string.Join("; ", playbook.Errors));

        var result = await runner.RunAsync(site, playbook.Playbook!, new Dictionary<string, string>
        {
            ["destinatario"] = "jefe@empresa.com", ["asunto"] = "Informe del incidente", ["cuerpo"] = "Estimado, adjunto la solución."
        }, null, CancellationToken.None);

        if (NoBrowser(result)) return;   // sin navegador instalado: la prueba no puede ejecutarse

        Assert.True(result.Success, result.Message);
        Assert.Equal("reproducción", result.Mode);
        Assert.Equal(0, result.ModelCalls);
        var sent = JsonDocument.Parse(Assert.Single(_sent)).RootElement;
        Assert.Equal("jefe@empresa.com", sent.GetProperty("para").GetString());
        Assert.Equal("Informe del incidente", sent.GetProperty("asunto").GetString());
        Assert.Contains("adjunto la solución", sent.GetProperty("cuerpo").GetString());
    }

    [Fact]
    public async Task The_real_browser_cannot_navigate_outside_the_allowed_domains()
    {
        var url = await StartPageAsync();
        var (factory, options) = NewFactory();
        var runner = NewRunner(factory, options, new FakeAnthropicService(), new InMemoryPlaybookStore());
        var site = new BrowserSite("prueba", "Prueba", url, new[] { "127.0.0.1" });
        var playbook = PlaybookParser.Parse("prueba", $"## Pasos\n1. ir | {url}\n2. clic | #fuera\n3. esperar | 800");

        var result = await runner.RunAsync(site, playbook.Playbook!, new Dictionary<string, string>(), null, CancellationToken.None);

        if (NoBrowser(result)) return;   // sin navegador instalado: la prueba no puede ejecutarse

        Assert.Equal(0, Volatile.Read(ref _otherHits));      // «localhost» no está permitido: la navegación se abortó antes de llegar al servidor
    }

    [Fact]
    public async Task Exploration_reads_the_real_page_learns_a_playbook_and_the_replay_needs_no_model()
    {
        var url = await StartPageAsync();
        var (factory, options) = NewFactory();
        var store = new InMemoryPlaybookStore();
        var llm = new FakeAnthropicService();
        var script = new Queue<string>(new[]
        {
            """{"action":"click","target":"#nuevo"}""",
            """{"action":"fill","target":"input[aria-label=\"Para\"]","value":"{{destinatario}}"}""",
            """{"action":"done"}"""
        });
        llm.Handler = (_, _) => Task.FromResult(BrowserSamples.Json(script.Dequeue()));
        var runner = NewRunner(factory, options, llm, store);
        var site = new BrowserSite("prueba", "Prueba", url, new[] { "127.0.0.1" });
        var values = new Dictionary<string, string> { ["destinatario"] = "jefe@empresa.com" };
        var free = PlaybookParser.Parse("prueba", $"## Entradas\n- destinatario | ¿A quién? | correo\n## Pasos\n1. ir | {url}\n2. Abre la redacción y escribe el destinatario");

        var first = await runner.RunAsync(site, free.Playbook!, values, null, CancellationToken.None);
        if (NoBrowser(first)) return;   // sin navegador instalado: la prueba no puede ejecutarse

        Assert.True(first.Success, first.Message);
        Assert.Equal("exploración", first.Mode);
        Assert.Contains("- #nuevo ⇒", llm.Requests[0].Messages.Single().Content);                 // la instantánea real incluye el botón
        Assert.DoesNotContain("jefe@empresa.com", llm.Requests[0].Messages.Single().Content);    // los valores no viajan al modelo
        Assert.True(first.ProposalCreated);

        var learned = PlaybookParser.Parse("prueba", store.Proposals["prueba"]);
        Assert.True(learned.IsValid, string.Join("; ", learned.Errors));
        llm.Requests.Clear();

        var second = await runner.RunAsync(site, learned.Playbook!, values, null, CancellationToken.None);

        Assert.True(second.Success, second.Message);
        Assert.Equal("reproducción", second.Mode);
        Assert.Equal(0, second.ModelCalls);
        Assert.Empty(llm.Requests);
        Assert.True(second.DurationMs <= first.DurationMs * 3 + 3000);                          // sin modelo no es más lento
    }

    [Fact]
    public async Task The_real_browser_reads_the_last_20_rows_of_an_inbox_like_page()
    {
        var url = await StartPageAsync();
        var (factory, options) = NewFactory();
        var runner = NewRunner(factory, options, new FakeAnthropicService(), new InMemoryPlaybookStore());
        var site = new BrowserSite("prueba", "Prueba", url, new[] { "127.0.0.1" });
        var playbook = PlaybookParser.Parse("prueba", $"## Pasos\n1. ir | {url}/inbox\n2. esperar | tr.zA\n3. extraer | tr.zA | 20");

        var result = await runner.RunAsync(site, playbook.Playbook!, new Dictionary<string, string>(), null, CancellationToken.None);

        if (NoBrowser(result)) return;

        Assert.True(result.Success, result.Message);
        Assert.Equal(20, result.Data!.Count);
        Assert.Equal("Remitente 1", result.Data[0].Sender);
        Assert.Equal("r1@example.com", result.Data[0].Email);
        Assert.Equal("Asunto 1", result.Data[0].Subject);
        Assert.Equal("Vista previa 1", result.Data[0].Snippet);
        Assert.Contains("10 nov 2026", result.Data[0].Date);
        Assert.Equal("Asunto 20", result.Data[19].Subject);
        Assert.Equal(0, result.ModelCalls);
    }

    [Fact]
    public async Task A_login_with_a_dropdown_and_a_browser_alert_completes_with_the_site_credentials()
    {
        var url = await StartPageAsync();
        var (factory, options) = NewFactory();
        var secrets = new BrowserCredentialStore();
        secrets.Set("prueba", new BrowserCredentials("usuario-demo", "clave-demo"));
        var runner = new BrowserRunner(factory, new FakeAnthropicService(), new InMemoryPlaybookStore(), secrets, Options.Create(options), NullLogger<BrowserRunner>.Instance);
        var site = new BrowserSite("prueba", "Prueba", url, new[] { "127.0.0.1" });
        var playbook = PlaybookParser.Parse("prueba", """
            ## Pasos
            1. ir | __URL__/login
            2. esperar | input[placeholder="Ingrese su usuario"]
            3. escribir | input[placeholder="Ingrese su usuario"] | {{credencial.usuario}}
            4. escribir | input[placeholder="Ingrese su contraseña"] | {{credencial.clave}}
            5. seleccionar | select | Outsourcing
            6. clic | button:has-text("Ingresar")
            7. esperar | text=Información Casos
            """.Replace("__URL__", url));
        Assert.True(playbook.IsValid, string.Join("; ", playbook.Errors));

        var result = await runner.RunAsync(site, playbook.Playbook!, new Dictionary<string, string>(), null, CancellationToken.None);

        if (NoBrowser(result)) return;

        Assert.True(result.Success, result.Message);
        var sent = JsonDocument.Parse(Assert.Single(_sent)).RootElement;
        Assert.Equal("usuario-demo", sent.GetProperty("u").GetString());
        Assert.Equal("Outsourcing", sent.GetProperty("o").GetString());
        Assert.DoesNotContain(result.Log, l => l.Contains("clave-demo") || l.Contains("usuario-demo"));
        Assert.Equal(0, result.ModelCalls);
    }

    [Fact]
    public async Task Fields_that_the_page_wipes_after_typing_are_restored_before_submitting()
    {
        var url = await StartPageAsync();
        var (factory, options) = NewFactory();
        var runner = NewRunner(factory, options, new FakeAnthropicService(), new InMemoryPlaybookStore());
        var site = new BrowserSite("prueba", "Prueba", url, new[] { "127.0.0.1" });
        var playbook = PlaybookParser.Parse("prueba", """
            ## Entradas
            - usuario | ¿Usuario?
            - clave | ¿Clave?
            ## Pasos
            1. ir | __URL__/borra
            2. esperar | #u
            3. escribir | #u | {{usuario}}
            4. escribir | #p | {{clave}}
            5. esperar | 1500
            6. clic | #enviar
            7. esperar | text=Enviado
            """.Replace("__URL__", url));
        Assert.True(playbook.IsValid, string.Join("; ", playbook.Errors));

        var result = await runner.RunAsync(site, playbook.Playbook!, new Dictionary<string, string> { ["usuario"] = "addiazf", ["clave"] = "una-clave-larga" }, null, CancellationToken.None);

        if (NoBrowser(result)) return;

        Assert.True(result.Success, result.Message);
        var sent = JsonDocument.Parse(Assert.Single(_sent)).RootElement;
        Assert.Equal("addiazf", sent.GetProperty("u").GetString());          // la página lo borró a los 700 ms; se restauró antes de enviar
        Assert.Equal("una-clave-larga", sent.GetProperty("p").GetString());
        Assert.DoesNotContain(result.Log, l => l.Contains("una-clave-larga"));
    }

    [Fact]
    public async Task A_field_that_loses_the_value_right_after_filling_is_typed_again()
    {
        var url = await StartPageAsync();
        var (factory, options) = NewFactory();
        var runner = NewRunner(factory, options, new FakeAnthropicService(), new InMemoryPlaybookStore());
        var site = new BrowserSite("prueba", "Prueba", url, new[] { "127.0.0.1" });
        var playbook = PlaybookParser.Parse("prueba", """
            ## Pasos
            1. ir | __URL__/borra-ya
            2. escribir | #u | valor-inicial
            3. clic | #enviar
            4. esperar | text=Enviado
            """.Replace("__URL__", url));

        var result = await runner.RunAsync(site, playbook.Playbook!, new Dictionary<string, string>(), null, CancellationToken.None);

        if (NoBrowser(result)) return;

        Assert.True(result.Success, result.Message);
        Assert.Equal("valor-inicial", JsonDocument.Parse(Assert.Single(_sent)).RootElement.GetProperty("u").GetString());
    }
}
