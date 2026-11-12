using System.Collections.Concurrent;

namespace ATLAS.Api.Auth;

/// <summary>Bloquea el acceso tras varios fallos seguidos del mismo usuario desde el mismo origen (spec 010, RF-003).</summary>
public sealed class LoginThrottle
{
    private readonly ConcurrentDictionary<string, (int Failures, DateTimeOffset LockedUntil)> _state = new();
    private readonly int _maxFailures;
    private readonly TimeSpan _lock;
    private readonly Func<DateTimeOffset> _now;

    public LoginThrottle(int maxFailures = 5, TimeSpan? lockFor = null, Func<DateTimeOffset>? now = null)
    {
        _maxFailures = maxFailures;
        _lock = lockFor ?? TimeSpan.FromMinutes(5);
        _now = now ?? (() => DateTimeOffset.UtcNow);
    }

    private static string Key(string user, string origin) => user.Trim().ToLowerInvariant() + "|" + origin;

    /// <summary>¿Está bloqueado ahora? (aunque la contraseña sea correcta).</summary>
    public bool IsLocked(string user, string origin) =>
        _state.TryGetValue(Key(user, origin), out var s) && s.LockedUntil > _now();

    /// <summary>Anota un fallo; al llegar al máximo bloquea.</summary>
    public void Fail(string user, string origin) =>
        _state.AddOrUpdate(Key(user, origin),
            _ => (1, DateTimeOffset.MinValue),
            (_, s) =>
            {
                var failures = s.Failures + 1;
                return failures >= _maxFailures ? (0, _now() + _lock) : (failures, DateTimeOffset.MinValue);
            });

    /// <summary>Un acceso correcto reinicia la cuenta.</summary>
    public void Succeed(string user, string origin) => _state.TryRemove(Key(user, origin), out _);
}
