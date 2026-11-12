---
bot: camila
flow: 16.4 Caja compensacion
---
## Flujo · 16.4 Caja compensacion

**Qué dice al empezar:** «Si quieres adicionar a algún familiar debes ingresar al portal web de la caja de compensación familiar o comunicarte con nuestro asesor Hernan Andrade al correo…»

**Se llega desde:**
- 16. Administración de personal › 1. ¿En que te puedo ayudar? ← evento `cajaCompensacion`

**Páginas:**
- 1. Información de adición familiar 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «1. Información de adición familiar»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"16.4 Caja compensacion","routes":[{"condition":"true","targetPage":"1. Información de adición familiar","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 16.4 Caja compensacion › 1. Información de adición familiar

**Al entrar, en este orden:**

1. 💬 Dice: «Si quieres adicionar a algún familiar debes ingresar al portal web de la caja de compensación familiar o comunicarte con nuestro asesor Hernan Andrade al correo handrade@outsourcing.com.co teléfono 31…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

```json camila:page
{"flow":"16.4 Caja compensacion","page":{"name":"1. Información de adición familiar","entry":{"messages":[{"type":"text","texts":["Si quieres adicionar a algún familiar debes ingresar al portal web de la caja de compensación familiar o comunicarte con nuestro asesor Hernan Andrade al correo handrade@outsourcing.com.co teléfono 3134633109 (aplica para Bogotá y Cundinamarca).\n\nPara cajas territoriales la gestión la debes realizarla directamente con ellas."],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

