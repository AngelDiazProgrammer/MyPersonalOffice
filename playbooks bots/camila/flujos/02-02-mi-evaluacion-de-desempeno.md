---
bot: camila
flow: 02. Mi Evaluación De Desempeño
---
## Flujo · 02. Mi Evaluación De Desempeño

**Qué dice al empezar:** «Aquí podrás recordar los resultados de tu valoración desempeño, reconocer lo que lograste y definir lo que fortalecerás para seguir creciendo en Outsourcing.»

**Se llega desde:**
- Default Start Flow › 05.Menu_Principal ← evento `evaluacion_desempeno`
- flujo (global) ← intent «route_EvaluacionDeDesemepeño»
- flujo Default Start Flow ← intent «route_EvaluacionDeDesemepeño»

**Webhooks que usa:** ⚡ Evaluación de Desempeño · ⚡ Obtener_Usuario_Por_Cedula

**Pide al usuario:** `competenciaAFortalecer` (01.4. Selección libre de competencia)

**Páginas:**
- 00. Mi_Evaluacion_De_Desempeño — ⚡ Evaluación de Desempeño
- 00.00 Creacion de aprametro restantes 
- 01. Resultados Evaluación De Desempeño 
- 01.1. Resultado mayor satisfacción 
- 01.2. Resultado a fortalecer 
- 01.3. Sugerir competencias a fortalecer 
- 01.4. Selección libre de competencia — ⌨️ pide datos
- 02. Mensaje2 
- 02. No Resultados Evaluación De Desempeño 
- 03. ¿Algo mas? 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.desempeno = true` → página «00.00 Creacion de aprametro restantes»  
  ↳ ⚡ Llama al webhook **Obtener_Usuario_Por_Cedula** [ObtenerUsuarioPorCedula] — lectura
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"02. Mi Evaluación De Desempeño","routes":[{"condition":"$session.params.desempeno = true","targetPage":"00.00 Creacion de aprametro restantes","fulfillment":{"messages":[],"setParams":[],"webhook":"Obtener_Usuario_Por_Cedula","tag":"ObtenerUsuarioPorCedula","generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 02. Mi Evaluación De Desempeño › 00. Mi_Evaluacion_De_Desempeño

**Al entrar, en este orden:**

1. 💬 Dice: «Aquí podrás recordar los resultados de tu valoración desempeño, reconocer lo que lograste y definir lo que fortalecerás para seguir creciendo en Outsourcing.»
2. 💬 Dice: «¿Te gustaría revisar y recordar tus resultados de valoración del desempeño?✅»
3. 🔘 Ofrece los botones: «Si» · «No»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `competencia1`, `competencia2`, `competencia3`, `competencia4`, `competencia5`, `notaCompetencia1`, `notaCompetencia2`, `notaCompetencia3`, `notaCompetencia4`, `notaCompetencia5`, `notaTotalED`, `nivelED`, `competenciaMenor1`, `competenciaMenor2`, `cargoEvaluacionDesempeno`, `nombresEvaluacionDesempeno`, `apellidosEvaluacionDesempeno`, `nombreCompletoEvaluacionDesempeno`, `notaCompetencia6`, `notaCompetencia7`

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`opcion_si`) → página «01. Resultados Evaluación De Desempeño»  
  ↳ ⚡ Llama al webhook **Evaluación de Desempeño** [ObtenerEvaluacionDesempeno] — lectura
- «No» (`opcion_no`) → página «02. No Resultados Evaluación De Desempeño»

```json camila:page
{"flow":"02. Mi Evaluación De Desempeño","page":{"name":"00. Mi_Evaluacion_De_Desempeño","entry":{"messages":[{"type":"text","texts":["Aquí podrás recordar los resultados de tu valoración desempeño, reconocer lo que lograste y definir lo que fortalecerás para seguir creciendo en Outsourcing."],"buttons":[]},{"type":"text","texts":["¿Te gustaría revisar y recordar tus resultados de\nvaloración del desempeño?✅"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"opcion_si"},{"text":"No","event":"opcion_no"}]}],"setParams":[],"generators":[]},"form":[{"name":"competencia1","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"competencia2","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"competencia3","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"competencia4","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"competencia5","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"notaCompetencia1","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"notaCompetencia2","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"notaCompetencia3","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"notaCompetencia4","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"notaCompetencia5","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"notaTotalED","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"nivelED","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"competenciaMenor1","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"competenciaMenor2","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"cargoEvaluacionDesempeno","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"nombresEvaluacionDesempeno","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"apellidosEvaluacionDesempeno","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"nombreCompletoEvaluacionDesempeno","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"notaCompetencia6","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"notaCompetencia7","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[],"events":[{"event":"opcion_si","targetPage":"01. Resultados Evaluación De Desempeño","fulfillment":{"messages":[],"setParams":[],"webhook":"Evaluación de Desempeño","tag":"ObtenerEvaluacionDesempeno","generators":[]}},{"event":"opcion_no","targetPage":"02. No Resultados Evaluación De Desempeño","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 02. Mi Evaluación De Desempeño › 00.00 Creacion de aprametro restantes

**Al entrar, en este orden:**

_No hace nada al entrar._

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `notaCompetencia8`, `competencia6`, `competencia7`, `competencia8`, `centroCostos`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «00. Mi_Evaluacion_De_Desempeño»

```json camila:page
{"flow":"02. Mi Evaluación De Desempeño","page":{"name":"00.00 Creacion de aprametro restantes","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"notaCompetencia8","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"competencia6","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"competencia7","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"competencia8","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"centroCostos","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"true","targetPage":"00. Mi_Evaluacion_De_Desempeño","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 02. Mi Evaluación De Desempeño › 01. Resultados Evaluación De Desempeño

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.totalCompetenciasED = 3 AND $session.params.notaCompetencia1 = 100 AND $session.params.notaCompetencia2 = 100 AND $session.params.notaCompetencia3 = 100` → página «01.4. Selección libre de competencia»  
  ↳ 💬 dice «Estos son los resultados de tu Evaluación de Desempeño:    *…»
- si `$session.params.totalCompetenciasED = 4 AND $session.params.notaCompetencia1 = 100 AND $session.params.notaCompetencia2 = 100 AND $session.params.notaCompetencia3 = 100 AND $session.params.notaCompetencia4 = 100` → página «01.4. Selección libre de competencia»  
  ↳ 💬 dice «Estos son los resultados de tu Evaluación de Desempeño:    *…»
- si `$session.params.totalCompetenciasED = 5 AND $session.params.notaCompetencia1 = 100 AND $session.params.notaCompetencia2 = 100 AND $session.params.notaCompetencia3 = 100 AND $session.params.notaCompetencia4 = 100 AND $session.params.notaCompetencia5 = 100` → página «01.4. Selección libre de competencia»  
  ↳ 💬 dice «Estos son los resultados de tu Evaluación de Desempeño:    *…»
- si `$session.params.totalCompetenciasED = 6 AND $session.params.notaCompetencia1 = 100 AND $session.params.notaCompetencia2 = 100 AND $session.params.notaCompetencia3 = 100 AND $session.params.notaCompetencia4 = 100 AND $session.params.notaCompetencia5 = 100 AND $session.params.notaCompetencia6 = 100` → página «01.4. Selección libre de competencia»  
  ↳ 💬 dice «Estos son los resultados de tu Evaluación de Desempeño:    *…»
- si `$session.params.totalCompetenciasED = 7 AND $session.params.notaCompetencia1 = 100 AND $session.params.notaCompetencia2 = 100 AND $session.params.notaCompetencia3 = 100 AND $session.params.notaCompetencia4 = 100 AND $session.params.notaCompetencia5 = 100 AND $session.params.notaCompetencia6 = 100 AND $session.params.notaCompetencia7 = 100` → página «01.4. Selección libre de competencia»  
  ↳ 💬 dice «Estos son los resultados de tu Evaluación de Desempeño:    *…»
- si `$session.params.totalCompetenciasED = 8 AND $session.params.notaCompetencia1 = 100 AND $session.params.notaCompetencia2 = 100 AND $session.params.notaCompetencia3 = 100 AND $session.params.notaCompetencia4 = 100 AND $session.params.notaCompetencia5 = 100 AND $session.params.notaCompetencia6 = 100 AND $session.params.notaCompetencia7 = 100 AND $session.params.notaCompetencia8 = 100` → página «01.4. Selección libre de competencia»  
  ↳ 💬 dice «Estos son los resultados de tu Evaluación de Desempeño:    *…»
- si `$session.params.totalCompetenciasED = 8` → página «01.2. Resultado a fortalecer»  
  ↳ 📌 asigna `nombreCompletoEvaluacionDesempeno` = `$session.params.nombresEvaluacionDesempeno $session.params.a…` · 💬 dice «Perfecto, estos son tus resultados:  Nivel: $session.params.…»
- si `$session.params.totalCompetenciasED = 7` → página «01.2. Resultado a fortalecer»  
  ↳ 📌 asigna `nombreCompletoEvaluacionDesempeno` = `$session.params.nombresEvaluacionDesempeno $session.params.a…` · 💬 dice «Perfecto, estos son tus resultados:  Nivel: $session.params.…»
- si `$session.params.totalCompetenciasED = 6` → página «01.2. Resultado a fortalecer»  
  ↳ 📌 asigna `nombreCompletoEvaluacionDesempeno` = `$session.params.nombresEvaluacionDesempeno $session.params.a…` · 💬 dice «Perfecto, estos son tus resultados:  Nivel: $session.params.…»
- si `$session.params.totalCompetenciasED = 5` → página «01.2. Resultado a fortalecer»  
  ↳ 📌 asigna `nombreCompletoEvaluacionDesempeno` = `$session.params.nombresEvaluacionDesempeno $session.params.a…` · 💬 dice «Perfecto, estos son tus resultados:  Nivel: $session.params.…»
- si `$session.params.totalCompetenciasED = 4` → página «01.2. Resultado a fortalecer»  
  ↳ 📌 asigna `nombreCompletoEvaluacionDesempeno` = `$session.params.nombresEvaluacionDesempeno $session.params.a…` · 💬 dice «Perfecto, estos son tus resultados:  Nivel: $session.params.…»
- si `$session.params.totalCompetenciasED = 3` → página «01.2. Resultado a fortalecer»  
  ↳ 📌 asigna `nombreCompletoEvaluacionDesempeno` = `$session.params.nombresEvaluacionDesempeno $session.params.a…` · 💬 dice «Perfecto, estos son tus resultados:  Nivel: $session.params.…»
- si `$session.params.notaTotalED = null AND $session.params.totalCompetenciasED = null` → flujo **12. ¿Algo Mas?**  
  ↳ 💬 dice «Gracias por tu interés en realizar tu Plan de Desarrollo Ind…»
- si `$session.params.totalCompetenciasED = 3 AND $session.params.notaCompetencia1 != null AND $session.params.notaCompetencia1 != 100 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia2 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia3` → página «01.4. Selección libre de competencia»  
  ↳ 💬 dice «Estos son los resultados de tu Evaluación de Desempeño:    *…»
- si `$session.params.totalCompetenciasED = 4 AND $session.params.notaCompetencia1 != null AND $session.params.notaCompetencia1 != 100 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia2 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia3 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia4` → página «01.4. Selección libre de competencia»  
  ↳ 💬 dice «Estos son los resultados de tu Evaluación de Desempeño:    *…»
- si `$session.params.totalCompetenciasED = 5 AND $session.params.notaCompetencia1 != null AND $session.params.notaCompetencia1 != 100 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia2 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia3 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia4 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia5` → página «01.4. Selección libre de competencia»  
  ↳ 💬 dice «Estos son los resultados de tu Evaluación de Desempeño:    *…»
- si `$session.params.totalCompetenciasED = 6 AND $session.params.notaCompetencia1 != null AND $session.params.notaCompetencia1 != 100 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia2 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia3 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia4 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia5 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia6` → página «01.4. Selección libre de competencia»  
  ↳ 💬 dice «Estos son los resultados de tu Evaluación de Desempeño:    *…»
- si `$session.params.totalCompetenciasED = 7 AND $session.params.notaCompetencia1 != null AND $session.params.notaCompetencia1 != 100 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia2 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia3 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia4 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia5 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia6 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia7` → página «01.4. Selección libre de competencia»  
  ↳ 💬 dice «Estos son los resultados de tu Evaluación de Desempeño:    *…»
- si `$session.params.totalCompetenciasED = 8 AND $session.params.notaCompetencia1 != null AND $session.params.notaCompetencia1 != 100 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia2 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia3 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia4 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia5 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia6 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia7 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia8` → página «01.4. Selección libre de competencia»  
  ↳ 💬 dice «Estos son los resultados de tu Evaluación de Desempeño:    *…»

```json camila:page
{"flow":"02. Mi Evaluación De Desempeño","page":{"name":"01. Resultados Evaluación De Desempeño","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"$session.params.totalCompetenciasED = 3 AND $session.params.notaCompetencia1 = 100 AND $session.params.notaCompetencia2 = 100 AND $session.params.notaCompetencia3 = 100","targetPage":"01.4. Selección libre de competencia","fulfillment":{"messages":[{"type":"text","texts":["Estos son los resultados de tu Evaluación de Desempeño:\r\n\r\n*Calificación general:* $session.params.notaTotalED\r\n\r\n*Resultados por competencia:*\r\n\r\n· $session.params.competencia1: $session.params.notaCompetencia1\r\n· $session.params.competencia2: $session.params.notaCompetencia2\r\n· $session.params.competencia3: $session.params.notaCompetencia3\r\n\r\n¡Felicitaciones! \uD83C\uDF89 Has obtenido una calificación de 100 en todas tus competencias, lo que refleja un excelente desempeño.\r\n\r\nSin embargo, siempre podemos continuar fortaleciendo nuestras habilidades y desarrollándonos profesionalmente.\r\n\r\nPor eso, queremos que seas tú quien elija cuál de estas competencias te gustaría continuar fortaleciendo en tu Plan de Desarrollo Individual."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 4 AND $session.params.notaCompetencia1 = 100 AND $session.params.notaCompetencia2 = 100 AND $session.params.notaCompetencia3 = 100 AND $session.params.notaCompetencia4 = 100","targetPage":"01.4. Selección libre de competencia","fulfillment":{"messages":[{"type":"text","texts":["Estos son los resultados de tu Evaluación de Desempeño:\r\n\r\n*Calificación general:* $session.params.notaTotalED\r\n\r\n*Resultados por competencia:*\r\n\r\n· $session.params.competencia1: $session.params.notaCompetencia1\r\n· $session.params.competencia2: $session.params.notaCompetencia2\r\n· $session.params.competencia3: $session.params.notaCompetencia3\r\n· $session.params.competencia4: $session.params.notaCompetencia4\r\n\r\n¡Felicitaciones! \uD83C\uDF89 Has obtenido una calificación de 100 en todas tus competencias, lo que refleja un excelente desempeño.\r\n\r\nSin embargo, siempre podemos continuar fortaleciendo nuestras habilidades y desarrollándonos profesionalmente.\r\n\r\nPor eso, queremos que seas tú quien elija cuál de estas competencias te gustaría continuar fortaleciendo en tu Plan de Desarrollo Individual."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 5 AND $session.params.notaCompetencia1 = 100 AND $session.params.notaCompetencia2 = 100 AND $session.params.notaCompetencia3 = 100 AND $session.params.notaCompetencia4 = 100 AND $session.params.notaCompetencia5 = 100","targetPage":"01.4. Selección libre de competencia","fulfillment":{"messages":[{"type":"text","texts":["Estos son los resultados de tu Evaluación de Desempeño:\r\n\r\n*Calificación general:* $session.params.notaTotalED\r\n\r\n*Resultados por competencia:*\r\n\r\n· $session.params.competencia1: $session.params.notaCompetencia1\r\n· $session.params.competencia2: $session.params.notaCompetencia2\r\n· $session.params.competencia3: $session.params.notaCompetencia3\r\n· $session.params.competencia4: $session.params.notaCompetencia4\r\n· $session.params.competencia5: $session.params.notaCompetencia5\r\n\r\n¡Felicitaciones! \uD83C\uDF89 Has obtenido una calificación de 100 en todas tus competencias, lo que refleja un excelente desempeño.\r\n\r\nSin embargo, siempre podemos continuar fortaleciendo nuestras habilidades y desarrollándonos profesionalmente.\r\n\r\nPor eso, queremos que seas tú quien elija cuál de estas competencias te gustaría continuar fortaleciendo en tu Plan de Desarrollo Individual."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 6 AND $session.params.notaCompetencia1 = 100 AND $session.params.notaCompetencia2 = 100 AND $session.params.notaCompetencia3 = 100 AND $session.params.notaCompetencia4 = 100 AND $session.params.notaCompetencia5 = 100 AND $session.params.notaCompetencia6 = 100","targetPage":"01.4. Selección libre de competencia","fulfillment":{"messages":[{"type":"text","texts":["Estos son los resultados de tu Evaluación de Desempeño:\r\n\r\n*Calificación general:* $session.params.notaTotalED\r\n\r\n*Resultados por competencia:*\r\n\r\n· $session.params.competencia1: $session.params.notaCompetencia1\r\n· $session.params.competencia2: $session.params.notaCompetencia2\r\n· $session.params.competencia3: $session.params.notaCompetencia3\r\n· $session.params.competencia4: $session.params.notaCompetencia4\r\n· $session.params.competencia5: $session.params.notaCompetencia5\r\n· $session.params.competencia6: $session.params.notaCompetencia6\r\n\r\n¡Felicitaciones! \uD83C\uDF89 Has obtenido una calificación de 100 en todas tus competencias, lo que refleja un excelente desempeño.\r\n\r\nSin embargo, siempre podemos continuar fortaleciendo nuestras habilidades y desarrollándonos profesionalmente.\r\n\r\nPor eso, queremos que seas tú quien elija cuál de estas competencias te gustaría continuar fortaleciendo en tu Plan de Desarrollo Individual."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 7 AND $session.params.notaCompetencia1 = 100 AND $session.params.notaCompetencia2 = 100 AND $session.params.notaCompetencia3 = 100 AND $session.params.notaCompetencia4 = 100 AND $session.params.notaCompetencia5 = 100 AND $session.params.notaCompetencia6 = 100 AND $session.params.notaCompetencia7 = 100","targetPage":"01.4. Selección libre de competencia","fulfillment":{"messages":[{"type":"text","texts":["Estos son los resultados de tu Evaluación de Desempeño:\r\n\r\n*Calificación general:* $session.params.notaTotalED\r\n\r\n*Resultados por competencia:*\r\n\r\n· $session.params.competencia1: $session.params.notaCompetencia1\r\n· $session.params.competencia2: $session.params.notaCompetencia2\r\n· $session.params.competencia3: $session.params.notaCompetencia3\r\n· $session.params.competencia4: $session.params.notaCompetencia4\r\n· $session.params.competencia5: $session.params.notaCompetencia5\r\n· $session.params.competencia6: $session.params.notaCompetencia6\r\n· $session.params.competencia7: $session.params.notaCompetencia7\r\n\r\n¡Felicitaciones! \uD83C\uDF89 Has obtenido una calificación de 100 en todas tus competencias, lo que refleja un excelente desempeño.\r\n\r\nSin embargo, siempre podemos continuar fortaleciendo nuestras habilidades y desarrollándonos profesionalmente.\r\n\r\nPor eso, queremos que seas tú quien elija cuál de estas competencias te gustaría continuar fortaleciendo en tu Plan de Desarrollo Individual."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 8 AND $session.params.notaCompetencia1 = 100 AND $session.params.notaCompetencia2 = 100 AND $session.params.notaCompetencia3 = 100 AND $session.params.notaCompetencia4 = 100 AND $session.params.notaCompetencia5 = 100 AND $session.params.notaCompetencia6 = 100 AND $session.params.notaCompetencia7 = 100 AND $session.params.notaCompetencia8 = 100","targetPage":"01.4. Selección libre de competencia","fulfillment":{"messages":[{"type":"text","texts":["Estos son los resultados de tu Evaluación de Desempeño:\r\n\r\n*Calificación general:* $session.params.notaTotalED\r\n\r\n*Resultados por competencia:*\r\n\r\n· $session.params.competencia1: $session.params.notaCompetencia1\r\n· $session.params.competencia2: $session.params.notaCompetencia2\r\n· $session.params.competencia3: $session.params.notaCompetencia3\r\n· $session.params.competencia4: $session.params.notaCompetencia4\r\n· $session.params.competencia5: $session.params.notaCompetencia5\r\n· $session.params.competencia6: $session.params.notaCompetencia6\r\n· $session.params.competencia7: $session.params.notaCompetencia7\r\n· $session.params.competencia8: $session.params.notaCompetencia8\r\n\r\n¡Felicitaciones! \uD83C\uDF89 Has obtenido una calificación de 100 en todas tus competencias, lo que refleja un excelente desempeño.\r\n\r\nSin embargo, siempre podemos continuar fortaleciendo nuestras habilidades y desarrollándonos profesionalmente.\r\n\r\nPor eso, queremos que seas tú quien elija cuál de estas competencias te gustaría continuar fortaleciendo en tu Plan de Desarrollo Individual."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 8","targetPage":"01.2. Resultado a fortalecer","fulfillment":{"messages":[{"type":"text","texts":["Perfecto, estos son tus resultados:\n\nNivel: $session.params.nivelED\nNOTA TOTAL: $session.params.notaTotalED %\n\n✅ $session.params.competencia1 $session.params.notaCompetencia1 %\n✅ $session.params.competencia2 $session.params.notaCompetencia2 %\n✅ $session.params.competencia3 $session.params.notaCompetencia3 %\n✅ $session.params.competencia4 $session.params.notaCompetencia4 %\n✅ $session.params.competencia5 $session.params.notaCompetencia5 %\n✅ $session.params.competencia6 $session.params.notaCompetencia6 %\n✅ $session.params.competencia7 $session.params.notaCompetencia7 %\n✅ $session.params.competencia8 $session.params.notaCompetencia8 %"],"buttons":[]}],"setParams":[{"parameter":"nombreCompletoEvaluacionDesempeno","value":"$session.params.nombresEvaluacionDesempeno $session.params.apellidosEvaluacionDesempeno"}],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 7","targetPage":"01.2. Resultado a fortalecer","fulfillment":{"messages":[{"type":"text","texts":["Perfecto, estos son tus resultados:\n\nNivel: $session.params.nivelED\nNOTA TOTAL: $session.params.notaTotalED %\n\n✅ $session.params.competencia1 $session.params.notaCompetencia1 %\n✅ $session.params.competencia2 $session.params.notaCompetencia2 %\n✅ $session.params.competencia3 $session.params.notaCompetencia3 %\n✅ $session.params.competencia4 $session.params.notaCompetencia4 %\n✅ $session.params.competencia5 $session.params.notaCompetencia5 %\n✅ $session.params.competencia6 $session.params.notaCompetencia6 %\n✅ $session.params.competencia7 $session.params.notaCompetencia7 %"],"buttons":[]}],"setParams":[{"parameter":"nombreCompletoEvaluacionDesempeno","value":"$session.params.nombresEvaluacionDesempeno $session.params.apellidosEvaluacionDesempeno"}],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 6","targetPage":"01.2. Resultado a fortalecer","fulfillment":{"messages":[{"type":"text","texts":["Perfecto, estos son tus resultados:\n\nNivel: $session.params.nivelED\nNOTA TOTAL: $session.params.notaTotalED %\n\n✅ $session.params.competencia1 $session.params.notaCompetencia1 %\n✅ $session.params.competencia2 $session.params.notaCompetencia2 %\n✅ $session.params.competencia3 $session.params.notaCompetencia3 %\n✅ $session.params.competencia4 $session.params.notaCompetencia4 %\n✅ $session.params.competencia5 $session.params.notaCompetencia5 %\n✅ $session.params.competencia6 $session.params.notaCompetencia6 %"],"buttons":[]}],"setParams":[{"parameter":"nombreCompletoEvaluacionDesempeno","value":"$session.params.nombresEvaluacionDesempeno $session.params.apellidosEvaluacionDesempeno"}],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 5","targetPage":"01.2. Resultado a fortalecer","fulfillment":{"messages":[{"type":"text","texts":["Perfecto, estos son tus resultados:\n\nNivel: $session.params.nivelED\nNOTA TOTAL: $session.params.notaTotalED %\n\n✅ $session.params.competencia1 $session.params.notaCompetencia1 %\n✅ $session.params.competencia2 $session.params.notaCompetencia2 %\n✅ $session.params.competencia3 $session.params.notaCompetencia3 %\n✅ $session.params.competencia4 $session.params.notaCompetencia4 %\n✅ $session.params.competencia5 $session.params.notaCompetencia5 %\n"],"buttons":[]}],"setParams":[{"parameter":"nombreCompletoEvaluacionDesempeno","value":"$session.params.nombresEvaluacionDesempeno $session.params.apellidosEvaluacionDesempeno"}],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 4","targetPage":"01.2. Resultado a fortalecer","fulfillment":{"messages":[{"type":"text","texts":["Perfecto, estos son tus resultados:\n\nNivel: $session.params.nivelED\nNOTA TOTAL: $session.params.notaTotalED %\n\n✅ $session.params.competencia1 $session.params.notaCompetencia1 %\n✅ $session.params.competencia2 $session.params.notaCompetencia2 %\n✅ $session.params.competencia3 $session.params.notaCompetencia3 %\n✅ $session.params.competencia4 $session.params.notaCompetencia4 %\n\n"],"buttons":[]}],"setParams":[{"parameter":"nombreCompletoEvaluacionDesempeno","value":"$session.params.nombresEvaluacionDesempeno $session.params.apellidosEvaluacionDesempeno"}],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 3","targetPage":"01.2. Resultado a fortalecer","fulfillment":{"messages":[{"type":"text","texts":["Perfecto, estos son tus resultados:\n\nNivel: $session.params.nivelED\nNOTA TOTAL: $session.params.notaTotalED %\n\n✅ $session.params.competencia1 $session.params.notaCompetencia1 %\n✅ $session.params.competencia2 $session.params.notaCompetencia2 %\n✅ $session.params.competencia3 $session.params.notaCompetencia3 %\n\n"],"buttons":[]}],"setParams":[{"parameter":"nombreCompletoEvaluacionDesempeno","value":"$session.params.nombresEvaluacionDesempeno $session.params.apellidosEvaluacionDesempeno"}],"generators":[]}},{"condition":"$session.params.notaTotalED = null AND $session.params.totalCompetenciasED = null","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[{"type":"text","texts":["Gracias por tu interés en realizar tu Plan de Desarrollo Individual. \uD83D\uDE0A\n\nSin embargo, para este periodo no cuentas con una calificación de evaluación de desempeño registrada."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 3 AND $session.params.notaCompetencia1 != null AND $session.params.notaCompetencia1 != 100 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia2 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia3","targetPage":"01.4. Selección libre de competencia","fulfillment":{"messages":[{"type":"text","texts":["Estos son los resultados de tu Evaluación de Desempeño:\r\n\r\n*Calificación general:* $session.params.notaTotalED\r\n\r\n*Resultados por competencia:*\r\n\r\n· $session.params.competencia1: $session.params.notaCompetencia1\r\n· $session.params.competencia2: $session.params.notaCompetencia2\r\n· $session.params.competencia3: $session.params.notaCompetencia3\r\n\r\nObservamos que todas tus competencias tienen la misma calificación. Esto significa que ninguna presenta una puntuación inferior a las demás.\r\n\r\nPor esta razón, queremos que seas tú quien elija cuál de estas competencias te gustaría fortalecer en tu Plan de Desarrollo Individual."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 4 AND $session.params.notaCompetencia1 != null AND $session.params.notaCompetencia1 != 100 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia2 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia3 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia4","targetPage":"01.4. Selección libre de competencia","fulfillment":{"messages":[{"type":"text","texts":["Estos son los resultados de tu Evaluación de Desempeño:\r\n\r\n*Calificación general:* $session.params.notaTotalED\r\n\r\n*Resultados por competencia:*\r\n\r\n· $session.params.competencia1: $session.params.notaCompetencia1\r\n· $session.params.competencia2: $session.params.notaCompetencia2\r\n· $session.params.competencia3: $session.params.notaCompetencia3\r\n· $session.params.competencia4: $session.params.notaCompetencia4\r\n\r\nObservamos que todas tus competencias tienen la misma calificación. Esto significa que ninguna presenta una puntuación inferior a las demás.\r\n\r\nPor esta razón, queremos que seas tú quien elija cuál de estas competencias te gustaría fortalecer en tu Plan de Desarrollo Individual."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 5 AND $session.params.notaCompetencia1 != null AND $session.params.notaCompetencia1 != 100 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia2 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia3 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia4 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia5","targetPage":"01.4. Selección libre de competencia","fulfillment":{"messages":[{"type":"text","texts":["Estos son los resultados de tu Evaluación de Desempeño:\r\n\r\n*Calificación general:* $session.params.notaTotalED\r\n\r\n*Resultados por competencia:*\r\n\r\n· $session.params.competencia1: $session.params.notaCompetencia1\r\n· $session.params.competencia2: $session.params.notaCompetencia2\r\n· $session.params.competencia3: $session.params.notaCompetencia3\r\n· $session.params.competencia4: $session.params.notaCompetencia4\r\n· $session.params.competencia5: $session.params.notaCompetencia5\r\n\r\nObservamos que todas tus competencias tienen la misma calificación. Esto significa que ninguna presenta una puntuación inferior a las demás.\r\n\r\nPor esta razón, queremos que seas tú quien elija cuál de estas competencias te gustaría fortalecer en tu Plan de Desarrollo Individual."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 6 AND $session.params.notaCompetencia1 != null AND $session.params.notaCompetencia1 != 100 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia2 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia3 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia4 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia5 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia6","targetPage":"01.4. Selección libre de competencia","fulfillment":{"messages":[{"type":"text","texts":["Estos son los resultados de tu Evaluación de Desempeño:\r\n\r\n*Calificación general:* $session.params.notaTotalED\r\n\r\n*Resultados por competencia:*\r\n\r\n· $session.params.competencia1: $session.params.notaCompetencia1\r\n· $session.params.competencia2: $session.params.notaCompetencia2\r\n· $session.params.competencia3: $session.params.notaCompetencia3\r\n· $session.params.competencia4: $session.params.notaCompetencia4\r\n· $session.params.competencia5: $session.params.notaCompetencia5\r\n· $session.params.competencia6: $session.params.notaCompetencia6\r\n\r\nObservamos que todas tus competencias tienen la misma calificación. Esto significa que ninguna presenta una puntuación inferior a las demás.\r\n\r\nPor esta razón, queremos que seas tú quien elija cuál de estas competencias te gustaría fortalecer en tu Plan de Desarrollo Individual."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 7 AND $session.params.notaCompetencia1 != null AND $session.params.notaCompetencia1 != 100 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia2 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia3 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia4 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia5 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia6 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia7","targetPage":"01.4. Selección libre de competencia","fulfillment":{"messages":[{"type":"text","texts":["Estos son los resultados de tu Evaluación de Desempeño:\r\n\r\n*Calificación general:* $session.params.notaTotalED\r\n\r\n*Resultados por competencia:*\r\n\r\n· $session.params.competencia1: $session.params.notaCompetencia1\r\n· $session.params.competencia2: $session.params.notaCompetencia2\r\n· $session.params.competencia3: $session.params.notaCompetencia3\r\n· $session.params.competencia4: $session.params.notaCompetencia4\r\n· $session.params.competencia5: $session.params.notaCompetencia5\r\n· $session.params.competencia6: $session.params.notaCompetencia6\r\n· $session.params.competencia7: $session.params.notaCompetencia7\r\n\r\nObservamos que todas tus competencias tienen la misma calificación. Esto significa que ninguna presenta una puntuación inferior a las demás.\r\n\r\nPor esta razón, queremos que seas tú quien elija cuál de estas competencias te gustaría fortalecer en tu Plan de Desarrollo Individual."],"buttons":[]}],"setParams":[],"generators":[]}},{"condition":"$session.params.totalCompetenciasED = 8 AND $session.params.notaCompetencia1 != null AND $session.params.notaCompetencia1 != 100 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia2 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia3 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia4 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia5 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia6 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia7 AND $session.params.notaCompetencia1 = $session.params.notaCompetencia8","targetPage":"01.4. Selección libre de competencia","fulfillment":{"messages":[{"type":"text","texts":["Estos son los resultados de tu Evaluación de Desempeño:\r\n\r\n*Calificación general:* $session.params.notaTotalED\r\n\r\n*Resultados por competencia:*\r\n\r\n· $session.params.competencia1: $session.params.notaCompetencia1\r\n· $session.params.competencia2: $session.params.notaCompetencia2\r\n· $session.params.competencia3: $session.params.notaCompetencia3\r\n· $session.params.competencia4: $session.params.notaCompetencia4\r\n· $session.params.competencia5: $session.params.notaCompetencia5\r\n· $session.params.competencia6: $session.params.notaCompetencia6\r\n· $session.params.competencia7: $session.params.notaCompetencia7\r\n· $session.params.competencia8: $session.params.notaCompetencia8\r\n\r\nObservamos que todas tus competencias tienen la misma calificación. Esto significa que ninguna presenta una puntuación inferior a las demás.\r\n\r\nPor esta razón, queremos que seas tú quien elija cuál de estas competencias te gustaría fortalecer en tu Plan de Desarrollo Individual."],"buttons":[]}],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 02. Mi Evaluación De Desempeño › 01.1. Resultado mayor satisfacción

**Al entrar, en este orden:**

1. 📌 Asigna: `mayorSatisfaccion` = ``

```json camila:page
{"flow":"02. Mi Evaluación De Desempeño","page":{"name":"01.1. Resultado mayor satisfacción","entry":{"messages":[],"setParams":[{"parameter":"$session.params.mayorSatisfaccion"}],"generators":[]},"form":[],"routes":[],"events":[],"groups":[]}}
```

### Página · 02. Mi Evaluación De Desempeño › 01.2. Resultado a fortalecer

**Al entrar, en este orden:**

1. 📌 Asigna: `competenciaAFortalecer` = ``

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «01.3. Sugerir competencias a fortalecer»

```json camila:page
{"flow":"02. Mi Evaluación De Desempeño","page":{"name":"01.2. Resultado a fortalecer","entry":{"messages":[],"setParams":[{"parameter":"$session.params.competenciaAFortalecer"}],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"01.3. Sugerir competencias a fortalecer","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 02. Mi Evaluación De Desempeño › 01.3. Sugerir competencias a fortalecer

**Al entrar, en este orden:**

_No hace nada al entrar._

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `mensajeGenerativoCompetencia1`, `mensajeGenerativoCompetencia2`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **02.1. Conversacional Evaluacion Desempeño**

```json camila:page
{"flow":"02. Mi Evaluación De Desempeño","page":{"name":"01.3. Sugerir competencias a fortalecer","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"mensajeGenerativoCompetencia1","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mensajeGenerativoCompetencia2","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"true","targetFlow":"02.1. Conversacional Evaluacion Desempeño","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 02. Mi Evaluación De Desempeño › 01.4. Selección libre de competencia

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `competenciaAFortalecer` — @sys.any · pregunta: «¿Cuál de estas competencias te gustaría fortalecer en tu Plan de Desarrollo Individual?»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL" AND $session.params.competenciaAFortalecer != null AND ($sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia1) OR $sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia2) OR $sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia3) OR $sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia4) OR $sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia5) OR $sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia6))` → flujo **02.1. Conversacional Evaluacion Desempeño**  
  ↳ 📌 asigna `seleccionLibrePDI` = `true`, `segundaCompetencia` = `false`, `mensajeUsuario1` = ``

```json camila:page
{"flow":"02. Mi Evaluación De Desempeño","page":{"name":"01.4. Selección libre de competencia","entry":{"messages":[],"setParams":[],"generators":[]},"form":[{"name":"competenciaAFortalecer","entity":"@sys.any","required":true,"prompt":{"messages":[{"type":"text","texts":["¿Cuál de estas competencias te gustaría fortalecer en tu Plan de Desarrollo Individual?"],"buttons":[]}],"setParams":[{"parameter":"competenciaAFortalecer"}],"generators":[]}}],"routes":[{"condition":"$page.params.status = \"FINAL\" AND $session.params.competenciaAFortalecer != null AND ($sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia1) OR $sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia2) OR $sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia3) OR $sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia4) OR $sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia5) OR $sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia6))","targetFlow":"02.1. Conversacional Evaluacion Desempeño","fulfillment":{"messages":[],"setParams":[{"parameter":"seleccionLibrePDI","value":true},{"parameter":"segundaCompetencia","value":false},{"parameter":"mensajeUsuario1"}],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 02. Mi Evaluación De Desempeño › 02. Mensaje2

**Al entrar, en este orden:**

_No hace nada al entrar._

```json camila:page
{"flow":"02. Mi Evaluación De Desempeño","page":{"name":"02. Mensaje2","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[],"groups":[]}}
```

### Página · 02. Mi Evaluación De Desempeño › 02. No Resultados Evaluación De Desempeño

**Al entrar, en este orden:**

1. 💬 Dice: «Esta bien, aunque ten presente que tu desempeño está en tus manos y es importante que consultes tus resultados con regularidad para comprender en qué punto estás ✅»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → flujo **12. ¿Algo Mas?**

```json camila:page
{"flow":"02. Mi Evaluación De Desempeño","page":{"name":"02. No Resultados Evaluación De Desempeño","entry":{"messages":[{"type":"text","texts":["Esta bien, aunque ten presente que tu desempeño está en tus manos y es importante que consultes tus resultados con regularidad para comprender en qué punto estás ✅"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true = true","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 02. Mi Evaluación De Desempeño › 03. ¿Algo mas?

**Al entrar, en este orden:**

1. 📌 Asigna: `desempeno` = ``, `consultaGenerativa` = ``, `mensajeGenerativoGlobal` = ``
2. 💬 Dice: «¿Algo más en lo que pueda ayudarte?»
3. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si_menu`) → flujo **Default Start Flow**  
  ↳ 📌 asigna `menu` = `true`, `desempeno` = `false`
- «No» (`no`) → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true`

```json camila:page
{"flow":"02. Mi Evaluación De Desempeño","page":{"name":"03. ¿Algo mas?","entry":{"messages":[{"type":"text","texts":["¿Algo más en lo que pueda ayudarte?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si_menu"},{"text":"No","event":"no"}]}],"setParams":[{"parameter":"$session.params.desempeno"},{"parameter":"$session.params.consultaGenerativa"},{"parameter":"$session.params.mensajeGenerativoGlobal"}],"generators":[]},"form":[],"routes":[],"events":[{"event":"si_menu","targetFlow":"Default Start Flow","fulfillment":{"messages":[],"setParams":[{"parameter":"menu","value":true},{"parameter":"$session.params.desempeno","value":false}],"generators":[]}},{"event":"no","targetFlow":"10. Cierre de conversación","fulfillment":{"messages":[],"setParams":[{"parameter":"cierre","value":true}],"generators":[]}}],"groups":[]}}
```

