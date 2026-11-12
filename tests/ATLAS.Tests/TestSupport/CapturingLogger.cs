using Microsoft.Extensions.Logging;

namespace ATLAS.Tests.TestSupport;

/// <summary>Una línea de registro capturada: categoría, nivel, mensaje ya formateado, campos del mensaje y campos de los ámbitos activos.</summary>
public sealed record CapturedLog(string Category, LogLevel Level, string Message, IReadOnlyDictionary<string, object?> Fields, IReadOnlyDictionary<string, object?> Scope);

/// <summary>Proveedor de registro en memoria que conserva los ámbitos (CorrelationId, TaskId...) vigentes al escribir cada línea.</summary>
public sealed class CapturingLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly object _gate = new();
    private readonly List<CapturedLog> _entries = new();
    private IExternalScopeProvider? _scopes;

    public IReadOnlyList<CapturedLog> Entries { get { lock (_gate) { return _entries.ToArray(); } } }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopes = scopeProvider;
    public ILogger CreateLogger(string categoryName) => new CapturingLogger(this, categoryName);
    public void Dispose() { }

    public ILoggerFactory CreateFactory() => LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Trace).AddProvider(this));

    private sealed class CapturingLogger : ILogger
    {
        private readonly CapturingLoggerProvider _owner;
        private readonly string _category;

        public CapturingLogger(CapturingLoggerProvider owner, string category) { _owner = owner; _category = category; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _owner._scopes?.Push(state);
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var scope = new Dictionary<string, object?>();
            _owner._scopes?.ForEachScope((s, target) =>
            {
                if (s is IEnumerable<KeyValuePair<string, object>> pairs)
                    foreach (var (key, value) in pairs) target[key] = value;
            }, scope);

            var fields = new Dictionary<string, object?>();
            if (state is IEnumerable<KeyValuePair<string, object?>> structured)
                foreach (var (key, value) in structured) fields[key] = value;

            lock (_owner._gate) { _owner._entries.Add(new CapturedLog(_category, logLevel, formatter(state, exception), fields, scope)); }
        }
    }
}
