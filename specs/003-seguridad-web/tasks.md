# Tareas 003 · Seguridad web

**Maestro:** `.specify/maestros/05_TASKS.md` · **Nota:** backlog **retroactivo**; estados respaldados por evidencia (`dotnet test`: 220 aprobadas, 2 omitidas de Anthropic en vivo; `dotnet list package --vulnerable`: sin hallazgos).

## Hito 1 · Guardia y cabeceras
- [x] **T-001 · Crear** `SecurityHeadersMiddleware` (I). Archivos: `Api/Middleware/SecurityMiddleware.cs`. Traza: RF-003, CA-003. Validación: `Every_response_carries_the_security_headers`.
- [x] **T-002 · Crear** `RequestGuardMiddleware` (origen y cabecera de cliente) (I). Archivos: `SecurityMiddleware.cs`. Traza: RF-001, RF-002, RN-001. Validación: `A_foreign_origin_is_rejected...`, `Requests_that_change_state_need_the_client_header`, `A_foreign_origin_cannot_change_state...`, `Paths_outside_the_guarded_prefixes...`.
- [x] **T-003 · Registrar** `UseOfficeSecurity` en ambos hosts y orígenes configurables (I). Archivos: `Extensions/OfficeApiExtensions.cs`, `Api/Program.cs`, `Web/Program.cs`. Dep.: T-001, T-002. Traza: RF-001, RF-003. Validación: sondeo del WebSocket (403 con origen ajeno, 101 con el propio).
- [x] **T-004 · Enviar** la cabecera de cliente desde la interfaz (I). Archivos: `Web/Program.cs`. Dep.: T-002. Traza: RF-002, RNF-001, CA-006. Validación: navegador (abrir oficina, escena, sin errores de política).

## Hito 2 · Salida de modelos y entrada de documentos
- [x] **T-005 · Extraer** `SafeMarkdown` sin imágenes y con enlaces seguros (I). Archivos: `Infrastructure/Documents/SafeMarkdown.cs`, `MarkdownView.razor`. Traza: RF-004, CA-004. Validación: `Images_are_never_loaded...`, `Raw_html_and_unsafe_links_are_neutralized...`.
- [x] **T-006 · Neutralizar** etiquetas de documento y añadir la regla de seguridad a los prompts (I). Archivos: `Orchestration/PromptBuilder.cs`. Traza: RF-005, RN-002, CA-005. Validación: `A_pdf_cannot_close_the_document_zone...`, `Agents_are_told_that_document_content_is_data...`.

## Hito 3 · Límites y errores
- [x] **T-007 · Limitar** el chat por minuto (I). Archivos: `OfficeApiExtensions.cs`, `Controllers.cs`. Traza: RF-006, CA-011. Validación: por definir (ver T-013).
- [x] **T-008 · Acotar** conversaciones y casos pendientes (I). Archivos: `Services/InMemoryStores.cs`, `KnowledgeWorkflow.Choice.cs`. Traza: RF-007, CA-007. Validación: `The_conversation_store_drops...`, `A_conversation_with_a_running_request...`, `Pending_cases_are_capped`.
- [x] **T-009 · Limpiar** mensajes de error de proveedores y de validación (I). Archivos: `Llm/ErrorText.cs`, `FreeApiService.cs`, `AnthropicService.cs`, `OfficeApiExtensions.cs`. Traza: RF-008, CA-008. Validación: `Provider_errors_are_shown_without_internal_identifiers...`.
- [x] **T-010 · Responder** localmente la elección sin caso pendiente (I). Archivos: `OrchestrationService.cs`. Traza: RF-011, CA-010. Validación: `Choosing_an_option_without_a_pending_case...`.

## Hito 4 · Dependencias y configuración
- [x] **T-011 · Actualizar** SignalR.Client a 8.0.33 y fijar paquetes transitivos de pruebas (P). Archivos: `Web.csproj`, `Tests.csproj`. Traza: RF-009, CA-009. Validación: `dotnet list package --vulnerable --include-transitive`.
- [x] **T-012 · Limitar** `AllowedHosts` de la API independiente (P). Archivos: `Api/appsettings.json`. Traza: RF-010. Validación: inspección de configuración.

## Hito 5 · Documentación y pendientes
- [x] **T-013a · Documentar** los controles en el README (D). Archivos: `README.md`. Traza: todos.
- [x] **T-013 · Probar** automáticamente el límite de tasa (429) y el `AllowedHosts` (T). Archivos: `tests/.../SecurityTests.cs` (`RateLimitTests`, `AllowedHostsTests`); `OfficeApiExtensions.AddOfficeRateLimiting` extraído para poder probarlo. Dep.: T-007, T-012. Traza: RF-006, RF-010, CA-011. Validación: pruebas contra servidor real.
- [x] **T-014 · Repetir** el barrido completo de sondeos de seguridad (subidas y exportaciones) como prueba reproducible (T). Archivos: `tests/security-probes/run_probes.py` (34 comprobaciones; requiere la app en marcha; no llama a modelos). Resultado: 34/34. Traza: RF-004, RF-008. Riesgo: el guion de sondeos se colgó en una ejecución y no se completó.

## Cobertura
Todos los RF con pruebas automatizadas o sondeos reproducibles; CA-006 (consola del navegador sin violaciones de CSP) solo manual hasta que se adopten pruebas de interfaz.
