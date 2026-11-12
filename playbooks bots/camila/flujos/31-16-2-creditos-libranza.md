---
bot: camila
flow: 16.2 Creditos libranza
---
## Flujo · 16.2 Creditos libranza

**Qué dice al empezar:** «¿En que te puedo ayudar?»

**Se llega desde:**
- 16. Administración de personal › 1. ¿En que te puedo ayudar? ← evento `creditoLibranza`

**Páginas:**
- 0. ¿En que puedo ayudarte? 
- 1. Entidades con convenio 
- 2. Documentos solicitud de crédito 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0. ¿En que puedo ayudarte?»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"16.2 Creditos libranza","routes":[{"condition":"true","targetPage":"0. ¿En que puedo ayudarte?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 16.2 Creditos libranza › 0. ¿En que puedo ayudarte?

**Al entrar, en este orden:**

1. 💬 Dice: «¿En que te puedo ayudar?»
2. 🔘 Ofrece los botones: «🏢 Ver convenios» · «📄 Solicitar crédito»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «🏢 Ver convenios» (`entidadesConvenio`) → página «1. Entidades con convenio»
- «📄 Solicitar crédito» (`solicitarCredito`) → página «2. Documentos solicitud de crédito»

```json camila:page
{"flow":"16.2 Creditos libranza","page":{"name":"0. ¿En que puedo ayudarte?","entry":{"messages":[{"type":"text","texts":["¿En que te puedo ayudar?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"\uD83C\uDFE2 Ver convenios","event":"entidadesConvenio"},{"text":"\uD83D\uDCC4 Solicitar crédito","event":"solicitarCredito"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"entidadesConvenio","targetPage":"1. Entidades con convenio","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"solicitarCredito","targetPage":"2. Documentos solicitud de crédito","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 16.2 Creditos libranza › 1. Entidades con convenio

**Al entrar, en este orden:**

1. 💬 Dice: «¡Conoce nuestros convenios! Revísalos en la imagen que aparece a continuación. 👇»
2. 💬 Muestra la tarjeta «Producto destacado»
3. 🖼️ Muestra una imagen https://fine360.outsourcing.com.co:9876/img/convenios-outsourcing-v2.jpg

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

```json camila:page
{"flow":"16.2 Creditos libranza","page":{"name":"1. Entidades con convenio","entry":{"messages":[{"type":"text","texts":["¡Conoce nuestros convenios! Revísalos en la\nimagen que aparece a continuación. \uD83D\uDC47                                             "],"buttons":[]},{"type":"info","texts":[],"buttons":[],"title":"Producto destacado","subtitle":"Señor usuario, por favor para más información ver la imagen adjunta."},{"type":"image","texts":[],"buttons":[],"title":"Imagen del producto","url":"https://fine360.outsourcing.com.co:9876/img/convenios-outsourcing-v2.jpg"}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 16.2 Creditos libranza › 2. Documentos solicitud de crédito

**Al entrar, en este orden:**

1. 💬 Dice: «Los documentos que requieres para hacer este proceso con la entidad son:  ✅ 3 últimos desprendibles de nómina ✅Certificado Laboral ✅Debes validar con la entidad financiera si requieres soportes adicio…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

```json camila:page
{"flow":"16.2 Creditos libranza","page":{"name":"2. Documentos solicitud de crédito","entry":{"messages":[{"type":"text","texts":["Los documentos que requieres para hacer este proceso con la entidad son:\n\n✅ 3 últimos desprendibles de nómina\n✅Certificado Laboral\n✅Debes validar con la entidad financiera si requieres soportes adicionales.\n\nEl tiempo estimado para que la compañía realice el estudio de la capacidad de endeudamiento y notifique a la entidad financiera es de 3 a 5 días hábiles. \uD83D\uDCB0 "],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

