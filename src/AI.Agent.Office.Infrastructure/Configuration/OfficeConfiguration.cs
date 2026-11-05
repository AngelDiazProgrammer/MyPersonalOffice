using Microsoft.Extensions.Configuration;

namespace AI.Agent.Office.Infrastructure.Configuration;

/// <summary>Locates repo-level files (.env, configuration/, agents/) regardless of where the app is started.</summary>
public static class OfficeConfiguration
{
    /// <summary>
    /// Loads <c>.env</c> into process environment variables (without overriding existing ones), then adds the shared
    /// <c>configuration/appsettings.json</c> and re-applies environment variables so they keep the highest priority.
    /// </summary>
    public static IConfigurationBuilder AddOfficeConfiguration(this IConfigurationBuilder builder)
    {
        var envFile = FindUpwards(".env");
        if (envFile is not null) LoadDotEnv(envFile);

        var shared = FindUpwards(Path.Combine("configuration", "appsettings.json"));
        if (shared is not null) builder.AddJsonFile(shared, optional: true, reloadOnChange: false);

        return builder.AddEnvironmentVariables();
    }

    public static string? ResolveDirectory(string configured)
    {
        if (string.IsNullOrWhiteSpace(configured)) return null;
        if (Path.IsPathRooted(configured)) return Directory.Exists(configured) ? configured : null;
        return FindUpwards(configured, directory: true);
    }

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
