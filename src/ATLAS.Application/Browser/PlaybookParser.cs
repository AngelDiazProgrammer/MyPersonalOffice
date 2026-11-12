using System.Text;
using System.Text.RegularExpressions;

namespace ATLAS.Application.Browser;

/// <summary>
/// Lee y escribe playbooks en Markdown.
/// <code>
/// ---
/// nombre: Enviar correo
/// disparadores: enviar correo, mandar correo
/// ---
/// ## Entradas
/// - destinatario | ¿A quién va dirigido? | correo
/// - cuerpo | redactar | Cuerpo formal del correo
/// ## Pasos
/// 1. ir | https://mail.google.com
/// 2. clic | text=Redactar | Abrir la redacción
/// 3. escribir | input[aria-label="Para"] | {{destinatario}}
/// 4. clic | div[aria-label^="Enviar"] | | Enviar | irreversible
/// 5. Comprueba que apareció la confirmación de envío   (sin «|»: paso libre que decide el modelo)
/// </code>
/// </summary>
public static class PlaybookParser
{
    public const int MaxSteps = 40;
    public const int MaxInputs = 12;
    public const string ReviewMarker = "{{REVISAR}}";

    private static readonly Regex Variable = new(@"\{\{\s*([^{}\s]+)\s*\}\}", RegexOptions.Compiled);
    private static readonly Regex Leading = new(@"^\s*(?:\d+[.)]|[-*])\s*", RegexOptions.Compiled);

    private static readonly Dictionary<string, StepAction> Actions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ir"] = StepAction.Goto, ["abrir"] = StepAction.Goto,
        ["clic"] = StepAction.Click, ["click"] = StepAction.Click,
        ["escribir"] = StepAction.Fill, ["llenar"] = StepAction.Fill,
        ["teclear"] = StepAction.Press, ["tecla"] = StepAction.Press,
        ["esperar"] = StepAction.Wait,
        ["seleccionar"] = StepAction.Select,
        ["verificar"] = StepAction.Verify, ["comprobar"] = StepAction.Verify,
        ["extraer"] = StepAction.Extract, ["leer"] = StepAction.Extract
    };

    private static readonly Dictionary<StepAction, string> ActionNames = new()
    {
        [StepAction.Goto] = "ir", [StepAction.Click] = "clic", [StepAction.Fill] = "escribir", [StepAction.Press] = "teclear",
        [StepAction.Wait] = "esperar", [StepAction.Select] = "seleccionar", [StepAction.Verify] = "verificar", [StepAction.Extract] = "extraer"
    };

    #region Lectura

    /// <summary>Lee un playbook y lo valida; devuelve los problemas con su número de línea.</summary>
    public static PlaybookParseResult Parse(string siteId, string markdown)
    {
        var errors = new List<string>();
        var title = string.Empty;
        var triggers = new List<string>();
        var inputs = new List<PlaybookInput>();
        var steps = new List<PlaybookStep>();

        var lines = (markdown ?? string.Empty).Replace("\r\n", "\n").Split('\n');
        var i = 0;

        // Cabecera opcional entre «---».
        if (lines.Length > 0 && lines[0].Trim() == "---")
        {
            i = 1;
            while (i < lines.Length && lines[i].Trim() != "---")
            {
                var colon = lines[i].IndexOf(':');
                if (colon > 0)
                {
                    var key = lines[i][..colon].Trim().ToLowerInvariant();
                    var value = lines[i][(colon + 1)..].Trim();
                    if (key is "nombre" or "título" or "titulo") title = value;
                    else if (key is "disparadores" or "triggers")
                        triggers.AddRange(value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                }
                i++;
            }
            i++;
        }

        var section = string.Empty;
        for (; i < lines.Length; i++)
        {
            var raw = lines[i];
            var line = raw.Trim();
            if (line.Length == 0) continue;

            if (line.StartsWith('#'))
            {
                var heading = line.TrimStart('#').Trim().ToLowerInvariant();
                if (line.StartsWith("# ") && !line.StartsWith("## ") && title.Length == 0) title = line[2..].Trim();
                else if (heading is "entradas" or "preguntas") section = "inputs";
                else if (heading is "pasos") section = "steps";
                continue;
            }

            if (section == "inputs") ParseInput(line, i + 1, inputs, errors);
            else if (section == "steps") ParseStep(line, i + 1, steps, errors);
        }

        Validate(siteId, inputs, steps, errors);
        if (title.Length == 0) title = siteId;

        var playbook = new Playbook(siteId, title, triggers, inputs, steps);
        return new PlaybookParseResult(errors.Count == 0 ? playbook : null, errors);
    }

    private static void ParseInput(string line, int number, List<PlaybookInput> inputs, List<string> errors)
    {
        var parts = Leading.Replace(line, string.Empty).Split('|', StringSplitOptions.TrimEntries);
        if (parts.Length < 2 || parts[0].Length == 0 || !Regex.IsMatch(parts[0], @"^[a-zA-Z][a-zA-Z0-9_-]*$"))
        {
            errors.Add($"Línea {number}: la entrada debe ser «nombre | pregunta» o «nombre | redactar | instrucción».");
            return;
        }

        if (inputs.Any(x => x.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add($"Línea {number}: la entrada «{parts[0]}» está repetida.");
            return;
        }

        if (parts[1].Equals("redactar", StringComparison.OrdinalIgnoreCase))
            inputs.Add(new PlaybookInput(parts[0], parts[0], true, parts.Length > 2 ? parts[2] : null));
        else
            inputs.Add(new PlaybookInput(parts[0], parts[1], false, null, parts.Length > 2 && parts[2].Length > 0 ? parts[2].ToLowerInvariant() : null));
    }

    private static void ParseStep(string line, int number, List<PlaybookStep> steps, List<string> errors)
    {
        var body = Leading.Replace(line, string.Empty);
        if (body.Length == 0) return;

        // Sin «|» es un paso libre (lo decide el modelo mirando la página).
        if (!body.Contains('|'))
        {
            steps.Add(new PlaybookStep(StepAction.Explore, body));
            return;
        }

        var parts = body.Split('|', StringSplitOptions.TrimEntries);
        if (!Actions.TryGetValue(parts[0], out var action))
        {
            errors.Add($"Línea {number}: acción desconocida «{parts[0]}» (usa ir, clic, escribir, teclear, esperar, seleccionar o verificar).");
            return;
        }

        var target = parts.Length > 1 ? parts[1] : string.Empty;
        var value = parts.Length > 2 && parts[2].Length > 0 ? parts[2] : null;
        string? note = null;
        var irreversible = false;
        for (var k = 3; k < parts.Length; k++)
        {
            if (parts[k].Equals("irreversible", StringComparison.OrdinalIgnoreCase)) irreversible = true;
            else if (parts[k].Length > 0 && note is null) note = parts[k];
        }

        if (action != StepAction.Press && target.Length == 0)
            errors.Add($"Línea {number}: la acción «{parts[0]}» necesita un objetivo (selector, URL o texto).");
        else if (action is StepAction.Fill or StepAction.Select && value is null)
            errors.Add($"Línea {number}: la acción «{parts[0]}» necesita un valor.");
        else if (action == StepAction.Press && target.Length == 0 && value is null)
            errors.Add($"Línea {number}: «teclear» necesita una tecla (por ejemplo «teclear | | Enter»).");
        else
            steps.Add(new PlaybookStep(action, target, value, note, irreversible));
    }

    private static void Validate(string siteId, List<PlaybookInput> inputs, List<PlaybookStep> steps, List<string> errors)
    {
        if (steps.Count == 0) errors.Add("El playbook no tiene pasos (sección «## Pasos»).");
        if (steps.Count > MaxSteps) errors.Add($"El playbook tiene más de {MaxSteps} pasos.");
        if (inputs.Count > MaxInputs) errors.Add($"El playbook tiene más de {MaxInputs} entradas.");

        var known = new HashSet<string>(inputs.Select(x => x.Name), StringComparer.OrdinalIgnoreCase) { "credencial.usuario", "credencial.clave" };
        for (var n = 0; n < steps.Count; n++)
        {
            var step = steps[n];
            if (step.Action == StepAction.Goto && step.Target.Length > 0 && !step.Target.Contains("{{")
                && !(Uri.TryCreate(step.Target, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https"))
                errors.Add($"Paso {n + 1}: «ir» necesita una URL http o https.");

            foreach (var text in new[] { step.Target, step.Value, step.Note })
            {
                if (text is null) continue;
                foreach (Match m in Variable.Matches(text))
                    if (!known.Contains(m.Groups[1].Value) && !m.Value.Equals(ReviewMarker, StringComparison.Ordinal))
                        errors.Add($"Paso {n + 1}: la variable {{{{{m.Groups[1].Value}}}}} no existe en «## Entradas».");
            }
        }
    }

    #endregion

    #region Escritura

    /// <summary>Genera el Markdown de un playbook (se usa para las propuestas aprendidas).</summary>
    public static string Render(Playbook playbook, string? comment = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.AppendLine($"nombre: {playbook.Title}");
        sb.AppendLine($"disparadores: {string.Join(", ", playbook.Triggers)}");
        sb.AppendLine("---");
        sb.AppendLine($"# {playbook.Title}");
        if (!string.IsNullOrWhiteSpace(comment)) sb.AppendLine().AppendLine($"> {comment}");
        sb.AppendLine().AppendLine("## Entradas");
        foreach (var input in playbook.Inputs)
        {
            if (input.Draft) sb.AppendLine($"- {input.Name} | redactar | {input.Instruction}");
            else sb.AppendLine($"- {input.Name} | {input.Question}{(input.Kind is null ? string.Empty : " | " + input.Kind)}");
        }

        sb.AppendLine().AppendLine("## Pasos");
        var number = 1;
        foreach (var step in playbook.Steps)
        {
            if (step.Action == StepAction.Explore)
            {
                sb.AppendLine($"{number++}. {step.Target}");
                continue;
            }

            var fields = new List<string> { ActionNames[step.Action], step.Target, step.Value ?? string.Empty };
            if (step.Note is not null || step.Irreversible) fields.Add(step.Note ?? string.Empty);
            if (step.Irreversible) fields.Add("irreversible");
            sb.AppendLine($"{number++}. {string.Join(" | ", fields).TrimEnd(' ', '|')}");
        }

        return sb.ToString();
    }

    /// <summary>Plantilla de un playbook nuevo: solo abre la URL del sitio.</summary>
    public static string Template(string name, string url) =>
        $"---\nnombre: {name}\ndisparadores: \n---\n# {name}\n\n## Entradas\n\n## Pasos\n1. ir | {url}\n";

    /// <summary>Sustituye {{variable}} por su valor; null si falta alguna variable.</summary>
    public static string? Substitute(string? text, Func<string, string?> resolve, out string? missing)
    {
        missing = null;
        if (text is null) return null;
        string? lost = null;
        var result = Variable.Replace(text, m =>
        {
            var value = resolve(m.Groups[1].Value);
            if (value is null) lost ??= m.Groups[1].Value;
            return value ?? string.Empty;
        });
        missing = lost;
        return result;
    }

    #endregion
}
