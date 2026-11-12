using ATLAS.Application.Browser;

namespace ATLAS.Tests.TestSupport;

/// <summary>Almacén de sitios en memoria.</summary>
public sealed class InMemorySiteStore : IBrowserSiteStore
{
    private readonly List<BrowserSite> _sites = new();
    public IReadOnlyList<BrowserSite> GetAll() => _sites.ToArray();
    public BrowserSite? Get(string id) => _sites.FirstOrDefault(s => s.Id == id);
    public Task SaveAsync(BrowserSite site, CancellationToken ct)
    {
        _sites.RemoveAll(s => s.Id == site.Id);
        _sites.Add(site);
        return Task.CompletedTask;
    }
    public Task<bool> DeleteAsync(string id, CancellationToken ct) => Task.FromResult(_sites.RemoveAll(s => s.Id == id) > 0);
}

/// <summary>Almacén de playbooks en memoria.</summary>
public sealed class InMemoryPlaybookStore : IPlaybookStore
{
    public Dictionary<string, string> Playbooks { get; } = new();
    public Dictionary<string, string> Proposals { get; } = new();
    public Task<string?> ReadAsync(string siteId, CancellationToken ct) => Task.FromResult(Playbooks.GetValueOrDefault(siteId));
    public Task WriteAsync(string siteId, string markdown, CancellationToken ct) { Playbooks[siteId] = markdown; return Task.CompletedTask; }
    public Task<string?> ReadProposalAsync(string siteId, CancellationToken ct) => Task.FromResult(Proposals.GetValueOrDefault(siteId));
    public Task WriteProposalAsync(string siteId, string markdown, CancellationToken ct) { Proposals[siteId] = markdown; return Task.CompletedTask; }
    public Task DeleteProposalAsync(string siteId, CancellationToken ct) { Proposals.Remove(siteId); return Task.CompletedTask; }
    public Task DeleteAsync(string siteId, CancellationToken ct) { Playbooks.Remove(siteId); Proposals.Remove(siteId); return Task.CompletedTask; }
}

/// <summary>Navegador simulado: registra las acciones y falla en los selectores indicados.</summary>
public sealed class FakeBrowserSession : IBrowserSession
{
    public List<string> Actions { get; } = new();
    public HashSet<string> BrokenSelectors { get; } = new();
    public HashSet<string> WorkingSelectors { get; } = new();
    public string CurrentUrl { get; set; } = "about:blank";
    public string RedirectAfterClickTo { get; set; } = string.Empty;
    public Func<PageSnapshot> Snapshot { get; set; } = () => new PageSnapshot("https://mail.example.com/inbox", "Bandeja", new[]
    {
        new PageElement("button.nuevo", "button", "Redactar"), new PageElement("input.para", "text", "Para")
    });

    private void Do(string action, string target)
    {
        if (BrokenSelectors.Contains(target) && !WorkingSelectors.Contains(target))
            throw new BrowserStepException($"No pude {action} «{target}».");
        Actions.Add($"{action} {target}");
    }

    public Task GoToAsync(string url, CancellationToken ct) { Actions.Add("goto " + url); CurrentUrl = url; return Task.CompletedTask; }
    public Task ClickAsync(string target, CancellationToken ct)
    {
        Do("click", target);
        if (RedirectAfterClickTo.Length > 0) CurrentUrl = RedirectAfterClickTo;
        return Task.CompletedTask;
    }
    public Task FillAsync(string target, string value, CancellationToken ct) { Do("fill", target); Actions.Add("value:" + value); return Task.CompletedTask; }
    public Task PressAsync(string target, string key, CancellationToken ct) { Do("press", target + key); return Task.CompletedTask; }
    public Task WaitAsync(string target, CancellationToken ct) { Do("wait", target); return Task.CompletedTask; }
    public Task SelectAsync(string target, string value, CancellationToken ct) { Do("select", target); return Task.CompletedTask; }
    public Task<bool> IsTextVisibleAsync(string text, CancellationToken ct) => Task.FromResult(!BrokenSelectors.Contains(text));
    public Task<PageSnapshot> SnapshotAsync(CancellationToken ct) => Task.FromResult(Snapshot());

    public List<ExtractedRow> ExtractRows { get; } = new();
    public Task<IReadOnlyList<ExtractedRow>> ExtractAsync(string rowSelector, int max, CancellationToken ct)
    {
        Actions.Add($"extract {rowSelector} {max}");
        return Task.FromResult<IReadOnlyList<ExtractedRow>>(ExtractRows.Take(max).ToArray());
    }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

/// <summary>Fábrica que entrega siempre la misma sesión simulada.</summary>
public sealed class FakeSessionFactory : IBrowserSessionFactory
{
    public FakeBrowserSession Session { get; } = new();
    public bool? LastHeadless { get; private set; }
    public int Opened { get; private set; }
    public int Logins { get; private set; }
    public List<string> Cleared { get; } = new();

    public Task<IBrowserSession> OpenAsync(BrowserSite site, bool headless, CancellationToken ct)
    {
        Opened++;
        LastHeadless = headless;
        return Task.FromResult<IBrowserSession>(Session);
    }

    public Task LoginManuallyAsync(BrowserSite site, TimeSpan timeout, CancellationToken ct) { Logins++; return Task.CompletedTask; }
    public void ClearProfile(string siteId) => Cleared.Add(siteId);
}
