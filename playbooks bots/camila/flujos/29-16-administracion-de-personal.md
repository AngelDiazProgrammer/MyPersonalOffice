---
bot: camila
flow: 16. Administración de personal
---
## Flujo · 16. Administración de personal

**Qué dice al empezar:** «Genial! ¿En que puedo ayudarte?»

**Se llega desde:**
- Default Start Flow › 05.Menu_Principal ← evento `administracionPersonal`

**Páginas:**
- 1. ¿En que te puedo ayudar? 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «1. ¿En que te puedo ayudar?»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"16. Administración de personal","routes":[{"condition":"true","targetPage":"1. ¿En que te puedo ayudar?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 16. Administración de personal › 1. ¿En que te puedo ayudar?

**Al entrar, en este orden:**

1. 💬 Dice: «Genial! ¿En que puedo ayudarte?»
2. 🔘 Ofrece los botones: «💵 1. Pagos» · «💳 2. Libranza» · «🏥 3. EPS» · «🏢 4. Caja comp.» · «💰 5. Cesantías» · «🪪 6. Carné» · «📄 7. Certif.» · «🛡️ 8. Seg. Social»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «💵 1. Pagos» (`pagos`) → flujo **16.1 Pagos**  
  ↳ 📌 asigna `intencion` = `route_AP_Pagos`
- «💳 2. Libranza» (`creditoLibranza`) → flujo **16.2 Creditos libranza**  
  ↳ 📌 asigna `intencion` = `route_AP_CreditoLibranza`
- «🏢 4. Caja comp.» (`cajaCompensacion`) → flujo **16.4 Caja compensacion**  
  ↳ 📌 asigna `intencion` = `route_AP_CajaCompensacion`
- «🏥 3. EPS» (`epsTramite`) → flujo **16.3 EPS**  
  ↳ 📌 asigna `intencion` = `route_AP_EPSTramite`
- «💰 5. Cesantías» (`cesantias`) → flujo **16.5 Cesantias**  
  ↳ 📌 asigna `intencion` = `route_AP_Cesantias`
- «🪪 6. Carné» (`carne`) → flujo **16.6 carne**  
  ↳ 📌 asigna `intencion` = `route_AP_Carne`
- «📄 7. Certif.» (`certificados`) → flujo **16.7 certificados**  
  ↳ 📌 asigna `intencion` = `route_AP_Certificados`
- «🛡️ 8. Seg. Social» (`seguridadSocial`) → flujo **16.8 Seguridad Social**  
  ↳ 📌 asigna `intencion` = `route_AP_SeguridadSocial`

```json camila:page
{"flow":"16. Administración de personal","page":{"name":"1. ¿En que te puedo ayudar?","entry":{"messages":[{"type":"text","texts":["Genial! ¿En que puedo ayudarte?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"\uD83D\uDCB5 1. Pagos","event":"pagos"},{"text":"\uD83D\uDCB3 2. Libranza","event":"creditoLibranza"},{"text":"\uD83C\uDFE5 3. EPS","event":"epsTramite"},{"text":"\uD83C\uDFE2 4. Caja comp.","event":"cajaCompensacion"},{"text":"\uD83D\uDCB0 5. Cesantías","event":"cesantias"},{"text":"\uD83E\uDEAA 6. Carné","event":"carne"},{"text":"\uD83D\uDCC4 7. Certif.","event":"certificados"},{"text":"\uD83D\uDEE1️ 8. Seg. Social","event":"seguridadSocial"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"pagos","targetFlow":"16.1 Pagos","fulfillment":{"messages":[],"setParams":[{"parameter":"intencion","value":"route_AP_Pagos"}],"generators":[]}},{"event":"creditoLibranza","targetFlow":"16.2 Creditos libranza","fulfillment":{"messages":[],"setParams":[{"parameter":"intencion","value":"route_AP_CreditoLibranza"}],"generators":[]}},{"event":"cajaCompensacion","targetFlow":"16.4 Caja compensacion","fulfillment":{"messages":[],"setParams":[{"parameter":"intencion","value":"route_AP_CajaCompensacion"}],"generators":[]}},{"event":"epsTramite","targetFlow":"16.3 EPS","fulfillment":{"messages":[],"setParams":[{"parameter":"intencion","value":"route_AP_EPSTramite"}],"generators":[]}},{"event":"cesantias","targetFlow":"16.5 Cesantias","fulfillment":{"messages":[],"setParams":[{"parameter":"intencion","value":"route_AP_Cesantias"}],"generators":[]}},{"event":"carne","targetFlow":"16.6 carne","fulfillment":{"messages":[],"setParams":[{"parameter":"intencion","value":"route_AP_Carne"}],"generators":[]}},{"event":"certificados","targetFlow":"16.7 certificados","fulfillment":{"messages":[],"setParams":[{"parameter":"intencion","value":"route_AP_Certificados"}],"generators":[]}},{"event":"seguridadSocial","targetFlow":"16.8 Seguridad Social","fulfillment":{"messages":[],"setParams":[{"parameter":"intencion","value":"route_AP_SeguridadSocial"}],"generators":[]}}],"groups":[]}}
```

