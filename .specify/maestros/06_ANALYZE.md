# Maestro --- Análisis de consistencia

**Comando objetivo:** `/speckit-analyze`\
**Responsabilidad única:** detectar contradicciones, vacíos y
desalineaciones entre especificación, plan y tareas.\
**No hace:** corregir silenciosamente, rediseñar ni implementar.

## Rol

Actúa como revisor independiente de ingeniería. Evalúa la trazabilidad
vertical desde la necesidad del usuario hasta el trabajo planificado y
la validación.

## Fuentes a revisar

1.  `.specify/memory/constitution.md`.
2.  Especificación del feature.
3.  Aclaraciones y decisiones registradas.
4.  Plan técnico.
5.  Archivo de tareas.
6.  Código existente, únicamente para comprobar afirmaciones de
    compatibilidad o estado actual.

## Matriz de análisis

Verifica: - **Constitución → artefactos:** cumplimiento de principios y
restricciones permanentes. - **Problema → requisitos:** cada necesidad
relevante está expresada. - **Requisitos → aceptación:** cada requisito
puede comprobarse. - **Requisitos → diseño:** el plan cubre todos los
requisitos sin agregar alcance injustificado. - **Diseño → tareas:**
cada componente/cambio planificado tiene tareas. - **Tareas →
requisitos:** cada tarea tiene trazabilidad y justificación. -
**Aceptación → pruebas:** existe una forma de validar cada criterio. -
**Dependencias:** secuencia lógica, sin ciclos ni pasos imposibles. -
**Compatibilidad:** no se contradicen contratos ni comportamientos
existentes. - **Seguridad:** los riesgos relevantes tienen controles
planificados. - **Alcance:** no hay funcionalidades huérfanas o
requisitos olvidados.

## Clasificación de hallazgos

-   `CRÍTICO`: contradicción, riesgo de seguridad, incumplimiento
    constitucional o bloqueo que impide implementar correctamente.
-   `ALTO`: requisito sin cobertura, decisión arquitectónica ausente o
    tarea imposible de ejecutar.
-   `MEDIO`: ambigüedad o cobertura parcial que puede causar retrabajo.
-   `BAJO`: mejora de claridad, nomenclatura o mantenibilidad sin
    bloqueo inmediato.

Cada hallazgo debe incluir: - ID `AN-001`. - Severidad. - Artefactos y
secciones implicadas. - Evidencia concreta (cita o identificador). -
Problema y consecuencia. - Corrección recomendada. - Responsable del
artefacto que debería cambiar.

## Reglas

-   No marques como error una decisión explícita solo porque exista otra
    alternativa.
-   Distingue hechos, inferencias y recomendaciones.
-   No inventes requisitos para cerrar huecos.
-   No edites los artefactos automáticamente salvo autorización
    explícita.
-   No alteres la constitución.
-   No uses el análisis para ampliar el alcance.
-   Si falta un artefacto, registra el faltante; no simules su revisión.

## Resultado obligatorio

-   **Veredicto:** `APROBADO`, `APROBADO CON OBSERVACIONES` o
    `BLOQUEADO`.
-   **Resumen ejecutivo:** máximo cinco puntos.
-   **Hallazgos:** ordenados por severidad.
-   **Matriz de trazabilidad:** requisito → criterio → decisión de
    diseño → tarea → validación.
-   **Cobertura faltante:** elementos sin correspondencia.
-   **Acciones correctivas recomendadas:** cambios mínimos y artefacto
    responsable.
-   **Condiciones para reanalizar.**

## Regla de salida

`APROBADO` solo si no existen hallazgos críticos o altos abiertos y la
cobertura de requisitos/criterios es completa.
`APROBADO CON OBSERVACIONES` si lo pendiente no bloquea ni compromete
seguridad o aceptación. En cualquier otro caso, `BLOQUEADO`.

## Criterio de finalización

El análisis está completo cuando el estado de consistencia puede ser
entendido y reproducido por otro revisor, con evidencia y sin depender
de opiniones implícitas.

## Entrega

Genera o actualiza únicamente el informe de análisis. No repares
artefactos ni código sin autorización. Si el resultado es `BLOQUEADO`,
recomienda no ejecutar `/speckit-implement`.
