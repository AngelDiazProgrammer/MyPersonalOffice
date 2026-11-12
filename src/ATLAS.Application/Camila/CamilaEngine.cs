using System.Globalization;
using ATLAS.Application.Agents;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace ATLAS.Application.Camila;

/// <summary>Ejecuta un generador de IA de Dialogflow con sus entradas y devuelve el texto producido.</summary>
public interface ICamilaGeneratorRunner
{
    Task<string> RunAsync(CamilaGeneratorDef generator, IReadOnlyDictionary<string, string> inputs, CancellationToken cancellationToken);
}

/// <summary>
/// Intérprete de los flujos del bot (semántica de Dialogflow CX acotada a lo que usa CamilaOS): páginas, formularios, rutas por
/// intent, evento o condición, grupos de rutas, webhooks y generadores. Lo comparten Camila IA UML y Camila IA Conversacional.
/// Trabaja con una cola de acciones para poder pausarse antes de un webhook de escritura y retomarse tras la confirmación.
/// </summary>
public sealed partial class CamilaEngine
{
    private const string NoMatchEvent = "sys.no-match-default";

    private readonly CamilaBot _bot;
    private readonly ICamilaWebhookService _hooks;
    private readonly ICamilaGeneratorRunner _generators;
    private readonly CamilaOptions _options;

    /// <summary>Crea el motor sobre un bot ya cargado.</summary>
    public CamilaEngine(CamilaBot bot, ICamilaWebhookService hooks, ICamilaGeneratorRunner generators, IOptions<CamilaOptions> options)
    {
        _bot = bot;
        _hooks = hooks;
        _generators = generators;
        _options = options.Value;
    }

    /// <summary>El bot que interpreta.</summary>
    public CamilaBot Bot => _bot;

    #region Entradas del usuario

    /// <summary>Abre la conversación como la abriría una persona: pasa por la ruta del intent de bienvenida del flujo inicial.</summary>
    public async Task<CamilaTurn> StartAsync(CamilaSession session, CancellationToken ct)
    {
        var turn = new CamilaTurn();
        session.Flow = _bot.StartFlow;
        session.Page = null;
        session.Params["idSessionWatson"] = session.ConversationId.ToString("N");

        var welcome = ScopeRoutes(session).FirstOrDefault(r => string.Equals(r.Intent, "Default Welcome Intent", StringComparison.OrdinalIgnoreCase));
        if (welcome is not null) EnqueueRoute(session, welcome, "Default Welcome Intent");
        else Enqueue(session, new AutoRouteAction());
        await RunAsync(session, turn, ct).ConfigureAwait(false);
        return Finish(session, turn);
    }

    /// <summary>La persona pulsó un botón: se busca el manejador del evento en la página, sus grupos y el flujo.</summary>
    public async Task<CamilaTurn> UserEventAsync(CamilaSession session, string eventName, CancellationToken ct)
    {
        var turn = new CamilaTurn();
        BeginTurn(session, eventName);
        var route = ScopeEvents(session).FirstOrDefault(r => string.Equals(r.Event, eventName, StringComparison.OrdinalIgnoreCase));
        if (route is null)
        {
            turn.Trace.Add(new("aviso", $"El evento «{eventName}» no tiene manejador en esta página."));
            await HandleNoMatchAsync(session, turn, ct).ConfigureAwait(false);
        }
        else
        {
            turn.Trace.Add(new("evento", $"Evento «{eventName}» → {Describe(route)}"));
            EnqueueRoute(session, route, "evento " + eventName);
            await RunAsync(session, turn, ct).ConfigureAwait(false);
        }
        return Finish(session, turn);
    }

    /// <summary>La persona escribió un intent ya identificado (lo usa la oficina Conversacional).</summary>
    public async Task<CamilaTurn> UserIntentAsync(CamilaSession session, string intentName, string userText, CancellationToken ct)
    {
        var turn = new CamilaTurn();
        BeginTurn(session, userText);
        var route = ScopeRoutes(session).FirstOrDefault(r => string.Equals(r.Intent, intentName, StringComparison.OrdinalIgnoreCase) && Passes(session, r));
        if (route is null)
        {
            turn.Trace.Add(new("aviso", $"El intent «{intentName}» no aplica aquí."));
            await HandleNoMatchAsync(session, turn, ct).ConfigureAwait(false);
        }
        else
        {
            turn.Trace.Add(new("ruta", $"Intent «{intentName}» → {Describe(route)}"));
            EnqueueRoute(session, route, "intent " + intentName);
            await RunAsync(session, turn, ct).ConfigureAwait(false);
        }
        return Finish(session, turn);
    }

    /// <summary>
    /// Texto libre: si coincide con un intent de la página lo toma; si no y hay un parámetro esperado, lo llena; si no, no-match.
    /// </summary>
    public async Task<CamilaTurn> UserTextAsync(CamilaSession session, string text, CancellationToken ct)
    {
        var turn = new CamilaTurn();
        BeginTurn(session, text);

        var matched = MatchIntent(session, text);
        if (matched is not null)
        {
            turn.Trace.Add(new("ruta", $"Texto → intent «{matched.Intent}» → {Describe(matched)}"));
            session.WaitingParam = null;
            EnqueueRoute(session, matched, "intent " + matched.Intent);
            await RunAsync(session, turn, ct).ConfigureAwait(false);
            return Finish(session, turn);
        }

        if (session.WaitingParam is { } waiting)
        {
            var form = CurrentPage(session)?.Form.FirstOrDefault(f => string.Equals(f.Name, waiting, StringComparison.OrdinalIgnoreCase));
            if (TryExtract(form?.Entity ?? "@sys.any", text, out var value))
            {
                session.Params[waiting] = value;
                session.WaitingParam = null;
                turn.Trace.Add(new("param", $"Parámetro «{waiting}» recibido."));
                Enqueue(session, new FormAction(), new AutoRouteAction());
                await RunAsync(session, turn, ct).ConfigureAwait(false);
                return Finish(session, turn);
            }
        }

        await HandleNoMatchAsync(session, turn, ct).ConfigureAwait(false);
        return Finish(session, turn);
    }

    /// <summary>Entrega un valor a un parámetro esperado, ya extraído (lo usa la oficina Conversacional).</summary>
    public async Task<CamilaTurn> ProvideValueAsync(CamilaSession session, string value, CancellationToken ct)
    {
        var turn = new CamilaTurn();
        BeginTurn(session, value);
        if (session.WaitingParam is not { } waiting)
        {
            turn.Trace.Add(new("aviso", "No hay ningún dato pendiente."));
            return Finish(session, turn);
        }

        var form = CurrentPage(session)?.Form.FirstOrDefault(f => string.Equals(f.Name, waiting, StringComparison.OrdinalIgnoreCase));
        if (!TryExtract(form?.Entity ?? "@sys.any", value, out var parsed))
        {
            turn.Trace.Add(new("aviso", $"El valor no cumple el formato de «{waiting}»."));
            await HandleNoMatchAsync(session, turn, ct).ConfigureAwait(false);
            return Finish(session, turn);
        }
        session.Params[waiting] = parsed;
        session.WaitingParam = null;
        turn.Trace.Add(new("param", $"Parámetro «{waiting}» recibido."));
        Enqueue(session, new FormAction(), new AutoRouteAction());
        await RunAsync(session, turn, ct).ConfigureAwait(false);
        return Finish(session, turn);
    }

    /// <summary>Salta a un flujo (y página). Solo para navegar y probar desde el UML.</summary>
    public async Task<CamilaTurn> GoToAsync(CamilaSession session, string flow, string? page, CancellationToken ct)
    {
        var turn = new CamilaTurn();
        BeginTurn(session, string.Empty);
        session.Queue.Clear();
        session.WaitingParam = null;
        session.PendingConfirm = null;
        Enqueue(session, new GoAction(page, flow));
        await RunAsync(session, turn, ct).ConfigureAwait(false);
        return Finish(session, turn);
    }

    /// <summary>Respuesta a la confirmación de un webhook de escritura.</summary>
    public async Task<CamilaTurn> ConfirmAsync(CamilaSession session, bool yes, CancellationToken ct)
    {
        var turn = new CamilaTurn();
        BeginTurn(session, yes ? "sí" : "no");
        var pending = session.PendingConfirm;
        session.PendingConfirm = null;
        if (pending is null)
        {
            turn.Trace.Add(new("aviso", "No hay nada pendiente de confirmar."));
            return Finish(session, turn);
        }

        if (yes)
        {
            if (session.Queue.First?.Value is FulfillAction fa) fa.Confirmed = true;
            await RunAsync(session, turn, ct).ConfigureAwait(false);
        }
        else
        {
            session.Queue.Clear();
            turn.Trace.Add(new("webhook", $"«{pending.Webhook}» cancelado por la persona: no se envió nada."));
            turn.Messages.Add(new CamilaOutMessage { Text = "Listo, no envié nada." });
        }
        return Finish(session, turn);
    }

    #endregion

    #region Consultas de estado (para la ficha UML y el modelo conversacional)

    /// <summary>La página actual, o null si está en la página de inicio del flujo.</summary>
    public CamilaPage? CurrentPage(CamilaSession session) =>
        session.Page is null || _bot.FindFlow(session.Flow) is not { } flow ? null : _bot.FindPage(flow, session.Page);

    /// <summary>Intents que la persona puede disparar ahora (rutas con intent de la página, sus grupos y el flujo).</summary>
    public IReadOnlyList<CamilaIntent> ScopeIntents(CamilaSession session) =>
        ScopeRoutes(session).Where(r => r.Intent is not null && Passes(session, r))
            .Select(r => _bot.FindIntent(r.Intent)).Where(i => i is not null).Select(i => i!)
            .GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).ToList();

    /// <summary>Rutas con condición, intent o evento que aplican a la página actual, en orden de prioridad.</summary>
    public IReadOnlyList<(string Origin, CamilaRoute Route)> ScopeDescribed(CamilaSession session)
    {
        var list = new List<(string, CamilaRoute)>();
        var flow = _bot.FindFlow(session.Flow);
        var page = CurrentPage(session);
        if (page is not null)
        {
            list.AddRange(page.Routes.Select(r => ("página", r)));
            list.AddRange(page.Events.Select(r => ("página (evento)", r)));
            foreach (var g in page.Groups) if (_bot.FindGroup(g, flow?.Name) is { } grp) list.AddRange(grp.Routes.Select(r => ($"grupo {grp.Name}", r)));
        }
        if (flow is not null)
        {
            list.AddRange(flow.Routes.Select(r => ("flujo", r)));
            list.AddRange(flow.Events.Select(r => ("flujo (evento)", r)));
            foreach (var g in flow.Groups) if (_bot.FindGroup(g, flow.Name) is { } grp) list.AddRange(grp.Routes.Select(r => ($"grupo {grp.Name}", r)));
        }
        return list;
    }

    #endregion

    #region Ámbito de rutas

    private IEnumerable<CamilaRoute> ScopeRoutes(CamilaSession session) =>
        ScopeDescribed(session).Where(t => !t.Origin.EndsWith("(evento)", StringComparison.Ordinal)).Select(t => t.Route);

    private IEnumerable<CamilaRoute> ScopeEvents(CamilaSession session)
    {
        var all = ScopeDescribed(session);
        return all.Where(t => t.Origin.EndsWith("(evento)", StringComparison.Ordinal)).Select(t => t.Route)
            .Concat(all.Where(t => !t.Origin.EndsWith("(evento)", StringComparison.Ordinal) && t.Route.Event is not null).Select(t => t.Route));
    }

    private bool Passes(CamilaSession session, CamilaRoute route) => Expression(session).Condition(route.Condition);

    private CamilaExpression Expression(CamilaSession session) => new(session.Params, PageParams(session), session.LastText);

    /// <summary>Parámetros de página: <c>status</c> es FINAL cuando no falta ningún parámetro obligatorio del formulario.</summary>
    private Dictionary<string, object?> PageParams(CamilaSession session)
    {
        var missing = CurrentPage(session)?.Form.Any(f => f.Required && !Filled(session, f.Name)) ?? false;
        return new Dictionary<string, object?> { ["status"] = missing ? "ACTIVE" : "FINAL" };
    }

    private static bool Filled(CamilaSession session, string name) =>
        session.Params.TryGetValue(name, out var v) && v is not null && !(v is string s && s.Length == 0);

    #endregion

    #region Coincidencia de intents

    /// <summary>La mejor ruta de intent para el texto, o null si ninguna alcanza el umbral.</summary>
    public CamilaRoute? MatchIntent(CamilaSession session, string text)
    {
        var words = Tokens(text);
        if (words.Count == 0) return null;

        CamilaRoute? best = null;
        double bestScore = 0;
        foreach (var route in ScopeRoutes(session))
        {
            if (route.Intent is null || !Passes(session, route)) continue;
            var intent = _bot.FindIntent(route.Intent);
            if (intent is null) continue;
            foreach (var phrase in intent.Phrases)
            {
                var score = Similarity(words, Tokens(phrase));
                if (score > bestScore) { bestScore = score; best = route; }
            }
        }
        return bestScore >= 0.75 ? best : null;
    }

    private static double Similarity(HashSet<string> text, HashSet<string> phrase)
    {
        if (phrase.Count == 0) return 0;
        var common = text.Intersect(phrase).Count();
        if (common == 0) return 0;
        if (text.SetEquals(phrase)) return 1;
        var jaccard = (double)common / text.Union(phrase).Count();
        var coverage = (double)common / phrase.Count;               // todas las palabras de la frase aparecen en el texto
        return phrase.Count >= 2 && coverage >= 1 && text.Count <= phrase.Count + 2 ? Math.Max(jaccard, 0.8) : jaccard;
    }

    /// <summary>Minúsculas, sin acentos ni puntuación.</summary>
    public static HashSet<string> Tokens(string text)
    {
        var sb = new StringBuilder();
        foreach (var ch in text.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : ' ');
        }
        return sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
    }

    /// <summary>Extrae el valor de un parámetro según su entidad: número, expresión regular, lista de sinónimos o texto libre.</summary>
    private bool TryExtract(string entity, string text, out object? value)
    {
        value = text.Trim();
        var name = entity.TrimStart('@');
        if (name is "sys.any" or "") return text.Trim().Length > 0;
        if (name is "sys.number" or "sys.number-integer")
        {
            var m = Regex.Match(text, @"-?\d+(?:[.,]\d+)?");
            if (!m.Success) return false;
            value = double.Parse(m.Value.Replace(',', '.'), CultureInfo.InvariantCulture);
            return true;
        }
        if (_bot.FindEntity(name) is not { } custom) return text.Trim().Length > 0;

        foreach (var pattern in custom.Values)
        {
            if (custom.Kind.Contains("REGEXP", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var m = Regex.Match(text, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(200));
                    if (m.Success) { value = m.Value; return true; }
                }
                catch (ArgumentException) { }
            }
            else if (text.Contains(pattern, StringComparison.OrdinalIgnoreCase)) { value = pattern; return true; }
        }
        return false;
    }

    #endregion

    #region Intérprete

    private static void Enqueue(CamilaSession session, params CamilaAction[] actions)
    {
        foreach (var a in actions) session.Queue.AddLast(a);
    }

    /// <summary>Pone al frente las acciones de una ruta: su fulfillment y su destino.</summary>
    private void EnqueueRoute(CamilaSession session, CamilaRoute route, string label)
    {
        session.Queue.Clear();
        session.PendingConfirm = null;
        Enqueue(session, new FulfillAction(route.Fulfillment, label));
        if (route.TargetPage is not null || route.TargetFlow is not null) Enqueue(session, new GoAction(route.TargetPage, route.TargetFlow));
        else Enqueue(session, new FormAction());
    }

    private static void BeginTurn(CamilaSession session, string text)
    {
        session.LastText = text;
        session.LastUsed = DateTime.UtcNow;
    }

    private static CamilaTurn Finish(CamilaSession session, CamilaTurn turn)
    {
        turn.WaitingParam = session.WaitingParam;
        turn.Confirm = session.PendingConfirm;
        turn.Ended = session.Ended;
        session.LastOptions = turn.Options.Count > 0 ? turn.Options.ToList() : new List<CamilaOption>();
        return turn;
    }

    /// <summary>Sin coincidencia: se usa el manejador <c>sys.no-match-default</c> de la página o el flujo.</summary>
    private async Task HandleNoMatchAsync(CamilaSession session, CamilaTurn turn, CancellationToken ct)
    {
        var handler = ScopeEvents(session).FirstOrDefault(r => string.Equals(r.Event, NoMatchEvent, StringComparison.OrdinalIgnoreCase));
        if (handler is null)
        {
            turn.Messages.Add(new CamilaOutMessage { Text = "No he entendido a qué te refieres." });
            return;
        }
        // El no-match que reintenta pregunta no debe perder el parámetro esperado.
        var waiting = session.WaitingParam;
        EnqueueRoute(session, handler, NoMatchEvent);
        await RunAsync(session, turn, ct).ConfigureAwait(false);
        session.WaitingParam ??= waiting;
    }

    private static string Describe(CamilaRoute r) =>
        r.TargetPage is not null ? $"página «{r.TargetPage}»" : r.TargetFlow is not null ? $"flujo «{r.TargetFlow}»" : "(sin destino: se queda en la página)";

    private async Task RunAsync(CamilaSession session, CamilaTurn turn, CancellationToken ct)
    {
        var steps = 0;
        while (session.Queue.First is { } node && !session.Ended)
        {
            if (++steps > _options.MaxAutoSteps)
            {
                turn.Trace.Add(new("aviso", "Se detuvo el avance automático: demasiados pasos seguidos (posible bucle entre páginas)."));
                session.Queue.Clear();
                break;
            }

            switch (node.Value)
            {
                case FulfillAction fa:
                    if (!await FulfillAsync(session, fa, turn, ct).ConfigureAwait(false)) return;      // pausa para confirmar
                    session.Queue.Remove(node);
                    break;

                case GoAction go:
                    session.Queue.Remove(node);
                    Go(session, go, turn);
                    break;

                case FormAction:
                    session.Queue.Remove(node);
                    if (AskNextParam(session, turn)) { session.Queue.Clear(); return; }
                    break;

                case AutoRouteAction:
                    session.Queue.Remove(node);
                    var auto = AutoRoutes(session).FirstOrDefault();
                    if (auto is not null)
                    {
                        turn.Trace.Add(new("ruta", $"Ruta automática ({auto.Condition}) → {Describe(auto)}"));
                        EnqueueRoute(session, auto, "ruta automática");
                    }
                    break;
            }
        }
    }

    private IEnumerable<CamilaRoute> AutoRoutes(CamilaSession session) =>
        ScopeRoutes(session).Where(r => r.Intent is null && r.Event is null && !string.IsNullOrWhiteSpace(r.Condition) && Passes(session, r));

    /// <summary>Cambia de página o de flujo y encola lo que hace el bot al entrar.</summary>
    private void Go(CamilaSession session, GoAction go, CamilaTurn turn)
    {
        switch (SpecialPage(go.Page))
        {
            case "END_SESSION":
                session.Ended = true;
                turn.Trace.Add(new("ruta", "Fin de la sesión."));
                return;
            case "END_FLOW":
                SetStart(session, _bot.StartFlow);
                Enqueue(session, new AutoRouteAction());
                return;
            case "CURRENT_PAGE":
                Enqueue(session, new FormAction());
                return;
            case "PREVIOUS_PAGE":
                if (session.History.Count > 0)
                {
                    var (flow, page) = session.History[^1];
                    session.History.RemoveAt(session.History.Count - 1);
                    session.Flow = flow;
                    session.Page = page;
                }
                Enqueue(session, new FormAction(), new AutoRouteAction());
                return;
            case "START_PAGE":
                Enqueue(session, new AutoRouteAction());
                session.Page = null;
                return;
        }

        if (go.Flow is not null)
        {
            if (_bot.FindFlow(go.Flow) is null)
            {
                turn.Trace.Add(new("aviso", $"El flujo «{go.Flow}» no existe en la base."));
                return;
            }
            Push(session);
            SetStart(session, go.Flow);
            if (go.Page is null) { Enqueue(session, new AutoRouteAction()); return; }
        }

        if (go.Page is not null)
        {
            var flow = _bot.FindFlow(session.Flow);
            var page = flow is null ? null : _bot.FindPage(flow, go.Page);
            if (page is null)
            {
                turn.Trace.Add(new("aviso", $"La página «{go.Page}» no existe en el flujo «{session.Flow}»."));
                return;
            }
            if (go.Flow is null) Push(session);
            session.Page = page.Name;
            session.WaitingParam = null;
            turn.Trace.Add(new("página", $"{session.Flow} › {page.Name}"));
            Enqueue(session, new FulfillAction(page.Entry, "entrada de " + page.Name), new FormAction(), new AutoRouteAction());
        }
    }

    /// <summary>
    /// Destinos especiales de Dialogflow. El export los trae con su nombre visible («End Session») y no con el técnico
    /// (<c>END_SESSION</c>); se aceptan ambos.
    /// </summary>
    public static string? SpecialPage(string? page)
    {
        var key = page?.Replace(" ", string.Empty).Replace("_", string.Empty).ToUpperInvariant();
        return key switch
        {
            "ENDSESSION" => "END_SESSION",
            "ENDFLOW" => "END_FLOW",
            "CURRENTPAGE" => "CURRENT_PAGE",
            "PREVIOUSPAGE" => "PREVIOUS_PAGE",
            "STARTPAGE" => "START_PAGE",
            _ => page
        };
    }

    /// <summary>El nombre es un destino especial y no una página real.</summary>
    public static bool IsSpecialPage(string? page) => page is not null && SpecialPage(page) is "END_SESSION" or "END_FLOW" or "CURRENT_PAGE" or "PREVIOUS_PAGE" or "START_PAGE";

    private static void Push(CamilaSession session) => session.History.Add((session.Flow, session.Page));

    private static void SetStart(CamilaSession session, string flow)
    {
        session.Flow = flow;
        session.Page = null;
        session.WaitingParam = null;
    }

    /// <summary>Pide el siguiente parámetro obligatorio sin valor; devuelve true si debe esperar a la persona.</summary>
    private bool AskNextParam(CamilaSession session, CamilaTurn turn)
    {
        var page = CurrentPage(session);
        var next = page?.Form.FirstOrDefault(f => f.Required && !Filled(session, f.Name));
        if (next is null)
        {
            session.WaitingParam = null;
            return false;
        }

        session.WaitingParam = next.Name;
        EmitMessages(session, next.Prompt, turn);
        turn.Trace.Add(new("param", $"Espera el dato «{next.Name}» ({next.Entity})."));
        return true;
    }

    #endregion

    #region Fulfillment

    /// <summary>
    /// Orden del bot original: parámetros → webhook → generadores → mensajes (los mensajes pueden usar lo que devolvió el webhook).
    /// Devuelve false si debe pausarse para pedir confirmación.
    /// </summary>
    private async Task<bool> FulfillAsync(CamilaSession session, FulfillAction act, CamilaTurn turn, CancellationToken ct)
    {
        var f = act.Fulfillment;

        if (act.Stage == 0)
        {
            foreach (var set in f.SetParams)
            {
                var name = set.Parameter.StartsWith("$session.params.", StringComparison.Ordinal) ? set.Parameter["$session.params.".Length..] : set.Parameter;
                session.Params[name] = Expression(session).Value(set.Value);
            }
            act.Stage = 1;
        }

        if (act.Stage == 1)
        {
            if (f.Webhook is not null)
            {
                var hook = _bot.FindWebhook(f.Webhook);
                if (hook is null) turn.Trace.Add(new("aviso", $"Webhook «{f.Webhook}» no encontrado."));
                else
                {
                    var mode = _hooks.ModeOf(hook);

                    // Las herramientas del agente ejecutor se imponen: sin ellas el webhook no se llama (spec 011, RF-010).
                    var isWrite = _hooks.IsWrite(hook);
                    if (isWrite ? !session.AllowWriteServices : !session.AllowReadServices)
                    {
                        turn.Trace.Add(new("aviso", $"«{hook.Name}» no se llamó: el agente ejecutor no tiene la herramienta {(isWrite ? AgentTools.WriteService : AgentTools.ReadService)}."));
                    }
                    else
                    {
                        if (mode == CamilaWebhookMode.Real && _options.ConfirmWrites && isWrite && !act.Confirmed)
                        {
                            session.PendingConfirm = _hooks.Preview(hook, f.Tag ?? string.Empty, session.Params, new CamilaWebhookContext(session.Flow, session.Page, session.LastText));
                            turn.Trace.Add(new("webhook", $"«{hook.Name}» es de escritura: espera la confirmación de la persona."));
                            return false;
                        }

                        var result = await _hooks.ExecuteAsync(hook, f.Tag ?? string.Empty, session.Params, ct, new CamilaWebhookContext(session.Flow, session.Page, session.LastText)).ConfigureAwait(false);
                        foreach (var (key, value) in result.Params) session.Params[key] = value;
                        var assigned = result.Params.Count > 0 ? " · asigna: " + string.Join(", ", result.Params.Keys) : string.Empty;
                        turn.Trace.Add(new("webhook", $"{hook.Name}{(f.Tag is null ? "" : $" [{f.Tag}]")} · {result.Mode} · {(result.Ok ? "ok" : "error")} · {result.ElapsedMs} ms · {result.Detail}{assigned}"));
                    }
                }
            }
            act.Stage = 2;
        }

        if (act.Stage == 2)
        {
            foreach (var call in f.Generators) await RunGeneratorAsync(session, call, turn, ct).ConfigureAwait(false);
            act.Stage = 3;
        }

        EmitMessages(session, f, turn);
        return true;
    }

    private async Task RunGeneratorAsync(CamilaSession session, CamilaGeneratorCall call, CamilaTurn turn, CancellationToken ct)
    {
        var def = _bot.FindGenerator(call.Generator);
        if (def is null)
        {
            turn.Trace.Add(new("aviso", $"Generador «{call.Generator}» no encontrado."));
            return;
        }

        var output = call.Output.StartsWith("$session.params.", StringComparison.Ordinal) ? call.Output["$session.params.".Length..] : call.Output;
        if (session.Mode == CamilaMode.Uml)
        {
            turn.Trace.Add(new("generador", $"{def.Name} → {output} (el UML no ejecuta generadores de IA; el parámetro queda sin valor)."));
            return;
        }

        var expr = Expression(session);
        var inputs = call.Inputs.ToDictionary(kv => kv.Key, kv => CamilaExpression.ToText(expr.Value(kv.Value)));
        try
        {
            session.Params[output] = (await _generators.RunAsync(def, inputs, ct).ConfigureAwait(false)).Trim();
            turn.Trace.Add(new("generador", $"{def.Name} → {output} · ok"));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            turn.Trace.Add(new("aviso", $"El generador {def.Name} falló: {ex.Message}"));
        }
    }

    private void EmitMessages(CamilaSession session, CamilaFulfillment f, CamilaTurn turn)
    {
        var expr = Expression(session);
        foreach (var m in f.Messages)
        {
            switch (m.Type)
            {
                case "buttons":
                    foreach (var b in m.Buttons) turn.Options.Add(new CamilaOption { Label = expr.Interpolate(b.Text), Event = b.Event, Link = b.Link is null ? null : expr.Interpolate(b.Link) });
                    break;
                case "info":
                    turn.Messages.Add(new CamilaOutMessage { Kind = "info", Title = m.Title is null ? null : expr.Interpolate(m.Title), Text = expr.Interpolate(m.Subtitle ?? string.Empty), Url = m.Url is null ? null : expr.Interpolate(m.Url) });
                    break;
                case "image":
                    turn.Messages.Add(new CamilaOutMessage { Kind = "image", Url = m.Url is null ? null : expr.Interpolate(m.Url), Text = m.Title ?? string.Empty });
                    break;
                default:
                    var text = expr.Interpolate(m.Texts.FirstOrDefault() ?? string.Empty);
                    if (text.Trim().Length > 0) turn.Messages.Add(new CamilaOutMessage { Text = text });
                    break;
            }
        }
    }

    #endregion
}
