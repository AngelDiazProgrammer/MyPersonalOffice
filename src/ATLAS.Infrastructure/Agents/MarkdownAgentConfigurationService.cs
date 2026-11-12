using System.Globalization;
using System.Text.RegularExpressions;
using ATLAS.Application.Agents;
using ATLAS.Application.Interfaces;
using ATLAS.Domain.Enums;
using ATLAS.Domain.ValueObjects;
using ATLAS.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATLAS.Infrastructure.Agents;

/// <summary>
/// Lee los roles de los agentes desde archivos Markdown. Un encabezado opcional tipo YAML (entre líneas
/// ---) trae los metadatos y el cuerpo es el prompt del sistema. Los agentes nuevos se agregan
/// registrando un archivo, nunca escribiendo C#.
/// </summary>
public sealed partial class MarkdownAgentConfigurationService : IAgentConfigurationService
{
    private readonly AgentsOptions _options;
    private readonly ILogger<MarkdownAgentConfigurationService> _logger;
    private readonly SemaphoreSlim _reloadGate = new(1, 1);
    private volatile ConfigurationReport _current = new(Array.Empty<AgentDefinition>(), Array.Empty<ConfigurationIssue>());

    /// <summary>Crea el servicio con la carpeta y el registro de agentes configurados.</summary>
    public MarkdownAgentConfigurationService(IOptions<AgentsOptions> options, ILogger<MarkdownAgentConfigurationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>Definiciones de todos los agentes cargados.</summary>
    public IReadOnlyList<AgentDefinition> GetDefinitions() => _current.Agents;

    /// <summary>Definición de un agente por su id.</summary>
    public AgentDefinition? GetDefinition(string agentId) =>
        _current.Agents.FirstOrDefault(a => string.Equals(a.Id, agentId, StringComparison.OrdinalIgnoreCase));

    /// <summary>Instrucciones (prompt) de un agente por su id.</summary>
    public string? GetInstructions(string agentId) => GetDefinition(agentId)?.Instructions;

    /// <summary>Último informe de validación.</summary>
    public ConfigurationReport Validate() => _current;

    /// <summary>
    /// Relee los archivos, valida cada agente (orquestador único, ids válidos, sin duplicados) y actualiza
    /// la configuración.
    /// </summary>
    public async Task<ConfigurationReport> ReloadAsync(CancellationToken cancellationToken = default)
    {
        await _reloadGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var issues = new List<ConfigurationIssue>();
            var definitions = new List<AgentDefinition>();
            var directory = OfficeConfiguration.ResolveDirectory(_options.Directory);

            if (directory is null)
            {
                issues.Add(new ConfigurationIssue("*", $"No se encontró la carpeta de agentes '{_options.Directory}'.", true));
            }
            else
            {
                foreach (var registration in ResolveRegistrations(directory))
                {
                    var definition = await LoadOneAsync(directory, registration, issues, cancellationToken).ConfigureAwait(false);
                    if (definition is null) continue;

                    if (definitions.Any(d => string.Equals(d.Id, definition.Id, StringComparison.OrdinalIgnoreCase)))
                    {
                        issues.Add(new ConfigurationIssue(definition.Id, "Id de agente duplicado; se ignora.", true));
                        continue;
                    }
                    definitions.Add(definition);
                }
            }

            var orchestrators = definitions.Count(d => d.IsOrchestrator);
            if (orchestrators == 0) issues.Add(new ConfigurationIssue("*", "Falta el agente orquestador.", true));
            if (orchestrators > 1) issues.Add(new ConfigurationIssue("*", "Solo puede existir un orquestador.", true));

            _current = new ConfigurationReport(definitions, issues);
            _logger.LogInformation("Configuración de agentes cargada: {Count} agentes, {Issues} incidencias", definitions.Count, issues.Count);
            return _current;
        }
        finally
        {
            _reloadGate.Release();
        }
    }

    /// <summary>Agentes a cargar: los registrados en la configuración o, si no hay, todos los .md de la carpeta.</summary>
    private IEnumerable<AgentRegistration> ResolveRegistrations(string directory)
    {
        if (_options.Definitions.Count > 0) return _options.Definitions;

        return Directory.EnumerateFiles(directory, "*.md")
            .Where(f => !string.Equals(Path.GetFileName(f), "README.md", StringComparison.OrdinalIgnoreCase))
            .Select(f => new AgentRegistration { Id = Path.GetFileNameWithoutExtension(f), File = Path.GetFileName(f) });
    }

    /// <summary>Carga y valida un archivo de agente; devuelve null si es inválido.</summary>
    private async Task<AgentDefinition?> LoadOneAsync(
        string directory, AgentRegistration registration, List<ConfigurationIssue> issues, CancellationToken ct)
    {
        var id = registration.Id?.Trim() ?? string.Empty;
        if (!IdPattern().IsMatch(id))
        {
            issues.Add(new ConfigurationIssue(id, "El id debe usar solo minúsculas, números y guiones.", true));
            return null;
        }

        var fileName = Path.GetFileName(registration.File); // never allow path traversal
        var path = Path.Combine(directory, fileName);
        if (!File.Exists(path))
        {
            issues.Add(new ConfigurationIssue(id, $"No existe el archivo Markdown '{fileName}'.", true));
            return null;
        }

        var content = await File.ReadAllTextAsync(path, ct).ConfigureAwait(false);
        var (meta, body) = SplitFrontMatter(content);

        if (string.IsNullOrWhiteSpace(body))
        {
            issues.Add(new ConfigurationIssue(id, $"El archivo '{fileName}' no contiene instrucciones.", true));
            return null;
        }

        var name = meta.GetValueOrDefault("name") ?? ToTitle(id);
        var workingStatus = Enum.TryParse<AgentStatus>(meta.GetValueOrDefault("workingStatus"), true, out var ws) ? ws : AgentStatus.Working;
        var isOrchestrator = ParseBool(meta.GetValueOrDefault("isOrchestrator")) || (id == "orchestrator" && !meta.ContainsKey("isOrchestrator"));

        return new AgentDefinition(
            id,
            name,
            meta.GetValueOrDefault("description") ?? string.Empty,
            meta.GetValueOrDefault("role") ?? name,
            fileName,
            body.Trim(),
            new AgentModelSettings(
                meta.GetValueOrDefault("model"),
                int.TryParse(meta.GetValueOrDefault("maxTokens"), out var mt) ? mt : null,
                double.TryParse(meta.GetValueOrDefault("temperature"), NumberStyles.Float, CultureInfo.InvariantCulture, out var t) ? t : null,
                meta.GetValueOrDefault("provider"),
                meta.GetValueOrDefault("freeModel")),
            workingStatus,
            isOrchestrator,
            ParseBool(meta.GetValueOrDefault("acceptsDocuments")),
            meta.GetValueOrDefault("color") ?? "#8b5cf6",
            meta.GetValueOrDefault("icon") ?? "🤖",
            string.Equals(meta.GetValueOrDefault("producesDocument"), "word", StringComparison.OrdinalIgnoreCase),
            ParseBool(meta.GetValueOrDefault("workflowOnly")),
            // Spec 011: uso específico, tipo, entorno, herramientas, base de conocimiento y datos personales. Los agentes anteriores siguen
            // cargando: su uso sale de la descripción.
            meta.GetValueOrDefault("purpose") ?? meta.GetValueOrDefault("description") ?? string.Empty,
            Pick(meta.GetValueOrDefault("kind"), AgentKinds.All, AgentKinds.Llm),
            Pick(meta.GetValueOrDefault("environment"), AgentEnvironments.All, AgentEnvironments.Office),
            (meta.GetValueOrDefault("tools") ?? string.Empty).Trim('[', ']').Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.ToLowerInvariant()).Distinct().ToList(),
            meta.GetValueOrDefault("knowledge") ?? string.Empty,
            ParseBool(meta.GetValueOrDefault("sensitive")),
            ParseBool(meta.GetValueOrDefault("system")) || SystemAgents.IsSystem(id));
    }

    /// <summary>Separa los metadatos del encabezado del cuerpo del archivo.</summary>
    internal static (Dictionary<string, string> Meta, string Body) SplitFrontMatter(string content)
    {
        var meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var text = content.TrimStart('﻿').Replace("\r\n", "\n");
        if (!text.StartsWith("---\n", StringComparison.Ordinal)) return (meta, text);

        var end = text.IndexOf("\n---", 4, StringComparison.Ordinal);
        if (end < 0) return (meta, text);

        foreach (var line in text[4..end].Split('\n'))
        {
            var idx = line.IndexOf(':');
            if (idx <= 0 || line.TrimStart().StartsWith('#')) continue;
            meta[line[..idx].Trim()] = line[(idx + 1)..].Trim().Trim('"', '\'');
        }

        var bodyStart = text.IndexOf('\n', end + 1);
        return (meta, bodyStart < 0 ? string.Empty : text[(bodyStart + 1)..]);
    }

    /// <summary>El valor si está en la lista permitida (sin distinguir mayúsculas); si no, el predeterminado.</summary>
    private static string Pick(string? value, string[] allowed, string fallback) =>
        allowed.FirstOrDefault(a => string.Equals(a, value?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? fallback;

    /// <summary>Lee un valor booleano de los metadatos (false si no es válido).</summary>
    private static bool ParseBool(string? value) => bool.TryParse(value, out var b) && b;

    /// <summary>Convierte un id ("qa-analyst") en un nombre legible.</summary>
    private static string ToTitle(string id) =>
        CultureInfo.InvariantCulture.TextInfo.ToTitleCase(id.Replace('-', ' '));

    [GeneratedRegex("^[a-z0-9][a-z0-9-]*$")]
    private static partial Regex IdPattern();
}
