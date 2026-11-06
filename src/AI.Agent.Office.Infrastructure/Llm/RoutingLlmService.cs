using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Infrastructure.Anthropic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Infrastructure.Llm;

/// <summary>
/// El único <see cref="IAnthropicService"/> que ve la aplicación. Tiene exactamente dos destinos: el
/// <see cref="FreeApiService"/> global (todos los gratuitos) y Claude, el proveedor de pago principal.
/// Primero van las APIs gratuitas; Claude solo se usa cuando ninguna puede responder, cuando la
/// solicitud lleva documentos del usuario (privacidad) o cuando se fuerza Claude para la solicitud o el
/// agente.
/// </summary>
public sealed class RoutingLlmService : IAnthropicService, ILlmRouterState, ILlmStatusService
{
    private readonly AnthropicService _claude;
    private readonly FreeApiService _free;
    private readonly ILogger<RoutingLlmService> _logger;
    private volatile LlmMode _mode;

    /// <summary>Crea el enrutador con Claude, las APIs gratuitas y el modo inicial.</summary>
    public RoutingLlmService(AnthropicService claude, FreeApiService free, IOptions<LlmOptions> options, ILogger<RoutingLlmService> logger)
    {
        _claude = claude;
        _free = free;
        _logger = logger;
        _mode = options.Value.Mode;
    }

    /// <summary>Modo de enrutamiento actual (se puede cambiar en caliente desde la interfaz).</summary>
    public LlmMode Mode
    {
        get => _mode;
        set => _mode = value;
    }

    /// <summary>True si algún destino puede atender solicitudes en el modo actual.</summary>
    public bool IsConfigured => Mode switch
    {
        LlmMode.ClaudeOnly => _claude.IsConfigured,
        LlmMode.FreeOnly => _free.AnyConfigured,
        _ => _free.AnyConfigured || _claude.IsConfigured
    };

    /// <summary>Estado del modo y de cada proveedor gratuito, para la interfaz.</summary>
    public Task<LlmStatusDto> GetStatusAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new LlmStatusDto(Mode.ToString(), _free.GetStatuses(), _claude.IsConfigured));

    /// <summary>Prueba un proveedor gratuito concreto con una petición mínima.</summary>
    public Task<LlmProviderTestDto> TestProviderAsync(string provider, string? model = null, CancellationToken cancellationToken = default) =>
        _free.TestProviderAsync(provider, model, cancellationToken);

    /// <summary>Envía la solicitud al destino que corresponde según el modo, la privacidad y la disponibilidad.</summary>
    public async Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var mode = Mode;
        var forceClaude = string.Equals(request.ProviderPreference, LlmProviders.Claude, StringComparison.OrdinalIgnoreCase);
        var claudeAllowed = mode != LlmMode.FreeOnly && _claude.IsConfigured;

        // Se omiten las APIs gratuitas cuando se pide Claude (y se puede usar) y SIEMPRE en las solicitudes con
        // documentos del usuario.
        var tryFree = mode != LlmMode.ClaudeOnly && !request.Sensitive && !(forceClaude && claudeAllowed);

        if (request.Sensitive && mode == LlmMode.FreeOnly)
            throw new LlmException("Esta solicitud incluye documentos y no se envían a APIs gratuitas. Cambia el modo a «Gratis primero» o «Solo Claude».");

        var reasons = new List<string>();
        if (tryFree)
        {
            if (!_free.AnyConfigured)
            {
                reasons.Add("no hay ninguna API gratuita configurada");
            }
            else
            {
                var response = await _free.TrySendAsync(request, reasons, cancellationToken).ConfigureAwait(false);
                if (response is not null) return response;
            }

            if (!claudeAllowed)
            {
                throw new LlmException(mode == LlmMode.FreeOnly
                    ? $"Ninguna API gratuita pudo responder ({string.Join("; ", reasons)})."
                    : $"Ninguna API gratuita pudo responder ({string.Join("; ", reasons)}) y Claude no está disponible (falta ANTHROPIC_API_KEY).");
            }

            _logger.LogInformation("Escalando a Claude ({Purpose}): {Reasons}", request.Purpose, string.Join("; ", reasons));
        }

        if (!_claude.IsConfigured)
            throw new LlmException("Falta la variable de entorno ANTHROPIC_API_KEY. Configúrala y reinicia la aplicación.");

        return await _claude.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
