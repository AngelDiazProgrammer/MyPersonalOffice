---
bot: camila
flow: 14.1. Encuesta de Retiro
---
## Flujo · 14.1. Encuesta de Retiro

**Qué dice al empezar:** «$session.params.pregunta1EncuestaRetiro»

**Se llega desde:**
- flujo Default Start Flow ← evento `Encuesta de retiro`
- flujo Default Start Flow ← intent «Encuesta De Retiro»

**Webhooks que usa:** ✍️ Finalizar Conversacion · ✍️ Guardar Encuesta De Retiro

**Pide al usuario:** `respuesta` (1. Respuesta 1 Pregunta 1), `respuesta` (1. Respuesta 2 Pregunta 1), `respuesta` (1. Respuesta 3 Pregunta 1), `respuesta` (1. Respuesta 4 Pregunta 1), `respuesta` (Repuesta En Desacuerdo), `respuesta` (Respuesta Totalmente Desacuerdo)

**Páginas:**
- 0. Definicion de Preguntas 
- 1. Primera Pregunta 
- 1. Respuesta 1 Pregunta 1 — ⌨️ pide datos
- 1. Respuesta 2 Pregunta 1 — ⌨️ pide datos
- 1. Respuesta 3 Pregunta 1 — ⌨️ pide datos
- 1. Respuesta 4 Pregunta 1 — ⌨️ pide datos
- 2. Segunda pregunta 
- 3. Tercera Pregunta 
- 4. Cuarta Pregunta 
- 5. Quinta Pregunta 
- 6. Despedida — ✍️ Finalizar Conversacion
- Guardar Pregunta Y Respuesta — ✍️ Guardar Encuesta De Retiro
- Procesamiento de respuestas 
- Repuesta En Desacuerdo — ⌨️ pide datos
- Respuesta Totalmente Desacuerdo — ⌨️ pide datos

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0. Definicion de Preguntas»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"14.1. Encuesta de Retiro","routes":[{"condition":"true","targetPage":"0. Definicion de Preguntas","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 14.1. Encuesta de Retiro › 0. Definicion de Preguntas

**Al entrar, en este orden:**

1. 📌 Asigna: `intencion` = ``
2. 💬 Dice: «Cuéntame»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `controlGuardarEncuesta`, `preguntaEncuestaRetiro`, `pregunta1EncuestaRetiro`, `sub1Pregunta1EncuestaRetiro`, `sub2Pregunta1EncuestaRetiro`, `sub3Pregunta1EncuestaRetiro`, `sub4Pregunta1EncuestaRetiro`, `pregunta2EncuestaRetiro`, `pregunta3EncuestaRetiro`, `sub3Pregunta2EncuestaRetiro`, `sub4Pregunta2EncuestaRetiro`, `pregunta4EncuestaRetiro`, `pregunta5EncuestaRetiro`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «1. Primera Pregunta»

```json camila:page
{"flow":"14.1. Encuesta de Retiro","page":{"name":"0. Definicion de Preguntas","entry":{"messages":[{"type":"text","texts":["Cuéntame"],"buttons":[]}],"setParams":[{"parameter":"$session.params.intencion"}],"generators":[]},"form":[{"name":"controlGuardarEncuesta","entity":"@sys.number-integer","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"preguntaEncuestaRetiro","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"pregunta1EncuestaRetiro","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"sub1Pregunta1EncuestaRetiro","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"sub2Pregunta1EncuestaRetiro","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"sub3Pregunta1EncuestaRetiro","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"sub4Pregunta1EncuestaRetiro","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"pregunta2EncuestaRetiro","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"pregunta3EncuestaRetiro","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"sub3Pregunta2EncuestaRetiro","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"sub4Pregunta2EncuestaRetiro","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"pregunta4EncuestaRetiro","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}},{"name":"pregunta5EncuestaRetiro","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"true","targetPage":"1. Primera Pregunta","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 14.1. Encuesta de Retiro › 1. Primera Pregunta

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.pregunta1EncuestaRetiro»
2. 🔘 Ofrece los botones: «Mejor oferta laboral» · «Horarios de estudio» · «Capacitación Inconforme» · «Desacuerdo con líder»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Mejor oferta laboral» (`mejor_oferta`) → página «1. Respuesta 1 Pregunta 1»
- «Horarios de estudio» (`horarios_estudio`) → página «1. Respuesta 2 Pregunta 1»
- «Capacitación Inconforme» (`capacitacion_inconforme`) → página «1. Respuesta 3 Pregunta 1»
- «Desacuerdo con líder» (`desacuerdo_lider`) → página «1. Respuesta 4 Pregunta 1»

```json camila:page
{"flow":"14.1. Encuesta de Retiro","page":{"name":"1. Primera Pregunta","entry":{"messages":[{"type":"text","texts":["$session.params.pregunta1EncuestaRetiro"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Mejor oferta laboral","event":"mejor_oferta"},{"text":"Horarios de estudio","event":"horarios_estudio"},{"text":"Capacitación Inconforme","event":"capacitacion_inconforme"},{"text":"Desacuerdo con líder","event":"desacuerdo_lider"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"mejor_oferta","targetPage":"1. Respuesta 1 Pregunta 1","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"horarios_estudio","targetPage":"1. Respuesta 2 Pregunta 1","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"capacitacion_inconforme","targetPage":"1. Respuesta 3 Pregunta 1","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"desacuerdo_lider","targetPage":"1. Respuesta 4 Pregunta 1","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 14.1. Encuesta de Retiro › 1. Respuesta 1 Pregunta 1

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.sub1Pregunta1EncuestaRetiro»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `respuesta` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.respuesta != null` → página «Guardar Pregunta Y Respuesta»  
  ↳ 📌 asigna `preguntaEncuestaRetiro` = `$session.params.pregunta1EncuestaRetiro Mejor oferta laboral…`, `controlGuardarEncuesta` = `1`

```json camila:page
{"flow":"14.1. Encuesta de Retiro","page":{"name":"1. Respuesta 1 Pregunta 1","entry":{"messages":[{"type":"text","texts":["$session.params.sub1Pregunta1EncuestaRetiro"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"respuesta","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.respuesta != null","targetPage":"Guardar Pregunta Y Respuesta","fulfillment":{"messages":[],"setParams":[{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta1EncuestaRetiro Mejor oferta laboral: $session.params.sub1Pregunta1EncuestaRetiro"},{"parameter":"controlGuardarEncuesta","value":1}],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 14.1. Encuesta de Retiro › 1. Respuesta 2 Pregunta 1

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.sub2Pregunta1EncuestaRetiro»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `respuesta` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.respuesta != null` → página «Guardar Pregunta Y Respuesta»  
  ↳ 📌 asigna `preguntaEncuestaRetiro` = `$session.params.pregunta1EncuestaRetiro Horarios de estudio:…`, `controlGuardarEncuesta` = `1`

```json camila:page
{"flow":"14.1. Encuesta de Retiro","page":{"name":"1. Respuesta 2 Pregunta 1","entry":{"messages":[{"type":"text","texts":["$session.params.sub2Pregunta1EncuestaRetiro"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"respuesta","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.respuesta != null","targetPage":"Guardar Pregunta Y Respuesta","fulfillment":{"messages":[],"setParams":[{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta1EncuestaRetiro Horarios de estudio: $session.params.sub2Pregunta1EncuestaRetiro"},{"parameter":"controlGuardarEncuesta","value":1}],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 14.1. Encuesta de Retiro › 1. Respuesta 3 Pregunta 1

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.sub3Pregunta1EncuestaRetiro»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `respuesta` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.respuesta != null` → página «Guardar Pregunta Y Respuesta»  
  ↳ 📌 asigna `preguntaEncuestaRetiro` = `$session.params.pregunta1EncuestaRetiro Inconformidad Capaci…`, `controlGuardarEncuesta` = `1`

```json camila:page
{"flow":"14.1. Encuesta de Retiro","page":{"name":"1. Respuesta 3 Pregunta 1","entry":{"messages":[{"type":"text","texts":["$session.params.sub3Pregunta1EncuestaRetiro"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"respuesta","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.respuesta != null","targetPage":"Guardar Pregunta Y Respuesta","fulfillment":{"messages":[],"setParams":[{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta1EncuestaRetiro Inconformidad Capacitaciones: $session.params.sub3Pregunta1EncuestaRetiro"},{"parameter":"controlGuardarEncuesta","value":1}],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 14.1. Encuesta de Retiro › 1. Respuesta 4 Pregunta 1

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.sub4Pregunta1EncuestaRetiro»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `respuesta` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.respuesta != null` → página «Guardar Pregunta Y Respuesta»  
  ↳ 📌 asigna `preguntaEncuestaRetiro` = `$session.params.pregunta1EncuestaRetiro Insatisfacción Jefe:…`, `controlGuardarEncuesta` = `1`

```json camila:page
{"flow":"14.1. Encuesta de Retiro","page":{"name":"1. Respuesta 4 Pregunta 1","entry":{"messages":[{"type":"text","texts":["$session.params.sub4Pregunta1EncuestaRetiro"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"respuesta","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.respuesta != null","targetPage":"Guardar Pregunta Y Respuesta","fulfillment":{"messages":[],"setParams":[{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta1EncuestaRetiro Insatisfacción Jefe: $session.params.sub4Pregunta1EncuestaRetiro"},{"parameter":"controlGuardarEncuesta","value":1}],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 14.1. Encuesta de Retiro › 2. Segunda pregunta

**Al entrar, en este orden:**

1. 📌 Asigna: `respuesta` = ``
2. 💬 Dice: «$session.params.pregunta2EncuestaRetiro»
3. 🔘 Ofrece los botones: «Totalmente de acuerdo» · «De acuerdo» · «En desacuerdo» · «Totalmente en desacuerdo»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `respuesta`

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Totalmente de acuerdo» (`totalmente_de_acuerdo`) → página «Procesamiento de respuestas»  
  ↳ 📌 asigna `respuesta` = `Totalmente de acuerdo`, `preguntaEncuestaRetiro` = `$session.params.pregunta2EncuestaRetiro`, `controlGuardarEncuesta` = `2`
- «De acuerdo» (`de_acuerdo`) → página «Procesamiento de respuestas»  
  ↳ 📌 asigna `respuesta` = `De acuerdo`, `preguntaEncuestaRetiro` = `$session.params.pregunta2EncuestaRetiro`, `controlGuardarEncuesta` = `2`
- «En desacuerdo» (`en_desacuerdo`) → página «Repuesta En Desacuerdo»  
  ↳ 📌 asigna `controlGuardarEncuesta` = `2`, `preguntaEncuestaRetiro` = `$session.params.pregunta2EncuestaRetiro En Desacuerdo: sub3P…`
- «Totalmente en desacuerdo» (`totalmente_en_desacuerdo`) → página «Respuesta Totalmente Desacuerdo»  
  ↳ 📌 asigna `controlGuardarEncuesta` = `2`, `preguntaEncuestaRetiro` = `$session.params.pregunta2EncuestaRetiro Totalmente En Desacu…`

```json camila:page
{"flow":"14.1. Encuesta de Retiro","page":{"name":"2. Segunda pregunta","entry":{"messages":[{"type":"text","texts":["$session.params.pregunta2EncuestaRetiro"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Totalmente de acuerdo","event":"totalmente_de_acuerdo"},{"text":"De acuerdo","event":"de_acuerdo"},{"text":"En desacuerdo","event":"en_desacuerdo"},{"text":"Totalmente en desacuerdo","event":"totalmente_en_desacuerdo"}]}],"setParams":[{"parameter":"respuesta"}],"generators":[]},"form":[{"name":"respuesta","entity":"@sys.any","required":false,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[],"events":[{"event":"totalmente_de_acuerdo","targetPage":"Procesamiento de respuestas","fulfillment":{"messages":[],"setParams":[{"parameter":"respuesta","value":"Totalmente de acuerdo"},{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta2EncuestaRetiro"},{"parameter":"controlGuardarEncuesta","value":2}],"generators":[]}},{"event":"de_acuerdo","targetPage":"Procesamiento de respuestas","fulfillment":{"messages":[],"setParams":[{"parameter":"respuesta","value":"De acuerdo"},{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta2EncuestaRetiro"},{"parameter":"controlGuardarEncuesta","value":2}],"generators":[]}},{"event":"en_desacuerdo","targetPage":"Repuesta En Desacuerdo","fulfillment":{"messages":[],"setParams":[{"parameter":"controlGuardarEncuesta","value":2},{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta2EncuestaRetiro En Desacuerdo: sub3Pregunta2EncuestaRetiro"}],"generators":[]}},{"event":"totalmente_en_desacuerdo","targetPage":"Respuesta Totalmente Desacuerdo","fulfillment":{"messages":[],"setParams":[{"parameter":"controlGuardarEncuesta","value":2},{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta2EncuestaRetiro Totalmente En Desacuerdo: sub4Pregunta2EncuestaRetiro"}],"generators":[]}}],"groups":[]}}
```

### Página · 14.1. Encuesta de Retiro › 3. Tercera Pregunta

**Al entrar, en este orden:**

1. 📌 Asigna: `respuesta` = ``
2. 💬 Dice: «$session.params.pregunta3EncuestaRetiro»
3. 🔘 Ofrece los botones: «Totalmente de acuerdo» · «De acuerdo» · «En desacuerdo» · «Totalmente en desacuerdo»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Totalmente de acuerdo» (`totalmente_de_acuerdo`) → página «Procesamiento de respuestas»  
  ↳ 📌 asigna `respuesta` = `Totalmente de acuerdo`, `preguntaEncuestaRetiro` = `$session.params.pregunta3EncuestaRetiro`, `controlGuardarEncuesta` = `3`
- «De acuerdo» (`de_acuerdo`) → página «Procesamiento de respuestas»  
  ↳ 📌 asigna `respuesta` = `De acuerdo`, `preguntaEncuestaRetiro` = `$session.params.pregunta3EncuestaRetiro`, `controlGuardarEncuesta` = `3`
- «En desacuerdo» (`en_desacuerdo`) → página «Repuesta En Desacuerdo»  
  ↳ 📌 asigna `controlGuardarEncuesta` = `3`, `preguntaEncuestaRetiro` = `$session.params.pregunta3EncuestaRetiro En Desacuerdo: sub3P…`
- «Totalmente en desacuerdo» (`totalmente_en_desacuerdo`) → página «Respuesta Totalmente Desacuerdo»  
  ↳ 📌 asigna `controlGuardarEncuesta` = `3`, `preguntaEncuestaRetiro` = `$session.params.pregunta3EncuestaRetiro En Desacuerdo: sub4P…`

```json camila:page
{"flow":"14.1. Encuesta de Retiro","page":{"name":"3. Tercera Pregunta","entry":{"messages":[{"type":"text","texts":["$session.params.pregunta3EncuestaRetiro"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Totalmente de acuerdo","event":"totalmente_de_acuerdo"},{"text":"De acuerdo","event":"de_acuerdo"},{"text":"En desacuerdo","event":"en_desacuerdo"},{"text":"Totalmente en desacuerdo","event":"totalmente_en_desacuerdo"}]}],"setParams":[{"parameter":"respuesta"}],"generators":[]},"form":[],"routes":[],"events":[{"event":"totalmente_de_acuerdo","targetPage":"Procesamiento de respuestas","fulfillment":{"messages":[],"setParams":[{"parameter":"respuesta","value":"Totalmente de acuerdo"},{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta3EncuestaRetiro"},{"parameter":"controlGuardarEncuesta","value":3}],"generators":[]}},{"event":"de_acuerdo","targetPage":"Procesamiento de respuestas","fulfillment":{"messages":[],"setParams":[{"parameter":"respuesta","value":"De acuerdo"},{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta3EncuestaRetiro"},{"parameter":"controlGuardarEncuesta","value":3}],"generators":[]}},{"event":"en_desacuerdo","targetPage":"Repuesta En Desacuerdo","fulfillment":{"messages":[],"setParams":[{"parameter":"controlGuardarEncuesta","value":3},{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta3EncuestaRetiro En Desacuerdo: sub3Pregunta2EncuestaRetiro"}],"generators":[]}},{"event":"totalmente_en_desacuerdo","targetPage":"Respuesta Totalmente Desacuerdo","fulfillment":{"messages":[],"setParams":[{"parameter":"controlGuardarEncuesta","value":3},{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta3EncuestaRetiro En Desacuerdo: sub4Pregunta2EncuestaRetiro"}],"generators":[]}}],"groups":[]}}
```

### Página · 14.1. Encuesta de Retiro › 4. Cuarta Pregunta

**Al entrar, en este orden:**

1. 📌 Asigna: `respuesta` = ``
2. 💬 Dice: «$session.params.pregunta4EncuestaRetiro»
3. 🔘 Ofrece los botones: «Totalmente de acuerdo» · «De acuerdo» · «En desacuerdo» · «Totalmente en desacuerdo»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Totalmente de acuerdo» (`totalmente_de_acuerdo`) → página «Procesamiento de respuestas»  
  ↳ 📌 asigna `respuesta` = `Totalmente de acuerdo`, `preguntaEncuestaRetiro` = `$session.params.pregunta4EncuestaRetiro`, `controlGuardarEncuesta` = `4`
- «De acuerdo» (`de_acuerdo`) → página «Procesamiento de respuestas»  
  ↳ 📌 asigna `respuesta` = `De acuerdo`, `preguntaEncuestaRetiro` = `$session.params.preguntaEncuestaRetiro`, `controlGuardarEncuesta` = `4`
- «En desacuerdo» (`en_desacuerdo`) → página «Repuesta En Desacuerdo»  
  ↳ 📌 asigna `controlGuardarEncuesta` = `4`, `preguntaEncuestaRetiro` = `$session.params.pregunta4EncuestaRetiro En Desacuerdo: $sess…`
- «Totalmente en desacuerdo» (`totalmente_en_desacuerdo`) → página «Respuesta Totalmente Desacuerdo»  
  ↳ 📌 asigna `controlGuardarEncuesta` = `4`, `preguntaEncuestaRetiro` = `$session.params.pregunta4EncuestaRetiro En Desacuerdo: $sess…`

```json camila:page
{"flow":"14.1. Encuesta de Retiro","page":{"name":"4. Cuarta Pregunta","entry":{"messages":[{"type":"text","texts":["$session.params.pregunta4EncuestaRetiro"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Totalmente de acuerdo","event":"totalmente_de_acuerdo"},{"text":"De acuerdo","event":"de_acuerdo"},{"text":"En desacuerdo","event":"en_desacuerdo"},{"text":"Totalmente en desacuerdo","event":"totalmente_en_desacuerdo"}]}],"setParams":[{"parameter":"respuesta"}],"generators":[]},"form":[],"routes":[],"events":[{"event":"totalmente_de_acuerdo","targetPage":"Procesamiento de respuestas","fulfillment":{"messages":[],"setParams":[{"parameter":"respuesta","value":"Totalmente de acuerdo"},{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta4EncuestaRetiro"},{"parameter":"controlGuardarEncuesta","value":4}],"generators":[]}},{"event":"de_acuerdo","targetPage":"Procesamiento de respuestas","fulfillment":{"messages":[],"setParams":[{"parameter":"respuesta","value":"De acuerdo"},{"parameter":"preguntaEncuestaRetiro","value":"$session.params.preguntaEncuestaRetiro"},{"parameter":"controlGuardarEncuesta","value":4}],"generators":[]}},{"event":"en_desacuerdo","targetPage":"Repuesta En Desacuerdo","fulfillment":{"messages":[],"setParams":[{"parameter":"controlGuardarEncuesta","value":4},{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta4EncuestaRetiro En Desacuerdo: $session.params.sub3Pregunta2EncuestaRetiro"}],"generators":[]}},{"event":"totalmente_en_desacuerdo","targetPage":"Respuesta Totalmente Desacuerdo","fulfillment":{"messages":[],"setParams":[{"parameter":"controlGuardarEncuesta","value":4},{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta4EncuestaRetiro En Desacuerdo: $session.params.sub4Pregunta2EncuestaRetiro"}],"generators":[]}}],"groups":[]}}
```

### Página · 14.1. Encuesta de Retiro › 5. Quinta Pregunta

**Al entrar, en este orden:**

1. 📌 Asigna: `respuesta` = ``
2. 💬 Dice: «$session.params.pregunta5EncuestaRetiro»
3. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → página «Guardar Pregunta Y Respuesta»  
  ↳ 📌 asigna `respuesta` = `Si`, `preguntaEncuestaRetiro` = `$session.params.pregunta5EncuestaRetiro`, `controlGuardarEncuesta` = `5`
- «No» (`no`) → página «Guardar Pregunta Y Respuesta»  
  ↳ 📌 asigna `respuesta` = `No`, `preguntaEncuestaRetiro` = `$session.params.pregunta5EncuestaRetiro`, `controlGuardarEncuesta` = `5`

```json camila:page
{"flow":"14.1. Encuesta de Retiro","page":{"name":"5. Quinta Pregunta","entry":{"messages":[{"type":"text","texts":["$session.params.pregunta5EncuestaRetiro"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"Si","event":"si"},{"text":"No","event":"no"}]}],"setParams":[{"parameter":"respuesta"}],"generators":[]},"form":[],"routes":[],"events":[{"event":"si","targetPage":"Guardar Pregunta Y Respuesta","fulfillment":{"messages":[],"setParams":[{"parameter":"respuesta","value":"Si"},{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta5EncuestaRetiro"},{"parameter":"controlGuardarEncuesta","value":5}],"generators":[]}},{"event":"no","targetPage":"Guardar Pregunta Y Respuesta","fulfillment":{"messages":[],"setParams":[{"parameter":"respuesta","value":"No"},{"parameter":"preguntaEncuestaRetiro","value":"$session.params.pregunta5EncuestaRetiro"},{"parameter":"controlGuardarEncuesta","value":5}],"generators":[]}}],"groups":[]}}
```

### Página · 14.1. Encuesta de Retiro › 6. Despedida

**Al entrar, en este orden:**

1. 💬 Dice: «¡Gracias por tu tiempo y sinceridad! Outsourcing S.A.S. BIC te desea muchos éxitos en tu camino profesional y recuerda que siempre serás parte de nuestra historia. ¡Hasta pronto!»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

```json camila:page
{"flow":"14.1. Encuesta de Retiro","page":{"name":"6. Despedida","entry":{"messages":[{"type":"text","texts":["¡Gracias por tu tiempo y sinceridad! Outsourcing S.A.S. BIC te desea muchos éxitos en tu camino profesional y recuerda que siempre serás parte de nuestra historia. ¡Hasta pronto!"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"End Session","fulfillment":{"messages":[],"setParams":[],"webhook":"Finalizar Conversacion","tag":"EndConversationWithPersonalizedMessage","generators":[]}}],"events":[],"groups":[]}}
```

### Página · 14.1. Encuesta de Retiro › Guardar Pregunta Y Respuesta

**Al entrar, en este orden:**

1. ✍️ Llama al webhook **Guardar Encuesta De Retiro** [InsertarEncuestaRetiro] — **escritura, pide confirmación**

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.controlGuardarEncuesta = 1` → página «2. Segunda pregunta»
- si `$session.params.controlGuardarEncuesta = 2` → página «3. Tercera Pregunta»
- si `$session.params.controlGuardarEncuesta = 3` → página «4. Cuarta Pregunta»
- si `$session.params.controlGuardarEncuesta = 4` → página «5. Quinta Pregunta»
- si `$session.params.controlGuardarEncuesta = 5` → página «6. Despedida»

```json camila:page
{"flow":"14.1. Encuesta de Retiro","page":{"name":"Guardar Pregunta Y Respuesta","entry":{"messages":[],"setParams":[],"webhook":"Guardar Encuesta De Retiro","tag":"InsertarEncuestaRetiro","generators":[]},"form":[],"routes":[{"condition":"$session.params.controlGuardarEncuesta = 1","targetPage":"2. Segunda pregunta","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.controlGuardarEncuesta = 2","targetPage":"3. Tercera Pregunta","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.controlGuardarEncuesta = 3","targetPage":"4. Cuarta Pregunta","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.controlGuardarEncuesta = 4","targetPage":"5. Quinta Pregunta","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.controlGuardarEncuesta = 5","targetPage":"6. Despedida","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 14.1. Encuesta de Retiro › Procesamiento de respuestas

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «Guardar Pregunta Y Respuesta»

```json camila:page
{"flow":"14.1. Encuesta de Retiro","page":{"name":"Procesamiento de respuestas","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetPage":"Guardar Pregunta Y Respuesta","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 14.1. Encuesta de Retiro › Repuesta En Desacuerdo

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.sub3Pregunta2EncuestaRetiro»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `respuesta` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.respuesta != null` → página «Procesamiento de respuestas»

```json camila:page
{"flow":"14.1. Encuesta de Retiro","page":{"name":"Repuesta En Desacuerdo","entry":{"messages":[{"type":"text","texts":["$session.params.sub3Pregunta2EncuestaRetiro"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"respuesta","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.respuesta != null","targetPage":"Procesamiento de respuestas","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 14.1. Encuesta de Retiro › Respuesta Totalmente Desacuerdo

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.sub4Pregunta2EncuestaRetiro»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `respuesta` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.respuesta != null` → página «Procesamiento de respuestas»

```json camila:page
{"flow":"14.1. Encuesta de Retiro","page":{"name":"Respuesta Totalmente Desacuerdo","entry":{"messages":[{"type":"text","texts":["$session.params.sub4Pregunta2EncuestaRetiro"],"buttons":[]}],"setParams":[],"generators":[]},"form":[{"name":"respuesta","entity":"@sys.any","required":true,"prompt":{"messages":[],"setParams":[],"generators":[]}}],"routes":[{"condition":"$session.params.respuesta != null","targetPage":"Procesamiento de respuestas","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

