namespace AI.Agent.Office.Application.Options;

/// <summary>Una oficina seleccionable: solo participan (y aparecen en la oficina 3D) los agentes de su lista.</summary>
public sealed class WorkspaceDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "🏢";
    public string Description { get; set; } = string.Empty;

    /// <summary>Ids de los agentes, en el orden de los puestos. Debe incluir al Orquestador.</summary>
    public List<string> Agents { get; set; } = new();
}

/// <summary>Oficinas configuradas (sección "Workspaces").</summary>
public sealed class WorkspacesOptions
{
    public const string SectionName = "Workspaces";

    /// <summary>Vacío = sin pantalla de selección: todos los agentes están disponibles (el comportamiento original).</summary>
    public List<WorkspaceDefinition> Items { get; set; } = new();
}
