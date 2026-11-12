using System.Security.Cryptography;

namespace ATLAS.Api.Auth;

/// <summary>
/// Hash de contraseñas con PBKDF2-SHA256 y sal por usuario (spec 010, D-3). Formato guardado:
/// <c>pbkdf2-sha256$iteraciones$sal$hash</c> (Base64). La comparación es de tiempo constante.
/// </summary>
public static class PasswordHasher
{
    public const string Prefix = "pbkdf2-sha256";
    private const int DefaultIterations = 210_000;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    /// <summary>Genera la línea de hash para una contraseña nueva.</summary>
    public static string Hash(string password, int iterations = DefaultIterations)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, HashBytes);
        return $"{Prefix}${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>Comprueba una contraseña contra su hash guardado; cualquier formato inválido da falso.</summary>
    public static bool Verify(string password, string stored)
    {
        var parts = stored.Split('$');
        if (parts.Length != 4 || parts[0] != Prefix || !int.TryParse(parts[1], out var iterations) || iterations < 1000) return false;
        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
    }
}
