using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AI.Agent.Office.Application.DTOs;

namespace AI.Agent.Office.Web.Services;

public sealed class OfficeApiException : Exception
{
    public OfficeApiException(string message) : base(message) { }
}

/// <summary>HTTP client for the Office API. The Web layer never talks to Anthropic directly.</summary>
public sealed class OfficeApiClient
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;

    public OfficeApiClient(HttpClient http) => _http = http;

    public Task<OfficeStateDto?> GetStateAsync(CancellationToken ct = default) =>
        GetAsync<OfficeStateDto>("api/office/state", ct);

    public Task<TokenUsageDto?> GetUsageAsync(CancellationToken ct = default) =>
        GetAsync<TokenUsageDto>("api/office/usage", ct);

    public Task<IReadOnlyList<AgentTaskDto>?> GetTasksAsync(CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<AgentTaskDto>>("api/tasks", ct);

    public async Task<ChatResponseDto> SendChatAsync(ChatRequestDto request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/chat", request, JsonOptions, ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<ChatResponseDto>(JsonOptions, ct))!;
    }

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

    public async Task CancelConversationAsync(Guid conversationId, CancellationToken ct = default)
    {
        using var response = await _http.PostAsync($"api/chat/{conversationId}/cancel", null, ct);
    }

    public async Task CancelTaskAsync(Guid taskId, CancellationToken ct = default)
    {
        using var response = await _http.PostAsync($"api/tasks/{taskId}/cancel", null, ct);
    }

    public async Task<ConfigurationReportDto?> ReloadAgentsAsync(CancellationToken ct = default)
    {
        using var response = await _http.PostAsync("api/agents/reload", null, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<ConfigurationReportDto>(JsonOptions, ct);
    }

    private async Task<T?> GetAsync<T>(string url, CancellationToken ct)
    {
        using var response = await _http.GetAsync(url, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

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
}
