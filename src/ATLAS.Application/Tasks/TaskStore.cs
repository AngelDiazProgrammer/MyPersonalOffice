using ATLAS.Application.Interfaces;
using ATLAS.Domain.Entities;

namespace ATLAS.Application.Tasks;

/// <summary>
/// Almacén en memoria de las tareas de los agentes (con tope de tareas guardadas) y de sus tokens de
/// cancelación.
/// </summary>
public sealed class TaskStore : ITaskStore
{
    private const int MaxStoredTasks = 500;

    private readonly object _gate = new();
    private readonly List<AgentTask> _tasks = new();
    private readonly Dictionary<Guid, CancellationTokenSource> _sources = new();
    private readonly HashSet<Guid> _userCancelled = new();

    /// <summary>Registra una tarea; si se supera el tope se descartan las más viejas.</summary>
    public void Add(AgentTask task)
    {
        lock (_gate)
        {
            _tasks.Add(task);
            if (_tasks.Count > MaxStoredTasks)
            {
                var removable = _tasks
                    .Where(t => t.Status is TaskStatus.Completed or TaskStatus.Failed or TaskStatus.Cancelled)
                    .Take(_tasks.Count - MaxStoredTasks)
                    .ToList();
                foreach (var r in removable) _tasks.Remove(r);
            }
        }
    }

    /// <summary>Busca una tarea por su id.</summary>
    public AgentTask? Get(Guid taskId)
    {
        lock (_gate) { return _tasks.FirstOrDefault(t => t.Id == taskId)?.Clone(); }
    }

    /// <summary>Todas las tareas guardadas.</summary>
    public IReadOnlyList<AgentTask> GetAll()
    {
        lock (_gate) { return _tasks.OrderByDescending(t => t.CreatedAt).Select(t => t.Clone()).ToArray(); }
    }

    /// <summary>Tareas de una conversación.</summary>
    public IReadOnlyList<AgentTask> GetByConversation(Guid conversationId)
    {
        lock (_gate) { return _tasks.Where(t => t.ConversationId == conversationId).Select(t => t.Clone()).ToArray(); }
    }

    /// <summary>Aplica un cambio a la tarea de forma atómica.</summary>
    public AgentTask? Update(Guid taskId, Action<AgentTask> mutation)
    {
        lock (_gate)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task is null) return null;
            mutation(task);
            return task.Clone();
        }
    }

    /// <summary>Asocia la tarea con su fuente de cancelación.</summary>
    public void RegisterCancellation(Guid taskId, CancellationTokenSource source)
    {
        lock (_gate) { _sources[taskId] = source; }
    }

    /// <summary>Quita la fuente de cancelación al terminar la tarea.</summary>
    public void UnregisterCancellation(Guid taskId)
    {
        lock (_gate) { _sources.Remove(taskId); }
    }

    /// <summary>Pide cancelar la tarea (la cancela el usuario); devuelve false si no estaba en curso.</summary>
    public bool RequestCancel(Guid taskId)
    {
        CancellationTokenSource? source;
        lock (_gate)
        {
            if (!_sources.TryGetValue(taskId, out source)) return false;
            _userCancelled.Add(taskId);
        }

        try { source.Cancel(); }
        catch (ObjectDisposedException) { return false; }
        return true;
    }

    /// <summary>Indica si la tarea fue cancelada por el usuario (y no por tiempo o error).</summary>
    public bool WasCancelledByUser(Guid taskId)
    {
        lock (_gate) { return _userCancelled.Contains(taskId); }
    }
}
