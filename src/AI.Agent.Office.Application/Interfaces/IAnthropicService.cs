namespace AI.Agent.Office.Application.Interfaces;

/// <summary>Single entry point to Claude. The API key never leaves the implementation.</summary>
public interface IAnthropicService
{
    bool IsConfigured { get; }
    Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken cancellationToken = default);
}
