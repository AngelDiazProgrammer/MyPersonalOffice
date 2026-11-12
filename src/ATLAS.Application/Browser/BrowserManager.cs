using ATLAS.Application.DTOs;
using ATLAS.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ATLAS.Application.Browser;

/// <summary>
/// Gestiona sitios y playbooks (alta, baja, edición, propuestas aprendidas), el inicio de sesión manual
/// y las ejecuciones directas desde el front. Todo pasa por las reglas de <see cref="BrowserPolicy"/>.
/// </summary>
public sealed class BrowserManager
{
    private readonly IBrowserSiteStore _sites;
    private readonly IPlaybookStore _playbooks;
    private readonly IBrowserSessionFactory _factory;
    private readonly IBrowserCredentialStore _secrets;
    private readonly BrowserRunner _runner;
    private readonly BrowserOptions _options;

    public BrowserManager(
        IBrowserSiteStore sites, IPlaybookStore playbooks, IBrowserSessionFactory factory,
        BrowserRunner runner, IBrowserCredentialStore secrets, IOptions<BrowserOptions> options)
    {
        _sites = sites;
        _playbooks = playbooks;
        _factory = factory;
        _secrets = secrets;
        _runner = runner;
        _options = options.Value;
    }

    public bool Enabled => _options.Enabled;

    public void ClearCredentials(string siteId) => _secrets.Clear(siteId);

    public bool SiteExists(string id) => _sites.Get(id ?? string.Empty) is not null;

    #region Sitios

    public async Task<IReadOnlyList<BrowserSiteDto>> ListAsync(CancellationToken ct)
    {
        var result = new List<BrowserSiteDto>();
        foreach (var site in _sites.GetAll().OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase))
        {
            var proposal = await _playbooks.ReadProposalAsync(site.Id, ct).ConfigureAwait(false);
            result.Add(ToDto(site, proposal is not null));
        }
        return result;
    }

    /// <summary>Crea o actualiza un sitio. Un sitio nuevo recibe un playbook de plantilla (solo abre la URL).</summary>
    public async Task<BrowserSiteDto> SaveSiteAsync(BrowserSite input, CancellationToken ct, BrowserCredentials? credentials = null)
    {
        var site = BrowserPolicy.Normalize(input);
        var errors = BrowserPolicy.ValidateSite(site, _options.AllowPrivateHosts);
        if (errors.Count > 0) throw new OfficeValidationException(errors);

        await _sites.SaveAsync(site, ct).ConfigureAwait(false);

        // Credenciales de la conexión (archivo .md por conexión). Lo que venga vacío conserva el valor anterior.
        if (credentials is { HasAny: true })
        {
            var old = _secrets.Get(site.Id);
            _secrets.Set(site.Id, new BrowserCredentials(credentials.Username ?? old?.Username, credentials.Password ?? old?.Password));
        }
        if (await _playbooks.ReadAsync(site.Id, ct).ConfigureAwait(false) is null)
            await _playbooks.WriteAsync(site.Id, PlaybookParser.Template(site.Name, site.Url), ct).ConfigureAwait(false);

        return ToDto(site, await _playbooks.ReadProposalAsync(site.Id, ct).ConfigureAwait(false) is not null);
    }

    /// <summary>Quita un sitio, su playbook, su propuesta y la sesión guardada del navegador.</summary>
    public async Task<bool> DeleteSiteAsync(string id, CancellationToken ct)
    {
        if (!await _sites.DeleteAsync(id, ct).ConfigureAwait(false)) return false;
        await _playbooks.DeleteAsync(id, ct).ConfigureAwait(false);
        _factory.ClearProfile(id);
        _secrets.Clear(id);
        return true;
    }

    #endregion

    #region Playbooks y propuestas

    public async Task<PlaybookDto?> GetPlaybookAsync(string siteId, CancellationToken ct)
    {
        if (_sites.Get(siteId) is null) return null;
        var markdown = await _playbooks.ReadAsync(siteId, ct).ConfigureAwait(false) ?? string.Empty;
        var proposal = await _playbooks.ReadProposalAsync(siteId, ct).ConfigureAwait(false);
        return ToDto(siteId, markdown, proposal);
    }

    /// <summary>Guarda el playbook editado desde el front, solo si es válido.</summary>
    public async Task<PlaybookDto> SavePlaybookAsync(string siteId, string markdown, CancellationToken ct)
    {
        RequireSite(siteId);
        if (markdown.Length > 20_000) throw new OfficeValidationException(new[] { "El playbook supera el tamaño permitido." });
        var parsed = PlaybookParser.Parse(siteId, markdown);
        if (!parsed.IsValid) throw new OfficeValidationException(parsed.Errors);

        await _playbooks.WriteAsync(siteId, markdown, ct).ConfigureAwait(false);
        return ToDto(siteId, markdown, await _playbooks.ReadProposalAsync(siteId, ct).ConfigureAwait(false));
    }

    /// <summary>Aprueba la propuesta aprendida: sustituye al playbook. Se rechaza si quedan valores por revisar (RN-002, RN-003).</summary>
    public async Task<PlaybookDto> ApproveProposalAsync(string siteId, CancellationToken ct)
    {
        RequireSite(siteId);
        var proposal = await _playbooks.ReadProposalAsync(siteId, ct).ConfigureAwait(false)
            ?? throw new OfficeValidationException(new[] { "No hay ninguna propuesta pendiente para este sitio." });

        if (proposal.Contains(PlaybookParser.ReviewMarker, StringComparison.Ordinal))
            throw new OfficeValidationException(new[] { $"La propuesta tiene valores por revisar ({PlaybookParser.ReviewMarker}). Edítala y reemplázalos por una {{{{variable}}}} antes de aprobarla." });
        var parsed = PlaybookParser.Parse(siteId, proposal);
        if (!parsed.IsValid) throw new OfficeValidationException(parsed.Errors);

        await _playbooks.WriteAsync(siteId, proposal, ct).ConfigureAwait(false);
        await _playbooks.DeleteProposalAsync(siteId, ct).ConfigureAwait(false);
        return ToDto(siteId, proposal, null);
    }

    public async Task RejectProposalAsync(string siteId, CancellationToken ct)
    {
        RequireSite(siteId);
        await _playbooks.DeleteProposalAsync(siteId, ct).ConfigureAwait(false);
    }

    /// <summary>Guarda una propuesta editada a mano (para resolver los «{{REVISAR}}» antes de aprobarla).</summary>
    public async Task<PlaybookDto> SaveProposalAsync(string siteId, string markdown, CancellationToken ct)
    {
        RequireSite(siteId);
        var parsed = PlaybookParser.Parse(siteId, markdown);
        if (!parsed.IsValid) throw new OfficeValidationException(parsed.Errors);
        await _playbooks.WriteProposalAsync(siteId, markdown, ct).ConfigureAwait(false);
        return ToDto(siteId, await _playbooks.ReadAsync(siteId, ct).ConfigureAwait(false) ?? string.Empty, markdown);
    }

    #endregion

    #region Inicio de sesión y ejecución directa

    /// <summary>Abre el navegador visible para que la persona inicie sesión a mano; la sesión queda en el perfil del sitio.</summary>
    public async Task LoginAsync(string siteId, CancellationToken ct)
    {
        var site = RequireSite(siteId);
        await _factory.LoginManuallyAsync(site, TimeSpan.FromMinutes(Math.Max(1, _options.LoginTimeoutMinutes)), ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Ejecución directa desde el front: la persona da todos los valores y pulsa «Ejecutar» (esa acción es la
    /// aprobación). Las credenciales son opcionales y no se guardan.
    /// </summary>
    public async Task<BrowserRunResultDto> RunAsync(string siteId, IReadOnlyDictionary<string, string>? values, BrowserCredentials? credentials, CancellationToken ct)
    {
        var site = RequireSite(siteId);
        if (!site.Enabled) throw new OfficeValidationException(new[] { "El sitio está desactivado." });

        var markdown = await _playbooks.ReadAsync(siteId, ct).ConfigureAwait(false);
        var parsed = PlaybookParser.Parse(siteId, markdown ?? string.Empty);
        if (!parsed.IsValid) throw new OfficeValidationException(parsed.Errors);

        var given = values ?? new Dictionary<string, string>();
        var missing = parsed.Playbook!.Inputs.Where(i => !given.TryGetValue(i.Name, out var v) || string.IsNullOrWhiteSpace(v)).Select(i => i.Name).ToArray();
        if (missing.Length > 0) throw new OfficeValidationException(new[] { "Faltan valores: " + string.Join(", ", missing) + "." });

        var result = await _runner.RunAsync(site, parsed.Playbook, given, credentials, ct).ConfigureAwait(false);
        return ToDto(result);
    }

    #endregion

    #region Utilidades

    private BrowserSite RequireSite(string id) =>
        _sites.Get(id) ?? throw new OfficeValidationException(new[] { "El sitio no existe." });

    private BrowserSiteDto ToDto(BrowserSite s, bool hasProposal) =>
        new(s.Id, s.Name, s.Url, s.AllowedDomains, s.Headless, s.Enabled, hasProposal, _secrets.Get(s.Id) is not null, _secrets.Get(s.Id)?.Username);

    private static PlaybookDto ToDto(string siteId, string markdown, string? proposal)
    {
        var parsed = PlaybookParser.Parse(siteId, markdown);
        var inputs = parsed.Playbook?.Inputs.Select(i => new PlaybookInputDto(i.Name, i.Draft ? "redactar" : i.Question, i.Draft)).ToArray()
                     ?? Array.Empty<PlaybookInputDto>();
        return new PlaybookDto(siteId, markdown, parsed.Errors, inputs, proposal);
    }

    public static BrowserRunResultDto ToDto(BrowserRunResult r) =>
        new(r.Success, r.Message, r.DurationMs, r.StepsExecuted, r.Mode, r.ModelCalls, r.ProposalCreated, r.Log,
            r.Data?.Select(d => new ExtractedRowDto(d.Text, d.Sender, d.Email, d.Subject, d.Snippet, d.Date)).ToArray());

    #endregion
}
