namespace AI.Agent.Office.Application.Interfaces;

/// <summary>
/// Punto único de entrada a los modelos (Claude y APIs gratuitas). La clave de la API nunca sale de la
/// implementación.
/// </summary>
public interface IAnthropicService
{
    bool IsConfigured { get; }
    Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken cancellationToken = default);
}
