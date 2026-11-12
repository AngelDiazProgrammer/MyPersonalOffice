using ATLAS.Application.Interfaces;
using ATLAS.Application.Options;
using Microsoft.Extensions.Options;

namespace ATLAS.Application.Services;

/// <summary>
/// Qué oficina (conjunto de agentes) está abierta. La app arranca sin ninguna, así que la persona
/// siempre elige primero. Sin oficinas configuradas todo está disponible y no hay que elegir.
/// </summary>
public sealed class WorkspaceService : IWorkspaceService
{
    private readonly IReadOnlyList<WorkspaceDefinition> _items;
    private volatile WorkspaceDefinition? _active;

    /// <summary>Crea el servicio con las oficinas configuradas.</summary>
    public WorkspaceService(IOptions<WorkspacesOptions> options)
    {
        _items = options.Value.Items.Where(w => !string.IsNullOrWhiteSpace(w.Id)).ToList();
    }

    /// <summary>Hay oficinas configuradas.</summary>
    public bool Enabled => _items.Count > 0;
    /// <summary>Hay oficinas y todavía no se eligió ninguna.</summary>
    public bool RequiresSelection => Enabled && _active is null;
    /// <summary>Oficina abierta, o null.</summary>
    public WorkspaceDefinition? Active => _active;
    /// <summary>Todas las oficinas configuradas.</summary>
    public IReadOnlyList<WorkspaceDefinition> GetAll() => _items;

    /// <summary>Abre una oficina por su id; devuelve false si no existe.</summary>
    public bool Select(string id)
    {
        var match = _items.FirstOrDefault(w => string.Equals(w.Id, id, StringComparison.OrdinalIgnoreCase));
        if (match is null) return false;
        _active = match;
        return true;
    }

    /// <summary>Cierra la oficina actual (vuelve a la pantalla de selección).</summary>
    public void Clear() => _active = null;

    /// <summary>Indica si el agente participa en la oficina abierta.</summary>
    public bool IsAgentActive(string agentId)
    {
        if (!Enabled) return true;
        var active = _active;
        return active is not null && active.Agents.Contains(agentId, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Posición del agente en la oficina (define su puesto en la oficina 2D).</summary>
    public int OrderOf(string agentId)
    {
        var active = _active;
        if (active is null) return int.MaxValue;
        var index = active.Agents.FindIndex(a => string.Equals(a, agentId, StringComparison.OrdinalIgnoreCase));
        return index < 0 ? int.MaxValue : index;
    }
}
