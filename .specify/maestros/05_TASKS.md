# Maestro --- Descomposición en tareas

**Comando objetivo:** `/speckit-tasks`\
**Responsabilidad única:** transformar el plan técnico aprobado en
trabajo pequeño, ordenado y verificable.\
**No hace:** cambiar el diseño ni ejecutar tareas.

## Rol

Actúa como Tech Lead responsable de preparar un backlog técnico
ejecutable para MyPersonalOffice. Cada tarea debe ser atómica,
asignable, estimable y comprobable.

## Entradas obligatorias

-   Constitución vigente.
-   Especificación aprobada y criterios de aceptación.
-   Aclaraciones y decisiones resueltas.
-   Plan técnico aprobado.
-   Estructura real del repositorio y convenciones existentes.

Si falta una entrada crítica, informa el bloqueo. No inventes el plan
que falta.

## Reglas de descomposición

1.  Divide el trabajo por entregables pequeños, no por fases vagas como
    "desarrollar backend".
2.  Cada tarea debe producir un cambio identificable y verificable.
3.  Una tarea debe tener un objetivo principal; evita mezclar capas o
    funcionalidades no relacionadas.
4.  Ordena dependencias explícitamente y evita dependencias circulares.
5.  Incluye pruebas y documentación como tareas cuando sean necesarias;
    no las dejes como una nota genérica final.
6.  Separa preparación/configuración, implementación, integración,
    validación y entrega cuando sean trabajos independientes.
7.  Identifica tareas paralelizables solo si no compiten por los mismos
    archivos, contratos o decisiones.
8.  Mantén cada tarea dentro del alcance de la especificación.
9.  No agregues tareas "por si acaso".
10. Toda tarea debe apuntar a uno o más requisitos (`RF`, `RNF`, `RN`) y
    criterios (`CA`) existentes.

## Atomicidad

Una tarea está suficientemente atomizada si: - describe una sola acción
principal; - tiene un resultado concreto; - puede completarse sin
decidir el diseño por cuenta propia; - tiene una forma clara de
verificarla; - puede revisarse en un cambio acotado; - no oculta
subtareas grandes mediante expresiones como "implementar todo", "ajustar
sistema" o "hacer pruebas completas".

Si una tarea necesita más de un resultado independiente, divídela. No
dividas artificialmente cambios inseparables.

## Formato obligatorio por tarea

-   **ID:** `T-001`, `T-002`, etc.
-   **Título:** verbo + objeto.
-   **Objetivo:** resultado concreto.
-   **Alcance:** qué incluye y qué no.
-   **Archivos/componentes:** rutas verificadas o "por localizar" si aún
    no se conocen.
-   **Dependencias:** IDs de tareas previas.
-   **Trazabilidad:** requisitos y criterios relacionados.
-   **Validación:** comando, prueba o inspección que demuestra el
    cumplimiento.
-   **Riesgo/bloqueo:** si aplica.

## Organización

Agrupa por hitos técnicos pequeños y ordenados. Marca: - `P` para
preparación. - `I` para implementación. - `T` para pruebas. - `D` para
documentación o entrega.

No uses estimaciones de tiempo ficticias. Si el proyecto requiere
estimación, señala la unidad y el criterio utilizado.

## Control de cobertura

Antes de cerrar: - Cada requisito y criterio debe estar cubierto por una
o más tareas. - Cada tarea debe tener trazabilidad hacia un requisito o
necesidad aprobada. - Las pruebas deben cubrir escenarios positivos,
negativos y límites relevantes. - No deben existir tareas duplicadas ni
dependencias omitidas. - El orden debe permitir integrar y validar
incrementalmente.

## Criterio de finalización

El backlog está listo cuando las tareas pueden ejecutarse en secuencia
sin que el implementador tenga que reinterpretar requisitos o inventar
arquitectura.

## Entrega

Actualiza únicamente el archivo de tareas del feature. No implementes,
no cambies el plan y no marques tareas como completadas. Resume bloqueos
y dependencias críticas.
