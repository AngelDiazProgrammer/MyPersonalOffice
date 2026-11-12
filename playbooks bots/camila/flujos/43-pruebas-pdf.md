---
bot: camila
flow: Pruebas pdf
---
## Flujo · Pruebas pdf

**Qué dice al empezar:** «por favor enviame un pdf»

**Se llega desde:**
- flujo Default Start Flow ← intent «PDF pruebas»

**Webhooks que usa:** ⚡ recuperarSessionWatsonId

**Pide al usuario:** `pdf` (enviar pdf)

**Páginas:**
- Mostrar pdf 
- enviar pdf — ⌨️ pide datos

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «enviar pdf»  
  ↳ ⚡ Llama al webhook **recuperarSessionWatsonId** [session-watson] — lectura
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"Pruebas pdf","routes":[{"condition":"true = true","targetPage":"enviar pdf","fulfillment":{"messages":[],"setParams":[],"webhook":"recuperarSessionWatsonId","tag":"session-watson","generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · Pruebas pdf › Mostrar pdf

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

```json camila:page
{"flow":"Pruebas pdf","page":{"name":"Mostrar pdf","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · Pruebas pdf › enviar pdf

**Al entrar, en este orden:**

1. 💬 Dice: «por favor enviame un pdf»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `pdf` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.pdf != null` → página «Mostrar pdf»

```json camila:page
{"flow":"Pruebas pdf","page":{"name":"enviar pdf","entry":{"messages":[{"type":"text","texts":["por favor enviame un pdf"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"pdf","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.pdf != null","targetPage":"Mostrar pdf","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

