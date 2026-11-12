# Tareas 012 · Playbooks de bots, LectorUML y CamilaIA

**Maestro:** `.specify/maestros/05_TASKS.md`

- [x] **T-001 · Formato y generador** de `playbooks bots/<bot>/` (maestro, un playbook por flujo, catálogos, `notas.md`) y de la narrativa `CamilaIA.md` sin bloques; comando `--bot-generar` (alias `--camila-generar`). RF-001..RF-004.
- [x] **T-002 · Cargador multi-bot** (`BotPlaybookSource`, `ICamilaBotSource`) y validación de referencias. RF-013.
- [x] **T-003 · LectorUML**: el workflow ejecuta el playbook del bot con el motor, con las herramientas del agente impuestas. RF-005.
- [x] **T-004 · CamilaIA como agente**: persona, tope de tokens, `sensitive` y herramientas desde `agents/camila-ia.md`; notas y narrativa; consumo atribuido a `agent:camila-ia`. RF-008, RF-009.
- [x] **T-005 · Destino del chat** (`ChatRequestDto.Target`, `CamilaTarget`), estados del agente ejecutor, `GET /api/bots`. RF-010.
- [x] **T-006 · Secciones**: Chatbots por bot y Conversacionales por agente, con tarjeta y entorno al hacer clic; las oficinas de Camila salen de la configuración. RF-006, RF-007, RF-011.
- [x] **T-007 · Pruebas** (`BotPlaybookTests`, `CamilaAgentDefinitionTests`, destino, recorrido del playbook real) y **verificación en la app real**. CA-001..CA-007.
