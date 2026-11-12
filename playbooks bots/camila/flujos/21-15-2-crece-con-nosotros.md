---
bot: camila
flow: 15.2 Crece con nosotros
---
## Flujo · 15.2 Crece con nosotros

**Qué dice al empezar:** «Para Outsourcing la experiencia de colaboradores indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinión acer…»

**Se llega desde:**
- 15. Viaje del colaborador › 4.despacho ← condición

**Webhooks que usa:** ✍️ Finalizar Conversacion

**Páginas:**
- 0. Encuesta — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.responderEncuestaCrece = true` → página «0. Encuesta»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"15.2 Crece con nosotros","routes":[{"condition":"$session.params.responderEncuestaCrece = true","targetPage":"0. Encuesta","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 15.2 Crece con nosotros › 0. Encuesta

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de colaboradores indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinión acerca de los espacios de formación y desarr…»
2. 🔘 Ofrece los botones: «Muy satisfactoria» · «Satisfactoria» · «Insatisfactoria» · «Muy Insatisfactoria»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

```json camila:page
{"flow":"15.2 Crece con nosotros","page":{"name":"0. Encuesta","entry":{"messages":[{"type":"text","texts":["Para Outsourcing la experiencia de colaboradores indispensable para cumplir con nuestro propósito de mejorar vidas\uD83D\uDE03. Por ello, queremos conocer tu opinión acerca de los espacios de formación y desarrollo de nuestra compañia."],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Muy satisfactoria","event":"muy_satisfactoria"},{"text":"Satisfactoria","event":"satisfactoria"},{"text":"Insatisfactoria","event":"insatisfecha"},{"text":"Muy Insatisfactoria","event":"muy_insatisfactoria"}]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"End Session","fulfillment":{"messages":[],"setParams":[],"webhook":"Finalizar Conversacion","tag":"EndConversationWithPersonalizedMessage","generators":[]}}],"events":[],"groups":[]}}
```

