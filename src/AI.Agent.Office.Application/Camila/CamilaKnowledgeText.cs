using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace AI.Agent.Office.Application.Camila;

/// <summary>
/// La base de conocimiento de Camila como texto para un modelo (spec 009, ajuste 2026-11-12): catálogo corto de flujos y webhooks que va
/// siempre en el contexto, y búsqueda de detalle bajo demanda. El agente conversacional lo usa como <b>conocimiento</b>, no como un
/// recorrido que deba seguir.
/// </summary>
public static partial class CamilaKnowledgeText
{
    private static readonly ConditionalWeakTable<CamilaBot, Cache> Caches = new();

    private sealed class Cache
    {
        public CamilaIndex Index = null!;
        public string Catalog = string.Empty;
        public Dictionary<string, List<string>> Producers = new(StringComparer.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"\$session\.params\.(?<n>[A-Za-z0-9_]+)")]
    private static partial Regex ParamRef();

    private static Cache For(CamilaBot bot) => Caches.GetValue(bot, b =>
    {
        var cache = new Cache { Index = new CamilaIndex(b) };
        foreach (var w in b.Webhooks)
            foreach (var p in Produces(w))
            {
                if (!cache.Producers.TryGetValue(p, out var list)) cache.Producers[p] = list = new List<string>();
                list.Add(w.Name);
            }
        cache.Catalog = BuildCatalog(b, cache);
        return cache;
    });

    #region Webhooks

    /// <summary>Variables de sesión que el webhook necesita (cuerpo, dirección y cabeceras que no son secretos).</summary>
    public static IReadOnlyList<string> Needs(CamilaWebhook w)
    {
        var sources = new List<string> { w.Uri, w.Body ?? string.Empty };
        sources.AddRange(w.Headers.Values.Where(v => !v.Contains("{{secret:", StringComparison.Ordinal)));
        return sources.SelectMany(s => ParamRef().Matches(s).Select(m => m.Groups["n"].Value)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>Variables de sesión que el webhook deja al responder.</summary>
    public static IReadOnlyList<string> Produces(CamilaWebhook w) =>
        w.Mapping.Select(m => m.Parameter.Replace("$session.params.", string.Empty)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>Línea del catálogo de un webhook: tipo, qué necesita, qué devuelve y de dónde sacar lo que falta.</summary>
    public static string WebhookLine(CamilaBot bot, CamilaWebhook w) => WebhookLine(w, For(bot));

    // Recibe la caché ya creada: se usa al construirla (llamar a For desde aquí volvería a entrar en su propio constructor).
    private static string WebhookLine(CamilaWebhook w, Cache cache)
    {
        var needs = Needs(w);
        var produces = Produces(w);
        var sb = new StringBuilder($"- «{w.Name}» ({(w.Kind == "write" ? "ESCRIBE datos reales: la plataforma pide confirmación" : "lectura")})");
        if (needs.Count > 0)
        {
            sb.Append("; necesita: ");
            sb.Append(string.Join(", ", needs.Select(n =>
                cache.Producers.TryGetValue(n, out var from) && from.Any(f => !string.Equals(f, w.Name, StringComparison.OrdinalIgnoreCase))
                    ? $"{n} (se obtiene con «{from.First(f => !string.Equals(f, w.Name, StringComparison.OrdinalIgnoreCase))}»)"
                    : $"{n} (lo da la persona)")));
        }
        if (produces.Count > 0) sb.Append("; devuelve: " + string.Join(", ", produces.Take(12)) + (produces.Count > 12 ? $" (+{produces.Count - 12})" : string.Empty));
        return sb.ToString();
    }

    #endregion

    #region Catálogo y búsqueda

    /// <summary>Catálogo que va siempre en el contexto: flujos (nombre y de qué tratan) y webhooks (qué hacen y qué necesitan).</summary>
    public static string Catalog(CamilaBot bot) => For(bot).Catalog;

    private static string BuildCatalog(CamilaBot bot, Cache cache)
    {
        var sb = new StringBuilder("## Temas que cubre la base (flujos del bot)\n");
        foreach (var f in bot.Flows)
        {
            var intro = f.Pages.SelectMany(p => p.Entry.Messages.Where(m => m.Type == "text").Select(m => m.Texts[0])).FirstOrDefault(t => t.Length > 15);
            sb.Append($"- {f.Name}{(intro is null ? string.Empty : ": " + Cut(intro, 90))}\n");
        }
        sb.Append("\n## Servicios (webhooks) que puedes llamar\n");
        foreach (var w in bot.Webhooks.Where(w => !w.Name.StartsWith("IA Generativa", StringComparison.OrdinalIgnoreCase))) sb.Append(WebhookLine(w, cache)).Append('\n');
        return sb.ToString();
    }

    /// <summary>
    /// Detalle de lo que la base sabe sobre una consulta: los flujos más parecidos (qué dice el bot, qué datos pide, qué servicios usa y los
    /// botones/rutas) y los servicios relacionados.
    /// </summary>
    public static string Search(CamilaBot bot, string query, int maxChars = 6000, IReadOnlyDictionary<string, string>? narrative = null)
    {
        var cache = For(bot);
        var sb = new StringBuilder();
        var flows = cache.Index.Search(query, 2);
        foreach (var f in flows)
        {
            if (sb.Length > maxChars) break;
            // Con la narrativa de la base de conocimiento se entrega la sección del flujo; sin ella, el resumen construido del playbook.
            if (narrative is not null && narrative.TryGetValue(f.Name, out var section) && section.Length > 0)
                sb.Append(section.Length > 3200 ? section[..3200] + "…" : section).Append('\n');
            else sb.Append(FlowText(bot, f, 2800)).Append('\n');
        }

        var words = CamilaEngine.Tokens(query).Where(t => t.Length > 2).Select(t => t.Length > 5 ? t[..5] : t).ToHashSet();
        var hooks = bot.Webhooks.Where(w => words.Overlaps(CamilaEngine.Tokens(w.Name + " " + string.Join(" ", Produces(w))).Select(t => t.Length > 5 ? t[..5] : t))).Take(5).ToList();
        if (hooks.Count > 0)
        {
            sb.Append("## Servicios relacionados\n");
            foreach (var w in hooks) sb.Append(WebhookLine(bot, w)).Append('\n');
        }

        return sb.Length == 0 ? "No encontré nada en la base sobre eso." : (sb.Length > maxChars ? sb.ToString()[..maxChars] + "…" : sb.ToString());
    }

    /// <summary>Un flujo contado como conocimiento: qué dice, qué pide, qué consulta o escribe y qué opciones ofrece.</summary>
    public static string FlowText(CamilaBot bot, CamilaFlow flow, int budget)
    {
        var sb = new StringBuilder($"### Tema: {flow.Name}\n");
        foreach (var p in flow.Pages)
        {
            if (sb.Length > budget) { sb.Append("… (hay más páginas)\n"); break; }
            var line = new StringBuilder($"- {p.Name}");
            var says = p.Entry.Messages.Where(m => m.Type == "text").Select(m => Cut(m.Texts[0], 220)).ToList();
            foreach (var s in says.Take(2)) line.Append($" · dice «{s}»");
            var buttons = p.Entry.Messages.Where(m => m.Type == "buttons").SelectMany(m => m.Buttons).Select(b => b.Text).ToList();
            if (buttons.Count > 0) line.Append(" · opciones: " + string.Join(", ", buttons.Take(8)));
            var asks = p.Form.Where(f => f.Required).Select(f =>
                f.Prompt.Messages.FirstOrDefault(m => m.Type == "text")?.Texts.FirstOrDefault() is { } q ? $"{f.Name} (pregunta «{Cut(q, 140)}»)" : f.Name).ToList();
            if (asks.Count > 0) line.Append(" · pide: " + string.Join(", ", asks));
            var hooks = new[] { p.Entry }.Concat(p.Routes.Select(r => r.Fulfillment)).Concat(p.Events.Select(r => r.Fulfillment)).Where(f => f.Webhook is not null)
                .Select(f => f.Webhook!).Distinct().ToList();
            if (hooks.Count > 0) line.Append(" · usa: " + string.Join(", ", hooks.Select(h => $"«{h}»")));
            if (line.Length > 12) sb.Append(line).Append('\n');
        }
        return sb.ToString();
    }

    #endregion

    private static string Cut(string text, int max)
    {
        var flat = text.Replace("\r", " ").Replace("\n", " ").Trim();
        return flat.Length <= max ? flat : flat[..max] + "…";
    }

    /// <summary>Índice por palabras de los flujos (lo comparte el agente conversacional).</summary>
    public static CamilaIndex IndexOf(CamilaBot bot) => For(bot).Index;
}
