---
bot: camila
flow: 17. Ex colaborador
---
## Flujo · 17. Ex colaborador

**Qué dice al empezar:** «¿En que puedo ayudarte?»

**Se llega desde:**
- Default Start Flow › 04.Ingreso_Existoso ← condición

**Páginas:**
- 1. ¿En que puedo ayudarte? 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «1. ¿En que puedo ayudarte?»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"17. Ex colaborador","routes":[{"condition":"true","targetPage":"1. ¿En que puedo ayudarte?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 17. Ex colaborador › 1. ¿En que puedo ayudarte?

**Al entrar, en este orden:**

1. 💬 Dice: «¿En que puedo ayudarte?»
2. 🔘 Ofrece los botones: «💵 1. Pagos» · «💰 2. Cesantías» · «📄 3. Certificados»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «💵 1. Pagos» (`pagos`) → flujo **17.1 pagos**
- «💰 2. Cesantías» (`cesantias`) → flujo **17.2 Cesantias**
- «📄 3. Certificados» (`certificados`) → flujo **17.3 Certificados**

```json camila:page
{"flow":"17. Ex colaborador","page":{"name":"1. ¿En que puedo ayudarte?","entry":{"messages":[{"type":"text","texts":["¿En que puedo ayudarte?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"\uD83D\uDCB5 1. Pagos","event":"pagos"},{"text":"\uD83D\uDCB0 2. Cesantías","event":"cesantias"},{"text":"\uD83D\uDCC4 3. Certificados","event":"certificados"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"pagos","targetFlow":"17.1 pagos","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"cesantias","targetFlow":"17.2 Cesantias","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"certificados","targetFlow":"17.3 Certificados","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

