using System.Text.RegularExpressions;
using ATLAS.Application.Interfaces;
using ATLAS.Application.Options;
using ATLAS.Domain.Enums;
using Microsoft.Extensions.Options;

namespace ATLAS.Application.Agents;

#region DTOs (spec 011)

/// <summary>Lo que el formulario crea o edita de un agente. Las claves internas del archivo que no están aquí se conservan al editar.</summary>
public sealed record AgentUpsertDto(
    string Id, string Name, string Role, string Purpose, string Description, string Kind, string Environment,
    IReadOnlyList<string>? Tools, string Knowledge, bool Sensitive, string Color, string Icon, int? MaxTokens, string Instructions);

/// <summary>Un agente en el catálogo: su definición, su uso, su estado y su consumo.</summary>
public sealed record AgentCatalogItemDto(
    string Id, string Name, string Role, string Purpose, string Description, string Kind, string Environment,
    IReadOnlyList<string> Tools, string Knowledge, bool Sensitive, string Color, string Icon, bool IsSystem, bool IsOrchestrator,
    IReadOnlyList<string> UsedBy, AgentStatus Status, long Requests, long InputTokens, long OutputTokens, string File);

/// <summary>Resultado de crear, editar o eliminar.</summary>
public sealed record AgentSaveResultDto(bool Ok, IReadOnlyList<string> Errors, AgentCatalogItemDto? Agent);

#endregion

#region Servicio

/// <summary>Administración de agentes desde el front: crear, editar, eliminar y consultar (spec 011).</summary>
public interface IAgentAdminService
{
    /// <summary>Catálogo de todos los agentes con su uso, estado y consumo.</summary>
    IReadOnlyList<AgentCatalogItemDto> GetCatalog();

    /// <summary>El agente tal como se edita (con sus instrucciones), o null si no existe.</summary>
    AgentUpsertDto? GetForEdit(string id);

    Task<AgentSaveResultDto> CreateAsync(AgentUpsertDto agent, CancellationToken cancellationToken = default);
    Task<AgentSaveResultDto> UpdateAsync(string id, AgentUpsertDto agent, CancellationToken cancellationToken = default);
    Task<AgentSaveResultDto> DeleteAsync(string id, CancellationToken cancellationToken = default);
}

/// <summary>Escribe y borra el archivo de un agente (lo implementa la infraestructura).</summary>
public interface IAgentFileStore
{
    /// <summary>Escribe el archivo del agente de forma atómica, conservando las claves del encabezado que el formulario no maneja.</summary>
    Task WriteAsync(AgentUpsertDto agent, bool create, CancellationToken cancellationToken);

    /// <summary>Borra el archivo del agente.</summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken);
}

#endregion

#region Validación

/// <summary>Reglas de un agente (spec 011, RF-002, RF-003, RF-007, RF-008).</summary>
public static partial class AgentValidator
{
    public const int MaxInstructionsChars = 60_000;

    [GeneratedRegex("^[a-z0-9][a-z0-9-]{1,40}$")]
    private static partial Regex IdPattern();

    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex ColorPattern();

    /// <summary>Errores del agente; vacío si es válido. <paramref name="current"/> es el agente actual al editar.</summary>
    public static IReadOnlyList<string> Validate(AgentUpsertDto a, bool create, IReadOnlyCollection<string> existingIds, AgentDefinition? current)
    {
        var errors = new List<string>();

        if (!IdPattern().IsMatch(a.Id ?? string.Empty)) errors.Add("El id debe tener 2 a 41 caracteres: minúsculas, números y guiones (sin espacios ni rutas).");
        else if (create && existingIds.Contains(a.Id, StringComparer.OrdinalIgnoreCase)) errors.Add($"Ya existe un agente con el id «{a.Id}».");
        if (create && string.Equals(a.Id, SystemAgents.Orchestrator, StringComparison.OrdinalIgnoreCase)) errors.Add("El orquestador es único y ya existe.");

        if (string.IsNullOrWhiteSpace(a.Name) || a.Name.Trim().Length > 60) errors.Add("El nombre es obligatorio (máximo 60 caracteres).");
        if (string.IsNullOrWhiteSpace(a.Role) || a.Role.Trim().Length > 80) errors.Add("El rol es obligatorio (máximo 80 caracteres).");
        if (string.IsNullOrWhiteSpace(a.Purpose) || a.Purpose.Trim().Length < 10) errors.Add("Cada agente debe tener un uso específico: escribe al menos 10 caracteres sobre para qué sirve.");
        else if (a.Purpose.Trim().Length > 300) errors.Add("El uso es una frase: máximo 300 caracteres.");

        if (!AgentKinds.All.Contains(a.Kind, StringComparer.OrdinalIgnoreCase)) errors.Add($"El tipo debe ser: {string.Join(", ", AgentKinds.All)}.");
        if (!AgentEnvironments.All.Contains(a.Environment, StringComparer.OrdinalIgnoreCase)) errors.Add($"El entorno debe ser: {string.Join(", ", AgentEnvironments.All)}.");

        foreach (var tool in a.Tools ?? Array.Empty<string>())
            if (!AgentTools.All.Contains(tool, StringComparer.OrdinalIgnoreCase)) errors.Add($"Herramienta no válida: «{tool}». Válidas: {string.Join(", ", AgentTools.All)}.");

        if (!string.IsNullOrEmpty(a.Color) && !ColorPattern().IsMatch(a.Color)) errors.Add("El color debe ser hexadecimal, por ejemplo #8b5cf6.");
        if (a.MaxTokens is < 256 or > 16000) errors.Add("El tope de tokens debe estar entre 256 y 16000.");
        if (string.IsNullOrWhiteSpace(a.Instructions)) errors.Add("Las instrucciones son obligatorias (para un ejecutor, describe qué hace).");
        else if (a.Instructions.Length > MaxInstructionsChars) errors.Add($"Las instrucciones superan {MaxInstructionsChars:N0} caracteres.");

        if (!create && current is not null)
        {
            if (!string.Equals(a.Id, current.Id, StringComparison.OrdinalIgnoreCase)) errors.Add("El id no se puede cambiar.");
            if (current.IsSystem && !string.Equals(a.Kind, current.Kind, StringComparison.OrdinalIgnoreCase)) errors.Add($"El tipo de «{current.Name}» es del sistema y no se puede cambiar.");
        }
        return errors;
    }
}

#endregion

#region Implementación

/// <summary>Administra agentes: valida, escribe su archivo y recarga el registro (spec 011).</summary>
public sealed class AgentAdminService : IAgentAdminService
{
    private readonly IAgentConfigurationService _definitions;
    private readonly IAgentCatalogService _catalog;
    private readonly IAgentRegistry _registry;
    private readonly IAgentFileStore _files;
    private readonly ITokenUsageTracker _usage;
    private readonly IOptions<WorkspacesOptions> _workspaces;
    private readonly IOfficeEventService _events;

    public AgentAdminService(IAgentConfigurationService definitions, IAgentCatalogService catalog, IAgentRegistry registry, IAgentFileStore files,
        ITokenUsageTracker usage, IOptions<WorkspacesOptions> workspaces, IOfficeEventService events)
    {
        _definitions = definitions;
        _catalog = catalog;
        _registry = registry;
        _files = files;
        _usage = usage;
        _workspaces = workspaces;
        _events = events;
    }

    /// <summary>Catálogo ordenado: orquestador, agentes del sistema y el resto.</summary>
    public IReadOnlyList<AgentCatalogItemDto> GetCatalog()
    {
        var usage = _usage.GetSnapshot().ByAgent;
        return _definitions.GetDefinitions()
            .OrderBy(d => d.IsOrchestrator ? 0 : d.IsSystem ? 1 : 2).ThenBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
            .Select(d => ToItem(d, usage)).ToList();
    }

    private AgentCatalogItemDto ToItem(AgentDefinition d, IReadOnlyDictionary<string, AgentUsage> usage)
    {
        usage.TryGetValue(d.Id, out var u);
        var usedBy = _workspaces.Value.Items.Where(w => w.Agents.Contains(d.Id, StringComparer.OrdinalIgnoreCase)).Select(w => "Oficina: " + w.Name).ToList();
        if (d.Environment == AgentEnvironments.Chatbot) usedBy.Add("Chatbots (ejecuta los flujos UML de los bots)");
        if (d.Environment == AgentEnvironments.Conversational) usedBy.Add("Agentes conversacionales");
        return new AgentCatalogItemDto(d.Id, d.Name, d.Role, string.IsNullOrWhiteSpace(d.Purpose) ? d.Description : d.Purpose, d.Description, d.Kind, d.Environment,
            d.ToolList, d.Knowledge, d.Sensitive, d.Color, d.Icon, d.IsSystem, d.IsOrchestrator, usedBy,
            _registry.Get(d.Id)?.Status ?? AgentStatus.Idle, u?.Requests ?? 0, u?.InputTokens ?? 0, u?.OutputTokens ?? 0, d.ConfigurationFile);
    }

    public AgentUpsertDto? GetForEdit(string id) => _definitions.GetDefinition(id) is { } d
        ? new AgentUpsertDto(d.Id, d.Name, d.Role, string.IsNullOrWhiteSpace(d.Purpose) ? d.Description : d.Purpose, d.Description, d.Kind, d.Environment,
            d.ToolList, d.Knowledge, d.Sensitive, d.Color, d.Icon, d.ModelSettings.MaxTokens, d.Instructions)
        : null;

    public async Task<AgentSaveResultDto> CreateAsync(AgentUpsertDto agent, CancellationToken cancellationToken = default)
    {
        agent = Normalize(agent);
        var errors = AgentValidator.Validate(agent, true, _definitions.GetDefinitions().Select(d => d.Id).ToList(), null);
        if (errors.Count > 0) return new AgentSaveResultDto(false, errors, null);

        await _files.WriteAsync(agent, true, cancellationToken).ConfigureAwait(false);
        return await ReloadAsync(agent.Id, $"Agente creado: {agent.Name}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<AgentSaveResultDto> UpdateAsync(string id, AgentUpsertDto agent, CancellationToken cancellationToken = default)
    {
        var current = _definitions.GetDefinition(id);
        if (current is null) return new AgentSaveResultDto(false, new[] { $"No existe el agente «{id}»." }, null);

        agent = Normalize(agent with { Id = id });
        var errors = AgentValidator.Validate(agent, false, Array.Empty<string>(), current);
        if (errors.Count > 0) return new AgentSaveResultDto(false, errors, null);

        await _files.WriteAsync(agent, false, cancellationToken).ConfigureAwait(false);
        return await ReloadAsync(id, $"Agente editado: {agent.Name}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<AgentSaveResultDto> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var current = _definitions.GetDefinition(id);
        if (current is null) return new AgentSaveResultDto(false, new[] { $"No existe el agente «{id}»." }, null);
        if (current.IsSystem || current.IsOrchestrator || SystemAgents.IsSystem(id))
            return new AgentSaveResultDto(false, new[] { $"«{current.Name}» es un agente del sistema: se puede editar pero no eliminar." }, null);

        var inOffices = _workspaces.Value.Items.Where(w => w.Agents.Contains(id, StringComparer.OrdinalIgnoreCase)).Select(w => w.Name).ToList();
        if (inOffices.Count > 0)
            return new AgentSaveResultDto(false, new[] { $"«{current.Name}» está en la(s) oficina(s): {string.Join(", ", inOffices)}. Quítalo de ellas (configuración de oficinas) antes de eliminarlo." }, null);

        await _files.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
        await _catalog.ReloadAsync(cancellationToken).ConfigureAwait(false);
        await _events.EmitAsync(OfficeEventType.OfficeActivityUpdated, $"Agente eliminado: {current.Name}").ConfigureAwait(false);
        return new AgentSaveResultDto(true, Array.Empty<string>(), null);
    }

    private async Task<AgentSaveResultDto> ReloadAsync(string id, string activity, CancellationToken ct)
    {
        var report = await _catalog.ReloadAsync(ct).ConfigureAwait(false);
        var problems = report.Issues.Where(i => i.IsError && string.Equals(i.AgentId, id, StringComparison.OrdinalIgnoreCase)).Select(i => i.Message).ToList();
        if (problems.Count > 0) return new AgentSaveResultDto(false, problems, null);

        // Solo el nombre en la actividad: las instrucciones no se registran (RNF-001).
        await _events.EmitAsync(OfficeEventType.OfficeActivityUpdated, activity).ConfigureAwait(false);
        var item = _definitions.GetDefinition(id) is { } d ? ToItem(d, _usage.GetSnapshot().ByAgent) : null;
        return new AgentSaveResultDto(true, Array.Empty<string>(), item);
    }

    private static AgentUpsertDto Normalize(AgentUpsertDto a) => a with
    {
        Id = (a.Id ?? string.Empty).Trim(),
        Name = (a.Name ?? string.Empty).Trim(),
        Role = (a.Role ?? string.Empty).Trim(),
        Purpose = (a.Purpose ?? string.Empty).Trim(),
        Description = string.IsNullOrWhiteSpace(a.Description) ? (a.Purpose ?? string.Empty).Trim() : a.Description.Trim(),
        Kind = (a.Kind ?? AgentKinds.Llm).Trim().ToLowerInvariant(),
        Environment = (a.Environment ?? AgentEnvironments.Office).Trim().ToLowerInvariant(),
        Tools = (a.Tools ?? Array.Empty<string>()).Select(t => t.Trim().ToLowerInvariant()).Where(t => t.Length > 0).Distinct().ToList(),
        Knowledge = (a.Knowledge ?? string.Empty).Trim(),
        Color = string.IsNullOrWhiteSpace(a.Color) ? "#8b5cf6" : a.Color.Trim(),
        Icon = string.IsNullOrWhiteSpace(a.Icon) ? "🤖" : a.Icon.Trim(),
        Instructions = (a.Instructions ?? string.Empty).Replace("\r\n", "\n").Trim()
    };
}

#endregion
