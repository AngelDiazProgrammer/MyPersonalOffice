using System.Net;
using System.Text.RegularExpressions;
using ATLAS.Api.Auth;
using ATLAS.Api.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ATLAS.Tests;

/// <summary>Spec 010 · acceso, contraseñas, bloqueo por intentos y registro en vivo.</summary>
public class PlatformTests
{
    #region Contraseñas y usuarios

    [Fact]
    public void Password_hash_verifies_only_the_right_password_and_never_contains_it()
    {
        var hash = PasswordHasher.Hash("una-clave-de-prueba", 1000);

        Assert.StartsWith("pbkdf2-sha256$1000$", hash);
        Assert.DoesNotContain("una-clave-de-prueba", hash);
        Assert.True(PasswordHasher.Verify("una-clave-de-prueba", hash));
        Assert.False(PasswordHasher.Verify("otra", hash));
        Assert.NotEqual(hash, PasswordHasher.Hash("una-clave-de-prueba", 1000));          // la sal es distinta cada vez
        Assert.False(PasswordHasher.Verify("x", "texto-sin-formato"));
        Assert.False(PasswordHasher.Verify("x", "pbkdf2-sha256$1000$no-es-base64$tampoco"));
    }

    private static string UsersFile(string user, string password)
    {
        var path = Path.Combine(Path.GetTempPath(), "usuarios-" + Guid.NewGuid().ToString("N") + ".md");
        File.WriteAllText(path, $"# Usuarios\n\n| Usuario | Rol | Contraseña |\n|---|---|---|\n| {user} | admin | {PasswordHasher.Hash(password, 1000)} |\n| plano | admin | contraseña-en-claro |\n");
        return path;
    }

    [Fact]
    public void User_store_reads_the_markdown_table_and_ignores_rows_without_a_hash()
    {
        var path = UsersFile("ana", "clave-de-ana");
        try
        {
            var store = new MarkdownUserStore(path);

            Assert.Equal("admin", store.Validate("ANA", "clave-de-ana")!.Role);              // el usuario no distingue mayúsculas
            Assert.Null(store.Validate("ana", "mala"));
            Assert.Null(store.Validate("nadie", "clave-de-ana"));
            Assert.Null(store.Validate("plano", "contraseña-en-claro"));                    // una contraseña en claro nunca sirve
            Assert.Single(store.Load());
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void The_users_file_of_the_repository_keeps_only_a_hash()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ATLAS.sln"))) dir = dir.Parent;
        var file = Path.Combine(dir!.FullName, "configuration", "usuarios.md");
        Assert.True(File.Exists(file), "Falta configuration/usuarios.md");

        var store = new MarkdownUserStore(file);
        var admin = Assert.Single(store.Load(), u => u.Name == "addiazf");

        Assert.Equal("admin", admin.Role);
        Assert.StartsWith(PasswordHasher.Prefix + "$", admin.PasswordHash);
        Assert.Null(store.Validate("addiazf", "no-es-la-clave"));
    }

    [Fact]
    public void Login_is_locked_after_five_failures_and_a_success_resets_the_count()
    {
        var now = DateTimeOffset.UtcNow;
        var throttle = new LoginThrottle(5, TimeSpan.FromMinutes(5), () => now);

        for (var i = 0; i < 4; i++) throttle.Fail("ana", "1.1.1.1");
        Assert.False(throttle.IsLocked("ana", "1.1.1.1"));
        throttle.Succeed("ana", "1.1.1.1");
        for (var i = 0; i < 4; i++) throttle.Fail("ana", "1.1.1.1");
        Assert.False(throttle.IsLocked("ana", "1.1.1.1"));

        throttle.Fail("ana", "1.1.1.1");
        Assert.True(throttle.IsLocked("ana", "1.1.1.1"));
        Assert.False(throttle.IsLocked("ana", "2.2.2.2"));                                  // otro origen no queda bloqueado
        Assert.False(throttle.IsLocked("otro", "1.1.1.1"));

        now = now.AddMinutes(6);
        Assert.False(throttle.IsLocked("ana", "1.1.1.1"));
    }

    #endregion

    #region Acceso por HTTP

    private sealed class Host : IAsyncDisposable
    {
        private readonly WebApplication _app;
        public HttpClient Client { get; }
        public string UsersPath { get; }
        public string Password { get; }
        public string Token { get; }

        public Host(WebApplication app, HttpClient client, string usersPath, string password, string token)
        {
            _app = app;
            Client = client;
            UsersPath = usersPath;
            Password = password;
            Token = token;
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _app.StopAsync();
            await _app.DisposeAsync();
            File.Delete(UsersPath);
        }
    }

    private static async Task<Host> StartAsync()
    {
        var password = "clave-" + Guid.NewGuid().ToString("N")[..8];
        var users = UsersFile("admin-prueba", password);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddOfficeAuth(builder.Configuration);
        builder.Services.AddSingleton<IUserStore>(new MarkdownUserStore(users));         // el último registro gana: se usa el archivo de la prueba

        var app = builder.Build();
        app.UseOfficeAuth();
        app.UseAntiforgery();
        app.MapOfficeAuth();
        app.MapGet("/api/secreto", () => "datos").WithName("secreto");
        app.MapGet("/pagina", () => "pagina");
        app.MapGet("/health", () => "ok").AllowAnonymous();
        await app.StartAsync();

        var address = app.Urls.First();
        var handler = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = true, CookieContainer = new CookieContainer() };
        var token = app.Services.GetRequiredService<InternalApiToken>().Value;
        return new Host(app, new HttpClient(handler) { BaseAddress = new Uri(address) }, users, password, token);
    }

    private static async Task<HttpResponseMessage> LoginAsync(HttpClient client, string user, string password)
    {
        var page = await client.GetStringAsync("/login");
        var token = Regex.Match(page, "name=\"__RequestVerificationToken\" value=\"([^\"]+)\"").Groups[1].Value;
        return await client.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token, ["usuario"] = user, ["clave"] = password, ["returnUrl"] = string.Empty
        }));
    }

    [Fact]
    public async Task Without_a_session_the_api_answers_401_pages_redirect_to_login_and_health_stays_public()
    {
        await using var host = await StartAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/api/secreto")).StatusCode);

        var page = await host.Client.GetAsync("/pagina");
        Assert.Equal(HttpStatusCode.Redirect, page.StatusCode);
        Assert.Contains("/login", page.Headers.Location!.ToString());

        Assert.Equal(HttpStatusCode.OK, (await host.Client.GetAsync("/health")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await host.Client.GetAsync("/login")).StatusCode);
    }

    [Fact]
    public async Task The_internal_token_opens_the_api_but_a_wrong_token_does_not()
    {
        await using var host = await StartAsync();

        using var good = new HttpRequestMessage(HttpMethod.Get, "/api/secreto");
        good.Headers.Add(InternalApiToken.HeaderName, host.Token);
        Assert.Equal(HttpStatusCode.OK, (await host.Client.SendAsync(good)).StatusCode);

        using var bad = new HttpRequestMessage(HttpMethod.Get, "/api/secreto");
        bad.Headers.Add(InternalApiToken.HeaderName, host.Token + "x");
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.SendAsync(bad)).StatusCode);
    }

    [Fact]
    public async Task Logging_in_gives_a_session_cookie_and_logging_out_removes_access()
    {
        await using var host = await StartAsync();

        var wrong = await LoginAsync(host.Client, "admin-prueba", "mala");
        Assert.Contains("error=1", wrong.Headers.Location!.ToString());                     // mensaje genérico, sin decir qué falló
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/api/secreto")).StatusCode);

        var ok = await LoginAsync(host.Client, "admin-prueba", host.Password);
        Assert.Equal(HttpStatusCode.Redirect, ok.StatusCode);
        var cookie = string.Join(";", ok.Headers.GetValues("Set-Cookie"));
        Assert.Contains("office.auth=", cookie);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(HttpStatusCode.OK, (await host.Client.GetAsync("/api/secreto")).StatusCode);

        await host.Client.GetAsync("/auth/logout");
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/api/secreto")).StatusCode);
    }

    [Fact]
    public async Task Five_wrong_passwords_lock_the_account_even_if_the_next_one_is_right()
    {
        await using var host = await StartAsync();

        for (var i = 0; i < 5; i++) await LoginAsync(host.Client, "admin-prueba", "mala-" + i);

        var blocked = await LoginAsync(host.Client, "admin-prueba", host.Password);
        Assert.Contains("bloqueado=1", blocked.Headers.Location!.ToString());
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/api/secreto")).StatusCode);
    }

    [Fact]
    public async Task The_login_form_without_the_antiforgery_token_is_rejected()
    {
        await using var host = await StartAsync();

        var response = await host.Client.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["usuario"] = "admin-prueba", ["clave"] = host.Password
        }));

        Assert.NotEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/api/secreto")).StatusCode);
    }

    #endregion

    #region Registro en vivo

    [Fact]
    public void Log_store_keeps_a_bounded_ring_and_returns_only_what_is_new()
    {
        var store = new InMemoryLogStore(50);
        for (var i = 1; i <= 80; i++) store.Add(DateTimeOffset.Now, "Info", "Camila", "mensaje " + i, null);

        var all = store.Read();
        Assert.Equal(50, all.Entries.Count);
        Assert.Equal("mensaje 31", all.Entries[0].Message);
        Assert.Equal(80, all.Last);

        var fresh = store.Read(78);
        Assert.Equal(new[] { "mensaje 79", "mensaje 80" }, fresh.Entries.Select(e => e.Message));

        store.Clear();
        Assert.Empty(store.Read().Entries);
        store.Add(DateTimeOffset.Now, "Info", "Camila", "nuevo", null);
        Assert.Equal(81, store.Read().Last);                                                 // la secuencia no se reinicia
    }

    [Fact]
    public void Logger_provider_captures_platform_logs_with_source_level_and_conversation_but_nothing_else()
    {
        var store = new InMemoryLogStore();
        using var factory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Debug).AddProvider(new InMemoryLoggerProvider(store)));

        var camila = factory.CreateLogger("ATLAS.Application.Camila.CamilaWorkflow");
        using (camila.BeginScope(new Dictionary<string, object> { ["ConversationId"] = "0a34c9a0-828a-4922-82f4-085f12051a26" }))
            camila.LogInformation("Camila UML · página · Inicio › Bienvenida");
        factory.CreateLogger("ATLAS.Application.Camila.CamilaWebhookService").LogWarning("Camila webhook X · modo Real · error · 12 ms");
        factory.CreateLogger("ATLAS.Infrastructure.Llm.RoutingLlmService").LogInformation("Llamada a modelo");
        factory.CreateLogger("ATLAS.Application.Agents.AgentRunner").LogError("falló");
        factory.CreateLogger("ATLAS.Application.Agents.AgentRunner").LogDebug("detalle que no se guarda");
        factory.CreateLogger("Microsoft.AspNetCore.Hosting").LogInformation("ruido del framework");

        var entries = store.Read().Entries;

        Assert.Equal(4, entries.Count);
        Assert.Equal(("Info", "Camila", "0a34c9a0"), (entries[0].Level, entries[0].Source, entries[0].ConversationId));
        Assert.Equal(("Aviso", "Webhook"), (entries[1].Level, entries[1].Source));
        Assert.Equal("Modelo", entries[2].Source);
        Assert.Equal(("Error", "Agente"), (entries[3].Level, entries[3].Source));
    }

    #endregion
}
