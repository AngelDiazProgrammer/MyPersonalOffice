# Especificación 007 · Conectividad a navegadores (Playwright)

**Estado:** Aprobada por la persona responsable en chat (2026-11-10), con las decisiones de la sección «Decisiones».
**Constitución:** v1.1.0 · **Maestro:** `.specify/maestros/02_SPECIFY.md` · **Fecha:** 2026-11-10

## Resumen
Playwright se integra como **servicio de back** (no como pruebas) para que la oficina opere sitios web en nombre de la persona. Cada sitio se configura desde el front (URL, dominios permitidos), tiene un **playbook** `.md` con sus entradas y pasos, y se ejecuta automáticamente al recibir la orden. Caso guía: Gmail — tras resolver un caso de la base de conocimiento, el modelo pregunta para qué y para quién es el correo, redacta el cuerpo, lo muestra para aprobación y, al aprobar, lo envía por el navegador.

## Problema
Hoy la oficina solo produce texto y documentos; la persona debe copiar y operar a mano en cada sitio. Se quiere automatizar esa operación sin APIs de terceros, con control humano sobre lo irreversible.

## Objetivo y valor
Delegar tareas web repetitivas con seguridad, y que el sistema **aprenda** los procesos para ejecutarlos cada vez más rápido (sin modelo).

## Usuarios y actores
Persona usuaria (configura sitios, responde preguntas, aprueba); Orquestador/flujo de navegador (pregunta, redacta, ejecuta); Playwright (opera el navegador).

## Historias de usuario
- HU-001: Como persona usuaria, quiero agregar y quitar sitios desde el front para decidir a qué sitios puede conectarse la oficina.
- HU-002: Como persona usuaria, quiero que cada sitio tenga un playbook `.md` editable con sus pasos.
- HU-003: Como persona usuaria, quiero iniciar sesión yo mismo (manualmente) una sola vez y que la oficina reutilice esa sesión.
- HU-004: Como persona usuaria, quiero que el modelo me pregunte lo necesario, redacte el contenido y me lo muestre para aprobarlo antes de enviarlo.
- HU-005: Como persona usuaria, quiero que el sistema aprenda un proceso nuevo (sin pasos estructurados) y lo repita más rápido, previa mi aprobación.
- HU-006: Como persona usuaria, quiero que si la web cambia el sistema se repare solo y me proponga actualizar el playbook.

## Requisitos funcionales
- **RF-001** CRUD de sitios desde el front y la API: id, nombre, URL inicial, dominios permitidos y modo del navegador (visible/invisible). Se guardan en `browser-data/sites.json`.
- **RF-002** Cada sitio tiene un playbook `playbooks/<id>.md`: cabecera (disparadores), `## Entradas`, `## Pasos`. Se lee, edita y valida desde la API/front.
- **RF-003** Entradas: preguntadas a la persona (una por una) o `redactar` por el modelo a partir del contexto de la conversación.
- **RF-004** Pasos estructurados (`ir`, `clic`, `escribir`, `teclear`, `esperar`, `seleccionar`, `verificar`) o texto libre (paso exploratorio que decide el modelo mirando la página).
- **RF-005** **Aprobación antes de ejecutar**: se muestra el borrador completo y solo se ejecuta tras aprobar (botón o mensaje). Editar es posible en lenguaje natural; cancelar descarta.
- **RF-006** **Login manual**: una acción abre el navegador visible con el perfil persistente del sitio; la persona inicia sesión y la sesión se reutiliza en las ejecuciones. Las credenciales opcionales se escriben en el front en cada ejecución, viven solo en memoria del proceso durante la ejecución y nunca se guardan, registran ni envían a un modelo.
- **RF-007** El navegador solo puede navegar a los dominios permitidos del sitio; cualquier otra navegación se bloquea y aborta la ejecución.
- **RF-008** **Aprendizaje**: cada ejecución registra las acciones realmente hechas. Si hubo exploración o reparación, se genera una **propuesta** de playbook (`playbooks/<id>.propuesta.md`) con pasos estructurados; solo reemplaza al playbook cuando la persona la aprueba.
- **RF-009** **Autorreparación**: si un paso estructurado falla, el modelo explora solo ese tramo (por la descripción del paso) y la ejecución continúa; se propone actualizar el playbook (RF-008).
- **RF-010** Cada ejecución informa: resultado, duración, pasos, modo (`reproducción` / `exploración` / `reparación`) y llamadas al modelo.
- **RF-011** El flujo se integra al chat: un disparador del playbook inicia las preguntas; las respuestas, la redacción, la aprobación y el resultado ocurren en la misma conversación.
- **RF-012** Si Playwright o el navegador no están instalados, se informa con el comando de instalación (sin detalles internos).

- **RF-013** *(2026-11-10)* Acción `extraer` (selector de filas, cantidad) para **leer** datos de la página. Un playbook de solo lectura (sin entradas a redactar ni pasos irreversibles) se ejecuta al pedirlo, **sin aprobación**, y devuelve una tabla. Caso: leer los últimos 20 correos de la bandeja.
- **RF-014** *(2026-11-10, modificado el mismo día)* El formulario del sitio pide, junto a la URL, usuario y clave opcionales. Se guardan en `browser-data/credentials/<id>.md` (**texto plano, solo entorno de pruebas**, por decisión de la persona responsable) para que sobrevivan al reinicio, y se usan como `{{credencial.usuario}}` / `{{credencial.clave}}` si la ejecución no trae otras. Nunca se devuelven por la API ni van a registros, eventos ni modelos; la carpeta está en `.gitignore`.
- **RF-016** *(2026-11-10)* Todos los campos de una conexión se pueden editar desde «Conexiones» (nombre, URL, dominios, navegador visible/invisible, activa/inactiva, usuario y clave; la clave vacía conserva la actual). El identificador es fijo porque nombra el playbook, el perfil del navegador y el archivo de credenciales.
- **RF-015** *(2026-11-10)* El contenido leído (por ejemplo los correos) se muestra a la persona pero **no se guarda en el historial** de la conversación que reciben los modelos.

## Requisitos no funcionales
- **RNF-001** Seguridad (Art. 11, 12): URL solo http/https; hosts privados/locales bloqueados salvo `Browser:AllowPrivateHosts`; sin credenciales en registros, eventos ni prompts; lo que lee la página y se envía a un modelo se marca `Sensitive` (solo Claude).
- **RNF-002** Límites (Art. 10): máximo de pasos, de acciones exploratorias, de tiempo por ejecución y de ejecuciones simultáneas.
- **RNF-003** Estado pendiente del chat en memoria con tope y caducidad.
- **RNF-004** Capas intactas (spec 005): Playwright vive en Infrastructure tras interfaces de Application.

## Reglas de negocio
- **RN-001** Ninguna acción marcada `irreversible` (enviar, pagar, borrar) se ejecuta sin aprobación explícita previa de esa ejecución.
- **RN-002** Un playbook aprendido nunca se activa sin aprobación (Art. 29).
- **RN-003** Nunca se aprenden credenciales ni contenido de la persona: los valores se registran como `{{variable}}`.
- **RN-004** El modelo propone; la persona aprueba (Art. 29).

## Criterios de aceptación
- **CA-001** Alta, edición y baja de un sitio persiste y valida URL/dominios (RF-001, RNF-001).
- **CA-002** El playbook se parsea y valida; errores se informan con línea (RF-002, RF-004).
- **CA-003** El flujo pregunta, redacta, muestra el borrador y espera aprobación; cancelar no ejecuta (RF-003, RF-005, RN-001).
- **CA-004** Con aprobación, se ejecutan los pasos en orden con las entradas sustituidas (RF-004, RF-010).
- **CA-005** Una navegación fuera de los dominios permitidos se bloquea (RF-007).
- **CA-006** Un paso exploratorio genera pasos estructurados en la propuesta y no registra credenciales (RF-008, RN-003).
- **CA-007** Un paso que falla se repara por exploración y la ejecución termina; se propone el playbook actualizado (RF-009).
- **CA-008** Aprobar la propuesta reemplaza los pasos y la siguiente ejecución no llama al modelo (RF-008, RF-010).
- **CA-009** Las credenciales no aparecen en registros, resultados ni en lo enviado al modelo (RF-006).
- **CA-011** `extraer` lee como máximo la cantidad pedida, falla con mensaje claro si no hay filas y un playbook de solo lectura no pide aprobación (RF-013).
- **CA-012** Las credenciales del sitio se guardan en su `.md`, sobreviven al reinicio, se borran al quitar el sitio o a petición y no aparecen en registros, resultados ni respuestas de la API (RF-014).
- **CA-014** Editar una conexión actualiza todos sus campos, conserva la clave si se deja vacía y vuelve a validar URL y dominios (RF-016).
- **CA-013** El contenido leído no queda en el historial de la conversación (RF-015).
- **CA-010** Con un navegador real y una página local, el playbook completo se ejecuta de extremo a extremo (RF-004).

## Alcance
**Incluido:** servicio Playwright, sitios, playbooks, flujo de chat con aprobación, aprendizaje, autorreparación, login manual, front «Conexiones», pruebas.
**Excluido:** almacenamiento de credenciales (decisión), uso de APIs de los sitios (decisión), CAPTCHA y 2FA automáticos (se resuelven en el login manual), autenticación de la app, ejecución programada, múltiples usuarios.

## Decisiones de la persona responsable
1. Credenciales: se escriben en el front (alta/edición del sitio o cada ejecución) y se **guardan en un `.md` por conexión, en texto plano, solo para pruebas** (ajuste del 2026-11-10: antes no se guardaban). **No apto para producción**: antes de salir de pruebas hay que cifrarlas o moverlas a un gestor de secretos (Art. 11).
2. Gmail y similares: **login manual**, sin APIs.
3. Navegador **invisible cuando sea estable**; visible por defecto (configurable por sitio y global).
4. Alcance: aprendizaje y autorreparación incluidos desde el inicio.

## Propuesta de enmienda (pendiente de aprobación; no aplicada)
Anexar a la constitución un artículo sobre «acciones con efectos externos»: aprobación humana previa, dominios permitidos y manejo de credenciales de terceros. Se propone, no se aplica (Art. 40).

## Datos e interfaces
REST `/api/browser/*`; contrato de chat ampliado con `BrowserApproval` (aditivo). Datos: `browser-data/sites.json`, `browser-data/profiles/<id>/`, `playbooks/*.md`.
