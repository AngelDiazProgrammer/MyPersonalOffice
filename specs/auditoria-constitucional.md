# Anexo · Auditoría del código contra la constitución v1.1.0

**Fecha:** 2026-11-10 · **Alcance:** los 42 artículos, contrastados con el repositorio real (Art. 41) · **Naturaleza:** informativa; los agentes proponen, no aprueban (Art. 29, 40).
**Estado de la base:** `dotnet build` sin advertencias ni errores; 253 pruebas aprobadas, 0 fallidas, 2 omitidas (Anthropic sin clave); sondeos de seguridad 34/34.

Leyenda: **Cumple** (con evidencia) · **Cumple con observación** · **Parcial** · **Aplazado** (decisión del responsable) · **N/A** (no aplica hoy).

## Resumen
| Estado | Artículos |
|---|---|
| Cumple | 1, 2, 4, 5, 6, 7, 8, 10, 12, 15, 17, 18, 22, 23, 25, 26, 27, 28, 30, 32, 33, 39, 41, 42 |
| Cumple con observación | 3, 9, 13, 14, 16, 19, 24, 35 |
| Parcial | 11, 31, 34, 36, 37 |
| Aplazado por decisión | 20 (privacidad por proveedor), 21 y 38 (autenticación, multiusuario) |
| Requiere acción del responsable | 29 y 40 (estado de aprobación de la constitución, OBS-01) |

Ninguna observación bloquea el uso actual (local, una persona, datos ficticios). Sí impiden declarar el producto «listo para producción» (Art. 36), lo cual ya consta en `docs/preparacion-produccion.md`.

## Auditoría por artículo
| Art. | Estado | Evidencia / observación |
|---|---|---|
| 1 Orientación empresarial | Cumple | Capas, pruebas, documentación, límites de consumo; decisiones justificadas en los `plan.md`. |
| 2 Arquitectura antes que improvisación | Cumple | Cambios 003–005 pasaron por spec/plan; `ArchitectureTests` impide dependencias nuevas indebidas. |
| 3 Seguridad desde el diseño | Cumple con observación | Mínimo privilegio, validación de entradas, cabeceras, guardia de origen, límite de tasa, saneador de Markdown, errores limpios (spec 003; 34 sondeos). Sin control de acceso por usuario (Art. 21, aplazado). Sin barrido de secretos del historial de git (OBS-10). |
| 4 Evolución controlada | Cumple | Los DTO públicos están fijados por pruebas de contrato; cambios con spec. |
| 5 Responsabilidad humana | Cumple | Los agentes no ejecutan comandos ni acciones externas; solo producen texto/documentos; la persona decide (p. ej. elección de solución). |
| 6 Arquitectura por capas | Cumple | `Domain`, `Application`, `Infrastructure`, `Api` (composición), `Web` (presentación). |
| 7 Reglas de dependencia | Cumple | Matriz de referencias verificada por `ArchitectureTests`; `Web` no ve `Infrastructure`; sin ciclos. |
| 8 Tecnologías | Cumple | C#/.NET 8, Blazor, REST + SignalR, agentes en Markdown. three.js (escena 3D) justificado por la spec 006. |
| 9 Independencia tecnológica | Cumple con observación | Proveedores tras `IAnthropicService`/`RoutingLlmService`; Application no conoce proveedores concretos. **OBS-02:** el nombre `IAnthropicService` y el campo público `AnthropicConfigured` contienen el nombre de un proveedor; el comportamiento es independiente, el nombre no. Renombrar cambia un contrato público (requiere spec). |
| 10 Persistencia | Cumple | Sin base de datos, estado en memoria con topes; cualquier persistencia exigirá spec. |
| 11 Agentes especializados | Parcial | **OBS-03:** solo 3 de 10 agentes (`developer`, `incident-responder`, `kb-researcher`) tienen sección explícita de límites o reglas; la mayoría no declara condiciones de finalización, manejo de errores ni reglas de seguridad propias. La regla común contra inyección de instrucciones se aplica por código (`PromptBuilder`), no en cada archivo. |
| 12 Orquestador | Cumple | Decisión estructurada validada, reintentos controlados, consolidación, contexto acotado (spec 006; `OrchestrationTests`). |
| 13 Contratos entre agentes | Cumple con observación | Las tareas llevan id, agente, objetivo, dependencias, estado, resultado y error (`AgentTaskDto`, decisión con JSON Schema). No hay «agente emisor» explícito porque solo el Orquestador delega. |
| 14 Definición de agentes | Cumple con observación | Versionados y sin credenciales; cargados y validados (`ConfigurationAndValidationTests`). Ver OBS-03 sobre consistencia de contenido. Sin cambios de agentes pendientes de spec. |
| 15 Proveedores de IA | Cumple | Anthropic, Groq, Cloudflare y Gemini por configuración independiente, tiempos de espera, pausas por límite, errores clasificados, registro de uso y duración. |
| 16 Respuestas de IA | Cumple con observación | Contenido de modelos tratado como no confiable (Markdown sin HTML ni imágenes, enlaces filtrados); verificación de hechos en el flujo de conocimiento. Para el resto de agentes la validación depende de la persona. |
| 17 Seguridad de agentes y herramientas | Cumple | Sin ejecución de comandos ni acceso a archivos por parte de agentes; PDF tratado como datos y etiquetas neutralizadas; descargas por id; sondeos de subida y exportación. |
| 18 Operaciones sensibles | Cumple | No hay operaciones con impacto externo; la elección de solución requiere acción de la persona. |
| 19 Secretos | Cumple con observación | `.env` ignorado y no versionado; `.env.example` sin claves. **OBS-10:** no se hizo un barrido de secretos en el historial de git. |
| 20 Protección de información | Aplazado | `Sensitive` fuerza Claude y los PDF no van a gratuitos. Falta la bandera de privacidad por proveedor (spec 004 T-014). La capa gratuita de Gemini puede usar el contenido: solo datos ficticios. |
| 21 Autenticación y autorización | Aplazado | Punto nulo por decisión (2026-11-10). Mitigación actual: guardia de origen, límite de tasa, `AllowedHosts` local. |
| 22 Registro y auditoría | Cumple | JSON, `CorrelationId`/`ConversationId`/`TaskId`/`AgentId`, proveedor, duración, tokens, errores; sin contenido (`LoggingTests`). |
| 23 Calidad del código | Cumple | Español, `#region` por funcionalidad, responsabilidades separadas, extensiones de composición. Sin análisis estático automatizado. |
| 24 Pruebas | Cumple con observación | Unitarias, integración (servidor real), contrato, seguridad (OWASP), arquitectura, registros, salud, documentación, regresión. **OBS-04/05:** sin pruebas de interfaz (aplazadas) y sin evaluaciones del comportamiento de agentes con un modelo real (las pruebas usan modelos simulados). |
| 25 Manejo de errores | Cumple | Reintentos acotados, errores clasificados, sin valores ficticios; los `catch` vacíos son limpieza de temporales y lectura tolerante de errores de proveedor (4 revisados). |
| 26 Rendimiento y recursos | Cumple | Concurrencia limitada, tiempo límite y cancelación por tarea, historial acotado, topes de memoria, modelos gratuitos primero. |
| 27 SDD | Cumple | Specs 001–006 (001–004 y 006 retroactivas, declarado en cada una; 005 con flujo completo). |
| 28 Flujo obligatorio | Cumple | Cada spec tiene spec, plan, tareas, análisis y convergencia; la 005 además aclaraciones. |
| 29 Aprobación de cambios | Cumple | Spec y plan 005 aprobados expresamente antes de implementar. Las retroactivas declaran que no sustituyen una aprobación que no existió. Ver OBS-01. |
| 30 Trazabilidad | Cumple | Matrices criterio → implementación → prueba en cada `convergence.md`. |
| 31 Diseño de interfaz | Parcial | **OBS-04:** estados de carga y error presentes, pero solo hay 4 usos de `aria-`/`role`/`alt`/`label` en toda la interfaz y no existe revisión de accesibilidad (teclado, contraste, lector de pantalla) ni pruebas de interfaz. |
| 32 Transparencia de ejecución | Cumple | Panel de tareas, actividad en tiempo real, estados del agente por eventos reales, errores visibles (spec 006, CA-013). |
| 33 Documentación técnica | Cumple | README, `docs/`, specs; código documentado en español. Los contratos están fijados por instantáneas y descritos en README. |
| 34 Decisiones arquitectónicas | Parcial | **OBS-08:** índice único en `docs/decisiones.md` y decisiones D-n en cada plan, pero no todas listan contexto, alternativas y consecuencias con el formato del artículo (algunas sí, p. ej. 001 D-1). |
| 35 Gestión de dependencias | Cumple con observación | `dotnet list package --vulnerable --include-transitive` y `--deprecated`: sin resultados en los 6 proyectos. **OBS-07:** QuestPDF se usa con licencia Community; revisar que las condiciones apliquen si el producto se comercializa. |
| 36 Preparación para producción | Parcial | Lista y `/health/ready` existen. **OBS-09:** la propia lista marca pendientes (TLS, autenticación, envío de registros, copias de seguridad); el producto **no** está listo para producción, y así consta. |
| 37 Despliegue y configuración | Parcial | Configuración externa (`appsettings`, `.env`). **OBS-06:** no hay pipeline de CI/CD (no existe `.github/`), ni configuración de producción separada, ni procedimiento de despliegue reproducible. |
| 38 Escalabilidad organizacional | Aplazado | El diseño por contratos y configuración no impide añadir usuarios u organizaciones, pero hoy el estado (oficina activa, conversaciones) es global al proceso. Se tratará como requisito explícito cuando se incorpore. |
| 39 Jerarquía normativa | Cumple | Las specs no contradicen la constitución; las enmiendas 1.1.0 siguen el registro formal. |
| 40 Modificación de la constitución | Cumple con acción pendiente | Enmiendas registradas con artículo, justificación, consecuencias y aprobación. Ver OBS-01. |
| 41 Responsabilidad de los agentes | Cumple | Inspección del código antes de modificar, validaciones ejecutadas, alcance respetado, riesgos informados. |
| 42 Definición de terminado | Cumple | Cada spec cierra con convergencia; pendientes declarados (aplazados) en lugar de ocultos. |

## Observaciones y propuestas
Ninguna se ha implementado: cada una exige decisión del responsable (Art. 29, 40, 41).

| Id | Artículo | Observación | Propuesta |
|---|---|---|---|
| OBS-01 | 29, 40 | La cabecera de la constitución sigue diciendo «Estado: Propuesta para aprobación» aunque la enmienda 1.1.0 consta aprobada. | El responsable confirma la aprobación y actualiza el estado (no lo modifico yo). |
| OBS-02 | 9 | Nombres de proveedor en contratos (`IAnthropicService`, `AnthropicConfigured`). | Spec de renombrado a `ILlmService`/`LlmConfigured` con migración de contrato. Riesgo medio por ser contrato público. |
| OBS-03 | 11, 14 | Los agentes no declaran de forma uniforme límites, finalización, manejo de errores y reglas de seguridad. | Spec de «estándar de definición de agente» + validador en `ConfigurationAndValidationTests`. |
| OBS-04 | 31, 24 | Accesibilidad no evaluada; sin pruebas de interfaz. | Revisión de accesibilidad y pruebas de interfaz (retoma las pruebas E2E aplazadas). |
| OBS-05 | 24 | Sin pruebas de comportamiento de agentes con modelo real. | Evaluaciones opcionales (omitidas sin clave) para los flujos críticos. |
| OBS-06 | 37 | Sin CI/CD ni configuración de producción separada. | Spec de CI (compilar, probar, `--vulnerable`) y de despliegue. |
| OBS-07 | 35 | Licencia Community de QuestPDF. | Confirmar elegibilidad antes de cualquier uso comercial. |
| OBS-08 | 34 | Decisiones sin formato ADR uniforme. | Normalizar las D-n a contexto/alternativas/consecuencias al tocar cada spec. |
| OBS-09 | 36 | Pendientes de producción declarados. | Spec de endurecimiento para producción (TLS, autenticación, registros, respaldo). |
| OBS-10 | 3, 19 | Sin barrido de secretos del historial. | Ejecutar una herramienta de barrido de secretos sobre el historial por parte del responsable. |

## Cómo se verificó
Lectura completa de la constitución; inspección de referencias entre proyectos, de `Application` y `Web`, de los 10 agentes y del `PromptBuilder`; `dotnet build`, `dotnet test`, `dotnet list package --vulnerable --include-transitive` y `--deprecated`; ejecución real de la aplicación con `/health/ready` y los 34 sondeos. No se revisó el historial de git en busca de secretos ni se evaluó la accesibilidad con herramientas.
