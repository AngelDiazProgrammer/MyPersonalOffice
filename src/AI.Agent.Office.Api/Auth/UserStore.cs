using AI.Agent.Office.Infrastructure.Configuration;

namespace AI.Agent.Office.Api.Auth;

/// <summary>Un usuario de la plataforma.</summary>
public sealed record PlatformUser(string Name, string Role, string PasswordHash);

/// <summary>Valida credenciales contra los usuarios configurados.</summary>
public interface IUserStore
{
    /// <summary>El usuario si las credenciales son correctas; null si no (sin distinguir qué falló).</summary>
    PlatformUser? Validate(string user, string password);
}

/// <summary>
/// Lee <c>configuration/usuarios.md</c>: una tabla Markdown <c>| usuario | rol | hash |</c> (spec 010, RF-002). La contraseña no se guarda en claro:
/// solo su hash con sal. Si el usuario no existe se hace igualmente una verificación falsa para no revelarlo por el tiempo de respuesta.
/// </summary>
public sealed class MarkdownUserStore : IUserStore
{
    private static readonly string DummyHash = PasswordHasher.Hash("no-existe");
    private readonly string? _path;

    /// <summary>Crea el almacén con la ruta del archivo (o la busca en la carpeta de configuración del repositorio).</summary>
    public MarkdownUserStore(string? path = null) => _path = path;

    /// <summary>El usuario si las credenciales son correctas.</summary>
    public PlatformUser? Validate(string user, string password)
    {
        var match = Load().FirstOrDefault(u => string.Equals(u.Name, user?.Trim(), StringComparison.OrdinalIgnoreCase));
        var ok = PasswordHasher.Verify(password ?? string.Empty, match?.PasswordHash ?? DummyHash);
        return ok ? match : null;
    }

    /// <summary>Usuarios del archivo (se relee en cada intento: son pocos y así un cambio se aplica sin reiniciar).</summary>
    public IReadOnlyList<PlatformUser> Load()
    {
        var path = _path ?? OfficeConfiguration.FindFile(Path.Combine("configuration", "usuarios.md"));
        if (path is null || !File.Exists(path)) return Array.Empty<PlatformUser>();

        var users = new List<PlatformUser>();
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (!line.StartsWith('|')) continue;
            var cells = line.Trim('|').Split('|', StringSplitOptions.TrimEntries);
            if (cells.Length >= 3 && cells[2].StartsWith(PasswordHasher.Prefix + "$", StringComparison.Ordinal))
                users.Add(new PlatformUser(cells[0], cells[1].Length == 0 ? "admin" : cells[1], cells[2]));
        }
        return users;
    }
}
