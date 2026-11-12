# Especificación 008 · Oficina 2D (retirada del 3D)

**Estado:** Aprobada por la persona responsable en chat (2026-11-10): «vamos a retirar el 3D, solo colocaremos agentes 2D».
**Constitución:** v1.1.0 · **Maestro:** `.specify/maestros/02_SPECIFY.md` · **Sustituye:** RF-011 y CA-014 de la spec 006 (oficina 3D).

## Resumen
La oficina 3D (three.js) se retira. En su lugar, cada agente de la oficina activa aparece como un puesto 2D con su estado en vivo.

## Problema
La escena 3D añadía complejidad (WebGL, ~1.400 líneas de JavaScript, una librería de terceros) sin aportar al objetivo actual, y no tenía pruebas automáticas.

## Requisitos funcionales
- **RF-001** Un puesto 2D por agente de la oficina activa: avatar, nombre, rol, estado (con color y etiqueta en español) y tarea actual.
- **RF-002** El estado se actualiza solo con los eventos reales (misma fuente que el resto de la interfaz).
- **RF-003** Clic (o teclado) en un puesto abre la ficha del agente: estado, tarea, estadísticas y actividad reciente; permite cancelar la tarea actual.
- **RF-004** Se elimina el código y la dependencia 3D (`office3d.js`, `three.js`) y sus referencias.

## Requisitos no funcionales
- **RNF-001** Accesible: los puestos son botones con foco visible y `aria-pressed`; se respeta `prefers-reduced-motion`.
- **RNF-002** La interfaz no depende de WebGL ni de scripts de terceros.

## Criterios de aceptación
- **CA-001** Con una oficina abierta, hay exactamente un puesto por agente de su lista (RF-001).
- **CA-002** El estado de un puesto cambia cuando llega un evento del agente (RF-002).
- **CA-003** Un clic abre y cierra la ficha del agente (RF-003).
- **CA-004** No quedan referencias a three.js ni a `officeScene` en el código ni en la configuración (RF-004).

## Alcance
**Incluido:** componente `AgentBoard`, retirada del 3D, estilos y documentación. **Excluido:** animaciones de personajes, pruebas de interfaz (aplazadas).
