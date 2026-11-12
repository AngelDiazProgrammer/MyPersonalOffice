using System.Text;
using AI.Agent.Office.Application.Agents;
using AI.Agent.Office.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Agent.Office.Infrastructure.Agents;

/// <summary>
/// Escribe y borra los archivos <c>agents/*.md</c> (spec 011). La escritura es atómica (archivo temporal y reemplazo), el id nunca forma una
/// ruta y, al editar, se conservan las claves del encabezado que el formulario no maneja (por ejemplo <c>workingStatus</c> o <c>acceptsDocuments</c>).
/// </summary>
public sealed class MarkdownAgentFileStore : IAgentFileStore
{
    // Claves que gestiona el formulario, en el orden en que se escriben.
    private static readonly string[] Managed =
        { "name", "role", "description", "purpose", "kind", "environment", "tools", "knowledge", "sensitive", "color", "icon", "maxTokens" };

    private readonly AgentsOptions _options;

    public MarkdownAgentFileStore(IOptions<AgentsOptions> options) => _options = options.Value;

    private string Directory => OfficeConfiguration.ResolveDirectory(_options.Directory)
        ?? throw new DirectoryNotFoundException($"No se encontró la carpeta de agentes '{_options.Directory}'.");

    private string PathOf(string id)
    {
        // El id ya se validó (minúsculas, números y guiones), pero se vuelve a impedir que forme una ruta.
        if (id.Contains('/') || id.Contains('\\') || id.Contains("..")) throw new ArgumentException("Id de agente no válido.");
        return Path.Combine(Directory, id + ".md");
    }

    public async Task WriteAsync(AgentUpsertDto agent, bool create, CancellationToken cancellationToken)
    {
        var path = PathOf(agent.Id);
        if (create && File.Exists(path)) throw new InvalidOperationException($"Ya existe el archivo del agente «{agent.Id}».");

        // Claves existentes que el formulario no maneja.
        var extra = new List<KeyValuePair<string, string>>();
        if (!create && File.Exists(path))
        {
            var (meta, _) = MarkdownAgentConfigurationService.SplitFrontMatter(await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false));
            extra.AddRange(meta.Where(kv => !Managed.Contains(kv.Key, StringComparer.OrdinalIgnoreCase)));
        }

        var sb = new StringBuilder("---\n");
        void Line(string key, string? value) { if (!string.IsNullOrWhiteSpace(value)) sb.Append(key).Append(": ").Append(OneLine(value)).Append('\n'); }

        Line("name", agent.Name);
        Line("role", agent.Role);
        Line("description", agent.Description);
        Line("purpose", agent.Purpose);
        Line("kind", agent.Kind);
        Line("environment", agent.Environment);
        Line("tools", agent.Tools is { Count: > 0 } ? string.Join(", ", agent.Tools) : null);
        Line("knowledge", agent.Knowledge);
        Line("sensitive", agent.Sensitive ? "true" : null);
        Line("color", agent.Color);
        Line("icon", agent.Icon);
        Line("maxTokens", agent.MaxTokens?.ToString());
        foreach (var (key, value) in extra) Line(key, value);
        sb.Append("---\n").Append(agent.Instructions.Trim()).Append('\n');

        var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        await File.WriteAllTextAsync(temp, sb.ToString(), new UTF8Encoding(false), cancellationToken).ConfigureAwait(false);
        File.Move(temp, path, overwrite: true);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var path = PathOf(id);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    /// <summary>Un valor del encabezado va en una sola línea y sin comillas que el lector recortaría.</summary>
    private static string OneLine(string value) => value.Replace("\r", " ").Replace("\n", " ").Replace("\"", "'").Trim();
}
