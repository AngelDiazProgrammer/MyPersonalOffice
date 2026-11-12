# Evaluación · Camila como playbook y como agentes propios

**Fecha:** 2026-11-11 · **Pedido:** «evalúa la posibilidad de que el chatbot UML sea un playbook con un agente especializado en ejecutarlo, y que Camila Conversacional sea su propio agente quien hable al ejecutarlo; la lógica de las oficinas virtuales se conserva y Camila tiene un entorno de ejecución aparte».
**Estado:** **implementada** como opción C en las specs [011](../specs/011-agentes/spec.md) y [012](../specs/012-playbooks-bots/spec.md) (2026-11-12): LectorUML es un ejecutor determinista, CamilaIA es un agente definido en `agents/camila-ia.md` y los bots viven en `playbooks bots/`. Queda pendiente la activación de oficina por sesión para las oficinas de agentes.

## Resumen
**Es posible y recomendable, con una condición: el agente del UML no debe ser un modelo de lenguaje.** Camila Conversacional sí debe convertirse en un agente definido en Markdown. Los playbooks por flujo ayudan, pero la **fuente de verdad sigue siendo el export** (si se editan a mano, se pierden al regenerar).

## Cómo está hoy
- **Motor determinista** (`CamilaEngine`) que interpreta el bot: páginas, formularios, rutas, webhooks (con confirmación de escrituras), generadores.
- **UML** = el motor + una vista (`CamilaUmlView`). **Conversacional** = (desde 2026-11-12) un agente libre (`CamilaChatAgent`) que usa la base como conocimiento y llama a los servicios; ya no usa el motor ni sigue flujos.
- **No son agentes de la plataforma:** no aparecen en el registro de agentes, sus prompts están en el código, y su consumo no se atribuye a un agente. Sí tienen entorno propio (sección Chatbots / Agentes conversacionales, sesión en memoria, registro en vivo).
- **Playbooks existentes** (spec 007): Markdown con entradas y pasos **de navegador** (Playwright). No sirven tal cual para un flujo de conversación.

## Opciones evaluadas
| Opción | Qué es | Fidelidad al bot | Esfuerzo | Riesgo |
|---|---|---|---|---|
| **A. Como hoy** | Motor + dos fachadas | Total | — | Los prompts viven en código; Camila no es «agente» de la plataforma |
| **B. Todo con LLM** | Un agente LLM ejecuta playbooks escritos en Markdown | **Baja**: puede saltarse pasos, parafrasear o inventar rutas; las escrituras dependerían del prompt | Medio | **Alto** (webhooks de escritura reales) |
| **C. Híbrido (recomendada)** | Agentes definidos en `agents/*.md`; el motor determinista es su herramienta; playbooks por flujo como unidad de lectura | Total en UML; alta en Conversacional | Medio | Bajo |

## Recomendación: opción C
1. **Agente «Camila · Ejecutor» (UML) — determinista.** Se registra como agente de la plataforma (aparece en el registro, emite estados y eventos, consumo 0 de tokens) pero **ejecuta el motor, no un modelo**. Así el UML sigue siendo «tal cual el JSON» y se conserva lo que más importa: confirmación antes de escribir, lista de hosts, secretos fuera del código.
2. **Agente «Camila» (Conversacional) — LLM definido en `agents/camila-conversacional.md`.** Persona, reglas («nunca inventes datos», «no muestres UML», tono), proveedor y límites salen del Markdown como el resto de agentes. Usa las **herramientas** que ya tiene hoy (`buscar`, `webhook`) como herramientas con permisos explícitos (la confirmación de escrituras la exige el código, no el prompt). Beneficios: consumo por agente, estados en la oficina/registro, cancelación, y cambiar su comportamiento sin recompilar.
3. **Playbooks por flujo** (`playbooks/camila/<flujo>.md`), **generados** desde el export con un encabezado (id, cómo se llega, datos que pide, webhooks y cuáles escriben) y el texto explicativo que ya produce `CamilaIA.md`. Sirven para: cargar solo el flujo relevante (menos tokens), ver diferencias por flujo en git y, opcionalmente, añadir **notas de tono/FAQ** en un bloque aparte que el regenerado no pisa.
4. **Entorno aparte:** ya existe (secciones propias, sesión propia). Falta solo que la **activación de la oficina deje de ser global**: hoy `WorkspaceService.Active` es un único valor del proceso; con varias personas o pestañas se pisarían. Es un requisito para el orquestador multiagente y conviene resolverlo en este mismo paso (estado por sesión).

## Por qué no un LLM para el UML
El UML existe para **mostrar y probar el bot tal cual**. Un agente LLM que «interprete» un playbook cambia el resultado entre ejecuciones, no puede garantizar que ejecute todos los pasos en orden y mezcla en el mismo lugar la decisión de escribir datos reales con una respuesta probabilística. Un ejecutor determinista tiene el mismo aspecto de «agente especializado» para la plataforma sin esos riesgos.

## Riesgos y cómo se mitigan
- **Dos fuentes de verdad** (playbook editado vs. export): los playbooks se generan; lo editable vive en un bloque separado.
- **Costo y latencia** del Conversacional (hoy 2 llamadas por turno): mantener solo Claude por los datos personales; evaluar fusionar interpretar+narrar en una sola llamada con herramientas.
- **Pruebas no deterministas:** el motor conserva sus pruebas; el agente se prueba con un modelo simulado como hoy.
- **Datos personales:** el agente recibe los datos de los webhooks; se mantiene `Sensitive` (solo Claude) y sin registrar cuerpos.

## Esfuerzo estimado (en dos entregas)
1. **Agentes en Markdown** (ejecutor determinista + Camila con herramientas) y activación por sesión: tamaño medio.
2. **Playbooks por flujo generados** + recarga en caliente: tamaño medio-pequeño.

## Lo que necesito de ti para especificarlo (spec 011)
1. ¿Apruebas la opción C (ejecutor del UML **determinista**, no LLM)?
2. ¿Quieres poder **editar** tono/FAQ de Camila en los playbooks (bloque que el regenerado no pisa) o prefieres que todo salga solo del export?
3. ¿La activación por sesión (varias pestañas/personas) entra en el mismo paso o después?
