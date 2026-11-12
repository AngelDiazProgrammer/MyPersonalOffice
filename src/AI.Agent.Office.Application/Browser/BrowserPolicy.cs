using System.Net;
using System.Text.RegularExpressions;

namespace AI.Agent.Office.Application.Browser;

/// <summary>Reglas de seguridad de los sitios: dominios permitidos, hosts privados y validación de altas.</summary>
public static class BrowserPolicy
{
    private static readonly Regex IdPattern = new(@"^[a-z0-9][a-z0-9-]{1,39}$", RegexOptions.Compiled);
    private static readonly Regex DomainPattern = new(@"^(?=.{1,253}$)([a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?\.)*[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?$", RegexOptions.Compiled);

    /// <summary>True si la URL pertenece a uno de los dominios permitidos (el dominio exacto o un subdominio). about:blank se admite.</summary>
    public static bool IsUrlAllowed(string? url, IEnumerable<string> allowedDomains)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        if (url.Equals("about:blank", StringComparison.OrdinalIgnoreCase)) return true;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")) return false;

        var host = uri.Host.ToLowerInvariant();
        foreach (var raw in allowedDomains)
        {
            var domain = raw.Trim().ToLowerInvariant();
            if (domain.Length == 0) continue;
            if (host == domain || host.EndsWith("." + domain, StringComparison.Ordinal)) return true;
        }
        return false;
    }

    /// <summary>True para localhost, nombres .local/.internal y direcciones IP privadas, de bucle local o de enlace local.</summary>
    public static bool IsPrivateHost(string host)
    {
        host = host.Trim().Trim('[', ']').ToLowerInvariant();
        if (host is "localhost" || host.EndsWith(".localhost", StringComparison.Ordinal)
            || host.EndsWith(".local", StringComparison.Ordinal) || host.EndsWith(".internal", StringComparison.Ordinal)) return true;
        if (!IPAddress.TryParse(host, out var ip)) return false;
        if (IPAddress.IsLoopback(ip)) return true;

        if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6UniqueLocal) return true;
        if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) return false;
        var b = ip.GetAddressBytes();
        return b[0] == 10 || b[0] == 0 || (b[0] == 172 && b[1] is >= 16 and <= 31) || (b[0] == 192 && b[1] == 168) || (b[0] == 169 && b[1] == 254);
    }

    /// <summary>Valida un sitio antes de guardarlo; devuelve los problemas (vacío si es válido).</summary>
    public static IReadOnlyList<string> ValidateSite(BrowserSite site, bool allowPrivateHosts)
    {
        var errors = new List<string>();
        if (!IdPattern.IsMatch(site.Id ?? string.Empty)) errors.Add("El identificador debe tener 2-40 caracteres: minúsculas, números y guiones.");
        if (string.IsNullOrWhiteSpace(site.Name) || site.Name.Length > 80) errors.Add("El nombre es obligatorio (máximo 80 caracteres).");

        Uri? uri = null;
        if (!Uri.TryCreate(site.Url, UriKind.Absolute, out uri) || uri.Scheme is not ("http" or "https"))
            errors.Add("La URL debe ser absoluta y usar http o https.");
        else if (!string.IsNullOrEmpty(uri.UserInfo))
            errors.Add("La URL no puede incluir usuario ni contraseña.");
        else if (!allowPrivateHosts && IsPrivateHost(uri.Host))
            errors.Add("No se permiten direcciones locales o privadas.");

        if (site.AllowedDomains is null || site.AllowedDomains.Count == 0) errors.Add("Indica al menos un dominio permitido.");
        else
        {
            if (site.AllowedDomains.Count > 10) errors.Add("Máximo 10 dominios permitidos.");
            foreach (var domain in site.AllowedDomains)
            {
                var d = domain.Trim().ToLowerInvariant();
                if (!DomainPattern.IsMatch(d)) errors.Add($"Dominio no válido: «{domain}».");
                else if (!allowPrivateHosts && IsPrivateHost(d)) errors.Add($"Dominio no permitido: «{domain}».");
            }
            if (uri is not null && errors.Count == 0 && !IsUrlAllowed(site.Url, site.AllowedDomains))
                errors.Add("La URL inicial debe pertenecer a uno de los dominios permitidos.");
        }

        return errors;
    }

    /// <summary>Normaliza un sitio (minúsculas en id y dominios, sin espacios).</summary>
    public static BrowserSite Normalize(BrowserSite site)
    {
        var url = site.Url?.Trim() ?? string.Empty;
        var domains = (site.AllowedDomains ?? Array.Empty<string>()).Select(d => d.Trim().ToLowerInvariant()).Where(d => d.Length > 0).Distinct().ToList();
        if (domains.Count == 0 && Uri.TryCreate(url, UriKind.Absolute, out var uri)) domains.Add(uri.Host.ToLowerInvariant());
        return site with { Id = site.Id?.Trim().ToLowerInvariant() ?? string.Empty, Name = site.Name?.Trim() ?? string.Empty, Url = url, AllowedDomains = domains };
    }
}
