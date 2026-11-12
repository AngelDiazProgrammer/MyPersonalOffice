---
bot: camila
flow: 01. Indicadores De Calidad
---
## Flujo · 01. Indicadores De Calidad

**Qué dice al empezar:** «Conocer tus indicadores de calidad te ayudará a ser más eficiente en tu rol. ✅»

**Se llega desde:**
- 13. Usuario inexistente/ inactivo › 0.1 condicional documento ← condición
- Default Start Flow › 05.Menu_Principal ← evento `calidad`
- flujo (global) ← intent «routeCalidad_AtributosEmbajador»
- flujo (global) ← intent «routeCalidad_CerrarMonitoreosPendientes»
- flujo (global) ← intent «routeCalidad_IndicadoresDeCalidad»
- flujo (global) ← intent «routeCalidad_MatrizDeCalidad»
- flujo (global) ← intent «routeCalidad_OportunidadesYFortalezas»
- flujo (global) ← intent «route_Calidad»
- … y 1 más

**Webhooks que usa:** ⚡ Mis_Atributos · ⚡ Mis_Casos_Abiertos · ⚡ Mis_Indicadores · ⚡ Obtener Url Matriz de Calidad · ⚡ Obtener_Usuario_Por_Cedula

**Pide al usuario:** `usuarioActivo` (00.ValidacionUsuarioKairos)

**Páginas:**
- 0.ValidacionTokenKairos — ⚡ Obtener_Usuario_Por_Cedula
- 00.ValidacionUsuarioKairos — ⌨️ pide datos
- 0001. Condicional_Atajos 
- 001. Menu_Indicadores — ⚡ Obtener Url Matriz de Calidad
- 002.UsuarioNoExiste 
- 003.UsuarioInactivo 
- 004.ParametrosIndicadores 
- 005.ParametrosIndicadoresRestantes — ⚡ Mis_Indicadores
- 006.ParametrosAtributos — ⚡ Mis_Atributos
- 01.1. Link PowerBI 
- 01.IndicadoresCalidad 
- 02. Atributos 
- 03. CerrarMonitoreos — ⚡ Mis_Casos_Abiertos
- 03.1.1 Muestra imagen 
- 03.1.1 RevisarSiHayCasos 
- 03.1.InformacionCasosAbiertos 
- 03.2 NoHayMonitoreosPendientes 
- 04 . Oportunidades 
- 05. Matriz 
- 06. ¿Algo mas? 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.indicadores = true AND $session.params.Token_Authorizacion_Kairos != null` → página «0.ValidacionTokenKairos»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"01. Indicadores De Calidad","routes":[{"condition":"$session.params.indicadores = true AND $session.params.Token_Authorizacion_Kairos != null","targetPage":"0.ValidacionTokenKairos","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 01. Indicadores De Calidad › 0.ValidacionTokenKairos

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «00.ValidacionUsuarioKairos»  
  ↳ ⚡ Llama al webhook **Obtener_Usuario_Por_Cedula** [ObtenerUsuarioPorCedula] — lectura

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"0.ValidacionTokenKairos","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true = true","targetPage":"00.ValidacionUsuarioKairos","fulfillment":{"messages":[],"setParams":[],"webhook":"Obtener_Usuario_Por_Cedula","tag":"ObtenerUsuarioPorCedula","generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 00.ValidacionUsuarioKairos

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `usuarioActivo` — @sys.any

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `usuarioNoExiste`, `usuarioInactivo`, `idCampana`, `Campana`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.existe = true AND $session.params.activo = true` → página «0001. Condicional_Atajos»
- si `$session.params.existe = false` → flujo **13. Usuario inexistente/ inactivo**  
  ↳ 📌 asigna `usuarioNoExiste` = `true`, `usuarioInactivo` = `false`
- si `$session.params.existe = true AND $session.params.activo = false` → flujo **13. Usuario inexistente/ inactivo**  
  ↳ 📌 asigna `usuarioNoExiste` = `false`, `usuarioInactivo` = `true`

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"00.ValidacionUsuarioKairos","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"usuarioNoExiste","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"usuarioInactivo","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"usuarioActivo","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"idCampana","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"Campana","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.existe = true AND $session.params.activo = true","targetPage":"0001. Condicional_Atajos","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.existe = false","targetFlow":"13. Usuario inexistente/ inactivo","fulfillment":{"messages":[],"setParams":[{"parameter":"usuarioNoExiste","value":true},{"parameter":"usuarioInactivo","value":false}],"generators":[]}},{"condition":"$session.params.existe = true AND $session.params.activo = false","targetFlow":"13. Usuario inexistente/ inactivo","fulfillment":{"messages":[],"setParams":[{"parameter":"usuarioNoExiste","value":false},{"parameter":"usuarioInactivo","value":true}],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 0001. Condicional_Atajos

**Al entrar, en este orden:**

_No hace nada al entrar._

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `url_MatrizCalidad`, `url_MatrizCalidad_Final`, `url_MatrizCalidad_Final_Encriptada`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.atajosGlobalesIndicadores != true` → página «001. Menu_Indicadores»

**Grupos de rutas compartidas:** Atajos_globales_submenu

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"0001. Condicional_Atajos","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"url_MatrizCalidad","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"url_MatrizCalidad_Final","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"url_MatrizCalidad_Final_Encriptada","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.atajosGlobalesIndicadores != true","targetPage":"001. Menu_Indicadores","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":["Atajos_globales_submenu"]}}
```

### Página · 01. Indicadores De Calidad › 001. Menu_Indicadores

**Al entrar, en este orden:**

1. 💬 Dice: «Conocer tus indicadores de calidad te ayudará a ser más eficiente en tu rol. ✅»
2. 💬 Dice: «Que quieres consultar hoy? 🤓»
3. 🔘 Ofrece los botones: «📊 Indic. Calidad» · «⭐ Atrib. Embajador» · «👁️‍🗨️ Monitoreos Pend.» · «🔍 Oport. y Fortal.» · «🧩 Mi Matriz Calidad» · «☰ Menú Principal»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «📊 Indic. Calidad» (`indicadores_calidad`) → página «004.ParametrosIndicadores»  
  ↳ 📌 asigna `intencion` = `routeCalidad_IndicadoresDeCalidad`
- «⭐ Atrib. Embajador» (`atributos_embajador`) → página «006.ParametrosAtributos»  
  ↳ 📌 asigna `intencion` = `routeCalidad_AtributosEmbajador`
- «👁️‍🗨️ Monitoreos Pend.» (`monitoreos_pendientes`) → página «03. CerrarMonitoreos»  
  ↳ 📌 asigna `intencion` = `routeCalidad_CerrarMonitoreosPendientes`
- «🔍 Oport. y Fortal.» (`oportunidades_fortalezas`) → página «04 . Oportunidades»  
  ↳ 📌 asigna `intencion` = `routeCalidad_OportunidadesYFortalezas`
- «🧩 Mi Matriz Calidad» (`mi_matriz_calidad`) → página «05. Matriz»  
  ↳ 📌 asigna `url_MatrizCalidad_Final` = `$session.params.url_MatrizCalidad$session.params.UsuarioRed`, `intencion` = `routeCalidad_MatrizDeCalidad` · ⚡ Llama al webhook **Obtener Url Matriz de Calidad** [GenerarUrlMatrizCalidad] — lectura
- «☰ Menú Principal» (`menu_principal`) → flujo **Default Start Flow**  
  ↳ 📌 asigna `menu` = `true`
💬 *Si el usuario escribe algo que coincide con:*
- intent «routeCalidad_IndicadoresDeCalidad» (ej. «indicadores calidad», «Indicadores Calidad») → página «004.ParametrosIndicadores»
- intent «routeCalidad_AtributosEmbajador» (ej. «atributos embajador de marca», «Mostrar mi evaluación de atributos de em…») → página «006.ParametrosAtributos»
- intent «routeCalidad_CerrarMonitoreosPendientes» (ej. «Cerrar monitoreos», «Cierra los monitoreos que faltan, por fa…») → página «03. CerrarMonitoreos»
- intent «routeCalidad_OportunidadesYFortalezas» (ej. «Dime mis oportunidades para crecer», «Quiero saber mis fortalezas en el trabaj…») → página «04 . Oportunidades»
- intent «routeCalidad_MatrizDeCalidad» (ej. «Quiero acceder a la matriz de calidad.», «Enséñame mi matriz de calidad, por favor…») → página «05. Matriz»  
  ↳ 📌 asigna `url_MatrizCalidad_Final` = `$session.params.url_MatrizCalidad$session.params.UsuarioRed` · ⚡ Llama al webhook **Obtener Url Matriz de Calidad** [GenerarUrlMatrizCalidad] — lectura
- intent «Menu_Principal» (ej. «Volver al inicio», «Quiero ir al menú principal») → flujo **Default Start Flow**  
  ↳ 📌 asigna `menu` = `true`
- ❔ *Sin coincidencia* → flujo **11. No Detecta**

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"001. Menu_Indicadores","entry":{"messages":[{"type":"text","texts":["Conocer tus indicadores de calidad te ayudará a ser\nmás eficiente en tu rol. ✅"],"buttons":[]},{"type":"text","texts":["Que quieres consultar hoy? \uD83E\uDD13"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"\uD83D\uDCCA Indic. Calidad","event":"indicadores_calidad"},{"text":"⭐ Atrib. Embajador","event":"atributos_embajador"},{"text":"\uD83D\uDC41️‍\uD83D\uDDE8️ Monitoreos Pend.","event":"monitoreos_pendientes"},{"text":"\uD83D\uDD0D Oport. y Fortal.","event":"oportunidades_fortalezas"},{"text":"\uD83E\uDDE9 Mi Matriz Calidad","event":"mi_matriz_calidad"},{"text":"☰ Menú Principal","event":"menu_principal"}]}],"setParams":[],"generators":[]},"form":[],"routes":[{"intent":"routeCalidad_IndicadoresDeCalidad","targetPage":"004.ParametrosIndicadores","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"intent":"routeCalidad_AtributosEmbajador","targetPage":"006.ParametrosAtributos","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"intent":"routeCalidad_CerrarMonitoreosPendientes","targetPage":"03. CerrarMonitoreos","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"intent":"routeCalidad_OportunidadesYFortalezas","targetPage":"04 . Oportunidades","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"intent":"routeCalidad_MatrizDeCalidad","targetPage":"05. Matriz","fulfillment":{"messages":[],"setParams":[{"parameter":"url_MatrizCalidad_Final","value":"$session.params.url_MatrizCalidad$session.params.UsuarioRed"}],"webhook":"Obtener Url Matriz de Calidad","tag":"GenerarUrlMatrizCalidad","generators":[]}},{"intent":"Menu_Principal","targetFlow":"Default Start Flow","fulfillment":{"messages":[],"setParams":[{"parameter":"menu","value":true}],"generators":[]}}],"events":[{"event":"sys.no-match-default","targetFlow":"11. No Detecta","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"indicadores_calidad","targetPage":"004.ParametrosIndicadores","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.intencion","value":" routeCalidad_IndicadoresDeCalidad"}],"generators":[]}},{"event":"atributos_embajador","targetPage":"006.ParametrosAtributos","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.intencion","value":" routeCalidad_AtributosEmbajador"}],"generators":[]}},{"event":"monitoreos_pendientes","targetPage":"03. CerrarMonitoreos","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.intencion","value":" routeCalidad_CerrarMonitoreosPendientes"}],"generators":[]}},{"event":"oportunidades_fortalezas","targetPage":"04 . Oportunidades","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.intencion","value":" routeCalidad_OportunidadesYFortalezas"}],"generators":[]}},{"event":"mi_matriz_calidad","targetPage":"05. Matriz","fulfillment":{"messages":[],"setParams":[{"parameter":"url_MatrizCalidad_Final","value":"$session.params.url_MatrizCalidad$session.params.UsuarioRed"},{"parameter":"$session.params.intencion","value":" routeCalidad_MatrizDeCalidad"}],"webhook":"Obtener Url Matriz de Calidad","tag":"GenerarUrlMatrizCalidad","generators":[]}},{"event":"menu_principal","targetFlow":"Default Start Flow","fulfillment":{"messages":[],"setParams":[{"parameter":"menu","value":true}],"generators":[]}}],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 002.UsuarioNoExiste

**Al entrar, en este orden:**

1. 💬 Dice: «Por ahora no te existes en al base de datos de Kairos, por favor comunícate con el área encargada»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true`

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"002.UsuarioNoExiste","entry":{"messages":[{"type":"text","texts":["Por ahora no te existes en al base de datos de Kairos, por favor comunícate con el área encargada"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true = true","targetFlow":"10. Cierre de conversación","fulfillment":{"messages":[],"setParams":[{"parameter":"cierre","value":true}],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 003.UsuarioInactivo

**Al entrar, en este orden:**

1. 💬 Dice: «Por ahora estas inactivo en al base de datos de Kairos, por favor comunícate con el área encargada»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true`

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"003.UsuarioInactivo","entry":{"messages":[{"type":"text","texts":["Por ahora estas inactivo en al base de datos de Kairos, por favor comunícate con el área encargada"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true = true","targetFlow":"10. Cierre de conversación","fulfillment":{"messages":[],"setParams":[{"parameter":"cierre","value":true}],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 004.ParametrosIndicadores

**Al entrar, en este orden:**

1. 💬 Dice: «Estos resultados te ayudaran a conocer tu desempeño trimestralmente y a seguir fortaleciendo tu crecimiento dentro del equipo. 📈  Tus resultados de calidad del último trimestre son:»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `mesActualMes`, `mesActualCantidadMonitoreos`, `mesActualNotaCalidad`, `mesActualCantidadCriticos`, `mesActualCantidadNoCriticos`, `mesActualNotaCriticos`, `mesActualNotaNoCriticos`, `mesAnteriorMes`, `mesAnteriorCantidadMonitoreos`, `mesAnteriorNotaCalidad`, `mesAnteriorCantidadCriticos`, `mesAnteriorCantidadNoCriticos`, `mesAnteriorNotaCriticos`, `mesAnteriorNotaNoCriticos`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «005.ParametrosIndicadoresRestantes»

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"004.ParametrosIndicadores","entry":{"messages":[{"type":"text","texts":["Estos resultados te ayudaran a conocer tu desempeño trimestralmente y a seguir fortaleciendo tu crecimiento dentro del equipo. \uD83D\uDCC8\n\nTus resultados de calidad del último trimestre son:"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"mesActualMes","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesActualCantidadMonitoreos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesActualNotaCalidad","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesActualCantidadCriticos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesActualCantidadNoCriticos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesActualNotaCriticos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesActualNotaNoCriticos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesAnteriorMes","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesAnteriorCantidadMonitoreos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesAnteriorNotaCalidad","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesAnteriorCantidadCriticos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesAnteriorCantidadNoCriticos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesAnteriorNotaCriticos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesAnteriorNotaNoCriticos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"true = true","targetPage":"005.ParametrosIndicadoresRestantes","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 005.ParametrosIndicadoresRestantes

**Al entrar, en este orden:**

_No hace nada al entrar._

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `mesPenultimoMes`, `mesPenultimoCantidadMonitoreos`, `mesPenultimoNotaCalidad`, `mesPenultimoCantidadCriticos`, `mesPenultimoCantidadNoCriticos`, `mesPenultimoNotaCriticos`, `mesPenultimoNotaNoCriticos`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «01.IndicadoresCalidad»  
  ↳ ⚡ Llama al webhook **Mis_Indicadores** [ObtenerEstadisticasEmbajador] — lectura

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"005.ParametrosIndicadoresRestantes","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"mesPenultimoMes","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesPenultimoCantidadMonitoreos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesPenultimoNotaCalidad","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesPenultimoCantidadCriticos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesPenultimoCantidadNoCriticos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesPenultimoNotaCriticos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesPenultimoNotaNoCriticos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"true = true","targetPage":"01.IndicadoresCalidad","fulfillment":{"messages":[],"setParams":[],"webhook":"Mis_Indicadores","tag":"ObtenerEstadisticasEmbajador","generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 006.ParametrosAtributos

**Al entrar, en este orden:**

1. 💬 Dice: «Conoce los atributos de embajador de marca asociados a tu rol. Estos reflejan los comportamientos y actitudes que fortalecen la experiencia que brindamos cada día. 😊»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `mesActualMes`, `mesActualPorcentanjeProactivo`, `mesActualPorcentanjeApasionado`, `mesActualPorcentanjeEmpatia`, `mesAnteriorMes`, `mesAnteriorPorcentanjeProactivo`, `mesAnteriorPorcentanjeApasionado`, `mesAnteriorPorcentanjeEmpatia`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «02. Atributos»  
  ↳ ⚡ Llama al webhook **Mis_Atributos** [ObtenerAtributosEmbajador] — lectura

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"006.ParametrosAtributos","entry":{"messages":[{"type":"text","texts":["Conoce los atributos de embajador de marca asociados a tu rol. Estos reflejan los comportamientos y actitudes que fortalecen la experiencia que brindamos cada día. \uD83D\uDE0A"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"mesActualMes","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesActualPorcentanjeProactivo","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesActualPorcentanjeApasionado","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesActualPorcentanjeEmpatia","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesAnteriorMes","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesAnteriorPorcentanjeProactivo","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesAnteriorPorcentanjeApasionado","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mesAnteriorPorcentanjeEmpatia","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"true = true","targetPage":"02. Atributos","fulfillment":{"messages":[],"setParams":[],"webhook":"Mis_Atributos","tag":"ObtenerAtributosEmbajador","generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 01.1. Link PowerBI

**Al entrar, en este orden:**

1. 💬 Dice: «Para consultar el detalle, ingresa con tu número de identificación aquí. 👇»
2. 💬 Dice: «https://bit.ly/Powerbi_Embajadores»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «06. ¿Algo mas?»

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"01.1. Link PowerBI","entry":{"messages":[{"type":"text","texts":["Para consultar el detalle, ingresa con tu número de identificación aquí. \uD83D\uDC47"],"buttons":[]},{"type":"text","texts":["https://bit.ly/Powerbi_Embajadores"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true = true","targetPage":"06. ¿Algo mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 01.IndicadoresCalidad

**Al entrar, en este orden:**

1. 💬 Dice: «MES: $session.params.MesActualMes  ✅Cantidad de Monitoreos:  Cantidad de monitoreos: $session.params.MesActualCantidadMonitoreos  ✅Calidad Emitida:  Nota: $session.params.MesActualNotaCalidad  ✅Errore…»
2. 💬 Dice: «MES: $session.params.MesAnteriorMes  ✅Cantidad de Monitoreos:  Cantidad de monitoreos: $session.params.MesAnteriorCantidadMonitoreos  ✅Calidad Emitida:  Nota: $session.params.MesAnteriorNotaCalidad  ✅…»
3. 💬 Dice: «MES: $session.params.MesPenultimoMes  ✅Cantidad de Monitoreos:  Cantidad de monitoreos: $session.params.MesPenultimoCantidadMonitoreos  ✅Calidad Emitida:  Nota: $session.params.MesPenultimoNotaCalidad…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «01.1. Link PowerBI»

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"01.IndicadoresCalidad","entry":{"messages":[{"type":"text","texts":["MES: $session.params.MesActualMes\n\n✅Cantidad de Monitoreos: \nCantidad de monitoreos: $session.params.MesActualCantidadMonitoreos\n\n✅Calidad Emitida: \nNota: $session.params.MesActualNotaCalidad\n\n✅Errores y Precisión Error Crítico:\nErrores: $session.params.MesActualCantidadCriticos\nPromedio: $session.params.MesActualNotaCriticos %\n\n✅Errores y Precisión Error No Crítico: \nErrores: $session.params.MesActualCantidadNoCriticos\nPromedio: $session.params.MesActualNotaNoCriticos %\n"],"buttons":[]},{"type":"text","texts":["MES: $session.params.MesAnteriorMes\n\n✅Cantidad de Monitoreos: \nCantidad de monitoreos: $session.params.MesAnteriorCantidadMonitoreos\n\n✅Calidad Emitida: \nNota: $session.params.MesAnteriorNotaCalidad\n\n✅Errores y Precisión Error Crítico:\nErrores: $session.params.MesAnteriorCantidadCriticos\nPromedio: $session.params.MesAnteriorNotaCriticos %\n\n✅Errores y Precisión Error No Crítico: \nErrores: $session.params.MesAnteriorCantidadNoCriticos\nPromedio: $session.params.MesAnteriorNotaNoCriticos %"],"buttons":[]},{"type":"text","texts":["MES: $session.params.MesPenultimoMes\n\n✅Cantidad de Monitoreos: \nCantidad de monitoreos: $session.params.MesPenultimoCantidadMonitoreos\n\n✅Calidad Emitida: \nNota: $session.params.MesPenultimoNotaCalidad\n\n✅Errores y Precisión Error Crítico:\nErrores: $session.params.MesPenultimoCantidadCriticos\nPromedio: $session.params.MesPenultimoNotaCriticos %\n\n✅Errores y Precisión Error No Crítico: \nErrores: $session.params.MesPenultimoCantidadNoCriticos\nPromedio: $session.params.MesPenultimoNotaNoCriticos %"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"01.1. Link PowerBI","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 02. Atributos

**Al entrar, en este orden:**

1. 💬 Dice: «Tus poderes de embajador de marca en los dos últimos meses están así:»
2. 💬 Dice: «📅 Mes: $session.params.mesActualMes  ✅ Proactivo: $session.params.mesActualPorcentanjeProactivo % ✅ Empático: $session.params.mesActualPorcentanjeEmpatia % ✅ Apasionado: $session.params.mesActualPorc…»
3. 💬 Dice: «📅 Mes: $session.params.mesAnteriorMes  ✅ Proactivo: $session.params.mesAnteriorPorcentanjeProactivo % ✅ Empático: $session.params.mesAnteriorPorcentanjeEmpatia % ✅ Apasionado: $session.params.mesAnte…»
4. 💬 Dice: «Para consultar el detalle, ingresa con tu número de identificación aquí. 👇»
5. 💬 Dice: «https://bit.ly/Powerbi_Embajadores»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «06. ¿Algo mas?»

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"02. Atributos","entry":{"messages":[{"type":"text","texts":["Tus poderes de embajador de marca en los dos últimos meses están así:"],"buttons":[]},{"type":"text","texts":["\uD83D\uDCC5 Mes: $session.params.mesActualMes\n\n✅ Proactivo: $session.params.mesActualPorcentanjeProactivo %\n✅ Empático: $session.params.mesActualPorcentanjeEmpatia %\n✅ Apasionado: $session.params.mesActualPorcentanjeApasionado %"],"buttons":[]},{"type":"text","texts":["\uD83D\uDCC5 Mes: $session.params.mesAnteriorMes\n\n✅ Proactivo: $session.params.mesAnteriorPorcentanjeProactivo %\n✅ Empático: $session.params.mesAnteriorPorcentanjeEmpatia %\n✅ Apasionado: $session.params.mesAnteriorPorcentanjeApasionado %"],"buttons":[]},{"type":"text","texts":["Para consultar el detalle, ingresa con tu número de identificación aquí. \uD83D\uDC47"],"buttons":[]},{"type":"text","texts":["https://bit.ly/Powerbi_Embajadores"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"06. ¿Algo mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 03. CerrarMonitoreos

**Al entrar, en este orden:**

1. 💬 Dice: «Tu cierre oportuno es clave para transformar los hallazgos en oportunidades reales de mejora. 💪»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `mesCasosAbiertos`, `cantidadCasosAbiertos`, `idCaso`, `fechaCaso`, `segmentoCaso`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «03.1.1 RevisarSiHayCasos»  
  ↳ ⚡ Llama al webhook **Mis_Casos_Abiertos** [ObtenerCasosAbiertos] — lectura

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"03. CerrarMonitoreos","entry":{"messages":[{"type":"text","texts":["Tu cierre oportuno es clave para transformar los hallazgos en oportunidades reales de mejora. \uD83D\uDCAA"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"mesCasosAbiertos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"cantidadCasosAbiertos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"idCaso","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"fechaCaso","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"segmentoCaso","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"true = true","targetPage":"03.1.1 RevisarSiHayCasos","fulfillment":{"messages":[],"setParams":[],"webhook":"Mis_Casos_Abiertos","tag":"ObtenerCasosAbiertos","generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 03.1.1 Muestra imagen

**Al entrar, en este orden:**

1. 💬 Muestra la tarjeta «Producto destacado»
2. 🖼️ Muestra una imagen $session.params.imagenMonitoreos

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «06. ¿Algo mas?»

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"03.1.1 Muestra imagen","entry":{"messages":[{"type":"info","texts":[],"buttons":[],"title":"Producto destacado","subtitle":"Señor usuario, por favor para más información ver la imagen adjunta."},{"type":"image","texts":[],"buttons":[],"title":"Imagen del producto","url":"$session.params.imagenMonitoreos"}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"06. ¿Algo mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 03.1.1 RevisarSiHayCasos

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mesCasosAbiertos = null` → página «03.2 NoHayMonitoreosPendientes»
- si `$session.params.mesCasosAbiertos != null` → página «03.1.InformacionCasosAbiertos»

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"03.1.1 RevisarSiHayCasos","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"$session.params.mesCasosAbiertos = null","targetPage":"03.2 NoHayMonitoreosPendientes","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.mesCasosAbiertos != null","targetPage":"03.1.InformacionCasosAbiertos","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 03.1.InformacionCasosAbiertos

**Al entrar, en este orden:**

1. 💬 Dice: «Tienes los siguientes casos pendientes por cerrar:  📅 Mes: $session.params.mesCasosAbiertos 📌 Cantidad:  $session.params.cantidadCasosAbiertos ✅ ID: $session.params.idCaso ✅ FECHA: $session.params.f…»
2. 💬 Dice: «Para realizar el cierre ingresa aquí. 👇  https://portalos.outsourcing.com.co:9540»
3. 💬 Dice: «Esta imagen te guiará como hacerlo eficientemente. 👇»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `imagenMonitoreos`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «03.1.1 Muestra imagen»  
  ↳ 📌 asigna `imagenMonitoreos` = `https://fine360.outsourcing.com.co:9876/img/monitoreos-kairo…`

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"03.1.InformacionCasosAbiertos","entry":{"messages":[{"type":"text","texts":["Tienes los siguientes casos pendientes por cerrar:\n\n\uD83D\uDCC5 Mes: $session.params.mesCasosAbiertos\n\uD83D\uDCCC Cantidad:  $session.params.cantidadCasosAbiertos\n✅ ID: $session.params.idCaso\n✅ FECHA: $session.params.fechaCaso\n✅ SEGMENTO: $session.params.segmentoCaso"],"buttons":[]},{"type":"text","texts":["Para realizar el cierre ingresa aquí. \uD83D\uDC47\n\nhttps://portalos.outsourcing.com.co:9540"],"buttons":[]},{"type":"text","texts":["Esta imagen te guiará como hacerlo eficientemente. \uD83D\uDC47"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"imagenMonitoreos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"true","targetPage":"03.1.1 Muestra imagen","fulfillment":{"messages":[],"setParams":[{"parameter":"imagenMonitoreos","value":"https://fine360.outsourcing.com.co:9876/img/monitoreos-kairos.jpg"}],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 03.2 NoHayMonitoreosPendientes

**Al entrar, en este orden:**

1. 💬 Dice: «No cuentas con monitoreos pendientes en esta ocasión»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «06. ¿Algo mas?»

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"03.2 NoHayMonitoreosPendientes","entry":{"messages":[{"type":"text","texts":["No cuentas con monitoreos pendientes en esta ocasión "],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"06. ¿Algo mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 04 . Oportunidades

**Al entrar, en este orden:**

1. 💬 Dice: «Conocer tus oportunidades y fortalezas en tiempo real ingresando aquí. 👇»
2. 💬 Dice: «https://bit.ly/Powerbi_Embajadores»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «06. ¿Algo mas?»

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"04 . Oportunidades","entry":{"messages":[{"type":"text","texts":["Conocer tus oportunidades y fortalezas en tiempo real ingresando aquí. \uD83D\uDC47"],"buttons":[]},{"type":"text","texts":["https://bit.ly/Powerbi_Embajadores"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"06. ¿Algo mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 05. Matriz

**Al entrar, en este orden:**

1. 💬 Dice: «Esta información te permitirá comprender tu desempeño en cada atributo y apoyar tu proceso de mejora continua. 🔎»
2. 💬 Dice: «$session.params.url_MatrizCalidad_Final_Encriptada»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «06. ¿Algo mas?»

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"05. Matriz","entry":{"messages":[{"type":"text","texts":["Esta información te permitirá comprender tu desempeño en cada atributo y apoyar tu proceso de mejora continua. \uD83D\uDD0E"],"buttons":[]},{"type":"text","texts":["$session.params.url_MatrizCalidad_Final_Encriptada"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"06. ¿Algo mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 01. Indicadores De Calidad › 06. ¿Algo mas?

**Al entrar, en este orden:**

1. 💬 Dice: «¿Algo más en lo que pueda ayudarte?»
2. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → página «001. Menu_Indicadores»
- «No» (`no`) → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true`
- ❔ *Sin coincidencia* → flujo **11. No Detecta**

```json camila:page
{"flow":"01. Indicadores De Calidad","page":{"name":"06. ¿Algo mas?","entry":{"messages":[{"type":"text","texts":["¿Algo más en lo que pueda ayudarte?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si"},{"text":"No","event":"no"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"si","targetPage":"001. Menu_Indicadores","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"no","targetFlow":"10. Cierre de conversación","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.cierre","value":true}],"generators":[]}},{"event":"sys.no-match-default","targetFlow":"11. No Detecta","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

