# Plan técnico 009 · Camila IA (UML y Conversacional)

**Maestro:** `.specify/maestros/04_PLAN.md` · **Estado:** implementado · **Constitución:** v1.1.0

## Resumen y objetivo técnico
Convertir el export de Dialogflow CX (`Bots/`) en `bases de conocimiento/CamilaIA.md` y ofrecer dos oficinas que lo usan: **Camila IA UML** (visor determinista, sin modelo) y **Camila IA Conversacional** (el mismo bot hablado con naturalidad). Ambas comparten un único motor de flujos.

## Referencias
`specs/009-camila-ia/spec.md`; specs 001 (oficinas), 002 (base de conocimiento), 004 (`Sensitive` fuerza Claude), 007 (patrón de flujo de chat con aprobación). Constitución Art. 3, 17, 18, 19, 20, 22.

## Decisiones de arquitectura
- **D-1** Modelo normalizado del bot (`CamilaBot`) en Application; `CamilaIA.md` lleva texto legible más un bloque `json camila:*` por elemento. El Markdown es la fuente de verdad en ejecución y se regenera con `--camila-generar` (determinista, sin fecha).
- **D-2** El conversor (`CamilaBotConverter`) vive en Infrastructure; saca las credenciales a `.env` y deja `{{secret:NOMBRE}}` en el modelo. `Bots/` queda en `.gitignore`; `.env.example` recibe solo los nombres.
- **D-3** Un único motor (`CamilaEngine`) interpreta páginas, formularios, rutas por intent/evento/condición, grupos de rutas, webhooks y generadores con una **cola de acciones**, lo que permite pausar antes de un webhook de escritura y retomar tras el «sí».
- **D-4** `CamilaExpression` evalúa las condiciones y funciones `$sys.func.*` / `$if` que usa el bot (el censo del export: IF, LOWER, ADD, ADD_DATE, COUNT, FORMAT_DATE, GET_FIELD, NOW, TO_NUMBER, TO_OBJECT, TO_TEXT).
- **D-5** Orden del fulfillment igual al original: parámetros → webhook → generadores → mensajes (tres respuestas del bot usan datos devueltos por el webhook).
- **D-6** `CamilaWebhookService`: modo por webhook (Desactivado/Simulado/Real, **Real por defecto** en la primera prueba), lista de hosts permitidos con puerto, solo https, sin redirecciones, secretos desde configuración, cuerpo y mapeo (`$.a[0].b`) como Dialogflow FLEXIBLE. Los registros guardan webhook, modo, resultado y tiempo; nunca cuerpos ni datos.
- **D-7** Escritura = nombre con guardar/envío/finalizar/actualizar/crear/registrar/enviar/certificad (o `ExtraWriteWebhooks`): se pide confirmación con el cuerpo exacto; sin un «sí» claro no se ejecuta.
- **D-8** **UML** (`CamilaUmlView`): ficha de la página (textos, botones, formulario, webhooks con modo, rutas), jerarquía (`/jerarquia`), salto (`/ir`), variables (`/vars`), reinicio. Los botones salen como `KnowledgeChoiceDto` con `MenuOnly` (campo nuevo, compatible con el contrato). No ejecuta generadores de IA.
- **D-9** **Conversacional**: dos llamadas por turno, ambas con `Sensitive` (solo Claude): (1) interpretar el mensaje (opción, dato, intent, flujo o charla) con salida estructurada, usando un **índice por flujo** para no cargar la base completa; (2) narrar el resultado del motor sin mostrar nada técnico. Los generadores de IA los ejecuta el modelo.
- **D-10** Estado por conversación en `CamilaSessionStore` (memoria, caducidad configurable); los parámetros nunca se escriben en disco. El texto de la persona no entra a la bitácora de actividad en estas oficinas.
- **D-12** Webhooks estándar de Dialogflow (sin `webhookType`): se envía el `WebhookRequest` (etiqueta, página, parámetros de sesión, texto); un POST sin cuerpo envía contenido vacío (evita HTTP 415, detectado en la prueba real).
- **D-13** Toda llamada al modelo de la Conversacional tiene tope de espera (`ModelTimeoutSeconds`); `IdentifiedParameter` impide saltar a flujos que requieren identificación (detectado en la prueba real: el salto directo dejaba la conversación sin respuesta).
- **D-11** `WorkspaceDefinition.Kind` distingue las oficinas de Camila; `OrchestrationService` las deriva a `CamilaWorkflow` antes del planificador (dependencia opcional, las pruebas existentes no cambian).

## Datos e interfaces
Sección `Camila` de `configuration/appsettings.json` (`DefaultWebhookMode`, `WebhookModes`, `AllowedHosts`, `ConfirmWrites`, `ExtraWriteWebhooks`, `WebhookTimeoutSeconds`, `MaxAutoSteps`, `SessionMinutes`); secretos `CAMILA_WH_*` en `.env`.

## Riesgos y mitigaciones
- Escrituras reales: confirmación obligatoria (D-7). Hosts: solo lista permitida (D-6). Credenciales: fuera del Markdown y de git (D-2); se recomienda rotarlas.
- Webhooks con autenticación de cuenta de servicio de Google (`Obtener_Usuario_Por_Cedula`, `Extraer información candidato de W`): no se reproduce; solo se envían sus cabeceras. Puede fallar con HTTP 401/403.
- Sin conexión a la red corporativa/VPN el modo Real falla con un mensaje comprensible.

## Pruebas
`tests/AI.Agent.Office.Tests/CamilaTests.cs`: expresiones, webhooks (cuerpo, secreto, lista de hosts, modos, errores sin cuerpo), motor (botones, formulario, auto-rutas, bucles, no-match, confirmación), conversión (credenciales fuera del Markdown, determinismo, ida y vuelta), `.env` sin sobrescribir y las dos oficinas con un modelo simulado.

## Enmienda 2026-11-12
- **D-9 (sustituida)** La Conversacional deja de usar `CamilaEngine`: `CamilaChatAgent` es un bucle de hasta 6 pasos (`responder` | `buscar` | `webhook`) sobre `CamilaKnowledgeText` (catálogo + búsqueda). El código impone hosts, secretos, datos faltantes, confirmación de escrituras, tope de búsquedas (2) y `Sensitive`. El motor solo atiende al UML.
- Se retira `Camila:IdentifiedParameter` (era una exigencia del recorrido por flujos).
