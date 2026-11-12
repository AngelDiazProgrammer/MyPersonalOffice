---
bot: camila
flow: 09.Convenios
---
## Flujo · 09.Convenios

**Qué dice al empezar:** «Los convenios son alianzas estratégicas que realizamos para ampliar el portafolio de beneficios de la familia OS. 😃»

**Se llega desde:**
- Default Start Flow › 05.Menu_Principal ← evento `convenios`
- flujo (global) ← intent «route_Convenios»
- flujo Default Start Flow ← intent «route_Convenios»

**Webhooks que usa:** ⚡ IA Generativa Camila - General

**Páginas:**
- 00. Alianzas estrategicas 
- 01. Decisión conocer conveniOS — ⚡ IA Generativa Camila - General
- 02. Enlace de conveniOS 
- 03. Algo mas 
- 04. Despedida 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.convenios = true` → página «00. Alianzas estrategicas»  
  ↳ 📌 asigna `intencion` = `route_Convenios` · ⚡ Llama al webhook **IA Generativa Camila - General** [consultaWeb] — lectura
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"09.Convenios","routes":[{"condition":"$session.params.convenios = true","targetPage":"00. Alianzas estrategicas","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.intencion","value":"route_Convenios"}],"webhook":"IA Generativa Camila - General","tag":"consultaWeb","generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 09.Convenios › 00. Alianzas estrategicas

**Al entrar, en este orden:**

1. 💬 Dice: «Los convenios son alianzas estratégicas que realizamos para ampliar el portafolio de beneficios de la familia OS. 😃»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «01. Decisión conocer conveniOS»

```json camila:page
{"flow":"09.Convenios","page":{"name":"00. Alianzas estrategicas","entry":{"messages":[{"type":"text","texts":["Los convenios son alianzas estratégicas que realizamos para ampliar el portafolio de beneficios de la familia OS. \uD83D\uDE03"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"01. Decisión conocer conveniOS","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 09.Convenios › 01. Decisión conocer conveniOS

**Al entrar, en este orden:**

1. 💬 Dice: «¿Quieres conocer nuestros convenios?. 🤩»
2. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`Si`) → página «02. Enlace de conveniOS»  
  ↳ 📌 asigna `consultaGenerativa` = `Debes dar el contexto de todos los convenios que manejamos s…` · ⚡ Llama al webhook **IA Generativa Camila - General** [ConsultaWeb] — lectura
- «No» (`No`) → página «03. Algo mas»

```json camila:page
{"flow":"09.Convenios","page":{"name":"01. Decisión conocer conveniOS","entry":{"messages":[{"type":"text","texts":["¿Quieres conocer nuestros convenios?. \uD83E\uDD29"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si"},{"text":"No","event":"no"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"Si","targetPage":"02. Enlace de conveniOS","fulfillment":{"messages":[],"setParams":[{"parameter":"consultaGenerativa","value":"Debes dar el contexto de todos los convenios que manejamos segun lo que tengas almacenado en tu base de conocimiento, responde humano y amigables, no dejes preguntas abiertas, contesta con sinceridad y brinda toda la informacion que tengas acerca de los convenios, nombres, contactos, informacion, de todo por favor, pero segementado, por favor que no se vea tanta informacion amontonada o inentendible, no dejes preguntas abiertas ni contextos abiertos, solo la informacion"}],"webhook":"IA Generativa Camila - General","tag":"ConsultaWeb","generators":[]}},{"event":"No","targetPage":"03. Algo mas","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 09.Convenios › 02. Enlace de conveniOS

**Al entrar, en este orden:**

1. 💬 Dice: «¡Conoce nuestros convenios! Revísalos en la imagen que aparece a continuación 👇»
2. 💬 Muestra la tarjeta «Producto destacado»
3. 🖼️ Muestra una imagen https://fine360.outsourcing.com.co:9876/img/convenios-outsourcing-v2.jpg

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «03. Algo mas»

```json camila:page
{"flow":"09.Convenios","page":{"name":"02. Enlace de conveniOS","entry":{"messages":[{"type":"text","texts":["¡Conoce nuestros convenios! Revísalos en la imagen que aparece a continuación \uD83D\uDC47"],"buttons":[]},{"type":"info","texts":[],"buttons":[],"title":"Producto destacado","subtitle":"Señor usuario, por favor para más información ver la imagen adjunta."},{"type":"image","texts":[],"buttons":[],"title":"Imagen del producto","url":"https://fine360.outsourcing.com.co:9876/img/convenios-outsourcing-v2.jpg"}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"03. Algo mas","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 09.Convenios › 03. Algo mas

**Al entrar, en este orden:**

1. 📌 Asigna: `convenios` = `false`, `consultaGenerativa` = ``, `mensajeGenerativoGlobal` = ``
2. 💬 Dice: «¿Algo más en lo que pueda ayudarte?»
3. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`Si`) → flujo **Default Start Flow**  
  ↳ 📌 asigna `menu` = `true`, `convenios` = `false`
- «No» (`No`) → página «04. Despedida»

```json camila:page
{"flow":"09.Convenios","page":{"name":"03. Algo mas","entry":{"messages":[{"type":"text","texts":["¿Algo más en lo que pueda ayudarte?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si"},{"text":"No","event":"no"}]}],"setParams":[{"parameter":"$session.params.convenios","value":false},{"parameter":"$session.params.consultaGenerativa"},{"parameter":"$session.params.mensajeGenerativoGlobal"}],"generators":[]},"form":[],"routes":[],"events":[{"event":"Si","targetFlow":"Default Start Flow","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.menu","value":true},{"parameter":"$session.params.convenios","value":false}],"generators":[]}},{"event":"No","targetPage":"04. Despedida","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 09.Convenios › 04. Despedida

**Al entrar, en este orden:**

_No hace nada al entrar._

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `cierre`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true`

```json camila:page
{"flow":"09.Convenios","page":{"name":"04. Despedida","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"cierre","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"true","targetFlow":"10. Cierre de conversación","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.cierre","value":true}],"generators":[]}}],"events":[],"groups":[]}}
```

