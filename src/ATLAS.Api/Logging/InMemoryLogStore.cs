using System.Collections.Concurrent;
using ATLAS.Application.DTOs;

namespace ATLAS.Api.Logging;

/// <summary>
/// Registro en vivo en memoria (spec 010, RF-011 a RF-015): un anillo con los últimos eventos de la plataforma. Solo guarda lo que ya
/// escriben los componentes (nombres, modos, resultados y tiempos); los componentes no registran datos personales ni cuerpos.
/// </summary>
public sealed class InMemoryLogStore
{
    private readonly object _gate = new();
    private readonly LinkedList<LogEntryDto> _entries = new();
    private readonly int _capacity;
    private long _seq;

    public InMemoryLogStore(int capacity = 1000) => _capacity = Math.Max(50, capacity);

    /// <summary>Agrega una entrada y descarta las más antiguas por encima del tope.</summary>
    public void Add(DateTimeOffset time, string level, string source, string message, string? conversationId)
    {
        lock (_gate)
        {
            _entries.AddLast(new LogEntryDto(++_seq, time, level, source, message.Length > 600 ? message[..600] + "…" : message, conversationId));
            while (_entries.Count > _capacity) _entries.RemoveFirst();
        }
    }

    /// <summary>Entradas con número de secuencia mayor que <paramref name="after"/>, de la más antigua a la más nueva.</summary>
    public LogsDto Read(long after = 0, int max = 400)
    {
        lock (_gate)
        {
            var list = _entries.Where(e => e.Seq > after).TakeLast(Math.Max(1, max)).ToList();
            return new LogsDto(list, _seq);
        }
    }

    /// <summary>Vacía el registro (la secuencia sigue creciendo para que los clientes no repitan entradas).</summary>
    public void Clear()
    {
        lock (_gate) _entries.Clear();
    }

    /// <summary>Origen legible según la categoría del registro.</summary>
    public static string SourceOf(string category) => category switch
    {
        _ when category.Contains(".Camila.", StringComparison.Ordinal) => category.EndsWith("CamilaWebhookService", StringComparison.Ordinal) ? "Webhook" : "Camila",
        _ when category.Contains(".Llm.", StringComparison.Ordinal) || category.Contains(".Anthropic.", StringComparison.Ordinal) => "Modelo",
        _ when category.Contains(".Agents.", StringComparison.Ordinal) || category.Contains("Orchestration", StringComparison.Ordinal) || category.Contains(".Tasks.", StringComparison.Ordinal) => "Agente",
        _ when category.Contains("Middleware", StringComparison.Ordinal) || category.Contains(".Auth.", StringComparison.Ordinal) => "Seguridad",
        _ when category.Contains(".Browser", StringComparison.Ordinal) => "Conexión",
        _ => "Sistema"
    };
}

/// <summary>Proveedor de <see cref="ILogger"/> que alimenta el <see cref="InMemoryLogStore"/> con los registros de la plataforma.</summary>
public sealed class InMemoryLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly InMemoryLogStore _store;
    private IExternalScopeProvider? _scopes;

    public InMemoryLoggerProvider(InMemoryLogStore store) => _store = store;

    public ILogger CreateLogger(string categoryName) =>
        categoryName.StartsWith("ATLAS.", StringComparison.Ordinal) ? new Logger(categoryName, _store, () => _scopes) : Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopes = scopeProvider;

    public void Dispose() { }

    private sealed class Logger : ILogger
    {
        private readonly string _category;
        private readonly InMemoryLogStore _store;
        private readonly Func<IExternalScopeProvider?> _scopes;

        public Logger(string category, InMemoryLogStore store, Func<IExternalScopeProvider?> scopes)
        {
            _category = category;
            _store = store;
            _scopes = scopes;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            string? conversation = null;
            _scopes()?.ForEachScope((scope, _) =>
            {
                if (scope is IEnumerable<KeyValuePair<string, object>> pairs)
                    foreach (var (key, value) in pairs)
                        if (key == "ConversationId") conversation = value?.ToString();
            }, 0);

            var message = formatter(state, exception);
            if (exception is not null) message += $" [{exception.GetType().Name}]";
            _store.Add(DateTimeOffset.Now, logLevel switch { LogLevel.Warning => "Aviso", LogLevel.Error or LogLevel.Critical => "Error", _ => "Info" },
                InMemoryLogStore.SourceOf(_category), message, conversation is { Length: > 8 } c ? c[..8] : conversation);
        }
    }
}
