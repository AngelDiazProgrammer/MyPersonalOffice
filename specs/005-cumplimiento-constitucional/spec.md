# Especificación 005 · Cumplimiento constitucional: observabilidad, contratos y límites de capas

**Estado:** **ESPECIFICACIÓN APROBADA (2026-11-10, responsable del proyecto)**; aclaraciones resueltas (ver `clarifications.md`); **plan técnico aprobado (2026-11-10)**; tareas y análisis generados; implementación en curso según `tasks.md`.
**Origen:** auditoría del código contra la constitución v1.1.0 (2026-11-10). **Maestro:** `.specify/maestros/02_SPECIFY.md`

## Resumen
**Cumplimiento constitucional:** cerrar las brechas que la auditoría encontró entre el código actual y los artículos de la constitución que no están cubiertos por las specs 001–004.

## Problema
Tras revisar el código, hay requisitos constitucionales que hoy se cumplen solo en parte: los registros no permiten reconstruir una solicitud completa (Art. 22), faltan pruebas de contratos (Art. 24), una capa de presentación referencia infraestructura directamente (Art. 7) y no hay una comprobación de preparación para producción (Art. 36).

## Objetivo y valor
Que el producto cumpla de forma verificable los artículos de calidad, observabilidad y arquitectura, sin añadir funcionalidades nuevas para la persona usuaria.

## Resultado de la auditoría (evidencia)
| Artículo | Estado | Evidencia |
|---|---|---|
| 7 Dependencias hacia el núcleo | **Cumple** en Domain y Application (no referencian Infrastructure, Api ni Web). **Observación:** `Web` usa tipos de Infrastructure (`SafeMarkdown`, `AddOfficeConfiguration`) porque aloja la API en proceso. | `grep` de referencias; `MarkdownView.razor`, `Program.cs` |
| 9, 15 Independencia de proveedores | Cumple: Application usa `IAnthropicService`; proveedores en Infrastructure por configuración. | spec 004 |
| 14, 19 Agentes y secretos | Cumple: agentes versionados sin credenciales; claves solo en `.env` (ignorado por git). | `agents/*.md`, `.gitignore` |
| 22 Registro y auditoría | **Parcial:** se registran agente, proveedor, tokens y errores; **no** hay identificador de solicitud/conversación en los registros ni tiempos de respuesta por llamada. | `AgentRunner`, `AnthropicService`, `FreeApiService` (solo `Stopwatch` en la prueba de proveedor) |
| 24 Pruebas | **Parcial:** hay pruebas unitarias, de integración (servidor real), de seguridad y de regresión (228); **faltan** pruebas de contratos de la API y de interfaz (esta última aplazada). | `tests/` |
| 25 Manejo de errores | Cumple: reintentos acotados, sin valores ficticios; los `catch` vacíos son limpieza de archivos temporales y lectura tolerante de errores de proveedor. | 4 `catch` revisados |
| 34 Decisiones arquitectónicas (ADR) | **Parcial:** las decisiones viven en los `plan.md` (D-n) pero no hay un índice transversal. | `specs/*/plan.md` |
| 36 Preparación para producción | **No evaluada:** no existe una lista de verificación; `/health` solo responde "ok" sin comprobar dependencias. | `OfficeApiExtensions.cs` |
| 21, 38 Autenticación y multiempresa | **Fuera de alcance por decisión del responsable (2026-11-10):** punto nulo. | specs 001 y 003 |

## Usuarios y actores
Responsable técnico y de operación (consume los registros y las pruebas); agentes de desarrollo.

## Requisitos funcionales
- **RF-001** Cada solicitud al chat recibe un identificador de correlación que aparece en todos los registros de esa solicitud (orquestación, tareas, llamadas a proveedores).
- **RF-002** Cada llamada a un proveedor registra agente, proveedor, modelo, duración y tokens, sin contenido de mensajes ni secretos.
- **RF-007** Los registros de la aplicación se emiten en formato JSON estructurado (una línea por evento) con marca de tiempo, nivel, mensaje y los identificadores de correlación como campos (decisión Q-002).
- **RF-008** La capa de presentación (`Web`) no utiliza ningún tipo, espacio de nombres ni referencia directa de Infrastructure; accede a esas capacidades únicamente a través de contratos definidos en Application (decisión Q-001). Esto incluye el renderizado seguro de Markdown y la configuración de arranque.
- **RF-003** Existen pruebas de contrato que fijan la forma de las respuestas públicas de la API (chat, tareas, agentes, oficinas, estado de modelos, exportación) para detectar cambios incompatibles.
- **RF-004** El estado de salud (`/health`) distingue «la aplicación responde» de «los componentes necesarios están listos» (agentes cargados, base de conocimiento cargada, al menos un proveedor de modelos configurado).
- **RF-005** Existe una lista de verificación de preparación para producción (Art. 36) versionada con el proyecto.
- **RF-006** Las decisiones arquitectónicas de las specs se pueden consultar desde un índice único (Art. 34).

## Requisitos no funcionales
- **RNF-001** Los registros no contienen secretos ni texto de los mensajes de la persona ni de documentos (Art. 22).
- **RNF-002** El cumplimiento de la separación de capas es verificable automáticamente (RF-008) y su incumplimiento rompe la compilación o las pruebas.

## Reglas de negocio
- **RN-001** Una funcionalidad no se considera «lista para producción» por funcionar en desarrollo (Art. 36).
- **RN-002** Ningún requisito de esta especificación cambia el comportamiento visible para la persona usuaria.

## Criterios de aceptación
- **CA-001** Dada una solicitud con tareas y llamadas a proveedores, cuando se revisan los registros, entonces todas sus líneas comparten el mismo identificador (RF-001).
- **CA-002** Dada una llamada a un proveedor, cuando se registra, entonces incluye duración y no incluye el texto del mensaje (RF-002, RNF-001).
- **CA-003** Dado un cambio que elimina o renombra un campo de una respuesta pública, cuando se ejecutan las pruebas, entonces falla una prueba de contrato (RF-003).
- **CA-004** Dado que falta el catálogo de agentes o ningún proveedor está configurado, cuando se consulta el estado de salud, entonces informa «no listo» con el motivo (RF-004).
- **CA-005** Dado el repositorio, cuando se busca la lista de verificación de producción, entonces existe y cada punto es comprobable (RF-005).
- **CA-006** Dado el índice de decisiones, cuando se consulta, entonces enlaza cada decisión de las specs 001–004 (RF-006).
- **CA-007** Dada cualquier salida de registro de la aplicación, cuando se procesa como JSON, entonces cada línea es un objeto JSON válido con marca de tiempo, nivel y mensaje, y los identificadores de correlación aparecen como campos (RF-007).
- **CA-008** Dado el código y los proyectos de `Web`, cuando se revisan sus referencias y su código fuente, entonces no hay referencia directa a Infrastructure ni uso de sus espacios de nombres, y las pruebas lo verifican (RF-008, RNF-002).
- **CA-009** Dado un texto Markdown con contenido inseguro, cuando `Web` lo muestra, entonces se aplican las mismas reglas de la spec 003 (sin imágenes, sin HTML crudo, enlaces seguros) mediante el contrato de Application (RF-008).

## Alcance
**Incluido:** registros correlacionados, con duración y en JSON; pruebas de contrato; salud con comprobaciones; lista de producción; índice de decisiones; separación estricta de `Web` respecto de Infrastructure.
**Excluido:** autenticación/autorización y multiempresa (punto nulo), pruebas de interfaz/Playwright (aplazadas), cambios funcionales visibles, almacenamiento de registros fuera de la aplicación.

## Datos e interfaces
Entradas: solicitudes y llamadas existentes. Salidas: líneas de registro enriquecidas, respuesta de salud ampliada, documentos de verificación. Sin cambios en los contratos públicos existentes (RN-002).

## Supuestos y dependencias
Depende de la spec 003 (seguridad) para la limpieza de mensajes en registros; usa el registro estándar de .NET.

## Riesgos funcionales y casos límite
Un identificador de correlación demasiado largo o proveniente del cliente no debe aceptarse sin validar; registrar demasiado detalle podría exponer contenido (RNF-001).

## Preguntas abiertas
Resueltas el 2026-11-10 (detalle en `clarifications.md`):
- **Q-001** (RESUELTA) `Web` **no** debe depender de Infrastructure: se comunica solo con Application mediante contratos. → RF-008, RNF-002, CA-008.
- **Q-002** (RESUELTA) Formato de registro: **JSON**. → RF-007, CA-007.
- **Q-003** (RESUELTA) Especificación **aprobada** para continuar con clarify y plan.

Resueltas al aprobar el plan: Q-004 correlación generada en el servidor, Q-005 salud solo listo/no listo, Q-006 documentos en `docs/`.
