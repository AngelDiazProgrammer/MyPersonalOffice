---
bot: camila
flow: 04. Referidos
---
## Flujo · 04. Referidos

**Qué dice al empezar:** «💼 ¿Cuál es el área de experiencia del referido?»

**Se llega desde:**
- Default Start Flow › 05.Menu_Principal ← evento `referidos`
- flujo (global) ← intent «route_Referidos»
- flujo Default Start Flow ← intent «route_Referidos»

**Webhooks que usa:** ✍️ Guardar_Referidos · ⚡ Obtener_Colaborador_Referidos

**Pide al usuario:** `documentoReferido` (01. Solicitar Referido), `nombreReferido` (01. Solicitar Referido), `correoReferido` (01. Solicitar Referido), `telefonoReferido` (01. Solicitar Referido)

**Páginas:**
- 00. Referidos 
- 01. Solicitar Referido — ⌨️ pide datos
- 01.1 Solicitar area de experiencia del referido 
- 01.2 Segunda lista de experiencias laborales 
- 02. ¿Otra área de experiencia? 
- 02.1. Otra área de experiencia 
- 02.2 Otra área de experiencia segunda lista 
- 03. Guardar Referido — ✍️ Guardar_Referidos
- 04. ¿Otro Referido? 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.referidos = true` → página «00. Referidos»  
  ↳ ⚡ Llama al webhook **Obtener_Colaborador_Referidos** [buscar-usuario] — lectura
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"04. Referidos","routes":[{"condition":"$session.params.referidos = true","targetPage":"00. Referidos","fulfillment":{"messages":[],"setParams":[],"webhook":"Obtener_Colaborador_Referidos","tag":"buscar-usuario","generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 04. Referidos › 00. Referidos

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.idUsuarioReferidos != null` → página «01. Solicitar Referido»  
  ↳ 💬 dice «Como colaborador de la Familia OS puedes referir a un famili…»
- si `$session.params.idUsuarioReferidos = null` → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true` · 💬 dice «Por ahora no te encuentras en la base para referir.»

```json camila:page
{"flow":"04. Referidos","page":{"name":"00. Referidos","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"$session.params.idUsuarioReferidos != null","targetPage":"01. Solicitar Referido","fulfillment":{"messages":[{"type":"text","texts":["Como colaborador de la Familia OS puedes referir a un familiar, amigo o conocido para hacer parte de nuestra compañía \uD83E\uDD1D. \r\n\r\n⚠️ Ten en cuenta que dos o más familiares no pueden trabajar en la misma cuenta o área, sin importar el grado de consanguinidad debido a posibles conflictos de interés  \uD83E\uDD1D.\r\n\r\nPara realizar el referido, te haré unas preguntas rápidas.\r\n\r\nComencemos \uD83D\uDE0A."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.idUsuarioReferidos = null","targetFlow":"10. Cierre de conversación","fulfillment":{"messages":[{"type":"text","texts":["Por ahora no te encuentras en la base para referir."],"buttons":[]}],"setParams":[{"parameter":"$session.params.cierre","value":"true"}],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 04. Referidos › 01. Solicitar Referido

**Al entrar, en este orden:**

1. 📌 Asigna: `referidos` = ``

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `documentoReferido` — @sys.any · pregunta: «🆔 Por favor, indícanos el número de documento del referido.»
- `nombreReferido` — @sys.any · pregunta: «👤 ¿Cuál es el nombre completo?»
- `correoReferido` — @sys.any · pregunta: «📧 ¿Cuál es el correo electrónico del referido?»
- `telefonoReferido` — @sys.any · pregunta: «📱 ¿Cuál es el número de teléfono del referido?»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `areaExperienciaReferido`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.documentoReferido != null AND $session.params.nombreReferido != null AND $session.params.correoReferido != null AND $session.params.telefonoReferido != null` → página «01.1 Solicitar area de experiencia del referido»

```json camila:page
{"flow":"04. Referidos","page":{"name":"01. Solicitar Referido","entry":{"messages":[],"setParams":[{"parameter":"$session.params.referidos"}],"generators":[]},"form":[{"name":"documentoReferido","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["\uD83C\uDD94 Por favor, indícanos el número de documento del referido."],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"nombreReferido","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["\uD83D\uDC64 ¿Cuál es el nombre completo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"correoReferido","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["\uD83D\uDCE7 ¿Cuál es el correo electrónico del referido?"],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"telefonoReferido","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["\uD83D\uDCF1 ¿Cuál es el número de teléfono del referido?"],"buttons":[]}],"setParams":[],"generators":[]}},{"name":"areaExperienciaReferido","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.documentoReferido != null AND $session.params.nombreReferido != null AND $session.params.correoReferido != null AND $session.params.telefonoReferido != null","targetPage":"01.1 Solicitar area de experiencia del referido","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 04. Referidos › 01.1 Solicitar area de experiencia del referido

**Al entrar, en este orden:**

1. 💬 Dice: «💼 ¿Cuál es el área de experiencia del referido?»
2. 🔘 Ofrece los botones: «💼 Comercial/Ventas» · «🎧 Atención cliente» · «📋 Admin/Financiero» · «🚚 Logística/Prod.» · «💻 Tecnología/Sist.» · «🏥 Salud» · «📚 Educación»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- evento `siguiente_pagina` → página «01.2 Segunda lista de experiencias laborales»
- «💼 Comercial/Ventas» (`comercial_ventas`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Comercial / Ventas`
- «🎧 Atención cliente» (`atencion_cliente`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Atención al cliente`
- «📋 Admin/Financiero» (`administrativo`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Administrativo/Financiero`
- «🚚 Logística/Prod.» (`logistica_operaciones`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Logística/Produccion`
- «💻 Tecnología/Sist.» (`tecnologia_sistemas`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Tecnología / Sistemas`
- «🏥 Salud» (`salud`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Salud`
- «📚 Educación» (`educacion`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Salud`

```json camila:page
{"flow":"04. Referidos","page":{"name":"01.1 Solicitar area de experiencia del referido","entry":{"messages":[{"type":"text","texts":["\uD83D\uDCBC ¿Cuál es el área de experiencia del referido?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"\uD83D\uDCBC Comercial/Ventas","event":"comercial_ventas"},{"text":"\uD83C\uDFA7 Atención cliente","event":"atencion_cliente"},{"text":"\uD83D\uDCCB Admin/Financiero","event":"administrativo"},{"text":"\uD83D\uDE9A Logística/Prod.","event":"logistica_operaciones"},{"text":"\uD83D\uDCBB Tecnología/Sist.","event":"tecnologia_sistemas"},{"text":"\uD83C\uDFE5 Salud","event":"salud"},{"text":"\uD83D\uDCDA Educación","event":"educacion"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"siguiente_pagina","targetPage":"01.2 Segunda lista de experiencias laborales","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"comercial_ventas","targetPage":"02. ¿Otra área de experiencia?","fulfillment":{"messages":[],"setParams":[{"parameter":"areaExperienciaReferido","value":"Comercial / Ventas"}],"generators":[]}},{"event":"atencion_cliente","targetPage":"02. ¿Otra área de experiencia?","fulfillment":{"messages":[],"setParams":[{"parameter":"areaExperienciaReferido","value":"Atención al cliente"}],"generators":[]}},{"event":"administrativo","targetPage":"02. ¿Otra área de experiencia?","fulfillment":{"messages":[],"setParams":[{"parameter":"areaExperienciaReferido","value":"Administrativo/Financiero"}],"generators":[]}},{"event":"logistica_operaciones","targetPage":"02. ¿Otra área de experiencia?","fulfillment":{"messages":[],"setParams":[{"parameter":"areaExperienciaReferido","value":"Logística/Produccion"}],"generators":[]}},{"event":"tecnologia_sistemas","targetPage":"02. ¿Otra área de experiencia?","fulfillment":{"messages":[],"setParams":[{"parameter":"areaExperienciaReferido","value":"Tecnología / Sistemas"}],"generators":[]}},{"event":"salud","targetPage":"02. ¿Otra área de experiencia?","fulfillment":{"messages":[],"setParams":[{"parameter":"areaExperienciaReferido","value":"Salud"}],"generators":[]}},{"event":"educacion","targetPage":"02. ¿Otra área de experiencia?","fulfillment":{"messages":[],"setParams":[{"parameter":"areaExperienciaReferido","value":"Salud"}],"generators":[]}}],"groups":[]}}
```

### Página · 04. Referidos › 01.2 Segunda lista de experiencias laborales

**Al entrar, en este orden:**

1. 💬 Dice: «💼 ¿Cuál es el área de experiencia del referido?»
2. 🔘 Ofrece los botones: «🔧 Servicios generales» · «🏛️ Sector público» · «🚀 Independiente/Empr.» · «❌ Ninguno/Sin exp.» · «⬅️ Volver»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «🔧 Servicios generales» (`servicios_generales`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Servicios generales`
- «🏛️ Sector público» (`sector_publico`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Sector público`
- «🚀 Independiente/Empr.» (`independiente_emprendimiento`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Independiente / Emprendimiento`
- «❌ Ninguno/Sin exp.» (`ninguno_sin_experiencia`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Ninguno / Sin experiencia`
- «⬅️ Volver» (`volver_pagina`) → página «01.1 Solicitar area de experiencia del referido»

```json camila:page
{"flow":"04. Referidos","page":{"name":"01.2 Segunda lista de experiencias laborales","entry":{"messages":[{"type":"text","texts":["\uD83D\uDCBC ¿Cuál es el área de experiencia del referido?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"\uD83D\uDD27 Servicios generales","event":"servicios_generales"},{"text":"\uD83C\uDFDB️ Sector público","event":"sector_publico"},{"text":"\uD83D\uDE80 Independiente/Empr.","event":"independiente_emprendimiento"},{"text":"❌ Ninguno/Sin exp.","event":"ninguno_sin_experiencia"},{"text":"⬅️ Volver","event":"volver_pagina"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"servicios_generales","targetPage":"02. ¿Otra área de experiencia?","fulfillment":{"messages":[],"setParams":[{"parameter":"areaExperienciaReferido","value":"Servicios generales"}],"generators":[]}},{"event":"sector_publico","targetPage":"02. ¿Otra área de experiencia?","fulfillment":{"messages":[],"setParams":[{"parameter":"areaExperienciaReferido","value":"Sector público"}],"generators":[]}},{"event":"independiente_emprendimiento","targetPage":"02. ¿Otra área de experiencia?","fulfillment":{"messages":[],"setParams":[{"parameter":"areaExperienciaReferido","value":"Independiente / Emprendimiento"}],"generators":[]}},{"event":"ninguno_sin_experiencia","targetPage":"03. Guardar Referido","fulfillment":{"messages":[],"setParams":[{"parameter":"areaExperienciaReferido","value":"Ninguno / Sin experiencia"}],"generators":[]}},{"event":"volver_pagina","targetPage":"01.1 Solicitar area de experiencia del referido","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 04. Referidos › 02. ¿Otra área de experiencia?

**Al entrar, en este orden:**

1. 💬 Dice: «✏️ ¿$session.params.nombreReferido cuenta con otra área de experiencia?»
2. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → página «02.1. Otra área de experiencia»
- «No» (`no`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `No`

```json camila:page
{"flow":"04. Referidos","page":{"name":"02. ¿Otra área de experiencia?","entry":{"messages":[{"type":"text","texts":["✏️ ¿$session.params.nombreReferido cuenta con otra área de experiencia?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si"},{"text":"No","event":"no"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"si","targetPage":"02.1. Otra área de experiencia","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"no","targetPage":"03. Guardar Referido","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.otraAreaExperienciaReferido","value":"No"}],"generators":[]}}],"groups":[]}}
```

### Página · 04. Referidos › 02.1. Otra área de experiencia

**Al entrar, en este orden:**

1. 💬 Dice: «💼 ¿Cuál es el área extra de experiencia del referido?»
2. 🔘 Ofrece los botones: «💼 Comercial/Ventas» · «🎧 Atención cliente» · «📋 Admin/Financiero» · «🚚 Logística/Prod.» · «💻 Tecnología/Sist.» · «🏥 Salud» · «📚 Educación»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `otraAreaExperienciaReferido`

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «💼 Comercial/Ventas» (`comercial_ventas`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Comercial / Ventas`
- «🎧 Atención cliente» (`atencion_cliente`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Atención al cliente`
- «📋 Admin/Financiero» (`administrativo`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Administrativo/Financiero`
- «🚚 Logística/Prod.» (`logistica_operaciones`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Logística / Produccion`
- «💻 Tecnología/Sist.» (`tecnologia_sistemas`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Tecnología / Sistemas`
- «🏥 Salud» (`salud`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Salud`
- «📚 Educación» (`educacion`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Educación`
- evento `siguiente_pagina` → página «02.2 Otra área de experiencia segunda lista»

```json camila:page
{"flow":"04. Referidos","page":{"name":"02.1. Otra área de experiencia","entry":{"messages":[{"type":"text","texts":["\uD83D\uDCBC ¿Cuál es el área extra de experiencia del referido?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"\uD83D\uDCBC Comercial/Ventas","event":"comercial_ventas"},{"text":"\uD83C\uDFA7 Atención cliente","event":"atencion_cliente"},{"text":"\uD83D\uDCCB Admin/Financiero","event":"administrativo"},{"text":"\uD83D\uDE9A Logística/Prod.","event":"logistica_operaciones"},{"text":"\uD83D\uDCBB Tecnología/Sist.","event":"tecnologia_sistemas"},{"text":"\uD83C\uDFE5 Salud","event":"salud"},{"text":"\uD83D\uDCDA Educación","event":"educacion"}]}],"setParams":[],"generators":[]},"form":[{"name":"otraAreaExperienciaReferido","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[],"events":[{"event":"comercial_ventas","targetPage":"03. Guardar Referido","fulfillment":{"messages":[],"setParams":[{"parameter":"otraAreaExperienciaReferido","value":"Comercial / Ventas"}],"generators":[]}},{"event":"atencion_cliente","targetPage":"03. Guardar Referido","fulfillment":{"messages":[],"setParams":[{"parameter":"otraAreaExperienciaReferido","value":"Atención al cliente"}],"generators":[]}},{"event":"administrativo","targetPage":"03. Guardar Referido","fulfillment":{"messages":[],"setParams":[{"parameter":"otraAreaExperienciaReferido","value":"Administrativo/Financiero"}],"generators":[]}},{"event":"logistica_operaciones","targetPage":"03. Guardar Referido","fulfillment":{"messages":[],"setParams":[{"parameter":"otraAreaExperienciaReferido","value":"Logística / Produccion"}],"generators":[]}},{"event":"tecnologia_sistemas","targetPage":"03. Guardar Referido","fulfillment":{"messages":[],"setParams":[{"parameter":"otraAreaExperienciaReferido","value":"Tecnología / Sistemas"}],"generators":[]}},{"event":"salud","targetPage":"03. Guardar Referido","fulfillment":{"messages":[],"setParams":[{"parameter":"otraAreaExperienciaReferido","value":"Salud"}],"generators":[]}},{"event":"educacion","targetPage":"03. Guardar Referido","fulfillment":{"messages":[],"setParams":[{"parameter":"otraAreaExperienciaReferido","value":"Educación"}],"generators":[]}},{"event":"siguiente_pagina","targetPage":"02.2 Otra área de experiencia segunda lista","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 04. Referidos › 02.2 Otra área de experiencia segunda lista

**Al entrar, en este orden:**

1. 💬 Dice: «💼 ¿Cuál es el área extra de experiencia del referido?»
2. 🔘 Ofrece los botones: «🔧 Servicios generales» · «🏛️ Sector público» · «🚀 Independiente/Empr.» · «⬅️ Volver»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «🔧 Servicios generales» (`servicios_generales`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Servicios generales`
- «🏛️ Sector público» (`sector_publico`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Sector público`
- «🚀 Independiente/Empr.» (`independiente_emprendimiento`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Independiente / Emprendimiento`
- «⬅️ Volver» (`volver_pagina`) → página «02.1. Otra área de experiencia»

```json camila:page
{"flow":"04. Referidos","page":{"name":"02.2 Otra área de experiencia segunda lista","entry":{"messages":[{"type":"text","texts":["\uD83D\uDCBC ¿Cuál es el área extra de experiencia del referido?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"\uD83D\uDD27 Servicios generales","event":"servicios_generales"},{"text":"\uD83C\uDFDB️ Sector público","event":"sector_publico"},{"text":"\uD83D\uDE80 Independiente/Empr.","event":"independiente_emprendimiento"},{"text":"⬅️ Volver","event":"volver_pagina"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"servicios_generales","targetPage":"03. Guardar Referido","fulfillment":{"messages":[],"setParams":[{"parameter":"otraAreaExperienciaReferido","value":"Servicios generales"}],"generators":[]}},{"event":"sector_publico","targetPage":"03. Guardar Referido","fulfillment":{"messages":[],"setParams":[{"parameter":"otraAreaExperienciaReferido","value":"Sector público"}],"generators":[]}},{"event":"independiente_emprendimiento","targetPage":"03. Guardar Referido","fulfillment":{"messages":[],"setParams":[{"parameter":"otraAreaExperienciaReferido","value":"Independiente / Emprendimiento"}],"generators":[]}},{"event":"volver_pagina","targetPage":"02.1. Otra área de experiencia","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 04. Referidos › 03. Guardar Referido

**Al entrar, en este orden:**

1. ✍️ Llama al webhook **Guardar_Referidos** [guardar] — **escritura, pide confirmación**
2. 💬 Dice: «Gracias por compartir la información. ✅  Hemos registrado el referido correctamente.»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «04. ¿Otro Referido?»

```json camila:page
{"flow":"04. Referidos","page":{"name":"03. Guardar Referido","entry":{"messages":[{"type":"text","texts":["Gracias por compartir la información. ✅\n\nHemos registrado el referido correctamente."],"buttons":[]}],"setParams":[],"webhook":"Guardar_Referidos","tag":"guardar","generators":[]},"form":[],"routes":[{"condition":"true = true","targetPage":"04. ¿Otro Referido?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 04. Referidos › 04. ¿Otro Referido?

**Al entrar, en este orden:**

1. 💬 Dice: «¿Deseas referir a otra persona? 🤓»
2. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → página «01. Solicitar Referido»  
  ↳ 📌 asigna `documentoReferido` = ``, `nombreReferido` = ``, `correoReferido` = ``, `telefonoReferido` = `` (+2)
- «No» (`no`) → flujo **12. ¿Algo Mas?**  
  ↳ 📌 asigna `documentoReferido` = ``, `nombreReferido` = ``, `correoReferido` = ``, `telefonoReferido` = `` (+2)

```json camila:page
{"flow":"04. Referidos","page":{"name":"04. ¿Otro Referido?","entry":{"messages":[{"type":"text","texts":["¿Deseas referir a otra persona? \uD83E\uDD13"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si"},{"text":"No","event":"no"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"si","targetPage":"01. Solicitar Referido","fulfillment":{"messages":[],"setParams":[{"parameter":"documentoReferido"},{"parameter":"nombreReferido"},{"parameter":"correoReferido"},{"parameter":"telefonoReferido"},{"parameter":"areaExperienciaReferido"},{"parameter":"otraAreaExperienciaReferido"}],"generators":[]}},{"event":"no","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[{"parameter":"documentoReferido"},{"parameter":"nombreReferido"},{"parameter":"correoReferido"},{"parameter":"telefonoReferido"},{"parameter":"areaExperienciaReferido"},{"parameter":"otraAreaExperienciaReferido"}],"generators":[]}}],"groups":[]}}
```

