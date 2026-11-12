---
bot: camila
flow: 06. Mi proceso De Selección
---
## Flujo · 06. Mi proceso De Selección

**Se llega desde:**
- Default Start Flow › 04.Ingreso_Existoso ← condición
- Default Start Flow › 05.Menu_Principal ← evento `seleccion`
- flujo (global) ← intent «route_ProcesoDeSeleccion»
- flujo Default Start Flow ← intent «route_ProcesoDeSeleccion»

**Webhooks que usa:** ⚡ Extraer información candidato de AIRA · ⚡ Extraer información candidato de W · ✍️ Finalizar Conversacion

**Páginas:**
- 00. Definicion de variables para consultas 
- 01. Mi_Proceso_De_Seleccion_AIRA — ⚡ Extraer información candidato de AIRA
- 02. Mi_Proceso_De_Seleccion_W — ⚡ Extraer información candidato de W
- 03. Definición de resultados globales 
- 04. Definición de resultados Aira 
- 05. Definición de resultados W 
- 06. Frases estado y fase W 
- 07. Frases estado AIRA 
- 08. Cierre flujo — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.seleccion = true` → página «00. Definicion de variables para consultas»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"06. Mi proceso De Selección","routes":[{"condition":"$session.params.seleccion = true","targetPage":"00. Definicion de variables para consultas","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 06. Mi proceso De Selección › 00. Definicion de variables para consultas

**Al entrar, en este orden:**

_No hace nada al entrar._

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `estadoW`, `faseW`, `estadoAira`, `globalAira`, `globalW`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «01. Mi_Proceso_De_Seleccion_AIRA»

```json camila:page
{"flow":"06. Mi proceso De Selección","page":{"name":"00. Definicion de variables para consultas","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"estadoW","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"faseW","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"estadoAira","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"globalAira","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"globalW","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"true","targetPage":"01. Mi_Proceso_De_Seleccion_AIRA","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 06. Mi proceso De Selección › 01. Mi_Proceso_De_Seleccion_AIRA

**Al entrar, en este orden:**

1. ⚡ Llama al webhook **Extraer información candidato de AIRA** [ConsultaCandidatoAIRA] — lectura

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «02. Mi_Proceso_De_Seleccion_W»
🔘 *Si el usuario pulsa un botón:*
- evento `sys.webhook-error` → página «02. Mi_Proceso_De_Seleccion_W»

```json camila:page
{"flow":"06. Mi proceso De Selección","page":{"name":"01. Mi_Proceso_De_Seleccion_AIRA","entry":{"messages":[],"setParams":[],"webhook":"Extraer información candidato de AIRA","tag":"ConsultaCandidatoAIRA","generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"02. Mi_Proceso_De_Seleccion_W","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.webhook-error","targetPage":"02. Mi_Proceso_De_Seleccion_W","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 06. Mi proceso De Selección › 02. Mi_Proceso_De_Seleccion_W

**Al entrar, en este orden:**

1. ⚡ Llama al webhook **Extraer información candidato de W** [https://default140443af8e794fceb3edde00131298.4f.environment.api.powerplatform.com/powerautomate/automations/direct/workflows/6593822f883f4cdc87c79560e93e5f22/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=-jHB9R2eCIWAjMPQ9z6dKoDv8xgfGfeKuySL1akSZXA] — lectura

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «03. Definición de resultados globales»

```json camila:page
{"flow":"06. Mi proceso De Selección","page":{"name":"02. Mi_Proceso_De_Seleccion_W","entry":{"messages":[],"setParams":[],"webhook":"Extraer información candidato de W","tag":"https://default140443af8e794fceb3edde00131298.4f.environment.api.powerplatform.com/powerautomate/automations/direct/workflows/6593822f883f4cdc87c79560e93e5f22/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=-jHB9R2eCIWAjMPQ9z6dKoDv8xgfGfeKuySL1akSZXA","generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"03. Definición de resultados globales","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 06. Mi proceso De Selección › 03. Definición de resultados globales

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.estadoW != null AND $session.params.estadoW != "NULL"` → página «06. Frases estado y fase W»
- si `$session.params.estadoAira != null` → página «07. Frases estado AIRA»
- si `$session.params.estadoW = null AND $session.params.estadoAira = null` → página «08. Cierre flujo»  
  ↳ 💬 dice «No encontramos un proceso de selección activo asociado a tu …»

```json camila:page
{"flow":"06. Mi proceso De Selección","page":{"name":"03. Definición de resultados globales","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"$session.params.estadoW != null AND $session.params.estadoW != \"NULL\"","targetPage":"06. Frases estado y fase W","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.estadoAira != null","targetPage":"07. Frases estado AIRA","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.estadoW = null AND $session.params.estadoAira = null","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["No encontramos un proceso de selección activo asociado a tu número de documento. Si consideras que es un error, por favor comunícate con el área de selección."],"buttons":[]}],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 06. Mi proceso De Selección › 04. Definición de resultados Aira

**Al entrar, en este orden:**

_No hace nada al entrar._

```json camila:page
{"flow":"06. Mi proceso De Selección","page":{"name":"04. Definición de resultados Aira","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[],"groups":[]}}
```

### Página · 06. Mi proceso De Selección › 05. Definición de resultados W

**Al entrar, en este orden:**

_No hace nada al entrar._

```json camila:page
{"flow":"06. Mi proceso De Selección","page":{"name":"05. Definición de resultados W","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[],"groups":[]}}
```

### Página · 06. Mi proceso De Selección › 06. Frases estado y fase W

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.faseW = "Capacitacion" AND $session.params.estadoW = "Agrupado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Bienvenido/a a Formación Outsourcing!   Nos alegra acompaña…»
- si `$session.params.faseW = "Decision" AND $session.params.estadoW = "Pendiente"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Hola! Actualmente nuestra área de selección se encuentra re…»
- si `$session.params.faseW = "Decision" AND $session.params.estadoW = "Rechazado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Hola! Actualmente nuestra área de selección se encuentra re…»
- si `$session.params.faseW = "Examen Medico" AND ($session.params.estadoW = "Examen Medico" OR $session.params.estadoW = "Programado")` → página «08. Cierre flujo»  
  ↳ 💬 dice «Actualmente estás en la etapa de examenes médicos (pendiente…»
- si `$session.params.faseW = "Examen Medico" AND $session.params.estadoW = "SST"` → página «08. Cierre flujo»  
  ↳ 💬 dice «Actualmente estás en la etapa de examenes médicos (en progra…»
- si `$session.params.faseW = "Examen Medico" AND $session.params.estadoW = "Rechazado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «Gracias por tu interés en formar parte de Outsourcing S.A.S.…»
- si `$session.params.faseW = "Capacitacion" AND $session.params.estadoW = "Capacitacion"` → página «08. Cierre flujo»  
  ↳ 💬 dice «Muy pronto iniciarás tu proceso de formación. En un tiempo m…»
- si `$session.params.faseW = "Capacitacion" AND $session.params.estadoW = "Evaluado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Has concluido tu etapa formativa! Nos enorgullece que hayas…»
- si `$session.params.faseW = "Capacitacion" AND $session.params.estadoW = "Rechazado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «Queremos agradecerte por el compromiso y la disposición que …»
- si `$session.params.faseW = "Contratacion" AND $session.params.estadoW = "Generado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Has sido seleccionado! Muy pronto el área de contratación t…»
- si `$session.params.faseW = "Contratacion" AND $session.params.estadoW = "Contratacion"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Has sido seleccionado! Muy pronto el área de contratación t…»
- si `$session.params.faseW = "Contratacion" AND $session.params.estadoW = "Enviado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Hola!  Hemos enviado tu contrato al correo electrónico regi…»
- si `$session.params.faseW = "Contratacion" AND $session.params.estadoW = "Contratado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Felicitaciones! Te damos la bienvenida a Outsourcing S.A.S.…»
- si `$session.params.faseW = "Contratacion" AND $session.params.estadoW = "Rechazado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «Hola, nuestro equipo de selección se pondrá en contacto cont…»
- siempre → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Hola! Actualmente nuestra área de selección se encuentra re…»

```json camila:page
{"flow":"06. Mi proceso De Selección","page":{"name":"06. Frases estado y fase W","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"$session.params.faseW = \"Capacitacion\" AND $session.params.estadoW = \"Agrupado\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["¡Bienvenido/a a Formación Outsourcing! \n\nNos alegra acompañarte en el inicio de esta nueva experiencia como futuro/a Embajador/a de Marca, por favor revisar tu citación a formación con fecha, lugar y persona de contacto.\n\n\n\uD83D\uDCCC Recuerda traer un documento diferente a la cédula para tu ingreso.\n\n¡Te esperamos para comenzar juntos este nuevo reto! "],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.faseW = \"Decision\" AND $session.params.estadoW = \"Pendiente\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["¡Hola! Actualmente nuestra área de selección se encuentra realizando la programación del siguiente paso de tu proceso.En las próximas 24 horas te notificaremos a tu correo electrónico  la información correspondiente."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.faseW = \"Decision\" AND $session.params.estadoW = \"Rechazado\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["¡Hola! Actualmente nuestra área de selección se encuentra realizando la programación del siguiente paso de tu proceso. En las próximas 24 horas te notificaremos a tu correo electrónico la información correspondiente."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.faseW = \"Examen Medico\" AND ($session.params.estadoW = \"Examen Medico\" OR $session.params.estadoW = \"Programado\")","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["Actualmente estás en la etapa de examenes médicos (pendiente de programación).\r\nDebes estar atento a las indicaciones de nuestro equipo de selección para avanzar al siguiente paso."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.faseW = \"Examen Medico\" AND $session.params.estadoW = \"SST\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["Actualmente estás en la etapa de examenes médicos (en programación).\r\nDebes estar atento a las indicaciones del equipo de Seguridad y Salud en el Trabajo para avanzar al siguiente paso."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.faseW = \"Examen Medico\" AND $session.params.estadoW = \"Rechazado\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["Gracias por tu interés en formar parte de Outsourcing S.A.S. BIC y el tiempo que dedicaste a participar en nuestro proceso de selección.\r\nDespués de una evaluación integral, queremos informarte que en esta oportunidad tu perfil no se ajusta a las necesidades actuales del cargo; sin embargo, guardaremos tu hoja de vida en nuestra base de datos para futuras convocatorias. Te deseamos éxitos en tus proyectos futuros. "],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.faseW = \"Capacitacion\" AND $session.params.estadoW = \"Capacitacion\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["Muy pronto iniciarás tu proceso de formación. En un tiempo máximo de 2 días recibirás en tu correo electrónico la información detallada sobre el lugar, fecha y horarios."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.faseW = \"Capacitacion\" AND $session.params.estadoW = \"Evaluado\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["¡Has concluido tu etapa formativa! Nos enorgullece que hayas llegado hasta aquí y agradecemos tu compromiso durante este proceso.\n\nTu próximo paso será asistir a tus exámenes médicos. ¡Te deseamos muchos éxitos en esta nueva etapa! "],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.faseW = \"Capacitacion\" AND $session.params.estadoW = \"Rechazado\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["Queremos agradecerte por el compromiso y la disposición que demostraste durante tu proceso formativo con nosotros.\n\nEn esta oportunidad no continuarás a la siguiente etapa del proceso; sin embargo, valoramos el tiempo y esfuerzo invertido durante tu capacitación."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.faseW = \"Contratacion\" AND $session.params.estadoW = \"Generado\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["¡Has sido seleccionado! Muy pronto el área de contratación te contactará para formalizar tu ingreso y realizar la firma de contrato."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.faseW = \"Contratacion\" AND $session.params.estadoW = \"Contratacion\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["¡Has sido seleccionado! Muy pronto el área de contratación te contactará para formalizar tu ingreso y realizar la firma de contrato."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.faseW = \"Contratacion\" AND $session.params.estadoW = \"Enviado\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["¡Hola!\r\nHemos enviado tu contrato al correo electrónico registrado. Te invitamos a revisarlo detalladamente y realizar la firma dentro de los tiempos establecidos, los cuales encontrarás especificados en la comunicación."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.faseW = \"Contratacion\" AND $session.params.estadoW = \"Contratado\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["¡Felicitaciones! Te damos la bienvenida a Outsourcing S.A.S. BIC.\r\nHemos recibido satisfactoriamente tu contrato firmado y agradecemos la confianza depositada en nuestra compañía. A partir de este momento, debes estar atento/a al mensaje de texto y correo donde te llegarán las indicaciones para que te presentes al inicio de tus labores."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.faseW = \"Contratacion\" AND $session.params.estadoW = \"Rechazado\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":[" Hola, nuestro equipo de selección se pondrá en contacto contigo para brindarte más información y revisar el estado de tu participación en la vacante."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"true","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["¡Hola! Actualmente nuestra área de selección se encuentra realizando la programación del siguiente paso de tu proceso. En las próximas 24 horas te notificaremos a tu correo electrónico la información correspondiente."],"buttons":[]}],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 06. Mi proceso De Selección › 07. Frases estado AIRA

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.estadoAira = "Pruebas"` → página «08. Cierre flujo»  
  ↳ 💬 dice «Has avanzado a la etapa de pruebas Psicotécnicas. Revisa tu …»
- si `$session.params.estadoAira = "Entrevista"` → página «08. Cierre flujo»  
  ↳ 💬 dice «Tus pruebas psicotécnicas tuvieron buenos resultados; avanza…»
- si `$session.params.estadoAira = "Antecedentes"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Gracias por postularte! En este momento estamos revisando s…»
- si `$session.params.estadoAira = "Contratado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Felicitaciones! 🎉 Tu proceso de selección ha finalizado co…»

```json camila:page
{"flow":"06. Mi proceso De Selección","page":{"name":"07. Frases estado AIRA","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"$session.params.estadoAira = \"Pruebas\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["Has avanzado a la etapa de pruebas Psicotécnicas. Revisa tu correo electrónico en la bandeja de entrada o en el Spam; e ingresa al enlace para realizarlas. Tiempo límite para completarlas: 1 día."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.estadoAira = \"Entrevista\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["Tus pruebas psicotécnicas tuvieron buenos resultados; avanzarás a la fase de entrevista. En un tiempo máximo de dos días te llegará a tu correo electrónico la notificación con el enlace y las indicaciones para tu entrevista. ¡Nos vemos pronto!"],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.estadoAira = \"Antecedentes\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["¡Gracias por postularte! En este momento estamos revisando si tu perfil cumple con los requisitos del cargo o si por el contrario, tu HV queda en nuestra base para futuras oportunidades laborales. Recibirás un mensaje de continuidad en el proceso o de agradecimiento por tu postulación. "],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.estadoAira = \"Contratado\"","targetPage":"08. Cierre flujo","fulfillment":{"messages":[{"type":"text","texts":["¡Felicitaciones! \uD83C\uDF89 Tu proceso de selección ha finalizado con éxito y te encuentras registrado(a) como Contratado. Muy pronto el equipo de Atracción de Talento y Gestión Humana se comunicará contigo para indicarte la fecha y los detalles de tu inducción. ¡Bienvenido(a) a la familia Outsourcing!"],"buttons":[]}],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 06. Mi proceso De Selección › 08. Cierre flujo

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

```json camila:page
{"flow":"06. Mi proceso De Selección","page":{"name":"08. Cierre flujo","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"End Session","fulfillment":{"messages":[],"setParams":[],"webhook":"Finalizar Conversacion","tag":"EndConversationWithPersonalizedMessage","generators":[]}}],"events":[],"groups":[]}}
```

