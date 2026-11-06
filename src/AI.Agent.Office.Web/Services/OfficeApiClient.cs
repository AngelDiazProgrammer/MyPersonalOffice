using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
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
