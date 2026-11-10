# Análisis de consistencia 003 · Seguridad web

**Maestro:** `.specify/maestros/06_ANALYZE.md` · **Fecha:** 2026-11-10 · **Constitución:** v1.1.0

## Veredicto: APROBADO CON OBSERVACIONES

## Resumen ejecutivo
1. Los controles cubren los hallazgos demostrados y las categorías OWASP relevantes (A01, A03, A04, A05, A06, A09).
2. RF-006 (límite de tasa) y RF-010 (hosts) ya tienen prueba automatizada (T-013); el barrido de sondeos es reproducible (T-014).
3. Sin autenticación: aceptado y declarado solo para uso local de un usuario (Art. 21).
4. La política de contenido conserva `unsafe-inline` por requisito de la interfaz.
5. Artefacto retroactivo.

## Hallazgos
- **AN-001 · CERRADO (2026-11-10)** — RF-006/CA-011. Resuelto con `RateLimitTests` (T-013).
- **AN-002 · MEDIO** — RN-003 vs Art. 21 y 38. Sin autenticación ni aislamiento. Consecuencia: no se puede desplegar en red compartida. Corrección: nueva especificación (Q-001). Responsable: especificación.
- **AN-003 · BAJO** — D-3. `unsafe-inline` reduce la protección de la política. Aceptado por requisito técnico; revisar al cambiar de tecnología de interfaz.
- **AN-004 · CERRADO (2026-11-10)** — T-014. El barrido se repitió completo: `tests/security-probes/run_probes.py`, 34/34.
- **AN-005 · BAJO** — Art. 28/29. Proceso retroactivo.

## Matriz de trazabilidad
| Requisito | Criterio | Diseño | Tarea | Validación |
|---|---|---|---|---|
| RF-001 | CA-001 | D-1, D-2 | T-002, T-003 | pruebas de guardia; sondeo WebSocket |
| RF-002 | CA-002 | D-1 | T-002, T-004 | `Requests_that_change_state...` |
| RF-003 | CA-003 | D-3 | T-001 | `Every_response_carries...` |
| RF-004 | CA-004 | D-4 | T-005 | `SafeMarkdownTests` |
| RF-005 | CA-005 | — | T-006 | `PromptInjectionTests` |
| RF-006 | CA-011 | D-5 | T-007, T-013 | `RateLimitTests` |
| RF-007 | CA-007 | D-6 | T-008 | `MemoryLimitTests` |
| RF-008 | CA-008 | D-7 | T-009 | `ErrorTextTests` |
| RF-009 | CA-009 | — | T-011 | `dotnet list package --vulnerable` |
| RF-010 | — | — | T-012, T-013 | `AllowedHostsTests` |
| RF-011 | CA-010 | — | T-010 | `LocalChoiceHandlingTests` |

## Cobertura faltante
Ninguna; solo CA-006 (consola del navegador) sigue manual por el aplazamiento de pruebas de interfaz.

## Acciones correctivas recomendadas
Autenticación: punto nulo por decisión del responsable (no desarrollar aún).

## Condiciones para reanalizar
Al completar T-013/T-014 o al cambiar el modelo de despliegue.
