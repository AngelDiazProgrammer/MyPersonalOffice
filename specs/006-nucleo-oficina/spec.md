# Especificación 006 · Núcleo de la oficina

**Nota (2026-11-10):** RF-011 y CA-014 (oficina 3D) fueron **sustituidos por la [spec 008](../008-oficina-2d/spec.md)** (oficina 2D).
**Estado:** Implementada — especificación **retroactiva** (Art. 28 y 30). Redactada a partir del comportamiento verificado en el repositorio y sus pruebas; no sustituye una aprobación previa que no existió (Art. 29).
**Constitución:** v1.1.0 · **Maestro:** `.specify/maestros/02_SPECIFY.md` · **Fecha:** 2026-11-10

## Resumen
Núcleo original de ATLAS: un Orquestador planifica y delega el trabajo a agentes definidos en Markdown; se ejecuta en paralelo con límites; se puede adjuntar un PDF; se exportan documentos Word/PDF; se cuenta el consumo de tokens; y una oficina 3D refleja el estado en tiempo real.

## Problema
La persona quiere pedir un trabajo en lenguaje natural y que un equipo de agentes especializados lo resuelva de forma visible, controlada y barata, sin programar cada agente.

## Objetivo y valor
Que el trabajo se reparta, se ejecute con aislamiento de errores y límites, y se pueda seguir y medir (tareas, actividad, tokens, escena 3D).

## Usuarios y actores
Persona usuaria (conversa, adjunta, exporta); Orquestador (planifica y consolida); agentes (ejecutan tareas); administrador de configuración (define agentes en Markdown).

## Historias de usuario
- HU-001: Como persona usuaria, quiero escribir una petición y recibir una respuesta consolidada, para no coordinar yo a los agentes.
- HU-002: Como administrador, quiero definir agentes en archivos Markdown, para añadir o ajustar roles sin programar.
- HU-003: Como persona usuaria, quiero adjuntar un PDF para que los agentes autorizados lo lean.
- HU-004: Como persona usuaria, quiero exportar un contenido a Word o PDF.
- HU-005: Como persona usuaria, quiero ver el consumo de tokens y su costo estimado.
- HU-006: Como persona usuaria, quiero ver en una oficina 3D qué agente trabaja en qué.

## Requisitos funcionales
- **RF-001** El Orquestador responde directamente o delega mediante una decisión estructurada validada (agentes existentes, máximo de tareas, ids únicos, dependencias sin ciclos); si es inválida se reintenta una vez y, si persiste, se da una respuesta segura.
- **RF-002** Las tareas independientes se ejecutan en paralelo; las dependientes esperan y reciben el resultado de sus prerrequisitos.
- **RF-003** La concurrencia está limitada, un agente ejecuta una tarea a la vez, cada tarea tiene tiempo límite, los errores de un agente no rompen a los demás y se puede cancelar una tarea o la solicitud completa.
- **RF-004** Una respuesta vacía se reintenta una vez; una cortada por límite de tokens se avisa a la persona.
- **RF-005** El resultado se consolida en una respuesta final; si la consolidación falla se entregan los resultados directos; una tarea única exitosa se devuelve tal cual (configurable).
- **RF-006** Los agentes se definen en `agents/*.md` (identidad, rol, instrucciones, configuración) y se pueden recargar sin reiniciar; las incidencias de configuración se informan.
- **RF-007** Se puede adjuntar un PDF (≤ 10 MB) cuyo texto se extrae localmente y solo llega a los agentes que aceptan documentos; los escaneados se detectan.
- **RF-008** Se puede exportar Markdown a Word (con estilo APA opcional) y PDF, y descargar los documentos generados.
- **RF-009** Se cuenta el consumo de tokens por agente y por proveedor, con costo estimado solo si hay tarifas configuradas.
- **RF-010** Cada cambio de estado de un agente se emite como evento en tiempo real; la interfaz solo refleja eventos.
- **RF-011** La interfaz muestra una oficina 3D animada con un puesto por agente y el chat, las tareas y la actividad.
- **RF-012** Los agentes no delegan entre sí ni ejecutan comandos: solo el Orquestador coordina.

## Requisitos no funcionales
- **RNF-001** Estado en memoria (sin base de datos), con topes (Art. 10).
- **RNF-002** El historial enviado al modelo se acota y los turnos viejos se resumen para ahorrar tokens.
- **RNF-003** Los mensajes de error son comprensibles y sin detalles internos.

## Reglas de negocio
- **RN-001** Mensajes vacíos o por encima del tope se rechazan.
- **RN-002** El texto del PDF se trata como datos, no como instrucciones.

## Criterios de aceptación
- **CA-001** Una decisión «responder» contesta sin delegar (RF-001).
- **CA-002** Una delegación ejecuta tareas y consolida (RF-001, RF-002, RF-005).
- **CA-003** Una tarea dependiente recibe el resultado previo (RF-002).
- **CA-004** Un agente que falla bloquea solo a sus dependientes (RF-003).
- **CA-005** La concurrencia respeta el máximo y un agente no solapa tareas (RF-003).
- **CA-006** Una tarea que excede el tiempo falla con mensaje claro y puede cancelarse (RF-003).
- **CA-007** Una respuesta vacía o cortada se trata como en RF-004.
- **CA-008** Una decisión inválida se corrige con un reintento o cae a respuesta segura (RF-001).
- **CA-009** El PDF adjunto llega solo a agentes que aceptan documentos (RF-007, RN-002).
- **CA-010** Se genera un Word/PDF descargable (RF-008).
- **CA-011** El consumo se agrupa por agente y proveedor y el costo solo aparece con tarifas (RF-009).
- **CA-012** Los Markdown de agentes se cargan, validan y recargan (RF-006).
- **CA-013** El estado del agente sigue eventos reales (RF-010).
- **CA-014** La oficina 3D muestra un puesto por agente de la lista activa y se anima con los eventos (RF-011).

## Alcance
**Incluido:** orquestación, ejecución de planes, agentes Markdown, PDF de entrada, exportación Word/PDF, contador de tokens, eventos y oficina 3D.
**Excluido:** OCR, streaming token a token, persistencia, autenticación, modelos 3D de terceros.

## Datos e interfaces
Entradas: mensaje, PDF, solicitud de exportación. Salidas: respuesta consolidada, tareas, actividad, uso de tokens, documentos. Interfaces: REST `/api/*` y hub `/hubs/office` (ver README y spec 005 para contratos).

## Supuestos y dependencias
Uno o más proveedores de modelos (spec 004); oficinas (spec 001); flujo de conocimiento (spec 002); seguridad (spec 003).

## Riesgos funcionales y casos límite
Planes con muchas tareas, PDF escaneados, modelos gratuitos que devuelven nombres de agente aproximados (se mapean al id real).

## Preguntas abiertas
- **Q-001** (APLAZADA) Pruebas de interfaz y de la escena 3D (Playwright): el CA-014 se verifica manualmente hasta adoptarlas.
- **Q-002** (APLAZADA) OCR y streaming: fuera de alcance.
