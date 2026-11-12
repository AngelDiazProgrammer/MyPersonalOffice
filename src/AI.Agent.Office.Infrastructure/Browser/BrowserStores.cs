using System.Text.Json;
using System.Text.RegularExpressions;
using AI.Agent.Office.Application.Browser;
using AI.Agent.Office.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Infrastructure.Browser;

/// <summary>Resuelve las carpetas de la conectividad a navegadores (datos y playbooks) junto a la raíz del repositorio.</summary>
internal static class BrowserPaths
{
    public static string Data(BrowserOptions options) => OfficeConfiguration.ResolveWritableDirectory(options.DataDirectory);
    public static string Playbooks(BrowserOptions options) => OfficeConfiguration.ResolveWritableDirectory(options.PlaybooksDirectory);
    public static string Profile(BrowserOptions options, string siteId) => Path.Combine(Data(options), "profiles", siteId);

    private static readonly Regex SafeId = new(@"^[a-z0-9][a-z0-9-]{0,39}$", RegexOptions.Compiled);

    /// <summary>Un identificador de sitio nunca puede escapar de su carpeta (sin separadores ni «..»).</summary>
    public static string RequireSafeId(string id)
    {
        if (!SafeId.IsMatch(id ?? string.Empty)) throw new ArgumentException("Identificador de sitio no válido.", nameof(id));
        return id!;
    }
}

/// <summary>Guarda los sitios en <c>browser-data/sites.json</c> (sin credenciales). Escritura atómica.</summary>
public sealed class JsonBrowserSiteStore : IBrowserSiteStore
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly BrowserOptions _options;
    private readonly object _gate = new();
    private List<BrowserSite>? _sites;

    public JsonBrowserSiteStore(IOptions<BrowserOptions> options) => _options = options.Value;

    private string FilePath => Path.Combine(BrowserPaths.Data(_options), "sites.json");

    private List<BrowserSite> Load()
    {
        if (_sites is not null) return _sites;
        try
        {
            _sites = File.Exists(FilePath)
                ? JsonSerializer.Deserialize<List<BrowserSite>>(File.ReadAllText(FilePath), Json) ?? new()
                : new();
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            _sites = new(); // un archivo dañado no debe impedir arrancar; se vuelve a escribir al guardar
        }
        return _sites;
    }

    public IReadOnlyList<BrowserSite> GetAll()
    {
        lock (_gate) return Load().ToArray();
    }

    public BrowserSite? Get(string id)
    {
        lock (_gate) return Load().FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public Task SaveAsync(BrowserSite site, CancellationToken cancellationToken)
    {
        BrowserPaths.RequireSafeId(site.Id);
        lock (_gate)
        {
            var list = Load();
            var index = list.FindIndex(s => s.Id.Equals(site.Id, StringComparison.OrdinalIgnoreCase));
            if (index >= 0) list[index] = site; else list.Add(site);
            Persist(list);
        }
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var list = Load();
            var removed = list.RemoveAll(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) > 0;
            if (removed) Persist(list);
            return Task.FromResult(removed);
        }
    }

    private void Persist(List<BrowserSite> list)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var temp = FilePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(list, Json));
        File.Move(temp, FilePath, overwrite: true);
    }
}

/// <summary>Guarda los playbooks como Markdown: <c>playbooks/&lt;id&gt;.md</c> y su propuesta aprendida <c>&lt;id&gt;.propuesta.md</c>.</summary>
public sealed class MarkdownPlaybookStore : IPlaybookStore
{
    private readonly BrowserOptions _options;

    public MarkdownPlaybookStore(IOptions<BrowserOptions> options) => _options = options.Value;

    private string Path_(string siteId, bool proposal) =>
        Path.Combine(BrowserPaths.Playbooks(_options), BrowserPaths.RequireSafeId(siteId) + (proposal ? ".propuesta.md" : ".md"));

    public async Task<string?> ReadAsync(string siteId, CancellationToken cancellationToken) => await ReadFileAsync(Path_(siteId, false), cancellationToken).ConfigureAwait(false);

    public Task WriteAsync(string siteId, string markdown, CancellationToken cancellationToken) => WriteFileAsync(Path_(siteId, false), markdown, cancellationToken);

    public async Task<string?> ReadProposalAsync(string siteId, CancellationToken cancellationToken) => await ReadFileAsync(Path_(siteId, true), cancellationToken).ConfigureAwait(false);

    public Task WriteProposalAsync(string siteId, string markdown, CancellationToken cancellationToken) => WriteFileAsync(Path_(siteId, true), markdown, cancellationToken);

    public Task DeleteProposalAsync(string siteId, CancellationToken cancellationToken)
    {
        var path = Path_(siteId, true);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string siteId, CancellationToken cancellationToken)
    {
        foreach (var path in new[] { Path_(siteId, false), Path_(siteId, true) })
            if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private static async Task<string?> ReadFileAsync(string path, CancellationToken ct) =>
        File.Exists(path) ? await File.ReadAllTextAsync(path, ct).ConfigureAwait(false) : null;

    private static async Task WriteFileAsync(string path, string markdown, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = path + ".tmp";
        await File.WriteAllTextAsync(temp, markdown, ct).ConfigureAwait(false);
        File.Move(temp, path, overwrite: true);
    }
}
