---
bot: camila
flow: 15. Viaje del colaborador
---
## Flujo · 15. Viaje del colaborador

**Qué dice al empezar:** «Gracias, $session.params.nombre. Ahora indícame tu número de cédula (sin puntos ni comas):»

**Se llega desde:**
- flujo Default Start Flow ← evento `Evaluar capacitación`
- flujo Default Start Flow ← evento `Evaluar contratación`
- flujo Default Start Flow ← evento `Evaluar herramientas`
- flujo Default Start Flow ← evento `Evaluar instalaciones`
- flujo Default Start Flow ← evento `Evaluar mi rol`
- flujo Default Start Flow ← evento `Evaluar selección`
- flujo Default Start Flow ← evento `Evaluar soporte`
- flujo Default Start Flow ← intent «EncuestaExperienciaRol»
- … y 11 más

**Pide al usuario:** `nombre` (0.datosNombre), `cedula` (1.datosCedula), `correo` (2.datosCorreo), `espacio` (3.datosEspacio)

**Páginas:**
- 0.datosNombre — ⌨️ pide datos
- 1.datosCedula — ⌨️ pide datos
- 2.datosCorreo — ⌨️ pide datos
- 3.datosEspacio — ⌨️ pide datos
- 4.despacho 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.responderEncuestasW = true` → flujo **15.1 Encuesta W**
- si `$session.params.KactusHerramientasTecnologicas1 = true` → flujo **15.5 Heramientas Kactus 1**
- si `$session.params.KactusHerramientasTecnologicas3 = true` → flujo **15.6 Herramientas Kactus 3**
- si `$session.params.EncuestaExperienciaRol = true` → flujo **15.7 Experiencia rol**
- si `$session.params.EncuestaInstalaciones = true` → flujo **15.8 Encuesta instalaciones**
- si `$session.params.EncuestaLiderMision = true` → flujo **15.9 Lider mision**
- si `$session.params.EncuestaHastaPronto = true` → flujo **15.10 Hasta pronto**
- siempre → página «0.datosNombre»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"15. Viaje del colaborador","routes":[{"condition":"$session.params.responderEncuestasW = true","targetFlow":"15.1 Encuesta W","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.KactusHerramientasTecnologicas1 = true","targetFlow":"15.5 Heramientas Kactus 1","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.KactusHerramientasTecnologicas3 = true","targetFlow":"15.6 Herramientas Kactus 3","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.EncuestaExperienciaRol = true","targetFlow":"15.7 Experiencia rol","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.EncuestaInstalaciones = true","targetFlow":"15.8 Encuesta instalaciones","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.EncuestaLiderMision = true","targetFlow":"15.9 Lider mision","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.EncuestaHastaPronto = true","targetFlow":"15.10 Hasta pronto","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"true","targetPage":"0.datosNombre","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 15. Viaje del colaborador › 0.datosNombre

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `nombre` — @sys.any · pregunta: «Para iniciar, por favor escribe tu nombre completo:»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «1.datosCedula»

```json camila:page
{"flow":"15. Viaje del colaborador","page":{"name":"0.datosNombre","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"nombre","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["Para iniciar, por favor escribe tu nombre completo:"],"buttons":[]}],"setParams":[],"generators":[]}}],"routes":[{"condition":"$page.params.status = \"FINAL\"","targetPage":"1.datosCedula","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 15. Viaje del colaborador › 1.datosCedula

**Al entrar, en este orden:**

1. 💬 Dice: «Gracias, $session.params.nombre. Ahora indícame tu número de cédula (sin puntos ni comas):»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `cedula` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «2.datosCorreo»

```json camila:page
{"flow":"15. Viaje del colaborador","page":{"name":"1.datosCedula","entry":{"messages":[{"type":"text","texts":["Gracias, $session.params.nombre. Ahora indícame tu número de cédula (sin puntos ni comas):"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"cedula","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$page.params.status = \"FINAL\"","targetPage":"2.datosCorreo","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 15. Viaje del colaborador › 2.datosCorreo

**Al entrar, en este orden:**

1. 💬 Dice: «Por favor compártenos tu correo electrónico:»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `correo` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «3.datosEspacio»

```json camila:page
{"flow":"15. Viaje del colaborador","page":{"name":"2.datosCorreo","entry":{"messages":[{"type":"text","texts":["Por favor compártenos tu correo electrónico:"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"correo","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$page.params.status = \"FINAL\"","targetPage":"3.datosEspacio","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 15. Viaje del colaborador › 3.datosEspacio

**Al entrar, en este orden:**

1. 💬 Dice: «¿A cuál espacio de formación o desarrollo asististe? (Ej: Inducción, Crece, Cuidamos, Evaluado, Contratado, etc.):»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `espacio` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «4.despacho»

```json camila:page
{"flow":"15. Viaje del colaborador","page":{"name":"3.datosEspacio","entry":{"messages":[{"type":"text","texts":["¿A cuál espacio de formación o desarrollo asististe? (Ej: Inducción, Crece, Cuidamos, Evaluado, Contratado, etc.):"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"espacio","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$page.params.status = \"FINAL\"","targetPage":"4.despacho","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 15. Viaje del colaborador › 4.despacho

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.responderEncuestaCrece = true` → flujo **15.2 Crece con nosotros**
- si `$session.params.responderEncuestaCuidamos = true` → flujo **15.3 Cuidamos de ti**
- si `$session.params.responderEncuestaInduccion = true` → flujo **15.4 Induccion**

```json camila:page
{"flow":"15. Viaje del colaborador","page":{"name":"4.despacho","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"$session.params.responderEncuestaCrece = true","targetFlow":"15.2 Crece con nosotros","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.responderEncuestaCuidamos = true","targetFlow":"15.3 Cuidamos de ti","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.responderEncuestaInduccion = true","targetFlow":"15.4 Induccion","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

