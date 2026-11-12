using System.Text.Json.Serialization;

namespace ATLAS.Application.Camila;

#region Modelo del bot (spec 009)

// Modelo normalizado del export de Dialogflow CX. Es lo que se serializa dentro de CamilaIA.md (bloques "camila:*") y lo que
// leen los dos motores. No contiene credenciales: los secretos son referencias {{secret:NOMBRE}} que se resuelven en ejecución.

/// <summary>Un botón o enlace de una respuesta.</summary>
public sealed class CamilaButton
{
    public string Text { get; set; } = string.Empty;
    public string? Event { get; set; }
    public string? Link { get; set; }
}

/// <summary>Una respuesta del bot: texto, botones, tarjeta informativa o imagen.</summary>
public sealed class CamilaMessage
{
    /// <summary>"text", "buttons", "info" o "image".</summary>
    public string Type { get; set; } = "text";
    /// <summary>Variantes del texto (el bot original elige una al azar; aquí se usa la primera).</summary>
    public List<string> Texts { get; set; } = new();
    public List<CamilaButton> Buttons { get; set; } = new();
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Url { get; set; }
}

/// <summary>Asignación de un parámetro de sesión (el valor puede ser una expresión).</summary>
public sealed class CamilaSet
{
    public string Parameter { get; set; } = string.Empty;
    public object? Value { get; set; }
}

/// <summary>Invocación de un generador de IA de Dialogflow.</summary>
public sealed class CamilaGeneratorCall
{
    public string Generator { get; set; } = string.Empty;
    public Dictionary<string, string> Inputs { get; set; } = new();
    public string Output { get; set; } = string.Empty;
}

/// <summary>Qué hace el bot al entrar a una página o al tomar una ruta.</summary>
public sealed class CamilaFulfillment
{
    public List<CamilaMessage> Messages { get; set; } = new();
    public List<CamilaSet> SetParams { get; set; } = new();
    public string? Webhook { get; set; }
    public string? Tag { get; set; }
    public List<CamilaGeneratorCall> Generators { get; set; } = new();

    [JsonIgnore]
    public bool IsEmpty => Messages.Count == 0 && SetParams.Count == 0 && Webhook is null && Generators.Count == 0;
}

/// <summary>Ruta de transición o manejador de evento.</summary>
public sealed class CamilaRoute
{
    public string? Intent { get; set; }
    public string? Condition { get; set; }
    public string? Event { get; set; }
    public string? TargetPage { get; set; }
    public string? TargetFlow { get; set; }
    public CamilaFulfillment Fulfillment { get; set; } = new();
}

/// <summary>Parámetro del formulario de una página.</summary>
public sealed class CamilaFormParam
{
    public string Name { get; set; } = string.Empty;
    public string Entity { get; set; } = "@sys.any";
    public bool Required { get; set; }
    public CamilaFulfillment Prompt { get; set; } = new();
}

/// <summary>Una página de un flujo.</summary>
public sealed class CamilaPage
{
    public string Name { get; set; } = string.Empty;
    public CamilaFulfillment Entry { get; set; } = new();
    public List<CamilaFormParam> Form { get; set; } = new();
    public List<CamilaRoute> Routes { get; set; } = new();
    public List<CamilaRoute> Events { get; set; } = new();
    public List<string> Groups { get; set; } = new();
}

/// <summary>Un flujo: sus rutas propias (la "página de inicio") y sus páginas.</summary>
public sealed class CamilaFlow
{
    public string Name { get; set; } = string.Empty;
    public List<CamilaRoute> Routes { get; set; } = new();
    public List<CamilaRoute> Events { get; set; } = new();
    public List<string> Groups { get; set; } = new();
    public List<CamilaPage> Pages { get; set; } = new();
}

/// <summary>Grupo de rutas compartido (atajos).</summary>
public sealed class CamilaRouteGroup
{
    public string Name { get; set; } = string.Empty;
    /// <summary>Nombre del flujo que lo define; vacío si es del agente (global).</summary>
    public string? Flow { get; set; }
    public List<CamilaRoute> Routes { get; set; } = new();
}

/// <summary>Un intent con sus frases de entrenamiento.</summary>
public sealed class CamilaIntent
{
    public string Name { get; set; } = string.Empty;
    public List<string> Phrases { get; set; } = new();
}

/// <summary>Entidad personalizada (expresión regular o lista de sinónimos).</summary>
public sealed class CamilaEntityType
{
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public List<string> Values { get; set; } = new();
}

/// <summary>Un webhook del bot. Las credenciales son referencias a secretos externos, nunca valores.</summary>
public sealed class CamilaWebhook
{
    public string Name { get; set; } = string.Empty;
    public string Uri { get; set; } = string.Empty;
    public string Method { get; set; } = "POST";
    /// <summary>"flexible" (cuerpo y mapeo propios) o "standard" (Dialogflow envía y espera su formato WebhookRequest/WebhookResponse).</summary>
    public string Type { get; set; } = "flexible";
    /// <summary>Cabeceras; un valor <c>{{secret:NOMBRE}}</c> se resuelve desde la configuración secreta.</summary>
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? Body { get; set; }
    public List<CamilaMapping> Mapping { get; set; } = new();
    public int TimeoutSeconds { get; set; } = 30;
    /// <summary>"read" o "write": los de escritura piden confirmación antes de ejecutarse.</summary>
    public string Kind { get; set; } = "read";
    public List<string> Notes { get; set; } = new();

    [JsonIgnore]
    public string Host => Uri.Length > 0 && System.Uri.TryCreate(Uri, UriKind.Absolute, out var u) ? u.Authority : string.Empty;
}

/// <summary>Asigna un valor de la respuesta (ruta JSON) a un parámetro de sesión.</summary>
public sealed class CamilaMapping
{
    public string Parameter { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

/// <summary>Generador de IA de Dialogflow (prompt con marcadores $nombre).</summary>
public sealed class CamilaGeneratorDef
{
    public string Name { get; set; } = string.Empty;
    /// <summary>Identificador con el que lo citan los flujos (el último tramo de la ruta del recurso).</summary>
    public string Id { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public double Temperature { get; set; } = 0.5;
}

/// <summary>El bot completo, con búsquedas por nombre.</summary>
public sealed class CamilaBot
{
    public List<CamilaFlow> Flows { get; set; } = new();
    public List<CamilaRouteGroup> Groups { get; set; } = new();
    public List<CamilaIntent> Intents { get; set; } = new();
    public List<CamilaEntityType> Entities { get; set; } = new();
    public List<CamilaWebhook> Webhooks { get; set; } = new();
    public List<CamilaGeneratorDef> Generators { get; set; } = new();
    public string StartFlow { get; set; } = "Default Start Flow";

    #region Búsquedas

    private Dictionary<string, CamilaFlow>? _flows;
    private Dictionary<string, CamilaIntent>? _intents;
    private Dictionary<string, CamilaWebhook>? _hooks;
    private Dictionary<string, CamilaGeneratorDef>? _gens;
    private Dictionary<string, CamilaEntityType>? _entities;

    public CamilaFlow? FindFlow(string? name) => name is null ? null : (_flows ??= Flows.GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase)).GetValueOrDefault(name);
    public CamilaPage? FindPage(CamilaFlow flow, string? name) => name is null ? null : flow.Pages.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
    public CamilaIntent? FindIntent(string? name) => name is null ? null : (_intents ??= Intents.GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase)).GetValueOrDefault(name);
    public CamilaWebhook? FindWebhook(string? name) => name is null ? null : (_hooks ??= Webhooks.GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase)).GetValueOrDefault(name);
    public CamilaEntityType? FindEntity(string? name) => name is null ? null : (_entities ??= Entities.GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase)).GetValueOrDefault(name.TrimStart('@'));

    /// <summary>Un generador por su nombre o por el identificador con el que lo cita un flujo.</summary>
    public CamilaGeneratorDef? FindGenerator(string? nameOrPath)
    {
        if (string.IsNullOrWhiteSpace(nameOrPath)) return null;
        _gens ??= Generators.SelectMany(g => new[] { (g.Name, g), (g.Id, g) }).Where(t => t.Item1.Length > 0)
            .GroupBy(t => t.Item1, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First().g, StringComparer.OrdinalIgnoreCase);
        var key = nameOrPath.Contains('/') ? nameOrPath[(nameOrPath.LastIndexOf('/') + 1)..] : nameOrPath;
        return _gens.GetValueOrDefault(key);
    }

    /// <summary>Grupo de rutas por nombre; prefiere el definido por el flujo y cae al global.</summary>
    public CamilaRouteGroup? FindGroup(string name, string? flow) =>
        Groups.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase) && string.Equals(g.Flow, flow, StringComparison.OrdinalIgnoreCase))
        ?? Groups.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(g.Flow))
        ?? Groups.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));

    #endregion
}

#endregion

