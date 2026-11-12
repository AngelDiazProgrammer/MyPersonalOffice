# ATLAS

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
dotnet run --project src/ATLAS.Web
```

Abre automáticamente <http://localhost:5080>. El proyecto Web aloja la API y el hub de SignalR **en el mismo proceso**, así que un solo comando levanta todo.

Desde Visual Studio: abre `ATLAS.sln` y ejecuta el perfil **ATLAS** del proyecto `ATLAS.Web`.

### API independiente (opcional)

```bash
dotnet run --project src/ATLAS.Api          # http://localhost:5081
```

Para que la UI la consuma en vez de alojarla: `Office__HostApi=false` y `Api__BaseUrl=http://localhost:5081`.

## Plataforma: acceso y secciones
La aplicación pide **usuario y contraseña** (cookie de sesión HttpOnly, 8 h, bloqueo 5 min tras 5 fallos). El administrador inicial está en [`configuration/usuarios.md`](configuration/usuarios.md) **como hash PBKDF2** (la contraseña no se guarda en claro). Para cambiarla o agregar a alguien: `dotnet run --project src/ATLAS.Web -- --hash-contrasena "<clave>"` y pegar la línea en la tabla.

Menú lateral: **Oficinas virtuales** (Desarrollo, Base de conocimiento), **Chatbots** (los bots de `playbooks bots/`, ejecutados por el agente LectorUML), **Agentes conversacionales** (CamilaIA), **Agentes** (catálogo y gestión: crear, editar y eliminar, cada uno con su uso específico), **Conexiones** y **Logs** (registro en vivo en memoria, sin datos personales). Camila tiene su propio entorno; las oficinas de agentes conservan su comportamiento. Detalle: [specs/010-plataforma](specs/010-plataforma/spec.md).

## Oficinas (instancias)

Al abrir la app **primero eliges una oficina**; solo se despliegan los agentes necesarios (menos desorden y lo personal queda aislado). Se define en `configuration/appsettings.json` → `Workspaces:Items` (id, nombre, icono, descripción y lista de agentes; el primero es el Orquestador):

| Oficina | Agentes |
|---|---|
| 💻 **Desarrollo** | Orquestador, Arquitecto, Desarrollador, Analista QA, Profesor |
| 📚 **Pruebas base de conocimiento** | Orquestador, Lector PDF, Investigador KB, Respondedor de incidentes, Auditor de cumplimiento, Redactor Word (que también hace de *editor de formato*) |

- La selección se guarda en el servidor (en memoria) y **se pide en cada arranque**; "cambiar de oficina" (cabecera) vuelve a la pantalla de selección y limpia la conversación.
- Solo los agentes de la oficina activa participan: planificación del Orquestador, flujo de la base de conocimiento, panel, tareas y oficina 2D (los puestos sobrantes no se muestran). Sin `Workspaces` configurado no hay pantalla de selección y todos los agentes están disponibles.
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

`Web` **no referencia ni ve `Infrastructure`**: usa solo contratos de `Application` (por ejemplo `IMarkdownRenderer`); `Api` es la raíz de composición que los conecta. Lo verifican las pruebas de arquitectura.

| Proyecto | Responsabilidad |
|---|---|
| `ATLAS.Domain` | Entidades (`Agent`, `AgentTask`, `Conversation`, `ChatMessage`, `OfficeActivity`, `OfficeDocument`), enums, objetos de valor, eventos y `IOfficeEventPublisher`. Sin dependencias. |
| `ATLAS.Application` | Orquestación (`OrchestrationService`, `PlanExecutor`), runner genérico de agentes, estado en memoria (singletons sincronizados), validadores, DTOs, interfaces (`IAnthropicService`, `IAgentConfigurationService`…). |
| `ATLAS.Infrastructure` | `AnthropicService` (HttpClientFactory, reintentos, límites), lector de Markdown, extractor PDF (PdfPig), archivos temporales, `.env`. |
| `ATLAS.Api` | Controladores finos, `OfficeHub` (SignalR), adaptador `SignalROfficeEventPublisher`, middleware de errores, CORS local. |
| `ATLAS.Web` | UI Blazor: oficina 2D (un puesto por agente con su estado en vivo), chat con Markdown, tareas, actividad y página «Conexiones». Sin lógica de negocio ni llamadas a Anthropic. |
| `ATLAS.Tests` | xUnit, sin API key. |

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
| GET/POST/PUT/DELETE | `/api/browser/sites[/{id}]` | Sitios de la conectividad a navegadores (spec 007) |
| GET/PUT | `/api/browser/sites/{id}/playbook` | Lee / guarda el playbook `.md` del sitio (se valida) |
| PUT/POST/DELETE | `/api/browser/sites/{id}/playbook/proposal[/approve]` | Propuesta aprendida: editar, aprobar (la activa) o descartar |
| POST | `/api/browser/sites/{id}/login` | Abre el navegador visible para iniciar sesión a mano |
| POST | `/api/browser/sites/{id}/runs` | Ejecución directa con valores y credenciales opcionales (no se guardan) |
| POST | `/api/browser/pending/{conversationId}/approve` · `/cancel` | Aprueba o cancela el borrador pendiente del chat |
| GET | `/health` | Vivo: la aplicación responde |
| GET | `/health/ready` | Listo: 200 si hay agentes válidos, base de conocimiento cargada (si está activa) y al menos un proveedor de modelos; 503 si no. Solo estado por componente, sin rutas ni claves |

### Registros

La consola emite **JSON, una línea por evento**, con hora UTC y alcances `CorrelationId` (generado por el servidor para cada solicitud), `ConversationId`, `TaskId` y `AgentId`. Cada llamada a un modelo registra proveedor, modelo, duración y tokens; **nunca** el texto de los mensajes ni de las respuestas.

### Conectividad a navegadores (Playwright)

La oficina puede operar sitios web por ti con Playwright: configuras el sitio en **🌐 Conexiones** (URL y, si quieres, usuario y clave, que se guardan en un `.md` por conexión, en texto plano, solo para pruebas), inicias sesión una vez a mano, y cada sitio tiene un playbook `playbooks/<id>.md` con sus preguntas y pasos. En el chat, un disparador («enviar correo») inicia las preguntas; el modelo redacta, **te muestra el borrador y solo ejecuta si lo apruebas**. El sistema aprende los procesos nuevos y los repite sin modelo. Los playbooks de **solo lectura** (por ejemplo «leer mis últimos 20 correos», `playbooks/gmail.md`) se ejecutan al pedirlos, sin aprobación, y muestran los resultados en una tabla. Explicación completa y paso a paso: [docs/conexiones-navegador.md](docs/conexiones-navegador.md).

### Documentación operativa

- [Conexiones a navegadores](docs/conexiones-navegador.md): sitios, playbooks, aprendizaje y seguridad.
- [Preparación para producción](docs/preparacion-produccion.md): lista comprobable antes de exponer la aplicación.
- [Índice de decisiones](docs/decisiones.md): decisiones de diseño de cada especificación.

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
- **La base de conocimiento es siempre el primer filtro:** se busca con el mensaje **y** con el texto de los PDF adjuntos (y en el futuro con cualquier fuente: buzones, apps). Si hay coincidencia, corre el flujo fijo; con adjuntos, el **Lector PDF va primero**: lee y analiza el documento y le entrega el resumen del caso al Investigador KB (solo el lector recibe el texto del PDF); si no, sigue la planificación normal.
- **Seguimiento con la persona (elegir solución):** cada situación de la base trae **5 soluciones** (`### Solución A · Título`, la recomendada lleva `(recomendada)`). El flujo ahora tiene dos fases:
  1. *Investigar:* el Investigador (con el resumen del Lector si hay PDF) identifica el caso. El chat responde con lo entendido y las opciones (armadas por el sistema desde la base, sin inventar) y **espera**; aún no hay Word ni PDF.
  2. *Entregar:* la persona elige con un botón (A–E), escribe la letra o "la recomendada", o escribe **su propia solución** en el campo de texto (el Auditor la contrasta con la base y la corrige si contradice una norma). Entonces el Respondedor redacta solo esa solución, el Redactor la formatea y el Auditor la verifica; se generan Word y PDF. "Cancelar" cierra el caso.
  - Si el mensaje siguiente trata de otra situación o trae otro PDF, se abre un caso nuevo. Una respuesta ambigua se repregunta sin gastar modelos.
  - Flag `Knowledge:OfferSolutionChoice` (`true`): con `false` se entrega el procedimiento recomendado directamente. Las situaciones con menos de 2 soluciones tampoco preguntan.
- **Respaldo del Lector PDF** (`Knowledge:RetryReaderWithClaude`, `true` por defecto): si el Lector PDF falla con las APIs gratuitas, su tarea se reintenta **una sola vez con Claude** antes de interrumpir el flujo. El Investigador nunca recibe el documento directamente; si Claude tampoco responde (o estás en modo "Solo gratis"), el flujo se interrumpe como siempre.
- **Flag `Knowledge:AllowFreeApisWithDocuments`** (`true` por defecto): con `true` el texto del PDF puede ir a las APIs gratuitas; con `false` toda tarea que lleve texto del PDF va solo a Claude. Las entradas confidenciales de la base siempre van solo a Claude.

## Documentos Word y PDF

- **Agente Redactor Word** (`agents/word-writer.md`): escribe en Markdown y el sistema lo convierte a un `.docx` real con Open XML (estilos, títulos, listas numeradas, tablas, código, citas, enlaces, pie con número de página; A4). Se descarga con el chip 📝 del chat o de la tarea.
- **Exportar a Word / PDF** cualquier respuesta del chat con los botones 📄 y 📕 (`POST /api/documents/word` y `POST /api/documents/pdf`, cuerpo `{title, markdown, style?}` con `style: "apa"` para APA). El PDF usa [QuestPDF](https://www.questpdf.com) con **licencia Community** (gratis para particulares y empresas con menos de 1 M USD de ingresos anuales; revisa su licencia si tu caso es otro).
- Los archivos se guardan en `generated-documents/` (24 h de retención, nombres saneados). Descarga: `GET /api/documents/generated/{id}`.

## Oficina 2D

Cada agente de la oficina activa tiene un puesto con su avatar, rol, **estado en vivo** (en reposo, pensando, trabajando, leyendo, programando, revisando, esperando, completado, error) y la tarea actual. Haz clic en un puesto para ver su ficha: estado, tarea, historial reciente y tareas respondidas por APIs gratuitas o Claude (y cancelar la tarea actual). Solo refleja eventos reales; respeta `prefers-reduced-motion`.

## Camila IA: bots, LectorUML y CamilaIA
El bot Camila (export de Dialogflow CX en `Bots/`, ignorado por git porque trae credenciales) queda descrito en un **playbook maestro** (`playbooks bots/camila/`: maestro, un playbook por flujo, catálogos y `notas.md` escritas a mano). Dos agentes lo usan: **LectorUML** (ejecutor sin modelo) ejecuta su flujo UML en *Chatbots*, y **CamilaIA** (agente con modelo) conversa libremente apoyándose en la narrativa `bases de conocimiento/CamilaIA.md`. Todo se genera con `dotnet run --project src/ATLAS.Web -- --bot-generar`. Los agentes se gestionan en la sección **Agentes**. Guía completa, seguridad y modos de webhook: [docs/camila-ia.md](docs/camila-ia.md).

## Ahorro de tokens

Todo es configurable en `configuration/appsettings.json` (secciones `Orchestration` y `Anthropic`), sin perder la delegación:

- **Sin consolidación redundante:** si se delega una sola tarea y sale bien, su respuesta se entrega tal cual (1 llamada menos y sin reescribirla). Con varias tareas se consolida igualmente (`SkipConsolidationForSingleTask`).
- **Prompt caching** de Anthropic en los prompts de sistema (`PromptCaching`); los tokens de caché se cuentan en el contador.
- **Historial acotado:** solo los últimos 8 turnos (`MaxHistoryMessages`) y los turnos antiguos se acortan a 700 caracteres (`HistoryMessageMaxChars`).
- **Prompts compactos** (Orquestador, protocolo y agentes) y agentes instruidos para responder de forma directa y concisa.
- **Límites de salida:** planificación 1024 tokens (`PlanningMaxTokens`), resultados previos recortados al pasarlos a otro agente o al consolidar (`MaxDependencyResultChars`, `MaxResultCharsForConsolidation`).
- **Modelo por agente:** en el front matter de un agente puedes poner `model:` (por ejemplo, un modelo más barato solo para el Orquestador).
- El encabezado muestra los tokens totales y por agente (clic para el detalle).

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

**Sondeos de seguridad reproducibles:** con la app en marcha, `python tests/security-probes/run_probes.py` ejecuta 34 comprobaciones (origen, CSRF, WebSocket, cabeceras, archivos, subidas y exportaciones) sin llamar a ningún modelo.

```bash
dotnet test
```

Incluyen pruebas de arquitectura (matriz de referencias entre proyectos), de contrato (instantáneas de los DTO públicos en `tests/ATLAS.Tests/Contracts/`; tras un cambio aprobado se regeneran con `UPDATE_CONTRACTS=1`), de registros, de salud y de documentación.

Cubren: carga/validación de Markdown, registro de agentes, validación de decisiones y dependencias, límites de concurrencia, timeouts y cancelación, cambios de estado, aislamiento de errores, consolidación, validación y extracción de PDF, y `AnthropicService` (reintentos, mapeo de errores, la key nunca en el body ni en errores) con un `HttpMessageHandler` simulado. Las **pruebas de integración con Anthropic** se ejecutan solo si `ANTHROPIC_API_KEY` está definida; si no, aparecen como omitidas.

## Seguridad

- **Secretos:** las claves viven solo en el backend (`.env`, ignorado por git) y nunca llegan al navegador ni a los registros.
- **Peticiones de otros sitios (CSRF y secuestro de WebSocket):** toda petición con `Origin` a `/api`, `/hubs` o `/_blazor` debe venir de un origen permitido (`Office:AllowedOrigins`, por defecto localhost) o del propio sitio; las que cambian algo en `/api` exigen además la cabecera `X-Office-Client` (la interfaz la envía sola). Si llamas a la API con scripts, agrégala.
- **Cabeceras:** `Content-Security-Policy` (solo recursos propios, sin `unsafe-eval`), `X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy` y `X-Frame-Options`.
- **Respuestas de los modelos:** Markdown sin HTML crudo, **sin imágenes** (podrían filtrar datos a otro servidor) y con enlaces solo http, https, mailto o internos.
- **Inyección de instrucciones desde PDF:** el texto del documento va entre etiquetas que se neutralizan si el PDF intenta cerrarlas, y los agentes reciben la regla de tratarlo como datos, no como órdenes.
- **Límites:** 30 peticiones por minuto al chat (`Office:ChatRequestsPerMinute`), máximo de conversaciones y casos pendientes en memoria, subida de PDF validada por extensión, firma y tamaño, `AllowedHosts` limitado a localhost en ambos hosts.
- **Errores:** sin trazas; los mensajes de los proveedores se limpian (sin identificadores de organización ni enlaces).
- **Dependencias:** sin paquetes con vulnerabilidades conocidas (`dotnet list package --vulnerable --include-transitive`).
- **Navegador (spec 007):** las credenciales de cada conexión se guardan en `browser-data/credentials/<id>.md` en **texto plano, solo para entorno de pruebas** (carpeta en `.gitignore`; no vuelven por la API, ni van a registros ni a modelos); el navegador solo navega; el navegador solo navega a los dominios permitidos de cada sitio; ninguna acción irreversible se ejecuta sin tu aprobación; lo que lee la página y se envía a un modelo va solo a Claude (`Sensitive`).
- **Alcance:** pensada para un único usuario local y sin autenticación; no la expongas en una red compartida sin añadir inicio de sesión (las tareas, conversaciones y la actividad son visibles para quien alcance la API).

## Limitaciones conocidas

- Estado solo en memoria: al reiniciar se pierden conversaciones, tareas y actividad.
- Sin OCR: los PDF escaneados se detectan pero no se leen.
- Los planes gratuitos tienen límites de uso y pueden cambiar; la app escala a Claude cuando se agotan.
- Sin streaming token a token: el chat recibe la respuesta consolidada al terminar.
- Un único usuario/sesión local; sin autenticación (pensado para `localhost`).
