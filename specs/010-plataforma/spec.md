# Especificación 010 · Plataforma: acceso, navegación por secciones y registros

**Estado:** Aprobada en chat por la persona responsable (2026-11-11): «rediseño total del front… login… secciones… logs… empecemos a darle forma de orquestador multiagente».
**Constitución:** v1.1.0 · **Maestro:** `.specify/maestros/02_SPECIFY.md` · **Relacionadas:** 001 (oficinas), 007 (conexiones), 009 (Camila IA).

## Resumen
La aplicación deja de ser un proyecto de prueba y pasa a ser una plataforma de orquestación multiagente con **acceso por usuario y contraseña**, una **navegación por secciones** y una sección de **Logs** en vivo. Camila IA pasa a tener su propio entorno, separado de las oficinas virtuales.

## Problema
Cualquiera que llegue a la dirección usa la app y sus conexiones; la pantalla inicial mezcla oficinas, chatbots y agentes; no hay forma de ver qué ocurre por dentro (webhooks, modelos, flujos); y los modelos gratuitos parecen «no funcionar» porque no se ve qué proveedor responde.

## Objetivo y valor
Entrar con credenciales, encontrar cada cosa en su sección y ver en vivo qué hace la plataforma, sin exponer datos personales.

## Usuarios y actores
Persona administradora (usuario `addiazf`). No hay autoservicio de altas por ahora.

## Historias de usuario
- **HU-001 (P1)** Como administrador, quiero iniciar sesión con usuario y contraseña, para que solo yo use la plataforma y sus conexiones.
- **HU-002 (P1)** Como administrador, quiero un menú con Oficinas virtuales, Chatbots, Agentes conversacionales, Conexiones y Logs, para llegar a cada función sin mezclarlas.
- **HU-003 (P1)** Como administrador, quiero ver en una sección de Logs lo que va pasando (webhooks, modelos, flujos, agentes), para entender y depurar.
- **HU-004 (P2)** Como administrador, quiero que Camila tenga un entorno propio, para que no se confunda con las oficinas de agentes.

## Requisitos funcionales
**Acceso**
- **RF-001** Toda página, API y canal en tiempo real exige sesión iniciada, salvo la pantalla de acceso, el estado de salud y los archivos estáticos de la interfaz.
- **RF-002** El usuario administrador inicial vive en un archivo Markdown de configuración; su contraseña **no** se guarda en claro sino como hash con sal.
- **RF-003** Tras 5 intentos fallidos seguidos de un usuario desde un mismo origen, el acceso se bloquea 5 minutos.
- **RF-004** La sesión caduca a las 8 horas de inactividad; «Salir» la cierra de inmediato.
- **RF-005** El mensaje de error del acceso no revela si falló el usuario o la contraseña.
- **RF-006** Las llamadas internas de la interfaz a la API se autentican sin exponer ningún secreto al navegador.

**Navegación**
- **RF-007** Menú lateral con: Oficinas virtuales (Desarrollo y Base de conocimiento), Chatbots (Camila IA UML), Agentes conversacionales (Camila IA Conversacional), Conexiones y Logs; muestra el usuario y «Salir».
- **RF-008** Cada oficina de agentes conserva su comportamiento actual (oficina 2D, tareas, actividad, chat).
- **RF-009** Camila IA UML y Camila IA Conversacional se abren en un entorno propio (chat más el registro de lo que hace), sin la oficina 2D.
- **RF-010** La sección de una oficina, chatbot o agente abre su espacio sin pasos intermedios cuando solo hay uno.

**Logs**
- **RF-011** La sección Logs muestra en vivo (actualización cada 2 s) hora, nivel, origen (Camila, Webhook, Modelo, Agente, Seguridad, Sistema), mensaje e identificador de conversación.
- **RF-012** Permite filtrar por origen, nivel y texto, pausar la actualización y limpiar la vista.
- **RF-013** Los logs no contienen cédulas, credenciales, cuerpos de webhooks ni textos de los mensajes de la persona.
- **RF-014** Camila registra por turno: dato recibido (solo el nombre), ruta tomada, webhook (modo, resultado, tiempo), generador y avisos.
- **RF-015** El registro es en memoria, con tope, y se pierde al reiniciar.

**Modelos**
- **RF-016** La oficina Conversacional de Camila puede usar los modelos gratuitos solo si se activa explícitamente (`Camila:AllowFreeModels`); por defecto usa solo Claude.

## Requisitos no funcionales
- **RNF-001** Cookie de sesión HttpOnly y SameSite estricta; formulario de acceso con protección antifalsificación.
- **RNF-002** Mantiene los estilos actuales de la interfaz.
- **RNF-003** La interfaz sigue accesible con teclado y foco visible.

## Reglas de negocio
- **RN-001** Los datos personales no se registran (Art. 20, 22).
- **RN-002** Las credenciales nunca se guardan en texto claro en el repositorio (Art. 19).

## Criterios de aceptación
- **CA-001** Sin sesión, abrir `/`, `/api/office/state` o el canal en tiempo real no entrega datos (redirige al acceso o responde 401).
- **CA-002** Con usuario y contraseña correctos entra y ve el menú; con otros, ve un error genérico.
- **CA-003** El archivo de usuarios no contiene la contraseña en claro y la verificación usa el hash.
- **CA-004** Cinco fallos seguidos bloquean el acceso aunque la sexta contraseña sea correcta.
- **CA-005** «Salir» cierra la sesión y las rutas vuelven a exigirla.
- **CA-006** El menú tiene las cinco secciones y cada una abre su contenido.
- **CA-007** En Logs aparece la llamada a un webhook de Camila con su modo y resultado, sin cuerpos ni cédulas.
- **CA-008** Filtrar por origen y texto reduce las filas; pausar detiene la actualización.
- **CA-009** Con `AllowFreeModels` en falso, Camila Conversacional no llama a modelos gratuitos.

## Alcance
**Incluido:** acceso, menú, secciones, Logs, entorno propio de Camila, hash de contraseñas, opción de modelos gratuitos para Camila. **Excluido:** varios usuarios y roles distintos del administrador, recuperación de contraseña, persistencia de logs, OAuth/SSO, la evaluación de Camila como playbook/agentes (documento aparte, sin implementar).

## Datos e interfaces
Archivo `configuration/usuarios.md` (usuario, rol, hash). API `GET /api/logs`. Endpoints de acceso `/login`, `/auth/login`, `/auth/logout`.

## Supuestos y dependencias
- Uso local de una sola persona; HTTPS cuando se despliegue fuera del equipo (la cookie será segura si la petición es https).
- La contraseña facilitada en el chat se convierte a hash: el repositorio no la guarda en claro (decisión de seguridad que se explicó a la persona responsable).

## Riesgos funcionales y casos límite
- Olvidar la contraseña: se regenera el hash con el comando de la guía.
- La API independiente (sin la interfaz) necesita la misma configuración de acceso y un token interno compartido.
- Logs en memoria: no sirven para auditoría histórica.

## Preguntas abiertas
Ninguna que bloquee.
