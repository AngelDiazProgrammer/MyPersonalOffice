# Conexiones a navegadores (Playwright)

Spec: [007](../specs/007-conectividad-navegadores/spec.md) · Plan: [plan](../specs/007-conectividad-navegadores/plan.md) · Estado: [convergencia](../specs/007-conectividad-navegadores/convergence.md)

La oficina puede **operar sitios web por ti** con Playwright (no es una librería de pruebas aquí: es un servicio de back). Tú decides a qué sitios, qué pasos y cuándo aprobar.

## Cómo se usa (caso Gmail)

1. Arranca la app (`dotnet run --project src/ATLAS.Web`) y abre **🌐 Conexiones** (enlace en la cabecera de la oficina, o `/conexiones`).
2. **Agregar sitio**: id `gmail`, nombre `Gmail`, URL `https://mail.google.com/mail/u/0/#inbox`, dominios `google.com, gmail.com`. Como ya existe `playbooks/gmail.md`, se usa ese playbook (si no existiera, se crea una plantilla).
3. **🔐 Iniciar sesión**: se abre el navegador visible; inicias sesión tú (incluido 2FA) y cierras la ventana. La sesión queda en el perfil `browser-data/profiles/gmail`. **No se guarda ninguna contraseña.**
4. En el chat (oficina «Pruebas base de conocimiento»), tras resolver un caso, escribe por ejemplo *«envíalo por correo a jefe@empresa.com»*:
   - El sistema pregunta lo que falta (`¿Para qué es el correo?`).
   - El modelo **redacta** asunto y cuerpo usando la conversación (la solución del caso).
   - Te muestra el **borrador** con una tarjeta: *Aprobar y ejecutar* / *Cancelar*, o puedes escribir «hazlo más corto» y lo redacta de nuevo.
   - Al aprobar, Playwright abre Gmail con tu sesión, escribe y envía. Te informa tiempo, modo y llamadas al modelo.
5. Si fue la primera vez (pasos en texto libre), el sistema **aprendió** el proceso y deja una **propuesta**. En *Conexiones → Playbook* la revisas y la apruebas: desde entonces se ejecuta **sin modelo**, mucho más rápido.

Si un paso falla porque Gmail cambió, el modelo repara solo ese tramo, la ejecución continúa y se propone el playbook actualizado.

## Leer tus últimos 20 correos (caso actual)

1. En «Conexiones» agrega el sitio `gmail` (URL `https://mail.google.com/mail/u/0/#inbox`, dominios `google.com, gmail.com`). Usuario y clave son opcionales y viven **solo en memoria** (se pierden al reiniciar la app).
2. Pulsa **🔐 Iniciar sesión**, entra a mano y cierra la ventana.
3. En el chat escribe, por ejemplo, *«lee mis últimos correos»*. Como el playbook (`playbooks/gmail.md`) solo lee, **se ejecuta sin pedir aprobación** y te muestra una tabla con remitente, asunto, fecha y vista previa.
4. También puedes usar «Conexiones → ▶ Ejecutar» y verás la tabla en el resultado.

El contenido de los correos se muestra pero **no se guarda en el historial** de la conversación (así no llega a ningún modelo en mensajes posteriores). Leer no gasta llamadas a modelos si los selectores siguen funcionando; si Google cambia su página, el paso se repara con Claude y se propone el playbook actualizado.
La redacción y el envío están aparcados: el playbook anterior quedó en `playbooks/ejemplos/gmail-enviar.md`.

## Editar una conexión

En «Conexiones» → **✏️ Editar conexión** puedes cambiar nombre, URL, dominios permitidos, navegador visible/invisible, activa/inactiva, usuario y clave. La clave no se muestra; si la dejas vacía se conserva la actual. El identificador no cambia (nombra el playbook, la sesión del navegador y el archivo de credenciales): para otro identificador, crea una conexión nueva. **🔑 Borrar credenciales** elimina el archivo `.md` de esa conexión.

## Dónde quedó cada cosa

| Qué | Dónde |
|---|---|
| Especificación, plan y tareas | `specs/007-conectividad-navegadores/` |
| Modelos (sitio, playbook, credenciales, resultado) | `src/ATLAS.Application/Browser/BrowserModels.cs` |
| Lectura/escritura de playbooks `.md` | `Application/Browser/PlaybookParser.cs` |
| Seguridad: dominios permitidos, hosts privados, validación de sitios | `Application/Browser/BrowserPolicy.cs` |
| **Ejecutor**: reproducción, exploración con modelo, reparación, grabación y propuesta | `Application/Browser/BrowserRunner.cs` |
| Flujo del chat: preguntar → redactar → aprobar → ejecutar | `Application/Browser/BrowserWorkflow.cs` (se engancha en `Orchestration/OrchestrationService.cs`) |
| Gestión de sitios, playbooks, propuestas, login y ejecución directa | `Application/Browser/BrowserManager.cs` |
| Interfaces (sesión de navegador, almacenes) | `Application/Browser/IBrowserServices.cs` |
| **Playwright** (perfil persistente, filtro de navegación, instantánea de la página) | `src/ATLAS.Infrastructure/Browser/PlaywrightSessionFactory.cs` |
| Sitios en `browser-data/sites.json`; playbooks en `playbooks/*.md` | `Infrastructure/Browser/BrowserStores.cs` |
| Credenciales en `browser-data/credentials/<id>.md` (texto plano, pruebas) | `Infrastructure/Browser/MarkdownCredentialStore.cs` |
| API REST `/api/browser/*` | `Api/Controllers/Controllers.cs` (`BrowserController`) |
| Página «Conexiones» | `src/ATLAS.Web/Components/Pages/Connections.razor` |
| Tarjeta de aprobación en el chat | `Web/Components/Chat/ChatMessageView.razor` |
| Contrato del chat ampliado (`browserApproval`, aditivo) | `Application/DTOs/Dtos.cs` |
| Configuración | sección `Browser` de `configuration/appsettings.json` |
| Playbook de ejemplo | `playbooks/gmail.md` |
| Pruebas | `tests/ATLAS.Tests/Browser*Tests.cs` |

## Formato del playbook

```markdown
---
nombre: Enviar correo
disparadores: enviar correo, mandar correo
---
## Entradas
- destinatario | ¿A quién va dirigido? | correo        ← se pregunta (tipo correo: se toma del mensaje si ya viene)
- motivo | ¿Para qué es el correo?                     ← se pregunta
- asunto | redactar | Asunto breve y claro             ← lo redacta el modelo
## Pasos
1. ir | https://mail.google.com/mail/u/0/#inbox
2. clic | div[gh="cm"] | | Abrir la redacción         ← acción | selector | valor | nota
3. escribir | input[name="to"] | {{destinatario}}
4. Escribe {{asunto}} en el campo Asunto              ← sin «|»: paso libre, lo decide el modelo
5. clic | div[role="button"][aria-label^="Enviar"] | | Enviar | irreversible
```

Acciones: `ir`, `clic`, `escribir`, `teclear`, `esperar`, `seleccionar`, `verificar`. Variables: las entradas (`{{destinatario}}`) y, si las escribes al ejecutar, `{{credencial.usuario}}` / `{{credencial.clave}}`. Un paso `irreversible` nunca se repara solo (para no duplicar un envío).

## Cómo aprende y se repara

- Cada ejecución **graba las acciones reales** que hizo. Si hubo pasos libres (exploración) o reparaciones, escribe `playbooks/<id>.propuesta.md` con pasos estructurados.
- Lo que el modelo escribe de forma literal nunca se aprende: se marca `{{REVISAR}}` y la propuesta **no se puede aprobar** hasta que lo reemplaces por una variable.
- La propuesta **no se activa sola**: la apruebas tú en *Conexiones*. Después, la reproducción no llama al modelo.
- Modos que verás en el resultado: **reproducción** (sin modelo), **exploración** (pasos libres), **reparación** (un paso falló y se rehizo).

## Seguridad

- **Credenciales (solo pruebas)**: las del formulario del sitio se guardan en `browser-data/credentials/<id>.md` en **texto plano**, para que sigan disponibles tras reiniciar. La carpeta está en `.gitignore`; nunca se devuelven por la API (la clave no vuelve al navegador) ni van a registros, eventos, resultados ni modelos. Para producción hay que cifrarlas o usar un gestor de secretos. Las que escribas en «▶ Ejecutar» solo se usan en esa ejecución.
- **Aprobación**: nada se ejecuta sin que apruebes el borrador completo; los pasos `irreversible` se anuncian en la tarjeta.
- **Dominios permitidos**: el navegador solo puede navegar a los dominios del sitio (se aborta cualquier otra navegación, también las de ventanas nuevas); un clic que te saque del alcance detiene la ejecución. Direcciones locales/privadas bloqueadas salvo `Browser:AllowPrivateHosts` (solo desarrollo).
- **Datos hacia modelos**: el borrador y la instantánea de la página se envían con `Sensitive=true`, es decir **solo a Claude** (nunca a las APIs gratuitas). Si no hay `ANTHROPIC_API_KEY`, redactar y explorar no funcionan, pero la reproducción de un playbook aprobado sí.
- **Límites**: máximo de acciones por ejecución, de acciones exploratorias por paso, tiempo total y una ejecución a la vez. Los borradores pendientes viven en memoria con tope y caducidad.
- **Sesiones**: `browser-data/profiles/` guarda sesiones iniciadas; está en `.gitignore`. Trátalo como una credencial.

## Navegador visible o invisible

`Browser:Headless` (global) y la opción por sitio al agregarlo. Por defecto **visible**, para que veas lo que hace; pasa a invisible cuando el proceso sea estable.
Por defecto se usa **Microsoft Edge** ya instalado (`Browser:Channel = msedge`), sin descargar nada. Para usar Chrome pon `chrome`. Para usar el Chromium de Playwright deja el canal vacío y descárgalo una vez:

```bash
pwsh src/ATLAS.Web/bin/Debug/net8.0/playwright.ps1 install chromium
```

## Páginas que borran lo que se escribe

Algunas aplicaciones se vuelven a pintar tras cargar y borran los campos. El servicio lo compensa: tras `ir` espera a que la red se calme, comprueba que cada `escribir` quedó (si no, limpia y teclea tecla por tecla, hasta 3 intentos) y, **antes de cada `clic`**, vuelve a poner los campos que la página haya borrado. Si aun así un campo no retiene el valor, el error dice «el campo no retuvo el valor escrito».

## Limitaciones conocidas

- Gmail y otros sitios pueden detectar la automatización o pedir verificaciones; por eso el inicio de sesión es manual. Los selectores de `playbooks/gmail.md` son un punto de partida (interfaz en español) y **no se han probado contra tu cuenta real**; la autorreparación está pensada para ajustarlos.
- El inicio de sesión manual (`Iniciar sesión`) abre una ventana real y no está cubierto por pruebas automáticas.
- Sin autenticación en la aplicación: quien alcance la API podría pedir ejecuciones con tus sesiones. Úsala solo en local.
- CAPTCHA y 2FA no se automatizan (se resuelven en el inicio de sesión manual).
