using System.Text;
using ATLAS.Infrastructure.Configuration;

namespace ATLAS.Infrastructure.Camila;

/// <summary>
/// Comando <c>--bot-generar [carpeta-del-export] [--id camila]</c> (spec 012; <c>--camila-generar</c> sigue funcionando como alias):
/// convierte <c>Bots/</c> en los playbooks maestros de <c>playbooks bots/&lt;id&gt;/</c> y en la narrativa
/// <c>bases de conocimiento/CamilaIA.md</c>, guarda las credenciales del export en <c>.env</c> (ignorado por git) y deja sus nombres,
/// sin valores, en <c>.env.example</c>. Es idempotente: no pisa secretos ya definidos ni <c>notas.md</c>.
/// </summary>
public static class CamilaGeneratorCommand
{
    /// <summary>Indica si los argumentos piden este comando.</summary>
    public static bool IsRequested(string[] args) =>
        args.Any(a => string.Equals(a, "--bot-generar", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "--camila-generar", StringComparison.OrdinalIgnoreCase));

    /// <summary>Ejecuta el comando y devuelve el código de salida.</summary>
    public static int Run(string[] args, TextWriter output)
    {
        var idAt = Array.FindIndex(args, a => string.Equals(a, "--id", StringComparison.OrdinalIgnoreCase));
        var botId = idAt >= 0 && idAt + 1 < args.Length ? args[idAt + 1] : "camila";
        var positional = args.Where((a, i) => !a.StartsWith("--", StringComparison.Ordinal) && !(idAt >= 0 && i == idAt + 1)).ToArray();

        var botsDir = positional.Length > 0 ? positional[0] : OfficeConfiguration.ResolveDirectory("Bots");
        if (botsDir is null || !Directory.Exists(botsDir))
        {
            output.WriteLine("No encuentro la carpeta Bots/ (export de Dialogflow CX). Indícala: --bot-generar <ruta>.");
            return 1;
        }

        var conversion = CamilaBotConverter.Convert(botsDir);
        var bot = conversion.Bot;

        // 1) Playbooks maestros del bot
        var meta = new BotPlaybookMeta(botId, botId == "camila" ? "Camila IA" : botId, "Asistente de recursos humanos de Outsourcing (export de Dialogflow CX: CamilaOS).",
            "🧩", "lector-uml", "camila-ia", "bases de conocimiento/CamilaIA.md", bot.StartFlow);
        var root = OfficeConfiguration.ResolveWritableDirectory("playbooks bots");
        var files = CamilaPlaybooks.Write(root, meta, bot);

        // 2) Narrativa de la base de conocimiento (sin bloques ejecutables)
        var knowledgeDir = OfficeConfiguration.ResolveWritableDirectory("bases de conocimiento");
        Directory.CreateDirectory(knowledgeDir);
        var mdPath = Path.Combine(knowledgeDir, "CamilaIA.md");
        File.WriteAllText(mdPath, CamilaMarkdown.Write(bot, includeBlocks: false), new UTF8Encoding(false));

        // 3) Credenciales
        var envPath = OfficeConfiguration.ResolveWritableDirectory(".env");
        var (added, kept) = AppendMissing(envPath, conversion.Secrets);
        var exampleAdded = AppendNames(OfficeConfiguration.ResolveWritableDirectory(".env.example"), conversion.Secrets.Keys);

        var pages = bot.Flows.Sum(f => f.Pages.Count);
        output.WriteLine($"✔ Playbooks: {Path.Combine(root, botId)}  ({files.Count} archivos; notas.md no se pisa)");
        output.WriteLine($"✔ Narrativa: {mdPath}");
        output.WriteLine($"  {bot.Flows.Count} flujos · {pages} páginas · {bot.Intents.Count} intents · {bot.Webhooks.Count} webhooks · {bot.Generators.Count} generadores");
        output.WriteLine($"✔ Secretos: {added} nuevos en .env, {kept} ya estaban definidos (no se sobrescriben); {exampleAdded} nombres añadidos a .env.example. Los valores no se muestran.");
        output.WriteLine("  Hosts de los webhooks (agrégalos a Camila:AllowedHosts solo si quieres que el modo Real los llame):");
        foreach (var host in conversion.Hosts) output.WriteLine($"    - {host}");
        foreach (var w in conversion.Warnings.Distinct().Take(20)) output.WriteLine($"  ⚠ {w}");
        foreach (var p in ATLAS.Application.Camila.CamilaBotValidator.Validate(bot).Take(10)) output.WriteLine($"  ⚠ referencia rota: {p}");
        output.WriteLine("  Recomendación: rota las credenciales del export original; estuvieron en texto plano.");
        return 0;
    }

    /// <summary>Agrega a .env las claves que no existan. Devuelve (nuevas, ya definidas).</summary>
    public static (int Added, int Kept) AppendMissing(string envPath, IReadOnlyDictionary<string, string> secrets)
    {
        var existing = File.Exists(envPath) ? File.ReadAllLines(envPath) : Array.Empty<string>();
        var defined = existing.Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith('#') && l.Contains('='))
            .Where(l => l[(l.IndexOf('=') + 1)..].Trim().Length > 0)
            .Select(l => l[..l.IndexOf('=')].Trim()).ToHashSet(StringComparer.Ordinal);

        var missing = secrets.Where(s => !defined.Contains(s.Key)).OrderBy(s => s.Key, StringComparer.Ordinal).ToList();
        if (missing.Count > 0)
        {
            var sb = new StringBuilder();
            if (existing.Length > 0 && existing[^1].Length > 0) sb.AppendLine();
            sb.AppendLine("# ---- Camila IA: credenciales de los webhooks del bot (generadas por --bot-generar; no se versionan) ----");
            foreach (var (name, value) in missing) sb.AppendLine($"{name}={value}");
            File.AppendAllText(envPath, sb.ToString(), new UTF8Encoding(false));
        }
        return (missing.Count, secrets.Count - missing.Count);
    }

    /// <summary>Agrega a .env.example los nombres de los secretos (sin valor).</summary>
    public static int AppendNames(string examplePath, IEnumerable<string> names)
    {
        var existing = File.Exists(examplePath) ? File.ReadAllText(examplePath) : string.Empty;
        var missing = names.Where(n => !existing.Contains(n + "=", StringComparison.Ordinal)).OrderBy(n => n, StringComparer.Ordinal).ToList();
        if (missing.Count == 0) return 0;

        var sb = new StringBuilder();
        if (existing.Length > 0 && !existing.EndsWith('\n')) sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("# ---- Camila IA: credenciales de los webhooks del bot (las escribe --bot-generar en tu .env) ----");
        foreach (var n in missing) sb.AppendLine($"{n}=");
        File.AppendAllText(examplePath, sb.ToString(), new UTF8Encoding(false));
        return missing.Count;
    }
}
