namespace AI.Agent.Office.Application.Options;

/// <summary>
/// Parámetros de la orquestación (sección "Orchestration" de la configuración): límites, tiempos y
/// ahorro de tokens.
/// </summary>
public sealed class OrchestrationOptions
{
    public const string SectionName = "Orchestration";

    /// <summary>Máximo de tareas que el Orquestador puede delegar por solicitud.</summary>
    public int MaxTasksPerRequest { get; set; } = 6;
    /// <summary>Máximo de agentes trabajando a la vez.</summary>
    public int MaxConcurrency { get; set; } = 3;
    /// <summary>Tiempo máximo de una tarea, en segundos.</summary>
    public int TaskTimeoutSeconds { get; set; } = 300;
    /// <summary>Tiempo máximo de la planificación, en segundos.</summary>
    public int PlanningTimeoutSeconds { get; set; } = 120;
    /// <summary>Tiempo máximo de la consolidación final, en segundos.</summary>
    public int ConsolidationTimeoutSeconds { get; set; } = 180;

    /// <summary>La síntesis es corta: un tope bajo ahorra tokens y tiempo.</summary>
    public int ConsolidationMaxTokens { get; set; } = 1200;
    /// <summary>Máximo de caracteres de un mensaje del usuario.</summary>
    public int MaxMessageChars { get; set; } = 8000;
    /// <summary>Máximo de caracteres de documento que recibe cada tarea.</summary>
    public int MaxDocumentCharsPerTask { get; set; } = 60000;
    /// <summary>Tope de tokens de salida de un agente cuando su Markdown no define uno.</summary>
    public int DefaultAgentMaxTokens { get; set; } = 4096;
    /// <summary>Milisegundos que se muestra el estado "Pensando" antes de seguir (efecto visual).</summary>
    public int ThinkingDisplayMs { get; set; } = 700;
    /// <summary>Segundos que un agente muestra "Completado" antes de volver a reposo.</summary>
    public int CompletedDisplaySeconds { get; set; } = 4;
    /// <summary>Máximo de eventos guardados en la bitácora.</summary>
    public int MaxActivityEntries { get; set; } = 300;

    // ---- Token savings ----

    /// <summary>Turnos de la conversación que se envían al Orquestador (los más viejos se descartan).</summary>
    public int MaxHistoryMessages { get; set; } = 8;

    /// <summary>
    /// Los mensajes anteriores se acortan a este número de caracteres (el último mensaje del usuario nunca
    /// se corta).
    /// </summary>
    public int HistoryMessageMaxChars { get; set; } = 700;

    /// <summary>
    /// Tope de salida del JSON del plan (6 tareas necesitan ~1,5-2,5k tokens). Solo se cobran los tokens
    /// generados, así que un tope holgado no cuesta más.
    /// </summary>
    public int PlanningMaxTokens { get; set; } = 3072;

    /// <summary>
    /// Caracteres totales de resultados que ve el Orquestador para su síntesis (repartidos entre tareas,
    /// mínimo 3000 cada una).
    /// </summary>
    public int MaxResultCharsForConsolidation { get; set; } = 32000;

    /// <summary>Tope de caracteres de cada resultado previo que se pasa a una tarea dependiente.</summary>
    public int MaxDependencyResultChars { get; set; } = 6000;

    /// <summary>
    /// Si se delegó exactamente una tarea y salió bien, su respuesta se devuelve tal cual, en vez de pagar
    /// una segunda llamada a Claude que solo la reescribiría.
    /// </summary>
    public bool SkipConsolidationForSingleTask { get; set; } = true;
}
