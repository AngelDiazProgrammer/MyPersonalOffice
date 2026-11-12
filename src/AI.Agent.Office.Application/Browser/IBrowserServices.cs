namespace AI.Agent.Office.Application.Browser;

/// <summary>Una sesión de navegador abierta sobre un sitio. Oculta Playwright a la capa de aplicación.</summary>
public interface IBrowserSession : IAsyncDisposable
{
    string CurrentUrl { get; }
    Task GoToAsync(string url, CancellationToken cancellationToken);
    Task ClickAsync(string target, CancellationToken cancellationToken);
    Task FillAsync(string target, string value, CancellationToken cancellationToken);

    /// <summary>Pulsa una tecla; con un objetivo vacío, sobre la página.</summary>
    Task PressAsync(string target, string key, CancellationToken cancellationToken);

    /// <summary>Espera a que un elemento sea visible o, si el objetivo es un número, esa cantidad de milisegundos.</summary>
    Task WaitAsync(string target, CancellationToken cancellationToken);

    Task SelectAsync(string target, string value, CancellationToken cancellationToken);
    Task<bool> IsTextVisibleAsync(string text, CancellationToken cancellationToken);
    Task<PageSnapshot> SnapshotAsync(CancellationToken cancellationToken);

    /// <summary>Lee hasta <paramref name="max"/> filas visibles que coincidan con un selector CSS (por ejemplo las filas de la bandeja).</summary>
    Task<IReadOnlyList<ExtractedRow>> ExtractAsync(string rowSelector, int max, CancellationToken cancellationToken);
}

/// <summary>
/// Credenciales de cada conexión. La implementación de la aplicación las guarda en un .md por conexión (texto plano, solo
/// pruebas, por decisión de la persona responsable); nunca van a registros, eventos, respuestas de la API ni modelos.
/// </summary>
public interface IBrowserCredentialStore
{
    void Set(string siteId, BrowserCredentials credentials);
    BrowserCredentials? Get(string siteId);
    void Clear(string siteId);
}

/// <summary>Implementación solo en memoria (pruebas).</summary>
public sealed class BrowserCredentialStore : IBrowserCredentialStore
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, BrowserCredentials> _items = new(StringComparer.OrdinalIgnoreCase);
    public void Set(string siteId, BrowserCredentials credentials) => _items[siteId] = credentials;
    public BrowserCredentials? Get(string siteId) => _items.TryGetValue(siteId, out var c) ? c : null;
    public void Clear(string siteId) => _items.TryRemove(siteId, out _);
}

/// <summary>Abre sesiones de navegador con el perfil persistente de cada sitio (inicio de sesión manual una sola vez).</summary>
public interface IBrowserSessionFactory
{
    /// <summary>Abre una sesión. Solo puede navegar a los dominios permitidos del sitio.</summary>
    Task<IBrowserSession> OpenAsync(BrowserSite site, bool headless, CancellationToken cancellationToken);

    /// <summary>Abre el navegador VISIBLE para que la persona inicie sesión a mano; termina cuando cierra la ventana o vence el tiempo.</summary>
    Task LoginManuallyAsync(BrowserSite site, TimeSpan timeout, CancellationToken cancellationToken);

    /// <summary>Borra el perfil guardado (sesión) de un sitio.</summary>
    void ClearProfile(string siteId);
}

/// <summary>Almacén de los sitios configurados.</summary>
public interface IBrowserSiteStore
{
    IReadOnlyList<BrowserSite> GetAll();
    BrowserSite? Get(string id);
    Task SaveAsync(BrowserSite site, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}

/// <summary>Almacén de los playbooks (.md) y de sus propuestas aprendidas.</summary>
public interface IPlaybookStore
{
    Task<string?> ReadAsync(string siteId, CancellationToken cancellationToken);
    Task WriteAsync(string siteId, string markdown, CancellationToken cancellationToken);
    Task<string?> ReadProposalAsync(string siteId, CancellationToken cancellationToken);
    Task WriteProposalAsync(string siteId, string markdown, CancellationToken cancellationToken);
    Task DeleteProposalAsync(string siteId, CancellationToken cancellationToken);
    Task DeleteAsync(string siteId, CancellationToken cancellationToken);
}
