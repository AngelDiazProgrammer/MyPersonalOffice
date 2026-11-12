---
bot: camila
flow: 16.5 Cesantias
---
## Flujo · 16.5 Cesantias

**Qué dice al empezar:** «¿En que puedo ayudarte?»

**Se llega desde:**
- 16. Administración de personal › 1. ¿En que te puedo ayudar? ← evento `cesantias`

**Páginas:**
- 0. ¿En que puedo ayudarte? 
- 1. Estudio 
- 2. Compra vivienda 
- 3. Credito hipotecario 
- 4. Arreglos locativos 
- 5. Impuesto predial 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0. ¿En que puedo ayudarte?»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

```json camila:flow
{"name":"16.5 Cesantias","routes":[{"condition":"true","targetPage":"0. ¿En que puedo ayudarte?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[{"event":"sys.no-match-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}},{"event":"sys.no-input-default","fulfillment":{"messages":[{"type":"text","texts":["Ups, no he entendido a que te refieres.","¿Podrías repetirlo, por favor?","¿Disculpa?","¿Decías?","¿Cómo?"],"buttons":[]}],"setParams":[],"generators":[]}}],"groups":[]}
```

### Página · 16.5 Cesantias › 0. ¿En que puedo ayudarte?

**Al entrar, en este orden:**

1. 💬 Dice: «¿En que puedo ayudarte?»
2. 🔘 Ofrece los botones: «📚 1. Estudio» · «🏠 2. Vivienda» · «🏡 3. Hipotecario» · «🔧 4. Arreglos hogar» · «📄 5. Imp. predial»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «📚 1. Estudio» (`estudio`) → página «1. Estudio»
- «🏠 2. Vivienda» (`compraVivienda`) → página «2. Compra vivienda»
- «🏡 3. Hipotecario» (`creditoHipotecario`) → página «3. Credito hipotecario»
- «🔧 4. Arreglos hogar» (`arreglosLocativos`) → página «4. Arreglos locativos»
- «📄 5. Imp. predial» (`impuestoPredial`) → página «5. Impuesto predial»

```json camila:page
{"flow":"16.5 Cesantias","page":{"name":"0. ¿En que puedo ayudarte?","entry":{"messages":[{"type":"text","texts":["¿En que puedo ayudarte?"],"buttons":[]},{"type":"buttons","texts":[],"buttons":[{"text":"\uD83D\uDCDA 1. Estudio","event":"estudio"},{"text":"\uD83C\uDFE0 2. Vivienda","event":"compraVivienda"},{"text":"\uD83C\uDFE1 3. Hipotecario","event":"creditoHipotecario"},{"text":"\uD83D\uDD27 4. Arreglos hogar","event":"arreglosLocativos"},{"text":"\uD83D\uDCC4 5. Imp. predial","event":"impuestoPredial"}]}],"setParams":[],"generators":[]},"form":[],"routes":[],"events":[{"event":"estudio","targetPage":"1. Estudio","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"compraVivienda","targetPage":"2. Compra vivienda","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"creditoHipotecario","targetPage":"3. Credito hipotecario","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"arreglosLocativos","targetPage":"4. Arreglos locativos","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"event":"impuestoPredial","targetPage":"5. Impuesto predial","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"groups":[]}}
```

### Página · 16.5 Cesantias › 1. Estudio

**Al entrar, en este orden:**

1. 💬 Dice: «Debes acercarte a tu fondo de cesantías y realizar directamente allí la solicitud de retiro por concepto de educación. Para ello, es posible que te soliciten documentos soporte, como la certificación …»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

```json camila:page
{"flow":"16.5 Cesantias","page":{"name":"1. Estudio","entry":{"messages":[{"type":"text","texts":["Debes acercarte a tu fondo de cesantías y realizar directamente allí la solicitud de retiro por concepto de educación. Para ello, es posible que te soliciten documentos soporte, como la certificación de matrícula, orden de pago o cualquier otro requisito definido por el fondo.\n\nTe recomendamos validar previamente los requisitos y canales de atención disponibles para agilizar el proceso y garantizar que la solicitud sea tramitada correctamente."],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 16.5 Cesantias › 2. Compra vivienda

**Al entrar, en este orden:**

1. 💬 Dice: «Debes realizar el siguiente proceso:  👇  ✅ Carta de solicitud dirigida a OUTSOURCING S.A.S BIC ✅ Promesa de compra y venta firmada por ambas partes ✅ Certificado del saldo de cesantías en el Fondo ✅ …»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

```json camila:page
{"flow":"16.5 Cesantias","page":{"name":"2. Compra vivienda","entry":{"messages":[{"type":"text","texts":["Debes realizar el siguiente proceso:  \uD83D\uDC47\n\n✅ Carta de solicitud dirigida a OUTSOURCING S.A.S BIC\n✅ Promesa de compra y venta firmada por ambas partes\n✅ Certificado del saldo de cesantías en el Fondo\n✅ Copia de la cédula al 150%\n✅ Envía los documentos al correo de auxnomina1@outsourcing.com.co \uD83D\uDCE7"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 16.5 Cesantias › 3. Credito hipotecario

**Al entrar, en este orden:**

1. 💬 Dice: «Debes realizar el siguiente proceso:  👇  ✅Carta de solicitud dirigida a OUTSOURCING S.A.S BIC ✅ Promesa de compra y venta firmada por ambas partes ✅ Certificado del saldo de cesantías en el Fondo. ✅ …»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

```json camila:page
{"flow":"16.5 Cesantias","page":{"name":"3. Credito hipotecario","entry":{"messages":[{"type":"text","texts":["Debes realizar el siguiente proceso:  \uD83D\uDC47\n\n✅Carta de solicitud dirigida a OUTSOURCING S.A.S BIC\n✅ Promesa de compra y venta firmada por ambas partes\n✅ Certificado del saldo de cesantías en el Fondo.\n✅ Copia de la cédula al 150%\n✅ Certificado del banco no mayor a 30 días con el saldo del crédito.\n✅ Envía los documentos al correo  auxnomina1@outsourcing.com.co \uD83D\uDCE7"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 16.5 Cesantias › 4. Arreglos locativos

**Al entrar, en este orden:**

1. 💬 Dice: «Debes realizar el siguiente proceso:  👇  ✅ Carta de solicitud dirigida a Outsourcing S.A.S. BIC ✅ Certificado de tradición y Libertad no mayor a 30 días de vigencia ✅ Certificado del saldo de cesantí…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

```json camila:page
{"flow":"16.5 Cesantias","page":{"name":"4. Arreglos locativos","entry":{"messages":[{"type":"text","texts":["Debes realizar el siguiente proceso:  \uD83D\uDC47\n\n✅ Carta de solicitud dirigida a Outsourcing S.A.S. BIC\n✅ Certificado de tradición y Libertad no mayor a 30 días de vigencia\n✅ Certificado del saldo de cesantías en el Fondo.\n✅ Copia de la cédula al 150%\n✅ Cotización de Materiales y mano de obra\n✅ Si el inmueble pertenece a tu cónyuge, adjuntar registro civil de matrimonio o declaración juramentada.\n✅ Envía los documentos al correo auxnomina1@outsourcing.com.co \uD83D\uDCE7"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

### Página · 16.5 Cesantias › 5. Impuesto predial

**Al entrar, en este orden:**

1. 💬 Dice: «Debes realizar el siguiente proceso:  👇  ✅Carta de solicitud dirigida a Outsourcing S.A.S. BIC ✅Certificado de tradición y Libertar no mayor 30 días de vigencia ✅ Certificado de saldo de cesantías en…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

```json camila:page
{"flow":"16.5 Cesantias","page":{"name":"5. Impuesto predial","entry":{"messages":[{"type":"text","texts":["Debes realizar el siguiente proceso:  \uD83D\uDC47\n\n✅Carta de solicitud dirigida a Outsourcing S.A.S. BIC\n✅Certificado de tradición y Libertar no mayor 30 días de vigencia\n✅ Certificado de saldo de cesantías en Fondo.\n✅ Copia de la cédula al 150%\n✅ Cotización de Materiales y mano de obra\n✅ Certificado del impuesto\n✅ Si el inmueble pertenece a tu cónyuge, adjunta el registro civil de matrimonio o la declaración juramentada.\n✅ Envía los documentos al correo de auxnomina1@outsourcing.com.co \uD83D\uDCE7"],"buttons":[]}],"setParams":[],"generators":[]},"form":[],"routes":[{"condition":"true","targetFlow":"12. ¿Algo Mas?","fulfillment":{"messages":[],"setParams":[],"generators":[]}}],"events":[],"groups":[]}}
```

