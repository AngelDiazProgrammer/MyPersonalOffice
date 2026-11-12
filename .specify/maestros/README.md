# Prompts maestros de Spec-Driven Development · ATLAS

Cada comando de Spec Kit tiene aquí un **prompt maestro** que fija su responsabilidad única, sus límites, su estructura de salida y sus reglas de entrega.
Los skills de `.claude/skills/speckit-*/SKILL.md` los referencian y **deben seguirlos**. Ningún maestro puede contradecir
[`.specify/memory/constitution.md`](../memory/constitution.md) (Art. 39: la constitución prevalece).

| Orden | Comando | Maestro | Responsabilidad única | No hace |
|---|---|---|---|---|
| 1 | `/speckit-constitution` | [`../memory/constitution.md`](../memory/constitution.md) | Principios permanentes (v1.0.0) | No se modifica en silencio (Art. 40) |
| 2 | `/speckit-specify` | [`02_SPECIFY.md`](02_SPECIFY.md) | Convertir una necesidad en especificación verificable (RF, RNF, RN, CA) | Diseñar, repartir tareas, tocar código |
| 3 | `/speckit-clarify` | [`03_CLARIFY.md`](03_CLARIFY.md) | Resolver ambigüedades relevantes (Q-001…) | Inventar requisitos, implementar |
| 4 | `/speckit-plan` | [`04_PLAN.md`](04_PLAN.md) | Definir cómo se implementa una especificación aprobada | Redefinir el problema, hacer tareas, programar |
| 5 | `/speckit-tasks` | [`05_TASKS.md`](05_TASKS.md) | Backlog atómico y verificable (T-001…) | Cambiar el diseño, ejecutar tareas |
| 6 | `/speckit-analyze` | [`06_ANALYZE.md`](06_ANALYZE.md) | Detectar contradicciones y vacíos (AN-001…; veredicto) | Corregir en silencio, implementar |
| 7 | `/speckit-implement` | [`07_IMPLEMENT.md`](07_IMPLEMENT.md) | Ejecutar tareas aprobadas, una a una, con evidencia | Ampliar alcance, tomar decisiones de producto |
| 8 | `/speckit-converge` | [`08_CONVERGE.md`](08_CONVERGE.md) | Comparar lo aprobado con lo construido (PEND-001…) | Implementar pendientes, declarar éxito sin evidencia |

`/speckit-checklist` no tiene maestro propio: sus listas deben trazar a la especificación aprobada y a la constitución.

## Flujo obligatorio (Art. 28)

Constitución → Specify → Clarify → Plan → Tasks → Analyze → Implement → Validación → Converge.
No se inicia una implementación significativa sin especificación y plan aprobados. `/speckit-implement` no corre si el último análisis es `BLOQUEADO`.

## Estado de los puntos detectados al integrar

1. **Art. 16** — resuelto: se eliminó «Revisar la base de conocimiento» (enmienda 1.1.0).
2. **Art. 19** — resuelto: «OWASP Top 10» se movió al Art. 3 y se refuerza en el Art. 24 (enmienda 1.1.0).
3. **Maestros 02, 03 y 04** — decisión del responsable: se dejan como están («seis agentes», «Groq/Cloudflare»). Verifica siempre el repositorio real.
