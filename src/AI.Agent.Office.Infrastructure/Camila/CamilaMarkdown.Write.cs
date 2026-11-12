using System.Text;
using System.Text.Json;
using AI.Agent.Office.Application.Camila;

namespace AI.Agent.Office.Infrastructure.Camila;

/// <summary>
/// Escritura de los documentos de un bot: la narrativa de la base de conocimiento (sin bloques) y los playbooks ejecutables (con bloques
/// <c>camila:*</c>), siempre con la explicación en lenguaje claro de <see cref="CamilaNarrative"/>.
/// </summary>
public static partial class CamilaMarkdown
{
    #region Documento completo (narrativa o playbook único)

    /// <summary>
    /// Genera el Markdown completo del bot. Con <paramref name="includeBlocks"/> falso es la <b>narrativa</b> de la base de conocimiento
    /// (la lee CamilaIA); con verdadero lleva además los bloques ejecutables. No lleva fecha ni credenciales: dos generaciones del mismo
    /// export son idénticas.
    /// </summary>
    public static string Write(CamilaBot bot, bool includeBlocks = true)
    {
        var sb = new StringBuilder();
        var index = CamilaNarrative.Build(bot);

        sb.Append("---\nconfidential: true\n---\n");
        sb.Append("# Camila IA · Base de conocimiento del bot CamilaOS\n\n");
        sb.Append("Generado automáticamente desde el export de Dialogflow CX (`Bots/`). **No lo edites a mano**: se regenera con `dotnet run --project src/AI.Agent.Office.Web -- --bot-generar` y el resultado es siempre el mismo.\n");
        sb.Append(includeBlocks
            ? "Cada elemento lleva su explicación en lenguaje claro y un bloque `camila:*` con la definición exacta que ejecuta el agente LectorUML.\n"
            : "Es la **narrativa** de la base de conocimiento: la lee el agente CamilaIA. El flujo ejecutable de cada bot está en sus playbooks (`playbooks bots/`).\n");
        sb.Append("Este archivo **no contiene credenciales**: los webhooks citan secretos externos con `{{secret:NOMBRE}}` (se definen en `.env`).\n\n");
        sb.Append(CamilaNarrative.Overview(bot, index));

        foreach (var f in bot.Flows) sb.Append(FlowSection(bot, index, f, includeBlocks));
        foreach (var kind in Catalogs) sb.Append(CatalogSection(bot, index, kind, includeBlocks));
        return sb.ToString().Replace("\r\n", "\n");
    }

    /// <summary>Catálogos de un bot, en el orden en que se escriben.</summary>
    public static readonly string[] Catalogs = { "groups", "intents", "entities", "generators", "webhooks" };

    #endregion

    #region Secciones (las comparten el documento completo y los playbooks por archivo)

    /// <summary>Un flujo: su explicación, su bloque y cada página con su explicación y su bloque.</summary>
    public static string FlowSection(CamilaBot bot, CamilaNarrative.Index index, CamilaFlow f, bool includeBlocks = true)
    {
        var sb = new StringBuilder();
        sb.Append($"## Flujo · {f.Name}\n\n");
        sb.Append(CamilaNarrative.Flow(bot, index, f));
        if (includeBlocks) sb.Append(Fence("flow", new FlowHeader { Name = f.Name, Routes = f.Routes, Events = f.Events, Groups = f.Groups }));

        foreach (var p in f.Pages)
        {
            sb.Append($"### Página · {f.Name} › {p.Name}\n\n");
            sb.Append(CamilaNarrative.Page(bot, f, p));
            if (includeBlocks) sb.Append(Fence("page", new PageBlock { Flow = f.Name, Page = p }));
        }
        return sb.ToString();
    }

    /// <summary>Un catálogo: <c>groups</c>, <c>intents</c>, <c>entities</c>, <c>generators</c> o <c>webhooks</c>.</summary>
    public static string CatalogSection(CamilaBot bot, CamilaNarrative.Index index, string kind, bool includeBlocks = true)
    {
        var sb = new StringBuilder();
        string F<T>(string block, T value) => includeBlocks ? Fence(block, value) : string.Empty;

        switch (kind)
        {
            case "groups":
                sb.Append("## Grupos de rutas (atajos)\n\nRutas compartidas por varias páginas o flujos (por ejemplo «volver al menú principal»).\n\n");
                foreach (var g in bot.Groups)
                {
                    sb.Append($"### Grupo · {g.Name}{(string.IsNullOrEmpty(g.Flow) ? " (global)" : $" (flujo {g.Flow})")}\n\n");
                    foreach (var r in g.Routes)
                        sb.Append("- " + (r.Intent is not null ? $"intent «{r.Intent}»" : r.Event is not null ? $"evento `{r.Event}`" : "siempre") + " → " + (r.TargetPage ?? r.TargetFlow ?? "se queda") + "\n");
                    sb.Append('\n');
                    sb.Append(F("group", g));
                }
                break;

            case "intents":
                sb.Append("## Intents\n\nFrases del usuario que el bot reconoce.\n\n");
                foreach (var i in bot.Intents)
                {
                    sb.Append($"### Intent · {i.Name}\n\nFrases: {string.Join(" | ", i.Phrases.Take(6))}{(i.Phrases.Count > 6 ? $" … ({i.Phrases.Count} en total)" : string.Empty)}\n\n");
                    sb.Append(F("intent", i));
                }
                break;

            case "entities":
                sb.Append("## Entidades\n\nFormato que debe tener un dato que el bot pide.\n\n");
                foreach (var e in bot.Entities)
                {
                    sb.Append($"### Entidad · {e.Name} ({e.Kind})\n\nValores válidos: {string.Join(" | ", e.Values.Take(6).Select(v => "`" + v + "`"))}\n\n");
                    sb.Append(F("entity", e));
                }
                break;

            case "generators":
                sb.Append("## Generadores de IA\n\nPrompts que el bot original enviaba a un modelo generativo. LectorUML no los ejecuta.\n\n");
                foreach (var g in bot.Generators)
                {
                    var uses = index.GeneratorUses.TryGetValue(g.Name, out var set) ? string.Join("; ", set.Take(4)) : "(no se usa)";
                    sb.Append($"### Generador · {g.Name}\n\nSe usa en: {uses}\n\nPrompt:\n\n> {g.Prompt.Replace("\n", "\n> ")}\n\n");
                    sb.Append(F("generator", g));
                }
                break;

            case "webhooks":
                sb.Append("## Webhooks\n\nServicios a los que llama el bot. Los de escritura (✍️) piden confirmación antes de ejecutarse.\n\n");
                foreach (var w in bot.Webhooks)
                {
                    sb.Append($"### Webhook · {w.Name}\n\n");
                    sb.Append(CamilaNarrative.Webhook(index, w));
                    sb.Append(F("webhook", w));
                }
                break;
        }
        return sb.ToString();
    }

    private static string Fence<T>(string kind, T value)
    {
        var json = JsonSerializer.Serialize(value, Json).Replace("```", "\\u0060\\u0060\\u0060", StringComparison.Ordinal);
        return $"```json camila:{kind}\n{json}\n```\n\n";
    }

    #endregion
}
