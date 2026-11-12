---
bot: camila
flow: 15.1 Encuesta W
---
## Flujo · 15.1 Encuesta W

**Qué dice al empezar:** «Para Outsourcing la experiencia de nuestros candidatos indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinió…»

**Se llega desde:**
- flujo 15. Viaje del colaborador ← condición

**Webhooks que usa:** ⚡ Extraer información candidato de W

**Pide al usuario:** `numero_documento` (0.1 Solicitar Cedula)

**Páginas:**
- 0.1 Solicitar Cedula — ⌨️ pide datos
- 0.Definir Encuesta W — ⚡ Extraer información candidato de W
- 1.Encuesta Proceso de Seleccion 
- 2. Encuesta Proceso de Capacitacion 
- 3. Encuesta Proceso de Contratacion 
- 4. Cierre 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0.1 Solicitar Cedula»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"15.1 Encuesta W","routes":[{"condition":"true","targetPage":"0.1 Solicitar Cedula","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 15.1 Encuesta W › 0.1 Solicitar Cedula

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `numero_documento` — @sys.any · pregunta: «Por favor digita tu número de documento (sin puntos ni espacios):»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL" OR $session.params.numero_documento != null` → página «0.Definir Encuesta W»

```json camila:page
{"flow":"15.1 Encuesta W","page":{"name":"0.1 Solicitar Cedula","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"numero_documento","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Por favor digita tu número de documento (sin puntos ni espacios):"],"buttons":[]}],"setParams":[],"generators":[]}}],"routes":[{"condition":"$page.params.status = \"FINAL\" OR $session.params.numero_documento != null","targetPage":"0.Definir Encuesta W","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 15.1 Encuesta W › 0.Definir Encuesta W

**Al entrar, en este orden:**

1. ⚡ Llama al webhook **Extraer información candidato de W** [https://default140443af8e794fceb3edde00131298.4f.environment.api.powerplatform.com/powerautomate/automations/direct/workflows/6593822f883f4cdc87c79560e93e5f22/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=-jHB9R2eCIWAjMPQ9z6dKoDv8xgfGfeKuySL1akSZXA] — lectura

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.faseW = "Decision" AND $session.params.estadoW = "Pendiente"` → página «1.Encuesta Proceso de Seleccion»
- si `$session.params.faseW = "Capacitacion" AND $session.params.estadoW = "Capacitacion"` → página «2. Encuesta Proceso de Capacitacion»
- si `$session.params.faseW = "Contratacion" AND $session.params.estadoW = "Contratacion"` → página «3. Encuesta Proceso de Contratacion»

```json camila:page
{"flow":"15.1 Encuesta W","page":{"name":"0.Definir Encuesta W","entry":{"messages":[],"setParams":[],"webhook":"Extraer información candidato de W","tag":"https://default140443af8e794fceb3edde00131298.4f.environment.api.powerplatform.com/powerautomate/automations/direct/workflows/6593822f883f4cdc87c79560e93e5f22/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=-jHB9R2eCIWAjMPQ9z6dKoDv8xgfGfeKuySL1akSZXA","generators":[]},"form":[],"routes":[{"condition":"$session.params.faseW = \"Decision\" AND $session.params.estadoW = \"Pendiente\"","targetPage":"1.Encuesta Proceso de Seleccion","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.faseW = \"Capacitacion\" AND $session.params.estadoW = \"Capacitacion\"","targetPage":"2. Encuesta Proceso de Capacitacion","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.faseW = \"Contratacion\" AND $session.params.estadoW = \"Contratacion\"","targetPage":"3. Encuesta Proceso de Contratacion","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 15.1 Encuesta W › 1.Encuesta Proceso de Seleccion

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de nuestros candidatos indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinión acerca del proceso de selección que re…»
2. 💬 Dice: «¿Qué tan satisfactoria fue tu experiencia con el proceso de selección?»
3. 🔘 Ofrece los botones: «Muy satisfecha» · «Satisfecha» · «Poco satisfecha» · «Insatisfecha»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Satisfecha» (`satisfecha`) → página «4. Cierre»
- «Poco satisfecha» (`poco_satisfecha`) → página «4. Cierre»
- «Insatisfecha» (`insatisfecha`) → página «4. Cierre»
- «Muy satisfecha» (`muy_satisfecha`) → página «4. Cierre»

```json camila:page
{"flow":"15.1 Encuesta W","page":{"name":"1.Encuesta Proceso de Seleccion","entry":{"messages":[{"type":"text","texts":["Para Outsourcing la experiencia de nuestros candidatos indispensable para cumplir con nuestro propósito de mejorar vidas\uD83D\uDE03. Por ello, queremos conocer tu opinión acerca del proceso de selección que realizaste con nosotros."],"buttons":[]},{"type":"text","texts":["¿Qué tan satisfactoria fue tu experiencia con el proceso de selección?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Muy satisfecha","event":"muy_satisfecha"},{"text":"Satisfecha","event":"satisfecha"},{"text":"Poco satisfecha","event":"poco_satisfecha"},{"text":"Insatisfecha","event":"insatisfecha"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"satisfecha","targetPage":"4. Cierre","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"poco_satisfecha","targetPage":"4. Cierre","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"insatisfecha","targetPage":"4. Cierre","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"muy_satisfecha","targetPage":"4. Cierre","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 15.1 Encuesta W › 2. Encuesta Proceso de Capacitacion

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinión acerca del proceso de capacitaci…»
2. 💬 Dice: «¿Qué tan satisfactoria fue tu experiencia con el proceso de capacitación?»
3. 🔘 Ofrece los botones: «Muy satisfecha» · «Satisfecha» · «Poco satisfecha» · «Insatisfecha»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Satisfecha» (`satisfecha`) → página «4. Cierre»
- «Poco satisfecha» (`poco_satisfecha`) → página «4. Cierre»
- «Insatisfecha» (`insatisfecha`) → página «4. Cierre»
- «Muy satisfecha» (`muy_satisfecha`) → página «4. Cierre»

```json camila:page
{"flow":"15.1 Encuesta W","page":{"name":"2. Encuesta Proceso de Capacitacion","entry":{"messages":[{"type":"text","texts":["Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas\uD83D\uDE03. Por ello, queremos conocer tu opinión acerca del proceso de capacitación de producto"],"buttons":[]},{"type":"text","texts":["¿Qué tan satisfactoria fue tu experiencia con el proceso de capacitación?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Muy satisfecha","event":"muy_satisfecha"},{"text":"Satisfecha","event":"satisfecha"},{"text":"Poco satisfecha","event":"poco_satisfecha"},{"text":"Insatisfecha","event":"insatisfecha"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"satisfecha","targetPage":"4. Cierre","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"poco_satisfecha","targetPage":"4. Cierre","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"insatisfecha","targetPage":"4. Cierre","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"muy_satisfecha","targetPage":"4. Cierre","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 15.1 Encuesta W › 3. Encuesta Proceso de Contratacion

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinión acerca de proceso de contratació…»
2. 💬 Dice: «¿Qué tan satisfactoria fue tu experiencia con el proceso de contratación?»
3. 🔘 Ofrece los botones: «Muy satisfecha» · «Satisfecha» · «Poco satisfecha» · «Insatisfecha»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- evento `sys.unknown-event` → se queda en la página  
  ↳ 💬 dice «Gracias por tu tiempo, $session.params.nombre. ¡Conversamos …»
- «Muy satisfecha» (`muy_satisfecha`) → página «4. Cierre»
- «Satisfecha» (`satisfecha`) → página «4. Cierre»
- «Poco satisfecha» (`poco_satisfecha`) → página «4. Cierre»
- «Insatisfecha» (`insatisfecha`) → página «4. Cierre»

```json camila:page
{"flow":"15.1 Encuesta W","page":{"name":"3. Encuesta Proceso de Contratacion","entry":{"messages":[{"type":"text","texts":["Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas\uD83D\uDE03. Por ello, queremos conocer tu opinión acerca de proceso de contratación de nuestra compañía."],"buttons":[]},{"type":"text","texts":["¿Qué tan satisfactoria fue tu experiencia con el proceso de contratación?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Muy satisfecha","event":"muy_satisfecha"},{"text":"Satisfecha","event":"satisfecha"},{"text":"Poco satisfecha","event":"poco_satisfecha"},{"text":"Insatisfecha","event":"insatisfecha"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"sys.unknown-event","fulfillment":{"messages":[{"type":"text","texts":["Gracias por tu tiempo, $session.params.nombre. ¡Conversamos en una próxima oportunidad! Recuerda que en OUTSOURCING nos mueve el propósito de Mejorar Vidas. Nuestro Compromiso. \uD83D\uDC96"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"muy_satisfecha","targetPage":"4. Cierre","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"satisfecha","targetPage":"4. Cierre","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"poco_satisfecha","targetPage":"4. Cierre","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"insatisfecha","targetPage":"4. Cierre","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 15.1 Encuesta W › 4. Cierre

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **10. Cierre de conversación**

```json camila:page
{"flow":"15.1 Encuesta W","page":{"name":"4. Cierre","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetFlow":"10. Cierre de conversación","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

