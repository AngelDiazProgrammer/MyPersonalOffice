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

## Oficinas (instancias)

Al abrir la app **primero eliges una oficina**; solo se despliegan los agentes necesarios (menos desorden y lo personal queda aislado). Se define en `configuration/appsettings.json` → `Workspaces:Items` (id, nombre, icono, descripción y lista de agentes; el primero es el Orquestador):

| Oficina | Agentes |
|---|---|
| 💻 **Desarrollo** | Orquestador, Arquitecto, Desarrollador, Analista QA, Profesor |
| 📚 **Pruebas base de conocimiento** | Orquestador, Lector PDF, Investigador KB, Respondedor de incidentes, Auditor de cumplimiento, Redactor Word (que también hace de *editor de formato*) |

- La selección se guarda en el servidor (en memoria) y **se pide en cada arranque**; "cambiar de oficina" (cabecera) vuelve a la pantalla de selección y limpia la conversación.
- Solo los agentes de la oficina activa participan: planificación del Orquestador, flujo de la base de conocimiento, panel, tareas y escena 3D (los escritorios sobrantes se ocultan). Sin `Workspaces` configurado no hay pantalla de selección y todos los agentes están disponibles.
- API: `GET /api/workspaces`, `POST /api/workspaces/select` (`{"id":"base-conocimiento"}`), `POST /api/workspaces/reset`. Mientras no haya una oficina elegida, `POST /api/chat` responde 400.

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
| `AI.Agent.Office.Web` | UI Blazor: oficina 3D (three.js) animada, chat con Markdown, tareas y actividad. Sin lógica de negocio ni llamadas a Anthropic. |
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

## APIs gratuitas primero y Claude solo si hace falta

La app usa **un único servicio global de APIs gratuitas** (`FreeApiService`): cada proveedor es solo una entrada en `Llm:FreeProviders` de `configuration/appsettings.json` (nombre, URL, variable de la clave, modelo, región). **Claude es el único proveedor aparte**, el de pago.

| Modo (selector del encabezado) | Comportamiento |
|---|---|
| **Gratis primero** (por defecto) | Prueba los proveedores gratuitos en orden; si ninguno puede (límite de uso, error, respuesta inservible, prompt demasiado grande), escala a Claude |
| **Solo Claude** | Como antes |
| **Solo gratis** | Nunca llama a Claude |

- **Proveedores incluidos:** Groq (`openai/gpt-oss-120b`), Cloudflare Workers AI (`@cf/meta/llama-3.3-70b-instruct-fp8-fast`) y Google Gemini (`gemini-flash-lite-latest`; `gemini-3.8-flash` suele dar 503 por alta demanda, clave en `GEMINI_API_KEY`, se obtiene en https://aistudio.google.com/apikey; su capa gratuita puede usar el contenido para mejorar los productos de Google, así que no la uses con datos confidenciales). Ambos usan el formato compatible con OpenAI; agregar otro es copiar una entrada.
- **Privacidad:** las solicitudes con **documentos adjuntos nunca van a APIs gratuitas** (solo a Claude). `Llm:AllowedRegions` permite restringir por región (por ejemplo `["US"]`). Se descartaron los planes gratuitos que pueden entrenar con tus datos (Gemini free, Mistral free, OpenRouter free) y los que no se pudieron verificar.
- **Límites de uso:** si un proveedor responde 429, se pone en pausa (respetando `Retry-After`) y se pasa al siguiente. Los prompts mayores que `MaxInputTokens` del proveedor van directo al siguiente / Claude.
- **Claves** (en `.env`, ver `.env.example`): `GROQ_API_KEY`, `CLOUDFLARE_API_TOKEN` + `CLOUDFLARE_ACCOUNT_ID`, y `ANTHROPIC_API_KEY`. Con solo una de ellas la app ya funciona.
- **Qué ve el usuario:** cada tarea muestra 🆓 (API gratuita) o ☁️ (Claude); el contador separa tokens gratuitos de los de Claude. Endpoints: `GET /api/llm/status`, `POST /api/llm/mode`.
- **Por agente** (front matter): `provider: claude` fuerza Claude; `freeModel: <id>` pide otro modelo a los proveedores gratuitos.

## Base de conocimiento y formato de entrega

- **Base de conocimiento:** archivos `.md` en [`bases de conocimiento/`](bases%20de%20conocimiento). Cada situación es un encabezado `## SEG-001 · Título` con `**Severidad:**`, `**Palabras clave:**` y su procedimiento (plazo, acciones, escalamiento, qué no hacer...). Incluye 5 situaciones ficticias de seguridad (SEG-001 a SEG-005). Un archivo con `confidential: true` en su cabecera solo se envía a Claude. Se recarga con `POST /api/knowledge/reload`; `GET /api/knowledge?query=...` muestra qué situación coincidiría.
- **Formato de entrega:** carpeta [`formatos de entrega/`](formatos%20de%20entrega). El archivo **más reciente** (`.md`, `.txt`, `.docx` o `.pdf`) es el formato activo; solo hay que soltar uno nuevo. Se incluye un ejemplo estilo APA.
- **Consulta ANTES de responder:** cuando un mensaje coincide con la base (por código como `SEG-003` o por palabras clave), la app no deja que un modelo decida el camino: ejecuta un flujo fijo con **un agente exclusivo por proceso**:
  1. 📚 *Investigador KB* identifica la situación (recibe las candidatas reales de la base).
  2. 🛡️ *Respondedor de Incidentes* redacta la respuesta solo con ese procedimiento.
  3. 📝 *Redactor Word* (también editor de formato) la entrega bajo el formato activo.
  4. ✅ *Auditor de Cumplimiento* verifica concordancia con la base y el formato (y corrige si hace falta).
- **Verificación automática (no depende del modelo):** el sistema extrae del texto final los tickets, formularios, teléfonos, correos, extensiones y plazos y comprueba que existan en la base. Si hay datos inventados, el auditor recibe UNA tarea de corrección con los hallazgos; si aún quedan, la respuesta lo advierte. Los agentes reciben además la lista blanca de datos permitidos.
- **Respuesta:** formato entregado + línea de fuente (`📚 SEG-005 — … · ✔ Verificación … · 🧾 Formato …`). Investigador, Respondedor y Auditor son internos (`workflowOnly`) y no aparecen en la planificación libre.
- **Entregable APA (Word + PDF):** al terminar, el sistema genera automáticamente el documento final en **.docx** y **.pdf** y los muestra como chips descargables en el chat. El título siempre es `Respuesta al incidente {CÓDIGO}: {título}` (el sistema lo normaliza aunque el modelo copie otro). `Knowledge:DocumentStyle`: `Auto` (APA si el formato activo menciona "APA"), `Apa` o `Default`.
  - **Estilo APA 7:** carta (Letter) con márgenes de 1", Times New Roman 12, interlineado doble, portada (título en negrita + autor/fecha), número de página arriba a la derecha, títulos de nivel 1 centrados en negrita, nivel 2 alineado a la izquierda en negrita, nivel 3 negrita cursiva, sangría de primera línea de 0,5" y **Referencias en página aparte** (con sangría francesa en el Word; en el PDF van alineadas a la izquierda porque QuestPDF no soporta sangría francesa).
- En la oficina 3D el investigador **no se levanta**: los libros vuelan de la estantería a su escritorio.
- **La base de conocimiento es siempre el primer filtro:** se busca con el mensaje **y** con el texto de los PDF adjuntos (y en el futuro con cualquier fuente: buzones, apps). Si hay coincidencia, corre el flujo fijo; con adjuntos, el **Lector PDF va primero**: lee y analiza el documento y le entrega el resumen del caso al Investigador KB (solo el lector recibe el texto del PDF); si no, sigue la planificación normal.
- **Seguimiento con la persona (elegir solución):** cada situación de la base trae **5 soluciones** (`### Solución A · Título`, la recomendada lleva `(recomendada)`). El flujo ahora tiene dos fases:
  1. *Investigar:* el Investigador (con el resumen del Lector si hay PDF) identifica el caso. El chat responde con lo entendido y las opciones (armadas por el sistema desde la base, sin inventar) y **espera**; aún no hay Word ni PDF.
  2. *Entregar:* la persona elige con un botón (A–E), escribe la letra o "la recomendada", o escribe **su propia solución** en el campo de texto (el Auditor la contrasta con la base y la corrige si contradice una norma). Entonces el Respondedor redacta solo esa solución, el Redactor la formatea y el Auditor la verifica; se generan Word y PDF. "Cancelar" cierra el caso.
  - Si el mensaje siguiente trata de otra situación o trae otro PDF, se abre un caso nuevo. Una respuesta ambigua se repregunta sin gastar modelos.
  - Flag `Knowledge:OfferSolutionChoice` (`true`): con `false` se entrega el procedimiento recomendado directamente. Las situaciones con menos de 2 soluciones tampoco preguntan.
- **Flag `Knowledge:AllowFreeApisWithDocuments`** (`true` por defecto): con `true` el texto del PDF puede ir a las APIs gratuitas; con `false` toda tarea que lleve texto del PDF va solo a Claude. Las entradas confidenciales de la base siempre van solo a Claude.

## Documentos Word y PDF

- **Agente Redactor Word** (`agents/word-writer.md`): escribe en Markdown y el sistema lo convierte a un `.docx` real con Open XML (estilos, títulos, listas numeradas, tablas, código, citas, enlaces, pie con número de página; A4). Se descarga con el chip 📝 del chat o de la tarea.
- **Exportar a Word / PDF** cualquier respuesta del chat con los botones 📄 y 📕 (`POST /api/documents/word` y `POST /api/documents/pdf`, cuerpo `{title, markdown, style?}` con `style: "apa"` para APA). El PDF usa [QuestPDF](https://www.questpdf.com) con **licencia Community** (gratis para particulares y empresas con menos de 1 M USD de ingresos anuales; revisa su licencia si tu caso es otro).
- Los archivos se guardan en `generated-documents/` (24 h de retención, nombres saneados). Descarga: `GET /api/documents/generated/{id}`.
- Cuando el Redactor termina, el personaje 3D va a la **impresora** antes de entregar el documento.

## Oficina 3D: interacciones

**Reglas de movimiento:** mientras hay trabajo, solo caminan el **Orquestador** (asigna tareas, va a la pizarra) y el agente que **entrega** su resultado al siguiente; el resto trabaja sentado con animaciones propias de su rol (libros, lupa, portapapeles, símbolos flotantes). Cuando se entrega la respuesta, el Orquestador recorre el equipo y **felicita a todos los agentes que participaron** (👏 ¡Bien hecho!), y al final todos celebran con confeti. Solo cuando **todos están libres** vuelven los cafés, las charlas y los estiramientos; si llega trabajo nuevo, esas caminatas sociales se cancelan.

Personajes con rasgos propios (peinado, ropa, accesorios), 8 puestos, TV con el estado en vivo, reloj en hora real, impresora, cafetera con vapor, estantería, sofá y sala de reuniones. Reaccionan a eventos reales: entrega de tareas, resultados pasados entre agentes (con choque de manos), impresión de documentos, partículas de código al programar, signos de pregunta al pensar, confeti al terminar, y en reposo toman café, charlan, se estiran o miran el móvil. **Clic en un personaje**: la cámara lo sigue y aparece su ficha (estado, tarea, historial, tareas gratis/Claude).

## Ahorro de tokens

Todo es configurable en `configuration/appsettings.json` (secciones `Orchestration` y `Anthropic`), sin perder la delegación:

- **Sin consolidación redundante:** si se delega una sola tarea y sale bien, su respuesta se entrega tal cual (1 llamada menos y sin reescribirla). Con varias tareas se consolida igualmente (`SkipConsolidationForSingleTask`).
- **Prompt caching** de Anthropic en los prompts de sistema (`PromptCaching`); los tokens de caché se cuentan en el contador.
- **Historial acotado:** solo los últimos 8 turnos (`MaxHistoryMessages`) y los turnos antiguos se acortan a 700 caracteres (`HistoryMessageMaxChars`).
- **Prompts compactos** (Orquestador, protocolo y agentes) y agentes instruidos para responder de forma directa y concisa.
- **Límites de salida:** planificación 1024 tokens (`PlanningMaxTokens`), resultados previos recortados al pasarlos a otro agente o al consolidar (`MaxDependencyResultChars`, `MaxResultCharsForConsolidation`).
- **Modelo por agente:** en el front matter de un agente puedes poner `model:` (por ejemplo, un modelo más barato solo para el Orquestador).
- El encabezado muestra los tokens totales y por agente (clic para el detalle).

## Vista 3D

La oficina es una escena WebGL real (three.js r160, incluida en `wwwroot/lib/three`, MIT, funciona sin internet): luces y sombras, ciudad nocturna en las ventanas, pantallas animadas según el estado de cada agente y personajes que caminan, se sientan y se pasan trabajo según los eventos reales. Arrastra para rotar, rueda para zoom, doble clic para restablecer la cámara. Requiere un navegador con WebGL.

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

## Convenciones del código

- Comentarios y documentación en **español**: resúmenes XML (`<summary>`) en clases y métodos, y comentarios de línea solo donde la lógica no es obvia.
- Las clases con varias responsabilidades se separan con `#region` / `#endregion` por funcionalidad (por ejemplo "Planificación del Orquestador", "Verificación automática de datos"). En JavaScript se usa `// #region`.
- Las clases pequeñas (un solo propósito) no llevan regiones.

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
- Los planes gratuitos tienen límites de uso y pueden cambiar; la app escala a Claude cuando se agotan.
- Sin streaming token a token: el chat recibe la respuesta consolidada al terminar.
- Un único usuario/sesión local; sin autenticación (pensado para `localhost`).
- Personajes y muebles 3D son figuras procedurales propias (low-poly), no modelos de terceros.
