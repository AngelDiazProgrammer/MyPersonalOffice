---
bot: camila
flow: 07. Actualizacion de datos Datix
---
## Flujo · 07. Actualizacion de datos Datix

**Qué dice al empezar:** «Datix es nuestra plataforma de registro y actualización de datos diseñada para mejorar tu experiencia en OS»

**Se llega desde:**
- 13. Usuario inexistente/ inactivo › 0.3 Usuario inactivo ← evento `sys.no-match-default`
- Default Start Flow › 05.Menu_Principal ← evento `actualizacion_datos`
- flujo (global) ← intent «route_ActualizarDatosDatix»
- flujo Default Start Flow ← intent «route_ActualizarDatosDatix»

**Webhooks que usa:** ✍️ Finalizar Conversacion · ⚡ IA Generativa Camila - General

**Páginas:**
- 00. Actualizar Datos — ⚡ IA Generativa Camila - General
- 02. Enlace Datix 
- 03. Recordatorio OS 
- 04. Algo Mas 
- 05. Despedida — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.actualizarDatos = true` → página «00. Actualizar Datos»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"07. Actualizacion de datos Datix","routes":[{"condition":"$session.params.actualizarDatos = true","targetPage":"00. Actualizar Datos","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 07. Actualizacion de datos Datix › 00. Actualizar Datos

**Al entrar, en este orden:**

1. 💬 Dice: «Datix es nuestra plataforma de registro y actualización de datos diseñada para mejorar tu experiencia en OS»
2. 💬 Dice: «¿Te gustaría revisar y actualizar tus datos? 📈»
3. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → página «02. Enlace Datix»
- «No» (`no`) → página «03. Recordatorio OS»
💬 *Si el usuario escribe algo que coincide con:*
- intent «route_RespuestaAfirmativa» (ej. «Si quiero actualizar mis datos», «Si») → página «02. Enlace Datix»  
  ↳ 📌 asigna `consultaGenerativa` = `Básate en esta respuesta "Puedes actualizar en la siguiente …` · ⚡ Llama al webhook **IA Generativa Camila - General** [ConsultaWeb] — lectura
- intent «route_RespuestaNegativa» (ej. «No, gracias», «No gracias») → página «03. Recordatorio OS»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 📌 asigna `consultaGenerativa` = `El usuario respondió algo que no fue claramente un Sí o un N…` · ⚡ Llama al webhook **IA Generativa Camila - General** [ConsultaWeb] — lectura · 💬 dice «$session.params.mensajeGenerativoGlobal»

```json camila:page
{"flow":"07. Actualizacion de datos Datix","page":{"name":"00. Actualizar Datos","entry":{"messages":[{"type":"text","texts":["Datix es nuestra plataforma de registro y actualización de datos diseñada para mejorar tu experiencia en OS"],"buttons":[]},{"type":"text","texts":["¿Te gustaría revisar y actualizar tus datos? \uD83D\uDCC8"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si"},{"text":"No","event":"no"}]}],"setParams":[],"generators":[]},"form":[],"routes":[{"intent":"route_RespuestaAfirmativa","condition":"true","targetPage":"02. Enlace Datix","fulfillment":{"messages":[],"setParams":[{"parameter":"consultaGenerativa","value":"Básate en esta respuesta \"Puedes actualizar en la siguiente url\" y construye la tuya propia y al final envia el link de Datix para actualizar los datos https://portalos.outsourcing.com.co:9660/"}],"webhook":"IA Generativa Camila - General","tag":"ConsultaWeb","generators":[]}},{"intent":"route_RespuestaNegativa","condition":"true","targetPage":"03. Recordatorio OS","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["$session.params.mensajeGenerativoGlobal"],"buttons":[]}],"setParams":[{"parameter":"consultaGenerativa","value":"El usuario respondió algo que no fue claramente un Sí o un No. Pídele amablemente que confirme si desea actualizar sus datos respondiendo Sí o No, y hazlo humano y amigable"}],"webhook":"IA Generativa Camila - General","tag":"ConsultaWeb","generators":[]}},{"event":"si","targetPage":"02. Enlace Datix","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"no","targetPage":"03. Recordatorio OS","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 07. Actualizacion de datos Datix › 02. Enlace Datix

**Al entrar, en este orden:**

1. 💬 Dice: «Actualiza en el siguiente enlace. 👉»
2. 💬 Dice: «https://bit.ly/Datix»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «04. Algo Mas»

```json camila:page
{"flow":"07. Actualizacion de datos Datix","page":{"name":"02. Enlace Datix","entry":{"messages":[{"type":"text","texts":["Actualiza en el siguiente enlace. \uD83D\uDC49"],"buttons":[]},{"type":"text","texts":["https://bit.ly/Datix"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"04. Algo Mas","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 07. Actualizacion de datos Datix › 03. Recordatorio OS

**Al entrar, en este orden:**

1. 💬 Dice: «Está bien👌, aunque recuerda que como colaborador es indispensable que actualices tus datos para estar conectados y hacer parte de todos los beneficios y actividades de nuestra compañía.»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «04. Algo Mas»

```json camila:page
{"flow":"07. Actualizacion de datos Datix","page":{"name":"03. Recordatorio OS","entry":{"messages":[{"type":"text","texts":["Está bien\uD83D\uDC4C, aunque recuerda que como colaborador es indispensable que actualices tus datos para estar conectados y hacer parte de todos los beneficios y actividades de nuestra compañía."],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"04. Algo Mas","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 07. Actualizacion de datos Datix › 04. Algo Mas

**Al entrar, en este orden:**

1. 📌 Asigna: `actualizarDatosDatix` = ``, `consultaGenerativa` = ``, `mensajeGenerativoGlobal` = ``
2. 💬 Dice: «¿Algo más en lo que pueda ayudarte?»
3. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → se queda en la página
🔘 *Si el usuario pulsa un botón:*
- «Si» (`Si`) → flujo **Default Start Flow**  
  ↳ 📌 asigna `menu` = `true`, `actualizarDatosDatix` = `false`
- «No» (`No`) → página «05. Despedida»

```json camila:page
{"flow":"07. Actualizacion de datos Datix","page":{"name":"04. Algo Mas","entry":{"messages":[{"type":"text","texts":["¿Algo más en lo que pueda ayudarte?\n"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si"},{"text":"No","event":"no"}]}],"setParams":[{"parameter":"$session.params.actualizarDatosDatix"},{"parameter":"$session.params.consultaGenerativa"},{"parameter":"$session.params.mensajeGenerativoGlobal"}],"generators":[]},"form":[],"routes":[{"condition":"true","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"Si","targetFlow":"Default Start Flow","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.menu","value":true},{"parameter":"$session.params.actualizarDatosDatix","value":false}],"generators":[]}},{"event":"No","targetPage":"05. Despedida","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 07. Actualizacion de datos Datix › 05. Despedida

**Al entrar, en este orden:**

1. 💬 Dice: «¡Gracias por actualizar tus datos!  Recuerda que en Outsourcing nos mueve el propósito de Mejorar Vidas, nuestro Compromiso.❤️»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `cierre`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ 📌 asigna `cierre` = `true` · ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

```json camila:page
{"flow":"07. Actualizacion de datos Datix","page":{"name":"05. Despedida","entry":{"messages":[{"type":"text","texts":["¡Gracias por actualizar tus datos!\n\nRecuerda que en Outsourcing nos mueve el propósito de Mejorar Vidas, nuestro Compromiso.❤️"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"cierre","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"true","targetPage":"End Session","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.cierre","value":true}],"webhook":"Finalizar Conversacion","tag":"EndConversationWithPersonalizedMessage","generators":[]}}],"events":[],"groups":[]}}
```

