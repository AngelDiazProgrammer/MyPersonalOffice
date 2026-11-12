using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Events;
using AI.Agent.Office.Domain.Enums;
using AI.Agent.Office.Web.Hubs;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Web.Services;

#region Tipos auxiliares

/// <summary>Dirección de la API que consume la interfaz (sección "Api").</summary>
public sealed class WebApiOptions
{
    public const string SectionName = "Api";
    public string BaseUrl { get; set; } = "http://localhost:5080";
}

/// <summary>Un mensaje mostrado en el chat, con sus tareas, documentos y opciones de solución.</summary>
public sealed record ChatItem(
    ChatRole Role, string Content, DateTimeOffset Time, bool IsError = false, string? PlanMessage = null,
    int TaskCount = 0, IReadOnlyList<GeneratedDocumentDto>? Documents = null,
    KnowledgeChoiceDto? Choice = null, BrowserApprovalDto? Approval = null);

/// <summary>Un entorno de pruebas: un bot (<c>bot</c>, lo ejecuta LectorUML) o un agente conversacional (<c>agent</c>).</summary>
public sealed record EnvironmentTarget(string Kind, string Id, string Name, string Icon, string Description, string Executor)
{
    /// <summary>Destino que viaja en cada mensaje del chat («bot:camila», «agent:camila-ia»).</summary>
    public string Wire => $"{Kind}:{Id}";
    public bool IsBot => Kind == "bot";
}

/// <summary>Un PDF adjunto al próximo mensaje.</summary>
public sealed record AttachedDocument(Guid Id, string FileName, int PageCount, bool RequiresOcr);

#endregion

#region Estado de la oficina para una conexión

/// <summary>
/// Modelo de vista de la oficina para una conexión del navegador. El estado visual solo cambia por
/// eventos reales de SignalR (más la foto inicial), nunca por los componentes.
/// </summary>
public sealed class OfficeStateService : IAsyncDisposable
{
    private const int MaxActivity = 40;

    private readonly OfficeApiClient _api;
    private readonly WebApiOptions _options;
    private readonly ILogger<OfficeStateService> _logger;
    private readonly string _internalToken;
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

    /// <summary>Crea el servicio con el cliente de la API y la configuración.</summary>
    public OfficeStateService(OfficeApiClient api, IOptions<WebApiOptions> options, ILogger<OfficeStateService> logger, AI.Agent.Office.Api.Auth.InternalApiToken internalToken)
    {
        _internalToken = internalToken.Value;
        _api = api;
        _options = options.Value;
        _logger = logger;
    }

    public event Action? Changed;

    /// <summary>Eventos de la oficina sin procesar, reenviados a la interfaz (presentación).</summary>
    public event Func<OfficeEventDto, Task>? EventForwarded;

    public ConnectionStatus Connection => _hub?.Status ?? ConnectionStatus.Disconnected;
    public Guid? ConversationId { get; private set; }
    public bool IsSending { get; private set; }
    public bool AnthropicConfigured { get; private set; } = true;
    public TokenUsageDto Usage { get; private set; } = new(0, 0, 0, Array.Empty<AgentUsageDto>(), null, 0, 0, 0);
    public LlmStatusDto? Llm { get; private set; }
    public WorkspacesDto? Workspaces { get; private set; }
    /// <summary>La persona debe elegir una oficina antes de usar la app.</summary>
    public bool WorkspaceRequired => Workspaces?.Required == true;
    /// <summary>Oficina abierta, o null.</summary>
    /// <summary>Entorno de Camila abierto (bot o agente conversacional), o null si se usa una oficina de agentes.</summary>
    public EnvironmentTarget? Target { get; private set; }
    public WorkspaceDto? ActiveWorkspace => Workspaces?.Available.FirstOrDefault(w => w.Id == Workspaces.ActiveId);
    public string? LastError { get; private set; }
    public bool IsUploading { get; private set; }

    public IReadOnlyList<AgentDto> Agents { get { lock (_gate) { return _agents.ToArray(); } } }
    public IReadOnlyList<AgentTaskDto> Tasks { get { lock (_gate) { return _tasks.ToArray(); } } }
    public IReadOnlyList<OfficeActivityDto> Activity { get { lock (_gate) { return _activity.ToArray(); } } }
    public IReadOnlyList<ChatItem> Messages { get { lock (_gate) { return _messages.ToArray(); } } }
    public IReadOnlyList<AttachedDocument> Attachments { get { lock (_gate) { return _attachments.ToArray(); } } }

    /// <summary>El agente orquestador.</summary>
    public AgentDto? Orchestrator => Agents.FirstOrDefault(a => a.IsOrchestrator);

    /// <summary>Especialistas que están trabajando ahora.</summary>
    public IReadOnlyList<AgentDto> ActiveAgents => Agents
        .Where(a => !a.IsOrchestrator && a.Status is not (AgentStatus.Idle or AgentStatus.Completed or AgentStatus.Error))
        .ToArray();

    /// <summary>Contador que sube cada vez que se asigna una tarea al agente (lo usa la escena para animar).</summary>
    public int AssignmentVersion(string agentId)
    {
        lock (_gate) { return _assignmentVersions.GetValueOrDefault(agentId); }
    }

#endregion

    #region Arranque

    /// <summary>Carga oficinas, estado y proveedores y abre la conexión en tiempo real (una sola vez).</summary>
    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        await RefreshWorkspacesAsync();
        await RefreshSnapshotAsync();
        await RefreshLlmAsync();

        _hub = new OfficeHubClient($"{_options.BaseUrl.TrimEnd('/')}/hubs/office", _internalToken);
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

    #endregion

    #region Oficinas

    /// <summary>Vuelve a leer qué oficinas hay y cuál está abierta.</summary>
    public async Task RefreshWorkspacesAsync()
    {
        try { Workspaces = await _api.GetWorkspacesAsync(); }
        catch (Exception ex) { _logger.LogWarning(ex, "No se pudo leer las oficinas"); }
        NotifyChanged();
    }

    /// <summary>Abre una oficina, limpia la conversación y recarga el estado.</summary>
    public async Task SelectWorkspaceAsync(string id)
    {
        try
        {
            Workspaces = await _api.SelectWorkspaceAsync(id);
            Target = null;
            ResetConversation();
            await RefreshSnapshotAsync();
            await RefreshLlmAsync();
        }
        catch (Exception ex) { LastError = ex.Message; }
        NotifyChanged();
    }

    /// <summary>Vuelve a la pantalla de selección de oficina y limpia la conversación.</summary>
    public async Task ChangeWorkspaceAsync()
    {
        try
        {
            Workspaces = await _api.ResetWorkspaceAsync();
            Target = null;
            ResetConversation();
            await RefreshSnapshotAsync();
        }
        catch (Exception ex) { LastError = ex.Message; }
        NotifyChanged();
    }

    /// <summary>
    /// Abre el entorno de un bot (lo ejecuta LectorUML) o de un agente conversacional (spec 012). No toca la oficina abierta en el servidor:
    /// el chat lleva su destino en cada mensaje. Empieza una conversación nueva.
    /// </summary>
    public void OpenTarget(EnvironmentTarget target)
    {
        ResetConversation();
        Target = target;
        NotifyChanged();
    }

    /// <summary>Descarta mensajes, adjuntos, tareas y actividad de la conversación actual.</summary>
    private void ResetConversation()
    {
        _sendCts?.Cancel();
        lock (_gate)
        {
            _messages.Clear();
            _attachments.Clear();
            _tasks.Clear();
            _activity.Clear();
        }
        ConversationId = null;
        IsSending = false;
    }

    #endregion

    #region Foto del estado y eventos en tiempo real

    /// <summary>Recarga agentes, tareas, actividad y consumo desde la API.</summary>
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

    /// <summary>
    /// Aplica un evento real: actualiza el estado del agente, la actividad y reenvía el evento a la escena
    /// 3D.
    /// </summary>
    private async Task OnEventAsync(OfficeEventDto e)
    {
        lock (_gate)
        {
            // Una recarga puede ya contener este evento (las claves de la lista de actividad deben ser únicas).
            if (!_activity.Any(x => x.Id == e.EventId))
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

        if (EventForwarded is { } forward) await forward(e);
        NotifyChanged();

        if (e.EventType is OfficeEventType.AgentCompleted or OfficeEventType.AgentError or OfficeEventType.OrchestrationCompleted)
            await RefreshUsageAsync();
        if (e.EventType == OfficeEventType.OrchestrationCompleted) _ = RefreshLlmAsync();

        // Otra pestaña (o la API) abrió o cerró una oficina: se la sigue.
        if (e.EventType == OfficeEventType.WorkspaceChanged)
        {
            await RefreshWorkspacesAsync();
            await RefreshSnapshotAsync();
        }

        if (e.EventType is OfficeEventType.TaskCreated or OfficeEventType.TaskCompleted or OfficeEventType.OrchestrationCompleted)
            await RefreshTasksAsync();
    }

    #endregion

    #region Modelos, consumo y tareas

    /// <summary>Recarga el modo de enrutamiento y el estado de los proveedores.</summary>
    public async Task RefreshLlmAsync()
    {
        try
        {
            Llm = await _api.GetLlmStatusAsync();
            NotifyChanged();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "No se pudo leer el estado del modelo local");
        }
    }

    /// <summary>Cambia el modo de enrutamiento (gratis primero, solo Claude, solo gratis).</summary>
    public async Task SetModeAsync(string mode)
    {
        try
        {
            Llm = await _api.SetLlmModeAsync(mode);
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
        }
        NotifyChanged();
    }

    /// <summary>Recarga el consumo de tokens.</summary>
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

    /// <summary>Recarga la lista de tareas.</summary>
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

    #endregion

    #region Chat y exportación

    /// <summary>Envía un mensaje; <paramref name="choiceId"/> es la solución elegida con un botón ("cancel" cierra el caso).</summary>
    public async Task SendAsync(string text, string? choiceId = null)
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
            var response = await _api.SendChatAsync(new ChatRequestDto(text, ConversationId, documentIds, choiceId, Target?.Wire), _sendCts.Token);
            ConversationId = response.ConversationId;
            lock (_gate)
            {
                _messages.Add(new ChatItem(ChatRole.Assistant, response.Reply, DateTimeOffset.Now,
                    response.IsError, response.PlanMessage, response.Tasks.Count, response.Documents, response.Choice, response.BrowserApproval));
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

    /// <summary>
    /// Aprueba el borrador del navegador con credenciales opcionales (no se guardan). El resultado se agrega al chat.
    /// </summary>
    public async Task ApproveBrowserAsync(string? username, string? password)
    {
        if (ConversationId is not { } id || IsSending) return;

        IsSending = true;
        LastError = null;
        NotifyChanged();
        try
        {
            var credentials = string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password) ? null : new BrowserCredentialsDto(username, password);
            var response = await _api.ApproveBrowserAsync(id, credentials);
            lock (_gate) { _messages.Add(new ChatItem(ChatRole.Assistant, response!.Reply, DateTimeOffset.Now, response.Result is { Success: false })); }
        }
        catch (Exception ex)
        {
            lock (_gate) { _messages.Add(new ChatItem(ChatRole.Assistant, ex.Message, DateTimeOffset.Now, true)); }
        }
        finally
        {
            IsSending = false;
            NotifyChanged();
        }
    }

    /// <summary>Cancela el borrador del navegador pendiente.</summary>
    public async Task CancelBrowserAsync()
    {
        if (ConversationId is { } id) await _api.CancelBrowserAsync(id);
        lock (_gate) { _messages.Add(new ChatItem(ChatRole.Assistant, "Entendido, cancelé la operación en el navegador. No se hizo nada.", DateTimeOffset.Now)); }
        NotifyChanged();
    }

    /// <summary>Cancela la solicitud en curso.</summary>
    public async Task CancelCurrentAsync()
    {
        if (ConversationId is { } id) await _api.CancelConversationAsync(id);
    }

    /// <summary>Cancela una tarea concreta.</summary>
    public Task CancelTaskAsync(Guid taskId) => _api.CancelTaskAsync(taskId);

    /// <summary>Exporta un Markdown a Word.</summary>
    public Task<GeneratedDocumentDto> ExportToWordAsync(string title, string markdown, string? style = null) => _api.ExportWordAsync(title, markdown, style);

    /// <summary>Exporta un Markdown a PDF.</summary>
    public Task<GeneratedDocumentDto> ExportToPdfAsync(string title, string markdown, string? style = null) => _api.ExportPdfAsync(title, markdown, style);

    /// <summary>Dirección de descarga de un documento generado.</summary>
    public string DownloadUrl(Guid documentId) => _api.DownloadUrl(documentId);

    #endregion

    #region Documentos adjuntos y cierre

    /// <summary>Sube un PDF y lo deja adjunto al próximo mensaje.</summary>
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

    /// <summary>Quita un adjunto antes de enviarlo.</summary>
    public void RemoveAttachment(Guid id)
    {
        lock (_gate) { _attachments.RemoveAll(a => a.Id == id); }
        NotifyChanged();
    }

    /// <summary>Avisa a los componentes que el estado cambió.</summary>
    private void NotifyChanged() => Changed?.Invoke();

    /// <summary>Cierra la conexión en tiempo real y cancela lo pendiente.</summary>
    public async ValueTask DisposeAsync()
    {
        _sendCts?.Cancel();
        if (_hub is not null) await _hub.DisposeAsync();
    }
    #endregion

}
