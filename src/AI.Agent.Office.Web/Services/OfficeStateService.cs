using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Events;
using AI.Agent.Office.Domain.Enums;
using AI.Agent.Office.Web.Hubs;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Web.Services;

public sealed class WebApiOptions
{
    public const string SectionName = "Api";
    public string BaseUrl { get; set; } = "http://localhost:5080";
}

public sealed record ChatItem(ChatRole Role, string Content, DateTimeOffset Time, bool IsError = false, string? PlanMessage = null, int TaskCount = 0);

public sealed record AttachedDocument(Guid Id, string FileName, int PageCount, bool RequiresOcr);

/// <summary>
/// Client-side view model of the office for one browser circuit. Visual state is only changed by
/// real events coming from SignalR (plus the initial snapshot), never by the components themselves.
/// </summary>
public sealed class OfficeStateService : IAsyncDisposable
{
    private const int MaxActivity = 40;

    private readonly OfficeApiClient _api;
    private readonly WebApiOptions _options;
    private readonly ILogger<OfficeStateService> _logger;
    private readonly object _gate = new();
    private readonly List<AgentDto> _agents = new();
    private readonly List<AgentTaskDto> _tasks = new();
    private readonly List<OfficeActivityDto> _activity = new();
    private readonly List<ChatItem> _messages = new();
    private readonly List<AttachedDocument> _attachments = new();
    private readonly Dictionary<string, int> _assignmentVersions = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _taskRefresh = new(1, 1);
    private OfficeHubClient? _hub;
    private CancellationTokenSource? _sendCts;
    private bool _initialized;

    public OfficeStateService(OfficeApiClient api, IOptions<WebApiOptions> options, ILogger<OfficeStateService> logger)
    {
        _api = api;
        _options = options.Value;
        _logger = logger;
    }

    public event Action? Changed;

    public ConnectionStatus Connection => _hub?.Status ?? ConnectionStatus.Disconnected;
    public Guid? ConversationId { get; private set; }
    public bool IsSending { get; private set; }
    public bool AnthropicConfigured { get; private set; } = true;
    public TokenUsageDto Usage { get; private set; } = new(0, 0, 0, Array.Empty<AgentUsageDto>(), null);
    public string? LastError { get; private set; }
    public bool IsUploading { get; private set; }

    public IReadOnlyList<AgentDto> Agents { get { lock (_gate) { return _agents.ToArray(); } } }
    public IReadOnlyList<AgentTaskDto> Tasks { get { lock (_gate) { return _tasks.ToArray(); } } }
    public IReadOnlyList<OfficeActivityDto> Activity { get { lock (_gate) { return _activity.ToArray(); } } }
    public IReadOnlyList<ChatItem> Messages { get { lock (_gate) { return _messages.ToArray(); } } }
    public IReadOnlyList<AttachedDocument> Attachments { get { lock (_gate) { return _attachments.ToArray(); } } }

    public AgentDto? Orchestrator => Agents.FirstOrDefault(a => a.IsOrchestrator);

    public IReadOnlyList<AgentDto> ActiveAgents => Agents
        .Where(a => !a.IsOrchestrator && a.Status is not (AgentStatus.Idle or AgentStatus.Completed or AgentStatus.Error))
        .ToArray();

    public int AssignmentVersion(string agentId)
    {
        lock (_gate) { return _assignmentVersions.GetValueOrDefault(agentId); }
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        await RefreshSnapshotAsync();

        _hub = new OfficeHubClient($"{_options.BaseUrl.TrimEnd('/')}/hubs/office");
        _hub.EventReceived += OnEventAsync;
        _hub.StatusChanged += _ => NotifyChanged();
        _hub.Reconnected += RefreshSnapshotAsync;

        try { await _hub.StartAsync(); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo conectar con SignalR");
            LastError = "No se pudo conectar con el canal en tiempo real.";
        }

        NotifyChanged();
    }

    public async Task RefreshSnapshotAsync()
    {
        try
        {
            var state = await _api.GetStateAsync();
            if (state is null) return;

            lock (_gate)
            {
                _agents.Clear();
                _agents.AddRange(state.Agents);
                _tasks.Clear();
                _tasks.AddRange(state.Tasks);
                _activity.Clear();
                _activity.AddRange(state.Activity.Take(MaxActivity));
            }
            Usage = state.Usage;
            AnthropicConfigured = state.AnthropicConfigured;
            LastError = null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo cargar el estado de la oficina");
            LastError = "No se pudo cargar el estado de la oficina.";
        }

        NotifyChanged();
    }

    private async Task OnEventAsync(OfficeEventDto e)
    {
        lock (_gate)
        {
            _activity.Insert(0, new OfficeActivityDto(e.EventId, e.AgentId, e.TaskId, e.EventType, e.Description, e.Timestamp));
            if (_activity.Count > MaxActivity) _activity.RemoveRange(MaxActivity, _activity.Count - MaxActivity);

            if (e.AgentId is not null && e.AdditionalData is { } data &&
                data.TryGetValue(EventDataKeys.AgentStatus, out var statusText) &&
                Enum.TryParse<AgentStatus>(statusText, out var status))
            {
                var index = _agents.FindIndex(a => string.Equals(a.Id, e.AgentId, StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                {
                    data.TryGetValue(EventDataKeys.TaskTitle, out var title);
                    _agents[index] = _agents[index] with
                    {
                        Status = status,
                        CurrentTaskTitle = title,
                        CurrentTaskId = status == AgentStatus.Idle ? null : (e.TaskId ?? _agents[index].CurrentTaskId),
                        LastActivityAt = e.Timestamp
                    };
                }
            }

            if (e.EventType == OfficeEventType.AgentTaskAssigned && e.AgentId is not null)
                _assignmentVersions[e.AgentId] = _assignmentVersions.GetValueOrDefault(e.AgentId) + 1;
        }

        NotifyChanged();

        if (e.EventType is OfficeEventType.AgentCompleted or OfficeEventType.AgentError or OfficeEventType.OrchestrationCompleted)
            await RefreshUsageAsync();

        if (e.EventType is OfficeEventType.TaskCreated or OfficeEventType.TaskCompleted or OfficeEventType.OrchestrationCompleted)
            await RefreshTasksAsync();
    }

    private async Task RefreshUsageAsync()
    {
        try
        {
            if (await _api.GetUsageAsync() is { } usage)
            {
                Usage = usage;
                NotifyChanged();
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "No se pudo refrescar el uso de tokens");
        }
    }

    private async Task RefreshTasksAsync()
    {
        if (!await _taskRefresh.WaitAsync(0)) return; // a refresh is already running; it will pick up the latest state
        try
        {
            var tasks = await _api.GetTasksAsync();
            if (tasks is null) return;
            lock (_gate)
            {
                _tasks.Clear();
                _tasks.AddRange(tasks.Take(50));
            }
            NotifyChanged();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "No se pudieron refrescar las tareas");
        }
        finally
        {
            _taskRefresh.Release();
        }
    }

    public async Task SendAsync(string text)
    {
        text = text.Trim();
        if (text.Length == 0 || IsSending) return;

        IReadOnlyList<Guid> documentIds;
        lock (_gate)
        {
            documentIds = _attachments.Select(a => a.Id).ToArray();
            var attachedNote = _attachments.Count > 0
                ? "\n\n📎 " + string.Join(", ", _attachments.Select(a => a.FileName))
                : string.Empty;
            _messages.Add(new ChatItem(ChatRole.User, text + attachedNote, DateTimeOffset.Now));
            _attachments.Clear();
        }

        IsSending = true;
        LastError = null;
        _sendCts = new CancellationTokenSource();
        NotifyChanged();

        try
        {
            var response = await _api.SendChatAsync(new ChatRequestDto(text, ConversationId, documentIds), _sendCts.Token);
            ConversationId = response.ConversationId;
            lock (_gate)
            {
                _messages.Add(new ChatItem(ChatRole.Assistant, response.Reply, DateTimeOffset.Now,
                    response.IsError, response.PlanMessage, response.Tasks.Count));
            }
        }
        catch (OperationCanceledException)
        {
            lock (_gate) { _messages.Add(new ChatItem(ChatRole.Assistant, "Solicitud cancelada.", DateTimeOffset.Now, true)); }
        }
        catch (Exception ex)
        {
            lock (_gate) { _messages.Add(new ChatItem(ChatRole.Assistant, ex.Message, DateTimeOffset.Now, true)); }
        }
        finally
        {
            IsSending = false;
            _sendCts?.Dispose();
            _sendCts = null;
            NotifyChanged();
        }

        _ = RefreshTasksAsync();
    }

    public async Task CancelCurrentAsync()
    {
        if (ConversationId is { } id) await _api.CancelConversationAsync(id);
    }

    public Task CancelTaskAsync(Guid taskId) => _api.CancelTaskAsync(taskId);

    public async Task AttachAsync(Stream content, string fileName, string contentType)
    {
        IsUploading = true;
        LastError = null;
        NotifyChanged();
        try
        {
            var doc = await _api.UploadDocumentAsync(content, fileName, contentType);
            lock (_gate) { _attachments.Add(new AttachedDocument(doc.Id, doc.FileName, doc.PageCount, doc.RequiresOcr)); }
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
        }
        finally
        {
            IsUploading = false;
            NotifyChanged();
        }
    }

    public void RemoveAttachment(Guid id)
    {
        lock (_gate) { _attachments.RemoveAll(a => a.Id == id); }
        NotifyChanged();
    }

    private void NotifyChanged() => Changed?.Invoke();

    public async ValueTask DisposeAsync()
    {
        _sendCts?.Cancel();
        if (_hub is not null) await _hub.DisposeAsync();
    }
}
