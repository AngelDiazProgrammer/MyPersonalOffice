---
bot: camila
flow: 11. No Detecta
---
## Flujo · 11. No Detecta

**Qué dice al empezar:** «$session.params.mensajeGenerativoGlobal»

**Se llega desde:**
- 01. Indicadores De Calidad › 001. Menu_Indicadores ← evento `sys.no-match-default`
- 01. Indicadores De Calidad › 06. ¿Algo mas? ← evento `sys.no-match-default`

**Webhooks que usa:** ⚡ IA Generativa Camila - General

**Páginas:**
- 00. Irespondible — ⚡ IA Generativa Camila - General
- 01.Menu 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «00. Irespondible»  
  ↳ ⚡ Llama al webhook **IA Generativa Camila - General** [consultaWeb] — lectura
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"11. No Detecta","routes":[{"condition":"true","targetPage":"00. Irespondible","fulfillment":{"messages":[],"setParams":[],"webhook":"IA Generativa Camila - General","tag":"consultaWeb","generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 11. No Detecta › 00. Irespondible

**Al entrar, en este orden:**

1. 📌 Asigna: `consultaGenerativa` = `Basándote en esta frase y continuando con la conversación "R…`
2. ⚡ Llama al webhook **IA Generativa Camila - General** [consultaWeb] — lectura
3. 💬 Dice: «$session.params.mensajeGenerativoGlobal»

```json camila:page
{"flow":"11. No Detecta","page":{"name":"00. Irespondible","entry":{"messages":[{"type":"text","texts":["$session.params.mensajeGenerativoGlobal"],"buttons":[]}],"setParams":[{"parameter":"consultaGenerativa","value":"Basándote en esta frase y continuando con la conversación \"Recuerda que puedo ayudarte con las siguientes opciones: Calidad, Evaluación De Desempeño, Líder Misión, Referidos, Indicadores ReconoSer, Proceso De Selección, Actualización Datos, Te Escuchamos y Convenios \uD83D\uDE0A\", construye una respuesta amable indicando que no entendiste lo que el usuario quiso decir, invítalo a elegir una de las opciones disponibles, no dejes preguntas abiertas, y hazlo humano y amigable"}],"webhook":"IA Generativa Camila - General","tag":"consultaWeb","generators":[]},"form":[],"routes":[],"events":[],"groups":[]}}
```

### Página · 11. No Detecta › 01.Menu

**Al entrar, en este orden:**

_No hace nada al entrar._

```json camila:page
{"flow":"11. No Detecta","page":{"name":"01.Menu","entry":{"messages":[],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[],"groups":[]}}
```

