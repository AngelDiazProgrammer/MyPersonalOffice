using System.Text.RegularExpressions;

namespace ATLAS.Infrastructure.Llm;

/// <summary>Limpia los mensajes de error de los proveedores antes de mostrarlos: sin identificadores internos y con largo acotado.</summary>
internal static partial class ErrorText
{
    private const int MaxLength = 240;

    /// <summary>Quita identificadores de organización, de petición (GUID) y enlaces de facturación, y recorta el texto.</summary>
    public static string Clean(string text)
    {
        var cleaned = OrganizationId().Replace(text, "org_…");
        cleaned = Guid().Replace(cleaned, "…");
        cleaned = Url().Replace(cleaned, "(enlace omitido)");
        cleaned = cleaned.Replace("\r", " ").Replace("\n", " ").Trim();
        return cleaned.Length <= MaxLength ? cleaned : cleaned[..MaxLength] + "…";
    }

    [GeneratedRegex(@"org_[A-Za-z0-9]+")]
    private static partial Regex OrganizationId();

    [GeneratedRegex(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}")]
    private static partial Regex Guid();

    [GeneratedRegex(@"https?://\S+")]
    private static partial Regex Url();
}
