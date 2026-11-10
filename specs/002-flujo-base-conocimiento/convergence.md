# Informe de convergencia 002 · Flujo de la base de conocimiento

**Maestro:** `.specify/maestros/08_CONVERGE.md` · **Fecha:** 2026-11-10 · **Constitución:** v1.1.0

## Estado de convergencia: COMPLETO (con observaciones registradas)

## Resumen ejecutivo
Los 12 criterios de aceptación tienen implementación y una prueba automatizada que se ejecuta y aprueba. Las decisiones Q-001 (se mantiene para pruebas) y Q-002 (respaldo del Lector PDF, variante A) están resueltas; queda aplazada la prueba de interfaz. Nada de lo pendiente impide el uso previsto con datos ficticios.

## Matriz requisito/criterio → implementación → prueba → evidencia → estado
| Criterio | Implementación | Prueba | Evidencia | Estado |
|---|---|---|---|---|
| CA-001 | `KnowledgeWorkflow`, `.Choice` | `The_case_is_investigated_and_the_options_are_offered...` | `dotnet test` (223 verdes) | Verificado |
| CA-002 | `KnowledgeWorkflow.TryPlanAsync`, `PlanExecutor` | `A_case_inside_an_attached_pdf_is_matched...` | `dotnet test` | Verificado |
| CA-003 | `ResolveChoiceAsync`, `BuildDeliveryPlan`, `OrchestrationService` | `Choosing_by_letter_delivers_only_that_solution` | `dotnet test` | Verificado |
| CA-004 | `BuildDeliveryPlan` (`ExtraAllowed`) | `A_custom_solution_is_reviewed...` | `dotnet test` | Verificado |
| CA-005 | `FindUnverified`, tarea de reparación | `Invented_tickets_phones_and_deadlines...` | `dotnet test` | Verificado |
| CA-006 | `OrchestrationService` | `Choosing_an_option_without_a_pending_case...` | `dotnet test` | Verificado |
| CA-007 | `KnowledgeOptions.OfferSolutionChoice` | `With_the_flag_off_the_full_flow_runs_in_one_go` | `dotnet test` | Verificado |
| CA-008 | `MarkdownWordGenerator` | `Apa_word_is_valid...`, `Apa_word_has_title_page...` | `dotnet test` | Verificado |
| CA-009 | `PlanExecutor`, `KnowledgeWorkflow` | `With_the_flag_off_tasks_carrying_the_pdf_text...` | `dotnet test` | Verificado |
| CA-011 | `PlanExecutor` (reintento), `AgentRunner.forceClaude` | `When_the_free_apis_fail_the_reader_retries_once_with_Claude_and_the_flow_continues` | `dotnet test` | Verificado |
| CA-012 | `PlanExecutor` | `When_Claude_also_fails_the_flow_stops_with_the_usual_message`, `With_the_flag_off_there_is_no_retry` | `dotnet test` | Verificado |
| CA-010 | `ExtractCorrectedText` | `A_correction_cut_by_the_token_limit...` | `dotnet test` | Verificado |
| RF-011 (exportar) | `DocumentsController`, `ChatMessageView` | sondeo HTTP | respuesta 200 con archivos válidos | Verificado parcial (UI manual) |

## Estado de tareas
Completadas: T-001..T-022, T-024..T-026. Pendiente: T-023 (aplazada por decisión del responsable).

## Desviaciones del plan
Previstas: el Lector PDF se incorporó después del diseño inicial (corrección pedida por la persona). No previstas: Gemini devolvía errores en arreglo JSON (corregido, spec 004); el auditor truncado reemplazaba el texto completo (corregido, T-010).

## Pruebas
223 aprobadas, 0 fallidas, 2 omitidas (integración con Anthropic sin clave en CI). Ejecuciones reales de los 5 casos con modelos gratuitos y Claude: códigos correctos y sin datos inventados (evidencia de sesión, no reproducible por máquina).

## Seguridad y riesgos residuales
AN-001 (privacidad con proveedores gratuitos; mantenida para pruebas por decisión del responsable). El reintento envía el documento solo a Claude. La spec 003 cubre la inyección desde PDF y la salida de Markdown.

## Deuda técnica
Preexistente: sin pruebas E2E. Introducida: QuestPDF no aplica sangría francesa (limitación documentada).

## Pendientes atómicos
- **PEND-001** — Decidir y documentar la política de privacidad por defecto (Q-001). Impacto: uso con datos reales. Artefacto: spec 002.
- ~~**PEND-002** — Respaldo del Lector PDF (Q-002)~~ — CERRADO 2026-11-10: variante A implementada (RF-014, T-024..T-026).
- **PEND-003** — Automatizar la prueba de interfaz (T-023). Artefacto: tasks 002.

## Recomendación de siguiente paso
Cerrar este ciclo y resolver PEND-001 antes de cualquier uso con datos reales.
