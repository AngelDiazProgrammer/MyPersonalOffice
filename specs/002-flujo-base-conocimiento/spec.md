# Especificación 002 · Flujo de la base de conocimiento (respuesta guiada con entregable APA)

**Estado:** Implementada — especificación **retroactiva** (Art. 28 y 30), reunida a partir de varias iteraciones sucesivas.
**Constitución:** v1.1.0 · **Maestro:** `.specify/maestros/02_SPECIFY.md` · **Fecha:** 2026-11-10

## Resumen
**Flujo de la base de conocimiento:** toda consulta (mensaje, PDF adjunto o ambos) se contrasta primero con una base de conocimiento de incidentes de seguridad; si coincide, agentes exclusivos por proceso identifican el caso, ofrecen soluciones, redactan la elegida y entregan un documento Word y PDF en el formato de entrega (APA).

## Problema
Las respuestas de los modelos a incidentes de seguridad eran genéricas e inventaban datos (tickets, plazos). La organización necesita respuestas fieles a su procedimiento interno, con alternativas y en un formato documental uniforme.

## Objetivo y valor
Respuestas acertadas y verificables según la base de conocimiento, con la persona decidiendo cómo resolver el caso y recibiendo un entregable listo (Word y PDF).

## Usuarios y actores
- **Persona usuaria** (empleado que reporta o analista que revisa un caso): envía el caso, elige o propone la solución, descarga el entregable.
- **Orquestador y agentes** de la oficina «Pruebas base de conocimiento».
- **Responsable de contenido:** mantiene la base y el formato de entrega (archivos).

## Historias de usuario
- HU-001: Como persona usuaria, quiero que mi caso se busque primero en la base, para recibir el procedimiento oficial.
- HU-002: Como persona usuaria, quiero adjuntar un PDF con el caso, para no copiar su texto.
- HU-003: Como persona usuaria, quiero ver varias soluciones y elegir una o escribir la mía, para decidir cómo resolverlo.
- HU-004: Como persona usuaria, quiero un documento Word y PDF en formato APA, para entregarlo o archivarlo.
- HU-005: Como responsable de contenido, quiero actualizar la base y el formato soltando archivos, para no programar.

## Requisitos funcionales
- **RF-001** La base de conocimiento se lee de archivos Markdown; cada situación tiene código, título, severidad, palabras clave y procedimiento (situaciones SEG-001 a SEG-005, ficticias).
- **RF-002** Cada mensaje y el texto de los documentos adjuntos se contrastan primero con la base; si hay coincidencia se ejecuta el flujo fijo, si no, la planificación normal.
- **RF-003** Cada proceso lo hace un agente exclusivo: Lector PDF (solo con adjuntos), Investigador KB, Respondedor de incidentes, Redactor Word (formato) y Auditor de cumplimiento.
- **RF-004** Con documentos adjuntos, el Lector PDF analiza el documento y entrega el resumen del caso al Investigador; solo el Lector recibe el texto del documento.
- **RF-005** El formato de entrega activo es el archivo más reciente (.md, .txt, .docx o .pdf) de la carpeta de formatos.
- **RF-006** El sistema verifica que los tickets, formularios, contactos y plazos del texto final existan en la base; ante datos sin respaldo solicita una única corrección al auditor y, si persisten, advierte en la respuesta.
- **RF-007** Cada situación ofrece 5 soluciones alternativas (una recomendada). Tras identificar el caso, el sistema presenta las opciones y espera la decisión sin generar entregable.
- **RF-008** La persona puede elegir una solución con un botón, por letra o como «la recomendada»; escribir su propia solución (que el auditor contrasta con la base); o cancelar. Un mensaje sobre otra situación o con otro PDF inicia un caso nuevo; una respuesta ambigua se repregunta.
- **RF-009** El entregable final se genera en Word y PDF; con un formato que menciona APA se aplica el estilo APA 7. El título siempre es «Respuesta al incidente {CÓDIGO}: {título}».
- **RF-010** El texto de documentos adjuntos solo se envía a proveedores gratuitos si la configuración lo permite; las situaciones confidenciales solo se envían a Claude.
- **RF-011** Las respuestas del chat pueden exportarse a Word o PDF con botones.
- **RF-012** Una corrección del auditor cortada por límite de tokens nunca reemplaza una respuesta completa.
- **RF-014** Si el Lector PDF falla, la tarea se reintenta una vez con Claude antes de interrumpir el flujo; el reintento lleva el documento solo a Claude (Q-002, decisión 2026-11-10: variante A). Si Claude no está disponible (modo solo gratis o sin clave), la falla se informa como hoy.
- **RF-013** Es configurable: ofrecer o no la elección de soluciones, el estilo del documento, el uso de proveedores gratuitos con documentos y el reintento del Lector PDF con Claude.

## Requisitos no funcionales
- **RNF-001** Los resultados de modelos se consideran no confiables hasta verificarse (Art. 16).
- **RNF-002** Una solicitud del flujo completo debe poder cancelarse por la persona y respetar tiempos máximos por tarea.
- **RNF-003** Los casos pendientes de elección se conservan en memoria con límite de cantidad y vencimiento.

## Reglas de negocio
- **RN-001** La base de conocimiento es el primer filtro de toda consulta.
- **RN-002** Los agentes no inventan tickets, formularios, contactos ni plazos: solo los de la base (y los que escribe la persona en su propia solución).
- **RN-003** Las opciones mostradas a la persona las compone el sistema desde la base, no un modelo.
- **RN-004** El Redactor Word actúa también como editor de formato y no genera su propio Word cuando el flujo produce el entregable final.

## Criterios de aceptación
- **CA-001** Dado un mensaje con la descripción de un phishing, cuando se envía, entonces se identifica SEG-003 y se presentan 5 opciones sin generar documentos (RF-002, RF-007).
- **CA-002** Dado un PDF adjunto con un caso, cuando se envía, entonces el Lector PDF trabaja primero y el Investigador recibe su resumen, no el documento (RF-004).
- **CA-003** Dado un caso pendiente, cuando se elige «B», entonces solo se redacta la solución B y se generan un .docx y un .pdf con título canónico (RF-008, RF-009).
- **CA-004** Dado un caso pendiente, cuando se escribe una solución propia, entonces el auditor la contrasta con la base y los datos escritos por la persona no se marcan como inventados (RF-008, RN-002).
- **CA-005** Dado un texto final con un ticket que no existe en la base, cuando se verifica, entonces se pide una corrección o se advierte (RF-006).
- **CA-006** Dada la elección sin caso pendiente, cuando se envía, entonces se responde sin llamar a ningún modelo (RF-008).
- **CA-007** Dada la opción `OfferSolutionChoice=false`, cuando se envía un caso, entonces se entrega directamente el procedimiento recomendado (RF-013).
- **CA-008** Dado un formato de entrega que menciona APA, cuando se genera el entregable, entonces el Word tiene Times New Roman 12, doble espacio, portada y referencias en página aparte (RF-009).
- **CA-009** Dado `AllowFreeApisWithDocuments=false`, cuando hay un PDF, entonces las tareas con su contenido se envían solo a Claude (RF-010).
- **CA-011** Dado un PDF cuyo análisis falla con los proveedores gratuitos, cuando el Lector PDF reintenta con Claude, entonces el flujo continúa con el resumen obtenido y el Investigador no recibe el documento (RF-014).
- **CA-012** Dado que el reintento con Claude también falla (o Claude no está disponible), cuando ocurre, entonces el flujo se interrumpe con el mensaje habitual y la tarea queda como fallida (RF-014).
- **CA-010** Dada una corrección del auditor truncada, cuando se compone la respuesta, entonces se conserva el texto completo original (RF-012).

## Alcance
**Incluido:** base en Markdown, flujo fijo, soluciones y elección, verificación de datos, entregable Word/PDF APA, exportación desde el chat, banderas de configuración.
**Excluido:** seguimiento posterior del caso con la persona, bandejas de correo/aplicaciones como fuente de casos, edición de la base desde la interfaz, persistencia de casos entre reinicios.

## Datos e interfaces
Entradas: mensaje, PDF, id de conversación, opción elegida o texto propio. Salidas: respuesta con fuente y verificación, opciones, documentos descargables. Estados: caso sin pendiente / pendiente de elección. Sistemas: modelos de lenguaje (gratuitos y Claude), archivos de la base y del formato.

## Supuestos y dependencias
- Base y formato son ficticios mientras dure la prueba.
- Depende de la oficina (spec 001) y de los proveedores (spec 004).

## Riesgos funcionales y casos límite
- Búsqueda por palabras clave: un caso descrito con otras palabras puede no coincidir.
- Una situación sin 2 o más soluciones entrega directamente el procedimiento.
- Si falla el Lector PDF, el flujo se detiene (el Investigador depende de su resumen).

## Preguntas abiertas
- **Q-001** (RESUELTA 2026-11-10) Se mantiene `AllowFreeApisWithDocuments=true` mientras el proyecto esté en pruebas con datos ficticios. Condición: pasar a `false` antes de usar datos reales (Art. 20).
- **Q-002** (RESUELTA 2026-11-10) Variante A: reintentar con Claude una vez; el Investigador nunca recibe el documento directamente (no se adopta la variante B).
