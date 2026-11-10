# Aclaraciones 005 · Cumplimiento constitucional

**Maestro:** `.specify/maestros/03_CLARIFY.md` · **Fecha:** 2026-11-10 · **Constitución:** v1.1.0

## Registro de decisiones

| ID | Respuesta del responsable (resumida fielmente) | Requisito o criterio afectado | Cambio aplicado | Estado |
|---|---|---|---|---|
| Q-001 | **Desaprobado** que `Web` dependa de Infrastructure: para eso existe la capa Application, que es la que se comunica con Infrastructure; `Web` no debe verla. | RF-008 (nuevo), RNF-002 (reescrito), CA-008, CA-009 (nuevos) | La excepción propuesta se elimina; el saneador de Markdown y la configuración de arranque deben quedar detrás de contratos de Application | Resuelta |
| Q-002 | **Sí, JSON** para los registros. | RF-007 (nuevo), CA-007 (nuevo) | Registros en JSON estructurado con campos de correlación | Resuelta |
| Q-003 | **Aprobada** la especificación para pasar a clarify y plan. | Estado del documento | Estado → «especificación aprobada; plan pendiente de aprobación» | Resuelta |

## Ambigüedades menores — RESUELTAS al aprobar el plan (2026-11-10: «Sí, aprobado», con las recomendaciones indicadas)

- **Q-004 · Origen del identificador de correlación.** *Por qué importa:* un identificador aceptado desde el cliente podría falsificarse o contener contenido arbitrario en los registros. *Opciones:* (a) generarlo siempre en el servidor; (b) aceptar uno opcional del cliente validado. *Recomendación (no decisión):* (a).
- **Q-005 · Detalle del estado de salud.** *Por qué importa:* un detalle excesivo expone información interna. *Opciones:* (a) solo «listo/no listo» con el nombre del componente que falla; (b) además mensajes técnicos. *Recomendación (no decisión):* (a), sin rutas ni claves.
- **Q-006 · Ubicación de la lista de producción y del índice de decisiones.** *Opciones:* (a) carpeta `docs/` en la raíz; (b) dentro de `specs/`. *Recomendación (no decisión):* (a), porque no pertenecen a una funcionalidad concreta.

## Criterio de finalización
No quedan preguntas bloqueantes ni ambigüedades de alto impacto; Q-004 a Q-006 quedan registradas como pendientes menores.
