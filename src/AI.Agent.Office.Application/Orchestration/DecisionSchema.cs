using System.Text.Json;
using AI.Agent.Office.Application.Interfaces;

namespace AI.Agent.Office.Application.Orchestration;

public static class DecisionSchema
{
    public const string ToolName = "submit_decision";

    public static StructuredOutputSpec Build(IEnumerable<string> delegableAgentIds, int maxTasks)
    {
        using var stream = new MemoryStream();
        using (var w = new Utf8JsonWriter(stream))
        {
            w.WriteStartObject();
            w.WriteString("type", "object");
            w.WriteStartObject("properties");

            w.WriteStartObject("action");
            w.WriteString("type", "string");
            w.WriteStartArray("enum");
            w.WriteStringValue(DecisionActions.Respond);
            w.WriteStringValue(DecisionActions.Delegate);
            w.WriteEndArray();
            w.WriteString("description", "respond: contestas tú directamente. delegate: asignas tareas a agentes especializados.");
            w.WriteEndObject();

            w.WriteStartObject("message");
            w.WriteString("type", "string");
            w.WriteString("description",
                "Si action=respond: la respuesta completa para el usuario (Markdown). Si action=delegate: una o dos frases que expliquen el plan al usuario.");
            w.WriteEndObject();

            w.WriteStartObject("tasks");
            w.WriteString("type", "array");
            w.WriteNumber("maxItems", maxTasks);
            w.WriteStartObject("items");
            w.WriteString("type", "object");
            w.WriteStartObject("properties");

            w.WriteStartObject("id");
            w.WriteString("type", "string");
            w.WriteString("description", "Identificador corto único dentro del plan (t1, t2...).");
            w.WriteEndObject();

            w.WriteStartObject("agentId");
            w.WriteString("type", "string");
            w.WriteStartArray("enum");
            foreach (var id in delegableAgentIds) w.WriteStringValue(id);
            w.WriteEndArray();
            w.WriteEndObject();

            w.WriteStartObject("title");
            w.WriteString("type", "string");
            w.WriteString("description", "Título corto de la tarea (máx. 60 caracteres).");
            w.WriteEndObject();

            w.WriteStartObject("task");
            w.WriteString("type", "string");
            w.WriteString("description", "Instrucciones completas y autocontenidas para el agente.");
            w.WriteEndObject();

            w.WriteStartObject("dependsOn");
            w.WriteString("type", "array");
            w.WriteStartObject("items");
            w.WriteString("type", "string");
            w.WriteEndObject();
            w.WriteString("description", "Ids de tareas cuyo resultado necesita esta tarea. Vacío si es independiente.");
            w.WriteEndObject();

            w.WriteStartObject("includeDocuments");
            w.WriteString("type", "boolean");
            w.WriteString("description", "true si el agente necesita el texto de los documentos adjuntos.");
            w.WriteEndObject();

            w.WriteEndObject(); // properties
            w.WriteStartArray("required");
            w.WriteStringValue("id");
            w.WriteStringValue("agentId");
            w.WriteStringValue("title");
            w.WriteStringValue("task");
            w.WriteEndArray();
            w.WriteEndObject(); // items
            w.WriteEndObject(); // tasks

            w.WriteEndObject(); // properties
            w.WriteStartArray("required");
            w.WriteStringValue("action");
            w.WriteStringValue("message");
            w.WriteEndArray();
            w.WriteEndObject();
        }

        return new StructuredOutputSpec(
            ToolName,
            "Registra la decisión del Orquestador: responder directamente o delegar tareas a los agentes.",
            System.Text.Encoding.UTF8.GetString(stream.ToArray()));
    }
}
