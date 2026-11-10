# Análisis de consistencia 004 · Proveedores de modelos

**Maestro:** `.specify/maestros/06_ANALYZE.md` · **Fecha:** 2026-11-10 · **Constitución:** v1.1.0

## Veredicto: APROBADO CON OBSERVACIONES

## Resumen ejecutivo
1. Los 12 RF y 10 CA tienen tarea y validación; el diseño respeta la independencia de proveedor (Art. 9, 15).
2. La privacidad por proveedor no es configurable: Gemini gratuito comparte la política con los demás (Q-001).
3. Dos decisiones quedan abiertas (T-014, T-015).
4. La validación de Gemini de extremo a extremo es evidencia de sesión, no una prueba reproducible.
5. Artefacto retroactivo.

## Hallazgos
- **AN-001 · MEDIO (decisión del responsable 2026-11-10: se mantiene para pruebas)** — RF-006 / Art. 20. La capa gratuita de Gemini puede usar el contenido para mejorar productos de Google; el sistema solo distingue «sensible» (solo Claude) o no. Consecuencia: contenido no marcado como sensible podría llegar a Gemini. Corrección: decidir Q-001 (bandera por proveedor) antes de datos reales. Responsable: especificación.
- **AN-002 · BAJO** — CA-009 ↔ validación. Depende de claves y red reales; no hay prueba de integración opcional etiquetada. Corrección: opcional.
- **AN-003 · BAJO** — Riesgo operativo. Los nombres de modelos gratuitos cambian (se observó retirada y alta demanda); mitigado por la verificación de arranque y la prueba de proveedor.
- **AN-004 · BAJO** — Art. 28/29. Proceso retroactivo.

## Matriz de trazabilidad
| Requisito | Criterio | Diseño | Tarea | Validación |
|---|---|---|---|---|
| RF-001 | CA-001 | D-2 | T-002 | `Free_first_answers...`, modos |
| RF-002 | — | D-1 | T-001, T-005 | `One_generic_service...` |
| RF-003 | CA-009 | D-8 | T-005, T-012 | evidencia de sesión |
| RF-004 | CA-002, CA-003 | D-3 | T-003 | pruebas de pausa y escalado |
| RF-005 | CA-004 | D-5 | T-006 | `An_error_wrapped_in_a_json_array...` |
| RF-006 | CA-005 | D-4 | T-004 | `Requests_with_documents_never_reach_free_APIs` |
| RF-007 | — | D-2 | T-002 | `Per_request_preference_forces_Claude` |
| RF-008 | CA-008 | — | T-011 | `Usage_is_split_per_provider` |
| RF-009 | CA-006 | D-7 | T-007 | `Startup_check_...` |
| RF-010 | CA-007 | D-6 | T-008 | `The_provider_test_reports...` |
| RF-011 | — | D-7 | T-010 | `ErrorTextTests` |
| RF-012 | CA-010 | — | T-009 | `InfrastructureTests` |

## Cobertura faltante
Ninguna de requisitos; decisiones T-014 y T-015.

## Acciones correctivas recomendadas
Resolver Q-001 antes de datos reales.

## Condiciones para reanalizar
Al decidir Q-001/Q-002 o añadir un proveedor.
