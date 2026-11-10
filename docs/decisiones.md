# Índice de decisiones

Las decisiones de diseño (D-n) viven en el `plan.md` de cada especificación; aquí se enlazan para encontrarlas rápido (Art. 35, spec 005).
Las decisiones de gobierno del proyecto están en la [constitución](../.specify/memory/constitution.md) (con su registro de enmiendas).

| Spec | Tema | Decisiones | Documento |
|---|---|---|---|
| 001 | Oficinas (espacios de trabajo) | D-1 servicio en memoria · D-2 oficinas como configuración · D-3 filtrado en la aplicación · D-4 evento `WorkspaceChanged` | [plan](../specs/001-oficinas/plan.md) |
| 002 | Flujo de la base de conocimiento | D-1 flujo fijo en código · D-2 agentes `workflowOnly` · D-3 dos fases · D-4 opciones compuestas por el sistema · D-5 verificación de hechos · D-6 elección determinista · D-7 entregables Word/PDF · D-8 editor de formato · D-9 reintento del lector con Claude | [plan](../specs/002-flujo-base-conocimiento/plan.md) |
| 003 | Seguridad web | D-1 guardia por middleware · D-2 rutas protegidas · D-3 CSP · D-4 saneador de Markdown · D-5 limitador de tasa · D-6 topes de memoria · D-7 limpieza de errores | [plan](../specs/003-seguridad-web/plan.md) |
| 004 | Proveedores gratuitos | D-1 `FreeApiService` único · D-2 `RoutingLlmService` · D-3 pausas por fallo · D-4 `Sensitive` fuerza Claude · D-5 fallos de lectura · D-6 prueba de proveedor · D-7 comprobación de modelos · D-8 Gemini | [plan](../specs/004-proveedores-gratuitos/plan.md) |
| 005 | Cumplimiento constitucional | D-1 `IMarkdownRenderer` · D-2 composición sin exponer Infrastructure · D-3 `PrivateAssets` · D-4 pruebas de arquitectura · D-5 correlación · D-6 duración por llamada · D-7 registros JSON · D-8 salud · D-9 contratos · D-10 documentos | [plan](../specs/005-cumplimiento-constitucional/plan.md) |
| 006 | Núcleo de la oficina | D-1 decisión por herramienta forzada · D-2 validación del plan · D-3 ejecutor con límites · D-4 agentes en Markdown · D-5 eventos como única fuente de estado · D-6 memoria con topes · D-7 PDF como datos · D-8 Word/PDF · D-9 contador de tokens · D-10 oficina 3D | [plan](../specs/006-nucleo-oficina/plan.md) |

## Decisiones de la persona responsable (resumen)
- Sin autenticación ni varios usuarios por ahora; la aplicación es local y de una sola persona.
- Sin base de datos: el estado vive en memoria con topes (Art. 10).
- Sin pruebas de extremo a extremo (Playwright) por ahora.
- `Web` no depende de `Infrastructure`; se comunica por contratos de `Application`.
- Registros en JSON; `/health/ready` solo informa listo / no listo; documentación en `docs/`.

Nota: la decisión 003/D-4 menciona `SafeMarkdown`; desde la spec 005 se llama `SafeMarkdownRenderer` y se usa mediante `IMarkdownRenderer`.

Detalle y estado de cada spec: [specs/README.md](../specs/README.md). Preparación para producción: [preparacion-produccion.md](preparacion-produccion.md).
