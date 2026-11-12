namespace ATLAS.Application.Browser;

#region Sitios

/// <summary>Un sitio web al que la oficina puede conectarse. Headless null = usa el valor global.</summary>
public sealed record BrowserSite(
    string Id,
    string Name,
    string Url,
    IReadOnlyList<string> AllowedDomains,
    bool? Headless = null,
    bool Enabled = true);

#endregion

#region Playbook

/// <summary>Acciones que puede ejecutar un paso del playbook.</summary>
public enum StepAction { Goto, Click, Fill, Press, Wait, Select, Verify, Explore, Extract }

/// <summary>
/// Dato de entrada del playbook: se le pregunta a la persona o lo redacta el modelo (<paramref name="Draft"/>).
/// </summary>
/// <param name="Kind">Tipo opcional («correo» extrae la dirección del mensaje).</param>
public sealed record PlaybookInput(string Name, string Question, bool Draft, string? Instruction, string? Kind = null);

/// <summary>Un paso del playbook. Explore = texto libre que decide el modelo mirando la página.</summary>
public sealed record PlaybookStep(StepAction Action, string Target, string? Value = null, string? Note = null, bool Irreversible = false);

/// <summary>Playbook de un sitio: disparadores del chat, entradas y pasos.</summary>
public sealed record Playbook(
    string SiteId,
    string Title,
    IReadOnlyList<string> Triggers,
    IReadOnlyList<PlaybookInput> Inputs,
    IReadOnlyList<PlaybookStep> Steps);

/// <summary>Resultado de leer un playbook: el playbook (si es válido) y los problemas encontrados.</summary>
public sealed record PlaybookParseResult(Playbook? Playbook, IReadOnlyList<string> Errors)
{
    public bool IsValid => Playbook is not null && Errors.Count == 0;
}

#endregion

#region Credenciales y ejecución

/// <summary>
/// Credenciales opcionales de una ejecución. Viven solo en memoria durante la ejecución; nunca se guardan,
/// registran ni envían a un modelo.
/// </summary>
public sealed class BrowserCredentials
{
    public string? Username { get; }
    public string? Password { get; }

    public BrowserCredentials(string? username, string? password)
    {
        Username = string.IsNullOrEmpty(username) ? null : username;
        Password = string.IsNullOrEmpty(password) ? null : password;
    }

    public bool HasAny => Username is not null || Password is not null;

    /// <summary>Nunca expone los valores (evita que acaben en un registro por accidente).</summary>
    public override string ToString() => "***";
}

/// <summary>Resultado de ejecutar un playbook.</summary>
/// <param name="Mode">«reproducción», «exploración» o «reparación».</param>
public sealed record BrowserRunResult(
    bool Success,
    string Message,
    long DurationMs,
    int StepsExecuted,
    string Mode,
    int ModelCalls,
    bool ProposalCreated,
    IReadOnlyList<string> Log,
    IReadOnlyList<ExtractedRow>? Data = null);

/// <summary>Fila leída de la página (por ejemplo un correo de la bandeja). Es contenido de la persona: nunca va a registros ni a modelos.</summary>
public sealed record ExtractedRow(string Text, string? Sender = null, string? Email = null, string? Subject = null, string? Snippet = null, string? Date = null);

/// <summary>Instantánea simplificada de la página para que el modelo decida el siguiente paso.</summary>
public sealed record PageSnapshot(string Url, string Title, IReadOnlyList<PageElement> Elements);

/// <summary>Elemento interactivo de la página: selector utilizable, rol, nombre visible y valor.</summary>
public sealed record PageElement(string Selector, string Role, string Name, string? Value = null);

#endregion

#region Excepciones

/// <summary>Se intentó salir del alcance permitido (dominio no autorizado, URL no válida). Aborta la ejecución sin reparar.</summary>
public sealed class BrowserSecurityException : Exception
{
    public BrowserSecurityException(string message) : base(message) { }
}

/// <summary>Un paso no se pudo ejecutar (elemento no encontrado, tiempo agotado...). Se puede intentar reparar.</summary>
public sealed class BrowserStepException : Exception
{
    public BrowserStepException(string message, Exception? inner = null) : base(message, inner) { }
}

/// <summary>El navegador o Playwright no están disponibles (por ejemplo, sin instalar).</summary>
public sealed class BrowserUnavailableException : Exception
{
    public BrowserUnavailableException(string message, Exception? inner = null) : base(message, inner) { }
}

/// <summary>Falta un dato necesario (credencial o entrada). Aborta la ejecución sin reparar.</summary>
public sealed class BrowserMissingValueException : Exception
{
    public BrowserMissingValueException(string message) : base(message) { }
}

#endregion
