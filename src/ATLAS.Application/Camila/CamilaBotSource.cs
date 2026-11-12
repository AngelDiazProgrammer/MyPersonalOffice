namespace ATLAS.Application.Camila;

#region Bots (spec 012)

/// <summary>Un bot descrito en su playbook maestro (<c>playbooks bots/&lt;id&gt;/bot.md</c>).</summary>
/// <param name="Executor">Agente ejecutor de su flujo UML (hoy <c>lector-uml</c>).</param>
/// <param name="Agent">Agente conversacional que lo usa como conocimiento (hoy <c>camila-ia</c>); vacío si no tiene.</param>
public sealed record BotInfo(string Id, string Name, string Description, string Icon, string Executor, string Agent, int Flows, int Pages, int Webhooks);

/// <summary>Entrega los bots cargados desde sus playbooks. Singleton: se recarga bajo demanda.</summary>
public interface ICamilaBotSource
{
    /// <summary>Los bots cargados (uno por carpeta con <c>bot.md</c>).</summary>
    IReadOnlyList<BotInfo> Bots { get; }

    /// <summary>El bot completo (todos sus flujos, catálogos y webhooks), o null si no existe.</summary>
    CamilaBot? Get(string botId);

    /// <summary>Notas escritas a mano (<c>notas.md</c>): tono, preguntas frecuentes, avisos. La regeneración nunca las pisa.</summary>
    string? Notes(string botId);

    /// <summary>Narrativa de la base de conocimiento por flujo (nombre del flujo → texto), sin bloques ejecutables.</summary>
    IReadOnlyDictionary<string, string>? Narrative(string botId);

    /// <summary>Problemas de referencias encontrados al cargar (páginas, flujos o webhooks que no existen).</summary>
    IReadOnlyList<string> Problems(string botId);

    /// <summary>Relee los playbooks; devuelve el número de bots cargados.</summary>
    Task<int> ReloadAsync(CancellationToken cancellationToken = default);
}

#endregion

#region Validación

/// <summary>Comprueba que lo que el playbook referencia existe (spec 012, RF-013).</summary>
public static class CamilaBotValidator
{
    /// <summary>Referencias rotas del bot; vacío si todo existe.</summary>
    public static IReadOnlyList<string> Validate(CamilaBot bot)
    {
        var problems = new List<string>();
        foreach (var flow in bot.Flows)
        {
            var routes = flow.Routes.Concat(flow.Events).Select(r => (Where: flow.Name, Route: r))
                .Concat(flow.Pages.SelectMany(p => p.Routes.Concat(p.Events).Select(r => (Where: flow.Name + " › " + p.Name, Route: r))))
                .Concat(flow.Pages.SelectMany(p => p.Form.Select(f => (Where: flow.Name + " › " + p.Name, Route: new CamilaRoute { Fulfillment = f.Prompt }))))
                .Concat(flow.Pages.Select(p => (Where: flow.Name + " › " + p.Name, Route: new CamilaRoute { Fulfillment = p.Entry })));

            foreach (var (where, route) in routes)
            {
                if (route.TargetFlow is not null && bot.FindFlow(route.TargetFlow) is null) problems.Add($"{where}: flujo inexistente «{route.TargetFlow}»");
                if (route.TargetPage is not null && !CamilaEngine.IsSpecialPage(route.TargetPage))
                {
                    var targetFlow = route.TargetFlow is not null ? bot.FindFlow(route.TargetFlow) : flow;
                    if (targetFlow is not null && bot.FindPage(targetFlow, route.TargetPage) is null) problems.Add($"{where}: página inexistente «{route.TargetPage}»");
                }
                if (route.Intent is not null && bot.FindIntent(route.Intent) is null) problems.Add($"{where}: intent inexistente «{route.Intent}»");
                if (route.Fulfillment.Webhook is not null && bot.FindWebhook(route.Fulfillment.Webhook) is null) problems.Add($"{where}: webhook inexistente «{route.Fulfillment.Webhook}»");
                foreach (var g in route.Fulfillment.Generators)
                    if (bot.FindGenerator(g.Generator) is null) problems.Add($"{where}: generador inexistente «{g.Generator}»");
            }
        }
        if (bot.FindFlow(bot.StartFlow) is null) problems.Add($"El flujo inicial «{bot.StartFlow}» no existe.");
        return problems.Distinct().ToList();
    }
}

#endregion
