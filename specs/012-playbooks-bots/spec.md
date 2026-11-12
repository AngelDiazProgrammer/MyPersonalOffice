# Especificación 012 · Playbooks maestros de bots, LectorUML y CamilaIA como agentes

**Estado:** Aprobada en chat por la persona responsable (2026-11-12): carpeta «playbooks bots», el flujo UML de cada bot es un playbook de pasos que LectorUML ejecuta, CamilaIA es el agente que lleva la conversación y la base de conocimiento, y se mantienen las dos fuentes (playbooks + narrativa).
**Constitución:** v1.1.0 · **Relacionadas:** 009, 010, 011.

## Resumen
Cada bot queda descrito en una carpeta de **playbooks maestros** (`playbooks bots/<bot>/`). Un único agente **LectorUML** (ejecutor, sin modelo) lee el playbook de un bot y ejecuta su flujo UML en el entorno de pruebas. **CamilaIA** es el agente conversacional que usa la base de conocimiento del bot. Los entornos de Chatbots y Conversacionales se construyen desde los bots y los agentes, no desde la configuración de oficinas.

## Requisitos funcionales
- **RF-001** Cada bot tiene una carpeta con un maestro `bot.md`, un playbook por flujo, catálogos (intents, entidades, generadores, webhooks, grupos de rutas) y `notas.md`.
- **RF-002** Cada página del playbook describe sus **pasos en orden** (asigna, llama webhook, ejecuta IA, dice, ofrece botones, pide datos, rutas) con su definición exacta ejecutable.
- **RF-003** Un comando genera (o regenera) los playbooks y la narrativa de la base de conocimiento desde el export del bot; el resultado es siempre el mismo y no contiene credenciales.
- **RF-004** `notas.md` es editable a mano y **nunca se sobrescribe** al regenerar; CamilaIA lo recibe como contexto (tono, preguntas frecuentes, avisos).
- **RF-005** Hay **un solo** agente LectorUML (ejecutor). Para cualquier bot, lee su playbook y ejecuta el flujo con botones, textos, formularios, rutas y webhooks tal como los define el bot.
- **RF-006** La sección Chatbots lista los **bots** (uno por playbook maestro); al hacer clic abre el entorno de pruebas ejecutado por LectorUML.
- **RF-007** La sección Conversacionales lista los agentes conversacionales (hoy CamilaIA); al hacer clic abre su entorno.
- **RF-008** CamilaIA es un agente definido en Markdown: su personalidad y reglas viven en `agents/camila-ia.md`; su consumo, estados y registro se atribuyen a ella.
- **RF-009** CamilaIA se apoya en la narrativa de la base de conocimiento (`bases de conocimiento/CamilaIA.md`, sin bloques ejecutables) y en el catálogo de servicios del bot; conversa libremente (spec 009, enmienda 2026-11-12).
- **RF-010** El chat indica su destino (bot o agente) en cada petición; no depende de una «oficina activa» global. Las oficinas de agentes conservan su comportamiento.
- **RF-011** Las oficinas «Camila IA UML» y «Camila IA Conversacional» salen de la configuración de oficinas.
- **RF-012** Los controles de la spec 009 siguen vigentes: modo de webhooks, hosts permitidos, confirmación de escrituras, solo Claude para datos personales, estado en memoria.
- **RF-013** Al cargar un bot se validan sus referencias (páginas, flujos, webhooks, intents) y los problemas se registran sin impedir el resto.

## Criterios de aceptación
- **CA-001** Generar dos veces produce archivos idénticos; `notas.md` editado a mano sobrevive.
- **CA-002** Ningún archivo generado contiene credenciales.
- **CA-003** LectorUML recorre el bot desde su playbook igual que antes (bienvenida, botones, formulario, webhook real con confirmación de escrituras).
- **CA-004** Chatbots muestra la tarjeta del bot Camila; al hacer clic abre el entorno; no se abre solo.
- **CA-005** Conversacionales muestra CamilaIA; su consumo aparece atribuido a `camila-ia` y sus estados en vivo.
- **CA-006** Sin `escribir_servicio` en CamilaIA no escribe aunque el modelo lo pida; con la herramienta pide confirmación.
- **CA-007** Dos entornos abiertos a la vez no se pisan.

## Alcance
**Incluido:** formato y generador de playbooks de bots, cargador multi-bot, LectorUML, CamilaIA como agente, destino explícito del chat, secciones por bot y por agente. **Excluido:** importar bots que no sean de Dialogflow CX, editor visual de playbooks.

## Supuestos
- El export `Bots/<bot>` sigue siendo la fuente; un bot nuevo agrega su carpeta y se genera con el mismo comando.
- Depende de la spec 011 (agentes).

## Preguntas abiertas
Ninguna que bloquee.
