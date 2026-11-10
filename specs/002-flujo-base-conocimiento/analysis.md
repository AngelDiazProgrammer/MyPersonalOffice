# Análisis de consistencia 002 · Flujo de la base de conocimiento

**Maestro:** `.specify/maestros/06_ANALYZE.md` · **Fecha:** 2026-11-10 · **Constitución:** v1.1.0

## Veredicto: APROBADO CON OBSERVACIONES

## Resumen ejecutivo
1. Spec, plan y tareas son coherentes; los 13 RF y 10 CA tienen tarea y validación automatizada.
2. Tras la enmienda del Art. 16, la consulta a la base como primer filtro es consistente con la constitución.
3. Existe una decisión de privacidad abierta (Art. 20): el texto de PDF puede ir a proveedores gratuitos por defecto.
4. El respaldo del Lector PDF (RF-014) quedó decidido (variante A) e implementado.
5. Artefacto retroactivo (Art. 29).

## Hallazgos
- **AN-001 · MEDIO (decisión del responsable 2026-11-10: se mantiene para pruebas)** — Spec Q-001 / RF-010 vs Art. 20. `AllowFreeApisWithDocuments` es `true` por defecto; el responsable lo autorizó para pruebas con datos ficticios. Con Gemini en capa gratuita (puede usar contenido para mejorar productos de Google) el riesgo aumenta. Consecuencia: con datos reales habría envío no autorizado a terceros. Corrección: registrar la autorización como decisión y exigir `false` antes de usar datos reales (spec). Responsable: especificación.
- **AN-002 · CERRADO (2026-11-10)** — RF-004 / Q-002. Si fallaba el Lector PDF, las tareas dependientes se cancelaban. Resolución: RF-014 (reintento único con Claude, variante A), tarea T-024, pruebas T-025. Verificado en `convergence.md`.
- **AN-003 · MEDIO** — RF-008 / T-021 / T-023. La interfaz de opciones y descargas solo se verificó manualmente. Corrección: T-023. Responsable: tareas.
- **AN-004 · BAJO** — RNF-002. Las tareas del flujo usan el tiempo máximo global (300 s), no uno específico. Aceptable.
- **AN-005 · BAJO** — Art. 28/29. Proceso retroactivo.

## Matriz de trazabilidad
| Requisito | Criterio | Diseño | Tarea | Validación |
|---|---|---|---|---|
| RF-001, RF-002 | CA-001 | D-1 | T-001, T-002, T-006 | `Each_test_situation_resolves...`, `A_matching_question_skips...` |
| RF-003, RN-004 | CA-001 | D-2, D-8 | T-005 | configuración |
| RF-004 | CA-002 | D-1 | T-007 | `A_case_inside_an_attached_pdf...` |
| RF-005 | — | D-7 | T-003 | `The_newest_file_is_the_active_format` |
| RF-006, RN-002 | CA-005 | D-5 | T-009 | `Invented_tickets...` |
| RF-007, RN-003 | CA-001 | D-3, D-4 | T-004, T-012 | `The_case_is_investigated...` |
| RF-008 | CA-003, CA-004, CA-006 | D-6 | T-013, T-014 | pruebas de elección |
| RF-009 | CA-003, CA-008 | D-7 | T-011, T-017..T-019 | pruebas APA y de entrega |
| RF-010 | CA-009 | D-1 | T-007, T-008 | `With_the_flag_off_tasks...`, `Confidential_entries...` |
| RF-011 | — | — | T-020 | sondeo HTTP; navegador |
| RF-012 | CA-010 | D-5 | T-010 | `A_correction_cut_by_the_token_limit...` |
| RF-013 | CA-007 | — | T-016, T-024 | `With_the_flag_off_the_full_flow...` |
| RF-014 | CA-011, CA-012 | D-9 | T-024, T-025 | `ReaderFallbackTests` |

## Cobertura faltante
Prueba automatizada de la interfaz (T-023).

## Acciones correctivas recomendadas
Decidir Q-001 y Q-002; ejecutar T-023.

## Condiciones para reanalizar
Al decidir Q-001/Q-002 o al pasar a datos reales.
