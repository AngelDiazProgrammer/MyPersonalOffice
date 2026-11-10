# Informe de convergencia 003 · Seguridad web

**Maestro:** `.specify/maestros/08_CONVERGE.md` · **Fecha:** 2026-11-10 · **Constitución:** v1.1.0

## Estado de convergencia: PARCIAL

## Resumen ejecutivo
Los controles están implementados y todos los requisitos tienen pruebas automatizadas o sondeos reproducibles aprobados (34/34). Queda solo CA-006 (ausencia de violaciones de política de contenido en la consola del navegador) como verificación manual, a la espera de las pruebas de interfaz aplazadas.

## Matriz requisito/criterio → implementación → prueba → evidencia → estado
| Criterio | Implementación | Prueba | Evidencia | Estado |
|---|---|---|---|---|
| CA-001 | `RequestGuardMiddleware` | `A_foreign_origin_is_rejected...` | `dotnet test`; sondeo: WS ajeno 403 / propio 101 | Verificado |
| CA-002 | `RequestGuardMiddleware` | `Requests_that_change_state_need_the_client_header` | `dotnet test`; sondeo `reset` 403/200 | Verificado |
| CA-003 | `SecurityHeadersMiddleware` | `Every_response_carries_the_security_headers` | `dotnet test`; respuesta real | Verificado |
| CA-004 | `SafeMarkdown` | `SafeMarkdownTests` | `dotnet test` | Verificado |
| CA-005 | `PromptBuilder` | `PromptInjectionTests` | `dotnet test` | Verificado |
| CA-006 | `Web/Program.cs`, CSP | — | navegador sin errores | Verificado manualmente |
| CA-007 | stores | `MemoryLimitTests` | `dotnet test` | Verificado |
| CA-008 | `ErrorText` | `ErrorTextTests` | `dotnet test` | Verificado |
| CA-009 | csproj | `dotnet list package --vulnerable` | salida sin hallazgos | Verificado |
| CA-010 | `OrchestrationService` | `LocalChoiceHandlingTests` | `dotnet test` | Verificado |
| CA-011 | `AddOfficeRateLimiting` | `Chat_requests_beyond_the_limit_get_429...` | `dotnet test` | Verificado |
| RF-010 | `appsettings.json` (Api, Web) | `Both_hosts_restrict_AllowedHosts_to_local_names` | `dotnet test` | Verificado |

## Estado de tareas
Completadas: T-001..T-014.

## Desviaciones del plan
Ninguna de diseño. Operativa: el guion de sondeos completo se colgó en una segunda ejecución y se sustituyó por sondeos dirigidos.

## Pruebas
226 aprobadas, 0 fallidas, 2 omitidas (Anthropic). Sondeos: `tests/security-probes/run_probes.py` 34/34.

## Seguridad y riesgos residuales
Sin autenticación (AN-002, punto nulo); límite por IP compartido; `unsafe-inline` (AN-003); el barrido de secretos del repositorio no se realizó.

## Deuda técnica
Preexistente: sin autenticación ni pruebas E2E. Introducida: ninguna conocida.

## Pendientes atómicos
- ~~PEND-001~~ y ~~PEND-002~~ — CERRADOS 2026-11-10 (T-013, T-014).
- **PEND-003** — Autenticación/autorización: punto nulo por decisión del responsable (no desarrollar aún).

## Recomendación de siguiente paso
Cerrar con observación: CA-006 manual hasta adoptar pruebas de interfaz (aplazadas).
