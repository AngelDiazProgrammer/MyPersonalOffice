using System.Collections.Concurrent;

namespace AI.Agent.Office.Application.Camila;

#region Opciones

/// <summary>Cómo se ejecuta un webhook.</summary>
public enum CamilaWebhookMode
{
    /// <summary>No se llama; el flujo sigue sin sus datos.</summary>
    Disabled,
    /// <summary>No se llama; se marca como simulado.</summary>
    Simulated,
    /// <summary>Se llama al servicio real (solo hosts de la lista permitida).</summary>
    Real
}

/// <summary>Configuración de Camila IA (sección "Camila").</summary>
public sealed class CamilaOptions
{
    public const string SectionName = "Camila";

    public bool Enabled { get; set; } = true;

    /// <summary>Carpeta de los playbooks maestros de bots (spec 012).</summary>
    public string BotsDirectory { get; set; } = "playbooks bots";

    /// <summary>Modo de todo webhook que no tenga uno propio. Primera prueba: Real (decisión del dueño del producto).</summary>
    public CamilaWebhookMode DefaultWebhookMode { get; set; } = CamilaWebhookMode.Real;

    /// <summary>Modo por webhook (nombre → Disabled | Simulated | Real).</summary>
    public Dictionary<string, CamilaWebhookMode> WebhookModes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Hosts (con puerto) a los que el modo Real puede llamar. Cualquier otro se rechaza.</summary>
    public List<string> AllowedHosts { get; set; } = new();

    /// <summary>Los webhooks de escritura piden confirmación explícita en la conversación antes de ejecutarse.</summary>
    public bool ConfirmWrites { get; set; } = true;

    /// <summary>Nombres de webhooks que se tratan como escritura aunque el generador no los haya marcado.</summary>
    public List<string> ExtraWriteWebhooks { get; set; } = new();


    /// <summary>Tope de tiempo de una llamada a un webhook (el del propio webhook si es menor).</summary>
    public int WebhookTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Permite que la oficina Conversacional use también los modelos gratuitos (según el modo de la plataforma). Por defecto es falso: sus
    /// llamadas llevan datos de colaboradores y solo se envían a Claude. Gemini en capa gratuita puede usar el contenido para mejorar sus productos.
    /// </summary>
    public bool AllowFreeModels { get; set; }

    /// <summary>Segundos máximos de espera a cada llamada al modelo en la oficina Conversacional (pasado el tiempo se degrada con un mensaje claro).</summary>
    public int ModelTimeoutSeconds { get; set; } = 30;

    /// <summary>Pasos automáticos máximos por mensaje (evita bucles entre páginas).</summary>
    public int MaxAutoSteps { get; set; } = 40;

    /// <summary>Minutos sin actividad tras los cuales se descarta el estado de una conversación.</summary>
    public int SessionMinutes { get; set; } = 120;
}

#endregion

#region Estado de la conversación

/// <summary>Una acción pendiente del intérprete de flujos.</summary>
public abstract record CamilaAction;
/// <summary>Ejecutar un fulfillment: parámetros, webhook, generadores y mensajes, en ese orden.</summary>
public sealed record FulfillAction(CamilaFulfillment Fulfillment, string Label) : CamilaAction { public int Stage { get; set; } public bool Confirmed { get; set; } }
/// <summary>Ir a una página o flujo (o a un destino especial: END_SESSION, END_FLOW, PREVIOUS_PAGE, CURRENT_PAGE).</summary>
public sealed record GoAction(string? Page, string? Flow) : CamilaAction;
/// <summary>Pedir los parámetros obligatorios que falten en el formulario de la página.</summary>
public sealed record FormAction : CamilaAction;
/// <summary>Tomar la primera ruta solo de condición que se cumpla.</summary>
public sealed record AutoRouteAction : CamilaAction;

/// <summary>Un webhook de escritura esperando que la persona confirme.</summary>
public sealed record CamilaPendingConfirm(string Webhook, string Tag, string Method, string Host, string Path, string Body);

/// <summary>
/// Estado de una conversación con Camila: los parámetros de sesión viven solo aquí, en memoria, y se descartan al terminar
/// (spec 009, RF-017). Nada de esto se escribe en disco.
/// </summary>
public sealed class CamilaSession
{
    public CamilaSession(Guid conversationId, CamilaMode mode)
    {
        ConversationId = conversationId;
        Mode = mode;
    }

    public Guid ConversationId { get; }
    public CamilaMode Mode { get; }
    public Dictionary<string, object?> Params { get; } = new(StringComparer.OrdinalIgnoreCase);
    public string Flow { get; set; } = string.Empty;
    /// <summary>Página actual; null = la "página de inicio" del flujo.</summary>
    public string? Page { get; set; }
    public List<(string Flow, string? Page)> History { get; } = new();
    public LinkedList<CamilaAction> Queue { get; } = new();
    public string? WaitingParam { get; set; }
    public CamilaPendingConfirm? PendingConfirm { get; set; }
    /// <summary>Solo Conversacional: webhook de escritura que el agente pidió y espera el «sí» de la persona.</summary>
    public string? PendingWebhookName { get; set; }
    /// <summary>Herramientas del agente ejecutor (spec 011, RF-010): si es falso, el UML no llama a webhooks de lectura / escritura.</summary>
    public bool AllowReadServices { get; set; } = true;
    public bool AllowWriteServices { get; set; } = true;
    public bool Ended { get; set; }
    public string LastText { get; set; } = string.Empty;
    public DateTime LastUsed { get; set; } = DateTime.UtcNow;
    /// <summary>Opciones (botones) de la última respuesta, por si la persona escribe su número o su texto.</summary>
    public List<CamilaOption> LastOptions { get; set; } = new();
    /// <summary>Turnos recientes (solo Conversacional), para dar contexto al modelo.</summary>
    public List<(string Role, string Text)> Dialogue { get; } = new();
    public SemaphoreSlim Gate { get; } = new(1, 1);
}

/// <summary>Modo de la oficina de Camila.</summary>
public enum CamilaMode { Uml, Conversational }

/// <summary>Una respuesta del bot.</summary>
public sealed class CamilaOutMessage
{
    /// <summary>"text", "info" o "image".</summary>
    public string Kind { get; set; } = "text";
    public string Text { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Url { get; set; }
}

/// <summary>Un botón que la persona puede pulsar.</summary>
public sealed class CamilaOption
{
    public string Label { get; set; } = string.Empty;
    public string? Event { get; set; }
    public string? Link { get; set; }
}

/// <summary>Una línea de lo que hizo el motor (webhooks, generadores, rutas), para el UML y la auditoría.</summary>
public sealed record CamilaTraceItem(string Kind, string Text);

/// <summary>Resultado de un turno del motor.</summary>
public sealed class CamilaTurn
{
    public List<CamilaOutMessage> Messages { get; } = new();
    public List<CamilaOption> Options { get; } = new();
    public List<CamilaTraceItem> Trace { get; } = new();
    public string? WaitingParam { get; set; }
    public CamilaPendingConfirm? Confirm { get; set; }
    public bool Ended { get; set; }
}

/// <summary>Almacén en memoria de las sesiones, con caducidad.</summary>
public sealed class CamilaSessionStore
{
    private readonly ConcurrentDictionary<Guid, CamilaSession> _sessions = new();

    /// <summary>La sesión de una conversación, o null si no existe o cambió de modo.</summary>
    public CamilaSession? Find(Guid id, CamilaMode mode) => _sessions.TryGetValue(id, out var s) && s.Mode == mode ? s : null;

    /// <summary>Guarda una sesión nueva (reemplaza la anterior de esa conversación).</summary>
    public CamilaSession Add(CamilaSession session)
    {
        _sessions[session.ConversationId] = session;
        return session;
    }

    /// <summary>Descarta una conversación.</summary>
    public void Remove(Guid id) => _sessions.TryRemove(id, out _);

    /// <summary>Descarta las sesiones inactivas.</summary>
    public int Prune(TimeSpan idle)
    {
        var limit = DateTime.UtcNow - idle;
        var removed = 0;
        foreach (var pair in _sessions)
            if (pair.Value.LastUsed < limit && _sessions.TryRemove(pair.Key, out _)) removed++;
        return removed;
    }

    public int Count => _sessions.Count;
}

#endregion
