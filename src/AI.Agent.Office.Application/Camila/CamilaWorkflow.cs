using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using AI.Agent.Office.Application.Agents;
using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Application.Camila;

/// <summary>Respuesta de un entorno de Camila: el texto (Markdown) y, en el UML, los botones de la página.</summary>
public sealed record CamilaReply(string Reply, KnowledgeChoiceDto? Choice, bool IsError = false);

/// <summary>
/// A quién va un mensaje del chat (spec 012, RF-010): <c>bot:&lt;id&gt;</c> (LectorUML ejecuta el flujo UML del bot) o
/// <c>agent:&lt;id&gt;</c> (un agente conversacional). Sustituye a la «oficina activa» global para estos entornos.
/// </summary>
public sealed record CamilaTarget(CamilaMode Mode, string Id)
{
    /// <summary>Lee «bot:camila» o «agent:camila-ia»; null si el texto no es un destino válido.</summary>
    public static CamilaTarget? Parse(string? target)
    {
        if (string.IsNullOrWhiteSpace(target)) return null;
        var parts = target.Trim().Split(':', 2);
        if (parts.Length != 2 || !System.Text.RegularExpressions.Regex.IsMatch(parts[1], "^[a-z0-9][a-z0-9-]{0,60}$")) return null;
        return parts[0].ToLowerInvariant() switch
        {
            "bot" => new CamilaTarget(CamilaMode.Uml, parts[1]),
            "agent" => new CamilaTarget(CamilaMode.Conversational, parts[1]),
            _ => null
        };
    }

    public override string ToString() => (Mode == CamilaMode.Uml ? "bot:" : "agent:") + Id;
}

/// <summary>
/// Atiende el chat de los entornos de Camila (spec 009 y 012): <b>Chatbots</b> (el agente LectorUML ejecuta el playbook de un bot con el
/// motor de flujos) y <b>Agentes conversacionales</b> (CamilaIA conversa libremente con la base de conocimiento).
/// </summary>
public sealed class CamilaWorkflow
{
    private static readonly ConditionalWeakTable<CamilaBot, CamilaIndex> Indexes = new();

    private readonly ICamilaBotSource _source;
    private readonly CamilaSessionStore _store;
    private readonly ICamilaWebhookService _hooks;
    private readonly ICamilaGeneratorRunner _generators;
    private readonly IAnthropicService _llm;
    private readonly IOptions<CamilaOptions> _options;
    private readonly ILogger<CamilaWorkflow> _logger;
    private readonly IAgentConfigurationService? _agents;

    /// <summary>Crea el flujo. <paramref name="agents"/> es opcional: sin él se usan valores por defecto (pruebas).</summary>
    public CamilaWorkflow(ICamilaBotSource source, CamilaSessionStore store, ICamilaWebhookService hooks, ICamilaGeneratorRunner generators,
        IAnthropicService llm, IOptions<CamilaOptions> options, ILogger<CamilaWorkflow> logger, IAgentConfigurationService? agents = null)
    {
        _source = source;
        _store = store;
        _hooks = hooks;
        _generators = generators;
        _llm = llm;
        _options = options;
        _logger = logger;
        _agents = agents;
    }

    #region Resolución del destino

    /// <summary>Qué bot y qué agente atienden un destino, o el motivo por el que no se puede.</summary>
    private sealed record Resolved(CamilaBot? Bot, BotInfo? Info, AgentDefinition? Agent, string? Error);

    private async Task<Resolved> ResolveAsync(CamilaTarget target, CancellationToken ct)
    {
        if (_source.Bots.Count == 0) await _source.ReloadAsync(ct).ConfigureAwait(false);

        if (target.Mode == CamilaMode.Uml)
        {
            var bot = _source.Get(target.Id);
            var info = _source.Bots.FirstOrDefault(b => string.Equals(b.Id, target.Id, StringComparison.OrdinalIgnoreCase));
            if (bot is null || info is null)
                return new Resolved(null, null, null, $"No encuentro el playbook del bot «{target.Id}» en `playbooks bots/`. Genéralo arrancando la aplicación con `--bot-generar` (guía: docs/camila-ia.md).");

            var executor = _agents?.GetDefinition(info.Executor);
            if (_agents is not null && executor is null) return new Resolved(null, null, null, $"Falta el agente ejecutor «{info.Executor}» (créalo en la sección Agentes).");
            return new Resolved(bot, info, executor, null);
        }

        var agent = _agents?.GetDefinition(target.Id);
        if (_agents is not null && (agent is null || agent.Kind != AgentKinds.Llm || agent.Environment != AgentEnvironments.Conversational))
            return new Resolved(null, null, null, $"El agente «{target.Id}» no existe o no es un agente conversacional con modelo.");

        // El bot que usa como conocimiento: el que declara el agente (knowledge) o el que lo nombra como su agente.
        var botId = agent?.Knowledge;
        var botInfo = _source.Bots.FirstOrDefault(b => !string.IsNullOrEmpty(botId) && string.Equals(b.Id, botId, StringComparison.OrdinalIgnoreCase))
            ?? _source.Bots.FirstOrDefault(b => string.Equals(b.Agent, target.Id, StringComparison.OrdinalIgnoreCase))
            ?? _source.Bots.FirstOrDefault();
        var knowledge = botInfo is null ? null : _source.Get(botInfo.Id);
        if (knowledge is null || botInfo is null)
            return new Resolved(null, null, null, "No hay ninguna base de conocimiento cargada para este agente: genera los playbooks con `--bot-generar` (guía: docs/camila-ia.md).");
        return new Resolved(knowledge, botInfo, agent, null);
    }

    /// <summary>Mensaje de error si el destino no se puede atender (para validar antes de empezar); null si es válido.</summary>
    public async Task<string?> ValidateAsync(CamilaTarget target, CancellationToken ct) => (await ResolveAsync(target, ct).ConfigureAwait(false)).Error;

    /// <summary>Id del agente que ejecuta el destino (LectorUML para un bot, el propio agente para uno conversacional).</summary>
    public string ExecutorOf(CamilaTarget target) =>
        target.Mode == CamilaMode.Conversational ? target.Id : _source.Bots.FirstOrDefault(b => string.Equals(b.Id, target.Id, StringComparison.OrdinalIgnoreCase))?.Executor ?? SystemAgents.LectorUml;

    #endregion

    #region Entrada

    /// <summary>Atiende un mensaje de la persona dirigido a un bot o a un agente conversacional.</summary>
    public async Task<CamilaReply> HandleAsync(CamilaTarget target, Guid conversationId, string text, string? optionId, CancellationToken ct)
    {
        var settings = _options.Value;
        if (!settings.Enabled) return new CamilaReply("Camila IA está desactivada (Camila:Enabled).", null, true);

        var resolved = await ResolveAsync(target, ct).ConfigureAwait(false);
        if (resolved.Error is not null || resolved.Bot is null) return new CamilaReply(resolved.Error ?? "No se pudo abrir el entorno.", null, true);
        var bot = resolved.Bot;

        var pruned = _store.Prune(TimeSpan.FromMinutes(settings.SessionMinutes));
        if (pruned > 0) _logger.LogInformation("Camila · se descartaron {Count} conversación(es) inactivas", pruned);
        var mode = target.Mode;
        var engine = new CamilaEngine(bot, _hooks, _generators, _options);

        var session = _store.Find(conversationId, mode);
        var isNew = session is null;
        session ??= _store.Add(new CamilaSession(conversationId, mode));

        await session.Gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (mode == CamilaMode.Uml)
            {
                // Las herramientas del agente ejecutor se imponen (spec 011, RF-010): sin escribir_servicio no se escribe.
                if (resolved.Agent is { } executor)
                {
                    session.AllowReadServices = executor.Allows(AgentTools.ReadService);
                    session.AllowWriteServices = executor.Allows(AgentTools.WriteService);
                }

                if (isNew)
                {
                    var first = await engine.StartAsync(session, ct).ConfigureAwait(false);
                    return RenderUml(bot, engine, session, first);
                }
                return await HandleUmlAsync(bot, engine, session, text, optionId, ct).ConfigureAwait(false);
            }

            // La Conversacional NO sigue los flujos del bot: es un agente libre que conversa apoyándose en la base de conocimiento
            // (ver CamilaChatAgent). Solo el UML usa el motor de flujos.
            var chat = new CamilaChatAgent(bot, resolved.Info?.Id, _source, resolved.Agent, _hooks, _llm, _options, _logger);
            return await chat.HandleAsync(session, text, ct).ConfigureAwait(false);
        }
        finally
        {
            session.Gate.Release();
        }
    }

    #endregion

    #region Camila IA UML

    private async Task<CamilaReply> HandleUmlAsync(CamilaBot bot, CamilaEngine engine, CamilaSession session, string text, string? optionId, CancellationToken ct)
    {
        text = text.Trim();

        // Confirmación de un webhook de escritura.
        if (session.PendingConfirm is not null)
        {
            var yes = optionId == "yes" || (optionId is null && IsYes(text));
            var no = optionId == "no" || (optionId is null && IsNo(text));
            if (!yes && !no) return Render(bot, engine, session, "Responde **sí** o **no**: el flujo está esperando tu confirmación para modificar datos.\n\n" + CamilaUmlView.ConfirmCard(session.PendingConfirm), null);
            return RenderUml(bot, engine, session, await engine.ConfirmAsync(session, yes, ct).ConfigureAwait(false));
        }

        if (text.StartsWith('/')) return await UmlCommandAsync(bot, engine, session, text, ct).ConfigureAwait(false);

        // Botón: el id es la posición (1..n) en las opciones de la última respuesta.
        var option = ResolveOption(session, text, optionId);
        if (option is { Event: { } ev }) return RenderUml(bot, engine, session, await engine.UserEventAsync(session, ev, ct).ConfigureAwait(false));

        return RenderUml(bot, engine, session, await engine.UserTextAsync(session, text, ct).ConfigureAwait(false));
    }

    private async Task<CamilaReply> UmlCommandAsync(CamilaBot bot, CamilaEngine engine, CamilaSession session, string text, CancellationToken ct)
    {
        var parts = text.Split(' ', 2, StringSplitOptions.TrimEntries);
        switch (parts[0].ToLowerInvariant())
        {
            case "/jerarquia" or "/jerarquía" or "/arbol" or "/árbol":
                return Render(bot, engine, session, CamilaUmlView.Hierarchy(bot, session.Flow, session.Page), null);

            case "/ficha":
                return Render(bot, engine, session, string.Empty, null, withCard: true);

            case "/vars":
                var vars = session.Params.Where(p => p.Value is not null).OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(p => $"- `{p.Key}` = {Shorten(CamilaExpression.ToText(p.Value), 70)}");
                return Render(bot, engine, session, "#### 🧮 Variables de la conversación (solo en memoria)\n\n" + (session.Params.Count == 0 ? "_Ninguna todavía._" : string.Join("\n", vars)) + "\n\n", null);

            case "/reiniciar":
                _store.Remove(session.ConversationId);
                var fresh = _store.Add(new CamilaSession(session.ConversationId, CamilaMode.Uml));
                return RenderUml(bot, engine, fresh, await engine.StartAsync(fresh, ct).ConfigureAwait(false), "🔄 Conversación reiniciada: se descartaron todas las variables.\n\n");

            case "/ir":
                if (parts.Length < 2) return Render(bot, engine, session, "Indica a dónde ir: `/ir <flujo> › <página>`.\n\n", null);
                var (flow, page, options) = FindTarget(bot, parts[1]);
                if (flow is null) return Render(bot, engine, session, options.Count == 0 ? "No encontré ese flujo. Usa `/jerarquia` para verlos.\n\n" : "Hay varios que coinciden, sé más específico:\n" + string.Join("\n", options.Select(o => "- " + o)) + "\n\n", null);
                return RenderUml(bot, engine, session, await engine.GoToAsync(session, flow.Name, page?.Name, ct).ConfigureAwait(false), $"➡️ Salto a **{flow.Name}{(page is null ? "" : " › " + page.Name)}** (las variables se conservan).\n\n");

            default:
                return Render(bot, engine, session, "Comandos: `/jerarquia` · `/ir <flujo> › <página>` · `/ficha` · `/vars` · `/reiniciar`. Para recorrer el bot, pulsa sus botones o escribe como lo haría una persona.\n\n", null);
        }
    }

    /// <summary>Busca «flujo › página» por coincidencia flexible (sin acentos ni mayúsculas, por fragmentos).</summary>
    private static (CamilaFlow? Flow, CamilaPage? Page, List<string> Candidates) FindTarget(CamilaBot bot, string query)
    {
        var segments = query.Split(new[] { '›', '>' }, 2, StringSplitOptions.TrimEntries);
        var flows = bot.Flows.Where(f => Matches(f.Name, segments[0])).ToList();
        var exact = flows.Where(f => Norm(f.Name) == Norm(segments[0])).ToList();
        if (exact.Count == 1) flows = exact;
        if (flows.Count != 1) return (null, null, flows.Select(f => f.Name).Take(8).ToList());

        var flow = flows[0];
        if (segments.Length < 2) return (flow, null, new List<string>());
        var pages = flow.Pages.Where(p => Matches(p.Name, segments[1])).ToList();
        return pages.Count >= 1 ? (flow, pages[0], new List<string>()) : (null, null, flow.Pages.Select(p => flow.Name + " › " + p.Name).Take(8).ToList());
    }

    private static bool Matches(string name, string query) => Norm(name).Contains(Norm(query), StringComparison.Ordinal);

    private static string Norm(string s) => string.Join(' ', CamilaEngine.Tokens(s).OrderBy(x => x, StringComparer.Ordinal));

    /// <summary>
    /// Registra lo que hizo el motor en el turno (ruta, página, generador, avisos). Solo nombres: nunca valores de parámetros, textos de la
    /// persona ni respuestas del bot (spec 010, RF-013 y RF-014). Los webhooks ya se registran en <see cref="CamilaWebhookService"/>.
    /// </summary>
    private void LogTurn(CamilaSession session, CamilaTurn turn)
    {
        var mode = session.Mode == CamilaMode.Uml ? "UML" : "Conversacional";
        foreach (var t in turn.Trace.Where(t => t.Kind != "webhook"))
            _logger.LogInformation("Camila {Mode} · {Kind} · {Text}", mode, t.Kind, t.Text);
        if (turn.Options.Count > 0) _logger.LogInformation("Camila {Mode} · el bot ofrece {Count} botón(es)", mode, turn.Options.Count);
        if (turn.Confirm is { } c) _logger.LogWarning("Camila {Mode} · espera confirmación para el webhook de escritura «{Webhook}»", mode, c.Webhook);
        if (turn.Ended) _logger.LogInformation("Camila {Mode} · la sesión del bot terminó", mode);
    }

    /// <summary>
    /// Lo que ve la persona en la consola de pruebas: solo lo que el bot muestra (textos, tarjetas, imágenes, enlaces y botones). El detalle
    /// técnico (página, rutas, datos que espera, webhooks, avisos) va al registro de la izquierda, no al chat.
    /// </summary>
    private CamilaReply RenderUml(CamilaBot bot, CamilaEngine engine, CamilaSession session, CamilaTurn turn, string prefix = "")
    {
        LogTurn(session, turn);
        var sb = new StringBuilder(prefix);
        sb.Append(CamilaUmlView.BotMessages(turn));
        if (turn.Ended) sb.Append("🔚 _La conversación terminó. Escribe `/reiniciar` para empezar de nuevo._\n\n");
        if (turn.Confirm is { } confirm) sb.Append(CamilaUmlView.ConfirmCard(confirm));
        if (sb.Length == 0 && turn.Options.Count == 0) sb.Append("_El bot no mostró nada en este paso (mira el registro)._");
        return Render(bot, engine, session, sb.ToString(), turn);
    }

    /// <summary>Arma los botones y, solo si se pide (<c>/ficha</c>), agrega la ficha técnica de la página actual.</summary>
    private CamilaReply Render(CamilaBot bot, CamilaEngine engine, CamilaSession session, string body, CamilaTurn? turn, bool withCard = false)
    {
        var sb = new StringBuilder(body);
        if (withCard && bot.FindFlow(session.Flow) is { } flow)
            sb.Append(CamilaUmlView.Card(bot, flow, engine.CurrentPage(session), _hooks));

        var options = new List<SolutionOptionDto>();
        if (session.PendingConfirm is not null)
        {
            options.Add(new SolutionOptionDto("yes", "Sí, enviar", "Ejecuta el webhook de escritura", true));
            options.Add(new SolutionOptionDto("no", "No, cancelar", "No se envía nada", false));
        }
        else
        {
            var index = 0;
            foreach (var o in session.LastOptions)
            {
                index++;
                if (o.Event is not null) options.Add(new SolutionOptionDto(index.ToString(), o.Label, o.Event, false));
            }
        }

        var choice = options.Count == 0 ? null : new KnowledgeChoiceDto("CAMILA", session.Page ?? session.Flow, null, options, MenuOnly: true);
        return new CamilaReply(sb.ToString().TrimEnd() + "\n", choice);
    }

    private static CamilaOption? ResolveOption(CamilaSession session, string text, string? optionId)
    {
        var raw = optionId ?? text;
        if (int.TryParse(raw, out var n) && n >= 1 && n <= session.LastOptions.Count) return session.LastOptions[n - 1];
        return null;
    }

    #endregion

    #region Utilidades

    internal static bool IsYes(string text) => CamilaEngine.Tokens(text).Overlaps(new[] { "si", "sii", "confirmo", "dale", "ok", "adelante", "claro", "enviar", "envialo", "procede", "yes" });

    internal static bool IsNo(string text) => CamilaEngine.Tokens(text).Overlaps(new[] { "no", "cancelar", "cancela", "cancelo", "detente" });

    internal static string Shorten(string s, int max) => s.Length <= max ? s : s[..max] + "…";

    #endregion
}

/// <summary>Índice de palabras por flujo para recuperar solo los flujos relevantes a un mensaje (la base completa no cabe en una consulta).</summary>
public sealed class CamilaIndex
{
    private readonly List<(CamilaFlow Flow, Dictionary<string, double> Weights)> _entries = new();
    private readonly CamilaBot _bot;

    public CamilaIndex(CamilaBot bot)
    {
        _bot = bot;
        foreach (var flow in bot.Flows)
        {
            var weights = new Dictionary<string, double>();
            void Add(string text, double w)
            {
                foreach (var t in CamilaEngine.Tokens(text).Where(t => t.Length > 2)) weights[Stem(t)] = Math.Max(weights.GetValueOrDefault(Stem(t)), w);
            }

            Add(flow.Name, 4);
            foreach (var page in flow.Pages)
            {
                Add(page.Name, 2);
                foreach (var m in page.Entry.Messages) foreach (var t in m.Texts) Add(t, 1);
                foreach (var r in page.Routes.Concat(page.Events).Concat(flow.Routes)) AddIntent(r.Intent, Add);
            }
            foreach (var r in flow.Routes.Concat(flow.Events)) AddIntent(r.Intent, Add);
            _entries.Add((flow, weights));
        }
    }

    private void AddIntent(string? intent, Action<string, double> add)
    {
        if (intent is null) return;
        add(intent, 3);
        if (_bot.FindIntent(intent) is { } i) foreach (var p in i.Phrases.Take(8)) add(p, 2);
    }

    private static string Stem(string t) => t.Length > 5 ? t[..5] : t;

    /// <summary>Los flujos que mejor coinciden con el texto.</summary>
    public IReadOnlyList<CamilaFlow> Search(string text, int max)
    {
        var query = CamilaEngine.Tokens(text).Where(t => t.Length > 2).Select(Stem).ToHashSet();
        return _entries.Select(e => (e.Flow, Score: query.Sum(q => e.Weights.GetValueOrDefault(q))))
            .Where(t => t.Score > 0).OrderByDescending(t => t.Score).ThenBy(t => t.Flow.Name, StringComparer.Ordinal).Take(max).Select(t => t.Flow).ToList();
    }

    /// <summary>Resumen corto del flujo para el modelo: nombre, páginas y los primeros textos.</summary>
    public string Digest(CamilaFlow flow)
    {
        var texts = flow.Pages.SelectMany(p => p.Entry.Messages.Where(m => m.Type == "text").Select(m => m.Texts[0]))
            .Select(t => t.Replace("\n", " ")).Where(t => t.Length > 12).Take(3).Select(t => t.Length > 200 ? t[..200] + "…" : t);
        return $"- «{flow.Name}» · páginas: {string.Join(", ", flow.Pages.Select(p => p.Name).Take(10))} · textos: {string.Join(" / ", texts)}";
    }
}
