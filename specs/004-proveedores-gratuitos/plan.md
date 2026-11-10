# Plan técnico 004 · Proveedores de modelos

**Maestro:** `.specify/maestros/04_PLAN.md` · **Estado:** implementado (plan retroactivo)

## Resumen y objetivo técnico
Un enrutador único que delega en un servicio genérico de APIs gratuitas compatibles con OpenAI y en el cliente de Claude, con pausas por proveedor, privacidad por sensibilidad y diagnóstico.

## Referencias
`spec.md`; constitución v1.1.0 (Art. 9, 15, 20, 25, 26).

## Estado actual relevante (evidencia)
`IAnthropicService` como contrato único (Application); `AnthropicService` con reintentos y caché de prompt; modos `FreeFirst/ClaudeOnly/FreeOnly`.

## Decisiones de arquitectura
- **D-1** Un solo `FreeApiService` para todos los gratuitos; los proveedores son entradas de configuración (alternativa descartada: un servicio por proveedor — Art. 9).
- **D-2** `RoutingLlmService` implementa `IAnthropicService`, `ILlmRouterState` y `ILlmStatusService`, de modo que Application no conoce proveedores concretos.
- **D-3** Pausas por proveedor según el tipo de fallo (429 → tiempo indicado; 401/403/404 → 10 min; 5xx/conexión → 30 s).
- **D-4** `Sensitive` fuerza Claude; los gratuitos nunca reciben solicitudes sensibles.
- **D-5** Cualquier excepción de lectura de un proveedor se convierte en fallo de ese proveedor (no en error de la solicitud).
- **D-6** Diagnóstico por endpoint `POST /api/llm/test` con modelo opcional.
- **D-7** Comprobación de modelos al arrancar en segundo plano, informativa, tolerante al prefijo `models/`.
- **D-8** Gemini mediante su capa compatible con OpenAI, modelo `gemini-flash-lite-latest` por estabilidad (el `gemini-3.8-flash` respondió 503 por alta demanda y el 2.5 fue retirado para cuentas nuevas).

## Diseño propuesto
`FreeApiService` (`TrySendAsync`, `SendToAsync`, `PostAsync`, `TestProviderAsync`, `ValidateModelsAsync`), `RoutingLlmService`, `AnthropicService`, `FreeProviderCheck`, `LlmOptions`/`FreeProviderOptions`, `ITokenUsageTracker` por proveedor, `LlmController` (`status`, `mode`, `test`).

## Flujos
Solicitud → modo → (si sensible: solo Claude) → gratuitos en orden → respuesta o escalado a Claude. Alternativos: modo solo gratis; Claude forzado; sin claves. Error: sin proveedor disponible → mensaje que explica qué configurar.

## Contratos
`LlmRequest`, `LlmResponse` (con `Provider`), `LlmStatusDto`, `FreeProviderStatusDto`, `LlmProviderTestDto/RequestDto`; configuración `Llm:FreeProviders` y variables `GROQ_API_KEY`, `CLOUDFLARE_API_TOKEN`, `CLOUDFLARE_ACCOUNT_ID`, `GEMINI_API_KEY`.

## Datos y persistencia
Sin persistencia; el consumo vive en memoria.

## Seguridad y privacidad
Claves solo del entorno (`.env` ignorado por git); nunca registradas; mensajes de error limpios (spec 003); la privacidad depende de `Sensitive` y de la bandera de la spec 002.

## Integraciones y proveedores
Tiempos máximos por proveedor, pausas, sin reintentos ilimitados; Claude reintenta sin `temperature`/`tool_choice` forzado una vez y lo recuerda por modelo.

## Cambios por proyecto/archivo
- Application: `Interfaces/ILlmServices.cs`, `Interfaces/LlmModels.cs`, `DTOs/Dtos.cs`, `Services/InMemoryStores.cs` (`TokenUsageTracker`).
- Infrastructure: `Llm/FreeApiService.cs`, `RoutingLlmService.cs`, `LlmOptions.cs`, `FreeProviderCheck.cs`, `ErrorText.cs`, `Anthropic/AnthropicService.cs`.
- Api: `Controllers/Controllers.cs` (`LlmController`).
- Web: `Home.razor` (modo y consumo por proveedor).
- Configuración: `configuration/appsettings.json`, `.env.example`.

## Pruebas
`LlmRoutingTests` (29), `TokenUsageTests`, `InfrastructureTests` (reintentos de Claude, `temperature`, `tool_choice`); verificación real: prueba de Gemini, solicitud simple y flujo completo con solo Gemini disponible.

## Despliegue/configuración/rollback
Claves en `.env`; quitar la clave de un proveedor lo desactiva; `Llm:Mode` fija el modo inicial.

## Riesgos, dependencias y decisiones pendientes
Q-001 y Q-002 de la especificación; DNS corporativo; cupos y nombres de modelos cambiantes.
