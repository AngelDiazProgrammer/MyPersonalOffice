# Plan técnico 002 · Flujo de la base de conocimiento

**Maestro:** `.specify/maestros/04_PLAN.md` · **Estado:** implementado (plan retroactivo)

## Resumen y objetivo técnico
Un flujo determinista, controlado por el sistema, que consulta la base antes de cualquier modelo, reparte el trabajo en agentes exclusivos, verifica los datos y entrega documentos, con un estado pendiente en memoria para la elección de solución.

## Referencias
`spec.md` de esta carpeta; constitución v1.1.0 (Art. 9, 11, 12, 16, 17, 20, 25, 26).

## Estado actual relevante (evidencia)
- Orquestación por decisión estructurada del modelo (`OrchestrationService.PlanAsync`) y `PlanExecutor` con dependencias.
- Word generado con Open XML (`MarkdownWordGenerator`); agentes definidos en `agents/*.md`; enrutamiento de modelos con modo Sensitive.

## Decisiones de arquitectura
- **D-1** Flujo fijo en código (`KnowledgeWorkflow`), no decidido por un modelo: da precisión y trazabilidad (alternativa: dejar al Orquestador delegar libremente — descartada por no garantizar el orden ni la consulta previa).
- **D-2** Agentes `workflowOnly` (no ofrecidos a la planificación libre).
- **D-3** Dos fases: investigación + opciones; entrega tras la elección. Estado pendiente por conversación en memoria (`PendingChoiceStore`, tope y vencimiento) — sin base de datos (Art. 10).
- **D-4** Las opciones al usuario las compone el sistema desde la base (no un modelo) para no inventar datos.
- **D-5** Verificación de hechos por el sistema (regex de identificadores, contactos y plazos) + una tarea de reparación del auditor.
- **D-6** Interpretación de la elección determinista (botón, letra, palabras, cancelar, otro caso, texto propio).
- **D-7** Entregables en Word (Open XML) y PDF (QuestPDF, licencia Community); estilo APA por detección en el formato (`Auto`).
- **D-8** El Redactor Word asume el rol de editor de formato (sin agente adicional).
- **D-9** Reintento del Lector PDF con Claude (RF-014): bandera por tarea `RetryWithClaude` (la fija el flujo según `Knowledge:RetryReaderWithClaude`); `PlanExecutor` reintenta UNA vez la ejecución del agente con `forceClaude` ante `LlmException` u otro error no cancelatorio (alternativa descartada: pasar el texto crudo al Investigador — rompe la regla de que solo el Lector ve el documento).

## Diseño propuesto
`MarkdownKnowledgeBaseService` (lectura y búsqueda), `DeliveryFormatService` (formato más reciente), `KnowledgeWorkflow` (+ `.Choice`: opciones, elección, plan de entrega), `OrchestrationService` (integración, entregables, normalización del título), `PlanExecutor` (omitir Word automático, documentos permitidos a gratuitos, reintento con Claude), `AgentRunner` (parámetro `forceClaude`), `KnowledgeEntry.Solutions`.

## Flujos
Fase 1: adjuntos → Lector PDF → Investigador → opciones y espera. Fase 2: elección → Respondedor → Redactor Word → Auditor → verificación → Word y PDF. Alternativos: sin opciones → entrega directa; cancelar; ambiguo → repregunta; otro caso → caso nuevo; reparación del auditor. Errores: tarea fallida → mensaje sin entregable; corrección truncada → se conserva el original.

## Contratos
`KnowledgePlan`, `PendingKnowledgeCase`, `ChoiceResolution`; DTO `KnowledgeChoiceDto`, `SolutionOptionDto`; `ChatRequestDto.ChoiceOptionId`; `ChatResponseDto.Choice/Documents`; endpoints `POST /api/documents/word|pdf`; archivos `bases de conocimiento/*.md` (`### Solución A · Título`) y `formatos de entrega/*`.

## Datos y persistencia
Lee archivos de la base y del formato; escribe documentos en `generated-documents/` (retención configurable) y estado pendiente en memoria.

## Seguridad y privacidad
Texto de documentos solo al Lector; bandera `AllowFreeApisWithDocuments`; entradas confidenciales solo a Claude; contenido de PDF tratado como datos (spec 003); sin datos inventados (RN-002).

## Integraciones y proveedores
Mediante `IAnthropicService` (enrutador). Tiempo máximo por tarea, cancelación y reintentos acotados existentes; sin reintentos nuevos ilimitados.

## Cambios por proyecto/archivo
- Domain: `Entities/KnowledgeEntry.cs`, `KnowledgeSolution.cs`, `AgentTask.cs`.
- Application: `Orchestration/KnowledgeWorkflow.cs`, `KnowledgeWorkflow.Choice.cs`, `OrchestrationService.cs`, `PlanExecutor.cs`, `Options/KnowledgeOptions.cs`, `Services/WordExportService.cs`, `DTOs/Dtos.cs`.
- Infrastructure: `Knowledge/*`, `Documents/MarkdownWordGenerator.cs`, `MarkdownPdfGenerator.cs`.
- Api: `Controllers/Controllers.cs` (`KnowledgeController`, `DocumentsController`).
- Web: `Components/Chat/ChatMessageView.razor`, `OfficeStateService.cs`, `wwwroot/css/office.css`.
- Contenido: `agents/{pdf-reader,kb-researcher,incident-responder,compliance-auditor,word-writer}.md`, `bases de conocimiento/`, `formatos de entrega/`.

## Pruebas
`KnowledgeTests`, `KnowledgeFactCheckTests`, `KnowledgeChoiceTests`, `WordTests`, `ApaAndWorkspaceTests` (APA), más ejecuciones reales de los 5 casos con proveedores reales.

## Despliegue/configuración/rollback
Sección `Knowledge` de `appsettings.json` (`OfferSolutionChoice`, `DocumentStyle`, `AllowFreeApisWithDocuments`, `RetryReaderWithClaude`, agentes). Rollback: `OfferSolutionChoice=false` o `Enabled=false`.

## Riesgos, dependencias y decisiones pendientes
Q-001 y Q-002 de la especificación; coincidencia por palabras clave; QuestPDF sin sangría francesa en PDF.
