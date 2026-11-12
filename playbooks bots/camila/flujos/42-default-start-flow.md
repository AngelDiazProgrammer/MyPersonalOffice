---
bot: camila
flow: Default Start Flow
---
## Flujo · Default Start Flow

**Qué dice al empezar:** «Hola, soy Camila, tu asistente de IA en experiencia de Outsourcing S.A.S. BIC, y estoy feliz de poder ayudarte el día de hoy»

**Se llega desde:**
- 01. Indicadores De Calidad › 001. Menu_Indicadores ← evento `menu_principal`
- 01. Indicadores De Calidad › 001. Menu_Indicadores ← intent «Menu_Principal»
- 02. Mi Evaluación De Desempeño › 03. ¿Algo mas? ← evento `si_menu`
- 03. Líder Misión › 03. ¿Algo Mas? ← evento `si`
- 07. Actualizacion de datos Datix › 04. Algo Mas ← evento `Si`
- 08.Te escuchamos › 02. Algo mas ← evento `Si`
- 09.Convenios › 03. Algo mas ← evento `Si`
- 12. ¿Algo Mas? › 01. ¿Algo mas? ← evento `si`
- … y 2 más

**Webhooks que usa:** ✍️ Guardar Formulario Fine360  · ⚡ IA Generativa Camila - General · ⚡ Login_API_Kairos · ⚡ recuperarSessionWatsonId

**Pide al usuario:** `nombreProvisional` (01. Bienvenida_CamilaIA), `Numero_Documento` (02.Captura_Numero_Documento), `exColaborador` (02.Captura_Numero_Documento), `exColaborador` (04.Ingreso_Existoso)

**Páginas:**
- 01. Bienvenida_CamilaIA — ⚡ recuperarSessionWatsonId · ⌨️ pide datos
- 01.1.Elige rol 
- 02.Captura_Numero_Documento — ✍️ Guardar Formulario Fine360  · ⌨️ pide datos
- 03.Validacion_Usuario_Kairos 
- 04.Ingreso_Existoso — ⌨️ pide datos
- 05.Menu_Principal — ⚡ Login_API_Kairos

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.menu = true` → página «05.Menu_Principal»  
  ↳ 📌 asigna `menu` = `false`, `consultaGenerativa` = `Basándote en esta pregunta "¿En que puedes ayudar? y ofrece …` · ⚡ Llama al webhook **IA Generativa Camila - General** [consultaWeb] — lectura
🔘 *Si el usuario pulsa un botón:*
- evento `Encuesta de retiro` → flujo **14.1. Encuesta de Retiro**  
  ↳ ⚡ Llama al webhook **recuperarSessionWatsonId** [session-watson] — lectura
- evento `Evaluar herramientas` → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `KactusHerramientasTecnologicas1` = `true`
- evento `Evaluar soporte` → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `KactusHerramientasTecnologicas3` = `true`
- evento `Evaluar mi rol` → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `EncuestaExperienciaRol` = `true`
- evento `Evaluar instalaciones` → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `EncuestaInstalaciones` = `true`
- evento `Evaluar selección` → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `responderEncuestasW` = `true`, `encuestaW_tipo` = `selección`
- evento `Evaluar capacitación` → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `responderEncuestasW` = `true`, `encuestaW_tipo` = `capacitación`
- evento `Evaluar contratación` → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `responderEncuestasW` = `true`, `encuestaW_tipo` = `contratación`
💬 *Si el usuario escribe algo que coincide con:*
- intent «Encuesta De Retiro» (ej. «Encuesta de retiro», «Encuesta De Retiro») → flujo **14.1. Encuesta de Retiro**  
  ↳ ⚡ Llama al webhook **recuperarSessionWatsonId** [session-watson] — lectura
- intent «Default Welcome Intent» (ej. «oa», «ola») → página «01. Bienvenida_CamilaIA»
- intent «PDF pruebas» (ej. «PdF», «Pdf») → flujo **Pruebas pdf**
- intent «KactusHerramientasTecnologicas1» (ej. «Evaluar herramientas», «Responder encuesta entrega herramientas») → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `KactusHerramientasTecnologicas1` = `true`
- intent «KactusHerramientasTecnologicas3» (ej. «Evaluar soporte», «Calificar soporte tecnologico») → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `KactusHerramientasTecnologicas3` = `true`
- intent «EncuestaExperienciaRol» (ej. «Evaluar mi rol», «Como te has sentido desarrollando tu rol») → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `EncuestaExperienciaRol` = `true`
- intent «EncuestaInstalaciones» (ej. «Calificar sede», «Calificar instalaciones de la compania») → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `EncuestaInstalaciones` = `true`
- intent «qrEncuestaCrece» (ej. «crece», «encuesta crece») → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `responderEncuestaCrece` = `true`
- intent «qrEncuestaCuidamos» (ej. «cuidamos», «encuesta cuidamos») → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `responderEncuestaCuidamos` = `true`
- intent «qrEncuestaInduccion» (ej. «induccion», «encuesta induccion») → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `responderEncuestaInduccion` = `true`
- intent «EncuestaLiderMision» (ej. «Valorar a mi lider», «Encuesta lider mision») → flujo **15. Viaje del colaborador**
- intent «EncuestaHastaPronto» (ej. «Garantizar encuesta de retiro», «Encuesta hasta pronto») → flujo **15. Viaje del colaborador**
- intent «route_EncuestaSelecciónW» (ej. «Evaluar selección») → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `responderEncuestasW` = `true`, `encuestaW_tipo` = `seleccion`
- intent «route_EncuestaCapacitaciónW» (ej. «Evaluar capacitación») → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `responderEncuestasW` = `true`, `encuestaW_tipo` = `capacitacion`
- intent «route_EncuestaContrataciónW» (ej. «Evaluar contratación») → flujo **15. Viaje del colaborador**  
  ↳ 📌 asigna `responderEncuestasW` = `true`, `encuestaW_tipo` = `contratacion`
- ❔ *Sin coincidencia* → página «01. Bienvenida_CamilaIA»  
  ↳ 📌 asigna `intencion` = `Default Welcome Intent`
- ❔ *Sin respuesta* → página «01. Bienvenida_CamilaIA»  
  ↳ 📌 asigna `intencion` = `Default Welcome Intent`

**Grupos de rutas compartidas:** Predeterminados

```json camila:flow
{"name":"Default Start Flow","routes":[{"intent":"Encuesta De Retiro","targetFlow":"14.1. Encuesta de Retiro","fulfillment":{"messages":[],"setParams":[],"webhook":"recuperarSessionWatsonId","tag":"session-watson","generators":[]}},{"intent":"Default Welcome Intent","targetPage":"01. Bienvenida_CamilaIA","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"intent":"PDF pruebas","targetFlow":"Pruebas pdf","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"intent":"KactusHerramientasTecnologicas1","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"KactusHerramientasTecnologicas1","value":true}],"generators":[]}},{"intent":"KactusHerramientasTecnologicas3","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"KactusHerramientasTecnologicas3","value":true}],"generators":[]}},{"intent":"EncuestaExperienciaRol","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"EncuestaExperienciaRol","value":true}],"generators":[]}},{"intent":"EncuestaInstalaciones","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"EncuestaInstalaciones","value":true}],"generators":[]}},{"intent":"qrEncuestaCrece","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"responderEncuestaCrece","value":true}],"generators":[]}},{"intent":"qrEncuestaCuidamos","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"responderEncuestaCuidamos","value":true}],"generators":[]}},{"intent":"qrEncuestaInduccion","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"responderEncuestaInduccion","value":true}],"generators":[]}},{"intent":"EncuestaLiderMision","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"intent":"EncuestaHastaPronto","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"intent":"route_EncuestaSelecciónW","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"responderEncuestasW","value":"true"},{"parameter":"encuestaW_tipo","value":"seleccion"}],"generators":[]}},{"intent":"route_EncuestaCapacitaciónW","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"responderEncuestasW","value":true},{"parameter":"encuestaW_tipo","value":"capacitacion"}],"generators":[]}},{"intent":"route_EncuestaContrataciónW","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"responderEncuestasW","value":true},{"parameter":"encuestaW_tipo","value":"contratacion"}],"generators":[]}},{"condition":"$session.params.menu = true","targetPage":"05.Menu_Principal","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.menu","value":false},{"parameter":"consultaGenerativa","value":"Basándote en esta pregunta \"¿En que puedes ayudar? y ofrece las siguientes opciones Calidad, Evaluacion De Desempeño, Líder Misión, Referidos, Indicadores ReconoSer, Proceso de Seleccion, Actualización Datos, Te Escuchamos, Convenios, y hazlo humano y amigable, no hace falta que te presentes o saludes ya que anteriormente ya te presente"}],"webhook":"IA Generativa Camila - General","tag":"consultaWeb","generators":[]}}],"events":[{"event":"sys.no-match-default","targetPage":"01. Bienvenida_CamilaIA","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.intencion","value":"Default Welcome Intent"}],"generators":[]}},{"event":"sys.no-input-default","targetPage":"01. Bienvenida_CamilaIA","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.intencion","value":" Default Welcome Intent"}],"generators":[]}},{"event":"Encuesta de retiro","targetFlow":"14.1. Encuesta de Retiro","fulfillment":{"messages":[],"setParams":[],"webhook":"recuperarSessionWatsonId","tag":"session-watson","generators":[]}},{"event":"Evaluar herramientas","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"KactusHerramientasTecnologicas1","value":true}],"generators":[]}},{"event":"Evaluar soporte","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"KactusHerramientasTecnologicas3","value":true}],"generators":[]}},{"event":"Evaluar mi rol","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"EncuestaExperienciaRol","value":true}],"generators":[]}},{"event":"Evaluar instalaciones","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"EncuestaInstalaciones","value":true}],"generators":[]}},{"event":"Evaluar selección","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"responderEncuestasW","value":true},{"parameter":"encuestaW_tipo","value":"selección"}],"generators":[]}},{"event":"Evaluar capacitación","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"responderEncuestasW","value":true},{"parameter":"encuestaW_tipo","value":"capacitación"}],"generators":[]}},{"event":"Evaluar contratación","targetFlow":"15. Viaje del colaborador","fulfillment":{"messages":[],"setParams":[{"parameter":"responderEncuestasW","value":true},{"parameter":"encuestaW_tipo","value":"contratación"}],"generators":[]}}],"groups":["Predeterminados"]}
```

### Página · Default Start Flow › 01. Bienvenida_CamilaIA

**Al entrar, en este orden:**

1. 💬 Dice: «Hola, soy Camila, tu asistente de IA en experiencia de Outsourcing S.A.S. BIC, y estoy feliz de poder ayudarte el día de hoy»
2. 💬 Dice: «¿Cómo te llamas? 😊»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `nombreProvisional` — @sys.any

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `menu_principal`, `IdSession`, `consultaGenerativa`, `sistemaGenerativo`, `mensajeGenerativoGlobal`, `IdSessionWatson`, `lastMatchedIntent`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.nombreProvisional != null` → página «01.1.Elige rol»  
  ↳ ⚡ Llama al webhook **recuperarSessionWatsonId** [session-watson] — lectura

```json camila:page
{"flow":"Default Start Flow","page":{"name":"01. Bienvenida_CamilaIA","entry":{"messages":[{"type":"text","texts":["Hola, soy Camila, tu asistente de IA en experiencia de Outsourcing S.A.S. BIC, y estoy feliz de poder ayudarte el día de hoy"],"buttons":[]},{"type":"text","texts":["¿Cómo te llamas? \uD83D\uDE0A"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"menu_principal","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"nombreProvisional","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"IdSession","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"consultaGenerativa","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"sistemaGenerativo","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mensajeGenerativoGlobal","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"IdSessionWatson","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"lastMatchedIntent","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.nombreProvisional != null","targetPage":"01.1.Elige rol","fulfillment":{"messages":[],"setParams":[],"webhook":"recuperarSessionWatsonId","tag":"session-watson","generators":[]}}],"events":[],"groups":[]}}
```

### Página · Default Start Flow › 01.1.Elige rol

**Al entrar, en este orden:**

1. 💬 Dice: «¡Un gusto $session.params.nombreProvisional!»
2. 🔘 Ofrece los botones: «👨‍💻 Soy colaborador» · «📝 Estoy en selección» · «👤 Soy Ex Colaborador»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `rol`

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «👨‍💻 Soy colaborador» (`colaborador`) → página «02.Captura_Numero_Documento»  
  ↳ 📌 asigna `rol` = `colaborador`
- «📝 Estoy en selección» (`procesoSeleccion`) → página «02.Captura_Numero_Documento»  
  ↳ 📌 asigna `rol` = `procesoDeSeleccion`
- «👤 Soy Ex Colaborador» (`excolaborador`) → página «02.Captura_Numero_Documento»  
  ↳ 📌 asigna `rol` = `excolaborador`

```json camila:page
{"flow":"Default Start Flow","page":{"name":"01.1.Elige rol","entry":{"messages":[{"type":"text","texts":["¡Un gusto $session.params.nombreProvisional! "],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"\uD83D\uDC68‍\uD83D\uDCBB Soy colaborador","event":"colaborador"},{"text":"\uD83D\uDCDD Estoy en selección","event":"procesoSeleccion"},{"text":"\uD83D\uDC64 Soy Ex Colaborador","event":"excolaborador"}]}],"setParams":[],"generators":[]},"form":[{"name":"rol","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[],"events":[{"event":"colaborador","targetPage":"02.Captura_Numero_Documento","fulfillment":{"messages":[],"setParams":[{"parameter":"rol","value":"colaborador"}],"generators":[]}},{"event":"procesoSeleccion","targetPage":"02.Captura_Numero_Documento","fulfillment":{"messages":[],"setParams":[{"parameter":"rol","value":"procesoDeSeleccion"}],"generators":[]}},{"event":"excolaborador","targetPage":"02.Captura_Numero_Documento","fulfillment":{"messages":[],"setParams":[{"parameter":"rol","value":"excolaborador"}],"generators":[]}}],"groups":[]}}
```

### Página · Default Start Flow › 02.Captura_Numero_Documento

**Al entrar, en este orden:**

1. 💬 Dice: «¿Puedes confirmarme tu número de identificación sin puntos ni comas?»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `Numero_Documento` — @Numero_Documento
- `exColaborador` — @sys.any

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `existe`, `activo`, `UsuarioRed`, `Nombres`, `Apellidos`, `id`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.Numero_Documento != null` → página «04.Ingreso_Existoso»  
  ↳ ✍️ Llama al webhook **Guardar Formulario Fine360 ** [GuardarFormulario] — **escritura, pide confirmación**

```json camila:page
{"flow":"Default Start Flow","page":{"name":"02.Captura_Numero_Documento","entry":{"messages":[{"type":"text","texts":["¿Puedes confirmarme tu número de identificación sin puntos ni comas? "],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"Numero_Documento","entity":"@Numero_Documento","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"existe","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"activo","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"UsuarioRed","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"Nombres","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"Apellidos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"id","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"exColaborador","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.Numero_Documento != null","targetPage":"04.Ingreso_Existoso","fulfillment":{"messages":[],"setParams":[],"webhook":"Guardar Formulario Fine360 ","tag":"GuardarFormulario","generators":[]}}],"events":[],"groups":[]}}
```

### Página · Default Start Flow › 03.Validacion_Usuario_Kairos

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.existe = false` → se queda en la página
- si `$session.params.existe = true AND $session.params.activo = false` → se queda en la página
- si `$session.params.existe = true AND $session.params.activo = true` → se queda en la página

```json camila:page
{"flow":"Default Start Flow","page":{"name":"03.Validacion_Usuario_Kairos","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"$session.params.existe = false","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.existe = true AND $session.params.activo = false","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.existe = true AND $session.params.activo = true","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · Default Start Flow › 04.Ingreso_Existoso

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `exColaborador` — @sys.any

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `indicadores`, `desempeno`, `lider_mision`, `viaje_colaborador`, `referidos`, `seleccion`, `reconoser`, `administracionPersonal`, `actualizarDatos`, `convenios`, `teEscuchamos`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.Numero_Documento != null AND $session.params.rol = colaborador` → página «05.Menu_Principal»
- si `$session.params.Numero_Documento != null AND $session.params.rol = procesoDeSeleccion` → flujo **06. Mi proceso De Selección**  
  ↳ 📌 asigna `seleccion` = `true`, `intencion` = `route_ProcesoDeSeleccion`
- si `$session.params.Numero_Documento != null AND $session.params.rol = excolaborador` → flujo **17. Ex colaborador**

```json camila:page
{"flow":"Default Start Flow","page":{"name":"04.Ingreso_Existoso","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"indicadores","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"desempeno","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"lider_mision","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"viaje_colaborador","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"referidos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"seleccion","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"reconoser","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"exColaborador","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"administracionPersonal","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"actualizarDatos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"convenios","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"teEscuchamos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.Numero_Documento != null AND $session.params.rol = colaborador","targetPage":"05.Menu_Principal","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.Numero_Documento != null AND $session.params.rol = procesoDeSeleccion","targetFlow":"06. Mi proceso De Selección","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.seleccion","value":true},{"parameter":"$session.params.intencion","value":"route_ProcesoDeSeleccion"}],"generators":[]}},{"condition":"$session.params.Numero_Documento != null AND $session.params.rol = excolaborador","targetFlow":"17. Ex colaborador","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · Default Start Flow › 05.Menu_Principal

**Al entrar, en este orden:**

1. 💬 Dice: «👋 ¡Genial! ¿En qué puedo ayudarte?»
2. 🔘 Ofrece los botones: «📊 Calidad» · «✏️ Valora. Desempeño» · «🤝 Referidos» · «🎯 Proceso Seleccion» · «🗂️ Actualizar Datos» · «💬 Te Escuchamos» · «🏢 Convenios» · «🔙 Reportar Retiro» · «👥 Admin. Personal»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `Token_Authorizacion_Reconoser`, `Token_Authorizacion_Kairos`, `menu`, `atajoGlobalMatriz`, `atajoGlobalOportunidades`, `atajosGlobalesIndicadores`, `atajoGlobalCerrarMonitoreos`, `atajoGlobalAtributos`, `atajoGlobalIndicadoresCalidad`, `idUsuarioReferidos`, `intencion`

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «📊 Calidad» (`calidad`) → flujo **01. Indicadores De Calidad**  
  ↳ 📌 asigna `indicadores` = `true`, `intencion` = `route_Calidad` · ⚡ Llama al webhook **Login_API_Kairos** [Login] — lectura
- «✏️ Valora. Desempeño» (`evaluacion_desempeno`) → flujo **02. Mi Evaluación De Desempeño**  
  ↳ 📌 asigna `intencion` = `route_EvaluacionDeDesemepeño`, `desempeno` = `true` · ⚡ Llama al webhook **Login_API_Kairos** [Login] — lectura
- evento `lider_mision` → flujo **03. Líder Misión**  
  ↳ ⚡ Llama al webhook **Login_API_Kairos** [Login] — lectura
- «🤝 Referidos» (`referidos`) → flujo **04. Referidos**  
  ↳ 📌 asigna `referidos` = `true`, `intencion` = `route_Referidos`
- evento `indicadores_reconoser` → flujo **05. Reconoser**  
  ↳ 📌 asigna `reconoser` = `true`
- «🗂️ Actualizar Datos» (`actualizacion_datos`) → flujo **07. Actualizacion de datos Datix**  
  ↳ 📌 asigna `actualizarDatos` = `true`, `intencion` = `route_ActualizarDatosDatix`
- «💬 Te Escuchamos» (`te_escuchamos`) → flujo **08.Te escuchamos**  
  ↳ 📌 asigna `teEscuchamos` = `true`, `intencion` = `route_TeEscuchamos`
- «🏢 Convenios» (`convenios`) → flujo **09.Convenios**  
  ↳ 📌 asigna `convenios` = `true`, `intencion` = `route_Convenios`
- «🔙 Reportar Retiro» (`reportarRetiro`) → flujo **14. Reportar Retiro**  
  ↳ 📌 asigna `intencion` = `route_ReportarRetiro`
- «👥 Admin. Personal» (`administracionPersonal`) → flujo **16. Administración de personal**  
  ↳ 📌 asigna `intencion` = `route_AdministracionPersonal`
- «🎯 Proceso Seleccion» (`seleccion`) → flujo **06. Mi proceso De Selección**  
  ↳ 📌 asigna `seleccion` = `true`, `intencion` = `route_ProcesoDeSeleccion`

```json camila:page
{"flow":"Default Start Flow","page":{"name":"05.Menu_Principal","entry":{"messages":[{"type":"text","texts":["\uD83D\uDC4B ¡Genial! ¿En qué puedo ayudarte?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"\uD83D\uDCCA Calidad","event":"calidad"},{"text":"✏️ Valora. Desempeño","event":"evaluacion_desempeno"},{"text":"\uD83E\uDD1D Referidos","event":"referidos"},{"text":"\uD83C\uDFAF Proceso Seleccion","event":"seleccion"},{"text":"\uD83D\uDDC2️ Actualizar Datos","event":"actualizacion_datos"},{"text":"\uD83D\uDCAC Te Escuchamos","event":"te_escuchamos"},{"text":"\uD83C\uDFE2 Convenios","event":"convenios"},{"text":"\uD83D\uDD19 Reportar Retiro","event":"reportarRetiro"},{"text":"\uD83D\uDC65 Admin. Personal","event":"administracionPersonal"}]}],"setParams":[],"generators":[]},"form":[{"name":"Token_Authorizacion_Reconoser","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"Token_Authorizacion_Kairos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"menu","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"atajoGlobalMatriz","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"atajoGlobalOportunidades","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"atajosGlobalesIndicadores","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"atajoGlobalCerrarMonitoreos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"atajoGlobalAtributos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"atajoGlobalIndicadoresCalidad","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"idUsuarioReferidos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"intencion","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[],"events":[{"event":"calidad","targetFlow":"01. Indicadores De Calidad","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.indicadores","value":true},{"parameter":"$session.params.intencion","value":"route_Calidad"}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"event":"evaluacion_desempeno","targetFlow":"02. Mi Evaluación De Desempeño","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.intencion","value":"route_EvaluacionDeDesemepeño"},{"parameter":"$session.params.desempeno","value":true}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"event":"lider_mision","targetFlow":"03. Líder Misión","fulfillment":{"messages":[],"setParams":[],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"event":"referidos","targetFlow":"04. Referidos","fulfillment":{"messages":[],"setParams":[{"parameter":"referidos","value":true},{"parameter":"$session.params.intencion","value":"route_Referidos"}],"generators":[]}},{"event":"indicadores_reconoser","targetFlow":"05. Reconoser","fulfillment":{"messages":[],"setParams":[{"parameter":"reconoser","value":true}],"generators":[]}},{"event":"actualizacion_datos","targetFlow":"07. Actualizacion de datos Datix","fulfillment":{"messages":[],"setParams":[{"parameter":"actualizarDatos","value":true},{"parameter":"$session.params.intencion","value":"route_ActualizarDatosDatix"}],"generators":[]}},{"event":"te_escuchamos","targetFlow":"08.Te escuchamos","fulfillment":{"messages":[],"setParams":[{"parameter":"teEscuchamos","value":true},{"parameter":"$session.params.intencion","value":"route_TeEscuchamos"}],"generators":[]}},{"event":"convenios","targetFlow":"09.Convenios","fulfillment":{"messages":[],"setParams":[{"parameter":"convenios","value":true},{"parameter":"$session.params.intencion","value":"route_Convenios"}],"generators":[]}},{"event":"reportarRetiro","targetFlow":"14. Reportar Retiro","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.intencion","value":"route_ReportarRetiro"}],"generators":[]}},{"event":"administracionPersonal","targetFlow":"16. Administración de personal","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.intencion","value":"route_AdministracionPersonal"}],"generators":[]}},{"event":"seleccion","targetFlow":"06. Mi proceso De Selección","fulfillment":{"messages":[],"setParams":[{"parameter":"seleccion","value":true},{"parameter":"$session.params.intencion","value":"route_ProcesoDeSeleccion"}],"generators":[]}}],"groups":[]}}
```

