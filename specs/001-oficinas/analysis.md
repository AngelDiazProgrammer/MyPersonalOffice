# Análisis de consistencia 001 · Oficinas

**Maestro:** `.specify/maestros/06_ANALYZE.md` · **Fecha:** 2026-11-10 · **Constitución:** v1.1.0

## Veredicto: APROBADO CON OBSERVACIONES

## Resumen ejecutivo
1. Spec, plan y tareas son coherentes y cubren todos los RF/RN.
2. El artefacto es retroactivo: no hubo aprobación previa (Art. 29); se deja constancia.
3. La elección global de oficina no soporta multiusuario (Art. 21 y 38).
4. Tres criterios de aceptación solo tienen validación manual.

## Hallazgos
- **AN-001 · MEDIO** — Spec §Alcance/Supuestos vs Art. 21 y 38. La elección de oficina es global al servidor. Consecuencia: en un despliegue compartido una persona cambiaría la oficina de otra. Corrección: mantener fuera de alcance hoy; abrir spec nueva cuando haya multiusuario (Q-001). Responsable: especificación.
- **AN-002 · MEDIO** — CA-002/CA-004/CA-007 ↔ tareas. Sin prueba automatizada (T-013 abierta). Consecuencia: regresiones de interfaz no detectables por CI. Corrección: ejecutar T-013. Responsable: tareas.
- **AN-003 · BAJO** — Art. 28/29. Proceso retroactivo. Corrección: ninguna; documentado.

## Matriz de trazabilidad
| Requisito | Criterio | Diseño | Tarea | Validación |
|---|---|---|---|---|
| RF-001 | CA-001 | D-1, selección | T-002, T-007, T-008 | WorkspaceTests; navegador |
| RF-002 | CA-002 | D-2 | T-001 | configuración; navegador |
| RF-003 | CA-002 | D-3 | T-004, T-009 | prueba de consulta; `debug()` |
| RF-004 | CA-003 | D-3 | T-005 | prueba de chat bloqueado |
| RF-005 | CA-004 | limpieza de conversación | T-008 | navegador |
| RF-006/007 | CA-006 | D-2 | T-001, T-002 | `WorkspaceTests` |
| RF-008 | CA-007 | D-4 | T-003, T-010 | navegador |
| RN-002 | CA-005 | D-3 | T-005 | prueba del planificador |

## Cobertura faltante
Pruebas automatizadas de interfaz (T-013).

## Acciones correctivas recomendadas
Ejecutar T-013; registrar Q-001 como spec futura.

## Condiciones para reanalizar
Cuando se implemente T-013 o se incorpore multiusuario.
