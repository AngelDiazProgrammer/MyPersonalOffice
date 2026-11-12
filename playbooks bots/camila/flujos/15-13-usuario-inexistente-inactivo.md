---
bot: camila
flow: 13. Usuario inexistente/ inactivo
---
## Flujo · 13. Usuario inexistente/ inactivo

**Qué dice al empezar:** «Lo sentimos, el número de identificación ingresado no se encuentra registrado en nuestra base de datos. Recuerda que debes ser un colaborador OS activo para pod…»

**Se llega desde:**
- 01. Indicadores De Calidad › 00.ValidacionUsuarioKairos ← condición

**Webhooks que usa:** ⚡ Obtener_Usuario_Por_Cedula

**Pide al usuario:** `Numero_Documento` (00.1 Solicitar numero de documento), `placa_carro` (00.1 Solicitar numero de documento)

**Páginas:**
- 0.1 condicional documento 
- 0.2 Usuario no registrado 
- 0.3 Usuario inactivo 
- 00.1 Solicitar numero de documento — ⚡ Obtener_Usuario_Por_Cedula · ⌨️ pide datos

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «00.1 Solicitar numero de documento»
💬 *Si el usuario escribe algo que coincide con:*
- intent «Default Welcome Intent» (ej. «oa», «ola») → página «00.1 Solicitar numero de documento»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"13. Usuario inexistente/ inactivo","routes":[{"intent":"Default Welcome Intent","targetPage":"00.1 Solicitar numero de documento","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"true","targetPage":"00.1 Solicitar numero de documento","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 13. Usuario inexistente/ inactivo › 0.1 condicional documento

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.Numero_Documento != null AND $session.params.usuarioNoExiste = true` → página «0.2 Usuario no registrado»
- si `$session.params.usuarioInactivo = true AND $session.params.Numero_Documento != null` → página «0.3 Usuario inactivo»
- si `$session.params.existe = true AND $session.params.activo = true` → flujo **01. Indicadores De Calidad**

```json camila:page
{"flow":"13. Usuario inexistente/ inactivo","page":{"name":"0.1 condicional documento","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"$session.params.Numero_Documento != null AND $session.params.usuarioNoExiste = true","targetPage":"0.2 Usuario no registrado","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.usuarioInactivo = true AND $session.params.Numero_Documento != null","targetPage":"0.3 Usuario inactivo","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.existe = true AND $session.params.activo = true","targetFlow":"01. Indicadores De Calidad","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 13. Usuario inexistente/ inactivo › 0.2 Usuario no registrado

**Al entrar, en este orden:**

1. 💬 Dice: «Lo sentimos, el número de identificación ingresado no se encuentra registrado en nuestra base de datos. Recuerda que debes ser un colaborador OS activo para poder continuar con esta conversación ✅»
2. 🔘 Ofrece los botones: «Ir a DATIX» · «Intentar con otra cédula»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Intentar con otra cédula» (`reintentar_cedula`) → página «00.1 Solicitar numero de documento»  
  ↳ 📌 asigna `Numero_Documento` = ``
- evento `sys.no-match-1` → flujo **10. Cierre de conversación**  
  ↳ 💬 dice «Si necesitas una ayuda mas especifica por favor comunicate c…»

```json camila:page
{"flow":"13. Usuario inexistente/ inactivo","page":{"name":"0.2 Usuario no registrado","entry":{"messages":[{"type":"text","texts":["Lo sentimos, el número de identificación ingresado no se encuentra registrado en nuestra base de datos.\nRecuerda que debes ser un colaborador OS activo para poder continuar con esta conversación ✅"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Ir a DATIX","link":"https://portalos.outsourcing.com.co:9660/"},{"text":"Intentar con otra cédula","event":"reintentar_cedula"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"reintentar_cedula","targetPage":"00.1 Solicitar numero de documento","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.Numero_Documento"}],"generators":[]}},{"event":"sys.no-match-1","targetFlow":"10. Cierre de conversación","fulfillment":{"messages":[{"type":"text","texts":["Si necesitas una ayuda mas especifica por favor comunicate con el area de recuersos humanos \uD83D\uDE0A"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 13. Usuario inexistente/ inactivo › 0.3 Usuario inactivo

**Al entrar, en este orden:**

1. 💬 Dice: «Si eres colaborador OS activo y consideras que existe un error, ingresa a DATIX para validar y actualizar tu información en nuestra base de datos 👇»
2. 🔘 Ofrece los botones: «Ir a DATIX» · «Intentar con otra cédula»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Intentar con otra cédula» (`reintentar_cedula`) → página «00.1 Solicitar numero de documento»  
  ↳ 📌 asigna `Numero_Documento` = ``
- ❔ *Sin coincidencia* → flujo **07. Actualizacion de datos Datix**  
  ↳ 💬 dice «Recuerda que puedes actualizar tus datos en plataforma datix…»

```json camila:page
{"flow":"13. Usuario inexistente/ inactivo","page":{"name":"0.3 Usuario inactivo","entry":{"messages":[{"type":"text","texts":["Si eres colaborador OS activo y consideras que existe un error, ingresa a DATIX para validar y actualizar tu información en nuestra base de datos \uD83D\uDC47"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Ir a DATIX","link":"https://portalos.outsourcing.com.co:9660/"},{"text":"Intentar con otra cédula","event":"reintentar_cedula"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"reintentar_cedula","targetPage":"00.1 Solicitar numero de documento","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.Numero_Documento"}],"generators":[]}},{"event":"sys.no-match-default","targetFlow":"07. Actualizacion de datos Datix","fulfillment":{"messages":[{"type":"text","texts":["Recuerda que puedes actualizar tus datos en plataforma datix \uD83E\uDEE1"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 13. Usuario inexistente/ inactivo › 00.1 Solicitar numero de documento

**Al entrar, en este orden:**

1. 📌 Asigna: `Numero_Documento` = ``, `placa_carro` = ``
2. 💬 Dice: «Tu número de cédula no es válido o no se encuentra activo en nuestra base de datos 😓»
3. 💬 Dice: «Por favor verifícalo y escríbelo nuevamente sin puntos ni comas»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `Numero_Documento` — @Numero_Documento
- `placa_carro` — @placa_carro

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.Numero_Documento != null` → página «0.1 condicional documento»  
  ↳ ⚡ Llama al webhook **Obtener_Usuario_Por_Cedula** [ObtenerUsuarioPorCedula] — lectura

```json camila:page
{"flow":"13. Usuario inexistente/ inactivo","page":{"name":"00.1 Solicitar numero de documento","entry":{"messages":[{"type":"text","texts":["Tu número de cédula no es válido o no se encuentra activo en nuestra base de datos \uD83D\uDE13"],"buttons":[]},{"type":"text","texts":["Por favor verifícalo y escríbelo nuevamente sin puntos ni comas"],"buttons":[]}],"setParams":[{"parameter":"Numero_Documento"},{"parameter":"placa_carro"}],"generators":[]},"form":[{"name":"Numero_Documento","entity":"@Numero_Documento","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"placa_carro","entity":"@placa_carro","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.Numero_Documento != null","targetPage":"0.1 condicional documento","fulfillment":{"messages":[],"setParams":[],"webhook":"Obtener_Usuario_Por_Cedula","tag":"ObtenerUsuarioPorCedula","generators":[]}}],"events":[],"groups":[]}}
```

