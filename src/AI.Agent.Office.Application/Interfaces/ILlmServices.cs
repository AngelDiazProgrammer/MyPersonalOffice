using AI.Agent.Office.Application.DTOs;

namespace AI.Agent.Office.Application.Interfaces;

public enum LlmMode
{
    /// <summary>Primero las APIs gratuitas (en el orden configurado); Claude solo cuando ninguna puede responder.</summary>
    FreeFirst,
    ClaudeOnly,
    FreeOnly
}

/// <summary>
/// Modo de enrutamiento que se puede cambiar en caliente (lo comparten el enrutador, la API y la
/// interfaz).
/// </summary>
public interface ILlmRouterState
{
    LlmMode Mode { get; set; }
}

/// <summary>
/// Oficinas seleccionables (Desarrollo, Pruebas base de conocimiento...). Solo participan los agentes
/// de la oficina activa.
/// </summary>
public interface IWorkspaceService
{
    bool Enabled { get; }
    bool RequiresSelection { get; }
    AI.Agent.Office.Application.Options.WorkspaceDefinition? Active { get; }
    IReadOnlyList<AI.Agent.Office.Application.Options.WorkspaceDefinition> GetAll();
    bool Select(string id);
    void Clear();
    bool IsAgentActive(string agentId);
    int OrderOf(string agentId);
}

/// <summary>Informa el estado de los proveedores de modelos.</summary>
public interface ILlmStatusService
{
    Task<LlmStatusDto> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Envía una petición mínima SOLO a un proveedor gratuito (sin pasar por el orden ni por Claude) para comprobar que funciona.</summary>
    Task<LlmProviderTestDto> TestProviderAsync(string provider, string? model = null, CancellationToken cancellationToken = default);
}
