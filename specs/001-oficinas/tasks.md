# Tareas 001 · Oficinas

**Maestro:** `.specify/maestros/05_TASKS.md` · **Nota:** backlog **retroactivo**; los estados reflejan evidencia del repositorio (compilación y `dotnet test`: 220 aprobadas, 2 omitidas de Anthropic en vivo).

Leyenda: `P` preparación · `I` implementación · `T` pruebas · `D` documentación.

## Hito 1 · Configuración y núcleo
- [x] **T-001 · Definir** las oficinas en configuración (P). Obj.: `Workspaces:Items` con las dos oficinas. Archivos: `configuration/appsettings.json`, `Options/WorkspacesOptions.cs`. Dep.: —. Traza: RF-002, RF-006. Validación: lectura de configuración en `WorkspaceTests`.
- [x] **T-002 · Crear** `IWorkspaceService` y `WorkspaceService` (I). Obj.: selección, activo, pertenencia y orden. Archivos: `Services/WorkspaceService.cs`, `Interfaces/ILlmServices.cs`. Dep.: T-001. Traza: RF-001, RF-007, RN-001. Validación: `WorkspaceTests` (selección, caso sin oficinas).
- [x] **T-003 · Agregar** el evento `WorkspaceChanged` (I). Archivos: `Domain/Enums/OfficeEventType.cs`. Dep.: —. Traza: RF-008.

## Hito 2 · Filtrado en la aplicación
- [x] **T-004 · Filtrar** agentes y exponer oficinas en la consulta (I). Archivos: `Services/OfficeQueryService.cs`, `DTOs/Dtos.cs`. Dep.: T-002. Traza: RF-003. Validación: `Query_service_lists_only_the_active_office_agents...`.
- [x] **T-005 · Exigir** oficina y limitar el equipo del Orquestador (I). Archivos: `Orchestration/OrchestrationService.cs`. Dep.: T-002. Traza: RF-004, RN-002. Validación: `Chat_is_blocked_until_an_office_is_chosen...`.
- [x] **T-006 · Restringir** el flujo de la base de conocimiento a agentes activos (I). Archivos: `Orchestration/KnowledgeWorkflow.cs`. Dep.: T-002. Traza: RN-003. Validación: pruebas de `KnowledgeTests` (agente ausente → planificación normal).

## Hito 3 · API e interfaz
- [x] **T-007 · Publicar** `GET/POST /api/workspaces` (select, reset) con evento (I). Archivos: `Api/Controllers/Controllers.cs`. Dep.: T-002, T-003. Traza: RF-001, RF-005, RF-008. Validación: sondeos HTTP (id inválido → 400).
- [x] **T-008 · Crear** la pantalla de selección y el botón «cambiar de oficina» (I). Archivos: `Pages/Home.razor`, `OfficeStateService.cs`, `OfficeApiClient.cs`, `office.css`. Dep.: T-007. Traza: RF-001, RF-005, CA-001, CA-004. Validación: inspección en navegador.
- [x] **T-009 · Adaptar** la escena 3D al cambio de lista de agentes y ocultar puestos sin dueño (I). Archivos: `wwwroot/js/office3d.js`. Dep.: T-008. Traza: RF-003, CA-002. Validación: inspección con `officeScene.debug()`.
- [x] **T-010 · Actualizar** la interfaz ante `WorkspaceChanged` externo (I). Archivos: `OfficeStateService.cs`. Dep.: T-007. Traza: RF-008, CA-007. Validación: inspección manual.

## Hito 4 · Pruebas y documentación
- [x] **T-011 · Escribir** pruebas unitarias de oficinas (T). Archivos: `tests/.../ApaAndWorkspaceTests.cs`. Dep.: T-002..T-006. Traza: CA-003, CA-005, CA-006. Validación: `dotnet test`.
- [x] **T-012 · Documentar** las oficinas en el README (D). Archivos: `README.md`. Dep.: T-008. Traza: RF-002, RF-006.
- [ ] **T-013 · (APLAZADA por decisión del responsable 2026-11-10) Automatizar** la prueba de interfaz de selección/cambio de oficina (T). Archivos: por localizar (proyecto E2E no existe). Dep.: T-008. Traza: CA-001, CA-002, CA-004. Validación: prueba Playwright. Riesgo: requiere acordar el enfoque de pruebas E2E.

## Cobertura
RF-001..RF-008 y RN-001..RN-003 cubiertos; CA-002, CA-004 y CA-007 solo con verificación manual hasta T-013.
