namespace AI.Agent.Office.Application.Options;

public sealed class DocumentOptions
{
    public const string SectionName = "Documents";

    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    public int MaxPages { get; set; } = 200;
    public int MaxExtractedChars { get; set; } = 200_000;
    public string[] AllowedExtensions { get; set; } = { ".pdf" };
    public string[] AllowedContentTypes { get; set; } = { "application/pdf", "application/x-pdf", "application/octet-stream" };
    public int MaxDocumentsInMemory { get; set; } = 20;
}
