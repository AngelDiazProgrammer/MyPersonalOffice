# Plan técnico 010 · Plataforma

**Maestro:** `.specify/maestros/04_PLAN.md` · **Estado:** implementado

## Resumen y objetivo técnico
Autenticación por cookie, menú lateral con secciones, almacén de logs en memoria y entorno propio de Camila, reutilizando los estilos y la oficina existentes.

## Referencias
`specs/010-plataforma/spec.md`; constitución Art. 3, 19, 20, 21, 22.

## Decisiones de arquitectura
- **D-1** Autenticación por **cookie** (`AddAuthentication().AddCookie`) en los dos hosts, con `FallbackPolicy` que exige sesión: nada queda abierto por olvido. Lo público se marca con `AllowAnonymous` (`/login`, `/auth/*`, `/health`).
- **D-2** El **formulario de acceso es HTML mínimo** servido por un endpoint (no un componente Blazor): la interfaz entera corre en modo interactivo sin prerenderizado y una cookie solo puede fijarse en una petición HTTP normal. Lleva token antifalsificación.
- **D-3** Usuarios en `configuration/usuarios.md` (tabla Markdown) con **PBKDF2-SHA256** (210 000 iteraciones, sal por usuario) y comparación en tiempo constante. El comando `--hash-contrasena` genera una línea nueva.
- **D-4** Esquema de autenticación **interno** (`X-Office-Internal`) para las llamadas de la interfaz a su propia API y al hub: token aleatorio por proceso (o `Office:InternalToken` si la API corre aparte). No sale al navegador porque el circuito de Blazor corre en el servidor.
- **D-5** Límite de intentos en memoria por usuario+origen (5 fallos → 5 min).
- **D-6** `InMemoryLogStore` + `InMemoryLoggerProvider` (anillo de 1 000 entradas) captura los registros `AI.Agent.*` ≥ Información; clasifica el origen por categoría y toma la conversación del ámbito. `GET /api/logs?after=` devuelve solo lo nuevo.
- **D-7** Camila registra por turno su traza (sin valores) mediante `ILogger`.
- **D-8** Menú en `MainLayout`; páginas `Oficinas`, `Chatbots`, `Agentes`, `Conexiones`, `Logs`. `WorkspaceDto` gana `Kind` para agrupar. Las oficinas de agentes conservan `OfficeSpace` (el contenido anterior de Home); Camila usa `CamilaSpace` (chat + registro filtrado).
- **D-9** `Camila:AllowFreeModels` (falso por defecto) decide si las llamadas de Camila llevan `Sensitive`.

## Pruebas
Hash y verificación, almacén de usuarios, bloqueo por intentos, autorización (sin sesión → 401/redirección; con token interno → 200), almacén de logs (tope, filtros, sin datos) y contratos. Verificación visual en la app real.
