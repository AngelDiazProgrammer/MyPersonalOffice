# Análisis de consistencia 005 · Cumplimiento constitucional

**Maestro:** `.specify/maestros/06_ANALYZE.md` · **Fecha:** 2026-11-10 · **Constitución:** v1.1.0
**Artefactos revisados:** `spec.md`, `clarifications.md`, `plan.md`, `tasks.md` y el código real (referencias de proyectos, `Program.cs`, `RoutingLlmService`, `OfficeApiExtensions`, `MarkdownView.razor`).

## Veredicto: APROBADO CON OBSERVACIONES

## Resumen ejecutivo
1. Todos los RF (001–008), RNF y CA (001–009) tienen decisión de diseño, tarea y validación.
2. Las decisiones del responsable (Q-001 a Q-003) y las del plan (Q-004 a Q-006) están reflejadas y no se contradicen con la constitución.
3. El código real coincide con las afirmaciones del plan (única dependencia de `Web` hacia Infrastructure: `SafeMarkdown` y `AddOfficeConfiguration`).
4. El riesgo técnico principal es R-1 (`PrivateAssets`), con una tarea que detiene la ejecución si no basta.
5. No hay hallazgos críticos ni altos.

## Hallazgos
- **AN-001 · MEDIO** — Plan D-3 / T-006. `PrivateAssets="compile"` sobre una referencia de proyecto debe comprobarse empíricamente: si el aislamiento no funciona, la prueba de arquitectura (T-007) fallaría pero el aislamiento por compilación no existiría. Evidencia: `Web.csproj` referencia `Api`, que referencia `Infrastructure`. Corrección: T-006 ya exige validar y detenerse ante el fallo; la decisión alternativa (capa de composición separada) requeriría aprobación. Responsable: plan.
- **AN-002 · MEDIO** — T-002/T-003 ↔ código. `SafeMarkdown` es hoy una clase **estática** usada por `SafeMarkdownTests` y `MarkdownView`; convertirla en instancia cambia sus llamadas. Evidencia: `SecurityTests.cs` (`SafeMarkdown.ToHtml`). Corrección: T-003 ya ajusta las pruebas; sin impacto funcional. Responsable: tareas.
- **AN-003 · BAJO** — Plan D-8 / spec RNF-001. El estado de salud no debe exponer rutas ni claves; la tarea T-013 lo verifica. Sin acción adicional.
- **AN-004 · BAJO** — Spec RN-002 («sin cambios visibles»). `/health/ready` y el formato de registro son cambios operativos nuevos, no visibles para la persona; consistente con la regla. Sin acción.
- **AN-005 · BAJO** — Dos hosts. `Api/Program.cs` también importa el espacio de nombres de Infrastructure (para `AddOfficeConfiguration`); T-004 lo sustituye por la extensión de `Api` y evita ambigüedad de nombres. Responsable: tareas.

## Matriz de trazabilidad
| Requisito | Criterio | Decisión de diseño | Tarea | Validación |
|---|---|---|---|---|
| RF-001 | CA-001 | D-5 | T-009 | T-011 (`LoggingTests`) |
| RF-002, RNF-001 | CA-002 | D-6 | T-010 | T-011 |
| RF-003 | CA-003 | D-9 | T-014 | T-014 (+ comprobación negativa) |
| RF-004 | CA-004 | D-8 | T-012 | T-013 (`HealthTests`) |
| RF-005 | CA-005 | D-10 | T-015 | T-017 (`DocsTests`) |
| RF-006 | CA-006 | D-10 | T-016 | T-017 |
| RF-007 | CA-007 | D-7 | T-008 | T-011 |
| RF-008, RNF-002 | CA-008, CA-009 | D-1, D-2, D-3, D-4 | T-001..T-007 | T-007 (`ArchitectureTests`), T-003 |

## Cobertura faltante
Ninguna.

## Acciones correctivas recomendadas
Ejecutar T-006 antes de T-007 y detenerse ante el fallo de `PrivateAssets` (AN-001). Sin cambios de artefactos.

## Condiciones para reanalizar
Si T-006 requiere una decisión nueva de arquitectura o si cambia el alcance de la spec.
