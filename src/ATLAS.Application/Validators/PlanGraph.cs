namespace ATLAS.Application.Validators;

/// <summary>Utilidades sobre el grafo de dependencias de un plan.</summary>
public static class PlanGraph
{
    /// <summary>
    /// Orden topológico (algoritmo de Kahn): las dependencias primero. Devuelve null si una dependencia no
    /// existe, una clave se repite o hay un ciclo.
    /// </summary>
    public static IReadOnlyList<T>? TopologicalOrder<T>(
        IReadOnlyList<T> items,
        Func<T, string> key,
        Func<T, IEnumerable<string>> dependencies)
    {
        var byKey = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (!byKey.TryAdd(key(item), item)) return null;
        }

        var remaining = items.ToDictionary(i => key(i), i => dependencies(i).Distinct(StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
        foreach (var deps in remaining.Values)
        {
            if (deps.Any(d => !byKey.ContainsKey(d))) return null;
        }

        var ordered = new List<T>(items.Count);
        while (remaining.Count > 0)
        {
            var ready = remaining.Where(kv => kv.Value.Count == 0).Select(kv => kv.Key).ToList();
            if (ready.Count == 0) return null; // cycle

            foreach (var k in ready)
            {
                ordered.Add(byKey[k]);
                remaining.Remove(k);
            }
            foreach (var deps in remaining.Values)
                foreach (var k in ready) deps.Remove(k);
        }

        return ordered;
    }
}
