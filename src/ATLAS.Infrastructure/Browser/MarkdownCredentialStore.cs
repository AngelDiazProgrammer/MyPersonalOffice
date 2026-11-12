using ATLAS.Application.Browser;
using Microsoft.Extensions.Options;

namespace ATLAS.Infrastructure.Browser;

/// <summary>
/// Guarda las credenciales de cada conexión en <c>browser-data/credentials/&lt;id&gt;.md</c> para que sobrevivan al reinicio.
/// DECISIÓN de la persona responsable (spec 007, 2026-11-10): **texto plano, solo entorno de pruebas**. La carpeta está en
/// <c>.gitignore</c>; las credenciales nunca se devuelven por la API, ni van a registros, eventos o modelos.
/// </summary>
public sealed class MarkdownCredentialStore : IBrowserCredentialStore
{
    private readonly BrowserOptions _options;
    private readonly object _gate = new();
    private readonly Dictionary<string, BrowserCredentials?> _cache = new(StringComparer.OrdinalIgnoreCase);

    public MarkdownCredentialStore(IOptions<BrowserOptions> options) => _options = options.Value;

    private string PathOf(string siteId) =>
        Path.Combine(BrowserPaths.Data(_options), "credentials", BrowserPaths.RequireSafeId(siteId) + ".md");

    public BrowserCredentials? Get(string siteId)
    {
        lock (_gate)
        {
            if (_cache.TryGetValue(siteId, out var cached)) return cached;
            var path = PathOf(siteId);
            var loaded = File.Exists(path) ? Parse(File.ReadAllText(path)) : null;
            _cache[siteId] = loaded;
            return loaded;
        }
    }

    public void Set(string siteId, BrowserCredentials credentials)
    {
        lock (_gate)
        {
            var path = PathOf(siteId);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var temp = path + ".tmp";
            File.WriteAllText(temp, Render(siteId, credentials));
            File.Move(temp, path, overwrite: true);
            _cache[siteId] = credentials;
        }
    }

    public void Clear(string siteId)
    {
        lock (_gate)
        {
            var path = PathOf(siteId);
            if (File.Exists(path)) File.Delete(path);
            _cache[siteId] = null;
        }
    }

    /// <summary>Contenido del archivo (público para poder probarlo).</summary>
    public static string Render(string siteId, BrowserCredentials c) =>
        $"# Credenciales · {siteId}\n\n" +
        "> ⚠️ Texto plano, solo entorno de pruebas. Esta carpeta está en `.gitignore`: no la subas a git ni la compartas.\n\n" +
        $"usuario: {c.Username}\n" +
        $"clave: {c.Password}\n";

    public static BrowserCredentials? Parse(string markdown)
    {
        string? user = null, pass = null;
        foreach (var raw in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            if (raw.StartsWith("usuario:", StringComparison.OrdinalIgnoreCase)) user = raw[8..].Trim();
            else if (raw.StartsWith("clave:", StringComparison.OrdinalIgnoreCase)) pass = raw[6..].TrimEnd();
        }
        var result = new BrowserCredentials(user, pass?.TrimStart());
        return result.HasAny ? result : null;
    }
}
