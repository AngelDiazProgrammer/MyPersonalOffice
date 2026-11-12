using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AI.Agent.Office.Application.Camila;

/// <summary>
/// Evaluador de las expresiones de Dialogflow CX que usa el bot: condiciones de rutas
/// (<c>$session.params.x = "a" AND ...</c>), funciones <c>$sys.func.*</c>, <c>$if(...)</c> y texto con parámetros incrustados.
/// Los valores son null, string, double, bool, List&lt;object?&gt; o Dictionary&lt;string, object?&gt;.
/// </summary>
public sealed partial class CamilaExpression
{
    private readonly IDictionary<string, object?> _session;
    private readonly IDictionary<string, object?> _page;
    private readonly string _requestText;
    private readonly Func<DateTime> _now;

    private string _src = string.Empty;
    private int _pos;

    /// <summary>Crea un evaluador sobre los parámetros de sesión y de página.</summary>
    public CamilaExpression(IDictionary<string, object?> session, IDictionary<string, object?>? page = null, string requestText = "", Func<DateTime>? now = null)
    {
        _session = session;
        _page = page ?? new Dictionary<string, object?>();
        _requestText = requestText;
        _now = now ?? (() => DateTime.Now);
    }

    #region API pública

    /// <summary>Evalúa una condición; cualquier error de sintaxis cuenta como falsa.</summary>
    public bool Condition(string? condition)
    {
        if (string.IsNullOrWhiteSpace(condition)) return true;
        try { return Truthy(Eval(condition)); }
        catch (Exception ex) when (ex is FormatException or InvalidOperationException or ArgumentException or JsonException or OverflowException) { return false; }
    }

    /// <summary>
    /// Valor de una asignación: una expresión pura (función, <c>$if</c> o una referencia) conserva su tipo; cualquier otro texto se
    /// interpola. Los valores que no son texto pasan tal cual.
    /// </summary>
    public object? Value(object? raw)
    {
        raw = Normalize(raw);
        if (raw is not string text) return raw;
        var trimmed = text.Trim();
        if (trimmed.StartsWith("$sys.func.", StringComparison.Ordinal) || trimmed.StartsWith("$if(", StringComparison.Ordinal) || SingleRef().IsMatch(trimmed))
        {
            try { return Eval(trimmed); }
            catch (Exception ex) when (ex is FormatException or InvalidOperationException or ArgumentException or JsonException or OverflowException) { return text; }
        }
        return Interpolate(text);
    }

    /// <summary>Reemplaza los <c>$session.params.x</c> de un texto por su valor.</summary>
    public string Interpolate(string text)
    {
        if (text.IndexOf('$') < 0) return text;
        text = text.Replace("$request.text", _requestText, StringComparison.Ordinal);
        return Ref().Replace(text, m => ToText(Lookup(m.Groups["scope"].Value, m.Groups["path"].Value)));
    }

    /// <summary>Convierte un JsonElement (o un valor ya normal) a los tipos del evaluador.</summary>
    public static object? Normalize(object? value) => value switch
    {
        null => null,
        JsonElement e => FromJson(e),
        int i => (double)i,
        long l => (double)l,
        decimal d => (double)d,
        float f => (double)f,
        _ => value
    };

    /// <summary>Convierte un JsonElement a null, string, double, bool, lista o diccionario.</summary>
    public static object? FromJson(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        JsonValueKind.String => e.GetString(),
        JsonValueKind.Number => e.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Array => e.EnumerateArray().Select(FromJson).ToList(),
        _ => e.EnumerateObject().ToDictionary(p => p.Name, p => FromJson(p.Value))
    };

    /// <summary>Texto de un valor: los números enteros sin decimales y los objetos como JSON.</summary>
    public static string ToText(object? v) => v switch
    {
        null => string.Empty,
        string s => s,
        bool b => b ? "true" : "false",
        double d => d.ToString("0.############", CultureInfo.InvariantCulture),
        _ => JsonSerializer.Serialize(v)
    };

    #endregion

    #region Referencias

    [GeneratedRegex(@"^\$(session|page)\.params\.[A-Za-z0-9_.\[\]]+$")]
    private static partial Regex SingleRef();

    [GeneratedRegex(@"\$(?<scope>session|page)\.params\.(?<path>[A-Za-z0-9_]+(?:\.[A-Za-z_][A-Za-z0-9_]*)*)")]
    private static partial Regex Ref();

    private object? Lookup(string scope, string path)
    {
        var bag = scope == "page" ? _page : _session;
        var parts = path.Split('.');
        if (!bag.TryGetValue(parts[0], out var current)) return null;
        for (var i = 1; i < parts.Length; i++)
        {
            if (current is Dictionary<string, object?> d && d.TryGetValue(parts[i], out var next)) current = next;
            else return null;
        }
        return current;
    }

    #endregion

    #region Análisis (descenso recursivo)

    private object? Eval(string source)
    {
        _src = source;
        _pos = 0;
        var result = Or();
        Skip();
        if (_pos < _src.Length) throw new FormatException($"Sobra texto en la expresión: {_src[_pos..]}");
        return result;
    }

    private void Skip() { while (_pos < _src.Length && char.IsWhiteSpace(_src[_pos])) _pos++; }

    private bool Peek(string token)
    {
        Skip();
        return string.Compare(_src, _pos, token, 0, token.Length, StringComparison.Ordinal) == 0;
    }

    /// <summary>Consume una palabra clave (AND, OR, NOT, CONTAINS) solo si va seguida de un límite de palabra.</summary>
    private bool Keyword(string word)
    {
        Skip();
        if (string.Compare(_src, _pos, word, 0, word.Length, StringComparison.Ordinal) != 0) return false;
        var end = _pos + word.Length;
        if (end < _src.Length && (char.IsLetterOrDigit(_src[end]) || _src[end] == '_')) return false;
        _pos = end;
        return true;
    }

    private object? Or()
    {
        var left = And();
        while (Keyword("OR")) { var right = And(); left = Truthy(left) || Truthy(right); }
        return left;
    }

    private object? And()
    {
        var left = Not();
        while (Keyword("AND")) { var right = Not(); left = Truthy(left) && Truthy(right); }
        return left;
    }

    private object? Not() => Keyword("NOT") ? !Truthy(Not()) : Compare();

    private object? Compare()
    {
        var left = Primary();
        Skip();
        foreach (var op in new[] { "!=", "<=", ">=", "=", "<", ">" })
        {
            if (!Peek(op)) continue;
            _pos += op.Length;
            return Compare(left, Primary(), op);
        }
        if (Keyword("CONTAINS"))
        {
            var right = Primary();
            return left is List<object?> list
                ? list.Any(x => Equal(x, right))
                : ToText(left).Contains(ToText(right), StringComparison.OrdinalIgnoreCase);
        }
        return left;
    }

    private object? Primary()
    {
        Skip();
        if (_pos >= _src.Length) throw new FormatException("Expresión incompleta.");
        var c = _src[_pos];

        if (c == '(')
        {
            _pos++;
            var inner = Or();
            Skip();
            Expect(')');
            return inner;
        }
        if (c == '"' || c == '\'') return Interpolate(ReadString(c));
        if (char.IsDigit(c) || (c == '-' && _pos + 1 < _src.Length && char.IsDigit(_src[_pos + 1]))) return ReadNumber();

        if (c == '$')
        {
            if (Peek("$sys.func.")) { _pos += "$sys.func.".Length; return Function(ReadWord()); }
            if (Peek("$if(")) { _pos += 3; return Function("IF"); }
            if (Peek("$request.text")) { _pos += "$request.text".Length; return _requestText; }
            var m = Ref().Match(_src, _pos);
            if (!m.Success || m.Index != _pos) throw new FormatException($"Referencia no válida en {_pos}.");
            _pos += m.Length;
            return Lookup(m.Groups["scope"].Value, m.Groups["path"].Value);
        }

        var word = ReadWord();
        return word switch
        {
            "true" => true,
            "false" => false,
            "null" => null,
            _ => word            // una palabra suelta (por ejemplo CIERRE) se compara como texto
        };
    }

    private string ReadWord()
    {
        Skip();
        var start = _pos;
        while (_pos < _src.Length && (char.IsLetterOrDigit(_src[_pos]) || _src[_pos] is '_' or '-' or '.')) _pos++;
        if (_pos == start) throw new FormatException($"Se esperaba una palabra en {_pos}.");
        return _src[start.._pos];
    }

    private string ReadString(char quote)
    {
        _pos++;
        var sb = new StringBuilder();
        while (_pos < _src.Length && _src[_pos] != quote)
        {
            if (_src[_pos] == '\\' && _pos + 1 < _src.Length) _pos++;
            sb.Append(_src[_pos++]);
        }
        if (_pos >= _src.Length) throw new FormatException("Texto sin cerrar.");
        _pos++;
        return sb.ToString();
    }

    private double ReadNumber()
    {
        var start = _pos;
        if (_src[_pos] == '-') _pos++;
        while (_pos < _src.Length && (char.IsDigit(_src[_pos]) || _src[_pos] == '.')) _pos++;
        return double.Parse(_src[start.._pos], CultureInfo.InvariantCulture);
    }

    private void Expect(char c)
    {
        Skip();
        if (_pos >= _src.Length || _src[_pos] != c) throw new FormatException($"Se esperaba '{c}' en {_pos}.");
        _pos++;
    }

    #endregion

    #region Funciones

    private object? Function(string name)
    {
        Skip();
        Expect('(');
        var args = new List<object?>();
        Skip();
        if (!Peek(")"))
        {
            do { args.Add(Or()); Skip(); } while (TryComma());
        }
        Expect(')');

        object? Arg(int i) => i < args.Count ? args[i] : null;

        switch (name.ToUpperInvariant())
        {
            case "IF": return Truthy(Arg(0)) ? Arg(1) : Arg(2);
            case "LOWER": return ToText(Arg(0)).ToLowerInvariant();
            case "UPPER": return ToText(Arg(0)).ToUpperInvariant();
            case "TO_TEXT": return ToText(Arg(0));
            case "TO_NUMBER": return ToNumber(Arg(0));
            case "ADD": return (ToNumber(Arg(0)) ?? 0) + (ToNumber(Arg(1)) ?? 0);
            case "COUNT": return Arg(0) is List<object?> l ? (double)l.Count : 0d;
            case "NOW": return _now();
            case "TO_OBJECT":
                var text = ToText(Arg(0));
                return string.IsNullOrWhiteSpace(text) ? null : FromJson(JsonDocument.Parse(text).RootElement);
            case "GET_FIELD":
                object? current = Arg(0);
                foreach (var part in ToText(Arg(1)).Split('.', StringSplitOptions.RemoveEmptyEntries))
                    current = current is Dictionary<string, object?> d && d.TryGetValue(part, out var next) ? next : null;
                return current;
            case "ADD_DATE": return AddDate(AsDate(Arg(0)), ToNumber(Arg(1)) ?? 0, ToText(Arg(2)));
            case "FORMAT_DATE": return FormatDate(AsDate(Arg(0)), ToText(Arg(1)));
            default: throw new FormatException($"Función no soportada: {name}.");
        }
    }

    private bool TryComma()
    {
        Skip();
        if (_pos < _src.Length && _src[_pos] == ',') { _pos++; return true; }
        return false;
    }

    private static DateTime AsDate(object? v) => v switch
    {
        DateTime d => d,
        string s when DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var d) => d,
        string s when DateTime.TryParseExact(s, new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) => d,
        _ => throw new FormatException("Fecha no válida.")
    };

    private static DateTime AddDate(DateTime d, double n, string unit) => unit.ToUpperInvariant() switch
    {
        "DAYS" => d.AddDays(n),
        "MONTHS" => d.AddMonths((int)n),
        "YEARS" => d.AddYears((int)n),
        "HOURS" => d.AddHours(n),
        "MINUTES" => d.AddMinutes(n),
        _ => throw new FormatException($"Unidad de fecha no soportada: {unit}.")
    };

    /// <summary>Formato de fecha estilo Java/Dialogflow (yyyy-MM-dd, dd/MM/yyyy) en cultura invariante.</summary>
    private static string FormatDate(DateTime d, string format) => d.ToString(format.Trim('"'), CultureInfo.InvariantCulture);

    private static double? ToNumber(object? v) => v switch
    {
        double d => d,
        bool b => b ? 1 : 0,
        string s when double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) => d,
        _ => null
    };

    #endregion

    #region Comparaciones

    private static bool Truthy(object? v) => v switch
    {
        null => false,
        bool b => b,
        string s => s.Length > 0 && !s.Equals("false", StringComparison.OrdinalIgnoreCase),
        double d => d != 0,
        _ => true
    };

    private static bool Equal(object? a, object? b)
    {
        if (a is null || b is null) return a is null && b is null || (a is string sa && sa.Length == 0 && b is null) || (b is string sb && sb.Length == 0 && a is null);
        if (ToNumber(a) is { } x && ToNumber(b) is { } y && a is not bool && b is not bool && (a is double || b is double) && IsNumeric(a) && IsNumeric(b)) return x == y;
        return string.Equals(ToText(a), ToText(b), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNumeric(object? v) => v is double || (v is string s && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out _));

    private static bool Compare(object? a, object? b, string op)
    {
        switch (op)
        {
            case "=": return Equal(a, b);
            case "!=": return !Equal(a, b);
        }
        int cmp;
        if (a is null || b is null) return false;
        if (a is DateTime || b is DateTime || (TryDate(a, out _) && TryDate(b, out _) && !IsNumeric(a)))
        {
            if (!TryDate(a, out var da) || !TryDate(b, out var db)) return false;
            cmp = da.CompareTo(db);
        }
        else if (ToNumber(a) is { } x && ToNumber(b) is { } y) cmp = x.CompareTo(y);
        else cmp = string.Compare(ToText(a), ToText(b), StringComparison.OrdinalIgnoreCase);

        return op switch { "<" => cmp < 0, ">" => cmp > 0, "<=" => cmp <= 0, ">=" => cmp >= 0, _ => false };
    }

    private static bool TryDate(object? v, out DateTime d)
    {
        d = default;
        if (v is DateTime dt) { d = dt; return true; }
        return v is string s && DateTime.TryParseExact(s, new[] { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy-MM-ddTHH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out d);
    }

    #endregion
}
