---
bot: camila
flow: 12. ¿Algo Mas?
---
## Flujo · 12. ¿Algo Mas?

**Qué dice al empezar:** «¿Algo más en lo que pueda ayudarte?»

**Se llega desde:**
- 02. Mi Evaluación De Desempeño › 01. Resultados Evaluación De Desempeño ← condición
- 02. Mi Evaluación De Desempeño › 02. No Resultados Evaluación De Desempeño ← condición
- 02.1. Conversacional Evaluacion Desempeño › 05. Deseas generar otro plan? ← evento `no`
- 02.1. Conversacional Evaluacion Desempeño › 05. Deseas generar otro plan? ← intent «route_RespuestaNegativa»
- 02.1. Conversacional Evaluacion Desempeño › PDI - Validar segunda competencia ← condición
- 04. Referidos › 04. ¿Otro Referido? ← evento `no`
- 05. Reconoser › 02.NoConocerIndicadoresReconoser ← condición
- 16.1 Pagos › 1. Dudas Pagos ← siempre
- … y 23 más

**Páginas:**
- 01. ¿Algo mas? 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «01. ¿Algo mas?»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"12. ¿Algo Mas?","routes":[{"condition":"true = true","targetPage":"01. ¿Algo mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 12. ¿Algo Mas? › 01. ¿Algo mas?

**Al entrar, en este orden:**

1. 📌 Asigna: `intencion` = ``
2. 💬 Dice: «¿Algo más en lo que pueda ayudarte?»
3. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → flujo **Default Start Flow**  
  ↳ 📌 asigna `menu` = `true`, `desempeno` = `false`, `liderMision` = `false`, `referidos` = `false` (+1)
- «No» (`no`) → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true`
💬 *Si el usuario escribe algo que coincide con:*
- intent «route_RespuestaAfirmativa» (ej. «Si quiero actualizar mis datos», «Si») → flujo **Default Start Flow**  
  ↳ 📌 asigna `menu` = `true`, `desempeno` = `false`, `liderMision` = `false`, `referidos` = `false` (+1)
- intent «route_RespuestaNegativa» (ej. «No, gracias», «No gracias») → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true`

```json camila:page
{"flow":"12. ¿Algo Mas?","page":{"name":"01. ¿Algo mas?","entry":{"messages":[{"type":"text","texts":["¿Algo más en lo que pueda ayudarte?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si"},{"text":"No","event":"no"}]}],"setParams":[{"parameter":"$session.params.intencion"}],"generators":[]},"form":[],"routes":[{"intent":"route_RespuestaAfirmativa","targetFlow":"Default Start Flow","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.menu","value":true},{"parameter":"$session.params.desempeno","value":false},{"parameter":"$session.params.liderMision","value":false},{"parameter":"$session.params.referidos","value":false},{"parameter":"$session.params.actualizarDatosDatix","value":false}],"generators":[]}},{"intent":"route_RespuestaNegativa","targetFlow":"10. Cierre de conversación","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.cierre","value":true}],"generators":[]}}],"events":[{"event":"si","targetFlow":"Default Start Flow","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.menu","value":true},{"parameter":"$session.params.desempeno","value":false},{"parameter":"$session.params.liderMision","value":false},{"parameter":"$session.params.referidos","value":false},{"parameter":"$session.params.actualizarDatosDatix","value":false}],"generators":[]}},{"event":"no","targetFlow":"10. Cierre de conversación","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.cierre","value":true}],"generators":[]}}],"groups":[]}}
```

