using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ATLAS.Application.Camila;
using ATLAS.Infrastructure.Agents;
using ATLAS.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATLAS.Infrastructure.Camila;

/// <summary>Datos del maestro <c>bot.md</c>.</summary>
public sealed record BotPlaybookMeta(string Id, string Name, string Description, string Icon, string Executor, string Agent, string Knowledge, string StartFlow);

/// <summary>Un bot ya leído de su carpeta de playbooks.</summary>
public sealed record LoadedBot(BotInfo Info, CamilaBot Bot, string? Notes, Dictionary<string, string>? Narrative, IReadOnlyList<string> Problems, string Knowledge);

/// <summary>
/// Playbooks maestros de bots (spec 012): <c>playbooks bots/&lt;bot&gt;/</c> con el maestro <c>bot.md</c>, un playbook por flujo en
/// <c>flujos/</c>, los catálogos en <c>catalogos/</c> y las <c>notas.md</c> escritas a mano. El generador reescribe todo menos las notas.
/// </summary>
public static partial class CamilaPlaybooks
{
    public const string MasterFile = "bot.md";
    public const string NotesFile = "notas.md";
    public const string FlowsDirectory = "flujos";
    public const string CatalogsDirectory = "catalogos";

    private static readonly Dictionary<string, string> CatalogFiles = new()
    {
        ["groups"] = "grupos.md", ["intents"] = "intents.md", ["entities"] = "entidades.md", ["generators"] = "generadores.md", ["webhooks"] = "webhooks.md"
    };

    [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex HtmlComment();

    #region Escritura

    /// <summary>
    /// Escribe (o regenera) la carpeta del bot. Borra y vuelve a escribir <c>flujos/</c> y <c>catalogos/</c> y el maestro; <c>notas.md</c> solo
    /// se crea si no existe. Devuelve los archivos escritos (relativos a la carpeta del bot).
    /// </summary>
    public static IReadOnlyList<string> Write(string botsRoot, BotPlaybookMeta meta, CamilaBot bot)
    {
        var dir = Path.Combine(botsRoot, meta.Id);
        var flowsDir = Path.Combine(dir, FlowsDirectory);
        var catalogsDir = Path.Combine(dir, CatalogsDirectory);
        foreach (var d in new[] { flowsDir, catalogsDir })
        {
            Directory.CreateDirectory(d);
            foreach (var old in Directory.EnumerateFiles(d, "*.md")) File.Delete(old);
        }

        var written = new List<string>();
        var index = CamilaNarrative.Build(bot);
        var utf8 = new UTF8Encoding(false);
        void Save(string relative, string text)
        {
            File.WriteAllText(Path.Combine(dir, relative), text.Replace("\r\n", "\n"), utf8);
            written.Add(relative.Replace('\\', '/'));
        }

        var width = bot.Flows.Count >= 100 ? 3 : 2;
        var flowFiles = new List<(string Name, string File, int Pages)>();
        for (var i = 0; i < bot.Flows.Count; i++)
        {
            var f = bot.Flows[i];
            var file = $"{FlowsDirectory}/{(i + 1).ToString("D" + width, CultureInfo.InvariantCulture)}-{Slugify(f.Name)}.md";
            flowFiles.Add((f.Name, file, f.Pages.Count));
            Save(file, $"---\nbot: {meta.Id}\nflow: {OneLine(f.Name)}\n---\n" + CamilaMarkdown.FlowSection(bot, index, f));
        }
        foreach (var kind in CamilaMarkdown.Catalogs)
            Save($"{CatalogsDirectory}/{CatalogFiles[kind]}", $"---\nbot: {meta.Id}\ncatalog: {kind}\n---\n" + CamilaMarkdown.CatalogSection(bot, index, kind));

        // Maestro
        var sb = new StringBuilder("---\n");
        sb.Append($"id: {meta.Id}\nname: {OneLine(meta.Name)}\ndescription: {OneLine(meta.Description)}\nicon: {meta.Icon}\n");
        sb.Append($"executor: {meta.Executor}\nagent: {meta.Agent}\nstartFlow: {OneLine(meta.StartFlow)}\nknowledge: {OneLine(meta.Knowledge)}\nsource: Bots/ (export de Dialogflow CX)\n---\n");
        sb.Append($"# Bot · {meta.Name} — playbook maestro\n\n");
        sb.Append($"{meta.Description}\n\n");
        sb.Append($"Este bot lo **ejecuta el agente `{meta.Executor}`** (sin modelo de lenguaje) en el entorno de pruebas de *Chatbots*, y lo **conversa el agente `{meta.Agent}`** apoyándose en la base de conocimiento `{meta.Knowledge}`.\n");
        sb.Append("Generado desde el export del bot; **no edites estos archivos** (se regeneran). Tus notas van en [`notas.md`](notas.md): nunca se pisan.\n\n");
        sb.Append(CamilaNarrative.Overview(bot, index).Replace("## Índice por flujo", "## Índice por flujo (resumen)"));
        sb.Append("## Playbooks de este bot\n\n");
        foreach (var (name, file, pages) in flowFiles) sb.Append($"- [{name}]({file}) — {pages} página(s)\n");
        sb.Append("\nCatálogos: ");
        sb.Append(string.Join(" · ", CamilaMarkdown.Catalogs.Select(k => $"[{CatalogFiles[k]}]({CatalogsDirectory}/{CatalogFiles[k]})")));
        sb.Append("\n");
        Save(MasterFile, sb.ToString());

        // Notas: se crean una sola vez y nunca se pisan.
        var notes = Path.Combine(dir, NotesFile);
        if (!File.Exists(notes))
        {
            File.WriteAllText(notes, NotesTemplate(meta), utf8);
            written.Add(NotesFile);
        }
        return written;
    }

    private static string NotesTemplate(BotPlaybookMeta meta) =>
        $"# Notas de {meta.Name}\n\n" +
        "Escribe aquí lo que quieras que el agente conversacional sepa y que **no sale del export**: tono, frases de la marca, preguntas frecuentes, avisos del momento.\n" +
        "Este archivo **nunca se sobrescribe** al regenerar el playbook y se entrega como contexto al agente.\n\n" +
        "<!-- Ejemplo (borra este comentario y escribe las tuyas):\n## Tono\nCercano y breve; tutea a la persona.\n\n## Preguntas frecuentes\n- ¿Hasta cuándo puedo pedir mi certificado? → ...\n-->\n";

    private static string OneLine(string s) => s.Replace("\r", " ").Replace("\n", " ").Trim();

    /// <summary>Nombre de archivo seguro: minúsculas, sin acentos, guiones.</summary>
    public static string Slugify(string name)
    {
        var sb = new StringBuilder();
        foreach (var ch in name.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(char.IsAsciiLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : '-');
        }
        var slug = Regex.Replace(sb.ToString(), "-+", "-").Trim('-');
        return slug.Length == 0 ? "flujo" : slug.Length > 60 ? slug[..60].TrimEnd('-') : slug;
    }

    #endregion

    #region Lectura

    /// <summary>Lee la carpeta de un bot. Devuelve null si no tiene un maestro válido.</summary>
    public static LoadedBot? Load(string botDir)
    {
        var master = Path.Combine(botDir, MasterFile);
        if (!File.Exists(master)) return null;

        var (meta, _) = MarkdownAgentConfigurationService.SplitFrontMatter(File.ReadAllText(master, Encoding.UTF8));
        var id = meta.GetValueOrDefault("id") ?? Path.GetFileName(botDir);
        if (!Regex.IsMatch(id, "^[a-z0-9][a-z0-9-]*$")) return null;

        var bot = new CamilaBot { StartFlow = meta.GetValueOrDefault("startFlow") ?? "Default Start Flow" };
        foreach (var sub in new[] { FlowsDirectory, CatalogsDirectory })
        {
            var d = Path.Combine(botDir, sub);
            if (!Directory.Exists(d)) continue;
            foreach (var file in Directory.EnumerateFiles(d, "*.md").OrderBy(f => f, StringComparer.Ordinal))
                CamilaMarkdown.ReadInto(bot, File.ReadAllText(file, Encoding.UTF8));
        }

        var notes = Path.Combine(botDir, NotesFile);
        var notesText = File.Exists(notes) ? HtmlComment().Replace(File.ReadAllText(notes, Encoding.UTF8), string.Empty).Trim() : null;
        // Solo el título y el texto de ayuda: no hay nada escrito todavía.
        if (notesText is not null && !notesText.Split('\n').Any(l => l.Trim().Length > 0 && !l.StartsWith('#') && !l.StartsWith("Escribe aquí") && !l.StartsWith("Este archivo **nunca"))) notesText = null;

        var info = new BotInfo(id, meta.GetValueOrDefault("name") ?? id, meta.GetValueOrDefault("description") ?? string.Empty, meta.GetValueOrDefault("icon") ?? "🧩",
            meta.GetValueOrDefault("executor") ?? "lector-uml", meta.GetValueOrDefault("agent") ?? string.Empty,
            bot.Flows.Count, bot.Flows.Sum(f => f.Pages.Count), bot.Webhooks.Count);
        return new LoadedBot(info, bot, notesText, null, CamilaBotValidator.Validate(bot), meta.GetValueOrDefault("knowledge") ?? string.Empty);
    }

    /// <summary>Divide la narrativa de la base de conocimiento por flujo (nombre del flujo → texto de su sección).</summary>
    public static Dictionary<string, string> ParseNarrative(string markdown)
    {
        var sections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? name = null;
        var body = new StringBuilder();
        void Flush()
        {
            if (name is not null) sections[name] = body.ToString().Trim();
            body.Clear();
        }

        foreach (var line in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                Flush();
                name = line.StartsWith("## Flujo · ", StringComparison.Ordinal) ? line["## Flujo · ".Length..].Trim() : null;
            }
            if (name is not null) body.AppendLine(line);
        }
        Flush();
        return sections;
    }

    #endregion
}

/// <summary>
/// Carga los bots de <c>playbooks bots/</c> y entrega a LectorUML y a CamilaIA su playbook, sus notas y la narrativa de la base de
/// conocimiento (spec 012).
/// </summary>
public sealed class BotPlaybookSource : ICamilaBotSource
{
    private readonly IOptions<CamilaOptions> _options;
    private readonly ILogger<BotPlaybookSource> _logger;
    private volatile Dictionary<string, LoadedBot> _bots = new(StringComparer.OrdinalIgnoreCase);

    public BotPlaybookSource(IOptions<CamilaOptions> options, ILogger<BotPlaybookSource> logger)
    {
        _options = options;
        _logger = logger;
    }

    public IReadOnlyList<BotInfo> Bots => _bots.Values.Select(b => b.Info).OrderBy(b => b.Name, StringComparer.OrdinalIgnoreCase).ToList();

    public CamilaBot? Get(string botId) => _bots.GetValueOrDefault(botId)?.Bot;

    public string? Notes(string botId) => _bots.GetValueOrDefault(botId)?.Notes;

    public IReadOnlyDictionary<string, string>? Narrative(string botId) => _bots.GetValueOrDefault(botId)?.Narrative;

    public IReadOnlyList<string> Problems(string botId) => _bots.GetValueOrDefault(botId)?.Problems ?? Array.Empty<string>();

    /// <summary>Relee todas las carpetas de bots; un bot con problemas se carga igual y los avisos quedan en el registro.</summary>
    public async Task<int> ReloadAsync(CancellationToken cancellationToken = default)
    {
        var root = OfficeConfiguration.ResolveDirectory(_options.Value.BotsDirectory);
        var loaded = new Dictionary<string, LoadedBot>(StringComparer.OrdinalIgnoreCase);

        if (root is null) _logger.LogWarning("No se encontró la carpeta de playbooks de bots «{Dir}». Genera los playbooks con --bot-generar.", _options.Value.BotsDirectory);
        else
        {
            foreach (var dir in Directory.EnumerateDirectories(root).OrderBy(d => d, StringComparer.Ordinal))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var bot = await Task.Run(() => CamilaPlaybooks.Load(dir), cancellationToken).ConfigureAwait(false);
                if (bot is null) continue;

                var narrative = ReadNarrative(bot.Knowledge);
                bot = bot with { Narrative = narrative };
                loaded[bot.Info.Id] = bot;
                _logger.LogInformation("Bot «{Bot}» cargado: {Flows} flujos, {Pages} páginas, {Webhooks} webhooks.", bot.Info.Id, bot.Info.Flows, bot.Info.Pages, bot.Info.Webhooks);
                foreach (var problem in bot.Problems.Take(10)) _logger.LogWarning("Bot «{Bot}»: {Problem}", bot.Info.Id, problem);
            }
        }

        _bots = loaded;
        return loaded.Count;
    }

    private static Dictionary<string, string>? ReadNarrative(string knowledge)
    {
        if (string.IsNullOrWhiteSpace(knowledge)) return null;
        var file = OfficeConfiguration.FindFile(knowledge.Replace('/', Path.DirectorySeparatorChar));
        return file is null ? null : CamilaPlaybooks.ParseNarrative(File.ReadAllText(file, Encoding.UTF8));
    }
}
