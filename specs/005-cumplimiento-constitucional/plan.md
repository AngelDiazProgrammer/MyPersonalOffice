# Plan técnico 005 · Cumplimiento constitucional

**Maestro:** `.specify/maestros/04_PLAN.md` · **Estado:** **APROBADO (2026-11-10, responsable del proyecto)**, incluidas las propuestas Q-004 (correlación generada en servidor), Q-005 (salud solo listo/no listo) y Q-006 (`docs/`).
**Fecha:** 2026-11-10 · **Constitución:** v1.1.0

## Resumen y objetivo técnico
Cerrar las brechas de la auditoría con cambios mínimos y sin alterar el comportamiento visible: registros JSON correlacionados con duración, pruebas de contrato, salud de «listo», documentos de producción y decisiones, y separación estricta `Web` → `Application` (sin ver Infrastructure).

## Referencias
`specs/005-cumplimiento-constitucional/spec.md` (aprobada), `clarifications.md`; constitución Art. 7, 22, 24, 34, 36; spec 003 (reglas del saneador de Markdown).

## Estado actual relevante (evidencia)
- `Web` referencia `Api` y `Application` (`ATLAS.Web.csproj`); `Api` referencia `Application` e `Infrastructure`. Por la referencia transitiva, `Web` puede ver tipos de Infrastructure.
- Usos directos en `Web`: `MarkdownView.razor` (`SafeMarkdown`) y `Program.cs` (`AddOfficeConfiguration`). `Web.csproj` además declara `Markdig` (ya sin uso propio).
- 42 puntos de registro con `ILogger<T>`; sin alcance de correlación; la duración solo se mide en la prueba de proveedor (`FreeApiService.TestProviderAsync`).
- `/health` devuelve `{status: "ok"}` sin comprobar componentes (`OfficeApiExtensions.UseOfficeApi`).
- Contratos públicos = DTO de `Application/DTOs/Dtos.cs`; sin pruebas que fijen su forma.

## Decisiones de arquitectura
- **D-1 Separación de capas (Q-001).** Se define `IMarkdownRenderer` en Application y su implementación en Infrastructure (hoy `SafeMarkdown`). `MarkdownView` inyecta el contrato. *Alternativas descartadas:* dejar el saneador en `Web` (duplica una regla de seguridad de la spec 003 en la capa de presentación); mantener la dependencia (rechazada por el responsable).
- **D-2 Composición sin exponer Infrastructure.** Se añaden extensiones en el proyecto `Api` (raíz de composición): `AddOfficeRendering()` registra `IMarkdownRenderer` y `AddOfficeConfiguration(...)` envuelve la carga de `.env`/configuración. `Web` solo llama a extensiones de `Api`. `Web` registra el renderizador también cuando consume una API remota (`Office:HostApi=false`).
- **D-3 Aislar la visibilidad.** `Api.csproj`: la referencia a Infrastructure se marca con `PrivateAssets="compile"` para que `Web` no la herede como referencia de compilación (a verificar empíricamente, ver riesgos). Se elimina `Markdig` de `Web.csproj`.
- **D-4 Verificación automática (RNF-002).** Prueba de arquitectura: (a) matriz de referencias permitidas entre proyectos leída de los `.csproj` (Domain no referencia a nadie; Application solo Domain; Infrastructure solo Application y Domain; Api puede componer; Web no referencia Infrastructure); (b) búsqueda en el código y los `.razor` de `Web` de `ATLAS.Infrastructure`.
- **D-5 Correlación (Q-004, propuesto: servidor).** `OrchestrationService.HandleMessageAsync` abre un alcance de registro con `CorrelationId` (GUID corto generado en el servidor, nunca aceptado del cliente) y `ConversationId`; `PlanExecutor.RunOneAsync` añade `TaskId` y `AgentId`. El alcance fluye por `async`, incluso en tareas paralelas. *Alternativa descartada:* aceptar un id del cliente (riesgo de falsificación/contenido arbitrario).
- **D-6 Duración y detalle por llamada.** `RoutingLlmService.SendAsync` mide con `Stopwatch` y registra una línea de información por llamada: `Purpose`, proveedor, modelo, duración (ms), tokens de entrada/salida y resultado; en fallo, tipo de excepción y duración. Nunca mensajes ni documentos (RNF-001).
- **D-7 Registros JSON (Q-002).** `AddJsonConsole` con `IncludeScopes=true` y marca de tiempo UTC, configurado en una extensión de `Api` (`AddOfficeLogging`) usada por ambos hosts. Se retira el formato de consola simple. *Alternativa descartada:* Serilog (dependencia nueva innecesaria, Art. 8).
- **D-8 Salud (Q-005, propuesto: solo listo/no listo).** `/health` se conserva sin cambios (vivo). Nuevo `/health/ready` con una comprobación `OfficeReadinessCheck` en `Api` que usa contratos de Application: agentes cargados y válidos, base de conocimiento con al menos una situación si `Knowledge:Enabled`, y al menos un proveedor de modelos configurado. Responde 200/503 con `{status, checks:[{name,status}]}` sin rutas ni claves.
- **D-9 Contratos (RF-003).** Pruebas de contrato por instantánea: se serializan instancias canónicas de los DTO públicos con las opciones JSON de la API y se comparan con archivos `tests/ATLAS.Tests/Contracts/*.json`; fallan si **desaparece o se renombra** un campo (agregar campos es compatible).
- **D-10 Documentos (Q-006, propuesto: `docs/`).** `docs/preparacion-produccion.md` (lista comprobable, Art. 36) y `docs/decisiones.md` (índice de decisiones D-n de las specs 001–005, Art. 34).

## Diseño propuesto
Componentes nuevos: `IMarkdownRenderer` (Application), `SafeMarkdownRenderer` (Infrastructure, sobre la lógica actual), extensiones de composición y de registro (Api), `OfficeReadinessCheck` (Api). Cambios de comportamiento: ninguno visible para la persona (RN-002); `/health/ready` y el formato de los registros son operativos.

## Flujos
Solicitud de chat → alcance de correlación → tareas (alcance anidado) → llamadas a proveedor (línea con duración) → fin. Arranque → registros JSON; `/health/ready` evalúa los tres componentes. Error: componente no listo → 503 con el nombre del componente.

## Contratos
Nuevo: `IMarkdownRenderer.ToHtml(string?) : string`. HTTP: `GET /health/ready` (nuevo); `/health` sin cambio. DTO públicos sin cambios. Configuración: sin claves nuevas (JSON siempre activo).

## Datos y persistencia
Sin persistencia nueva. Los registros solo salen por consola.

## Seguridad y privacidad
Sin contenido de mensajes ni documentos ni secretos en los registros; correlación generada en servidor; el estado de salud no expone rutas ni claves; el renderizador conserva las reglas de la spec 003.

## Integraciones y proveedores
Sin cambios en llamadas a proveedores; solo se mide y registra su duración.

## Cambios por proyecto/archivo (verificados)
- Application: `Interfaces/` (nuevo `IMarkdownRenderer`), `Orchestration/OrchestrationService.cs` (alcance), `Orchestration/PlanExecutor.cs` (alcance de tarea).
- Infrastructure: `Documents/SafeMarkdown.cs` → implementación del contrato; `Llm/RoutingLlmService.cs` (duración).
- Api: `Extensions/OfficeApiExtensions.cs` (`AddOfficeRendering`, `AddOfficeConfiguration`, `AddOfficeLogging`, salud), nuevo `Health/OfficeReadinessCheck.cs`, `Program.cs`, `ATLAS.Api.csproj` (`PrivateAssets`).
- Web: `Components/Shared/MarkdownView.razor`, `Program.cs`, `ATLAS.Web.csproj` (sin `Markdig`).
- Tests: nueva prueba de arquitectura, pruebas de contrato (`Contracts/*.json`), prueba de JSON de registros, prueba de `/health/ready`, ajuste de `SafeMarkdownTests`.
- Documentación: `docs/preparacion-produccion.md`, `docs/decisiones.md`, `README.md`.

## Pruebas
- Arquitectura (CA-008): matriz de referencias y búsqueda en `Web`.
- Contratos (CA-003): instantáneas de `ChatResponseDto`, `AgentDto`, `AgentTaskDto`, `WorkspacesDto`, `LlmStatusDto`, `OfficeStateDto`, `GeneratedDocumentDto`, `KnowledgeChoiceDto`, `TokenUsageDto`.
- Registros (CA-001, CA-002, CA-007): un registrador en memoria con alcances comprueba que las líneas de una solicitud comparten `CorrelationId`, que la llamada a proveedor trae duración y que no aparece el texto del mensaje; el formato JSON se valida con un `StringWriter`.
- Salud (CA-004): servidor real en puerto libre con agentes/proveedores simulados (listo y no listo).
- Markdown (CA-009): las pruebas existentes de `SafeMarkdownTests` contra el contrato.
- Documentos (CA-005, CA-006): comprobación de existencia y enlaces del índice.
- Regresión: `dotnet test` completo y `tests/security-probes/run_probes.py` (34 comprobaciones).

## Despliegue/configuración/rollback
Sin configuración nueva. Rollback: revertir el cambio; `/health` original se mantiene. Los consumidores que lean los registros deberán esperar JSON.

## Riesgos, dependencias y decisiones pendientes
- **R-1** `PrivateAssets="compile"` podría no ocultar Infrastructure a `Web` en todos los casos; la prueba D-4 es la red de seguridad y, si no basta, se evaluará una capa de composición separada (nueva decisión a aprobar).
- **R-2** El JSON dificulta la lectura en desarrollo; se acepta por decisión del responsable.
- **R-3** Alcances de registro en tareas paralelas: se verificará con la prueba de correlación.
- **Decisiones aprobadas:** Q-004 (correlación en servidor), Q-005 (salud solo listo/no listo), Q-006 (`docs/`).
- **Dependencia:** ninguna externa; sin paquetes nuevos.
