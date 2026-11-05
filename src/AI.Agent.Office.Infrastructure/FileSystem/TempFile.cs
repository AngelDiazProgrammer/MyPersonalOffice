namespace AI.Agent.Office.Infrastructure.FileSystem;

/// <summary>A temporary file that is always deleted when disposed. Documents are never stored permanently.</summary>
public sealed class TempFile : IAsyncDisposable
{
    public string Path { get; }

    private TempFile(string path) => Path = path;

    public static async Task<TempFile> CreateAsync(Stream content, string extension, CancellationToken cancellationToken)
    {
        var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ai-agent-office");
        Directory.CreateDirectory(directory);

        var path = System.IO.Path.Combine(directory, $"{Guid.NewGuid():N}{extension}");
        var temp = new TempFile(path);
        try
        {
            await using var target = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await content.CopyToAsync(target, cancellationToken).ConfigureAwait(false);
            return temp;
        }
        catch
        {
            await temp.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public ValueTask DisposeAsync()
    {
        try { if (File.Exists(Path)) File.Delete(Path); }
        catch (IOException) { /* best effort: the OS temp cleaner will remove it */ }
        catch (UnauthorizedAccessException) { }
        return ValueTask.CompletedTask;
    }
}
