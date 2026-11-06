using System.Globalization;
using AI.Agent.Office.Domain.Enums;
using TaskStatus = AI.Agent.Office.Domain.Enums.TaskStatus;

namespace AI.Agent.Office.Web.Components.Shared;

/// <summary>Ayudas de presentación: etiquetas, nombres CSS y utilidades de color.</summary>
public static class StatusInfo
{
    /// <summary>Etiqueta en español del estado de un agente.</summary>
    public static string Label(AgentStatus status) => status switch
    {
        AgentStatus.Idle => "En reposo",
        AgentStatus.Thinking => "Pensando",
        AgentStatus.Working => "Trabajando",
        AgentStatus.Reading => "Leyendo",
        AgentStatus.Coding => "Programando",
        AgentStatus.Reviewing => "Revisando",
        AgentStatus.Waiting => "Esperando",
        AgentStatus.Completed => "Completado",
        AgentStatus.Error => "Error",
        _ => status.ToString()
    };

    /// <summary>Nombre de la clase CSS del estado.</summary>
    public static string Css(AgentStatus status) => status.ToString().ToLowerInvariant();

    /// <summary>Emoji de la burbuja de estado.</summary>
    public static string Bubble(AgentStatus status) => status switch
    {
        AgentStatus.Thinking => "💭",
        AgentStatus.Working => "⚙️",
        AgentStatus.Reading => "📖",
        AgentStatus.Coding => "⌨️",
        AgentStatus.Reviewing => "🔍",
        AgentStatus.Waiting => "⏳",
        AgentStatus.Completed => "✅",
        AgentStatus.Error => "⚠️",
        _ => string.Empty
    };

    /// <summary>Etiqueta en español del estado de una tarea.</summary>
    public static string Label(TaskStatus status) => status switch
    {
        TaskStatus.Pending => "Pendiente",
        TaskStatus.Assigned => "Asignada",
        TaskStatus.Running => "En curso",
        TaskStatus.Completed => "Completada",
        TaskStatus.Failed => "Fallida",
        TaskStatus.Cancelled => "Cancelada",
        _ => status.ToString()
    };

    /// <summary>True si la tarea aún no terminó.</summary>
    public static bool IsActive(TaskStatus status) => status is TaskStatus.Pending or TaskStatus.Assigned or TaskStatus.Running;

    /// <summary>Icono de un tipo de evento para la actividad.</summary>
    public static string EventIcon(OfficeEventType type) => type switch
    {
        OfficeEventType.AgentStarted => "🟢",
        OfficeEventType.AgentThinking => "💭",
        OfficeEventType.AgentTaskAssigned => "📌",
        OfficeEventType.AgentProgress => "⚙️",
        OfficeEventType.AgentCompleted => "✅",
        OfficeEventType.AgentError => "⛔",
        OfficeEventType.TaskCreated => "🗂️",
        OfficeEventType.TaskCompleted => "🏁",
        OfficeEventType.OrchestrationStarted => "🚀",
        OfficeEventType.OrchestrationCompleted => "🎯",
        _ => "•"
    };

    /// <summary>Oscurece (factor &lt; 1) o aclara (&gt; 1) un color #rrggbb.</summary>
    public static string Shade(string hex, double factor)
    {
        if (!TryParse(hex, out var r, out var g, out var b)) return "#4c1d95";
        static int Clamp(double v) => (int)Math.Clamp(v, 0, 255);
        return $"#{Clamp(r * factor):x2}{Clamp(g * factor):x2}{Clamp(b * factor):x2}";
    }

    /// <summary>Convierte un color #rrggbb y una transparencia en rgba().</summary>
    public static string Rgba(string hex, double alpha)
    {
        if (!TryParse(hex, out var r, out var g, out var b)) { r = 139; g = 92; b = 246; }
        return string.Create(CultureInfo.InvariantCulture, $"rgba({r},{g},{b},{alpha})");
    }

    /// <summary>Lee los componentes rojo, verde y azul de un color #rrggbb.</summary>
    private static bool TryParse(string hex, out int r, out int g, out int b)
    {
        r = g = b = 0;
        if (string.IsNullOrWhiteSpace(hex)) return false;
        var h = hex.TrimStart('#');
        if (h.Length != 6) return false;
        return int.TryParse(h[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r)
               && int.TryParse(h[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out g)
               && int.TryParse(h[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b);
    }
}
