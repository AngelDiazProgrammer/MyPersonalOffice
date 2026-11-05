namespace AI.Agent.Office.Domain.Entities;

/// <summary>Text extracted from an uploaded document. Kept in memory only.</summary>
public sealed class OfficeDocument
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string FileName { get; init; }
    public int PageCount { get; init; }
    public required string Text { get; init; }
    public bool Truncated { get; init; }
    public bool RequiresOcr { get; init; }
    public DateTimeOffset UploadedAt { get; init; } = DateTimeOffset.UtcNow;
}
