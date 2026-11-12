using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AI.Agent.Office.Application.Agents;
using AI.Agent.Office.Application.DTOs;

namespace AI.Agent.Office.Web.Services;

/// <summary>Error devuelto por la API, con un mensaje para mostrar a la persona.</summary>
public sealed class OfficeApiException : Exception
{
    public OfficeApiException(string message) : base(message) { }
}

/// <summary>Cliente HTTP de la API de la oficina. La capa Web nunca habla directamente con Anthropic.</summary>
public sealed class OfficeApiClient
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;

    /// <summary>Crea el cliente con su HttpClient configurado.</summary>
    public OfficeApiClient(HttpClient http) => _http = http;

    #region Bots (spec 012)

    /// <summary>Los bots con playbook maestro (los ejecuta LectorUML).</summary>
    public async Task<IReadOnlyList<AI.Agent.Office.Application.Camila.BotInfo>> GetBotsAsync(CancellationToken ct = default) =>
        await GetAsync<List<AI.Agent.Office.Application.Camila.BotInfo>>("api/bots", ct) ?? new List<AI.Agent.Office.Application.Camila.BotInfo>();

    #endregion

    #region Agentes (spec 011)

    /// <summary>Catálogo de todos los agentes.</summary>
    public async Task<IReadOnlyList<AgentCatalogItemDto>> GetAgentCatalogAsync(CancellationToken ct = default) =>
        await GetAsync<List<AgentCatalogItemDto>>("api/agents/catalog", ct) ?? new List<AgentCatalogItemDto>();

    /// <summary>Un agente tal como se edita.</summary>
    public Task<AgentUpsertDto?> GetAgentDefinitionAsync(string id, CancellationToken ct = default) =>
        GetAsync<AgentUpsertDto>($"api/agents/{Uri.EscapeDataString(id)}/definition", ct);

    /// <summary>Crea un agente; los errores de validación vuelven en el resultado.</summary>
    public async Task<AgentSaveResultDto> CreateAgentAsync(AgentUpsertDto agent, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/agents", agent, JsonOptions, ct);
        return await ReadSaveAsync(response, ct);
    }

    /// <summary>Edita un agente.</summary>
    public async Task<AgentSaveResultDto> UpdateAgentAsync(string id, AgentUpsertDto agent, CancellationToken ct = default)
    {
        using var response = await _http.PutAsJsonAsync($"api/agents/{Uri.EscapeDataString(id)}", agent, JsonOptions, ct);
        return await ReadSaveAsync(response, ct);
    }

    /// <summary>Elimina un agente.</summary>
    public async Task<AgentSaveResultDto> DeleteAgentAsync(string id, CancellationToken ct = default)
    {
        using var response = await _http.DeleteAsync($"api/agents/{Uri.EscapeDataString(id)}", ct);
        return await ReadSaveAsync(response, ct);
    }

    private static async Task<AgentSaveResultDto> ReadSaveAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            if (await response.Content.ReadFromJsonAsync<AgentSaveResultDto>(JsonOptions, ct) is { } result) return result;
        }
        catch (JsonException) { /* respuesta que no es un resultado de agente: se cae al mensaje genérico */ }
        return new AgentSaveResultDto(false, new[] { $"La API respondió {(int)response.StatusCode}." }, null);
    }

    #endregion

    #region Registros en vivo (spec 010)

    /// <summary>Entradas nuevas del registro en vivo desde el número de secuencia <paramref name="after"/>.</summary>
    public Task<LogsDto?> GetLogsAsync(long after, CancellationToken ct = default) =>
        GetAsync<LogsDto>($"api/logs?after={after}", ct);

    /// <summary>Vacía el registro en memoria.</summary>
    public async Task ClearLogsAsync(CancellationToken ct = default)
    {
        using var response = await _http.PostAsync("api/logs/clear", null, ct);
        await EnsureSuccessAsync(response, ct);
    }

    #endregion

    #region Consultas y oficinas

    /// <summary>Foto completa del estado de la oficina.</summary>
    public Task<OfficeStateDto?> GetStateAsync(CancellationToken ct = default) =>
        GetAsync<OfficeStateDto>("api/office/state", ct);

    /// <summary>Modo de enrutamiento y estado de los proveedores.</summary>
    public Task<LlmStatusDto?> GetLlmStatusAsync(CancellationToken ct = default) =>
        GetAsync<LlmStatusDto>("api/llm/status", ct);

    /// <summary>Cambia el modo de enrutamiento.</summary>
    public async Task<LlmStatusDto?> SetLlmModeAsync(string mode, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/llm/mode", new LlmModeRequestDto(mode), JsonOptions, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<LlmStatusDto>(JsonOptions, ct);
    }

    /// <summary>Oficinas disponibles y cuál está abierta.</summary>
    public Task<WorkspacesDto?> GetWorkspacesAsync(CancellationToken ct = default) =>
        GetAsync<WorkspacesDto>("api/workspaces", ct);

    /// <summary>Abre una oficina.</summary>
    public async Task<WorkspacesDto?> SelectWorkspaceAsync(string id, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/workspaces/select", new WorkspaceSelectRequestDto(id), JsonOptions, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<WorkspacesDto>(JsonOptions, ct);
    }

    /// <summary>Vuelve a la selección de oficina.</summary>
    public async Task<WorkspacesDto?> ResetWorkspaceAsync(CancellationToken ct = default)
    {
        using var response = await _http.PostAsync("api/workspaces/reset", null, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<WorkspacesDto>(JsonOptions, ct);
    }

    /// <summary>Consumo de tokens.</summary>
    public Task<TokenUsageDto?> GetUsageAsync(CancellationToken ct = default) =>
        GetAsync<TokenUsageDto>("api/office/usage", ct);

    /// <summary>Lista de tareas.</summary>
    public Task<IReadOnlyList<AgentTaskDto>?> GetTasksAsync(CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<AgentTaskDto>>("api/tasks", ct);

    #endregion

    #region Chat, documentos y exportación

    /// <summary>Envía un mensaje al Orquestador.</summary>
    public async Task<ChatResponseDto> SendChatAsync(ChatRequestDto request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/chat", request, JsonOptions, ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<ChatResponseDto>(JsonOptions, ct))!;
    }

    /// <summary>Sube un PDF para que el Lector PDF lo analice.</summary>
    public async Task<DocumentDto> UploadDocumentAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        var file = new StreamContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType) ? "application/pdf" : contentType);
        form.Add(file, "file", fileName);

        using var response = await _http.PostAsync("api/documents", form, ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<DocumentDto>(JsonOptions, ct))!;
    }

    /// <summary>Exporta un Markdown a Word.</summary>
    public async Task<GeneratedDocumentDto> ExportWordAsync(string title, string markdown, string? style = null, CancellationToken ct = default) =>
        await ExportAsync("api/documents/word", title, markdown, style, ct);

    /// <summary>Exporta un Markdown a PDF.</summary>
    public async Task<GeneratedDocumentDto> ExportPdfAsync(string title, string markdown, string? style = null, CancellationToken ct = default) =>
        await ExportAsync("api/documents/pdf", title, markdown, style, ct);

    /// <summary>Envía la solicitud de exportación al endpoint indicado.</summary>
    private async Task<GeneratedDocumentDto> ExportAsync(string url, string title, string markdown, string? style, CancellationToken ct)
    {
        using var response = await _http.PostAsJsonAsync(url, new WordExportRequestDto(title, markdown, style), JsonOptions, ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<GeneratedDocumentDto>(JsonOptions, ct))!;
    }

    /// <summary>Dirección de descarga de un documento generado.</summary>
    public string DownloadUrl(Guid documentId) => new Uri(_http.BaseAddress!, $"api/documents/generated/{documentId}").ToString();

    /// <summary>Cancela la solicitud en curso de una conversación.</summary>
    public async Task CancelConversationAsync(Guid conversationId, CancellationToken ct = default)
    {
        using var response = await _http.PostAsync($"api/chat/{conversationId}/cancel", null, ct);
    }

    /// <summary>Cancela una tarea.</summary>
    public async Task CancelTaskAsync(Guid taskId, CancellationToken ct = default)
    {
        using var response = await _http.PostAsync($"api/tasks/{taskId}/cancel", null, ct);
    }

    /// <summary>Pide a la API releer los Markdown de los agentes.</summary>
    public async Task<ConfigurationReportDto?> ReloadAgentsAsync(CancellationToken ct = default)
    {
        using var response = await _http.PostAsync("api/agents/reload", null, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<ConfigurationReportDto>(JsonOptions, ct);
    }

    #endregion

    #region Conectividad a navegadores (spec 007)

    public Task<IReadOnlyList<BrowserSiteDto>?> GetBrowserSitesAsync(CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<BrowserSiteDto>>("api/browser/sites", ct);

    public Task<BrowserSiteDto?> CreateBrowserSiteAsync(BrowserSiteRequestDto site, CancellationToken ct = default) =>
        SendAsync<BrowserSiteRequestDto, BrowserSiteDto>(HttpMethod.Post, "api/browser/sites", site, ct);

    public Task<BrowserSiteDto?> UpdateBrowserSiteAsync(string id, BrowserSiteRequestDto site, CancellationToken ct = default) =>
        SendAsync<BrowserSiteRequestDto, BrowserSiteDto>(HttpMethod.Put, $"api/browser/sites/{Uri.EscapeDataString(id)}", site, ct);

    public async Task DeleteBrowserSiteAsync(string id, CancellationToken ct = default)
    {
        using var response = await _http.DeleteAsync($"api/browser/sites/{Uri.EscapeDataString(id)}", ct);
        await EnsureSuccessAsync(response, ct);
    }

    /// <summary>Olvida las credenciales en memoria de un sitio.</summary>
    public async Task ClearBrowserCredentialsAsync(string id, CancellationToken ct = default)
    {
        using var response = await _http.DeleteAsync($"api/browser/sites/{Uri.EscapeDataString(id)}/credentials", ct);
        await EnsureSuccessAsync(response, ct);
    }

    public Task<PlaybookDto?> GetPlaybookAsync(string siteId, CancellationToken ct = default) =>
        GetAsync<PlaybookDto>($"api/browser/sites/{Uri.EscapeDataString(siteId)}/playbook", ct);

    public Task<PlaybookDto?> SavePlaybookAsync(string siteId, string markdown, CancellationToken ct = default) =>
        SendAsync<PlaybookTextRequestDto, PlaybookDto>(HttpMethod.Put, $"api/browser/sites/{Uri.EscapeDataString(siteId)}/playbook", new PlaybookTextRequestDto(markdown), ct);

    public Task<PlaybookDto?> SaveProposalAsync(string siteId, string markdown, CancellationToken ct = default) =>
        SendAsync<PlaybookTextRequestDto, PlaybookDto>(HttpMethod.Put, $"api/browser/sites/{Uri.EscapeDataString(siteId)}/playbook/proposal", new PlaybookTextRequestDto(markdown), ct);

    public async Task<PlaybookDto?> ApproveProposalAsync(string siteId, CancellationToken ct = default)
    {
        using var response = await _http.PostAsync($"api/browser/sites/{Uri.EscapeDataString(siteId)}/playbook/proposal/approve", null, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<PlaybookDto>(JsonOptions, ct);
    }

    public async Task RejectProposalAsync(string siteId, CancellationToken ct = default)
    {
        using var response = await _http.DeleteAsync($"api/browser/sites/{Uri.EscapeDataString(siteId)}/playbook/proposal", ct);
        await EnsureSuccessAsync(response, ct);
    }

    /// <summary>Abre el navegador visible para iniciar sesión a mano; responde cuando se cierra la ventana.</summary>
    public async Task BrowserLoginAsync(string siteId, CancellationToken ct = default)
    {
        using var response = await _http.PostAsync($"api/browser/sites/{Uri.EscapeDataString(siteId)}/login", null, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public Task<BrowserRunResultDto?> RunBrowserAsync(string siteId, BrowserRunRequestDto request, CancellationToken ct = default) =>
        SendAsync<BrowserRunRequestDto, BrowserRunResultDto>(HttpMethod.Post, $"api/browser/sites/{Uri.EscapeDataString(siteId)}/runs", request, ct);

    public Task<BrowserApproveResponseDto?> ApproveBrowserAsync(Guid conversationId, BrowserCredentialsDto? credentials, CancellationToken ct = default) =>
        SendAsync<BrowserCredentialsDto?, BrowserApproveResponseDto>(HttpMethod.Post, $"api/browser/pending/{conversationId}/approve", credentials, ct);

    public async Task CancelBrowserAsync(Guid conversationId, CancellationToken ct = default)
    {
        using var response = await _http.PostAsync($"api/browser/pending/{conversationId}/cancel", null, ct);
    }

    private async Task<TResponse?> SendAsync<TRequest, TResponse>(HttpMethod method, string url, TRequest body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, url) { Content = JsonContent.Create(body, options: JsonOptions) };
        using var response = await _http.SendAsync(request, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct);
    }

    #endregion

    #region Utilidades HTTP

    /// <summary>GET que devuelve el JSON deserializado.</summary>
    private async Task<T?> GetAsync<T>(string url, CancellationToken ct)
    {
        using var response = await _http.GetAsync(url, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    /// <summary>Si la respuesta es un error, lanza una excepción con el mensaje de la API.</summary>
    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;

        var message = $"El servidor respondió {(int)response.StatusCode}.";
        try
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            if (doc.RootElement.TryGetProperty("detail", out var detail) && detail.GetString() is { Length: > 0 } text)
                message = text;
        }
        catch (JsonException) { }

        throw new OfficeApiException(message);
    }
    #endregion

}
