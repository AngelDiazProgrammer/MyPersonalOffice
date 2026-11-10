# Tareas 006 · Núcleo de la oficina

**Maestro:** `.specify/maestros/05_TASKS.md` · **Nota:** backlog **retroactivo**; los estados reflejan evidencia del repositorio (253 pruebas aprobadas, 2 omitidas de Anthropic sin clave).

- [x] **T-001 · Orquestación y decisión validada** (I). RF-001, RF-004, RF-005. Prueba: `OrchestrationTests`.
- [x] **T-002 · Ejecución de planes** con paralelismo, dependencias, límites, tiempo y cancelación (I). RF-002, RF-003. Prueba: `OrchestrationTests`.
- [x] **T-003 · Agentes en Markdown**, validación y recarga (I). RF-006. Prueba: `ConfigurationAndValidationTests`.
- [x] **T-004 · PDF de entrada** con extracción local y entrega solo a agentes autorizados (I). RF-007. Prueba: `InfrastructureTests`, `OrchestrationTests`.
- [x] **T-005 · Exportación Word/PDF** (I). RF-008. Prueba: `WordTests`, `ApaAndWorkspaceTests`.
- [x] **T-006 · Contador de tokens** por agente y proveedor (I). RF-009. Prueba: `TokenUsageTests`.
- [x] **T-007 · Eventos en tiempo real** (I). RF-010. Prueba: `OrchestrationTests` (estado del agente por eventos).
- [x] **T-008 · Oficina 3D y chat** (I). RF-011. Verificación manual (sin prueba automática).
- [ ] **T-009 · Pruebas de interfaz y de la escena 3D** (T). CA-014. Aplazada por decisión (Playwright).
