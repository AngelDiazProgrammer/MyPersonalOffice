using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AI.Agent.Office.Application.Camila;
using Microsoft.Extensions.Logging;

namespace AI.Agent.Office.Infrastructure.Camila;

/// <summary>
/// Formato de <c>CamilaIA.md</c>: texto legible (para personas y para el modelo) y, tras cada elemento, un bloque
/// <c>```json camila:*</c> con su definición exacta. El generador escribe el archivo; el cargador lo vuelve a leer.
/// El archivo no lleva fecha ni credenciales: dos generaciones del mismo export son idénticas.
/// </summary>
public static partial class CamilaMarkdown
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    [GeneratedRegex("```json camila:(?<kind>[a-z]+)\n(?<json>.*?)\n```", RegexOptions.Singleline)]
    private static partial Regex Block();


    #region Lectura

    /// <summary>Reconstruye el bot desde el Markdown. Devuelve null si no hay bloques.</summary>
    public static CamilaBot? Read(string markdown)
    {
        var bot = new CamilaBot();
        var any = ReadInto(bot, markdown);

        var start = Regex.Match(markdown, @"Flujo inicial:\s*(.+)");
        if (start.Success) bot.StartFlow = start.Groups[1].Value.Trim();
        return any ? bot : null;
    }

    /// <summary>
    /// Agrega al bot los elementos de los bloques <c>camila:*</c> de un Markdown (un playbook de flujo o un catálogo). Los flujos deben
    /// leerse antes que sus páginas, como están en cada archivo. Devuelve si encontró algún bloque.
    /// </summary>
    public static bool ReadInto(CamilaBot bot, string markdown)
    {
        var any = false;
        var flows = bot.Flows.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);

        foreach (Match m in Block().Matches(markdown.Replace("\r\n", "\n")))
        {
            var json = m.Groups["json"].Value;
            any = true;
            switch (m.Groups["kind"].Value)
            {
                case "flow":
                    var h = JsonSerializer.Deserialize<FlowHeader>(json, Json)!;
                    var flow = new CamilaFlow { Name = h.Name, Routes = h.Routes, Events = h.Events, Groups = h.Groups };
                    flows[flow.Name] = flow;
                    bot.Flows.Add(flow);
                    break;
                case "page":
                    var pb = JsonSerializer.Deserialize<PageBlock>(json, Json)!;
                    if (flows.TryGetValue(pb.Flow, out var owner)) owner.Pages.Add(pb.Page);
                    break;
                case "group": bot.Groups.Add(JsonSerializer.Deserialize<CamilaRouteGroup>(json, Json)!); break;
                case "intent": bot.Intents.Add(JsonSerializer.Deserialize<CamilaIntent>(json, Json)!); break;
                case "entity": bot.Entities.Add(JsonSerializer.Deserialize<CamilaEntityType>(json, Json)!); break;
                case "generator": bot.Generators.Add(JsonSerializer.Deserialize<CamilaGeneratorDef>(json, Json)!); break;
                case "webhook": bot.Webhooks.Add(JsonSerializer.Deserialize<CamilaWebhook>(json, Json)!); break;
            }
        }
        return any;
    }

    #endregion

    private sealed class FlowHeader
    {
        public string Name { get; set; } = string.Empty;
        public List<CamilaRoute> Routes { get; set; } = new();
        public List<CamilaRoute> Events { get; set; } = new();
        public List<string> Groups { get; set; } = new();
    }

    private sealed class PageBlock
    {
        public string Flow { get; set; } = string.Empty;
        public CamilaPage Page { get; set; } = new();
    }
}
