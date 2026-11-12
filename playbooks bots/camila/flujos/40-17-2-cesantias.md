---
bot: camila
flow: 17.2 Cesantias
---
## Flujo · 17.2 Cesantias

**Qué dice al empezar:** «¿En que puedo ayudarte?»

**Se llega desde:**
- 17. Ex colaborador › 1. ¿En que puedo ayudarte? ← evento `cesantias`

**Pide al usuario:** `numDoc` (0.Respuesta), `numContrato` (0.Respuesta)

**Páginas:**
- 0.Respuesta — ⌨️ pide datos
- 1.Elección 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0.Respuesta»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"17.2 Cesantias","routes":[{"condition":"true","targetPage":"0.Respuesta","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 17.2 Cesantias › 0.Respuesta

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `numDoc` — @sys.any · pregunta: «Digita el número de documento:»
- `numContrato` — @sys.any · pregunta: «Selecciona el número de contrato a consultar»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «1.Elección»

```json camila:page
{"flow":"17.2 Cesantias","page":{"name":"0.Respuesta","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"numDoc","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Digita el número de documento:"],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"numContrato","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Selecciona el número de contrato a consultar"],"buttons":[]}],"setParams":[],"generators":[]}}],"routes":[{"condition":"$page.params.status = \"FINAL\"","targetPage":"1.Elección","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 17.2 Cesantias › 1.Elección

**Al entrar, en este orden:**

1. 💬 Dice: «¿En que puedo ayudarte?»
2. 🔘 Ofrece los botones: «📅 Mismo año» · «🗓️ Otro año»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «🗓️ Otro año» (`anioDiferente`) → flujo **12. ¿Algo Mas?**  
  ↳ 💬 dice «A continuación te remitimos tu carta de retiro de cesantias …»
- «📅 Mismo año» (`mismoAnio`) → flujo **12. ¿Algo Mas?**  
  ↳ 💬 dice «Por el periodo laborado tus cesantías se encuentran cancelad…»

```json camila:page
{"flow":"17.2 Cesantias","page":{"name":"1.Elección","entry":{"messages":[{"type":"text","texts":["¿En que puedo ayudarte?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"\uD83D\uDCC5 Mismo año","event":"mismoAnio"},{"text":"\uD83D\uDDD3️ Otro año","event":"anioDiferente"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"anioDiferente","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[{"type":"text","texts":["A continuación te remitimos tu carta de retiro de cesantias para que realices el proceso con el fondo✅"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"mismoAnio","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[{"type":"text","texts":["Por el periodo laborado tus cesantías se encuentran canceladas en la liquidación ✅"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}}
```

