---
bot: camila
flow: 15.9 Lider mision
---
## Flujo · 15.9 Lider mision

**Qué dice al empezar:** «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, queremos que valore…»

**Se llega desde:**
- flujo 15. Viaje del colaborador ← condición

**Webhooks que usa:** ✍️ Finalizar Conversacion

**Páginas:**
- 0.Encuesta — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0.Encuesta»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"15.9 Lider mision","routes":[{"condition":"true","targetPage":"0.Encuesta","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 15.9 Lider mision › 0.Encuesta

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, queremos que valores a tu líder en los atributos: ganador, …»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

```json camila:page
{"flow":"15.9 Lider mision","page":{"name":"0.Encuesta","entry":{"messages":[{"type":"text","texts":["Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas \uD83D\uDE03. Por ello, queremos que valores a tu líder en los atributos: ganador, inspirador y estratega. Ingresa aquí \uD83D\uDC49 XXX"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"End Session","fulfillment":{"messages":[],"setParams":[],"webhook":"Finalizar Conversacion","tag":"EndConversationWithPersonalizedMessage","generators":[]}}],"events":[],"groups":[]}}
```

