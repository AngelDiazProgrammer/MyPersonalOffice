---
bot: camila
flow: 17.3 Certificados
---
## Flujo · 17.3 Certificados

**Qué dice al empezar:** «Selecciona el tipo de certificado que necesites»

**Se llega desde:**
- 17. Ex colaborador › 1. ¿En que puedo ayudarte? ← evento `certificados`

**Webhooks que usa:** ⚡ Login_API_Kairos · ✍️ certificadosAdminPersonal-Basico · ✍️ certificadosAdminPersonal-Ingresos

**Pide al usuario:** `documento` (1. datos), `fechaExpedicion` (1. datos), `correoElectronico` (3. correo certificado basico), `mesCertificado` (3. correo certificado basico), `anoCertificado` (3. correo certificado basico), `mesFinalCertificado` (3. correo certificado basico), `anoFinalCertificado` (3. correo certificado basico), `anioCertificado` (4. info ingresos y retenciones), `correoIngresos` (4. info ingresos y retenciones), `mesCertificado` (5. envio datos certificado), `anioCertificado` (5. envio datos certificado), `correoElectronico` (5. envio datos certificado), `finalMesCertificado` (5. envio datos certificado), `finalAnioCertificado` (5. envio datos certificado)

**Páginas:**
- 1. datos — ⚡ Login_API_Kairos · ⌨️ pide datos
- 2.seleccionar certificado 
- 3. correo certificado basico — ✍️ certificadosAdminPersonal-Basico · ⌨️ pide datos
- 4. info ingresos y retenciones — ✍️ certificadosAdminPersonal-Ingresos · ⌨️ pide datos
- 5. envio datos certificado — ✍️ certificadosAdminPersonal-Basico · ⌨️ pide datos

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «1. datos»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"17.3 Certificados","routes":[{"condition":"true","targetPage":"1. datos","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 17.3 Certificados › 1. datos

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `documento` — @sys.any · pregunta: «Digita el número de documento»
- `fechaExpedicion` — @sys.any · pregunta: «Ingresa fecha de expedición de tu documento DD-MM-AAAA»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «2.seleccionar certificado»  
  ↳ ⚡ Llama al webhook **Login_API_Kairos** [Login] — lectura

```json camila:page
{"flow":"17.3 Certificados","page":{"name":"1. datos","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"documento","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Digita el número de documento"],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"fechaExpedicion","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Ingresa fecha de expedición de tu documento DD-MM-AAAA"],"buttons":[]}],"setParams":[],"generators":[]}}],"routes":[{"condition":"$page.params.status = \"FINAL\"","targetPage":"2.seleccionar certificado","fulfillment":{"messages":[],"setParams":[],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}}],"events":[],"groups":[]}}
```

### Página · 17.3 Certificados › 2.seleccionar certificado

**Al entrar, en este orden:**

1. 💬 Dice: «Selecciona el tipo de certificado que necesites»
2. 🔘 Ofrece los botones: «📄 Cert. Básico» · «📄 Laboral especial» · «💰 Ingresos y rete.»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `tipoCertificado`

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «📄 Laboral especial» (`laboralEspecial`) → flujo **12. ¿Algo Mas?**  
  ↳ 💬 dice «Para solicitar el certificado laboral especial con cargos u …»
- «📄 Cert. Básico» (`certificadoBasico`) → página «3. correo certificado basico»
- «💰 Ingresos y rete.» (`ingresosRetenciones`) → página «4. info ingresos y retenciones»  
  ↳ 📌 asigna `tipoCertificado` = `ingresos`

```json camila:page
{"flow":"17.3 Certificados","page":{"name":"2.seleccionar certificado","entry":{"messages":[{"type":"text","texts":["Selecciona el tipo de certificado que necesites"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"\uD83D\uDCC4 Cert. Básico","event":"certificadoBasico"},{"text":"\uD83D\uDCC4 Laboral especial","event":"laboralEspecial"},{"text":"\uD83D\uDCB0 Ingresos y rete.","event":"ingresosRetenciones"}]}],"setParams":[],"generators":[]},"form":[{"name":"tipoCertificado","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[],"events":[{"event":"laboralEspecial","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[{"type":"text","texts":["Para solicitar el certificado laboral especial con cargos u otras especificaciones (funciones, dirigido a una empresa en particular, entre otros), debes enviar tu solicitud al correo certificacioneslaborales@outsourcing.com.co, indicando los datos y las especificaciones requeridas \uD83D\uDCE7"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"certificadoBasico","targetPage":"3. correo certificado basico","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"ingresosRetenciones","targetPage":"4. info ingresos y retenciones","fulfillment":{"messages":[],"setParams":[{"parameter":"tipoCertificado","value":"ingresos"}],"generators":[]}}],"groups":[]}}
```

### Página · 17.3 Certificados › 3. correo certificado basico

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `correoElectronico` — @sys.any · pregunta: «Por favor escribe el correo electrónico que registraste con nosotros para recibir tu certificado laboral»
- `mesCertificado` — @sys.any · pregunta: «Por favor escribe el mes desde donde quieres que se genere el certificado.»
- `anoCertificado` — @sys.any · pregunta: «Escribe el año desde donde desees solicitar el certificado. Recuerda que solo se puede solicitar a partir del año 2017, …»
- `mesFinalCertificado` — @sys.any · pregunta: «Por favor escribe el mes final para realizar la consulta.»
- `anoFinalCertificado` — @sys.any · pregunta: «Escribe el año hasta donde desees solicitar el certificado.»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «5. envio datos certificado»  
  ↳ 📌 asigna `tipoCertificado` = `planilla` · ✍️ Llama al webhook **certificadosAdminPersonal-Basico** [administracion-personal-certificados] — **escritura, pide confirmación** · 💬 dice «Tu solicitud ha sido registrada exitosamente y será tramitad…»

```json camila:page
{"flow":"17.3 Certificados","page":{"name":"3. correo certificado basico","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"correoElectronico","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Por favor escribe el correo electrónico que registraste con nosotros para recibir tu certificado laboral"],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"mesCertificado","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Por favor escribe el mes desde donde quieres que se genere el certificado."],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"anoCertificado","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Escribe el año desde donde desees solicitar el certificado. Recuerda que solo se puede solicitar a partir del año 2017, si deseas este certificado de años anteriores puedes escribir al correo certificacioneslaborales@outsourcing.com.co"],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"mesFinalCertificado","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Por favor escribe el mes final para realizar la consulta."],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"anoFinalCertificado","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Escribe el año hasta donde desees solicitar el certificado."],"buttons":[]}],"setParams":[],"generators":[]}}],"routes":[{"condition":"$page.params.status = \"FINAL\"","targetPage":"5. envio datos certificado","fulfillment":{"messages":[{"type":"text","texts":["Tu solicitud ha sido registrada exitosamente y será tramitada máximo en un 1 día hábil. \uD83D\uDE03"],"buttons":[]}],"setParams":[{"parameter":"tipoCertificado","value":"planilla"}],"webhook":"certificadosAdminPersonal-Basico","tag":"administracion-personal-certificados","generators":[]}}],"events":[],"groups":[]}}
```

### Página · 17.3 Certificados › 4. info ingresos y retenciones

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `anioCertificado` — @sys.any · pregunta: «Por favor ingresa el año (Cuatro dígitos, Ejemplo: 2001) del cual necesitas generar tu certificado.»
- `correoIngresos` — @sys.any · pregunta: «Por favor escribe el correo electrónico en el cual deseas recibir tu certificado de ingresos y retenciones»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «5. envio datos certificado»  
  ↳ ✍️ Llama al webhook **certificadosAdminPersonal-Ingresos** [administracion-personal-certificados] — **escritura, pide confirmación** · 💬 dice «Tu solicitud ha sido registrada exitosamente y será tramitad…»

```json camila:page
{"flow":"17.3 Certificados","page":{"name":"4. info ingresos y retenciones","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"anioCertificado","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Por favor ingresa el año (Cuatro dígitos, Ejemplo: 2001) del cual necesitas generar tu certificado."],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"correoIngresos","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Por favor escribe el correo electrónico en el cual deseas recibir tu certificado de ingresos y retenciones"],"buttons":[]}],"setParams":[],"generators":[]}}],"routes":[{"condition":"$page.params.status = \"FINAL\"","targetPage":"5. envio datos certificado","fulfillment":{"messages":[{"type":"text","texts":["Tu solicitud ha sido registrada exitosamente y será tramitada máximo en un 1 día hábil. \uD83D\uDE03"],"buttons":[]}],"setParams":[],"webhook":"certificadosAdminPersonal-Ingresos","tag":"administracion-personal-certificados","generators":[]}}],"events":[],"groups":[]}}
```

### Página · 17.3 Certificados › 5. envio datos certificado

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `mesCertificado` — @sys.any · pregunta: «Por favor escribe el mes desde donde quieres que se genere el certificado.»
- `anioCertificado` — @sys.any · pregunta: «Escribe el año desde donde desees solicitar el certificado. Recuerda que solo se puede solicitar a partir del año 2017, …»
- `correoElectronico` — @sys.any · pregunta: «Por favor escribe el correo electrónico en el cual deseas recibir tu certificado de planillas de pago.»
- `finalMesCertificado` — @sys.any · pregunta: «Por favor escribe el mes final para realizar la consulta.»
- `finalAnioCertificado` — @sys.any · pregunta: «Escribe el año hasta donde desees solicitar el certificado.»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → flujo **12. ¿Algo Mas?**  
  ↳ ✍️ Llama al webhook **certificadosAdminPersonal-Basico** [administracion-personal-certificados] — **escritura, pide confirmación** · 💬 dice «Tu solicitud ha sido registrada exitosamente y será tramitad…»

```json camila:page
{"flow":"17.3 Certificados","page":{"name":"5. envio datos certificado","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"mesCertificado","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Por favor escribe el mes desde donde quieres que se genere el certificado."],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"anioCertificado","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Escribe el año desde donde desees solicitar el certificado. Recuerda que solo se puede solicitar a partir del año 2017, si deseas este certificado de años anteriores puedes escribir al correo certificacioneslaborales@outsourcing.com.co"],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"correoElectronico","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Por favor escribe el correo electrónico en el cual deseas recibir tu certificado de planillas de pago."],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"finalMesCertificado","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Por favor escribe el mes final para realizar la consulta."],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"finalAnioCertificado","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Escribe el año hasta donde desees solicitar el certificado."],"buttons":[]}],"setParams":[],"generators":[]}}],"routes":[{"condition":"$page.params.status = \"FINAL\"","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[{"type":"text","texts":["Tu solicitud ha sido registrada exitosamente y será tramitada máximo en un 1 día hábil. \uD83D\uDE03"],"buttons":[]}],"setParams":[],"webhook":"certificadosAdminPersonal-Basico","tag":"administracion-personal-certificados","generators":[]}}],"events":[],"groups":[]}}
```

