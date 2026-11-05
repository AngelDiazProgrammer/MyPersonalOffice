using System.Globalization;
using System.Text.RegularExpressions;
using AI.Agent.Office.Application.Agents;
using AI.Agent.Office.Application.Interfaces;
using AI.Agent.Office.Domain.Enums;
using AI.Agent.Office.Domain.ValueObjects;
using AI.Agent.Office.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Infrastructure.Agents;

/// <summary>
/// Reads agent roles from Markdown files. Optional YAML-like front matter (between --- lines) carries metadata;
/// the body is the system prompt. New agents are added by registering a file, never by writing C#.
/// </summary>
public sealed partial class MarkdownAgentConfigurationService : IAgentConfigurationService
{
    private readonly AgentsOptions _options;
    private readonly ILogger<MarkdownAgentConfigurationService> _logger;
    private readonly SemaphoreSlim _reloadGate = new(1, 1);
    private volatile ConfigurationReport _current = new(Array.Empty<AgentDefinition>(), Array.Empty<ConfigurationIssue>());

    public MarkdownAgentConfigurationService(IOptions<AgentsOptions> options, ILogger<MarkdownAgentConfigurationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public IReadOnlyList<AgentDefinition> GetDefinitions() => _current.Agents;

    public AgentDefinition? GetDefinition(string agentId) =>
        _current.Agents.FirstOrDefault(a => string.Equals(a.Id, agentId, StringComparison.OrdinalIgnoreCase));

    public string? GetInstructions(string agentId) => GetDefinition(agentId)?.Instructions;

    public ConfigurationReport Validate() => _current;

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

    private IEnumerable<AgentRegistration> ResolveRegistrations(string directory)
    {
        if (_options.Definitions.Count > 0) return _options.Definitions;

        return Directory.EnumerateFiles(directory, "*.md")
            .Where(f => !string.Equals(Path.GetFileName(f), "README.md", StringComparison.OrdinalIgnoreCase))
            .Select(f => new AgentRegistration { Id = Path.GetFileNameWithoutExtension(f), File = Path.GetFileName(f) });
    }

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
                double.TryParse(meta.GetValueOrDefault("temperature"), NumberStyles.Float, CultureInfo.InvariantCulture, out var t) ? t : null),
            workingStatus,
            isOrchestrator,
            ParseBool(meta.GetValueOrDefault("acceptsDocuments")),
            meta.GetValueOrDefault("color") ?? "#8b5cf6",
            meta.GetValueOrDefault("icon") ?? "🤖");
    }

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

    private static bool ParseBool(string? value) => bool.TryParse(value, out var b) && b;

    private static string ToTitle(string id) =>
        CultureInfo.InvariantCulture.TextInfo.ToTitleCase(id.Replace('-', ' '));

    [GeneratedRegex("^[a-z0-9][a-z0-9-]*$")]
    private static partial Regex IdPattern();
}
