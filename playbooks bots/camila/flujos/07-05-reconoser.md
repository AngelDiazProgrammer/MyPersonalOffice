---
bot: camila
flow: 05. Reconoser
---
## Flujo · 05. Reconoser

**Qué dice al empezar:** «Tus resultados hablan de tu esfuerzo. Revisarlos es una oportunidad para reconocer tu avance y definir el siguiente paso.»

**Se llega desde:**
- Default Start Flow › 05.Menu_Principal ← evento `indicadores_reconoser`
- flujo (global) ← intent «route_Reconoser»
- flujo Default Start Flow ← intent «route_Reconoser»

**Webhooks que usa:** ⚡ Login_API_Reconoser · ⚡ Mis indicadores ReconoSER

**Páginas:**
- 00. Mis_Indicadores_ReconoSer 
- 01.1. SolicitarMesIndicadores 
- 01.1.1 Mas meses 
- 01.2. Confirmar información de consulta — ⚡ Mis indicadores ReconoSER
- 01.3. Mostrar resultados Reconoser 
- 01.ConocerIndicadoresReconoser 
- 02.NoConocerIndicadoresReconoser 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.reconoser = true` → página «00. Mis_Indicadores_ReconoSer»  
  ↳ ⚡ Llama al webhook **Login_API_Reconoser** [Authentication] — lectura
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"05. Reconoser","routes":[{"condition":"$session.params.reconoser = true","targetPage":"00. Mis_Indicadores_ReconoSer","fulfillment":{"messages":[],"setParams":[],"webhook":"Login_API_Reconoser","tag":"Authentication","generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 05. Reconoser › 00. Mis_Indicadores_ReconoSer

**Al entrar, en este orden:**

1. 📌 Asigna: `intencion` = ``
2. 💬 Dice: «Tus resultados hablan de tu esfuerzo. Revisarlos es una oportunidad para reconocer tu avance y definir el siguiente paso.»
3. 💬 Dice: «¿Te gustaría conocer más a profundidad el  programa?»
4. 🔘 Ofrece los botones: «Si» · «No»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `anio_actual`, `anio_anterior`, `mes_elegido`, `mes_elegido_consulta`

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → página «01.ConocerIndicadoresReconoser»  
  ↳ 📌 asigna `anio_actual` = `$sys.func.FORMAT_DATE($sys.func.NOW(), yyyy)`, `anio_anterior` = `$sys.func.FORMAT_DATE($sys.func.ADD_DATE($sys.func.NOW(), -1…`
- «No» (`no`) → página «02.NoConocerIndicadoresReconoser»

```json camila:page
{"flow":"05. Reconoser","page":{"name":"00. Mis_Indicadores_ReconoSer","entry":{"messages":[{"type":"text","texts":["Tus resultados hablan de tu esfuerzo. Revisarlos es\nuna oportunidad para reconocer tu avance y definir\nel siguiente paso."],"buttons":[]},{"type":"text","texts":["¿Te gustaría conocer más a profundidad el\r\nprograma?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si"},{"text":"No","event":"no"}]}],"setParams":[{"parameter":"$session.params.intencion"}],"generators":[]},"form":[{"name":"anio_actual","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"anio_anterior","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mes_elegido","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"mes_elegido_consulta","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[],"events":[{"event":"si","targetPage":"01.ConocerIndicadoresReconoser","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.anio_actual","value":"$sys.func.FORMAT_DATE($sys.func.NOW(), yyyy)"},{"parameter":"$session.params.anio_anterior","value":"$sys.func.FORMAT_DATE($sys.func.ADD_DATE($sys.func.NOW(), -1, \"YEARS\"), \"yyyy\")"}],"generators":[]}},{"event":"no","targetPage":"02.NoConocerIndicadoresReconoser","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 05. Reconoser › 01.1. SolicitarMesIndicadores

**Al entrar, en este orden:**

1. 💬 Dice: «Por favor elige el mes a consultar»
2. 🔘 Ofrece los botones: «Enero» · «Febrero» · «Marzo» · «Abril» · «Mayo» · «Junio» · «Julio» · «Agosto» · «Septiembre» · «Más meses»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Enero» (`enero`) → página «01.2. Confirmar información de consulta»  
  ↳ 📌 asigna `mes_elegido` = `Enero`, `mes_elegido_consulta` = `1`
- «Febrero» (`febrero`) → página «01.2. Confirmar información de consulta»  
  ↳ 📌 asigna `mes_elegido` = `Febrero`, `mes_elegido_consulta` = `2`
- «Marzo» (`marzo`) → página «01.2. Confirmar información de consulta»  
  ↳ 📌 asigna `mes_elegido` = `Marzo`, `mes_elegido_consulta` = `3`
- «Abril» (`abril`) → página «01.2. Confirmar información de consulta»  
  ↳ 📌 asigna `mes_elegido` = `Abril`, `mes_elegido_consulta` = `4`
- «Mayo» (`mayo`) → página «01.2. Confirmar información de consulta»  
  ↳ 📌 asigna `mes_elegido` = `Mayo`, `mes_elegido_consulta` = `5`
- «Junio» (`junio`) → página «01.2. Confirmar información de consulta»  
  ↳ 📌 asigna `mes_elegido` = `Junio`, `mes_elegido_consulta` = `6`
- «Julio» (`julio`) → página «01.2. Confirmar información de consulta»  
  ↳ 📌 asigna `mes_elegido` = `Julio`, `mes_elegido_consulta` = `7`
- «Agosto» (`agosto`) → página «01.2. Confirmar información de consulta»  
  ↳ 📌 asigna `mes_elegido` = `Agosto`, `mes_elegido_consulta` = `8`
- «Septiembre» (`septiembre`) → página «01.2. Confirmar información de consulta»  
  ↳ 📌 asigna `mes_elegido` = `Septiembre`, `mes_elegido_consulta` = `9`
- «Más meses» (`mas_meses`) → página «01.1.1 Mas meses»

```json camila:page
{"flow":"05. Reconoser","page":{"name":"01.1. SolicitarMesIndicadores","entry":{"messages":[{"type":"text","texts":["Por favor elige el mes a consultar"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Enero","event":"enero"},{"text":"Febrero","event":"febrero"},{"text":"Marzo","event":"marzo"},{"text":"Abril","event":"abril"},{"text":"Mayo","event":"mayo"},{"text":"Junio","event":"junio"},{"text":"Julio","event":"julio"},{"text":"Agosto","event":"agosto"},{"text":"Septiembre","event":"septiembre"},{"text":"Más meses","event":"mas_meses"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"enero","targetPage":"01.2. Confirmar información de consulta","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.mes_elegido","value":"Enero"},{"parameter":"$session.params.mes_elegido_consulta","value":1}],"generators":[]}},{"event":"febrero","targetPage":"01.2. Confirmar información de consulta","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.mes_elegido","value":"Febrero"},{"parameter":"$session.params.mes_elegido_consulta","value":2}],"generators":[]}},{"event":"marzo","targetPage":"01.2. Confirmar información de consulta","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.mes_elegido","value":"Marzo"},{"parameter":"$session.params.mes_elegido_consulta","value":3}],"generators":[]}},{"event":"abril","targetPage":"01.2. Confirmar información de consulta","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.mes_elegido","value":"Abril"},{"parameter":"$session.params.mes_elegido_consulta","value":4}],"generators":[]}},{"event":"mayo","targetPage":"01.2. Confirmar información de consulta","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.mes_elegido","value":"Mayo"},{"parameter":"$session.params.mes_elegido_consulta","value":5}],"generators":[]}},{"event":"junio","targetPage":"01.2. Confirmar información de consulta","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.mes_elegido","value":"Junio"},{"parameter":"$session.params.mes_elegido_consulta","value":6}],"generators":[]}},{"event":"julio","targetPage":"01.2. Confirmar información de consulta","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.mes_elegido","value":"Julio"},{"parameter":"$session.params.mes_elegido_consulta","value":7}],"generators":[]}},{"event":"agosto","targetPage":"01.2. Confirmar información de consulta","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.mes_elegido","value":"Agosto"},{"parameter":"$session.params.mes_elegido_consulta","value":8}],"generators":[]}},{"event":"septiembre","targetPage":"01.2. Confirmar información de consulta","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.mes_elegido","value":"Septiembre"},{"parameter":"$session.params.mes_elegido_consulta","value":9}],"generators":[]}},{"event":"mas_meses","targetPage":"01.1.1 Mas meses","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 05. Reconoser › 01.1.1 Mas meses

**Al entrar, en este orden:**

1. 💬 Dice: «Por favor elige el mes a consultar»
2. 🔘 Ofrece los botones: «Octubre» · «Noviembre» · «Diciembre» · «Más meses»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Octubre» (`octubre`) → página «01.2. Confirmar información de consulta»  
  ↳ 📌 asigna `mes_elegido` = `Octubre`, `mes_elegido_consulta` = `10`
- «Noviembre» (`noviembre`) → página «01.2. Confirmar información de consulta»  
  ↳ 📌 asigna `mes_elegido` = `Noviembre`, `mes_elegido_consulta` = `11`
- «Diciembre» (`diciembre`) → página «01.2. Confirmar información de consulta»  
  ↳ 📌 asigna `mes_elegido` = `Diciembre`, `mes_elegido_consulta` = `12`
- «Más meses» (`mas_meses`) → página «01.1. SolicitarMesIndicadores»

```json camila:page
{"flow":"05. Reconoser","page":{"name":"01.1.1 Mas meses","entry":{"messages":[{"type":"text","texts":["Por favor elige el mes a consultar"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Octubre","event":"octubre"},{"text":"Noviembre","event":"noviembre"},{"text":"Diciembre","event":"diciembre"},{"text":"Más meses","event":"mas_meses"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"octubre","targetPage":"01.2. Confirmar información de consulta","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.mes_elegido","value":"Octubre"},{"parameter":"$session.params.mes_elegido_consulta","value":10}],"generators":[]}},{"event":"noviembre","targetPage":"01.2. Confirmar información de consulta","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.mes_elegido","value":"Noviembre"},{"parameter":"$session.params.mes_elegido_consulta","value":11}],"generators":[]}},{"event":"diciembre","targetPage":"01.2. Confirmar información de consulta","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.mes_elegido","value":"Diciembre"},{"parameter":"$session.params.mes_elegido_consulta","value":12}],"generators":[]}},{"event":"mas_meses","targetPage":"01.1. SolicitarMesIndicadores","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 05. Reconoser › 01.2. Confirmar información de consulta

**Al entrar, en este orden:**

1. 💬 Dice: «Los indicadores se consultaran para el siguiente mes: $session.params.mes_elegido del $session.params.anio_elegido»
2. 💬 Dice: «¿Es correcto?»
3. 🔘 Ofrece los botones: «Si» · «No»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `indicador1`, `resultado1`, `indicador2`, `resultado2`, `indicador3`, `resultado3`, `indicador4`, `resultado4`

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → página «01.3. Mostrar resultados Reconoser»  
  ↳ ⚡ Llama al webhook **Mis indicadores ReconoSER** [GetIndicadoresReconocer] — lectura
- «No» (`no`) → página «01.ConocerIndicadoresReconoser»  
  ↳ 📌 asigna `mes_elegido` = ``, `mes_elegido_consulta` = ``, `anio_elegido` = ``

```json camila:page
{"flow":"05. Reconoser","page":{"name":"01.2. Confirmar información de consulta","entry":{"messages":[{"type":"text","texts":["Los indicadores se consultaran para el siguiente mes:\n$session.params.mes_elegido del $session.params.anio_elegido"],"buttons":[]},{"type":"text","texts":["¿Es correcto?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si"},{"text":"No","event":"no"}]}],"setParams":[],"generators":[]},"form":[{"name":"indicador1","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"resultado1","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"indicador2","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"resultado2","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"indicador3","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"resultado3","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"indicador4","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"resultado4","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[],"events":[{"event":"si","targetPage":"01.3. Mostrar resultados Reconoser","fulfillment":{"messages":[],"setParams":[],"webhook":"Mis indicadores ReconoSER","tag":"GetIndicadoresReconocer","generators":[]}},{"event":"no","targetPage":"01.ConocerIndicadoresReconoser","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.mes_elegido"},{"parameter":"$session.params.mes_elegido_consulta"},{"parameter":"$session.params.anio_elegido"}],"generators":[]}}],"groups":[]}}
```

### Página · 05. Reconoser › 01.3. Mostrar resultados Reconoser

**Al entrar, en este orden:**

1. 💬 Dice: «Perfecto, estos son tus resultados:»
2. 💬 Dice: «✅ $session.params.indicador1 $session.params.resultado1 % ✅ $session.params.indicador2 $session.params.resultado2 % ✅ $session.params.indicador3 $session.params.resultado3 % ✅ $session.params.indicado…»

```json camila:page
{"flow":"05. Reconoser","page":{"name":"01.3. Mostrar resultados Reconoser","entry":{"messages":[{"type":"text","texts":["Perfecto, estos son tus resultados:"],"buttons":[]},{"type":"text","texts":["✅ $session.params.indicador1 $session.params.resultado1 %\n✅ $session.params.indicador2 $session.params.resultado2 %\n✅ $session.params.indicador3 $session.params.resultado3 %\n✅ $session.params.indicador4 $session.params.resultado4 %"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[],"groups":[]}}
```

### Página · 05. Reconoser › 01.ConocerIndicadoresReconoser

**Al entrar, en este orden:**

1. 💬 Dice: «Perfecto, por favor indicame el año que deseas consultar»
2. 🔘 Ofrece los botones: «$session.params.anio_actual» · «$session.params.anio_anterior»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `anio_elegido`

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «$session.params.anio_anterior» (`anio_anterior`) → página «01.1. SolicitarMesIndicadores»  
  ↳ 📌 asigna `anio_elegido` = `$session.params.anio_anterior`
- «$session.params.anio_actual» (`anio_actual`) → página «01.1. SolicitarMesIndicadores»  
  ↳ 📌 asigna `anio_elegido` = `$session.params.anio_actual`

```json camila:page
{"flow":"05. Reconoser","page":{"name":"01.ConocerIndicadoresReconoser","entry":{"messages":[{"type":"text","texts":["Perfecto, por favor indicame el año que deseas consultar"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"$session.params.anio_actual","event":"anio_actual"},{"text":"$session.params.anio_anterior","event":"anio_anterior"}]}],"setParams":[],"generators":[]},"form":[{"name":"anio_elegido","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[],"events":[{"event":"anio_anterior","targetPage":"01.1. SolicitarMesIndicadores","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.anio_elegido","value":"$session.params.anio_anterior"}],"generators":[]}},{"event":"anio_actual","targetPage":"01.1. SolicitarMesIndicadores","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.anio_elegido","value":"$session.params.anio_actual"}],"generators":[]}}],"groups":[]}}
```

### Página · 05. Reconoser › 02.NoConocerIndicadoresReconoser

**Al entrar, en este orden:**

1. 💬 Dice: «Entiendo, no te preocupes. Aunque recuerda que es importante conocer muy bien el programa y sus políticas para que puedas disfrutar de todos sus beneficios.»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → flujo **12. ¿Algo Mas?**

```json camila:page
{"flow":"05. Reconoser","page":{"name":"02.NoConocerIndicadoresReconoser","entry":{"messages":[{"type":"text","texts":["Entiendo, no te preocupes. Aunque recuerda que es\nimportante conocer muy bien el programa y sus\npolíticas para que puedas disfrutar de todos sus\nbeneficios."],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true = true","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

