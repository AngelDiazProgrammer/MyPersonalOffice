using System.Text;

namespace AI.Agent.Office.Application.Camila;

/// <summary>Presentación en Markdown de la oficina «Camila IA UML»: jerarquía, ficha de página y respuestas del bot sin reformular.</summary>
public static class CamilaUmlView
{
    #region Jerarquía

    /// <summary>Árbol completo: flujos y sus páginas, con las rutas de salida hacia otros flujos.</summary>
    public static string Hierarchy(CamilaBot bot, string? currentFlow, string? currentPage)
    {
        var sb = new StringBuilder("## 🌳 Jerarquía del bot CamilaOS\n\n");
        sb.Append($"{bot.Flows.Count} flujos · {bot.Flows.Sum(f => f.Pages.Count)} páginas · inicio: **{bot.StartFlow}**\n\n```\n");
        foreach (var f in bot.Flows)
        {
            sb.Append($"{(string.Equals(f.Name, currentFlow, StringComparison.OrdinalIgnoreCase) ? "▶ " : "")}📁 {f.Name}\n");
            for (var i = 0; i < f.Pages.Count; i++)
            {
                var last = i == f.Pages.Count - 1;
                var here = string.Equals(f.Name, currentFlow, StringComparison.OrdinalIgnoreCase) && string.Equals(f.Pages[i].Name, currentPage, StringComparison.OrdinalIgnoreCase);
                var hook = HooksOf(f.Pages[i]).Count > 0 ? " ⚡" : string.Empty;
                sb.Append($"  {(last ? "└─" : "├─")} {(here ? "▶ " : "")}{f.Pages[i].Name}{hook}\n");
            }
        }
        sb.Append("```\n\n⚡ = la página llama a un webhook. Usa `/ir <flujo> › <página>` para saltar, por ejemplo `/ir 01 › 001`.\n");
        return sb.ToString();
    }

    private static List<string> HooksOf(CamilaPage p) =>
        new[] { p.Entry.Webhook }.Concat(p.Routes.Select(r => r.Fulfillment.Webhook)).Concat(p.Events.Select(r => r.Fulfillment.Webhook))
            .Where(w => w is not null).Select(w => w!).Distinct().ToList();

    #endregion

    #region Ficha de página

    /// <summary>Ficha de la página: textos originales, botones, formulario, webhooks y rutas.</summary>
    public static string Card(CamilaBot bot, CamilaFlow flow, CamilaPage? page, ICamilaWebhookService hooks)
    {
        var sb = new StringBuilder();
        if (page is null)
        {
            sb.Append($"#### 🧩 Ficha · {flow.Name} (inicio del flujo)\n\n");
            AppendRoutes(sb, flow.Routes.Select(r => ("flujo", r)).Concat(flow.Events.Select(r => ("flujo", r))).ToList());
            return sb.ToString();
        }

        sb.Append($"#### 🧩 Ficha · {flow.Name} › {page.Name}\n\n");

        var texts = page.Entry.Messages.Where(m => m.Type == "text").ToList();
        if (texts.Count > 0)
        {
            sb.Append("**Textos originales**\n");
            foreach (var t in texts) sb.Append("> " + string.Join("  \n> ", t.Texts.Select(x => x.Replace("\n", " "))) + "\n\n");
        }

        var buttons = page.Entry.Messages.Where(m => m.Type == "buttons").SelectMany(m => m.Buttons).ToList();
        if (buttons.Count > 0) sb.Append("**Botones:** " + string.Join(" · ", buttons.Select(b => $"`{b.Text}` → {(b.Event is not null ? "evento `" + b.Event + "`" : b.Link)}")) + "\n\n");

        if (page.Form.Count > 0)
            sb.Append("**Formulario:** " + string.Join(", ", page.Form.Select(f => $"`{f.Name}` ({f.Entity}{(f.Required ? ", obligatorio" : string.Empty)})")) + "\n\n");

        foreach (var hookName in HooksOf(page))
            if (bot.FindWebhook(hookName) is { } w)
                sb.Append($"**Webhook:** `{w.Name}` · {w.Method} {w.Host} · modo **{hooks.ModeOf(w)}** · {(hooks.IsWrite(w) ? "escritura" : "lectura")}\n\n");
            else sb.Append($"**Webhook:** `{hookName}` (no está en la base)\n\n");

        var routes = page.Routes.Select(r => ("página", r)).Concat(page.Events.Select(r => ("página", r)))
            .Concat(page.Groups.SelectMany(g => bot.FindGroup(g, flow.Name)?.Routes.Select(r => ("grupo " + g, r)) ?? Enumerable.Empty<(string, CamilaRoute)>()))
            .Concat(flow.Routes.Select(r => ("flujo", r))).Concat(flow.Events.Select(r => ("flujo", r))).ToList();
        AppendRoutes(sb, routes);
        return sb.ToString();
    }

    private static void AppendRoutes(StringBuilder sb, IReadOnlyList<(string Origin, CamilaRoute Route)> routes)
    {
        if (routes.Count == 0) return;
        sb.Append("**Rutas**\n");
        foreach (var (origin, r) in routes.Take(14))
        {
            var when = r.Intent is not null ? $"intent «{r.Intent}»" : r.Event is not null ? $"evento `{r.Event}`" : "automática";
            var cond = string.IsNullOrWhiteSpace(r.Condition) || r.Condition == "true" ? string.Empty : $" · si `{r.Condition}`";
            var target = r.TargetPage is not null ? $"página «{r.TargetPage}»" : r.TargetFlow is not null ? $"flujo «{r.TargetFlow}»" : "se queda";
            var hook = r.Fulfillment.Webhook is not null ? $" · ⚡ {r.Fulfillment.Webhook}" : string.Empty;
            sb.Append($"- ({origin}) {when}{cond} → {target}{hook}\n");
        }
        if (routes.Count > 14) sb.Append($"- … y {routes.Count - 14} ruta(s) más\n");
        sb.Append('\n');
    }

    #endregion

    #region Respuesta del bot

    /// <summary>Los mensajes del bot, tal cual (texto, tarjetas, imágenes y enlaces).</summary>
    public static string BotMessages(CamilaTurn turn)
    {
        var sb = new StringBuilder();
        foreach (var m in turn.Messages)
        {
            switch (m.Kind)
            {
                case "info":
                    sb.Append($"**{m.Title}**{(string.IsNullOrWhiteSpace(m.Text) ? "" : " — " + m.Text)}{(m.Url is null ? "" : $" [abrir]({m.Url})")}\n\n");
                    break;
                case "image":
                    sb.Append(m.Url is null ? string.Empty : $"![{m.Text}]({m.Url})\n\n");
                    break;
                default:
                    sb.Append(m.Text.Replace("\n", "  \n") + "\n\n");
                    break;
            }
        }

        var links = turn.Options.Where(o => o.Link is not null).ToList();
        if (links.Count > 0) sb.Append(string.Join(" · ", links.Select(l => $"[{l.Label}]({l.Link})")) + "\n\n");
        return sb.ToString();
    }

    /// <summary>La traza de lo que hizo el motor (webhooks, generadores, rutas), para auditar el recorrido.</summary>
    public static string Trace(CamilaTurn turn)
    {
        var lines = turn.Trace.Where(t => t.Kind is "webhook" or "generador" or "aviso" or "param").ToList();
        if (lines.Count == 0) return string.Empty;
        var sb = new StringBuilder("🔧 **Qué hizo el bot**\n");
        foreach (var t in lines)
            sb.Append($"- {(t.Kind switch { "webhook" => "⚡", "generador" => "🤖", "aviso" => "⚠️", _ => "✏️" })} {t.Text}\n");
        return sb + "\n";
    }

    /// <summary>Tarjeta de confirmación de un webhook de escritura.</summary>
    public static string ConfirmCard(CamilaPendingConfirm c)
    {
        var body = string.IsNullOrWhiteSpace(c.Body) ? "(sin cuerpo)" : c.Body;
        return "### ⚠️ Confirmación necesaria\n\n" +
               $"El flujo va a **modificar datos reales** llamando a `{c.Webhook}`.\n\n" +
               $"- Destino: `{c.Method} {c.Host}{c.Path}`\n- Se enviará:\n\n```json\n{body}\n```\n\n" +
               "¿Lo envío? Pulsa **Sí, enviar** o **No, cancelar**.\n";
    }

    #endregion
}
