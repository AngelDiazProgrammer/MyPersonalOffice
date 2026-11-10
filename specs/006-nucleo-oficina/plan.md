# Plan técnico 006 · Núcleo de la oficina

**Maestro:** `.specify/maestros/04_PLAN.md` · **Estado:** implementado (plan retroactivo)

## Resumen y objetivo técnico
Describir la arquitectura existente que satisface la spec 006, sin cambios de código.

## Referencias
`specs/006-nucleo-oficina/spec.md`; constitución v1.1.0 (Art. 6, 7, 10, 22, 38); README «Arquitectura».

## Estado actual relevante (evidencia)
Capas `Domain` ← `Application` ← `Infrastructure`; `Api` como raíz de composición y `Web` Blazor (spec 005). Agentes en `agents/*.md`.

## Decisiones de arquitectura
- **D-1** Decisión del Orquestador por herramienta forzada con JSON Schema (`DecisionSchema`), sin analizar texto libre.
- **D-2** Validación del plan por `PlanGraph`/`Validators` (existencia, ids, ciclos, máximo de tareas) con un reintento.
- **D-3** `PlanExecutor` con semáforo de concurrencia global, un candado por agente, tiempo límite y cancelación por token.
- **D-4** Agentes como configuración Markdown (`AgentCatalogService`, `AgentRegistry`) para evitar una clase por agente.
- **D-5** Todo cambio de estado pasa por `AgentStateService`, que emite eventos; la interfaz solo los refleja (hub SignalR).
- **D-6** Estado en memoria con topes (`InMemoryStores`, `TaskStore`), sin persistencia (Art. 10).
- **D-7** Texto del PDF entre etiquetas neutralizadas (`PromptBuilder`) y regla de tratarlo como datos.
- **D-8** Exportación Word por Open XML y PDF por QuestPDF (Community) en `WordExportService`.
- **D-9** `TokenUsageTracker` agrupa por agente y proveedor; costo solo con `PricingOptions`.
- **D-10** Oficina 3D con three.js en la interfaz, con figuras procedurales propias.

## Diseño propuesto
Sin cambios: ver `Application/Orchestration`, `Application/Agents`, `Application/Services`, `Infrastructure/Documents` y `Web/wwwroot`.

## Flujos
Petición → decisión → validación → ejecución del plan → consolidación → respuesta; eventos durante todo el proceso.

## Contratos y datos
DTO públicos fijados por pruebas de contrato (spec 005).

## Seguridad y privacidad
Spec 003; registros sin contenido (spec 005).

## Pruebas (existentes)
`OrchestrationTests`, `ConfigurationAndValidationTests`, `InfrastructureTests`, `WordTests`, `TokenUsageTests`, `ApaAndWorkspaceTests`.

## Riesgos y mitigaciones
Consumo de modelos (límites y ahorro de tokens); interfaz 3D sin pruebas automáticas (verificación manual).
