---
bot: camila
flow: 10. Cierre de conversación
---
## Flujo · 10. Cierre de conversación

**Qué dice al empezar:** «Gracias por tu tiempo. ¡Conversamos en una próxima oportunidad! Recuerda que en OUTSOURCING nos mueve el propósito de Mejorar Vidas. Nuestro Compromiso. ❤️»

**Se llega desde:**
- 01. Indicadores De Calidad › 002.UsuarioNoExiste ← condición
- 01. Indicadores De Calidad › 003.UsuarioInactivo ← condición
- 01. Indicadores De Calidad › 06. ¿Algo mas? ← evento `no`
- 02. Mi Evaluación De Desempeño › 03. ¿Algo mas? ← evento `no`
- 03. Líder Misión › 03. ¿Algo Mas? ← evento `no`
- 04. Referidos › 00. Referidos ← condición
- 08.Te escuchamos › 05. Despedida ← siempre
- 09.Convenios › 04. Despedida ← siempre
- … y 5 más

**Webhooks que usa:** ✍️ Finalizar Conversacion

**Páginas:**
- 01. Cerrar Conversacion — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.cierre = true` → página «01. Cerrar Conversacion»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"10. Cierre de conversación","routes":[{"condition":"$session.params.cierre = true","targetPage":"01. Cerrar Conversacion","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 10. Cierre de conversación › 01. Cerrar Conversacion

**Al entrar, en este orden:**

1. 💬 Dice: «Gracias por tu tiempo. ¡Conversamos en una próxima oportunidad! Recuerda que en OUTSOURCING nos mueve el propósito de Mejorar Vidas. Nuestro Compromiso. ❤️»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.cierre = true` → 🔚 **termina la conversación**  
  ↳ 📌 asigna `cierre` = `false` · ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

```json camila:page
{"flow":"10. Cierre de conversación","page":{"name":"01. Cerrar Conversacion","entry":{"messages":[{"type":"text","texts":["Gracias por tu tiempo. ¡Conversamos en una próxima oportunidad!\nRecuerda que en OUTSOURCING nos mueve el propósito de Mejorar Vidas. Nuestro Compromiso. ❤️"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"$session.params.cierre = true","targetPage":"End Session","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.cierre","value":false}],"webhook":"Finalizar Conversacion","tag":"EndConversationWithPersonalizedMessage","generators":[]}}],"events":[],"groups":[]}}
```

