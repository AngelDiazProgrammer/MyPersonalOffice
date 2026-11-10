# Informe de convergencia 005 · Cumplimiento constitucional

**Maestro:** `.specify/maestros/08_CONVERGE.md` · **Fecha:** 2026-11-10 · **Constitución:** v1.1.0

## Estado de convergencia: COMPLETO

## Resumen ejecutivo
Las 19 tareas (T-001..T-019) están hechas y verificadas con pruebas automáticas y con una ejecución real de la aplicación: `Web` ya no ve `Infrastructure`, los registros son JSON correlacionados y con duración por llamada (sin contenido), existe `/health/ready`, los contratos públicos están fijados por instantáneas y la documentación de operación está creada y probada.

## Matriz criterio → implementación → prueba → evidencia → estado
| Criterio | Implementación | Prueba | Evidencia | Estado |
|---|---|---|---|---|
| CA-001 | `OrchestrationService` (alcance `CorrelationId`), `PlanExecutor` (`TaskId`, `AgentId`) | `LoggingTests.All_the_logs_of_one_request_share...` | `dotnet test` | Verificado |
| CA-002 | `RoutingLlmService.SendAsync` | `LlmRoutingTests` (2 pruebas de registro), `LoggingTests.The_logs_never_contain...` | `dotnet test` | Verificado |
| CA-003 | `ContractTests` + `Contracts/*.json` (9 DTO) | `Public_contract_keeps_every_field...` | `dotnet test`; comprobación negativa: renombrar `SizeBytes` hizo fallar 2 pruebas y se revirtió | Verificado |
| CA-004 | `Api/Health/ReadinessChecks.cs`, `/health/ready` | `HealthTests` | 503 con `not-ready` por componente; en ejecución real 200 `ready` | Verificado (ver desviación 1) |
| CA-005 | `docs/preparacion-produccion.md` | `DocsTests` | existe; enlaces resueltos | Verificado |
| CA-006 | `docs/decisiones.md` | `DocsTests.The_decisions_index_covers_every_specification` | enlaza los `plan.md` de las specs 001–005 | Verificado |
| CA-007 | `AddOfficeLogging` (`AddJsonConsole`) | `JsonLoggingConfigurationTests`; ejecución real | líneas de consola en JSON con `Timestamp`, `LogLevel`, `Message`, `Scopes` | Verificado |
| CA-008 | `PrivateAssets="compile"`, `IMarkdownRenderer`, `AddOfficeRendering` | `ArchitectureTests` | la referencia directa desde `Web` deja de compilar (comprobación negativa previa) | Verificado |
| CA-009 | `SafeMarkdownRenderer` tras `IMarkdownRenderer` | `SafeMarkdownTests` (misma batería de la spec 003) | `dotnet test` | Verificado |

## Estado de tareas
Completadas: T-001..T-019. Pendientes: ninguna.

## Desviaciones del plan
1. **Salud (D-8):** en lugar de una única `OfficeReadinessCheck` se implementaron tres comprobaciones (`AgentsReadyCheck`, `KnowledgeReadyCheck`, `ProvidersReadyCheck`) en `ReadinessChecks.cs`; el motivo del «no listo» se comunica por el nombre del componente fallido (decisión Q-005: sin mensajes técnicos).
2. **Contratos (D-9):** la comparación es por forma (rutas y tipos JSON), no byte a byte, para que añadir campos siga siendo compatible; regeneración con `UPDATE_CONTRACTS=1`.

## Pruebas
253 aprobadas, 0 fallidas, 2 omitidas (integración con Anthropic sin clave). Compilación: 0 advertencias, 0 errores. Sondeos `tests/security-probes/run_probes.py`: 34/34 contra la aplicación en marcha. Arranque real: `GET /health` 200, `GET /health/ready` 200 con los tres componentes `ready`, registros en JSON con alcances.

## Seguridad y riesgos residuales
`/health/ready` no expone rutas ni claves (prueba específica y sondeos). Los registros no contienen el texto de mensajes ni respuestas. Sin barrido de secretos del repositorio (no solicitado). Persisten los riesgos de la spec 003/004: sin autenticación y privacidad de la capa gratuita de Gemini.

## Deuda técnica
Preexistente: sin pruebas de extremo a extremo. Introducida: ninguna conocida. La spec 003/D-4 nombra `SafeMarkdown` (histórico); la nota está en `docs/decisiones.md`.

## Pendientes atómicos
- **PEND-001** — Resuelto: los maestros 02–08 de `.specify/maestros/` son idénticos a los de Descargas; `01_Constitution.md` no se copia a esa carpeta (la constitución vive en `.specify/memory/` con las enmiendas v1.1.0).
- **PEND-002** — Autenticación / varios usuarios, bandera de privacidad por proveedor y selector de proveedor siguen pospuestos por decisión de la persona responsable. Artefacto: specs 003/004.

## Recomendación de siguiente paso
Cerrar este ciclo y hacer el commit de la spec 005; tratar PEND-001 antes de la siguiente spec.
