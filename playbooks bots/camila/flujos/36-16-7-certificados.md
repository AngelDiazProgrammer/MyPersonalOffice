---
bot: camila
flow: 16.7 certificados
---
## Flujo · 16.7 certificados

**Qué dice al empezar:** «¿En que puedo ayudarte?»

**Se llega desde:**
- 16. Administración de personal › 1. ¿En que te puedo ayudar? ← evento `certificados`

**Páginas:**
- 0. ¿En que puedo ayudarte? 
- 1. Laboral/Ingresos 
- 2. Laboral/Especial 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0. ¿En que puedo ayudarte?»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"16.7 certificados","routes":[{"condition":"true","targetPage":"0. ¿En que puedo ayudarte?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 16.7 certificados › 0. ¿En que puedo ayudarte?

**Al entrar, en este orden:**

1. 💬 Dice: «¿En que puedo ayudarte?»
2. 🔘 Ofrece los botones: «📄 Laboral/Ingresos» · «📄 Laboral especial»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «📄 Laboral/Ingresos» (`certificadoWeb`) → página «1. Laboral/Ingresos»
- «📄 Laboral especial» (`laboralEspecial`) → página «2. Laboral/Especial»

```json camila:page
{"flow":"16.7 certificados","page":{"name":"0. ¿En que puedo ayudarte?","entry":{"messages":[{"type":"text","texts":["¿En que puedo ayudarte?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"\uD83D\uDCC4 Laboral/Ingresos","event":"certificadoWeb"},{"text":"\uD83D\uDCC4 Laboral especial","event":"laboralEspecial"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"certificadoWeb","targetPage":"1. Laboral/Ingresos","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"laboralEspecial","targetPage":"2. Laboral/Especial","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 16.7 certificados › 1. Laboral/Ingresos

**Al entrar, en este orden:**

1. 💬 Dice: «Para descargar tu certificado accede a 👇 🔗 CONECTADOSWEB https://conectadosweb.outsourcing.com.co/frmLogin.aspx»
2. 💬 Dice: «Si, presentas inconsistencias al ingresar por favor comunícate con auxnomina1@outsourcing.com.co para poder ayudarte. 📧»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

```json camila:page
{"flow":"16.7 certificados","page":{"name":"1. Laboral/Ingresos","entry":{"messages":[{"type":"text","texts":["Para descargar tu certificado accede a \uD83D\uDC47\n\uD83D\uDD17 CONECTADOSWEB\nhttps://conectadosweb.outsourcing.com.co/frmLogin.aspx"],"buttons":[]},{"type":"text","texts":["Si, presentas inconsistencias al ingresar por favor comunícate con auxnomina1@outsourcing.com.co para poder ayudarte. \uD83D\uDCE7"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 16.7 certificados › 2. Laboral/Especial

**Al entrar, en este orden:**

1. 💬 Dice: «Para solicitar el certificado laboral especial con cargos u otras especificaciones (funciones, dirigido a una empresa en particular, entre otros), debes enviar tu solicitud al correo certificacionesla…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

```json camila:page
{"flow":"16.7 certificados","page":{"name":"2. Laboral/Especial","entry":{"messages":[{"type":"text","texts":["Para solicitar el certificado laboral especial con cargos u otras especificaciones (funciones, dirigido a una empresa en particular, entre otros), debes enviar tu solicitud al correo certificacioneslaborales@outsourcing.com.co, indicando los datos y las especificaciones requeridas. \uD83D\uDCE7"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

