using System.Collections.Concurrent;
using ATLAS.Application.Interfaces;
using ATLAS.Domain.Entities;
using ATLAS.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATLAS.Infrastructure.FileSystem;

/// <summary>Guarda los documentos generados en una carpeta local, con retención limitada y nombres saneados.</summary>
public sealed class GeneratedDocumentStore : IGeneratedDocumentStore
{
    private readonly GeneratedDocumentsOptions _options;
    private readonly ILogger<GeneratedDocumentStore> _logger;
    private readonly ConcurrentDictionary<Guid, GeneratedDocument> _documents = new();
    private readonly Lazy<string> _directory;

    /// <summary>Crea el almacén con la carpeta y la retención configuradas.</summary>
    public GeneratedDocumentStore(IOptions<GeneratedDocumentsOptions> options, ILogger<GeneratedDocumentStore> logger)
    {
        _options = options.Value;
        _logger = logger;
        _directory = new Lazy<string>(() =>
        {
            var path = OfficeConfiguration.ResolveWritableDirectory(_options.Directory);
            Directory.CreateDirectory(path);
            return path;
        });
    }

    /// <summary>
    /// Guarda el archivo con un prefijo único y borra los vencidos. Nunca confía en el nombre recibido:
    /// descarta cualquier ruta.
    /// </summary>
    public async Task<GeneratedDocument> SaveAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        Cleanup();

        var safeName = Path.GetFileName(fileName); // never trust a name: strip any directory part
        var id = Guid.NewGuid();
        var path = Path.Combine(_directory.Value, $"{id:N}_{safeName}");
        await File.WriteAllBytesAsync(path, content, cancellationToken).ConfigureAwait(false);

        var document = new GeneratedDocument { Id = id, FileName = safeName, FilePath = path, SizeBytes = content.LongLength };
        _documents[id] = document;
        return document;
    }

    /// <summary>Busca un documento generado por su id (null si venció o no existe).</summary>
    public GeneratedDocument? Get(Guid id) =>
        _documents.TryGetValue(id, out var document) && File.Exists(document.FilePath) ? document : null;

    /// <summary>Borra los documentos cuya retención venció.</summary>
    private void Cleanup()
    {
        var limit = DateTimeOffset.UtcNow.AddHours(-Math.Max(1, _options.RetentionHours));
        foreach (var expired in _documents.Values.Where(d => d.CreatedAt < limit).ToList())
        {
            _documents.TryRemove(expired.Id, out _);
            try { File.Delete(expired.FilePath); }
            catch (Exception ex) { _logger.LogDebug(ex, "No se pudo borrar {Path}", expired.FilePath); }
        }
    }
}
