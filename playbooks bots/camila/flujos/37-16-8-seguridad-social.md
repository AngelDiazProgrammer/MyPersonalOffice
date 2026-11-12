---
bot: camila
flow: 16.8 Seguridad Social
---
## Flujo · 16.8 Seguridad Social

**Qué dice al empezar:** «Por favor selecciona una opción:»

**Se llega desde:**
- 16. Administración de personal › 1. ¿En que te puedo ayudar? ← evento `seguridadSocial`

**Pide al usuario:** `tipoDoc` (1. Formulario Planillas), `numDoc` (1. Formulario Planillas), `periodoPlanilla` (1. Formulario Planillas), `correoPlanilla` (1. Formulario Planillas)

**Páginas:**
- 0.menu  
- 1. Formulario Planillas — ⌨️ pide datos
- 2.Confirmacion Planillas 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0.menu »
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"16.8 Seguridad Social","routes":[{"condition":"true","targetPage":"0.menu ","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 16.8 Seguridad Social › 0.menu 

**Al entrar, en este orden:**

1. 💬 Dice: «Por favor selecciona una opción:»
2. 🔘 Ofrece los botones: «📄 Planillas Pago»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$request.text = "📄 Planillas de pago" OR $request.text = "Planillas de pago"` → página «1. Formulario Planillas»
🔘 *Si el usuario pulsa un botón:*
- «📄 Planillas Pago» (`planillaspago`) → página «1. Formulario Planillas»

```json camila:page
{"flow":"16.8 Seguridad Social","page":{"name":"0.menu ","entry":{"messages":[{"type":"text","texts":["Por favor selecciona una opción:"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"\uD83D\uDCC4 Planillas Pago","event":"planillasPago"}]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"$request.text = \"\uD83D\uDCC4 Planillas de pago\" OR $request.text = \"Planillas de pago\"","targetPage":"1. Formulario Planillas","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"planillaspago","targetPage":"1. Formulario Planillas","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 16.8 Seguridad Social › 1. Formulario Planillas

**Al entrar, en este orden:**

1. 💬 Dice: «La planilla de pago solo la puedes solicitar a partir del año 2017. Si requieres certificados de años anteriores, debes enviar tu solicitud al correo auxnomina1@outsourcing.com.co📧»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `tipoDoc` — @sys.any · pregunta: «Digita el tipo de documento»
- `numDoc` — @sys.any · pregunta: «Digita el número de documento»
- `periodoPlanilla` — @sys.any · pregunta: «Ingresa mes y año MM / AAA»
- `correoPlanilla` — @sys.any · pregunta: «Escribe el correo electrónico en el cual deseas recibir tu certificado de planillas de pago»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «2.Confirmacion Planillas»

```json camila:page
{"flow":"16.8 Seguridad Social","page":{"name":"1. Formulario Planillas","entry":{"messages":[{"type":"text","texts":["La planilla de pago solo la puedes solicitar a partir del año 2017. Si requieres certificados de años anteriores, debes enviar tu solicitud al correo auxnomina1@outsourcing.com.co\uD83D\uDCE7"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"tipoDoc","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Digita el tipo de documento"],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"numDoc","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Digita el número de documento"],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"periodoPlanilla","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Ingresa mes y año MM / AAA"],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"correoPlanilla","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Escribe el correo electrónico en el cual deseas recibir tu certificado de planillas de pago"],"buttons":[]}],"setParams":[],"generators":[]}}],"routes":[{"condition":"$page.params.status = \"FINAL\"","targetPage":"2.Confirmacion Planillas","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 16.8 Seguridad Social › 2.Confirmacion Planillas

**Al entrar, en este orden:**

1. 💬 Dice: «Tu solicitud ha sido registrada exitosamente y será tramitada máximo en un 1 día hábil.»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

```json camila:page
{"flow":"16.8 Seguridad Social","page":{"name":"2.Confirmacion Planillas","entry":{"messages":[{"type":"text","texts":["Tu solicitud ha sido registrada exitosamente y será tramitada máximo en un 1 día hábil."],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

