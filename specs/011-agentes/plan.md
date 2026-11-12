# Plan técnico 011 · Agentes

**Maestro:** `.specify/maestros/04_PLAN.md` · **Estado:** implementado

## Resumen y objetivo técnico
Extender `AgentDefinition`, leer los nuevos campos del encabezado de `agents/*.md`, un servicio de administración que escribe esos archivos y una pantalla «Agentes».

## Decisiones de arquitectura
- **D-1** `AgentDefinition` gana `Purpose`, `Kind`, `Environment`, `Tools`, `Knowledge`, `Sensitive` e `IsSystem` con valores por defecto (los agentes existentes siguen cargando; su uso sale de `description`).
- **D-2** La carpeta `agents/` es el registro; `Agents:Definitions` queda vacío (ya soportado: si no hay lista, se cargan todos los `.md`).
- **D-3** `IAgentAdminService` (Application) con implementación Markdown (Infrastructure): valida con `AgentValidator` (puro), escribe de forma atómica, conserva las claves del encabezado que el formulario no maneja, y recarga el catálogo.
- **D-4** Agentes del sistema: `orchestrator`, `camila-ia`, `lector-uml` (también `system: true`): editables, no eliminables, id y tipo fijos.
- **D-5** `AgentTools` define las herramientas válidas (`buscar_conocimiento`, `consultar_servicio`, `escribir_servicio`, `leer_playbook`); el ejecutor de CamilaIA las impone.
- **D-6** API: `GET api/agents/catalog`, `GET api/agents/{id}/definition`, `POST api/agents`, `PUT api/agents/{id}`, `DELETE api/agents/{id}`.
- **D-7** Front: ítem «Agentes» (`/agentes`) con tarjetas y editor; la sección de pruebas conversacionales pasa a `/conversacionales`.

## Pruebas
Validador, escritura/edición/eliminación sobre carpeta temporal (conserva claves, bloquea sistema y en uso), carga de los campos nuevos, herramientas impuestas, contratos y verificación visual.
