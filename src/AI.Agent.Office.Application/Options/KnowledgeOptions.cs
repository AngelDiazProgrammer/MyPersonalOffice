namespace AI.Agent.Office.Application.Options;

/// <summary>Parámetros de la base de conocimiento y de su flujo (sección "Knowledge").</summary>
public sealed class KnowledgeOptions
{
    public const string SectionName = "Knowledge";

    public bool Enabled { get; set; } = true;

    /// <summary>Carpeta con los archivos Markdown de la base de conocimiento.</summary>
    public string Directory { get; set; } = "bases de conocimiento";

    /// <summary>
    /// Carpeta donde se coloca el formato de entrega (APA, plantilla...). El archivo más reciente es el
    /// formato activo.
    /// </summary>
    public string FormatDirectory { get; set; } = "formatos de entrega";

    /// <summary>Puntaje mínimo para que un mensaje entre al flujo de la base (un código como SEG-003 siempre entra).</summary>
    public double MinScore { get; set; } = 4;

    /// <summary>Cuántas situaciones candidatas puede elegir el investigador.</summary>
    public int MaxCandidates { get; set; } = 3;

    /// <summary>Máximo de caracteres del formato que se entrega a los agentes.</summary>
    public int MaxFormatChars { get; set; } = 6000;

    /// <summary>Estilo del Word y PDF generados: Auto (APA cuando el formato activo menciona APA), Apa o Default.</summary>
    public string DocumentStyle { get; set; } = "Auto";

    /// <summary>
    /// true: el flujo de la base puede usar las APIs gratuitas aunque el caso venga de un documento
    /// adjunto. false: toda tarea que lleve texto del documento se envía solo a Claude.
    /// </summary>
    public bool AllowFreeApisWithDocuments { get; set; } = true;

    /// <summary>
    /// true: tras identificar el caso se muestran las soluciones alternativas de la base y la persona elige
    /// una (o escribe la suya). false: se entrega directamente el procedimiento recomendado.
    /// </summary>
    public bool OfferSolutionChoice { get; set; } = true;

    /// <summary>Mínimo de soluciones que necesita una situación para ofrecer una elección.</summary>
    public int MinSolutionsForChoice { get; set; } = 2;

    /// <summary>Caracteres del texto de los documentos adjuntos que se usan para buscar en la base.</summary>
    public int MaxQueryDocumentChars { get; set; } = 4000;

    // Un agente exclusivo por proceso del flujo (ids de los agentes de la carpeta agents/).
    /// <summary>Lee los documentos adjuntos y le entrega el resumen del caso al investigador.</summary>
    public string ReaderAgentId { get; set; } = "pdf-reader";
    public string ResearcherAgentId { get; set; } = "kb-researcher";
    public string ResponderAgentId { get; set; } = "incident-responder";
    public string FormatterAgentId { get; set; } = "word-writer";
    public string AuditorAgentId { get; set; } = "compliance-auditor";
}
