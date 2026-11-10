# Informe de convergencia 004 · Proveedores de modelos

**Maestro:** `.specify/maestros/08_CONVERGE.md` · **Fecha:** 2026-11-10 · **Constitución:** v1.1.0

## Estado de convergencia: COMPLETO (con observaciones registradas)

## Resumen ejecutivo
Todos los criterios tienen implementación verificada por pruebas automatizadas, salvo CA-009 (Gemini real), validado en sesión con claves reales. Quedan decisiones de privacidad por proveedor y de selector sin tomar; no impiden el uso previsto con datos ficticios.

## Matriz requisito/criterio → implementación → prueba → evidencia → estado
| Criterio | Implementación | Prueba | Evidencia | Estado |
|---|---|---|---|---|
| CA-001 | `RoutingLlmService`, `FreeApiService` | `Free_first_answers_with_the_first_provider...` | `dotnet test` | Verificado |
| CA-002 | `FreeApiService.Cool` | `A_rate_limited_provider_is_paused...` | `dotnet test` | Verificado |
| CA-003 | `RoutingLlmService` | `When_every_free_provider_fails_Claude_takes_over`, `Free_only_mode...` | `dotnet test` | Verificado |
| CA-004 | `ExtractError`, `TrySendAsync` | `An_error_wrapped_in_a_json_array...` | `dotnet test` | Verificado |
| CA-005 | `RoutingLlmService` | `Requests_with_documents_never_reach_free_APIs` | `dotnet test` | Verificado |
| CA-006 | `ValidateModelsAsync` | `Startup_check_accepts_model_ids_listed_with_the_models_prefix...` | `dotnet test` | Verificado |
| CA-007 | `TestProviderAsync`, `LlmController` | `The_provider_test_reports_success_or_the_real_error...` | `dotnet test`; llamada real | Verificado |
| CA-008 | `TokenUsageTracker` | `Usage_is_split_per_provider` | `dotnet test` | Verificado |
| CA-009 | configuración de Gemini | — | prueba real: 1 llamada simple y 5 llamadas del flujo (Word y PDF) con solo Gemini | Verificado en sesión (no reproducible por CI) |
| CA-010 | `AnthropicService` | `Models_that_deprecate_temperature_are_retried_without_it...` | `dotnet test` | Verificado |

## Estado de tareas
Completadas: T-001..T-013. Pendientes: T-014, T-015 (decisiones).

## Desviaciones del plan
No previstas: error de Gemini en forma de arreglo JSON (corregido, T-006); modelo `gemini-3.8-flash` con 503 y el 2.5 retirado (cambio a `gemini-flash-lite-latest`); el DNS corporativo impidió resolver el host de Gemini hasta que la red lo permitió (externo al código).

## Pruebas
220 aprobadas, 0 fallidas, 2 omitidas (integración con Anthropic sin clave en CI).

## Seguridad y riesgos residuales
AN-001 (privacidad en capa gratuita de Gemini); claves solo en `.env` (no revisado con barrido de secretos).

## Deuda técnica
Preexistente: sin prueba de integración opcional por proveedor. Introducida: ninguna conocida.

## Pendientes atómicos
- **PEND-001** — Decidir la bandera de privacidad por proveedor (T-014). Impacto: datos reales. Artefacto: spec 004.
- **PEND-002** — Decidir el selector de proveedor en la interfaz (T-015). Artefacto: spec 004.

## Recomendación de siguiente paso
Cerrar este ciclo; resolver PEND-001 antes de cualquier uso con datos reales.
