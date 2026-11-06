using AI.Agent.Office.Domain.Entities;

namespace AI.Agent.Office.Application.Interfaces;

/// <summary>Situación de la base encontrada por una búsqueda, con su puntaje de afinidad.</summary>
public sealed record KnowledgeHit(KnowledgeEntry Entry, double Score);

/// <summary>La base de conocimiento de la empresa: archivos Markdown en la carpeta "bases de conocimiento".</summary>
public interface IKnowledgeBaseService
{
    /// <summary>(Re)lee todos los archivos Markdown. Devuelve cuántas situaciones se cargaron.</summary>
    Task<int> ReloadAsync(CancellationToken cancellationToken = default);

    IReadOnlyList<KnowledgeEntry> GetAll();
    KnowledgeEntry? GetByCode(string code);

    /// <summary>
    /// Situaciones que mejor coinciden con el texto (un código como "SEG-003" gana de inmediato), de mejor
    /// a peor.
    /// </summary>
    IReadOnlyList<KnowledgeHit> Search(string query, int maxResults);
}

/// <summary>Formato de entrega activo: nombre del archivo y su texto.</summary>
public sealed record DeliveryFormat(string FileName, string Text);

/// <summary>
/// El formato de entrega que deben seguir las respuestas: el archivo más reciente de la carpeta
/// "formatos de entrega".
/// </summary>
public interface IDeliveryFormatService
{
    Task<DeliveryFormat?> GetActiveFormatAsync(CancellationToken cancellationToken = default);
}
