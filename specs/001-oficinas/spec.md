# Especificación 001 · Oficinas (instancias de trabajo)

**Estado:** Implementada — especificación **retroactiva** (Art. 28 y 30). Redactada a partir del comportamiento verificado en el repositorio; no sustituye una aprobación previa que no existió.
**Constitución:** v1.1.0 · **Maestro:** `.specify/maestros/02_SPECIFY.md`
**Fecha:** 2026-11-10

## Resumen
**Oficinas:** al abrir la aplicación la persona elige primero una oficina; solo los agentes de esa oficina participan y se muestran.

## Problema
La oficina virtual mostraba a todos los agentes a la vez, lo que generaba desorden visual y mezclaba trabajo de desarrollo con las pruebas de la base de conocimiento, que la persona quiere aislar.

## Objetivo y valor
Que la persona vea y use únicamente el equipo necesario para el tipo de trabajo elegido, con la oficina limpia y separada por propósito.

## Usuarios y actores
- **Persona usuaria:** elige la oficina, conversa y cambia de oficina.
- **Orquestador:** coordina solo a los agentes de la oficina activa.
- **Administrador de configuración:** define oficinas y agentes (sin cambiar código).

## Historias de usuario
- HU-001: Como persona usuaria, quiero elegir una oficina al iniciar, para ver solo los agentes que necesito.
- HU-002: Como persona usuaria, quiero cambiar de oficina, para pasar de un tipo de trabajo a otro sin mezclar conversaciones.
- HU-003: Como administrador, quiero definir oficinas en la configuración, para añadir equipos sin programar.

## Requisitos funcionales
- **RF-001** Al iniciar la aplicación, antes de cualquier otra acción, se muestra la selección de oficina (cada arranque del servidor).
- **RF-002** Existen dos oficinas: **Desarrollo** (Orquestador, Arquitecto, Desarrollador, Analista QA, Profesor) y **Pruebas base de conocimiento** (Orquestador, Lector PDF, Investigador KB, Respondedor de incidentes, Auditor de cumplimiento, Redactor Word).
- **RF-003** La oficina activa determina los agentes que aparecen en el panel de agentes, las tareas, la actividad y la oficina 3D; los puestos sin dueño no se muestran.
- **RF-004** Mientras no se haya elegido una oficina, el chat rechaza los mensajes con un aviso que pide elegirla.
- **RF-005** La persona puede volver a la selección con «cambiar de oficina»; al hacerlo se limpian la conversación, los adjuntos, las tareas y la actividad visibles.
- **RF-006** Las oficinas (id, nombre, icono, descripción y lista de agentes en orden de puestos) se definen en la configuración.
- **RF-007** Si no hay oficinas configuradas, no hay pantalla de selección y todos los agentes están disponibles.
- **RF-008** Abrir o cerrar una oficina se notifica en tiempo real a las interfaces conectadas, que se actualizan.

## Requisitos no funcionales
- **RNF-001** La oficina elegida se conserva en el servidor mientras dure el proceso y no se persiste entre reinicios.

## Reglas de negocio
- **RN-001** El Orquestador está siempre en la lista de agentes de cada oficina.
- **RN-002** El Orquestador solo puede planificar y delegar a agentes de la oficina activa; los agentes ajenos no se le ofrecen.
- **RN-003** Los flujos fijos (p. ej. base de conocimiento) solo se ejecutan si todos sus agentes pertenecen a la oficina activa; si no, se usa la planificación normal.

## Criterios de aceptación
- **CA-001** Dado un arranque nuevo, cuando se abre la aplicación, entonces se muestran las dos oficinas con sus agentes y ninguna conversación es posible (RF-001, RF-004).
- **CA-002** Dado que se elige «Pruebas base de conocimiento», cuando se carga la interfaz, entonces se ven exactamente 6 agentes en ese orden y la escena 3D muestra solo esos puestos (RF-002, RF-003).
- **CA-003** Dado que no hay oficina elegida, cuando se envía un mensaje al chat, entonces se responde un error de validación que pide elegir una oficina (RF-004).
- **CA-004** Dado un caso en curso, cuando se pulsa «cambiar de oficina», entonces reaparece la selección y la conversación queda vacía (RF-005).
- **CA-005** Dado que se elige «Desarrollo», cuando el Orquestador planifica, entonces el equipo que se le ofrece no incluye agentes de la otra oficina (RN-002).
- **CA-006** Dada una configuración sin oficinas, cuando se inicia, entonces no se pide elegir y todos los agentes están activos (RF-007).
- **CA-007** Dada una oficina elegida desde otro cliente, cuando ocurre, entonces la interfaz abierta se actualiza sin recargar (RF-008).

## Alcance
**Incluido:** selección, filtrado por oficina, cambio de oficina, configuración, aviso en tiempo real.
**Excluido:** oficinas por usuario u organización (aislamiento multiusuario), persistencia de la elección, permisos por oficina.

## Datos e interfaces
Entradas: id de oficina. Salidas: oficinas disponibles, oficina activa, indicador «se requiere elegir». Estados: sin oficina / con oficina. Sistemas relacionados: planificador del Orquestador, flujo de la base de conocimiento, interfaz web y escena 3D.

## Supuestos y dependencias
- Un único usuario local (la elección es global al servidor).
- Los agentes referenciados existen en la configuración de agentes.

## Riesgos funcionales y casos límite
- Con varios navegadores abiertos comparten la misma oficina (consecuencia de la elección global).
- Una oficina que referencia un agente inexistente simplemente no lo muestra.

## Preguntas abiertas
- **Q-001** (APLAZADA 2026-11-10) Aislamiento por usuario: queda como punto nulo, no se desarrolla por ahora (el uso es de un solo usuario local).
