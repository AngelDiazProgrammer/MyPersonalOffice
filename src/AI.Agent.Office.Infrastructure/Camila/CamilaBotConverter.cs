using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AI.Agent.Office.Application.Camila;

namespace AI.Agent.Office.Infrastructure.Camila;

/// <summary>Resultado de convertir el export: el bot, los secretos extraídos y los avisos.</summary>
public sealed class CamilaConversion
{
    public CamilaBot Bot { get; } = new();
    /// <summary>Nombre del secreto → valor. Nunca se escribe en CamilaIA.md: el llamador lo guarda en .env.</summary>
    public Dictionary<string, string> Secrets { get; } = new(StringComparer.Ordinal);
    public List<string> Warnings { get; } = new();
    /// <summary>Hosts (con puerto) a los que apuntan los webhooks.</summary>
    public SortedSet<string> Hosts { get; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Convierte el export de Dialogflow CX (carpeta <c>Bots/</c>) al modelo de Camila (spec 009, RF-001 a RF-006). Es determinista: el mismo
/// export produce siempre el mismo resultado. Las credenciales literales se sacan a <see cref="CamilaConversion.Secrets"/> y en el
/// modelo quedan como referencias <c>{{secret:NOMBRE}}</c>.
/// </summary>
public static partial class CamilaBotConverter
{
    private static readonly Regex SensitiveHeader = SensitiveHeaderRegex();

    [GeneratedRegex("authorization|api-?key|x-api|password|passwd|token|secret|user", RegexOptions.IgnoreCase)]
    private static partial Regex SensitiveHeaderRegex();

    #region Entrada

    /// <summary>Lee la carpeta del export y arma el modelo.</summary>
    public static CamilaConversion Convert(string botsDirectory)
    {
        if (!Directory.Exists(botsDirectory)) throw new DirectoryNotFoundException($"No existe la carpeta del bot: {botsDirectory}");

        var result = new CamilaConversion();
        var agent = ReadJson(Path.Combine(botsDirectory, "agent.json"));
        if (agent is { } a && a.TryGetProperty("startFlow", out var start) && start.GetString() is { Length: > 0 } startName) result.Bot.StartFlow = startName;

        ReadGroups(Path.Combine(botsDirectory, "agentTransitionRouteGroups"), null, result);
        ReadFlows(Path.Combine(botsDirectory, "flows"), result);
        ReadIntents(Path.Combine(botsDirectory, "intents"), result);
        ReadEntities(Path.Combine(botsDirectory, "entityTypes"), result);
        ReadGenerators(Path.Combine(botsDirectory, "generators"), result);
        ReadWebhooks(Path.Combine(botsDirectory, "webhooks"), result);

        result.Bot.Flows.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
        result.Bot.Intents.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
        result.Bot.Webhooks.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
        result.Bot.Generators.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
        result.Bot.Entities.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
        result.Bot.Groups.Sort((x, y) => string.Compare(x.Flow + "/" + x.Name, y.Flow + "/" + y.Name, StringComparison.Ordinal));
        return result;
    }

    private static JsonElement? ReadJson(string path)
    {
        if (!File.Exists(path)) return null;
        return JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8), new JsonDocumentOptions { AllowTrailingCommas = true }).RootElement.Clone();
    }

    private static string Str(JsonElement e, string name) => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? string.Empty : string.Empty;

    private static string? StrOrNull(JsonElement e, string name) => Str(e, name) is { Length: > 0 } s ? s : null;

    private static IEnumerable<JsonElement> Items(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Array ? v.EnumerateArray() : Enumerable.Empty<JsonElement>();

    private static IEnumerable<string> Files(string dir, string pattern = "*.json") =>
        Directory.Exists(dir) ? Directory.EnumerateFiles(dir, pattern).OrderBy(f => f, StringComparer.Ordinal) : Enumerable.Empty<string>();

    private static IEnumerable<string> Dirs(string dir) =>
        Directory.Exists(dir) ? Directory.EnumerateDirectories(dir).OrderBy(f => f, StringComparer.Ordinal) : Enumerable.Empty<string>();

    #endregion

    #region Flujos, páginas y rutas

    private static void ReadFlows(string flowsDir, CamilaConversion result)
    {
        foreach (var dir in Dirs(flowsDir))
        {
            var main = Files(dir).FirstOrDefault(f => string.Equals(Path.GetFileNameWithoutExtension(f), Path.GetFileName(dir), StringComparison.Ordinal));
            if (main is null || ReadJson(main) is not { } flowJson) { result.Warnings.Add($"Carpeta de flujo sin archivo principal: {Path.GetFileName(dir)}"); continue; }

            var flow = new CamilaFlow
            {
                Name = Str(flowJson, "displayName"),
                Routes = Items(flowJson, "transitionRoutes").Select(r => Route(r, result)).ToList(),
                Events = Items(flowJson, "eventHandlers").Select(r => Route(r, result)).ToList(),
                Groups = Items(flowJson, "transitionRouteGroups").Select(g => g.GetString() ?? string.Empty).Where(g => g.Length > 0).ToList()
            };

            foreach (var file in Files(Path.Combine(dir, "pages")))
                if (ReadJson(file) is { } pageJson) flow.Pages.Add(Page(pageJson, result));
            flow.Pages.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

            ReadGroups(Path.Combine(dir, "transitionRouteGroups"), flow.Name, result);
            result.Bot.Flows.Add(flow);
        }
    }

    private static void ReadGroups(string dir, string? flow, CamilaConversion result)
    {
        foreach (var file in Files(dir))
        {
            if (ReadJson(file) is not { } g) continue;
            var name = Str(g, "displayName");
            if (result.Bot.Groups.Any(x => x.Name == name && x.Flow == flow)) continue;        // el export repite grupos con otro nombre de archivo
            result.Bot.Groups.Add(new CamilaRouteGroup { Name = name, Flow = flow, Routes = Items(g, "transitionRoutes").Select(r => Route(r, result)).ToList() });
        }
    }

    private static CamilaPage Page(JsonElement p, CamilaConversion result)
    {
        var page = new CamilaPage
        {
            Name = Str(p, "displayName"),
            Entry = p.TryGetProperty("entryFulfillment", out var entry) ? Fulfillment(entry, result) : new CamilaFulfillment(),
            Routes = Items(p, "transitionRoutes").Select(r => Route(r, result)).ToList(),
            Events = Items(p, "eventHandlers").Select(r => Route(r, result)).ToList(),
            Groups = Items(p, "transitionRouteGroups").Select(g => g.GetString() ?? string.Empty).Where(g => g.Length > 0).ToList()
        };

        if (p.TryGetProperty("form", out var form))
        {
            foreach (var fp in Items(form, "parameters"))
            {
                var fill = fp.TryGetProperty("fillBehavior", out var fb) ? fb : default;
                page.Form.Add(new CamilaFormParam
                {
                    Name = Str(fp, "displayName"),
                    Entity = StrOrNull(fp, "entityType") ?? "@sys.any",
                    Required = fp.TryGetProperty("required", out var req) && req.ValueKind == JsonValueKind.True,
                    Prompt = fill.ValueKind == JsonValueKind.Object && fill.TryGetProperty("initialPromptFulfillment", out var ip) ? Fulfillment(ip, result) : new CamilaFulfillment()
                });
            }
        }
        return page;
    }

    private static CamilaRoute Route(JsonElement r, CamilaConversion result) => new()
    {
        Intent = StrOrNull(r, "intent"),
        Condition = StrOrNull(r, "condition"),
        Event = StrOrNull(r, "event"),
        TargetPage = StrOrNull(r, "targetPage"),
        TargetFlow = StrOrNull(r, "targetFlow"),
        Fulfillment = r.TryGetProperty("triggerFulfillment", out var f) ? Fulfillment(f, result) : new CamilaFulfillment()
    };

    private static CamilaFulfillment Fulfillment(JsonElement f, CamilaConversion result)
    {
        var ful = new CamilaFulfillment { Webhook = StrOrNull(f, "webhook"), Tag = StrOrNull(f, "tag") };
        if (f.TryGetProperty("conditionalCases", out _)) result.Warnings.Add("Hay respuestas condicionales (conditionalCases) que no se interpretan.");

        foreach (var m in Items(f, "messages"))
        {
            if (m.TryGetProperty("text", out var t))
            {
                var texts = Items(t, "text").Select(x => x.GetString() ?? string.Empty).Where(x => x.Length > 0).ToList();
                if (texts.Count > 0) ful.Messages.Add(new CamilaMessage { Type = "text", Texts = texts });
            }
            else if (m.TryGetProperty("payload", out var payload))
            {
                foreach (var row in Items(payload, "richContent"))
                {
                    var buttons = new CamilaMessage { Type = "buttons" };
                    foreach (var el in row.EnumerateArray())
                    {
                        switch (Str(el, "type"))
                        {
                            case "button":
                                buttons.Buttons.Add(new CamilaButton
                                {
                                    Text = Str(el, "text"),
                                    Event = el.TryGetProperty("event", out var ev) ? StrOrNull(ev, "event") : null,
                                    Link = StrOrNull(el, "link")
                                });
                                break;
                            case "info":
                                ful.Messages.Add(new CamilaMessage { Type = "info", Title = StrOrNull(el, "title"), Subtitle = StrOrNull(el, "subtitle"), Url = StrOrNull(el, "actionLink") });
                                break;
                            case "image":
                                ful.Messages.Add(new CamilaMessage { Type = "image", Url = StrOrNull(el, "rawUrl"), Title = StrOrNull(el, "accessibilityText") });
                                break;
                            default:
                                result.Warnings.Add($"Elemento de contenido enriquecido no soportado: {Str(el, "type")}");
                                break;
                        }
                    }
                    if (buttons.Buttons.Count > 0) ful.Messages.Add(buttons);
                }
            }
        }

        foreach (var s in Items(f, "setParameterActions"))
            ful.SetParams.Add(new CamilaSet { Parameter = Str(s, "parameter"), Value = s.TryGetProperty("value", out var v) ? CamilaExpression.FromJson(v) : null });

        foreach (var g in Items(f, "generators"))
        {
            var call = new CamilaGeneratorCall { Generator = Str(g, "generator"), Output = Str(g, "outputParameter") };
            foreach (var input in Items(g, "inputParameters")) call.Inputs[Str(input, "key")] = Str(input, "value");
            ful.Generators.Add(call);
        }
        return ful;
    }

    #endregion

    #region Intents, entidades y generadores

    private static void ReadIntents(string dir, CamilaConversion result)
    {
        foreach (var folder in Dirs(dir))
        {
            var main = Files(folder).FirstOrDefault();
            if (main is null || ReadJson(main) is not { } json) continue;
            var intent = new CamilaIntent { Name = Str(json, "displayName") };
            foreach (var file in Files(Path.Combine(folder, "trainingPhrases")))
                if (ReadJson(file) is { } phrases)
                    foreach (var tp in Items(phrases, "trainingPhrases"))
                    {
                        var text = string.Concat(Items(tp, "parts").Select(p => Str(p, "text"))).Trim();
                        if (text.Length > 0 && !intent.Phrases.Contains(text)) intent.Phrases.Add(text);
                    }
            result.Bot.Intents.Add(intent);
        }
    }

    private static void ReadEntities(string dir, CamilaConversion result)
    {
        foreach (var folder in Dirs(dir))
        {
            var main = Files(folder).FirstOrDefault();
            if (main is null || ReadJson(main) is not { } json) continue;
            var entity = new CamilaEntityType { Name = Str(json, "displayName"), Kind = Str(json, "kind") };
            var regexp = entity.Kind.Contains("REGEXP", StringComparison.OrdinalIgnoreCase);
            foreach (var file in Files(Path.Combine(folder, "entities")))
                if (ReadJson(file) is { } entries)
                    foreach (var e in Items(entries, "entities"))
                    {
                        if (regexp) entity.Values.Add(Str(e, "value"));
                        else entity.Values.AddRange(Items(e, "synonyms").Select(s => s.GetString() ?? string.Empty).Where(s => s.Length > 0));
                    }
            result.Bot.Entities.Add(entity);
        }
    }

    private static void ReadGenerators(string dir, CamilaConversion result)
    {
        foreach (var folder in Dirs(dir))
        {
            var main = Files(folder).FirstOrDefault();
            if (main is null || ReadJson(main) is not { } json) continue;
            var gen = new CamilaGeneratorDef { Name = Str(json, "displayName"), Id = Str(json, "name") };
            if (json.TryGetProperty("modelParameter", out var mp) && mp.TryGetProperty("temperature", out var temp) && temp.ValueKind == JsonValueKind.Number)
                gen.Temperature = Math.Round(temp.GetDouble(), 2);
            foreach (var file in Files(Path.Combine(folder, "phrases")))
                if (ReadJson(file) is { } phrases)
                    gen.Prompt = string.Join("\n", Items(phrases, "promptTexts").Select(p => Str(p, "text")));
            result.Bot.Generators.Add(gen);
        }
    }

    #endregion

    #region Webhooks y secretos

    private static void ReadWebhooks(string dir, CamilaConversion result)
    {
        foreach (var file in Files(dir))
        {
            if (ReadJson(file) is not { } json || !json.TryGetProperty("genericWebService", out var g)) continue;

            var hook = new CamilaWebhook
            {
                Name = Str(json, "displayName"),
                Uri = Str(g, "uri"),
                Method = StrOrNull(g, "httpMethod") ?? "POST",
                Body = StrOrNull(g, "requestBody"),
                TimeoutSeconds = json.TryGetProperty("timeout", out var to) && to.TryGetProperty("seconds", out var secs) && secs.ValueKind == JsonValueKind.Number ? secs.GetInt32() : 30
            };
            hook.Kind = CamilaWebhookService.GuessKind(hook.Name);
            var slug = Slug(hook.Name);

            foreach (var (name, value) in Headers(g))
            {
                if (SensitiveHeader.IsMatch(name) && value.Length > 0 && !value.Contains("$session.params.", StringComparison.Ordinal))
                {
                    var secretName = $"CAMILA_WH_{slug}_{Slug(name)}";
                    result.Secrets[secretName] = value;
                    hook.Headers[name] = "{{secret:" + secretName + "}}";
                }
                else hook.Headers[name] = value;
            }

            var user = StrOrNull(g, "username");
            var pass = StrOrNull(g, "password");
            if (user is not null && pass is not null)
            {
                var secretName = $"CAMILA_WH_{slug}_BASIC";
                result.Secrets[secretName] = System.Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{pass}"));
                hook.Headers["Authorization"] = "Basic {{secret:" + secretName + "}}";
            }

            if (g.TryGetProperty("serviceAgentAuth", out _) || g.TryGetProperty("oauthConfig", out _))
                hook.Notes.Add("Usa autenticación de cuenta de servicio de Google (serviceAgentAuth/oauth): Camila no la reproduce; solo envía las cabeceras indicadas.");
            // Sin webhookType Dialogflow usa el formato ESTÁNDAR: envía su WebhookRequest y espera sessionInfo.parameters en la respuesta.
            hook.Type = string.Equals(StrOrNull(g, "webhookType"), "FLEXIBLE", StringComparison.OrdinalIgnoreCase) ? "flexible" : "standard";

            foreach (var m in Items(g, "parameterMapping")) hook.Mapping.Add(new CamilaMapping { Parameter = Str(m, "key"), Path = Str(m, "value") });

            if (Uri.TryCreate(hook.Uri, UriKind.Absolute, out var uri)) result.Hosts.Add(uri.Authority);
            else result.Warnings.Add($"El webhook «{hook.Name}» no tiene una URL absoluta válida.");
            foreach (var note in hook.Notes) result.Warnings.Add($"{hook.Name}: {note}");
            result.Bot.Webhooks.Add(hook);
        }
    }

    /// <summary>Las cabeceras vienen como objeto o como lista de pares clave/valor, según el webhook.</summary>
    private static IEnumerable<(string Name, string Value)> Headers(JsonElement g)
    {
        if (!g.TryGetProperty("requestHeaders", out var h)) yield break;
        if (h.ValueKind == JsonValueKind.Object)
        {
            foreach (var p in h.EnumerateObject()) yield return (p.Name, p.Value.GetString() ?? string.Empty);
        }
        else if (h.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in h.EnumerateArray())
                if (item.ValueKind == JsonValueKind.Object && StrOrNull(item, "key") is { } key) yield return (key, Str(item, "value"));
        }
    }

    /// <summary>MAYÚSCULAS_CON_GUIONES_BAJOS, sin acentos.</summary>
    public static string Slug(string text)
    {
        var sb = new StringBuilder();
        foreach (var ch in text.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(char.IsAsciiLetterOrDigit(ch) ? char.ToUpperInvariant(ch) : '_');
        }
        return Regex.Replace(sb.ToString(), "_+", "_").Trim('_');
    }

    #endregion
}
