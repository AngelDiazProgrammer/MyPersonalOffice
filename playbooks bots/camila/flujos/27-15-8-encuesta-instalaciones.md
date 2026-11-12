---
bot: camila
flow: 15.8 Encuesta instalaciones
---
## Flujo · 15.8 Encuesta instalaciones

**Qué dice al empezar:** «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, queremos conocer tu…»

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
{"name":"15.8 Encuesta instalaciones","routes":[{"condition":"true","targetPage":"0.Encuesta","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 15.8 Encuesta instalaciones › 0.Encuesta

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, queremos conocer tu opinión acerca de las instalaciones de …»
2. 💬 Dice: «Califique de acuerdo con las siguientes opciones cómo es tu experiencia frente a las instalaciones de la sede:»
3. 🔘 Ofrece los botones: «Muy satisfactoria» · «Satisfactoria» · «Insatisfactoria» · «Muy Insatisfactoria»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

```json camila:page
{"flow":"15.8 Encuesta instalaciones","page":{"name":"0.Encuesta","entry":{"messages":[{"type":"text","texts":["Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas \uD83D\uDE03. Por ello, queremos conocer tu opinión acerca de las instalaciones de nuestra compañía."],"buttons":[]},{"type":"text","texts":["Califique de acuerdo con las siguientes opciones cómo es tu experiencia frente a las instalaciones de la sede:"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Muy satisfactoria","event":"muy_satisfactoria"},{"text":"Satisfactoria","event":"satisfactoria"},{"text":"Insatisfactoria","event":"insatisfecha"},{"text":"Muy Insatisfactoria","event":"muy_insatisfactoria"}]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"End Session","fulfillment":{"messages":[],"setParams":[],"webhook":"Finalizar Conversacion","tag":"EndConversationWithPersonalizedMessage","generators":[]}}],"events":[],"groups":[]}}
```

