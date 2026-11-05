using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Domain.Entities;

namespace AI.Agent.Office.Application.Tasks;

public sealed class TaskStore : ITaskStore
{
    private const int MaxStoredTasks = 500;

    private readonly object _gate = new();
    private readonly List<AgentTask> _tasks = new();
    private readonly Dictionary<Guid, CancellationTokenSource> _sources = new();
    private readonly HashSet<Guid> _userCancelled = new();

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

    public AgentTask? Get(Guid taskId)
    {
        lock (_gate) { return _tasks.FirstOrDefault(t => t.Id == taskId)?.Clone(); }
    }

    public IReadOnlyList<AgentTask> GetAll()
    {
        lock (_gate) { return _tasks.OrderByDescending(t => t.CreatedAt).Select(t => t.Clone()).ToArray(); }
    }

    public IReadOnlyList<AgentTask> GetByConversation(Guid conversationId)
    {
        lock (_gate) { return _tasks.Where(t => t.ConversationId == conversationId).Select(t => t.Clone()).ToArray(); }
    }

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

    public void RegisterCancellation(Guid taskId, CancellationTokenSource source)
    {
        lock (_gate) { _sources[taskId] = source; }
    }

    public void UnregisterCancellation(Guid taskId)
    {
        lock (_gate) { _sources.Remove(taskId); }
    }

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

    public bool WasCancelledByUser(Guid taskId)
    {
        lock (_gate) { return _userCancelled.Contains(taskId); }
    }
}
