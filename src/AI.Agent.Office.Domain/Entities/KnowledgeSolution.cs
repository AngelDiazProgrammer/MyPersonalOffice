namespace AI.Agent.Office.Domain.Entities;

/// <summary>Una solución alternativa de una situación (por ejemplo "Solución B · Bloqueo temporal de la cuenta").</summary>
/// <param name="Id">Letra de la opción (A, B, C...).</param>
/// <param name="Title">Nombre corto de la solución.</param>
/// <param name="Recommended">True si el título la marca como "(recomendada)".</param>
/// <param name="Summary">Cuándo conviene (primera línea descriptiva).</param>
/// <param name="Body">Texto completo de la solución tal como está en el archivo.</param>
public sealed record KnowledgeSolution(string Id, string Title, bool Recommended, string Summary, string Body);
