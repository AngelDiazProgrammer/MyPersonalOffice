---
bot: camila
flow: 14. Reportar Retiro
---
## Flujo · 14. Reportar Retiro

**Qué dice al empezar:** «¡Gracias por la información! Estaré enviando la encuesta de retiro al colaborador de tu equipo.»

**Se llega desde:**
- Default Start Flow › 05.Menu_Principal ← evento `reportarRetiro`

**Webhooks que usa:** ✍️ Envio HSM Encuesta de Retiro Camila · ✍️ Finalizar Conversacion · ✍️ Guardar Datos Colaborador Encuesta De Retiro

**Pide al usuario:** `retiroNombreColaborador` (01. Solicitar datos colaborador), `retiroNumeroTelColaborador` (01. Solicitar datos colaborador), `retiroNumeroDocumentoColaborador` (01. Solicitar datos colaborador)

**Páginas:**
- 01. Solicitar datos colaborador — ⌨️ pide datos
- 02. Guardar Datos Colaborador Retirado — ✍️ Guardar Datos Colaborador Encuesta De Retiro
- 03. Ejecutar HSM Camila Envió de Encuesta — ✍️ Envio HSM Encuesta de Retiro Camila · ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «01. Solicitar datos colaborador»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"14. Reportar Retiro","routes":[{"condition":"true","targetPage":"01. Solicitar datos colaborador","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 14. Reportar Retiro › 01. Solicitar datos colaborador

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `retiroNombreColaborador` — @sys.any · pregunta: «Ingresa el nombre del colaborador»
- `retiroNumeroTelColaborador` — @sys.any · pregunta: «Ingresa el número de celular del colaborador»
- `retiroNumeroDocumentoColaborador` — @sys.any · pregunta: «Ingresa el número de identificación del colaborador»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.retiroNombreColaborador != null AND $session.params.retiroNumeroTelColaborador != null AND $session.params.retiroNumeroDocumentoColaborador != null` → página «02. Guardar Datos Colaborador Retirado»

```json camila:page
{"flow":"14. Reportar Retiro","page":{"name":"01. Solicitar datos colaborador","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"retiroNombreColaborador","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Ingresa el nombre del colaborador"],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"retiroNumeroTelColaborador","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Ingresa el número de celular del colaborador"],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"retiroNumeroDocumentoColaborador","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Ingresa el número de identificación del colaborador"],"buttons":[]}],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.retiroNombreColaborador != null AND $session.params.retiroNumeroTelColaborador != null AND $session.params.retiroNumeroDocumentoColaborador != null","targetPage":"02. Guardar Datos Colaborador Retirado","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 14. Reportar Retiro › 02. Guardar Datos Colaborador Retirado

**Al entrar, en este orden:**

1. ✍️ Llama al webhook **Guardar Datos Colaborador Encuesta De Retiro** [InsertarColaborador] — **escritura, pide confirmación**
2. 💬 Dice: «¡Gracias por la información! Estaré enviando la encuesta de retiro al colaborador de tu equipo.»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «03. Ejecutar HSM Camila Envió de Encuesta»

```json camila:page
{"flow":"14. Reportar Retiro","page":{"name":"02. Guardar Datos Colaborador Retirado","entry":{"messages":[{"type":"text","texts":["¡Gracias por la información! Estaré enviando la encuesta de retiro al colaborador de tu equipo."],"buttons":[]}],"setParams":[],"webhook":"Guardar Datos Colaborador Encuesta De Retiro","tag":"InsertarColaborador","generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"03. Ejecutar HSM Camila Envió de Encuesta","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 14. Reportar Retiro › 03. Ejecutar HSM Camila Envió de Encuesta

**Al entrar, en este orden:**

1. ✍️ Llama al webhook **Envio HSM Encuesta de Retiro Camila** [EnvioHSMGupshup] — **escritura, pide confirmación**

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

```json camila:page
{"flow":"14. Reportar Retiro","page":{"name":"03. Ejecutar HSM Camila Envió de Encuesta","entry":{"messages":[],"setParams":[],"webhook":"Envio HSM Encuesta de Retiro Camila","tag":"EnvioHSMGupshup","generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"End Session","fulfillment":{"messages":[],"setParams":[],"webhook":"Finalizar Conversacion","tag":"EndConversationWithPersonalizedMessage","generators":[]}}],"events":[],"groups":[]}}
```

