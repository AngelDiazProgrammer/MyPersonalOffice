# Tareas 005 · Cumplimiento constitucional

**Maestro:** `.specify/maestros/05_TASKS.md` · **Fecha:** 2026-11-10
**Entradas:** constitución v1.1.0; `spec.md` (aprobada); `clarifications.md`; `plan.md` (**aprobado 2026-11-10**, incluye Q-004 servidor, Q-005 solo listo/no listo, Q-006 `docs/`).

Leyenda: `P` preparación · `I` implementación · `T` pruebas · `D` documentación. Todas las tareas están pendientes.

## Hito 1 · Separación de capas (RF-008, RNF-002)
- [x] **T-001 · Crear** el contrato `IMarkdownRenderer` (I). Objetivo: interfaz `ToHtml(string?) : string` en Application. Archivos: `src/ATLAS.Application/Interfaces/` (archivo nuevo). Dep.: —. Traza: RF-008, CA-009. Validación: compila.
- [x] **T-002 · Convertir** `SafeMarkdown` en implementación del contrato (I). Objetivo: `SafeMarkdownRenderer : IMarkdownRenderer` con las mismas reglas de la spec 003. Archivos: `Infrastructure/Documents/SafeMarkdown.cs`. Dep.: T-001. Traza: RF-008, CA-009. Validación: `SafeMarkdownTests` ajustadas a la instancia (T-003).
- [x] **T-003 · Ajustar** las pruebas del saneador al contrato (T). Archivos: `tests/ATLAS.Tests/SecurityTests.cs` (`SafeMarkdownTests`). Dep.: T-002. Traza: CA-009. Validación: `dotnet test`.
- [x] **T-004 · Crear** las extensiones de composición de `Api`: `AddOfficeRendering` y `AddOfficeConfiguration` (I). Objetivo: `Web` y los hosts no usan Infrastructure. Archivos: `Api/Extensions/OfficeApiExtensions.cs` (o archivo nuevo en `Extensions/`), `Api/Program.cs`. Dep.: T-002. Traza: RF-008. Validación: compila; los hosts arrancan.
- [x] **T-005 · Migrar** `Web` a los contratos (I). Objetivo: `MarkdownView` inyecta `IMarkdownRenderer`; `Program.cs` usa las extensiones de `Api`; se quita `Markdig` de `Web.csproj` y todo `using ...Infrastructure`. Archivos: `Web/Components/Shared/MarkdownView.razor`, `Web/Program.cs`, `Web/ATLAS.Web.csproj`. Dep.: T-004. Traza: RF-008, CA-008. Validación: compila; la interfaz muestra Markdown (inspección).
- [x] **T-006 · Aislar** la visibilidad de Infrastructure (I). Objetivo: la referencia `Api → Infrastructure` con `PrivateAssets="compile"`. Archivos: `Api/ATLAS.Api.csproj`. Dep.: T-005. Traza: RNF-002. Validación: `Web` compila sin ver Infrastructure y la app arranca (si falla, se detiene y se pide decisión: riesgo R-1 del plan).
- [x] **T-007 · Probar** la arquitectura (T). Objetivo: matriz de referencias entre proyectos y ausencia de `ATLAS.Infrastructure` en el código y `.razor` de `Web`. Archivos: `tests/ATLAS.Tests/ArchitectureTests.cs` (nuevo). Dep.: T-005, T-006. Traza: CA-008, RNF-002. Validación: `dotnet test`; la prueba falla si se introduce la referencia (comprobación negativa documentada).

## Hito 2 · Registros (RF-001, RF-002, RF-007, RNF-001)
- [x] **T-008 · Configurar** los registros JSON (I). Objetivo: extensión `AddOfficeLogging` con consola JSON, ámbitos y hora UTC, usada por ambos hosts. Archivos: `Api/Extensions/`, `Api/Program.cs`, `Web/Program.cs`. Dep.: T-004. Traza: RF-007, CA-007. Validación: salida de arranque en JSON (inspección) y T-011.
- [x] **T-009 · Abrir** el ámbito de correlación (I). Objetivo: `CorrelationId` (GUID corto generado en el servidor) y `ConversationId` en `HandleMessageAsync`; `TaskId` y `AgentId` en `RunOneAsync`. Archivos: `Application/Orchestration/OrchestrationService.cs`, `PlanExecutor.cs`. Dep.: —. Traza: RF-001, CA-001. Validación: T-011.
- [x] **T-010 · Registrar** la duración de cada llamada a un proveedor (I). Objetivo: línea por llamada con `Purpose`, proveedor, modelo, ms, tokens y resultado, sin contenido. Archivos: `Infrastructure/Llm/RoutingLlmService.cs`. Dep.: —. Traza: RF-002, RNF-001, CA-002. Validación: T-011.
- [x] **T-011 · Probar** correlación, duración, ausencia de contenido y formato JSON (T). Archivos: `tests/ATLAS.Tests/LoggingTests.cs` (nuevo). Dep.: T-008, T-009, T-010. Traza: CA-001, CA-002, CA-007, RNF-001. Validación: `dotnet test` (registrador en memoria; el texto del mensaje de prueba no aparece en ninguna línea).

## Hito 3 · Salud (RF-004)
- [x] **T-012 · Crear** `OfficeReadinessCheck` y `/health/ready` (I). Objetivo: 200/503 con `{status, checks:[{name,status}]}`; agentes válidos, base con situaciones (si `Knowledge:Enabled`) y ≥1 proveedor configurado; sin rutas ni claves; `/health` intacto. Archivos: `Api/Health/OfficeReadinessCheck.cs` (nuevo), `Api/Extensions/OfficeApiExtensions.cs`. Dep.: T-004. Traza: RF-004, CA-004. Validación: T-013.
- [x] **T-013 · Probar** la salud (T). Archivos: `tests/ATLAS.Tests/HealthTests.cs` (nuevo). Dep.: T-012. Traza: CA-004. Validación: `dotnet test` (estado listo y no listo; sin datos sensibles en la respuesta).

## Hito 4 · Contratos (RF-003)
- [x] **T-014 · Fijar** los contratos públicos con instantáneas (T). Objetivo: serializar instancias canónicas de los 9 DTO del plan y compararlas con archivos de instantánea; falla si desaparece o se renombra un campo. Archivos: `tests/ATLAS.Tests/ContractTests.cs`, `tests/ATLAS.Tests/Contracts/*.json` (nuevos). Dep.: —. Traza: RF-003, CA-003. Validación: `dotnet test` y una comprobación negativa (renombrar un campo en un DTO provoca el fallo).

## Hito 5 · Documentación (RF-005, RF-006)
- [x] **T-015 · Redactar** la lista de preparación para producción (D). Archivos: `docs/preparacion-produccion.md`. Dep.: T-012. Traza: RF-005, CA-005. Validación: cada punto es comprobable (comando, endpoint o archivo).
- [x] **T-016 · Redactar** el índice de decisiones (D). Archivos: `docs/decisiones.md`. Dep.: —. Traza: RF-006, CA-006. Validación: enlaza las decisiones D-n de las specs 001–005.
- [x] **T-017 · Probar** que los documentos existen y sus enlaces resuelven (T). Archivos: `tests/ATLAS.Tests/DocsTests.cs` (nuevo). Dep.: T-015, T-016. Traza: CA-005, CA-006. Validación: `dotnet test`.
- [x] **T-018 · Actualizar** el README (D). Archivos: `README.md` (registros JSON, `/health/ready`, `docs/`, capas). Dep.: T-008, T-012, T-015. Traza: RF-005, RF-007.

## Hito 6 · Validación final
- [x] **T-019 · Validar** el conjunto (T). Objetivo: `dotnet build`, `dotnet test` completos y `tests/security-probes/run_probes.py` (34/34) sin regresiones; arranque de la app con registros JSON y `/health/ready` en 200. Dep.: todas. Traza: todos. Validación: salidas reales registradas en `convergence.md`.

## Control de cobertura
| Requisito | Tareas |
|---|---|
| RF-001 | T-009, T-011 |
| RF-002 | T-010, T-011 |
| RF-003 | T-014 |
| RF-004 | T-012, T-013 |
| RF-005 | T-015, T-017 |
| RF-006 | T-016, T-017 |
| RF-007 | T-008, T-011 |
| RF-008 | T-001..T-007 |
| RNF-001 | T-010, T-011 |
| RNF-002 | T-006, T-007 |
| CA-001..CA-009 | cubiertos por las tareas anteriores (ver Traza) |

Dependencias críticas: T-006 puede requerir una decisión nueva si `PrivateAssets` no basta (R-1).
