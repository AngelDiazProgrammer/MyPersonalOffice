# Plan técnico 012 · Playbooks de bots, LectorUML y CamilaIA

**Maestro:** `.specify/maestros/04_PLAN.md` · **Estado:** implementado

## Resumen y objetivo técnico
Partir el `CamilaIA.md` de 1 MB en playbooks por bot y por flujo, cargar varios bots, ejecutar el UML con un agente ejecutor y mover CamilaIA a un agente real, con el chat dirigido por destino.

## Decisiones de arquitectura
- **D-1** Carpeta `playbooks bots/<bot>/` con `bot.md` (maestro con encabezado: id, nombre, descripción, icono, `executor`, `agent`, `startFlow`, `knowledge`), `flujos/*.md`, `catalogos/*.md` y `notas.md`. El generador limpia `flujos/` y `catalogos/` y nunca toca `notas.md`.
- **D-2** Cada archivo conserva el formato ya probado: explicación en lenguaje claro más bloques ` ```json camila:* ` exactos. El cargador agrupa todos los bloques en un `CamilaBot` (el mismo modelo y el mismo motor).
- **D-3** `ICamilaBotSource` pasa a ser multi-bot: `Bots`, `Get(id)`, `Notes(id)`, `Narrative(id)`, `ReloadAsync`.
- **D-4** La narrativa de la base de conocimiento (`CamilaIA.md`) se genera **sin bloques**; CamilaIA la usa en su herramienta `buscar_conocimiento` (sección del flujo más parecido).
- **D-5** LectorUML = agente `executor`; su runtime es `CamilaEngine` + `CamilaUmlView`; elige el bot por el destino de la petición.
- **D-6** CamilaIA = agente `llm`; `CamilaChatAgent` lee su `AgentDefinition` (instrucciones, tope de tokens, herramientas, `sensitive`) y añade el protocolo técnico (acciones y esquema) y `notas.md`. Llamadas con `Purpose = agent:camila-ia` para atribuir consumo.
- **D-7** `ChatRequestDto.Target` (`bot:<id>` | `agent:<id>`), validado; el orquestador lo atiende antes de exigir una oficina. Las oficinas de agentes siguen igual.
- **D-8** Estados de agente (`Working`/`Completed`) del agente ejecutor en cada turno.
- **D-9** El comando `--bot-generar [id]` sustituye a `--camila-generar` (que queda como alias); la configuración `Camila:BotsDirectory` indica la carpeta.

## Pruebas
Generador (determinismo, `notas.md` intacto, sin secretos), cargador multi-bot y validación de referencias, LectorUML sobre un bot de prueba y sobre el real, CamilaIA con definición (herramientas impuestas, notas, narrativa), destino del chat, contratos y verificación visual.
