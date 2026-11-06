namespace AI.Agent.Office.Domain.Entities;

/// <summary>
/// Documento producido por la oficina (por ejemplo un Word o un PDF). El archivo vive en disco hasta
/// que vence su retención.
/// </summary>
public sealed class GeneratedDocument
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string FileName { get; init; }
    public required string FilePath { get; init; }
    public long SizeBytes { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
