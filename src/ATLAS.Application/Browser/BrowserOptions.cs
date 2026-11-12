namespace ATLAS.Application.Browser;

/// <summary>Configuración de la conectividad a navegadores (sección «Browser»).</summary>
public sealed class BrowserOptions
{
    public const string SectionName = "Browser";

    public bool Enabled { get; set; } = true;

    /// <summary>Carpeta de datos (sitios y perfiles del navegador). No se versiona.</summary>
    public string DataDirectory { get; set; } = "browser-data";

    /// <summary>Carpeta de los playbooks (.md) de cada sitio.</summary>
    public string PlaybooksDirectory { get; set; } = "playbooks";

    /// <summary>Navegador invisible por defecto. Visible mientras se afina; invisible cuando el proceso es estable.</summary>
    public bool Headless { get; set; } = false;

    /// <summary>Navegador instalado que se usa («msedge», «chrome»). Vacío = el Chromium que descarga Playwright.</summary>
    public string? Channel { get; set; } = "msedge";

    public int MaxSteps { get; set; } = 60;
    public int MaxExploreActions { get; set; } = 12;
    public int RunTimeoutSeconds { get; set; } = 180;
    public int StepTimeoutSeconds { get; set; } = 15;
    public int MaxConcurrentRuns { get; set; } = 1;
    public int LoginTimeoutMinutes { get; set; } = 5;

    /// <summary>Permite sitios en direcciones locales o privadas (solo para pruebas y desarrollo).</summary>
    public bool AllowPrivateHosts { get; set; } = false;

    public int PendingMaxItems { get; set; } = 50;
    public int PendingLifetimeMinutes { get; set; } = 60;
}
