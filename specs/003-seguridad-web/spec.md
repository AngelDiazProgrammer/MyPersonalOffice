# Especificación 003 · Endurecimiento de seguridad de la aplicación web y la API

**Estado:** Implementada — especificación **retroactiva** (Art. 28 y 30), derivada de la revisión de QA/seguridad del 2026-11-06.
**Constitución:** v1.1.0 (Art. 3, 17, 19, 21, 22, 24, 35) · **Maestro:** `.specify/maestros/02_SPECIFY.md` · **Fecha:** 2026-11-10

## Resumen
**Endurecimiento de seguridad:** cierra los riesgos encontrados en la revisión: peticiones desde otros sitios, salida insegura de modelos, inyección desde documentos, cabeceras, abuso de recursos y dependencias vulnerables.

## Problema
La revisión demostró que una página ajena abierta en el mismo navegador podía conectarse al canal en tiempo real y leer la actividad de la oficina, y forzar cambios de estado; las respuestas de modelos podían cargar imágenes externas (fuga de datos); un PDF podía inyectar instrucciones; faltaban cabeceras de seguridad y límites; había un paquete con vulnerabilidad alta.

## Objetivo y valor
Que la aplicación resista los ataques web y de inyección identificados (categorías OWASP Top 10 relevantes) manteniendo su uso local sin fricción para la persona.

## Usuarios y actores
- **Persona usuaria:** usa la interfaz sin pasos extra.
- **Sitio web ajeno / contenido externo (PDF, respuesta de modelo):** actores no confiables.
- **Responsable de operación:** configura orígenes y límites.

## Historias de usuario
- HU-001: Como persona usuaria, quiero que otros sitios no puedan leer ni cambiar mi oficina, para proteger mi información.
- HU-002: Como persona usuaria, quiero que una respuesta de un modelo no pueda enviar mis datos a otro servidor, para evitar fugas.
- HU-003: Como responsable, quiero límites y dependencias seguras, para operar sin abusos ni vulnerabilidades conocidas.

## Requisitos funcionales
- **RF-001** Toda petición con origen a `/api`, `/hubs` y `/_blazor` se rechaza (403) si su origen no está permitido ni es el propio sitio; los orígenes permitidos son configurables.
- **RF-002** Las peticiones que cambian estado (POST, PUT, PATCH, DELETE) a `/api` deben incluir la cabecera propia del cliente; si no, se rechazan (403). Las lecturas no la requieren.
- **RF-003** Toda respuesta incluye política de contenido (solo recursos propios, sin evaluación dinámica de código), no adivinar tipos, no enviar el origen de referencia, permisos restringidos y no incrustación en otros sitios.
- **RF-004** El Markdown mostrado desde modelos no carga imágenes (muestra su texto alternativo), no admite HTML crudo y solo enlaza http, https, mailto o internos.
- **RF-005** El texto de documentos se entrega a los agentes dentro de etiquetas de datos que se neutralizan si el documento intenta cerrarlas, y los agentes reciben la regla de tratarlo como datos y no incluir imágenes ni enlaces del documento.
- **RF-006** El chat se limita por origen de red (por defecto 30 solicitudes por minuto, configurable).
- **RF-007** Las conversaciones y los casos pendientes en memoria tienen un máximo; no se descartan las conversaciones con una solicitud en curso.
- **RF-008** Los errores de proveedores mostrados a la persona no contienen identificadores internos, enlaces ni textos largos; los errores de validación se muestran en español sin detalles del framework.
- **RF-009** El proyecto no declara dependencias con vulnerabilidades conocidas.
- **RF-010** Ambos hosts aceptan únicamente nombres de host locales.
- **RF-011** Elegir una opción o cancelar sin caso pendiente se responde sin llamar a un modelo (reduce consumo).

## Requisitos no funcionales
- **RNF-001** La interfaz oficial funciona sin cambios para la persona (conexión en tiempo real, carga de la escena 3D, envío de mensajes).
- **RNF-002** Los rechazos quedan registrados sin datos sensibles.

## Reglas de negocio
- **RN-001** Un sitio ajeno no puede leer ni modificar el estado de la oficina aunque la persona tenga la aplicación abierta.
- **RN-002** El contenido de documentos y respuestas de modelos es información externa no confiable (Art. 16, 17).
- **RN-003** La falta de autenticación se acepta mientras el uso sea de un solo usuario local (Art. 21); queda declarada como límite.

## Criterios de aceptación
- **CA-001** Dado un origen ajeno, cuando intenta negociar o abrir el WebSocket del canal en tiempo real, entonces recibe 403; con el origen propio, la conexión se establece (RF-001).
- **CA-002** Dada una petición POST a `/api` sin la cabecera del cliente, cuando se envía, entonces recibe 403; con la cabecera, se procesa (RF-002).
- **CA-003** Dada cualquier respuesta, cuando se inspeccionan sus cabeceras, entonces contiene la política de contenido sin evaluación dinámica de código, `nosniff`, `no-referrer` y `SAMEORIGIN` (RF-003).
- **CA-004** Dado un Markdown con una imagen externa, un script y enlaces `javascript:`/`data:`, cuando se muestra, entonces no se genera imagen, ni script, ni esos enlaces, y los enlaces seguros se conservan (RF-004).
- **CA-005** Dado un PDF cuyo texto incluye etiquetas de cierre de la zona de datos, cuando se arma el prompt, entonces solo existen las etiquetas del sistema (RF-005).
- **CA-006** Dado el uso normal de la interfaz con las protecciones activas, cuando se abre una oficina y se carga la escena, entonces no hay errores de política de contenido en la consola (RNF-001).
- **CA-007** Dada una superación del máximo de conversaciones o casos pendientes, cuando se crea otra, entonces se descartan las más antiguas salvo las en curso (RF-007).
- **CA-008** Dado un error de proveedor con identificadores y enlaces, cuando se muestra, entonces se limpia y se acota (RF-008).
- **CA-009** Dado el análisis de paquetes, cuando se consulta, entonces no hay vulnerabilidades reportadas (RF-009).
- **CA-010** Dada la elección sin caso pendiente, cuando se envía, entonces no se llama a ningún modelo (RF-011).
- **CA-011** Dadas más solicitudes por minuto que el límite, cuando se envían al chat, entonces las excedentes reciben 429 (RF-006).

## Alcance
**Incluido:** controles anteriores y su documentación.
**Excluido:** autenticación y autorización de usuarios, aislamiento multiempresa, cifrado de datos en reposo, escaneo de secretos del repositorio.

## Datos e interfaces
Cabeceras HTTP, lista de orígenes permitidos, límite por minuto, cabecera de cliente. Sistemas: navegador, canal en tiempo real, API.

## Supuestos y dependencias
Uso local de un usuario; scripts externos deben enviar la cabecera propia para cambiar estado.

## Riesgos funcionales y casos límite
El límite por origen de red agrupa a todos los clientes locales; la política de contenido permite scripts y estilos en línea por requisito de la interfaz.

## Preguntas abiertas
- **Q-001** (APLAZADA 2026-11-10) Autenticación y autorización: punto nulo, no se desarrolla aún. Mientras tanto la aplicación no debe exponerse en una red compartida.
