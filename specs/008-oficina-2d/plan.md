# Plan técnico 008 · Oficina 2D

**Maestro:** `.specify/maestros/04_PLAN.md` · **Estado:** implementado

## Resumen y objetivo técnico
Sustituir `OfficeScene` (3D) y `AgentStrip` por `AgentBoard` (2D, Blazor + CSS) reutilizando `OfficeStateService` y la ficha del agente.

## Referencias
`specs/008-oficina-2d/spec.md`; spec 006 (D-10, RF-011, CA-014).

## Decisiones de arquitectura
- **D-1** `AgentBoard.razor` hereda de `OfficeComponentBase` y se redibuja con los mismos eventos que el resto de la interfaz; no tiene lógica de negocio.
- **D-2** Estados con las mismas variables CSS de color (`--c-*`) y etiquetas de `StatusInfo`; la ficha reutiliza el estilo `.agent-card`.
- **D-3** Se eliminan `office3d.js`, `wwwroot/lib/three`, el `<script type="module">` y la promesa `officeSceneReady`.
- **D-4** Animación mínima (pulso del estado) desactivada con `prefers-reduced-motion`.

## Pruebas
Sin pruebas de interfaz (aplazadas); verificación visual en la app real. La suite existente y los contratos no cambian.
