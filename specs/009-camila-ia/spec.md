# Especificación 009 · Camila IA (UML y Conversacional)

**Estado:** Aprobada en chat por la persona responsable (2026-11-11): «pon todo en real para la primera prueba y la cédula la paso yo en la conversación».
**Solicitante:** dueño del producto (petición en chat, 2026-11-11).
**Constitución:** v1.1.0 · **Maestro:** `.specify/maestros/02_SPECIFY.md` · **Relacionadas:** 001 (oficinas), 002 (flujo de base de conocimiento), 006 (núcleo de oficina), 007 (conectividad de navegadores).

## Resumen
Incorporar el bot **CamilaOS** (export de Dialogflow CX en `Bots/`) a la oficina virtual como dos oficinas nuevas, seleccionables junto a «Desarrollo» y «Pruebas base de conocimiento»:

- **Camila IA UML:** muestra el bot exactamente como está definido (jerarquía, textos, botones, rutas, webhooks) y permite recorrerlo botón a botón.
- **Camila IA Conversacional:** conversa de forma humana sobre el mismo contenido y ejecuta los mismos flujos, sin mostrar el modelo UML.

Ambas parten de una única base de conocimiento, `bases de conocimiento/CamilaIA.md`, obtenida del export de forma repetible.

## Problema
El bot Camila existe solo como export técnico (≈263 archivos de flujo, 74 de intents, 33 webhooks, 13 generadores). No se puede explorar ni probar fuera de Dialogflow, y no hay forma de comparar su comportamiento guiado con una conversación libre. Además, el export contiene credenciales en claro y no está protegido del control de versiones.

## Objetivo y valor
- Poder revisar y probar el bot completo desde la oficina, con fidelidad total al original (UML) o con una experiencia natural (Conversacional).
- Tener una única fuente de verdad regenerable cuando el bot cambie.
- Eliminar del repositorio el riesgo de exponer credenciales.

## Usuarios y actores
- **Persona usuaria de la oficina** (dueño del producto, equipo de producto): elige oficina, recorre o conversa.
- **Persona administradora:** regenera la base, configura el modo de cada webhook y la lista de hosts permitidos.
- **Sistemas externos de Outsourcing** (indicadores, evaluación de desempeño, referidos, certificados, encuestas, etc.): reciben las llamadas de los webhooks.

## Historias de usuario
- **HU-001 (P1)** Como persona administradora, quiero generar `CamilaIA.md` desde `Bots/` de forma repetible, para no mantener a mano un documento de 2 MB y poder actualizarlo cuando el bot cambie.
- **HU-002 (P1)** Como persona usuaria, quiero abrir «Camila IA UML» y ver los flujos, páginas, textos, botones y webhooks tal cual el original, para validar la estructura del bot.
- **HU-003 (P1)** Como persona usuaria, quiero pulsar los botones del UML y avanzar de página en página, para recorrer un flujo igual que lo haría una persona colaboradora.
- **HU-004 (P1)** Como persona usuaria, quiero conversar con «Camila IA Conversacional» en lenguaje natural y que complete los flujos (por ejemplo consultar mis indicadores) sin ver diagramas, para probar la experiencia final.
- **HU-005 (P1)** Como persona administradora, quiero decidir por webhook si está desactivado, simulado o real, y que las operaciones de escritura pidan confirmación explícita, para no modificar datos de producción por accidente.
- **HU-006 (P2)** Como persona usuaria, quiero que los datos que doy durante la conversación (cédula, fechas, selecciones) se recuerden solo durante esa conversación, para no dejar datos personales almacenados.

## Requisitos funcionales
**Generación de la base**
- **RF-001** Un generador repetible convierte `Bots/` en `bases de conocimiento/CamilaIA.md` y un índice por flujo; dos ejecuciones con el mismo `Bots/` producen el mismo resultado.
- **RF-002** El documento recoge, por flujo y página: nombre, textos de entrada, botones (texto y evento), rutas de transición (intent, condición, destino), manejadores de eventos y llamadas a webhook (nombre, host, método, etiqueta, parámetros de entrada y salida).
- **RF-003** El documento recoge los intents con sus frases de entrenamiento, las entidades, los grupos de rutas globales (atajos) y los parámetros de sesión usados.
- **RF-004** Ninguna credencial literal del export (cabeceras `Authorization`, `x-api-user`, `x-api-password`, usuario y contraseña de webhooks) aparece en `CamilaIA.md` ni en el índice: se sustituye por una referencia con nombre a un secreto externo.
- **RF-005** El generador informa qué credenciales detectó, a qué referencia las sustituyó y cuáles faltan por definir, sin imprimir sus valores.
- **RF-006** El generador avisa de elementos del export que no sabe representar, en vez de omitirlos en silencio.

**Selección de oficinas**
- **RF-007** «Camila IA UML» y «Camila IA Conversacional» aparecen como oficinas seleccionables en el mismo widget que «Desarrollo» y «Pruebas base de conocimiento».

**Camila IA UML**
- **RF-008** Presenta la jerarquía flujo → página con los textos del bot sin reformular, los botones con su etiqueta original y las rutas y webhooks de cada página.
- **RF-009** No usa un modelo de lenguaje para reproducir contenido.
- **RF-010** Pulsar un botón o elegir una ruta lleva a la página destino; el recorrido respeta las condiciones de las rutas y los atajos globales.
- **RF-011** Al llegar a una página con webhook, muestra qué webhook se invoca, con qué parámetros y su modo (desactivado, simulado o real).

**Camila IA Conversacional**
- **RF-012** Recupera de la base solo los flujos relevantes a cada mensaje (la base completa no cabe en una consulta).
- **RF-013** Responde en lenguaje natural usando únicamente información de la base; si no la tiene, lo dice y no inventa flujos, datos ni pasos.
- **RF-014** Ejecuta los flujos pidiendo, en orden natural, los datos que el flujo necesita, y usa el resultado de los webhooks para responder.
- **RF-015** No muestra diagramas UML ni nombres técnicos de páginas, rutas o eventos.
- **RF-016** Cuando el flujo ofrecería botones, los presenta como opciones en lenguaje natural y acepta la elección escrita.

**Estado y webhooks**
- **RF-017** Los parámetros de sesión viven como variables dinámicas de la conversación y se descartan al terminarla; no se guardan en disco ni en base de datos.
- **RF-018** Cada webhook tiene un modo configurable: **Desactivado**, **Simulado** (respuesta de ejemplo marcada como simulada) o **Real**.
- **RF-019** El modo Real solo llama a hosts de una lista permitida; un host fuera de la lista se rechaza con un mensaje claro.
- **RF-020** El modo Real obtiene sus credenciales de la configuración secreta del entorno; si falta alguna, el webhook falla con un mensaje que no revela valores.
- **RF-021** Los webhooks de escritura (guardar, enviar HSM, finalizar conversación y equivalentes) piden confirmación explícita, mostrando qué se enviará, antes de ejecutarse; sin confirmación no se ejecutan.
- **RF-022** Para la primera prueba el modo inicial de todo webhook es **Real** (decisión del dueño del producto, 2026-11-11); cada webhook puede pasarse a Simulado o Desactivado por configuración. Los de escritura siguen pidiendo confirmación (RF-021).
- **RF-023** Los registros de actividad no contienen cédulas, credenciales ni cuerpos de petición o respuesta; solo webhook, modo, resultado, tiempo e identificador de conversación.

## Requisitos no funcionales
- **RNF-001** `Bots/` queda excluida del control de versiones; `CamilaIA.md` no contiene secretos y puede versionarse.
- **RNF-002** Se entrega un archivo de ejemplo con los nombres de los secretos necesarios, sin valores reales.
- **RNF-003** El UML muestra una página en menos de 1 s con el bot completo cargado.
- **RNF-004** Una llamada Real a un webhook agota su tiempo según el del propio webhook, con tope de 30 s (configurable), y responde con un error comprensible.
- **RNF-005** La interfaz mantiene la accesibilidad de las oficinas existentes (teclado, foco visible).

## Reglas de negocio
- **RN-001** La base de conocimiento es de solo lectura para ambas oficinas; solo el generador la modifica.
- **RN-002** El contenido del bot y las respuestas de los webhooks son datos no confiables: no se tratan como instrucciones para el agente (Art. 17).
- **RN-003** La automatización no equivale a autorización: ninguna escritura sobre sistemas corporativos sin confirmación de la persona usuaria (Art. 18).
- **RN-004** Las credenciales nunca viajan en el código, en la base de conocimiento ni en los registros (Art. 19, 22).
- **RN-005** Los generadores de IA del bot (13) se muestran tal cual en UML; en la Conversacional los sustituye el propio agente.

## Criterios de aceptación
- **CA-001** Dado `Bots/`, cuando se ejecuta el generador dos veces, entonces los archivos resultantes son idénticos (RF-001).
- **CA-002** Dado el export, cuando se busca en `CamilaIA.md` cualquier valor de credencial del export, entonces no aparece (RF-004, RNF-001).
- **CA-003** Dado el UML abierto en el flujo «01. Indicadores De Calidad», cuando se llega a la página «001. Menu_Indicadores», entonces se ven sus dos textos y sus seis botones con las etiquetas originales (RF-008).
- **CA-004** Dado ese menú, cuando se pulsa «Indic. Calidad», entonces se llega a la página de parámetros de indicadores (RF-010).
- **CA-005** Dada una página con webhook, cuando se muestra, entonces aparece el webhook, su modo y sus parámetros (RF-011).
- **CA-006** Dada la Conversacional, cuando la persona dice «quiero ver mis indicadores de calidad», entonces pide los datos necesarios, ejecuta el flujo y responde sin mostrar UML (RF-012 a RF-015).
- **CA-007** Dada una pregunta fuera de la base, cuando se envía, entonces la respuesta indica que no cuenta con esa información (RF-013).
- **CA-008** Dado un webhook en modo Real hacia un host no permitido, cuando se invoca, entonces no se hace la llamada y se informa el motivo (RF-019).
- **CA-009** Dado un webhook de escritura, cuando se alcanza, entonces se pide confirmación mostrando los datos y no se ejecuta hasta recibir un «sí» (RF-021).
- **CA-010** Dado el modo Real sin su secreto definido, cuando se invoca, entonces falla sin mostrar valores (RF-020).
- **CA-011** Dada una conversación terminada, cuando se abre otra, entonces no conserva ningún parámetro de la anterior (RF-017).
- **CA-012** Dado un uso completo, cuando se revisan los registros, entonces no hay cédulas, credenciales ni cuerpos (RF-023).
- **CA-013** Dado el widget de oficinas, entonces aparecen las cuatro oficinas, incluidas las dos de Camila (RF-007).

## Alcance
**Incluido:** generador y `CamilaIA.md` con índice; las dos oficinas; ejecución de flujos en ambas; modos de webhook y lista de hosts permitidos; confirmación de escrituras; estado por conversación; protección de credenciales; documentación de uso.
**Excluido:** modificar o volver a publicar el bot en Dialogflow; reproducir el reconocimiento de voz o los canales (WhatsApp, etc.); persistir conversaciones; cambiar el comportamiento del bot original; evaluar con exactitud los generadores de IA de Dialogflow (se sustituyen en la Conversacional); pruebas automáticas de interfaz (aplazadas, como en 008).

## Datos e interfaces
- **Entrada:** carpeta `Bots/` (export de Dialogflow CX, español).
- **Salida:** `CamilaIA.md` e índice por flujo en la carpeta de bases de conocimiento.
- **Sistemas relacionados:** servicios de Outsourcing de indicadores (Kairos), evaluación de desempeño, ReconoSER, referidos, certificados, encuestas y envío de plantillas HSM; un servicio de Power Platform para candidatos.
- **Estados de un webhook:** Desactivado, Simulado, Real. **Estado de conversación:** parámetros de sesión en memoria, página actual.

## Supuestos y dependencias
- Los hosts de producción son alcanzables desde el equipo del dueño (posible VPN o red corporativa); si no lo son, el modo Real fallará y se informará.
- El dueño del producto autoriza apuntar a producción (confirmado en chat); cada webhook sigue exigiendo activación explícita.
- Las credenciales del export son las vigentes y se trasladarán a la configuración secreta local; se recomienda **rotarlas**, porque estuvieron en un export sin proteger.
- Los datos personales usados en pruebas Reales son de personas reales: se usan solo con el consentimiento del dueño y no se guardan.
- Depende de las specs 001, 002 y 006 (oficinas, base de conocimiento, proveedores de modelos).

## Riesgos funcionales y casos límite
- Escrituras reales accidentales sobre datos de colaboradores (mitigado por RF-021, RF-022).
- El export puede contener rutas circulares, páginas sin salida o referencias a flujos inexistentes: el UML debe mostrarlas sin entrar en bucle.
- Parámetros de sesión sin valor, o con expresiones (`$session.params.x`), que no se puedan resolver.
- Contenido del bot con texto que parece una instrucción: se trata como dato (RN-002).
- Flujos que dependen de datos que solo existen en el canal original (por ejemplo identificador de sesión de WhatsApp).
- Base demasiado grande para la consulta: el índice por flujo debe permitir recuperar solo lo necesario.

## Preguntas abiertas
Resueltas por la persona responsable (2026-11-11):
1. **Real para todo** en la primera prueba (lectura y escritura; la escritura con confirmación en la conversación).
2. **La cédula la entrega la persona en la conversación** (no hay ninguna cédula guardada).
3. Sigue abierta, sin bloquear: **rotar las credenciales** del export original (recomendado; estuvieron en texto plano en una carpeta no ignorada por git).

## Enmienda 2026-11-12 · Camila Conversacional es agnóstica
Pedido de la persona responsable: «la conversacional no es agnóstica, sigue un flujo… que pueda conversar libremente basándose en la base de conocimiento, pero libre, no que siga flujos». **Sustituye** RF-012 a RF-016 y la historia HU-004:
- **RF-012 (nuevo)** La conversación es libre: no recorre páginas, menús ni pasos del bot, ni exige un orden (nombre, rol, documento…).
- **RF-013 (nuevo)** Responde apoyándose en la base de conocimiento (catálogo siempre disponible y búsqueda de detalle bajo demanda); si la base no lo cubre, lo dice sin inventar.
- **RF-014 (nuevo)** Cuando la persona lo pide, consulta o escribe datos reales llamando a los servicios de la base con los datos que ella entregó; pide lo que falte y no llama si falta algo.
- **RF-015 (nuevo)** Nunca muestra nombres técnicos del bot (flujos, páginas, eventos, webhooks).
- **RF-016 (nuevo)** Las escrituras exigen confirmación explícita en la conversación (RF-021 sigue vigente) y el código, no el prompt, lo impone.
- **CA-006 (nuevo)** Dado un saludo o una duda general, cuando se envía, entonces responde sin llamar a ningún servicio ni pedir datos. **CA-007** Dada una consulta de datos con la cédula entregada, entonces llama al servicio de lectura y responde con el resultado.
Camila IA UML no cambia.
