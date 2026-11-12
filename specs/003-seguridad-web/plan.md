# Plan técnico 003 · Seguridad web

**Maestro:** `.specify/maestros/04_PLAN.md` · **Estado:** implementado (plan retroactivo)

## Resumen y objetivo técnico
Añadir middlewares transversales (cabeceras y guardia de peticiones), saneamiento de salida y de entrada de prompts, límites y actualización de paquetes, sin tocar la lógica de negocio.

## Referencias
`spec.md`; constitución v1.1.0 (Art. 3 con OWASP Top 10, 16, 17, 19, 22, 24, 35).

## Estado actual relevante (evidencia)
CORS restringido, host Web con `AllowedHosts` local, excepciones centralizadas; sondeos de QA demostraron el secuestro de WebSocket, los POST sin cuerpo desde otro origen y las imágenes externas.

## Decisiones de arquitectura
- **D-1** Guardia propia por middleware (origen + cabecera de cliente) en lugar de tokens antifalsificación: la API es JSON y los clientes son propios; el WebSocket no se rige por CORS (alternativa descartada: solo CORS).
- **D-2** Se protegen `/api`, `/hubs` y `/_blazor` (el canal de Blazor también acepta WebSocket).
- **D-3** CSP con `'unsafe-inline'` para script/estilo (requisito de Blazor Server y de la interfaz) pero sin `unsafe-eval` ni fuentes externas.
- **D-4** Saneador de Markdown en `Infrastructure` (`SafeMarkdown`; desde la spec 005 es `SafeMarkdownRenderer` tras `IMarkdownRenderer`) para poder probarlo; la vista solo lo invoca.
- **D-5** Limitador de tasa nativo de ASP.NET Core por IP con política «chat».
- **D-6** Topes de memoria en los almacenes en lugar de una persistencia (Art. 10).
- **D-7** Limpieza de mensajes de proveedor en una utilidad interna (`ErrorText`).

## Diseño propuesto
`SecurityHeadersMiddleware`, `RequestGuardMiddleware` (`UseOfficeSecurity` en ambos hosts); `AddRateLimiter` + `[EnableRateLimiting("chat")]`; `InvalidModelStateResponseFactory` en español; `PromptBuilder.NeutralizeDocumentTags` y regla de seguridad en prompts; `SafeMarkdown`; `ConversationStore.MaxConversations`, `PendingChoiceStore.MaxPending`; `ErrorText.Clean`.

## Flujos
Petición → cabeceras → guardia (origen / cabecera) → CORS → límite de tasa → endpoint. Rechazo → 403 JSON con registro de advertencia. Excedente → 429.

## Contratos
Cabecera `X-Office-Client`; configuración `Office:AllowedOrigins`, `Office:ChatRequestsPerMinute`; el cliente HTTP de la interfaz agrega la cabecera por defecto.

## Datos y persistencia
Ninguna nueva.

## Seguridad y privacidad
Es el objeto del feature. Mapeo OWASP: A01 control de acceso (origen, CSRF), A02/A05 configuración (cabeceras, hosts), A03 inyección (prompt injection, Markdown), A04 diseño inseguro (límites), A06 componentes vulnerables (paquetes), A09 registro (rechazos registrados sin datos).

## Integraciones y proveedores
Sin cambios, salvo el saneado de sus mensajes de error.

## Cambios por proyecto/archivo
- Api: `Middleware/SecurityMiddleware.cs`, `Extensions/OfficeApiExtensions.cs`, `Controllers/Controllers.cs`, `Program.cs`, `appsettings.json`.
- Web: `Program.cs`, `Components/Shared/MarkdownView.razor`, `ATLAS.Web.csproj`.
- Application: `Orchestration/PromptBuilder.cs`, `Services/InMemoryStores.cs`, `Orchestration/KnowledgeWorkflow.Choice.cs`, `Orchestration/OrchestrationService.cs`.
- Infrastructure: `Documents/SafeMarkdown.cs`, `Llm/ErrorText.cs`, `Llm/FreeApiService.cs`, `Anthropic/AnthropicService.cs`.
- Tests: `SecurityTests.cs`, `ATLAS.Tests.csproj` (referencia a Api).

## Pruebas
`SecurityTests` (servidor real en puerto libre para guardia y cabeceras; saneador, prompts, topes, limpieza de errores, elección local). Verificación de paquetes con `dotnet list package --vulnerable`. Sondeos HTTP y comprobación en navegador.

## Despliegue/configuración/rollback
Configurable: orígenes y límite. Los scripts propios deben enviar `X-Office-Client`. Rollback: quitar `UseOfficeSecurity`.

## Riesgos, dependencias y decisiones pendientes
Sin autenticación (Q-001); límite por IP compartido; el limitador no tiene prueba automatizada.
