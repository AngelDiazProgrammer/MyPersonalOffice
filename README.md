# AI Agent Office

Oficina virtual multiagente de uso personal. Conversas con un **Orquestador** que decide si responde él mismo o delega en agentes especialistas; todos ejecutan llamadas reales a Claude (API oficial de Anthropic) desde un backend .NET, y ves a los agentes trabajar en una oficina animada en tiempo real (SignalR).

- **Stack:** C#, .NET 8, ASP.NET Core Web API, Blazor Web App (Interactive Server), SignalR, HttpClientFactory, System.Text.Json.
- **Sin base de datos:** el estado vive en memoria mientras la app está en ejecución. Los roles de los agentes son archivos Markdown.
- **Seguridad:** la API key solo existe en el backend (variable de entorno / `.env`), nunca viaja al navegador.

## Requisitos

- [.NET SDK 8.0 o superior](https://dotnet.microsoft.com/download) (los proyectos apuntan a `net8.0`; necesitas el runtime de .NET 8).
- Una API key de Anthropic (<https://console.anthropic.com>).

## Configuración

1. Copia `.env.example` a `.env` y completa los valores (el archivo `.env` está en `.gitignore`):

   ```env
   ANTHROPIC_API_KEY=sk-ant-...
   ANTHROPIC_MODEL=claude-sonnet-5-5
   ```

   También puedes definirlas como variables de entorno del sistema (tienen prioridad sobre `.env`).
2. `ANTHROPIC_MODEL` es configurable: usa un identificador vigente de la [lista oficial de modelos](https://docs.anthropic.com/en/docs/about-claude/models). Cada agente puede sobrescribirlo con `model:` en su Markdown.
3. El resto de parámetros (límites, timeouts, concurrencia, agentes registrados) está en [`configuration/appsettings.json`](configuration/appsettings.json).

## Ejecución

```bash
dotnet run --project src/AI.Agent.Office.Web
```

Abre automáticamente <http://localhost:5080>. El proyecto Web aloja la API y el hub de SignalR **en el mismo proceso**, así que un solo comando levanta todo.

Desde Visual Studio: abre `AI.Agent.Office.sln` y ejecuta el perfil **AI Agent Office** del proyecto `AI.Agent.Office.Web`.

### API independiente (opcional)

```bash
dotnet run --project src/AI.Agent.Office.Api          # http://localhost:5081
```

Para que la UI la consuma en vez de alojarla: `Office__HostApi=false` y `Api__BaseUrl=http://localhost:5081`.

## Cómo se usa

- Escribe en el chat (Enter envía, Shift+Enter nueva línea). Prueba: *"Diseña la arquitectura de una API de pedidos y evalúa sus riesgos"*.
- El Orquestador responde directo a lo simple y delega lo complejo. Verás: tarea asignada (aro dorado), estado del agente (pensando → trabajando/programando/leyendo/revisando → completado), el feed de actividad y el panel de tareas (con resultado de cada agente y botón para cancelar).
- 📎 adjunta un **PDF** (máx. 10 MB): se extrae el texto localmente y el Lector PDF lo analiza. Los PDF escaneados se detectan y se informa que requieren OCR.

## Arquitectura

```
Web (Blazor) ──HTTP/SignalR──▶ Api ──▶ Application ──▶ Domain
                                  ╲          ▲
                                   ╲──▶ Infrastructure (implementa contratos de Application)
```

| Proyecto | Responsabilidad |
|---|---|
| `AI.Agent.Office.Domain` | Entidades (`Agent`, `AgentTask`, `Conversation`, `ChatMessage`, `OfficeActivity`, `OfficeDocument`), enums, objetos de valor, eventos y `IOfficeEventPublisher`. Sin dependencias. |
| `AI.Agent.Office.Application` | Orquestación (`OrchestrationService`, `PlanExecutor`), runner genérico de agentes, estado en memoria (singletons sincronizados), validadores, DTOs, interfaces (`IAnthropicService`, `IAgentConfigurationService`…). |
| `AI.Agent.Office.Infrastructure` | `AnthropicService` (HttpClientFactory, reintentos, límites), lector de Markdown, extractor PDF (PdfPig), archivos temporales, `.env`. |
| `AI.Agent.Office.Api` | Controladores finos, `OfficeHub` (SignalR), adaptador `SignalROfficeEventPublisher`, middleware de errores, CORS local. |
| `AI.Agent.Office.Web` | UI Blazor: oficina SVG animada con CSS, chat con Markdown, tareas y actividad. Sin lógica de negocio ni llamadas a Anthropic. |
| `AI.Agent.Office.Tests` | xUnit, sin API key. |

### Flujo de una solicitud

1. `POST /api/chat` → `OrchestrationService`.
2. El Orquestador recibe historial + mensaje y responde **mediante una llamada a herramienta forzada** (`submit_decision`, con JSON Schema): `{"action":"respond|delegate","message":…,"tasks":[{id,agentId,title,task,dependsOn,includeDocuments}]}`. No hay parsing de texto libre.
3. La decisión se valida (agentes existentes, máximo de tareas, ids únicos, dependencias sin ciclos). Si es inválida, se reintenta una vez devolviéndole los errores; si persiste, respuesta segura.
4. `PlanExecutor` ejecuta el plan: tareas independientes en paralelo, dependientes tras sus prerrequisitos (reciben su resultado), con concurrencia global limitada, un agente a la vez, timeout por tarea, aislamiento de errores y cancelación.
5. El Orquestador consolida los resultados en la respuesta final (si falla Claude al consolidar, se entregan los resultados directos).

Los agentes **no delegan** entre sí ni ejecutan comandos: solo el Orquestador coordina.

### Eventos en tiempo real

Todo cambio de estado pasa por `AgentStateService`, que actualiza el registro y emite un evento; la UI solo refleja eventos (nunca cambia estados por sí misma). Hub: `/hubs/office`, un método de cliente por evento: `AgentStarted`, `AgentThinking`, `AgentTaskAssigned`, `AgentProgress`, `AgentCompleted`, `AgentError`, `TaskCreated`, `TaskCompleted`, `OrchestrationStarted`, `OrchestrationCompleted`, `OfficeActivityUpdated`. Cada uno lleva `EventId, AgentId, TaskId, EventType, Description, Timestamp, AdditionalData` (estado del agente, título de tarea…).

### Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/chat` | Envía un mensaje; devuelve la respuesta consolidada y las tareas |
| POST | `/api/chat/{id}/cancel` | Cancela la solicitud en curso |
| GET | `/api/chat/{id}` | Conversación |
| GET | `/api/agents`, `/api/agents/{id}`, `/api/agents/{id}/activity` | Agentes e historial |
| POST | `/api/agents/reload` | Recarga los Markdown sin reiniciar |
| GET | `/api/tasks` · POST `/api/tasks/{id}/cancel` | Tareas / cancelar |
| POST | `/api/documents` | Sube un PDF (valida y extrae; no se guarda el archivo) |
| GET | `/api/office/state`, `/api/office/activity` | Estado completo / actividad |
| GET | `/health` | Salud |

## Agentes y roles en Markdown

Los roles viven en [`agents/`](agents). Cada archivo tiene metadatos opcionales (front matter) y el cuerpo es el *system prompt*:

```markdown
---
name: Desarrollador
role: Ingeniero de software
description: Especialista en C# y .NET…   # el Orquestador lo usa para elegir agente
workingStatus: Coding        # Working | Coding | Reading | Reviewing (animación mientras trabaja)
acceptsDocuments: false      # true => recibe el texto de los PDF adjuntos
isOrchestrator: false
model: claude-…              # opcional
maxTokens: 6144              # opcional
temperature: 0.3             # opcional
color: "#38bdf8"
icon: 💻
---
Instrucciones del agente…
```

**Agregar un agente sin tocar código:** crea `agents/mi-agente.md` y regístralo en `configuration/appsettings.json` (`Agents:Definitions`, o deja la lista vacía para autodescubrir todos los `.md`). Luego reinicia o llama a `POST /api/agents/reload`. El motor usa un único `AgentRunner` reutilizable: no hay una clase por agente.

## Pruebas

```bash
dotnet test
```

Cubren: carga/validación de Markdown, registro de agentes, validación de decisiones y dependencias, límites de concurrencia, timeouts y cancelación, cambios de estado, aislamiento de errores, consolidación, validación y extracción de PDF, y `AnthropicService` (reintentos, mapeo de errores, la key nunca en el body ni en errores) con un `HttpMessageHandler` simulado. Las **pruebas de integración con Anthropic** se ejecutan solo si `ANTHROPIC_API_KEY` está definida; si no, aparecen como omitidas.

## Seguridad

API key solo en backend; errores sin stack traces (middleware central); validación de mensajes, extensión/MIME/firma/tamaño de PDF; archivos temporales eliminados siempre; CORS restringido a orígenes locales; `AllowedHosts` limitado a localhost; Markdown renderizado sin HTML crudo y con enlaces filtrados; los agentes no ejecutan comandos del sistema.

## Limitaciones conocidas

- Estado solo en memoria: al reiniciar se pierden conversaciones, tareas y actividad.
- Sin OCR: los PDF escaneados se detectan pero no se leen.
- Sin streaming token a token: el chat recibe la respuesta consolidada al terminar.
- Un único usuario/sesión local; sin autenticación (pensado para `localhost`).
- Las animaciones son SVG/CSS 2D propias, no sprites de terceros.
