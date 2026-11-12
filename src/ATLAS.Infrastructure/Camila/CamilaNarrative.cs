using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ATLAS.Application.Camila;

namespace ATLAS.Infrastructure.Camila;

/// <summary>
/// Texto legible de <c>CamilaIA.md</c> (spec 009, ajustado el 2026-11-11): para cada flujo y página explica, en el orden en que ocurre,
/// qué asigna, qué webhook llama (lectura o escritura), qué dice, qué dato pide al usuario y adónde puede ir. Se genera solo desde el
/// modelo del bot: no inventa nada que no esté en el export.
/// </summary>
public static partial class CamilaNarrative
{
    private const string Read = "⚡";
    private const string Write = "✍️";

    [GeneratedRegex(@"\$session\.params\.(?<n>[A-Za-z0-9_]+)")]
    private static partial Regex ParamRef();

    [GeneratedRegex(@"(?<!"")\$session\.params\.(?<n>[A-Za-z0-9_]+)(?!"")")]
    private static partial Regex UnquotedParam();

    #region Índices del bot

    /// <summary>Dónde se usa cada webhook y desde dónde se llega a cada flujo; se calcula una vez por documento.</summary>
    public sealed class Index
    {
        public Dictionary<string, SortedSet<string>> WebhookUses { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, SortedSet<string>> FlowEntries { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, SortedSet<string>> GeneratorUses { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<(string Flow, string Page, CamilaFormParam Param)> RequiredInputs { get; } = new();
    }

    /// <summary>Recorre todo el bot una vez.</summary>
    public static Index Build(CamilaBot bot)
    {
        var index = new Index();

        void Use(CamilaFulfillment f, string where)
        {
            if (f.Webhook is not null) Add(index.WebhookUses, f.Webhook, where);
            foreach (var g in f.Generators) Add(index.GeneratorUses, bot.FindGenerator(g.Generator)?.Name ?? g.Generator, where);
        }

        void Route(CamilaRoute r, string flow, string? page, string how)
        {
            var origin = page is null ? $"flujo {flow}" : $"{flow} › {page}";
            Use(r.Fulfillment, $"{origin} ({how})");
            if (r.TargetFlow is not null && !string.Equals(r.TargetFlow, flow, StringComparison.OrdinalIgnoreCase))
                Add(index.FlowEntries, r.TargetFlow, $"{origin} ← {Trigger(r)}");
        }

        foreach (var flow in bot.Flows)
        {
            foreach (var r in flow.Routes) Route(r, flow.Name, null, "ruta " + Trigger(r));
            foreach (var r in flow.Events) Route(r, flow.Name, null, "evento " + Trigger(r));
            foreach (var page in flow.Pages)
            {
                Use(page.Entry, $"{flow.Name} › {page.Name} (al entrar)");
                foreach (var p in page.Form)
                {
                    Use(p.Prompt, $"{flow.Name} › {page.Name} (al pedir {p.Name})");
                    if (p.Required) index.RequiredInputs.Add((flow.Name, page.Name, p));
                }
                foreach (var r in page.Routes) Route(r, flow.Name, page.Name, "ruta " + Trigger(r));
                foreach (var r in page.Events) Route(r, flow.Name, page.Name, "evento " + Trigger(r));
            }
        }
        foreach (var g in bot.Groups)
            foreach (var r in g.Routes) Route(r, g.Flow ?? "(global)", null, $"grupo {g.Name}");

        return index;
    }

    private static void Add(Dictionary<string, SortedSet<string>> map, string key, string value)
    {
        if (!map.TryGetValue(key, out var set)) map[key] = set = new SortedSet<string>(StringComparer.Ordinal);
        set.Add(value);
    }

    #endregion

    #region Resumen general

    /// <summary>Cabecera: cómo leer el documento, mapa de flujos, webhooks de escritura, datos que se piden y generadores.</summary>
    public static string Overview(CamilaBot bot, Index index)
    {
        var sb = new StringBuilder();

        sb.Append("## Cómo leer este documento\n\n");
        sb.Append("Cada **página** es un paso del bot. Al llegar a una página ocurre, **siempre en este orden**:\n\n");
        sb.Append("1. 📌 **Asigna variables** (parámetros de sesión).\n");
        sb.Append($"2. {Read}/{Write} **Llama a un webhook** (si la página lo tiene).\n");
        sb.Append("3. 🤖 **Ejecuta generadores de IA** (si los tiene).\n");
        sb.Append("4. 💬 **Dice** sus mensajes y 🔘 **ofrece botones**.\n");
        sb.Append("5. ⌨️ **Pide al usuario** los datos obligatorios de su formulario que aún no tenga (y espera su respuesta).\n");
        sb.Append("6. 🔀 **Evalúa las rutas automáticas** (solo condición): si una se cumple, sigue sin esperar al usuario.\n");
        sb.Append("7. ⏳ **Espera al usuario**: un botón, una frase que coincide con un intent, o nada que coincida (*sin coincidencia*).\n\n");
        sb.Append($"- {Read} **webhook de lectura**: consulta datos; no cambia nada. {Write} **webhook de escritura**: guarda, envía o modifica datos reales; la plataforma **pide confirmación** antes de ejecutarlo.\n");
        sb.Append("- Un destino «END_SESSION» termina la conversación; «END_FLOW» vuelve al flujo inicial.\n");
        sb.Append("- Las rutas se prueban en este orden: página → grupos de rutas de la página → flujo → grupos del flujo.\n");
        sb.Append("- Debajo de cada elemento hay un bloque `camila:*` con su definición exacta; **es lo que lee la plataforma**. El texto de arriba se genera a partir de él.\n\n");

        sb.Append("## Resumen\n\n");
        var pages = bot.Flows.Sum(f => f.Pages.Count);
        sb.Append($"- Flujos: {bot.Flows.Count} · Páginas: {pages} · Intents: {bot.Intents.Count} · Webhooks: {bot.Webhooks.Count} ({bot.Webhooks.Count(w => w.Kind == "write")} de escritura) · Generadores de IA: {bot.Generators.Count} · Entidades: {bot.Entities.Count}\n");
        sb.Append($"- Flujo inicial: {bot.StartFlow}\n\n");

        sb.Append("## Mapa de flujos\n\n");
        sb.Append("| Flujo | Páginas | Pide datos | Lee | Escribe | IA | Se llega desde |\n|---|---:|---|---|---|---|---|\n");
        foreach (var f in bot.Flows)
        {
            var hooks = HooksOf(f, bot);
            var asks = f.Pages.SelectMany(p => p.Form.Where(x => x.Required)).Select(x => x.Name).Distinct().Take(4).ToList();
            var from = index.FlowEntries.TryGetValue(f.Name, out var set) ? set.Count.ToString() : "0";
            sb.Append($"| {f.Name} | {f.Pages.Count} | {(asks.Count == 0 ? "—" : string.Join(", ", asks.Select(a => "`" + a + "`")))} | {JoinOrDash(hooks.Where(h => h.Kind != "write").Select(h => h.Name))} | {JoinOrDash(hooks.Where(h => h.Kind == "write").Select(h => h.Name))} | {(f.Pages.Any(p => AllFulfillments(p).Any(x => x.Generators.Count > 0)) ? "🤖" : "—")} | {from} ruta(s) |\n");
        }
        sb.Append('\n');

        sb.Append($"## {Write} Webhooks de escritura (piden confirmación)\n\n");
        var writes = bot.Webhooks.Where(w => w.Kind == "write").ToList();
        if (writes.Count == 0) sb.Append("Ninguno.\n\n");
        foreach (var w in writes)
        {
            sb.Append($"- **{w.Name}** — {w.Method} `{w.Host}` · se usa en: {JoinUses(index, w.Name)}\n");
        }
        if (writes.Count > 0) sb.Append('\n');

        sb.Append("## ⌨️ Datos que el bot pide al usuario\n\n");
        if (index.RequiredInputs.Count == 0) sb.Append("Ninguno.\n\n");
        foreach (var (flow, page, p) in index.RequiredInputs.OrderBy(x => x.Flow, StringComparer.Ordinal).ThenBy(x => x.Page, StringComparer.Ordinal))
        {
            var ask = p.Prompt.Messages.FirstOrDefault(m => m.Type == "text")?.Texts.FirstOrDefault();
            sb.Append($"- `{p.Name}` ({p.Entity}) — {flow} › {page}{(ask is null ? string.Empty : $": «{Cut(ask, 110)}»")}\n");
        }
        sb.Append('\n');

        sb.Append("## 🤖 Generadores de IA\n\n");
        foreach (var g in bot.Generators)
            sb.Append($"- **{g.Name}** — se usa en: {(index.GeneratorUses.TryGetValue(g.Name, out var uses) ? string.Join("; ", uses.Take(4)) : "(no se usa)")}\n");
        sb.Append('\n');

        sb.Append("## Índice por flujo\n\n");
        foreach (var f in bot.Flows)
            sb.Append($"- **{f.Name}** — {f.Pages.Count} página(s): {string.Join(" · ", f.Pages.Select(p => p.Name))}\n");
        sb.Append('\n');
        return sb.ToString();
    }

    private static string JoinOrDash(IEnumerable<string> values)
    {
        var list = values.Distinct().ToList();
        return list.Count == 0 ? "—" : string.Join(", ", list);
    }

    private static string JoinUses(Index index, string webhook) =>
        index.WebhookUses.TryGetValue(webhook, out var uses) ? string.Join("; ", uses.Take(5)) + (uses.Count > 5 ? $"; … ({uses.Count} usos)" : string.Empty) : "(sin usos)";

    private static IEnumerable<CamilaFulfillment> AllFulfillments(CamilaPage p) =>
        new[] { p.Entry }.Concat(p.Form.Select(f => f.Prompt)).Concat(p.Routes.Select(r => r.Fulfillment)).Concat(p.Events.Select(r => r.Fulfillment));

    private static List<CamilaWebhook> HooksOf(CamilaFlow flow, CamilaBot bot) =>
        flow.Pages.SelectMany(AllFulfillments).Concat(flow.Routes.Select(r => r.Fulfillment)).Concat(flow.Events.Select(r => r.Fulfillment))
            .Where(f => f.Webhook is not null).Select(f => bot.FindWebhook(f.Webhook)).Where(w => w is not null).Select(w => w!)
            .GroupBy(w => w.Name).Select(g => g.First()).OrderBy(w => w.Name, StringComparer.Ordinal).ToList();

    #endregion

    #region Flujo

    /// <summary>Qué es el flujo, desde dónde se llega, qué datos usa y qué páginas tiene.</summary>
    public static string Flow(CamilaBot bot, Index index, CamilaFlow f)
    {
        var sb = new StringBuilder();
        var intro = f.Pages.SelectMany(p => p.Entry.Messages.Where(m => m.Type == "text").Select(m => m.Texts[0])).FirstOrDefault(t => t.Length > 12);
        if (intro is not null) sb.Append($"**Qué dice al empezar:** «{Cut(intro, 160)}»\n\n");

        if (index.FlowEntries.TryGetValue(f.Name, out var from))
            sb.Append("**Se llega desde:**\n" + string.Join("\n", from.Take(8).Select(x => "- " + x)) + (from.Count > 8 ? $"\n- … y {from.Count - 8} más" : string.Empty) + "\n\n");

        var hooks = HooksOf(f, bot);
        if (hooks.Count > 0) sb.Append("**Webhooks que usa:** " + string.Join(" · ", hooks.Select(h => $"{Icon(h)} {h.Name}")) + "\n\n");

        var asks = f.Pages.SelectMany(p => p.Form.Where(x => x.Required).Select(x => $"`{x.Name}` ({p.Name})")).ToList();
        if (asks.Count > 0) sb.Append("**Pide al usuario:** " + string.Join(", ", asks) + "\n\n");

        sb.Append("**Páginas:**\n");
        foreach (var p in f.Pages) sb.Append($"- {p.Name} {PageBadges(bot, p)}\n");
        sb.Append('\n');
        sb.Append(RoutesBlock("Rutas propias del flujo (valen en todas sus páginas)", f.Routes, f.Events, bot, f.Name, null));
        if (f.Groups.Count > 0) sb.Append("**Grupos de rutas compartidas:** " + string.Join(", ", f.Groups) + "\n\n");
        return sb.ToString();
    }

    private static string PageBadges(CamilaBot bot, CamilaPage p)
    {
        var badges = new List<string>();
        var hooks = AllFulfillments(p).Where(f => f.Webhook is not null).Select(f => bot.FindWebhook(f.Webhook)).Where(w => w is not null).Select(w => w!).GroupBy(w => w.Name).Select(g => g.First());
        badges.AddRange(hooks.Select(h => $"{Icon(h)} {h.Name}"));
        if (p.Form.Any(f => f.Required)) badges.Add("⌨️ pide datos");
        if (AllFulfillments(p).Any(f => f.Generators.Count > 0)) badges.Add("🤖");
        return badges.Count == 0 ? string.Empty : "— " + string.Join(" · ", badges);
    }

    #endregion

    #region Página

    /// <summary>La página explicada en el orden en que ocurre.</summary>
    public static string Page(CamilaBot bot, CamilaFlow flow, CamilaPage p)
    {
        var sb = new StringBuilder();
        var n = 1;

        sb.Append("**Al entrar, en este orden:**\n\n");
        var any = false;
        void Step(string text) { sb.Append($"{n++}. {text}\n"); any = true; }

        if (p.Entry.SetParams.Count > 0) Step("📌 Asigna: " + string.Join(", ", p.Entry.SetParams.Select(SetText)));
        if (p.Entry.Webhook is not null) Step(WebhookStep(bot, p.Entry.Webhook, p.Entry.Tag));
        foreach (var g in p.Entry.Generators) Step(GeneratorStep(bot, g));
        foreach (var m in p.Entry.Messages.Where(m => m.Type != "buttons")) Step(MessageText(m));
        var buttons = p.Entry.Messages.Where(m => m.Type == "buttons").SelectMany(m => m.Buttons).ToList();
        if (buttons.Count > 0) Step("🔘 Ofrece los botones: " + string.Join(" · ", buttons.Select(b => $"«{b.Text}»")));
        if (!any) sb.Append("_No hace nada al entrar._\n");
        sb.Append('\n');

        var required = p.Form.Where(f => f.Required).ToList();
        if (required.Count > 0)
        {
            sb.Append("**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):\n");
            foreach (var f in required)
            {
                var ask = f.Prompt.Messages.FirstOrDefault(m => m.Type == "text")?.Texts.FirstOrDefault();
                sb.Append($"- `{f.Name}` — {f.Entity}{(ask is null ? string.Empty : $" · pregunta: «{Cut(ask, 120)}»")}{(f.Prompt.Webhook is null ? string.Empty : $" · al preguntar llama a {f.Prompt.Webhook}")}\n");
            }
            sb.Append('\n');
        }
        var optional = p.Form.Where(f => !f.Required).Select(f => $"`{f.Name}`").ToList();
        if (optional.Count > 0) sb.Append($"**Variables del formulario (no obligatorias, normalmente las llena un webhook):** {string.Join(", ", optional)}\n\n");

        sb.Append(RoutesBlock("Qué pasa después", p.Routes, p.Events, bot, flow.Name, buttons));
        if (p.Groups.Count > 0) sb.Append("**Grupos de rutas compartidas:** " + string.Join(", ", p.Groups) + "\n\n");
        return sb.ToString();
    }

    private static string RoutesBlock(string title, List<CamilaRoute> routes, List<CamilaRoute> events, CamilaBot bot, string flow, List<CamilaButton>? buttons)
    {
        if (routes.Count == 0 && events.Count == 0) return string.Empty;
        var sb = new StringBuilder($"**{title}:**\n\n");

        var auto = routes.Where(r => r.Intent is null && r.Event is null).ToList();
        if (auto.Count > 0)
        {
            sb.Append("🔀 *Automáticas (sin esperar al usuario, en este orden):*\n");
            foreach (var r in auto) sb.Append($"- {When(r)} → {Destination(r)}{Effects(r.Fulfillment, bot)}\n");
        }

        var byEvent = routes.Concat(events).Where(r => r.Event is not null && r.Event != "sys.no-match-default" && r.Event != "sys.no-input-default").ToList();
        if (byEvent.Count > 0)
        {
            sb.Append("🔘 *Si el usuario pulsa un botón:*\n");
            foreach (var r in byEvent)
            {
                var label = buttons?.FirstOrDefault(b => string.Equals(b.Event, r.Event, StringComparison.OrdinalIgnoreCase))?.Text;
                sb.Append($"- {(label is null ? $"evento `{r.Event}`" : $"«{label}» (`{r.Event}`)")}{CondText(r)} → {Destination(r)}{Effects(r.Fulfillment, bot)}\n");
            }
        }

        var byIntent = routes.Where(r => r.Intent is not null).ToList();
        if (byIntent.Count > 0)
        {
            sb.Append("💬 *Si el usuario escribe algo que coincide con:*\n");
            foreach (var r in byIntent)
            {
                var phrases = bot.FindIntent(r.Intent)?.Phrases.Take(2).ToList();
                sb.Append($"- intent «{r.Intent}»{(phrases is { Count: > 0 } ? $" (ej. «{string.Join("», «", phrases.Select(x => Cut(x, 40)))}»)" : string.Empty)}{CondText(r)} → {Destination(r)}{Effects(r.Fulfillment, bot)}\n");
            }
        }

        var nomatch = routes.Concat(events).Where(r => r.Event is "sys.no-match-default" or "sys.no-input-default").ToList();
        foreach (var r in nomatch)
            sb.Append($"- ❔ *{(r.Event == "sys.no-match-default" ? "Sin coincidencia" : "Sin respuesta")}* → {Destination(r)}{Effects(r.Fulfillment, bot)}\n");

        sb.Append('\n');
        return sb.ToString();
    }

    #endregion

    #region Webhook

    /// <summary>Qué envía, qué devuelve y dónde se usa.</summary>
    public static string Webhook(Index index, CamilaWebhook w)
    {
        var sb = new StringBuilder();
        sb.Append($"- **Tipo:** {(w.Kind == "write" ? $"{Write} **escritura** — modifica datos reales; la plataforma pide confirmación antes de ejecutarlo" : $"{Read} lectura — consulta datos")}\n");
        sb.Append($"- **Llamada:** `{w.Method} {w.Uri}` · formato {(w.Type == "standard" ? "estándar de Dialogflow (recibe etiqueta, página y todas las variables de sesión)" : "flexible (cuerpo propio)")} · tiempo máximo {w.TimeoutSeconds} s\n");

        var secrets = w.Headers.Where(h => h.Value.Contains("{{secret:", StringComparison.Ordinal)).Select(h => h.Key).ToList();
        var dynamic = w.Headers.Where(h => h.Value.Contains("$session.params.", StringComparison.Ordinal)).Select(h => $"{h.Key} ← {h.Value}").ToList();
        if (secrets.Count > 0) sb.Append($"- **Credenciales:** cabecera(s) {string.Join(", ", secrets)} desde `.env` (no están en este archivo)\n");
        if (dynamic.Count > 0) sb.Append($"- **Autorización dinámica:** {string.Join("; ", dynamic)}\n");

        var sent = SentFields(w);
        if (sent.Count > 0) sb.Append("- **Envía:** " + string.Join(", ", sent) + "\n");
        else if (w.Type != "standard") sb.Append("- **Envía:** (sin cuerpo)\n");

        if (w.Mapping.Count > 0)
            sb.Append("- **Devuelve y guarda en la sesión:** " + string.Join(", ", w.Mapping.Take(14).Select(m => $"`{m.Parameter.Replace("$session.params.", string.Empty)}` ← `{m.Path}`")) + (w.Mapping.Count > 14 ? $", … ({w.Mapping.Count} datos)" : string.Empty) + "\n");
        else if (w.Type == "standard") sb.Append("- **Devuelve:** variables de sesión en `sessionInfo.parameters`\n");

        foreach (var n in w.Notes) sb.Append($"- ⚠️ {n}\n");
        sb.Append($"- **Se usa en:** {JoinUses(index, w.Name)}\n");
        return sb.Append('\n').ToString();
    }

    /// <summary>Campos del cuerpo con la variable de sesión de la que salen.</summary>
    private static List<string> SentFields(CamilaWebhook w)
    {
        var list = new List<string>();
        if (string.IsNullOrWhiteSpace(w.Body)) return list;
        try
        {
            // Una variable sin comillas (por ejemplo un número) no es JSON válido: se le ponen para poder leer la estructura.
            var body = UnquotedParam().Replace(w.Body, "\"$$session.params.${n}\"");
            using var doc = JsonDocument.Parse(body, new JsonDocumentOptions { AllowTrailingCommas = true });
            Flatten(doc.RootElement, string.Empty, list);
        }
        catch (JsonException)
        {
            list.AddRange(ParamRef().Matches(w.Body).Select(m => "`" + m.Groups["n"].Value + "`").Distinct());
        }
        return list;
    }

    private static void Flatten(JsonElement e, string path, List<string> into)
    {
        switch (e.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var p in e.EnumerateObject()) Flatten(p.Value, path.Length == 0 ? p.Name : path + "." + p.Name, into);
                break;
            case JsonValueKind.Array:
                var i = 0;
                foreach (var item in e.EnumerateArray()) Flatten(item, $"{path}[{i++}]", into);
                break;
            default:
                var text = e.ValueKind == JsonValueKind.String ? e.GetString() ?? string.Empty : e.ToString();
                into.Add(text.Contains("$session.params.", StringComparison.Ordinal)
                    ? $"`{path}` ← {string.Join(" + ", ParamRef().Matches(text).Select(m => "`" + m.Groups["n"].Value + "`"))}"
                    : $"`{path}` = «{Cut(text, 30)}»");
                break;
        }
    }

    #endregion

    #region Frases

    private static string Icon(CamilaWebhook w) => w.Kind == "write" ? Write : Read;

    private static string Trigger(CamilaRoute r) =>
        r.Intent is not null ? $"intent «{r.Intent}»" : r.Event is not null ? $"evento `{r.Event}`" : string.IsNullOrWhiteSpace(r.Condition) || r.Condition == "true" ? "siempre" : "condición";

    private static string When(CamilaRoute r) =>
        string.IsNullOrWhiteSpace(r.Condition) || r.Condition == "true" ? "siempre" : $"si `{r.Condition}`";

    private static string CondText(CamilaRoute r) =>
        string.IsNullOrWhiteSpace(r.Condition) || r.Condition == "true" ? string.Empty : $" · solo si `{r.Condition}`";

    private static string Destination(CamilaRoute r)
    {
        var page = r.TargetPage;
        if (page is not null && CamilaEngine.IsSpecialPage(page))
            return CamilaEngine.SpecialPage(page) switch { "END_SESSION" => "🔚 **termina la conversación**", "END_FLOW" => "↩️ vuelve al flujo inicial", "CURRENT_PAGE" => "se queda en esta página", "PREVIOUS_PAGE" => "vuelve a la página anterior", _ => "inicio del flujo" };
        if (r.TargetFlow is not null) return page is null ? $"flujo **{r.TargetFlow}**" : $"flujo **{r.TargetFlow}** › página «{page}»";
        return page is null ? "se queda en la página" : $"página «{page}»";
    }

    private static string Effects(CamilaFulfillment f, CamilaBot bot)
    {
        var parts = new List<string>();
        if (f.SetParams.Count > 0) parts.Add("📌 asigna " + string.Join(", ", f.SetParams.Take(4).Select(SetText)) + (f.SetParams.Count > 4 ? $" (+{f.SetParams.Count - 4})" : string.Empty));
        if (f.Webhook is not null) parts.Add(WebhookStep(bot, f.Webhook, f.Tag));
        foreach (var g in f.Generators) parts.Add(GeneratorStep(bot, g));
        var say = f.Messages.FirstOrDefault(m => m.Type == "text")?.Texts.FirstOrDefault();
        if (say is not null) parts.Add($"💬 dice «{Cut(say, 60)}»");
        return parts.Count == 0 ? string.Empty : "  \n  ↳ " + string.Join(" · ", parts);
    }

    private static string WebhookStep(CamilaBot bot, string name, string? tag)
    {
        var w = bot.FindWebhook(name);
        var icon = w is null ? "⚡" : Icon(w);
        var kind = w is null ? "(no está en la base)" : w.Kind == "write" ? "**escritura, pide confirmación**" : "lectura";
        return $"{icon} Llama al webhook **{name}**{(string.IsNullOrEmpty(tag) ? string.Empty : $" [{tag}]")} — {kind}";
    }

    private static string GeneratorStep(CamilaBot bot, CamilaGeneratorCall g)
    {
        var def = bot.FindGenerator(g.Generator);
        var output = g.Output.Replace("$session.params.", string.Empty);
        return $"🤖 Ejecuta el generador de IA **{def?.Name ?? g.Generator}** con {string.Join(", ", g.Inputs.Keys.Select(k => "`" + k + "`"))} → guarda en `{output}`";
    }

    private static string MessageText(CamilaMessage m) => m.Type switch
    {
        "info" => $"💬 Muestra la tarjeta «{m.Title}»{(m.Url is null ? string.Empty : $" con enlace {m.Url}")}",
        "image" => $"🖼️ Muestra una imagen {m.Url}",
        _ => "💬 Dice: «" + Cut(m.Texts.Count > 0 ? m.Texts[0] : string.Empty, 200) + "»" + (m.Texts.Count > 1 ? $" (+{m.Texts.Count - 1} variante(s))" : string.Empty)
    };

    private static string SetText(CamilaSet s)
    {
        var name = s.Parameter.Replace("$session.params.", string.Empty);
        var value = CamilaExpression.Normalize(s.Value);
        return $"`{name}` = {(value is string str ? "`" + Cut(str, 60) + "`" : "`" + CamilaExpression.ToText(value) + "`")}";
    }

    private static string Cut(string text, int max)
    {
        var flat = text.Replace("\r", " ").Replace("\n", " ").Trim();
        return flat.Length <= max ? flat : flat[..max] + "…";
    }

    #endregion
}
