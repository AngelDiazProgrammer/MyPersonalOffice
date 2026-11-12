---
bot: camila
flow: 08.Te escuchamos
---
## Flujo · 08.Te escuchamos

**Qué dice al empezar:** «Este es un espacio para que compartas tus opiniones, sugerencias o comentarios de forma escrita y contribuyas a nuestro propósito de Mejorar Vidas.    ✍️ Déjano…»

**Se llega desde:**
- Default Start Flow › 05.Menu_Principal ← evento `te_escuchamos`
- flujo (global) ← intent «route_TeEscuchamos»
- flujo Default Start Flow ← intent «route_TeEscuchamos»

**Webhooks que usa:** ✍️ Guardar Comentario Te Escuchamos · ⚡ IA Generativa Camila - General

**Pide al usuario:** `comentario` (04. Capturar comentario)

**Páginas:**
- 00. Dejar algun comentario 
- 01. Decision Dejar algun comentario 
- 02. Algo mas 
- 03. Capturar datos 
- 04. Capturar comentario — ⚡ IA Generativa Camila - General · ⌨️ pide datos
- 04.1 Responder comentario — ✍️ Guardar Comentario Te Escuchamos
- 05. Despedida 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.teEscuchamos = true` → página «00. Dejar algun comentario»  
  ↳ ⚡ Llama al webhook **IA Generativa Camila - General** [consultaWeb] — lectura
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"08.Te escuchamos","routes":[{"condition":"$session.params.teEscuchamos = true","targetPage":"00. Dejar algun comentario","fulfillment":{"messages":[],"setParams":[],"webhook":"IA Generativa Camila - General","tag":"consultaWeb","generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 08.Te escuchamos › 00. Dejar algun comentario

**Al entrar, en este orden:**

1. 💬 Dice: «Este es un espacio para que compartas tus opiniones, sugerencias o comentarios de forma escrita y contribuyas a nuestro propósito de Mejorar Vidas.    ✍️ Déjanos tu mensaje por escrito y ayúdanos a se…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «01. Decision Dejar algun comentario»

```json camila:page
{"flow":"08.Te escuchamos","page":{"name":"00. Dejar algun comentario","entry":{"messages":[{"type":"text","texts":["Este es un espacio para que compartas tus opiniones, sugerencias o comentarios de forma escrita y contribuyas a nuestro propósito de Mejorar Vidas. \n \n✍️ Déjanos tu mensaje por escrito y ayúdanos a seguir construyendo juntos una mejor experiencia para todos."],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"01. Decision Dejar algun comentario","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 08.Te escuchamos › 01. Decision Dejar algun comentario

**Al entrar, en este orden:**

1. 📌 Asigna: `nombreDatix` = ``, `numeroDocumentoDatix` = ``, `comentario` = ``
2. 💬 Dice: «¿Quieres dejarnos algún comentario u opinión? 😀»
3. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «No» (`No`) → página «02. Algo mas»
- «Si» (`Si`) → página «03. Capturar datos»

```json camila:page
{"flow":"08.Te escuchamos","page":{"name":"01. Decision Dejar algun comentario","entry":{"messages":[{"type":"text","texts":["¿Quieres dejarnos algún comentario u opinión? \uD83D\uDE00"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si"},{"text":"No","event":"no"}]}],"setParams":[{"parameter":"$session.params.nombreDatix"},{"parameter":"$session.params.numeroDocumentoDatix"},{"parameter":"$session.params.comentario"}],"generators":[]},"form":[],"routes":[],"events":[{"event":"No","targetPage":"02. Algo mas","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"Si","targetPage":"03. Capturar datos","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 08.Te escuchamos › 02. Algo mas

**Al entrar, en este orden:**

1. 📌 Asigna: `teEscuchamos` = ``, `consultaGenerativa` = ``, `mensajeGenerativoGlobal` = ``
2. 💬 Dice: «¿Algo más en lo que pueda ayudarte?»
3. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`Si`) → flujo **Default Start Flow**  
  ↳ 📌 asigna `menu` = `true`
- «No» (`No`) → página «05. Despedida»

```json camila:page
{"flow":"08.Te escuchamos","page":{"name":"02. Algo mas","entry":{"messages":[{"type":"text","texts":["¿Algo más en lo que pueda ayudarte?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si"},{"text":"No","event":"no"}]}],"setParams":[{"parameter":"$session.params.teEscuchamos"},{"parameter":"$session.params.consultaGenerativa"},{"parameter":"$session.params.mensajeGenerativoGlobal"}],"generators":[]},"form":[],"routes":[],"events":[{"event":"Si","targetFlow":"Default Start Flow","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.menu","value":true}],"generators":[]}},{"event":"No","targetPage":"05. Despedida","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 08.Te escuchamos › 03. Capturar datos

**Al entrar, en este orden:**

1. 💬 Dice: «¿Qué comentario, opinión o sugerencia te gustaría compartir?✍️»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «04. Capturar comentario»

```json camila:page
{"flow":"08.Te escuchamos","page":{"name":"03. Capturar datos","entry":{"messages":[{"type":"text","texts":["¿Qué comentario, opinión o sugerencia te gustaría compartir?✍️"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"04. Capturar comentario","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 08.Te escuchamos › 04. Capturar comentario

**Al entrar, en este orden:**

1. 📌 Asigna: `consultaGenerativa` = `Vas a basarte en la respuesta que nos genera la persona la c…`

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `comentario` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.comentario != null` → página «04.1 Responder comentario»  
  ↳ ⚡ Llama al webhook **IA Generativa Camila - General** [ConsultaWeb] — lectura

```json camila:page
{"flow":"08.Te escuchamos","page":{"name":"04. Capturar comentario","entry":{"messages":[],"setParams":[{"parameter":"consultaGenerativa","value":"Vas a basarte en la respuesta que nos genera la persona la cual es esta \" + $session.params.comentario + \", y vas a responder amablemente llevando su idea personalmente y enfatizando pero siempre dejando en claro esta idea general sin pasarla textualmente\" Perfecto, gracias por compartirnos tu comentario u opinión, lo revisaremos para emitirte una respuesta. \uD83D\uDE00 \" y por favor no dejes preguntas abiertas \""}],"generators":[]},"form":[{"name":"comentario","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.comentario != null","targetPage":"04.1 Responder comentario","fulfillment":{"messages":[],"setParams":[],"webhook":"IA Generativa Camila - General","tag":"ConsultaWeb","generators":[]}}],"events":[],"groups":[]}}
```

### Página · 08.Te escuchamos › 04.1 Responder comentario

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.mensajeGenerativoGlobal»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «02. Algo mas»  
  ↳ ✍️ Llama al webhook **Guardar Comentario Te Escuchamos** [GuardarTeEscuchamos] — **escritura, pide confirmación**

```json camila:page
{"flow":"08.Te escuchamos","page":{"name":"04.1 Responder comentario","entry":{"messages":[{"type":"text","texts":["$session.params.mensajeGenerativoGlobal"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"02. Algo mas","fulfillment":{"messages":[],"setParams":[],"webhook":"Guardar Comentario Te Escuchamos","tag":"GuardarTeEscuchamos","generators":[]}}],"events":[],"groups":[]}}
```

### Página · 08.Te escuchamos › 05. Despedida

**Al entrar, en este orden:**

_No hace nada al entrar._

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `cierre`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true`

```json camila:page
{"flow":"08.Te escuchamos","page":{"name":"05. Despedida","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"cierre","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"true","targetFlow":"10. Cierre de conversación","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.cierre","value":true}],"generators":[]}}],"events":[],"groups":[]}}
```

