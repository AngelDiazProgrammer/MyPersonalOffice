# Plan técnico 007 · Conectividad a navegadores

**Maestro:** `.specify/maestros/04_PLAN.md` · **Estado:** aprobado para implementar

## Resumen y objetivo técnico
Añadir un servicio de automatización de navegador con Playwright, desacoplado tras interfaces, con playbooks Markdown, aprobación humana, aprendizaje y autorreparación, integrado al chat y al front.

## Referencias
`specs/007-conectividad-navegadores/spec.md`; constitución v1.1.0 (Art. 10, 11, 12, 29); specs 004 (`Sensitive`) y 005 (capas).

## Estado actual relevante (evidencia)
`OrchestrationService` ya mantiene estado pendiente por conversación (`IPendingChoiceStore`) para el flujo de la base de conocimiento; las interfaces de Application se implementan en Infrastructure y se componen en Api; el front solo habla con la API.

## Decisiones de arquitectura
- **D-1** `IBrowserSession`/`IBrowserSessionFactory` (Application) ocultan Playwright; la implementación vive en Infrastructure. Permite probar el ejecutor con un navegador simulado.
- **D-2** `BrowserRunner` (Application) ejecuta playbooks: reproducción determinista, exploración con modelo y reparación por paso; hace cumplir dominios permitidos y límites.
- **D-3** Perfil persistente por sitio (`browser-data/profiles/<id>`) = login manual una vez; sin guardar credenciales.
- **D-4** Credenciales en memoria solo durante la ejecución (`BrowserCredentials`, sin `ToString`, nunca en registros/eventos/prompts).
- **D-5** Playbook = Markdown con `## Entradas` y `## Pasos` en formato de líneas `acción | objetivo | valor | nota`; una línea sin `|` es un paso exploratorio.
- **D-6** Aprendizaje: el ejecutor graba las acciones reales; si hubo exploración/reparación escribe `<id>.propuesta.md` (valores como `{{variable}}`); aprobar la propuesta la promueve.
- **D-7** `BrowserWorkflow` (Application) es una máquina de estados por conversación (preguntar → redactar → aprobar → ejecutar) con almacén en memoria acotado; el chat la consulta antes del flujo de conocimiento.
- **D-8** Lo que ve el modelo (borrador, instantánea de página) se envía con `Sensitive=true` (solo Claude).
- **D-9** Sitios en `browser-data/sites.json` (ignorado por git); playbooks en `playbooks/` (versionados, sin secretos).
- **D-10** El front tiene la página `/conexiones` y una tarjeta de aprobación en el chat; las credenciales se escriben en campos de contraseña del front, nunca en el chat.

## Diseño propuesto
Application/Browser: modelos, `PlaybookParser`, `BrowserRunner`, `BrowserWorkflow`, opciones. Infrastructure/Browser: `JsonBrowserSiteStore`, `MarkdownPlaybookStore`, `PlaywrightSessionFactory`. Api: `BrowserController`. Web: `Connections.razor`, tarjeta de aprobación y cliente.

## Flujos
Chat: disparador → preguntas → borrador → aprobación (botón + credenciales opcionales) → ejecución → resultado (+ propuesta si aprendió).
Aprendizaje: paso libre → exploración (instantánea → acción JSON → ejecutar) → grabación → propuesta → aprobación → reproducción.

## Riesgos y mitigaciones
Sitios que detectan automatización (login manual + perfil persistente); cambios de la web (reparación); acciones irreversibles (aprobación); fuga de datos a modelos (`Sensitive`); navegación fuera de alcance (dominios permitidos y filtro de rutas).
