using System.Text.Json;
using System.Text.Json.Serialization;
using AI.Agent.Office.Application.DTOs;
using AI.Agent.Office.Domain.Enums;
using TaskStatus = AI.Agent.Office.Domain.Enums.TaskStatus;

namespace AI.Agent.Office.Tests;

/// <summary>
/// Spec 005 (RF-003, D-9): fija la forma pública de los DTO. Se serializa una instancia canónica con las opciones de la API
/// y se compara con <c>Contracts/*.json</c>: falla si un campo desaparece, se renombra o cambia de tipo (agregar campos es compatible).
/// Para regenerar las instantáneas tras un cambio aprobado: variable de entorno <c>UPDATE_CONTRACTS=1</c>.
/// </summary>
public class ContractTests
{
    private static readonly JsonSerializerOptions Api = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly Guid Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static AgentDto Agent() => new("dev", "Dev", "desc", "rol", AgentStatus.Idle, Id, "tarea", "agents/dev.md", Now, "#fff", "x", false, true, "modelo");
    private static AgentTaskDto Task1() => new(Id, Id, "dev", "t1", "Título", "desc", TaskStatus.Completed, Now, Now, Now, "res", null, new[] { "t0" }, Id, "doc.docx", "claude");
    private static TokenUsageDto Usage() => new(1, 2, 3, new[] { new AgentUsageDto("dev", 1, 2, 3) }, 0.5m, 4, 5, 6, new[] { new ProviderUsageDto("groq", true, 1, 2, 3) });
    private static GeneratedDocumentDto Doc() => new(Id, "informe.docx", 10);
    private static KnowledgeChoiceDto Choice() => new("C1", "Caso", "s1", new[] { new SolutionOptionDto("s1", "Sol", "Resumen", true) });

    public static IEnumerable<object[]> Contracts() => new[]
    {
        new object[] { "ChatResponseDto", new ChatResponseDto(Id, "hola", true, false, "plan", new[] { Task1() }, new[] { Doc() }, Choice()) },
        new object[] { "AgentDto", Agent() },
        new object[] { "AgentTaskDto", Task1() },
        new object[] { "WorkspacesDto", new WorkspacesDto(new[] { new WorkspaceDto("w", "Oficina", "i", "desc", new[] { new WorkspaceAgentDto("dev", "Dev", "x", "rol") }) }, "w", false) },
        new object[] { "LlmStatusDto", new LlmStatusDto("auto", new[] { new FreeProviderStatusDto("groq", "us", "m", true, false, null) }, true) },
        new object[] { "OfficeStateDto", new OfficeStateDto(new[] { Agent() }, new[] { Task1() }, new[] { new OfficeActivityDto(Id, "dev", Id, OfficeEventType.TaskCompleted, "ok", Now) }, Usage(), true, "w") },
        new object[] { "GeneratedDocumentDto", Doc() },
        new object[] { "KnowledgeChoiceDto", Choice() },
        new object[] { "TokenUsageDto", Usage() },
    };

    private static string SnapshotPath(string name)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "AI.Agent.Office.sln"))) dir = dir.Parent;
        return Path.Combine(dir!.FullName, "tests", "AI.Agent.Office.Tests", "Contracts", name + ".json");
    }

    /// <summary>Recorre el JSON y devuelve «ruta → tipo» (los elementos de una lista se unifican en <c>[]</c>).</summary>
    private static void Shape(JsonElement e, string path, Dictionary<string, string> into)
    {
        switch (e.ValueKind)
        {
            case JsonValueKind.Object:
                into[path] = "object";
                foreach (var p in e.EnumerateObject()) Shape(p.Value, path + "." + p.Name, into);
                break;
            case JsonValueKind.Array:
                into[path] = "array";
                foreach (var item in e.EnumerateArray()) Shape(item, path + "[]", into);
                break;
            case JsonValueKind.True or JsonValueKind.False: into[path] = "boolean"; break;
            default: into[path] = e.ValueKind.ToString().ToLowerInvariant(); break;
        }
    }

    [Theory]
    [MemberData(nameof(Contracts))]
    public void Public_contract_keeps_every_field_of_the_approved_snapshot(string name, object dto)
    {
        var actualJson = JsonSerializer.Serialize(dto, dto.GetType(), Api);
        var path = SnapshotPath(name);
        if (Environment.GetEnvironmentVariable("UPDATE_CONTRACTS") == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, actualJson);
        }

        Assert.True(File.Exists(path), $"Falta la instantánea {path} (ejecuta con UPDATE_CONTRACTS=1 tras aprobar el contrato).");
        var expected = new Dictionary<string, string>();
        var actual = new Dictionary<string, string>();
        Shape(JsonDocument.Parse(File.ReadAllText(path)).RootElement, "$", expected);
        Shape(JsonDocument.Parse(actualJson).RootElement, "$", actual);

        var broken = expected.Where(kv => !actual.TryGetValue(kv.Key, out var kind) || (kind != kv.Value && kv.Value != "null" && kind != "null")).Select(kv => kv.Key).ToList();
        Assert.True(broken.Count == 0, $"El contrato {name} perdió o cambió de tipo: {string.Join(", ", broken)}");
    }
}
