using AI.Agent.Office.Application.Browser;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace AI.Agent.Office.Infrastructure.Browser;

/// <summary>
/// Abre sesiones de Playwright con un perfil persistente por sitio (<c>browser-data/profiles/&lt;id&gt;</c>):
/// la persona inicia sesión a mano una vez y la sesión se reutiliza. Solo permite navegar a los dominios del sitio.
/// </summary>
public sealed class PlaywrightSessionFactory : IBrowserSessionFactory, IAsyncDisposable
{
    private readonly BrowserOptions _options;
    private readonly ILogger<PlaywrightSessionFactory> _logger;
    private readonly SemaphoreSlim _init = new(1, 1);
    private IPlaywright? _playwright;

    public PlaywrightSessionFactory(IOptions<BrowserOptions> options, ILogger<PlaywrightSessionFactory> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    #region Apertura

    public async Task<IBrowserSession> OpenAsync(BrowserSite site, bool headless, CancellationToken cancellationToken)
    {
        var context = await LaunchAsync(site, headless, cancellationToken).ConfigureAwait(false);

        // Toda navegación (también de pestañas nuevas y marcos) a un dominio no permitido se aborta.
        await context.RouteAsync("**/*", async route =>
        {
            var request = route.Request;
            if (request.IsNavigationRequest && !IsInternal(request.Url) && !BrowserPolicy.IsUrlAllowed(request.Url, site.AllowedDomains))
            {
                _logger.LogWarning("Navegación bloqueada fuera de los dominios permitidos del sitio {SiteId}", site.Id);
                await route.AbortAsync("blockedbyclient").ConfigureAwait(false);
                return;
            }
            await route.ContinueAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);

        // Playwright descarta por defecto los diálogos del navegador. Las alertas informativas (alert) se aceptan para poder
        // continuar (p. ej. «Es necesario aceptar las notificaciones para continuar»); confirmaciones y preguntas se rechazan.
        context.Dialog += async (_, dialog) =>
        {
            try
            {
                if (dialog.Type == "alert") await dialog.AcceptAsync().ConfigureAwait(false);
                else await dialog.DismissAsync().ConfigureAwait(false);
            }
            catch (PlaywrightException) { /* la página ya lo cerró */ }
        };

        var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync().ConfigureAwait(false);
        return new PlaywrightSession(context, page, TimeSpan.FromSeconds(Math.Max(2, _options.StepTimeoutSeconds)));
    }

    public async Task LoginManuallyAsync(BrowserSite site, TimeSpan timeout, CancellationToken cancellationToken)
    {
        // Visible y sin filtro de dominios: la persona manda (puede pasar por un proveedor de identidad).
        var context = await LaunchAsync(site, headless: false, cancellationToken).ConfigureAwait(false);
        var closed = new TaskCompletionSource();
        context.Close += (_, _) => closed.TrySetResult();

        try
        {
            var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync().ConfigureAwait(false);
            await page.GotoAsync(site.Url).ConfigureAwait(false);
            await Task.WhenAny(closed.Task, Task.Delay(timeout, cancellationToken)).ConfigureAwait(false);
        }
        catch (PlaywrightException) { /* la persona cerró la ventana durante la carga */ }
        finally
        {
            try { await context.CloseAsync().ConfigureAwait(false); } catch (PlaywrightException) { }
        }
    }

    public void ClearProfile(string siteId)
    {
        try
        {
            var dir = BrowserPaths.Profile(_options, BrowserPaths.RequireSafeId(siteId));
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning("No se pudo borrar el perfil guardado del sitio {SiteId}", siteId);
        }
    }

    private async Task<IBrowserContext> LaunchAsync(BrowserSite site, bool headless, CancellationToken ct)
    {
        var playwright = await GetPlaywrightAsync(ct).ConfigureAwait(false);
        var profile = BrowserPaths.Profile(_options, BrowserPaths.RequireSafeId(site.Id));
        Directory.CreateDirectory(profile);

        try
        {
            return await playwright.Chromium.LaunchPersistentContextAsync(profile, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = headless,
                Channel = string.IsNullOrWhiteSpace(_options.Channel) ? null : _options.Channel,
                Timeout = 30_000,
                Args = new[] { "--disable-blink-features=AutomationControlled" },
                IgnoreDefaultArgs = new[] { "--enable-automation" }
            }).ConfigureAwait(false);
        }
        catch (PlaywrightException ex)
        {
            _logger.LogWarning("No se pudo abrir el navegador para el sitio {SiteId}: {Reason}", site.Id, FirstLine(ex.Message));
            var hint = ex.Message.Contains("Executable doesn't exist", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("distribution", StringComparison.OrdinalIgnoreCase)
                ? $"No encuentro el navegador «{(_options.Channel is { Length: > 0 } ? _options.Channel : "chromium")}». Instala Microsoft Edge o Chrome, o descarga el de Playwright (ver docs/conexiones-navegador.md) y deja «Browser:Channel» vacío."
                : "No pude abrir el navegador. Si hay otra ventana de este sitio abierta (por ejemplo el inicio de sesión manual), ciérrala y reintenta.";
            throw new BrowserUnavailableException(hint, ex);
        }
    }

    private async Task<IPlaywright> GetPlaywrightAsync(CancellationToken ct)
    {
        if (_playwright is not null) return _playwright;
        await _init.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            return _playwright ??= await Playwright.CreateAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is PlaywrightException or IOException)
        {
            throw new BrowserUnavailableException("No se pudo iniciar Playwright (ver docs/conexiones-navegador.md).", ex);
        }
        finally
        {
            _init.Release();
        }
    }

    private static bool IsInternal(string url) =>
        url.StartsWith("about:", StringComparison.OrdinalIgnoreCase) || url.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
        || url.StartsWith("blob:", StringComparison.OrdinalIgnoreCase) || url.StartsWith("chrome-error:", StringComparison.OrdinalIgnoreCase);

    private static string FirstLine(string text) => text.Split('\n')[0].Trim();

    public async ValueTask DisposeAsync()
    {
        _playwright?.Dispose();
        await Task.CompletedTask;
    }

    #endregion
}

/// <summary>Sesión de Playwright: traduce los fallos de localización a <see cref="BrowserStepException"/>.</summary>
internal sealed class PlaywrightSession : IBrowserSession
{
    private readonly IBrowserContext _context;
    private readonly IPage _page;
    private readonly float _timeoutMs;

    public PlaywrightSession(IBrowserContext context, IPage page, TimeSpan stepTimeout)
    {
        _context = context;
        _page = page;
        _timeoutMs = (float)stepTimeout.TotalMilliseconds;

        // Al navegar a otra página, lo escrito antes ya no aplica.
        _page.FrameNavigated += (_, frame) =>
        {
            if (frame == _page.MainFrame) lock (_filled) _filled.Clear();
        };
    }

    /// <summary>Campos escritos en la página actual (selector → valor): se vuelven a poner si la página los borra antes de enviar.</summary>
    private readonly Dictionary<string, string> _filled = new();

    /// <summary>Primer elemento VISIBLE que coincide: sitios como Gmail duplican elementos ocultos con el mismo selector.</summary>
    private ILocator Loc(string target) => _page.Locator(target).Filter(new LocatorFilterOptions { Visible = true }).First;

    public string CurrentUrl => _page.Url;

    public Task GoToAsync(string url, CancellationToken ct) =>
        Guard($"abrir {url}", async () =>
        {
            await _page.GotoAsync(url, new PageGotoOptions { Timeout = _timeoutMs * 2, WaitUntil = WaitUntilState.DOMContentLoaded }).ConfigureAwait(false);
            // Mejor esfuerzo: muchas aplicaciones de una sola página se pintan de nuevo al terminar de cargar y borran lo escrito antes.
            try { await _page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 5000 }).ConfigureAwait(false); }
            catch (PlaywrightException) { }
        });

    public Task ClickAsync(string target, CancellationToken ct) =>
        Guard($"pulsar «{target}»", async () =>
        {
            await RestoreFilledAsync().ConfigureAwait(false);
            await Loc(target).ClickAsync(new LocatorClickOptions { Timeout = _timeoutMs }).ConfigureAwait(false);
        });

    public Task FillAsync(string target, string value, CancellationToken ct) =>
        Guard($"escribir en «{target}»", async () =>
        {
            var field = Loc(target);
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                if (attempt == 1)
                {
                    await field.FillAsync(value, new LocatorFillOptions { Timeout = _timeoutMs }).ConfigureAwait(false);
                }
                else
                {
                    // La página borró o no aceptó el valor: se limpia y se teclea tecla por tecla (como una persona).
                    await field.ClickAsync(new LocatorClickOptions { Timeout = _timeoutMs }).ConfigureAwait(false);
                    await field.FillAsync(string.Empty, new LocatorFillOptions { Timeout = _timeoutMs }).ConfigureAwait(false);
                    await field.PressSequentiallyAsync(value, new LocatorPressSequentiallyOptions { Delay = 30, Timeout = _timeoutMs * 2 }).ConfigureAwait(false);
                }

                await Task.Delay(300).ConfigureAwait(false);          // deja que la página termine de reaccionar
                if (await HoldsValueAsync(field, value).ConfigureAwait(false))
                {
                    lock (_filled) _filled[target] = value;
                    return;
                }
            }

            throw new BrowserStepException("el campo no retuvo el valor escrito (la página lo borra)");
        });

    /// <summary>True si el campo conserva el valor. Los elementos que no son de texto (contenteditable...) no se pueden comprobar y se dan por buenos.</summary>
    private static async Task<bool> HoldsValueAsync(ILocator field, string value)
    {
        try { return await field.InputValueAsync(new LocatorInputValueOptions { Timeout = 2000 }).ConfigureAwait(false) == value; }
        catch (PlaywrightException) { return true; }
    }

    /// <summary>Antes de pulsar (p. ej. «Ingresar»), vuelve a escribir los campos que la página haya borrado entretanto.</summary>
    private async Task RestoreFilledAsync()
    {
        KeyValuePair<string, string>[] fields;
        lock (_filled) fields = _filled.ToArray();

        foreach (var (target, value) in fields)
        {
            try
            {
                var field = Loc(target);
                if (await field.CountAsync().ConfigureAwait(false) == 0) continue;
                if (!await HoldsValueAsync(field, value).ConfigureAwait(false))
                    await field.FillAsync(value, new LocatorFillOptions { Timeout = 3000 }).ConfigureAwait(false);
            }
            catch (PlaywrightException) { /* la página cambió: no hay nada que restaurar */ }
        }
    }

    public Task PressAsync(string target, string key, CancellationToken ct) =>
        Guard($"teclear {key}", () => string.IsNullOrEmpty(target)
            ? _page.Keyboard.PressAsync(key)
            : Loc(target).PressAsync(key, new LocatorPressOptions { Timeout = _timeoutMs }));

    public Task WaitAsync(string target, CancellationToken ct) =>
        int.TryParse(target, out var ms)
            ? Task.Delay(Math.Clamp(ms, 0, 10_000), ct)
            : Guard($"esperar «{target}»", () => Loc(target).WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = _timeoutMs }));

    public Task SelectAsync(string target, string value, CancellationToken ct) =>
        Guard($"seleccionar en «{target}»", () => Loc(target).SelectOptionAsync(value, new LocatorSelectOptionOptions { Timeout = _timeoutMs }));

    public async Task<bool> IsTextVisibleAsync(string text, CancellationToken ct)
    {
        try
        {
            await _page.GetByText(text).First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = Math.Min(_timeoutMs, 5000) }).ConfigureAwait(false);
            return true;
        }
        catch (PlaywrightException) { return false; }
    }

    public async Task<PageSnapshot> SnapshotAsync(CancellationToken ct)
    {
        try
        {
            var json = await _page.EvaluateAsync<System.Text.Json.JsonElement?>(SnapshotScript).ConfigureAwait(false);
            var elements = new List<PageElement>();
            if (json is { ValueKind: System.Text.Json.JsonValueKind.Array } array)
                foreach (var e in array.EnumerateArray())
                    elements.Add(new PageElement(Str(e, "selector"), Str(e, "role"), Str(e, "name"), Str(e, "value")));
            return new PageSnapshot(_page.Url, await _page.TitleAsync().ConfigureAwait(false), elements);
        }
        catch (PlaywrightException ex)
        {
            throw new BrowserStepException("No pude leer la página.", ex);
        }
    }

    public async Task<IReadOnlyList<ExtractedRow>> ExtractAsync(string rowSelector, int max, CancellationToken ct)
    {
        try
        {
            var json = await _page.EvaluateAsync<System.Text.Json.JsonElement?>(ExtractScript, new { sel = rowSelector, max }).ConfigureAwait(false);
            var rows = new List<ExtractedRow>();
            if (json is { ValueKind: System.Text.Json.JsonValueKind.Array } array)
                foreach (var e in array.EnumerateArray())
                    rows.Add(new ExtractedRow(Str(e, "text"), NullIfEmpty(Str(e, "sender")), NullIfEmpty(Str(e, "email")),
                        NullIfEmpty(Str(e, "subject")), NullIfEmpty(Str(e, "snippet")), NullIfEmpty(Str(e, "date"))));
            return rows;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new BrowserStepException($"No pude leer las filas «{rowSelector}» ({ex.GetType().Name}: {FirstLine(ex.Message)}).", ex);
        }
    }

    private static string? NullIfEmpty(string text) => string.IsNullOrWhiteSpace(text) ? null : text;

    /// <summary>Lee las filas visibles; para bandejas de correo web intenta remitente, asunto, vista previa y fecha.</summary>
    private const string ExtractScript = """
        (args) => {
          const vis = e => { const r = e.getBoundingClientRect(); return r.width > 0 && r.height > 0; };
          const clean = e => e ? (e.innerText || e.textContent || '').trim().replace(/\s+/g, ' ') : '';
          return [...document.querySelectorAll(args.sel)].filter(vis).slice(0, args.max).map(r => {
            const who = r.querySelector('[email]');
            const when = r.querySelector('.xW span');
            return {
              text: clean(r).slice(0, 300),
              sender: who ? (who.getAttribute('name') || who.getAttribute('email') || '') : '',
              email: who ? (who.getAttribute('email') || '') : '',
              subject: clean(r.querySelector('.bog')),
              snippet: clean(r.querySelector('.y2')).replace(/^-\s*/, ''),
              date: (when && when.getAttribute('title')) || clean(r.querySelector('.xW'))
            };
          });
        }
        """;

    public async ValueTask DisposeAsync()
    {
        try { await _context.CloseAsync().ConfigureAwait(false); } catch (PlaywrightException) { }
    }

    private static async Task Guard(string what, Func<Task> action)
    {
        try { await action().ConfigureAwait(false); }
        catch (PlaywrightException ex) { throw new BrowserStepException($"No pude {what} ({FirstLine(ex.Message)}).", ex); }
        catch (TimeoutException ex) { throw new BrowserStepException($"Tiempo agotado al {what}.", ex); }
    }

    private static string FirstLine(string text) => text.Split('\n')[0].Trim();

    private static string Str(System.Text.Json.JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == System.Text.Json.JsonValueKind.String ? v.GetString() ?? string.Empty : string.Empty;

    /// <summary>Lista los elementos interactivos visibles con un selector utilizable. Nunca incluye el valor de contraseñas.</summary>
    private const string SnapshotScript = """
        () => {
          const vis = e => { const r = e.getBoundingClientRect(); const s = getComputedStyle(e); return r.width > 0 && r.height > 0 && s.visibility !== 'hidden' && s.display !== 'none'; };
          const esc = v => v.replace(/\\/g, '\\\\').replace(/"/g, '\\"');
          const q = 'a[href],button,input,textarea,select,[role=button],[role=textbox],[role=link],[role=menuitem],[role=checkbox],[contenteditable=true],[contenteditable=""]';
          const out = []; const seen = new Set();
          for (const e of document.querySelectorAll(q)) {
            if (!vis(e)) continue;
            const tag = e.tagName.toLowerCase();
            const role = e.getAttribute('role') || (tag === 'a' ? 'link' : tag === 'input' ? (e.type || 'text') : tag);
            const label = e.getAttribute('aria-label');
            const name = (label || e.innerText || e.placeholder || e.title || e.getAttribute('name') || '').trim().replace(/\s+/g, ' ').slice(0, 80);
            let sel = null;
            if (e.id && /^[A-Za-z][\w-]*$/.test(e.id)) sel = '#' + e.id;
            else if (label) sel = tag + '[aria-label="' + esc(label) + '"]';
            else if (e.getAttribute('name')) sel = tag + '[name="' + esc(e.getAttribute('name')) + '"]';
            else if (e.placeholder) sel = tag + '[placeholder="' + esc(e.placeholder) + '"]';
            else if (name && (tag === 'button' || tag === 'a' || role === 'button' || role === 'link')) sel = 'text="' + esc(name) + '"';
            if (!sel || seen.has(sel)) continue;
            seen.add(sel);
            const value = (tag === 'input' && e.type !== 'password') ? (e.value || '').slice(0, 60) : '';
            out.push({ selector: sel, role, name, value });
            if (out.length >= 80) break;
          }
          return out;
        }
        """;
}
