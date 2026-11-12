# Especificaciones de MyPersonalOffice

Índice de las especificaciones (Spec-Driven Development, Art. 27–30 de la constitución v1.1.0). Cada carpeta contiene, siguiendo los
[maestros](../.specify/maestros/README.md): `spec.md` → `plan.md` → `tasks.md` → `analysis.md` → `convergence.md`.

## Especificaciones retroactivas (2026-11-10)
Las funcionalidades siguientes se construyeron antes de adoptar Spec Kit; se documentaron a partir del repositorio y sus pruebas para cumplir la
trazabilidad del Art. 30. **No sustituyen una aprobación previa que no existió** (Art. 29).

| Spec | Funcionalidad | Veredicto de análisis | Convergencia | Pendientes clave |
|---|---|---|---|---|
| [001-oficinas](001-oficinas/spec.md) | Selección de oficina y filtrado de agentes | Aprobado con observaciones | PARCIAL | Pruebas E2E de interfaz; aislamiento por usuario |
| [002-flujo-base-conocimiento](002-flujo-base-conocimiento/spec.md) | Consulta guiada a la base, soluciones, entregable APA | Aprobado con observaciones | COMPLETO (con observaciones) | Privacidad por defecto con PDF (Art. 20, mantenida para pruebas) |
| [003-seguridad-web](003-seguridad-web/spec.md) | Endurecimiento web y API (OWASP) | Aprobado con observaciones | PARCIAL (solo CA-006 manual) | Autenticación (punto nulo); pruebas de interfaz aplazadas |
| [004-proveedores-gratuitos](004-proveedores-gratuitos/spec.md) | APIs gratuitas primero (Groq, Cloudflare, Gemini) y Claude | Aprobado con observaciones | COMPLETO (con observaciones) | Privacidad por proveedor; selector de proveedor |
| [006-nucleo-oficina](006-nucleo-oficina/spec.md) | Orquestación, agentes en Markdown, PDF/Word, tokens y oficina 3D | Aprobado con observaciones | PARCIAL (3D solo manual) | Pruebas de interfaz / 3D |
| [008-oficina-2d](008-oficina-2d/spec.md) | Retirada del 3D; puestos 2D con estado en vivo | Aprobado en chat | COMPLETO (con observaciones) | Pruebas de interfaz |
| [009-camila-ia](009-camila-ia/spec.md) | Camila IA UML y Conversacional sobre el export de Dialogflow (`CamilaIA.md`) | Aprobado en chat | COMPLETO salvo prueba en vivo | T-012 (prueba en producción, a cargo de la persona responsable) |
| [010-plataforma](010-plataforma/spec.md) | Login, menú por secciones, Logs y entorno propio de Camila | Aprobado en chat | COMPLETO salvo verificación final | Ver tareas |
| [011-agentes](011-agentes/spec.md) | Catálogo de agentes con uso específico y gestión desde el front | Aprobado en chat | COMPLETO | — |
| [012-playbooks-bots](012-playbooks-bots/spec.md) | Playbooks maestros de bots, LectorUML y CamilaIA como agentes | Aprobado en chat | COMPLETO | — |
| [007-conectividad-navegadores](007-conectividad-navegadores/spec.md) | Playwright como servicio: sitios, playbooks, aprobación, aprendizaje y autorreparación | Aprobado en chat | COMPLETO (con observaciones) | Prueba real con Gmail (requiere tu sesión); enmienda constitucional propuesta |

## Propuestas pendientes de aprobación
| Spec | Tema | Estado |
|---|---|---|
| [005-cumplimiento-constitucional](005-cumplimiento-constitucional/spec.md) | Registros JSON correlacionados, pruebas de contrato, salud y producción, separación `Web` → `Application` | Spec, plan y tareas aprobados · implementación completa (T-001..T-019) · [informe de convergencia](005-cumplimiento-constitucional/convergence.md): COMPLETO |

## Capacidades anteriores aún sin especificación
Documentadas en la spec 006. Solo queda sin spec propia la documentación del código y sus convenciones (README, «Convenciones del código»).

## Decisiones del responsable (2026-11-10)
1. **Privacidad con proveedores gratuitos:** se mantiene como está para pruebas con datos ficticios (spec 002 Q-001, spec 004 Q-001). Se reabre antes de usar datos reales.
2. **Respaldo del Lector PDF** (spec 002 Q-002): variante A — reintentar una vez con Claude (RF-014). **Implementado y probado.**
3. **Pruebas E2E / Playwright:** aplazadas; primero se deja el código listo con spec (T-013 spec 001 y T-023 spec 002). Las tareas T-013 y T-014 de la spec 003 (probar límite de tasa y hosts, repetir sondeos) no dependen de Playwright y siguen vigentes.
4. **Autenticación y aislamiento multiusuario:** punto nulo, no se desarrolla por ahora (specs 001 y 003).

## Análisis global de coherencia (2026-11-10)
Revisión no destructiva de las specs 001–006 (maestro `06_ANALYZE`):
- Cada spec tiene `spec`, `plan`, `tasks`, `analysis` y `convergence`; la 005 además `clarifications`.
- Tareas abiertas, todas aplazadas por decisión de la persona responsable: pruebas E2E (001 T-013, 002 T-023, 006 T-009) y privacidad/selector de proveedor (004 T-014, T-015). Ninguna bloquea el uso previsto.
- Referencias a `SafeMarkdown` en specs 001–004 son históricas; el nombre vigente es `SafeMarkdownRenderer` (ver `docs/decisiones.md`).
- Secretos: `.env` está ignorado por git y no está versionado; `.env.example` no contiene claves. No se hizo barrido por patrones del historial.
- Índice de decisiones y lista de producción en `docs/`.

## Auditoría constitucional
[Anexo de auditoría artículo por artículo](auditoria-constitucional.md) (2026-11-10): 24 artículos cumplen, 8 cumplen con observación, 5 parciales (11, 31, 34, 36, 37) y 3 aplazados por decisión; 10 observaciones propuestas (OBS-01..OBS-10), ninguna implementada sin tu aprobación.
