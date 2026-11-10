# Plan técnico 001 · Oficinas

**Maestro:** `.specify/maestros/04_PLAN.md` · **Estado:** implementado (plan retroactivo)

## Resumen y objetivo técnico
Filtrar agentes, planificación y visualización por una oficina activa mantenida en memoria, definida por configuración, sin cambiar la arquitectura ni introducir persistencia.

## Referencias
Especificación `specs/001-oficinas/spec.md`; constitución v1.1.0 (Art. 6, 7, 10, 38).

## Estado actual relevante (evidencia)
- Capas Domain/Application/Infrastructure/Api/Web (`src/`), agentes en `agents/*.md`, registro en `configuration/appsettings.json`.
- Los agentes se leían todos desde el registro; la 3D usaba siempre 12 puestos fijos.

## Decisiones de arquitectura
- **D-1** Servicio de aplicación `IWorkspaceService` con estado en memoria (alternativa descartada: persistir en archivo/DB — Art. 10 exige necesidad y especificación aprobada).
- **D-2** Oficinas como configuración (`Workspaces:Items`), sin una clase por oficina.
- **D-3** El filtrado se aplica en la capa de aplicación (consulta, orquestación y flujo fijo), no solo en la interfaz (Art. 21: no basta ocultar en la UI).
- **D-4** La interfaz se actualiza por evento `WorkspaceChanged` (alternativa: sondeo).

## Diseño propuesto
- `WorkspaceService` (selección, activo, `IsAgentActive`, `OrderOf`).
- `OfficeQueryService.GetAgents/GetWorkspaces`: agentes de la oficina activa ordenados (Orquestador primero).
- `OrchestrationService`: exige oficina elegida y limita el equipo; `KnowledgeWorkflow.TryPlanAsync` valida que sus agentes estén activos.
- `WorkspacesController`: `GET /api/workspaces`, `POST select`, `POST reset`.
- Web: pantalla de selección en `Home.razor`, `OfficeStateService` (selección, cambio, limpieza de conversación).
- 3D: `setAgents` rehace los puestos al cambiar la lista; los escritorios sin dueño se ocultan.

## Flujos
Principal: arranque → selección → carga de agentes → trabajo. Alternativos: «cambiar de oficina» (limpia estado); chat sin oficina → 400; oficina elegida desde otro cliente → evento → recarga. Error: id desconocido → 400 «Oficina desconocida».

## Contratos
`WorkspacesDto(Available, ActiveId, Required)`, `WorkspaceDto`, `WorkspaceAgentDto`, `WorkspaceSelectRequestDto`; evento `OfficeEventType.WorkspaceChanged`; `OfficeStateDto.ActiveWorkspaceId`.

## Datos y persistencia
Lee `Workspaces` de configuración. Escribe solo en memoria. Sin cambios de formato de agentes.

## Seguridad y privacidad
Validación de id; los endpoints de cambio de estado están protegidos por la guardia de la especificación 003 (origen y cabecera). Sin datos sensibles.

## Integraciones y proveedores
No aplica.

## Cambios por proyecto/archivo
- Application: `Services/WorkspaceService.cs`, `Options/WorkspacesOptions.cs`, `Interfaces/ILlmServices.cs` (`IWorkspaceService`), `Services/OfficeQueryService.cs`, `Orchestration/OrchestrationService.cs`, `Orchestration/KnowledgeWorkflow.cs`, `DTOs/Dtos.cs`.
- Domain: `Enums/OfficeEventType.cs`.
- Api: `Controllers/Controllers.cs` (`WorkspacesController`).
- Web: `Components/Pages/Home.razor`, `Services/OfficeStateService.cs`, `Services/OfficeApiClient.cs`, `wwwroot/js/office3d.js`, `wwwroot/css/office.css`.
- Configuración: `configuration/appsettings.json`.

## Pruebas
Unitarias: `WorkspaceTests` (selección obligatoria, filtrado de agentes, chat bloqueado, orden de puestos). Interfaz y 3D: verificación manual en navegador (sin prueba automatizada).

## Despliegue/configuración/rollback
Solo configuración; quitar `Workspaces` restaura el comportamiento original.

## Riesgos, dependencias y decisiones pendientes
- Estado global: no apto para multiusuario (Q-001 de la especificación).
- Sin pruebas E2E de la pantalla de selección ni de la escena.
