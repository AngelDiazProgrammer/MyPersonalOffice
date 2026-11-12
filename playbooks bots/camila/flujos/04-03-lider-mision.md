---
bot: camila
flow: 03. Líder Misión
---
## Flujo · 03. Líder Misión

**Qué dice al empezar:** «👤 Aquí podrás recordar tus resultados de Líder Misión, reconocer lo que lograste y definir lo que fortalecerás para ser un líder: 🌟 inspirador, 🏆 ganador y �…»

**Se llega desde:**
- Default Start Flow › 05.Menu_Principal ← evento `lider_mision`
- flujo (global) ← intent «route_LiderMision»
- flujo Default Start Flow ← intent «route_LiderMision»

**Webhooks que usa:** ⚡ IA Generativa Camila - General · ⚡ Lider Mision · ⚡ Lider Mision - IA Generativa Camila

**Pide al usuario:** `competencia1` (01. Notas Lider Mision), `mayorSatisfaccion` (01.1.Atributo Mayor Satisfaccion), `atributoAFortalecer` (01.2. Atributo A Fortalecer)

**Páginas:**
- 00. Líder_Misión — ⚡ Lider Mision · ⚡ IA Generativa Camila - General
- 01. Notas Lider Mision — ⌨️ pide datos
- 01.1.Atributo Mayor Satisfaccion — ⌨️ pide datos
- 01.2. Atributo A Fortalecer — ⌨️ pide datos
- 01.3 . Sugerencia Atributos — ⚡ Lider Mision - IA Generativa Camila
- 01.4 CompetenciaMenor1 
- 02. No Consultar Notas Lider Mision 
- 03. ¿Algo Mas? 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «00. Líder_Misión»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"03. Líder Misión","routes":[{"condition":"true","targetPage":"00. Líder_Misión","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 03. Líder Misión › 00. Líder_Misión

**Al entrar, en este orden:**

1. 📌 Asigna: `intencion` = ``
2. 💬 Dice: «👤 Aquí podrás recordar tus resultados de Líder Misión, reconocer lo que lograste y definir lo que fortalecerás para ser un líder: 🌟 inspirador, 🏆 ganador y 🧭 estratega»
3. 💬 Dice: «¿Te gustaría revisar y recordar tus resultados? 📊»
4. 🔘 Ofrece los botones: «Si» · «No»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `competencia1LM`, `competencia2LM`, `competencia3LM`, `notaTotalLM`, `notaCompetencia2LM`, `notaCompetencia1LM`, `notaCompetencia3LM`, `nivelLM`, `competenciaMenor1LiderMision`

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → página «01. Notas Lider Mision»  
  ↳ ⚡ Llama al webhook **Lider Mision** [ObtenerLiderMision] — lectura
- «No» (`no`) → página «02. No Consultar Notas Lider Mision»
- ❔ *Sin coincidencia* → página «00. Líder_Misión»  
  ↳ 📌 asigna `consultaWeb` = `Basándote en esta frase y continuando con la conversación "R…` · ⚡ Llama al webhook **IA Generativa Camila - General** [consultaWeb] — lectura · 💬 dice «$session.params.mensajeGenerativoGlobal»

```json camila:page
{"flow":"03. Líder Misión","page":{"name":"00. Líder_Misión","entry":{"messages":[{"type":"text","texts":["\uD83D\uDC64 Aquí podrás recordar tus resultados de Líder Misión, reconocer lo que lograste y definir lo que fortalecerás para ser un líder: \uD83C\uDF1F inspirador, \uD83C\uDFC6 ganador y \uD83E\uDDED estratega"],"buttons":[]},{"type":"text","texts":["¿Te gustaría revisar y recordar tus resultados? \uD83D\uDCCA"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si"},{"text":"No","event":"no"}]}],"setParams":[{"parameter":"$session.params.intencion"}],"generators":[]},"form":[{"name":"competencia1LM","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"competencia2LM","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"competencia3LM","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"notaTotalLM","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"notaCompetencia2LM","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"notaCompetencia1LM","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"notaCompetencia3LM","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"nivelLM","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"competenciaMenor1LiderMision","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[],"events":[{"event":"si","targetPage":"01. Notas Lider Mision","fulfillment":{"messages":[],"setParams":[],"webhook":"Lider Mision","tag":"ObtenerLiderMision","generators":[]}},{"event":"no","targetPage":"02. No Consultar Notas Lider Mision","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"sys.no-match-default","targetPage":"00. Líder_Misión","fulfillment":{"messages":[{"type":"text","texts":["$session.params.mensajeGenerativoGlobal"],"buttons":[]}],"setParams":[{"parameter":"consultaWeb","value":"Basándote en esta frase y continuando con la conversación \"Recuerda que puedo ayudarte con las siguientes opciones: Calidad, Evaluación De Desempeño, Líder Misión, Referidos, Indicadores ReconoSer, Proceso De Selección, Actualización Datos, Te Escuchamos y Convenios \uD83D\uDE0A\", construye una respuesta amable indicando que no entendiste lo que el usuario quiso decir, invítalo a elegir una de las opciones disponibles, no dejes preguntas abiertas, y hazlo humano y amigable"}],"webhook":"IA Generativa Camila - General","tag":"consultaWeb","generators":[]}}],"groups":[]}}
```

### Página · 03. Líder Misión › 01. Notas Lider Mision

**Al entrar, en este orden:**

1. 💬 Dice: «Perfecto, estos son tus resultados:»
2. 💬 Dice: «📊 NOTA TOTAL: $session.params.notaTotalLM %  ✅ $session.params.competencia1LM $session.params.notaCompetencia1LM % ✅ $session.params.competencia2LM $session.params.notaCompetencia2LM % ✅ $session.par…»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `competencia1` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.competencia1 != null` → página «01.1.Atributo Mayor Satisfaccion»
- siempre → página «01.1.Atributo Mayor Satisfaccion»

```json camila:page
{"flow":"03. Líder Misión","page":{"name":"01. Notas Lider Mision","entry":{"messages":[{"type":"text","texts":["Perfecto, estos son tus resultados:"],"buttons":[]},{"type":"text","texts":["\uD83D\uDCCA NOTA TOTAL: $session.params.notaTotalLM %\n\n✅ $session.params.competencia1LM $session.params.notaCompetencia1LM %\n✅ $session.params.competencia2LM $session.params.notaCompetencia2LM %\n✅ $session.params.competencia3LM $session.params.notaCompetencia3LM %\n"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"competencia1","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.competencia1 != null","targetPage":"01.1.Atributo Mayor Satisfaccion","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"true","targetPage":"01.1.Atributo Mayor Satisfaccion","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 03. Líder Misión › 01.1.Atributo Mayor Satisfaccion

**Al entrar, en este orden:**

1. 💬 Dice: «¿Qué atributo te genera mayor satisfacción y por qué?»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `mayorSatisfaccion` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mayorSatisfaccion != null` → página «01.2. Atributo A Fortalecer»

```json camila:page
{"flow":"03. Líder Misión","page":{"name":"01.1.Atributo Mayor Satisfaccion","entry":{"messages":[{"type":"text","texts":["¿Qué atributo te genera mayor satisfacción y por qué?"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"mayorSatisfaccion","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.mayorSatisfaccion != null","targetPage":"01.2. Atributo A Fortalecer","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 03. Líder Misión › 01.2. Atributo A Fortalecer

**Al entrar, en este orden:**

1. 💬 Dice: «¡Que bien! Te felicito por tu desempeño 🙌, ahora ¿Qué atributo te gustaría fortalecer?»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `atributoAFortalecer` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.atributoAFortalecer != null` → página «01.3 . Sugerencia Atributos»

```json camila:page
{"flow":"03. Líder Misión","page":{"name":"01.2. Atributo A Fortalecer","entry":{"messages":[{"type":"text","texts":["¡Que bien! Te felicito por tu desempeño \uD83D\uDE4C, ahora ¿Qué atributo te gustaría fortalecer?"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"atributoAFortalecer","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.atributoAFortalecer != null","targetPage":"01.3 . Sugerencia Atributos","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 03. Líder Misión › 01.3 . Sugerencia Atributos

**Al entrar, en este orden:**

1. 💬 Dice: «¡Muy bien! Actuar con humildad es reconocer qué debemos mejorar. Sigue adelante y crea tu plan de desarrollo por atributos para avanzar💪. De acuerdo con tus resultados las atributos a fortalecer son:»
2. 🔘 Ofrece los botones: «$session.params.competenciaMenor1LiderMision»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `mensajeGenerativoCompetencia1Lider`

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «$session.params.competenciaMenor1LiderMision» (`competenciaMenor1`) → página «01.4 CompetenciaMenor1»  
  ↳ ⚡ Llama al webhook **Lider Mision - IA Generativa Camila** [ConsultaWeb] — lectura

```json camila:page
{"flow":"03. Líder Misión","page":{"name":"01.3 . Sugerencia Atributos","entry":{"messages":[{"type":"text","texts":["¡Muy bien! Actuar con humildad es reconocer qué debemos mejorar. Sigue adelante y crea tu plan de desarrollo por atributos para avanzar\uD83D\uDCAA. De acuerdo con tus resultados las atributos a fortalecer son: "],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"$session.params.competenciaMenor1LiderMision","event":"competenciaMenor1"}]}],"setParams":[],"generators":[]},"form":[{"name":"mensajeGenerativoCompetencia1Lider","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[],"events":[{"event":"competenciaMenor1","targetPage":"01.4 CompetenciaMenor1","fulfillment":{"messages":[],"setParams":[],"webhook":"Lider Mision - IA Generativa Camila","tag":"ConsultaWeb","generators":[]}}],"groups":[]}}
```

### Página · 03. Líder Misión › 01.4 CompetenciaMenor1

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.mensajeGenerativoCompetencia1Lider»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «03. ¿Algo Mas?»

```json camila:page
{"flow":"03. Líder Misión","page":{"name":"01.4 CompetenciaMenor1","entry":{"messages":[{"type":"text","texts":["$session.params.mensajeGenerativoCompetencia1Lider"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"03. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 03. Líder Misión › 02. No Consultar Notas Lider Mision

**Al entrar, en este orden:**

1. 💬 Dice: «Esta bien, aunque ten presente que tu desempeño está en tus manos y es importante que consultes tus resultados con regularidad para comprender en qué punto estás 😊»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «03. ¿Algo Mas?»

```json camila:page
{"flow":"03. Líder Misión","page":{"name":"02. No Consultar Notas Lider Mision","entry":{"messages":[{"type":"text","texts":["Esta bien, aunque ten presente que tu desempeño está en tus manos y es importante que consultes tus resultados con regularidad para comprender en qué punto estás \uD83D\uDE0A"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true = true","targetPage":"03. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 03. Líder Misión › 03. ¿Algo Mas?

**Al entrar, en este orden:**

1. 📌 Asigna: `liderMision` = ``, `mensajeGenerativoGlobal` = ``, `consultaGenerativa` = ``
2. 💬 Dice: «¿Algo más en lo que pueda ayudarte?»
3. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → flujo **Default Start Flow**  
  ↳ 📌 asigna `menu` = `true`, `liderMision` = `false`
- «No» (`no`) → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true`

```json camila:page
{"flow":"03. Líder Misión","page":{"name":"03. ¿Algo Mas?","entry":{"messages":[{"type":"text","texts":["¿Algo más en lo que pueda ayudarte?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si"},{"text":"No","event":"no"}]}],"setParams":[{"parameter":"$session.params.liderMision"},{"parameter":"$session.params.mensajeGenerativoGlobal"},{"parameter":"$session.params.consultaGenerativa"}],"generators":[]},"form":[],"routes":[],"events":[{"event":"si","targetFlow":"Default Start Flow","fulfillment":{"messages":[],"setParams":[{"parameter":"menu","value":true},{"parameter":"$session.params.liderMision","value":false}],"generators":[]}},{"event":"no","targetFlow":"10. Cierre de conversación","fulfillment":{"messages":[],"setParams":[{"parameter":"cierre","value":true}],"generators":[]}}],"groups":[]}}
```

