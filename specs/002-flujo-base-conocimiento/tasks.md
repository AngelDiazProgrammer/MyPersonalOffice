# Tareas 002 · Flujo de la base de conocimiento

**Maestro:** `.specify/maestros/05_TASKS.md` · **Nota:** backlog **retroactivo**; estados respaldados por evidencia del repositorio (`dotnet test`: 220 aprobadas, 2 omitidas de Anthropic en vivo).

Leyenda: `P` preparación · `I` implementación · `T` pruebas · `D` documentación.

## Hito 1 · Contenido y lectura
- [x] **T-001 · Redactar** la base (5 situaciones, 5 soluciones cada una) (P). Archivos: `bases de conocimiento/seguridad-incidentes.md`. Traza: RF-001, RF-007. Validación: `Every_real_situation_has_five_distinct_solutions_and_exactly_one_recommended`, `The_shipped_knowledge_base_has_five_well_formed_situations`.
- [x] **T-002 · Implementar** lectura y búsqueda de la base (I). Archivos: `Infrastructure/Knowledge/MarkdownKnowledgeBaseService.cs`. Traza: RF-001, RF-002. Validación: `Each_test_situation_resolves...`, `An_explicit_code_always_wins`, `Unrelated_questions_do_not_enter...`.
- [x] **T-003 · Implementar** el formato de entrega activo (I). Archivos: `Knowledge/DeliveryFormatService.cs`, `formatos de entrega/formato-respuesta-apa.md`. Traza: RF-005. Validación: `The_newest_file_is_the_active_format`, `A_word_template_is_read_as_text...`.
- [x] **T-004 · Parsear** soluciones en `KnowledgeEntry` (I). Archivos: `Domain/Entities/KnowledgeEntry.cs`, `KnowledgeSolution.cs`. Dep.: T-001. Traza: RF-007. Validación: `A_solution_stops_at_the_next_section...`.

## Hito 2 · Agentes y flujo fijo
- [x] **T-005 · Definir** los agentes del flujo (P). Archivos: `agents/{pdf-reader,kb-researcher,incident-responder,compliance-auditor,word-writer}.md`, registro en `appsettings.json`. Traza: RF-003, RN-004. Validación: pruebas de configuración (10 agentes).
- [x] **T-006 · Construir** el plan fijo y su integración con el Orquestador (I). Archivos: `Orchestration/KnowledgeWorkflow.cs`, `OrchestrationService.cs`. Dep.: T-002, T-005. Traza: RF-002, RF-003, RN-001. Validación: `A_matching_question_skips_free_planning...`, `Questions_outside_the_knowledge_base_use_the_normal_planner...`.
- [x] **T-007 · Incluir** al Lector PDF primero y limitarle el texto del documento (I). Archivos: `KnowledgeWorkflow.cs`, `PlanExecutor.cs`, `Domain/Entities/AgentTask.cs`. Dep.: T-006. Traza: RF-004, RF-010, CA-002, CA-009. Validación: `A_case_inside_an_attached_pdf...`, `With_the_flag_off_tasks_carrying_the_pdf_text_go_to_Claude_only`.
- [x] **T-008 · Aplicar** la privacidad de entradas confidenciales (I). Archivos: `KnowledgeWorkflow.cs`. Traza: RF-010. Validación: `Confidential_entries_are_only_ever_sent_to_Claude`.

## Hito 3 · Verificación de datos
- [x] **T-009 · Verificar** hechos (identificadores, contactos, plazos) y reparar una vez (I). Archivos: `KnowledgeWorkflow.cs`, `OrchestrationService.cs`. Traza: RF-006, RN-002, CA-005. Validación: `Invented_tickets_phones_and_deadlines_trigger_one_automatic_repair...`, `If_the_repair_does_not_fix_it_the_reply_warns...`.
- [x] **T-010 · Ignorar** correcciones truncadas (I). Archivos: `KnowledgeWorkflow.cs`. Traza: RF-012, CA-010. Validación: `A_correction_cut_by_the_token_limit_never_replaces...`.
- [x] **T-011 · Normalizar** el título del documento (I). Archivos: `KnowledgeWorkflow.cs`, `OrchestrationService.cs`. Traza: RF-009. Validación: `The_document_always_starts_with_the_canonical_title`.

## Hito 4 · Soluciones y elección
- [x] **T-012 · Presentar** opciones y guardar el caso pendiente (I). Archivos: `KnowledgeWorkflow.Choice.cs`, `OrchestrationService.cs`. Dep.: T-004, T-006. Traza: RF-007, RN-003, RNF-003, CA-001. Validación: `The_case_is_investigated_and_the_options_are_offered...`.
- [x] **T-013 · Interpretar** la elección (botón, letra, palabras, cancelar, otro caso, texto propio) (I). Archivos: `KnowledgeWorkflow.Choice.cs`. Dep.: T-012. Traza: RF-008, CA-003, CA-004. Validación: `Choosing_by_letter_delivers_only_that_solution`, `A_button_choice_id_is_used_exactly`, `A_custom_solution_is_reviewed...`, `Cancel_closes_the_case...`, `An_unclear_short_answer_asks_again...`, `A_message_about_another_situation_starts_a_new_case...`.
- [x] **T-014 · Responder** localmente una elección sin caso pendiente (I). Archivos: `OrchestrationService.cs`. Traza: RF-008, CA-006. Validación: `Choosing_an_option_without_a_pending_case_is_answered_locally...`.
- [x] **T-015 · Acotar** el estado pendiente (tope y vencimiento) (I). Archivos: `KnowledgeWorkflow.Choice.cs`. Traza: RNF-003. Validación: `Pending_cases_are_capped`.
- [x] **T-016 · Agregar** la bandera `OfferSolutionChoice` (I). Archivos: `Options/KnowledgeOptions.cs`, `appsettings.json`. Traza: RF-013, CA-007. Validación: `With_the_flag_off_the_full_flow_runs_in_one_go`.

## Hito 5 · Entregables
- [x] **T-017 · Generar** Word con estilo APA (I). Archivos: `Documents/MarkdownWordGenerator.cs`. Traza: RF-009, CA-008. Validación: `Apa_word_is_valid_and_uses_letter...`, `Apa_word_has_title_page_hanging_references...`.
- [x] **T-018 · Generar** PDF con estilo APA (I). Archivos: `Documents/MarkdownPdfGenerator.cs`, `Services/WordExportService.cs`. Traza: RF-009. Validación: `Pdf_apa_is_a_real_letter_pdf...`, `Pdf_default_handles_lists_tables_code_and_quotes`.
- [x] **T-019 · Entregar** Word y PDF al terminar el flujo (I). Archivos: `OrchestrationService.cs`. Dep.: T-017, T-018. Traza: RF-009. Validación: `The_answer_is_delivered_as_word_and_pdf_in_apa_style...`, `A_non_apa_format_gives_the_default_style`.
- [x] **T-020 · Exportar** respuestas desde el chat y descargar por tipo (I). Archivos: `Api/Controllers/Controllers.cs`, `Web/.../ChatMessageView.razor`, `OfficeStateService.cs`. Traza: RF-011. Validación: sondeo HTTP `POST /api/documents/word|pdf`; navegador.

## Hito 6 · Interfaz y documentación
- [x] **T-021 · Mostrar** botones de opción, campo para solución propia y cancelar (I). Archivos: `ChatMessageView.razor`, `office.css`. Traza: RF-008. Validación: navegador (sin prueba automatizada).
- [x] **T-022 · Documentar** el flujo y las banderas (D). Archivos: `README.md`. Traza: todos.
- [ ] **T-023 · (APLAZADA por decisión del responsable 2026-11-10: primero se deja el código listo con spec) Automatizar** la prueba de interfaz de opciones y descargas (T). Archivos: por localizar. Dep.: T-021. Traza: RF-008, RF-011. Validación: prueba Playwright. Riesgo: depende de adoptar E2E.

## Cobertura
Todos los RF/RN/CA cubiertos por pruebas automatizadas, salvo la interfaz de opciones y la exportación del chat (T-021, T-020 parcialmente) hasta T-023.

## Hito 7 · Reintento del Lector PDF con Claude (RF-014, decisión 2026-11-10)
- [x] **T-024 · Reintentar** una vez con Claude cuando falle el Lector PDF (I). Archivos: `Options/KnowledgeOptions.cs`, `Orchestration/OrchestratorDecision.cs`, `Domain/Entities/AgentTask.cs`, `Orchestration/OrchestrationService.cs`, `Orchestration/KnowledgeWorkflow.cs`, `Orchestration/PlanExecutor.cs`, `Agents/AgentRunner.cs`, `Interfaces/IOfficeServices.cs`. Dep.: T-007. Traza: RF-014, CA-011, CA-012. Validación: pruebas de T-025 (`ReaderFallbackTests`).
- [x] **T-025 · Probar** el reintento (éxito, doble falla, bandera apagada) (T). Archivos: `tests/.../KnowledgeTests.cs`. Dep.: T-024. Traza: CA-011, CA-012. Validación: `dotnet test` (`When_the_free_apis_fail_the_reader_retries_once_with_Claude...`, `When_Claude_also_fails_the_flow_stops...`, `With_the_flag_off_there_is_no_retry`).
- [x] **T-026 · Documentar** la bandera `RetryReaderWithClaude` (D). Archivos: `README.md`, `configuration/appsettings.json`. Dep.: T-024. Traza: RF-013, RF-014.
