using Microsoft.Extensions.Configuration;

namespace ATLAS.Infrastructure.Configuration;

/// <summary>
/// Ubica los archivos de la raíz del repositorio (.env, configuration/, agents/) sin importar desde
/// dónde se inicie la app.
/// </summary>
public static class OfficeConfiguration
{
    /// <summary>
    /// Carga <c>.env</c> en las variables de entorno del proceso (sin pisar las existentes), agrega el
    /// <c>configuration/appsettings.json</c> compartido y vuelve a aplicar las variables de entorno para
    /// que conserven la máxima prioridad.
    /// </summary>
    public static IConfigurationBuilder AddOfficeConfiguration(this IConfigurationBuilder builder)
    {
        var envFile = FindUpwards(".env");
        if (envFile is not null) LoadDotEnv(envFile);

        var shared = FindUpwards(Path.Combine("configuration", "appsettings.json"));
        if (shared is not null) builder.AddJsonFile(shared, optional: true, reloadOnChange: false);

        return builder.AddEnvironmentVariables();
    }

    /// <summary>Busca una carpeta (absoluta o relativa) subiendo desde los directorios de trabajo.</summary>
    public static string? ResolveDirectory(string configured)
    {
        if (string.IsNullOrWhiteSpace(configured)) return null;
        if (Path.IsPathRooted(configured)) return Directory.Exists(configured) ? configured : null;
        return FindUpwards(configured, directory: true);
    }

    /// <summary>
    /// Resuelve una carpeta para escribir: la absoluta tal cual y la relativa junto a la raíz del
    /// repositorio (el padre de agents/).
    /// </summary>
    public static string ResolveWritableDirectory(string configured)
    {
        if (Path.IsPathRooted(configured)) return configured;
        var agents = FindUpwards("agents", directory: true);
        var root = agents is not null ? Path.GetDirectoryName(agents)! : AppContext.BaseDirectory;
        return Path.Combine(root, configured);
    }

    /// <summary>Busca un archivo por ruta relativa subiendo desde los directorios de trabajo y de la aplicación.</summary>
    public static string? FindFile(string relative) => FindUpwards(relative);

    /// <summary>Busca hacia arriba en el árbol de carpetas hasta encontrar la ruta.</summary>
    private static string? FindUpwards(string relative, bool directory = false)
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var dir = new DirectoryInfo(start);
            while (dir is not null)
            {
                var candidate = Path.Combine(dir.FullName, relative);
                if (directory ? Directory.Exists(candidate) : File.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }
        }
        return null;
    }

    /// <summary>
    /// Lee el archivo .env (CLAVE=valor) sin sobrescribir variables ya definidas; un valor en blanco
    /// bloquea la lectura.
    /// </summary>
    private static void LoadDotEnv(string path)
    {
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;

            var idx = line.IndexOf('=');
            if (idx <= 0) continue;

            var key = line[..idx].Trim();
            var value = line[(idx + 1)..].Trim().Trim('"', '\'');
            if (value.Length == 0) continue; // an empty value in .env must not mask a real environment variable

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                Environment.SetEnvironmentVariable(key, value);
        }
    }
}
