# Tareas 004 · Proveedores de modelos

**Maestro:** `.specify/maestros/05_TASKS.md` · **Nota:** backlog **retroactivo**; estados respaldados por evidencia (`dotnet test`: 220 aprobadas, 2 omitidas de Anthropic en vivo).

## Hito 1 · Enrutador y proveedores
- [x] **T-001 · Crear** el cliente genérico de APIs gratuitas (I). Archivos: `Infrastructure/Llm/FreeApiService.cs`, `LlmOptions.cs`. Traza: RF-002, RN-002. Validación: `One_generic_service_serves_every_provider...`, `A_provider_with_a_missing_placeholder_variable_is_not_configured`.
- [x] **T-002 · Crear** el enrutador y los modos (I). Archivos: `RoutingLlmService.cs`, `Interfaces/ILlmServices.cs`. Dep.: T-001. Traza: RF-001, RF-007, CA-001. Validación: `Free_first_answers...`, `Claude_only_mode_skips_free_APIs`, `Free_only_mode_never_uses_Claude...`, `The_mode_can_be_switched_at_runtime`, `Per_request_preference_forces_Claude`.
- [x] **T-003 · Pausar** y omitir proveedores según el fallo (I). Archivos: `FreeApiService.cs`. Traza: RF-004, CA-002, CA-003. Validación: `A_rate_limited_provider_is_paused...`, `When_every_free_provider_fails_Claude_takes_over`, `Rejected_keys_pause...`, `A_model_the_key_cannot_use...`.
- [x] **T-004 · Garantizar** la privacidad de solicitudes sensibles (I). Archivos: `RoutingLlmService.cs`. Traza: RF-006, RN-003, CA-005. Validación: `Requests_with_documents_never_reach_free_APIs`, `Sensitive_requests_in_free_only_mode_are_refused...`.
- [x] **T-005 · Registrar** Groq, Cloudflare y Gemini en la configuración (P). Archivos: `configuration/appsettings.json`, `.env.example`. Traza: RF-002, RF-003. Validación: lectura de configuración; `GET /api/llm/status`.

## Hito 2 · Resiliencia y diagnóstico
- [x] **T-006 · Tratar** errores con forma de arreglo y respuestas ilegibles como fallo del proveedor (I). Archivos: `FreeApiService.cs` (`ExtractError`, `TrySendAsync`). Traza: RF-005, CA-004. Validación: `An_error_wrapped_in_a_json_array...`.
- [x] **T-007 · Aceptar** el prefijo `models/` en la verificación de arranque (I). Archivos: `FreeApiService.cs`. Traza: RF-009, CA-006. Validación: `Startup_check_accepts_model_ids_listed_with_the_models_prefix...`, `Startup_check_flags_a_configured_model_missing...`.
- [x] **T-008 · Publicar** la prueba de proveedor (I). Archivos: `FreeApiService.cs`, `RoutingLlmService.cs`, `Controllers.cs`, `Dtos.cs`. Traza: RF-010, CA-007. Validación: `The_provider_test_reports_success_or_the_real_error...`; prueba real de Gemini.
- [x] **T-009 · Reintentar** Claude sin `temperature` y con `tool_choice` automático (I). Archivos: `Anthropic/AnthropicService.cs`. Traza: RF-012, CA-010. Validación: `Models_that_deprecate_temperature...`, `Models_that_reject_forced_tool_choice...`.
- [x] **T-010 · Limpiar** mensajes de error mostrados (I). Archivos: `ErrorText.cs`. Traza: RF-011. Validación: `Provider_errors_are_shown_without_internal_identifiers...` (spec 003).

## Hito 3 · Consumo e interfaz
- [x] **T-011 · Separar** el consumo por proveedor y agente (I). Archivos: `Services/InMemoryStores.cs`, `OfficeQueryService.cs`, `Home.razor`. Traza: RF-008, CA-008. Validación: `Usage_is_split_per_provider`, `Usage_is_grouped_per_agent...`.

## Hito 4 · Validación real y documentación
- [x] **T-012 · Validar** Gemini de extremo a extremo con solo ese proveedor disponible (T). Traza: RF-003, CA-009. Validación: solicitud simple (1 llamada) y flujo completo de la base (5 llamadas, Word y PDF) — evidencia de sesión.
- [x] **T-013 · Documentar** proveedores, claves y modelo de Gemini (D). Archivos: `README.md`. Traza: RF-003.
- [ ] **T-014 · Decidir** y, si procede, implementar una bandera de privacidad por proveedor (I). Archivos: por localizar. Traza: RF-006 (Q-001). Riesgo: requiere decisión del responsable.
- [ ] **T-015 · Decidir** si se agrega selector de proveedor en la interfaz (I). Traza: Q-002.

## Cobertura
RF-001..RF-012 cubiertos; CA-009 validado con evidencia de sesión (no reproducible por CI).
