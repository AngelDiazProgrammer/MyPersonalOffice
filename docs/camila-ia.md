# Camila IA · guía de uso (spec 009)

Dos oficinas nuevas, **Camila IA UML** y **Camila IA Conversacional**, construidas sobre el export de Dialogflow CX del bot CamilaOS. Decisiones: [plan 009](../specs/009-camila-ia/plan.md).

## 0. Cómo encajan las piezas (spec 011 y 012)
| Pieza | Qué es | Dónde vive |
|---|---|---|
| **Bot** | El bot Camila descrito en un **playbook maestro**: el flujo UML de cada página en pasos ejecutables | `playbooks bots/camila/` |
| **LectorUML** | Agente **ejecutor** (sin modelo). Lee el playbook de un bot y ejecuta su flujo en *Chatbots* | `agents/lector-uml.md` |
| **CamilaIA** | Agente **con modelo** que conversa libremente apoyándose en la base de conocimiento; es quien lleva la conversación | `agents/camila-ia.md` |
| **Base de conocimiento** | Narrativa legible del bot (sin bloques ejecutables), la que lee CamilaIA | `bases de conocimiento/CamilaIA.md` |
| **Notas** | Tono, preguntas frecuentes y avisos escritos a mano; la regeneración nunca las pisa | `playbooks bots/camila/notas.md` |

Los agentes se crean, editan y eliminan en la sección **Agentes** del menú (cada uno con su uso específico). *Chatbots* muestra los bots (clic para abrir el entorno de pruebas, lo ejecuta LectorUML) y *Agentes conversacionales* muestra a CamilaIA.

## 1. Generar los playbooks y la base
```bash
dotnet run --project src/AI.Agent.Office.Web -- --bot-generar
```
(`--camila-generar` sigue funcionando como alias.) Lee `Bots/` y escribe:
- `playbooks bots/camila/bot.md` (maestro), `flujos/NN-<flujo>.md` (un playbook por flujo, con los pasos en orden y su bloque ejecutable), `catalogos/*.md` (intents, entidades, generadores, webhooks, grupos de rutas) y `notas.md` (solo si no existe).
- `bases de conocimiento/CamilaIA.md`: la narrativa, sin bloques.

Es determinista: el mismo export da los mismos archivos. **No edites a mano** los playbooks ni la narrativa (se regeneran): lo que quieras añadir va en `notas.md`. Para otro bot: `--bot-generar <carpeta-export> --id <bot>`.

- Las credenciales del export (cabeceras `x-api-*`, usuario/clave de webhooks) **no van a los archivos**: se guardan en `.env` como `CAMILA_WH_*` (sin pisar las que ya existan) y los playbooks solo citan `{{secret:NOMBRE}}`. `.env.example` recibe los nombres, sin valores.
- Las cabeceras `Authorization: Bearer $session.params.…` no son secretos: el token lo obtiene el propio flujo (webhook de login).
- `Bots/` está en `.gitignore`. **Rota las credenciales** del export original: estuvieron en texto plano.

### Cómo están escritos los playbooks
Pensado para leerse de arriba abajo:
- **Cómo leer este documento**: el orden exacto en que ocurre todo al llegar a una página (📌 asigna → ⚡/✍️ webhook → 🤖 IA → 💬 dice y 🔘 botones → ⌨️ pide datos → 🔀 rutas automáticas → ⏳ espera).
- **Mapa de flujos** (qué pide, qué lee, qué escribe, desde dónde se llega), **webhooks de escritura**, **datos que el bot pide al usuario** y **generadores de IA**.
- Por **flujo**: qué dice al empezar, desde dónde se llega, qué webhooks usa y qué páginas tiene.
- Por **página**: sus pasos numerados, qué pide al usuario (y con qué pregunta), y *qué pasa después*: rutas automáticas, botones (con su texto y evento), frases que coinciden con un intent y «sin coincidencia», cada una con su destino y sus efectos (↳ asigna / llama webhook / dice).
- Por **webhook**: tipo (⚡ lectura / ✍️ escritura), llamada, qué **envía** (campo ← variable), qué **devuelve y guarda** (ruta JSON → variable) y **dónde se usa**.
- Debajo de cada elemento de un playbook va su bloque ` ```json camila:* ` exacto: es lo que ejecuta LectorUML. La narrativa de `CamilaIA.md` es lo mismo sin los bloques.

## 2. Camila IA UML
Muestra el bot **sin reformular**: en cada paso, los textos del bot, la **ficha de la página** (textos originales, botones y su evento, formulario, webhooks con su modo, rutas por intent/evento/condición) y una traza de lo que hizo (webhooks, generadores). No usa modelo de lenguaje.

| Comando | Qué hace |
|---|---|
| botones | avanzan como en el bot |
| texto libre | se compara con los intents de la página (o llena el dato que el bot espera) |
| `/jerarquia` | árbol de flujos y páginas (⚡ = llama a un webhook) |
| `/ir <flujo> › <página>` | salta (búsqueda flexible: `/ir 01 › 001`) |
| `/ficha` · `/vars` · `/reiniciar` | ficha actual · variables de la conversación · empezar de cero |

Los generadores de IA de Dialogflow **no se ejecutan** en el UML (el parámetro queda vacío y la traza lo indica).

## 3. Camila IA Conversacional (agente libre)
**No sigue los flujos del bot.** Conversa libremente y usa `CamilaIA.md` como **conocimiento**, no como guion: no hay menús, pasos ni orden fijo, y no usa el motor de flujos (eso es solo del UML).

Cómo funciona un turno (hasta 6 pasos; en cada uno el modelo elige una acción):
- **responder**: contesta a la persona (termina el turno).
- **buscar**: pide el detalle de un tema a la base (qué dice el bot, qué datos pide, qué opciones ofrece). Máximo 2 por turno; si ya buscó eso, debe contestar con lo que tiene o decir con sinceridad que no sabe.
- **webhook**: consulta o escribe datos reales llamando a los mismos servicios del bot, con los datos que la persona dio (por ejemplo su cédula). Si falta un dato lo pide conversando o llama antes al servicio que lo devuelve.

Siempre en contexto: un **catálogo** corto de los temas de la base y de los servicios (qué hacen, si leen o escriben, qué necesitan y qué devuelven). Lo que el modelo **no decide** (lo aplica el código): hosts permitidos, secretos desde `.env` (el modelo nunca los ve), «faltan datos» antes de llamar, **confirmación explícita de las escrituras** (se muestra el cuerpo exacto y solo un «sí» claro lo envía; sin sí ni no ni siquiera consulta al modelo) y Claude como único modelo (`Sensitive`). Si Claude tarda más de `Camila:ModelTimeoutSeconds` (30 s) lo dice con claridad. Los datos de la conversación viven en memoria.

## 4. Webhooks (Real, con controles)
Configuración en la sección `Camila` de `configuration/appsettings.json`:

- `DefaultWebhookMode`: `Real` (primera prueba) · `Simulated` · `Disabled`. `WebhookModes` lo cambia por webhook, por ejemplo `{ "Guardar_Referidos": "Simulated" }`.
- `AllowedHosts`: únicos hosts (con puerto) a los que el modo Real puede llamar; solo https y sin redirecciones.
- `ConfirmWrites: true`: los webhooks de **escritura** (nombres con guardar, envío, finalizar, actualizar, crear, registrar, enviar, certificad; ampliable con `ExtraWriteWebhooks`) muestran el cuerpo exacto y esperan un **«sí»** explícito. Sin él, no se envía nada.
- Las respuestas se asignan a parámetros según el mapeo del webhook (`$.data[0].x`). Los registros guardan webhook, modo, resultado y tiempo; **nunca** cuerpos, cédulas ni credenciales.
- Hay dos tipos de webhook: **flexible** (cuerpo y mapeo propios) y **estándar** de Dialogflow (recibe el `WebhookRequest` con etiqueta y parámetros de sesión, p. ej. `recuperarSessionWatsonId`); ambos se soportan.
- Sin red corporativa o VPN el modo Real responde con un error comprensible. Los dos webhooks con autenticación de cuenta de servicio de Google (`Obtener_Usuario_Por_Cedula`, `Extraer información candidato de W`) solo envían sus cabeceras y pueden devolver 401/403.

### Modelos gratuitos y Camila
Camila **no usa los modelos gratuitos** (Groq, Cloudflare, Gemini) aunque el modo de la plataforma sea «Gratis primero»: sus llamadas llevan datos de colaboradores y se marcan `Sensitive`, que fuerza Claude. Para permitirlos: `Camila:AllowFreeModels: true` (ten en cuenta que Gemini en capa gratuita puede usar el contenido para mejorar sus productos). Las oficinas de agentes sí los usan.

## 5. Registro de lo que pasa
La sección **Logs** del menú (y el panel de cada entorno de Camila) muestra en vivo: página a la que llegó, ruta tomada, dato recibido (solo el nombre), webhook (modo, resultado, tiempo), generador y llamadas a modelos. Nunca muestra cédulas, credenciales, cuerpos ni lo que escribe la persona.

## 6. Datos de la conversación
La cédula y cualquier dato que el flujo pida se entregan **en la conversación** y viven solo en memoria (variables de sesión); se descartan al reiniciar, al caducar (`SessionMinutes`) o al cerrar la app. Nada se escribe en disco, y el texto de la persona no entra a la bitácora de actividad en estas oficinas.
