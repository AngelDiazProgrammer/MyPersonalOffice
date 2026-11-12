# Informe de convergencia 007 · Conectividad a navegadores

**Maestro:** `.specify/maestros/08_CONVERGE.md` · **Fecha:** 2026-11-10 · **Constitución:** v1.1.0

## Estado de convergencia: COMPLETO (con observaciones)

## Resumen ejecutivo
Los criterios CA-001..CA-010 están implementados y cubiertos por pruebas automáticas, incluida una ejecución de extremo a extremo con un navegador real (Microsoft Edge vía Playwright) contra una página local. Quedan sin verificar automáticamente el inicio de sesión manual y el uso contra una cuenta real de Gmail (requieren la sesión de la persona).

## Matriz criterio → implementación → prueba → estado
| Criterio | Implementación | Prueba | Estado |
|---|---|---|---|
| CA-001 Alta/edición/baja de sitios | `BrowserManager`, `BrowserPolicy`, `JsonBrowserSiteStore`, `BrowserController` | `BrowserStoreAndManagerTests`, `BrowserPolicyTests`; API comprobada a mano en la app real | Verificado |
| CA-002 Playbook parseado y validado | `PlaybookParser` | `PlaybookTests` (incl. «todos los playbooks del repositorio son válidos») | Verificado |
| CA-003 Preguntar, redactar, mostrar y esperar aprobación | `BrowserWorkflow` | `BrowserWorkflowTests` (nada se ejecuta sin aprobar; cancelar; editar) + flujo real en la interfaz | Verificado |
| CA-004 Ejecución en orden con entradas | `BrowserRunner` | `BrowserRunnerTests.Replay_runs_every_step...` | Verificado |
| CA-005 Dominios no permitidos bloqueados | `BrowserPolicy`, `BrowserRunner`, filtro de rutas en `PlaywrightSessionFactory` | `BrowserRunnerTests` + `BrowserEndToEndTests.The_real_browser_cannot_navigate...` | Verificado |
| CA-006 Exploración → propuesta sin credenciales ni literales | `BrowserRunner` (grabación, `Mask`) | `BrowserRunnerTests.An_exploration_step...`, `What_the_model_types_literally...` | Verificado |
| CA-007 Reparación de un paso | `BrowserRunner.RunStepAsync` | `BrowserRunnerTests.A_failing_step_is_repaired...`, `An_irreversible_step_that_fails_is_never_repaired` | Verificado |
| CA-008 Aprobar la propuesta → sin modelo | `BrowserManager.ApproveProposalAsync` | `BrowserRunnerTests.Approving_the_proposal...`, `A_proposal_with_values_to_review_cannot_be_approved`; E2E con reproducción sin llamadas al modelo | Verificado |
| CA-009 Credenciales nunca en registros/resultados/modelo | `BrowserCredentials`, `BrowserRunner.Describe/Resolve` | `BrowserRunnerTests.Credentials_are_typed_but_never_logged...`, `The_run_log_never_contains...`, `BrowserStoreAndManagerTests.The_sites_file_never_contains_credentials` | Verificado |
| CA-010 Extremo a extremo con navegador real | `PlaywrightSessionFactory` | `BrowserEndToEndTests` (3 pruebas: playbook completo con envío, bloqueo de navegación, aprendizaje + reproducción sin modelo) | Verificado |

## Ampliación 2026-11-10 (RF-013..RF-015)
| Criterio | Prueba | Estado |
|---|---|---|
| CA-011 Extraer filas / solo lectura sin aprobación | `BrowserReadingTests`, `BrowserEndToEndTests.The_real_browser_reads_the_last_20_rows...` (navegador real) | Verificado |
| CA-012 Credenciales del sitio en `.md` (sobreviven al reinicio) | `BrowserCredentialFileTests`, `BrowserSiteCredentialsTests` | Verificado |
| CA-014 Edición completa de la conexión | `BrowserCredentialFileTests.Editing_a_connection...`; formulario comprobado en la app real | Verificado |
| CA-013 Contenido leído fuera del historial | `BrowserReadingTests.A_read_only_playbook_runs_at_once...` | Verificado |
| Selectores de Gmail (`tr.zA`, `.bog`, `.y2`, `.xW`) | — | **Sin verificar contra Gmail real** |

## Estado de tareas
Completadas: T-001..T-013.

## Desviaciones del plan
- No se escribieron pruebas automáticas del controlador (`BrowserController`); se verificó contra la app en ejecución (alta, duplicado, host privado, cabecera CSRF, playbook, flujo de chat) y por los contratos (`ContractTests`: `BrowserApprovalDto`, `BrowserSiteDto`, `BrowserRunResultDto`, `PlaybookDto`, `ChatResponseDto`).
- La instalación del navegador no se automatiza: por defecto se usa Edge ya instalado (`Browser:Channel`); el comando para el Chromium de Playwright está documentado (RF-012 se cumple con el mensaje de error y la documentación).

## Pruebas
318 aprobadas, 0 fallidas, 2 omitidas (integración con Anthropic sin clave, preexistentes). Build sin errores.
En la app real: `/health/ready` 200; alta de sitio con CSRF (403 sin cabecera), duplicado y host privado rechazados con mensaje claro; flujo de chat completo con un proveedor real (pregunta → pregunta → borrador → tarjeta → cancelar) y página `/conexiones` renderizada.

## Seguridad y riesgos residuales
- **Credenciales en texto plano** en `browser-data/credentials/*.md` (decisión para pruebas): cualquiera con acceso a esa carpeta o a la API local puede usarlas. Ignoradas por git. No apto para producción.
- Sin autenticación en la app (decisión vigente): quien alcance la API podría ejecutar playbooks con las sesiones guardadas. Solo uso local.
- `browser-data/profiles/` guarda sesiones iniciadas (ignorado por git).
- Redactar y explorar envían contenido a Claude (`Sensitive=true`): requiere `ANTHROPIC_API_KEY`.
- Los nombres de host que resuelven a direcciones privadas no se detectan (sin DNS); solo se bloquean literales y sufijos locales.

## Deuda técnica
- Sin pruebas de interfaz para la página «Conexiones» ni para la tarjeta de aprobación (E2E aplazado por decisión).
- Los selectores de `playbooks/gmail.md` son un punto de partida sin verificar contra una cuenta real.

## Pendientes atómicos
- **PEND-001** — Probar `playbooks/gmail.md` con tu cuenta (inicio de sesión manual + un envío real) y aprobar la propuesta aprendida. Impacto: confirma el caso guía en producción. Artefacto: `docs/conexiones-navegador.md`.
- **PEND-002** — Cubrir con prueba automática el inicio de sesión manual (ventana real). Impacto: regresiones en el perfil persistente. Artefacto: `PlaywrightSessionFactory.LoginManuallyAsync`.
- **PEND-003** — Revisar y, si se aprueba, redactar la enmienda constitucional «acciones con efectos externos» (spec 007, sección «Propuesta de enmienda»). Artefacto: constitución (Art. 40, la aprueba la persona responsable).
- **PEND-004** — Pruebas de interfaz de «Conexiones» y la tarjeta de aprobación. Artefacto: spec 007.

## Recomendación de siguiente paso
Probar el caso Gmail con tu sesión (PEND-001) en modo visible; cuando el proceso quede estable y aprobada la propuesta aprendida, pasar el sitio a navegador invisible.
