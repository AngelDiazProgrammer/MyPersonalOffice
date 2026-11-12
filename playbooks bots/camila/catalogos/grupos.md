---
bot: camila
catalog: groups
---
## Grupos de rutas (atajos)

Rutas compartidas por varias páginas o flujos (por ejemplo «volver al menú principal»).

### Grupo · Atajos (global)

- intent «Menu_Principal» → Default Start Flow

```json camila:group
{"name":"Atajos","routes":[{"intent":"Menu_Principal","targetFlow":"Default Start Flow","fulfillment":{"messages":[],"setParams":[],"generators":[]}}]}
```

### Grupo · Atajos_Flow (global)

- intent «Menu_Principal» → Default Start Flow

```json camila:group
{"name":"Atajos_Flow","routes":[{"intent":"Menu_Principal","condition":"$session.params.menu = true","targetFlow":"Default Start Flow","fulfillment":{"messages":[],"setParams":[],"generators":[]}}]}
```

### Grupo · Atajos_Flow ( (global)

- intent «Menu_Principal» → Default Start Flow

```json camila:group
{"name":"Atajos_Flow (","routes":[{"intent":"Menu_Principal","targetFlow":"Default Start Flow","fulfillment":{"messages":[],"setParams":[],"generators":[]}}]}
```

### Grupo · Atajos_Flow (copy) (global)

- intent «route_Calidad» → 01. Indicadores De Calidad
- intent «route_EvaluacionDeDesemepeño» → 02. Mi Evaluación De Desempeño
- intent «route_LiderMision» → 03. Líder Misión
- intent «route_Referidos» → 04. Referidos
- intent «route_Reconoser» → 05. Reconoser
- intent «route_ProcesoDeSeleccion» → 06. Mi proceso De Selección
- intent «route_ActualizarDatosDatix» → 07. Actualizacion de datos Datix
- intent «route_TeEscuchamos» → 08.Te escuchamos
- intent «route_Convenios» → 09.Convenios

```json camila:group
{"name":"Atajos_Flow (copy)","routes":[{"intent":"route_Calidad","targetFlow":"01. Indicadores De Calidad","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.indicadores","value":true}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"intent":"route_EvaluacionDeDesemepeño","targetFlow":"02. Mi Evaluación De Desempeño","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.desempeno","value":true},{"parameter":"consultaGenerativa","value":"Basándote en esta frase y continuando con la conversación \"Aquí podrás recordar tus resultados de desempeño, reconocer lo que lograste y definir lo que fortalecerás para seguir creciendo en Outsourcing \uD83D\uDCAA ¿Te gustaría revisar y recordar tus resultados de valoración de desempeño?\", construye una respuesta, no dejes preguntas abiertas, indica al usuario que puede responder Si o No, y hazlo humano y amigable"}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"intent":"route_LiderMision","targetFlow":"03. Líder Misión","fulfillment":{"messages":[],"setParams":[{"parameter":"lider_mision","value":true},{"parameter":"$session.params.liderMision","value":true},{"parameter":"consultaGenerativa","value":"Basándote en esta frase y continuando con la conversación \"Aquí podrás recordar tus resultados de Líder Misión, reconocer lo que lograste y definir lo que fortalecerás para ser un líder inspirador, ganador y estratega. ¿Te gustaría revisar y recordar tus resultados? \uD83D\uDCAA\", construye una respuesta, no dejes preguntas abiertas, indica al usuario que puede responder Si o No, y hazlo humano y amigable"}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"intent":"route_Referidos","targetFlow":"04. Referidos","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.referidos","value":true},{"parameter":"consultaGenerativa","value":"Basándote en esta frase y continuando con la conversación \"Recuerda que como colaborador de la Familia OS puedes referir a un familiar, amigo o conocido para hacer parte de nuestra compañía. Ten en cuenta que dos o más familiares no pueden trabajar en la misma cuenta sin importar grado de consanguinidad, por conflicto de intereses. Refiere aquí: https://frontos.outsourcing.com.co:8600/ \uD83C\uDF89\", construye una respuesta, no dejes preguntas abiertas, ofrece la opción Menú Principal, y hazlo humano y amigable y por favor no olvides adjuntar el link en la respuesta"}],"generators":[]}},{"intent":"route_Reconoser","targetFlow":"05. Reconoser","fulfillment":{"messages":[],"setParams":[{"parameter":"reconoser","value":true}],"webhook":"Login_API_Reconoser","tag":"Authentication","generators":[]}},{"intent":"route_ProcesoDeSeleccion","targetFlow":"06. Mi proceso De Selección","fulfillment":{"messages":[],"setParams":[{"parameter":"seleccion","value":true}],"generators":[]}},{"intent":"route_ActualizarDatosDatix","targetFlow":"07. Actualizacion de datos Datix","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.actualizarDatosDatix","value":true},{"parameter":"consultaGenerativa","value":"Basándote EXACTAMENTE en esta frase sin omitir ninguna parte: \"Datix es nuestra plataforma de registro y actualización de datos diseñada para mejorar tu experiencia en OS Construye una respuesta humana y amigable que explique brevemente qué es Datix, y pregunta si les gustaria revisar o actualizar sus datos personales como pregunta final."}],"generators":[]}},{"intent":"route_TeEscuchamos","targetFlow":"08.Te escuchamos","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.teEscuchamos","value":true},{"parameter":"consultaGenerativa","value":"Basándote en esta frase y continuando con la conversación \"Es un espacio para escuchar tu voz y contribuir a nuestro propósito de Mejorar Vidas \uD83D\uDDE3️ ¿Quieres dejarnos algún comentario u opinión? \uD83D\uDE00\", construye una respuesta, no dejes preguntas abiertas, indica al usuario que puede responder Si o No, y hazlo humano y amigable"}],"generators":[]}},{"intent":"route_Convenios","targetFlow":"09.Convenios","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.convenios","value":true},{"parameter":"consultaGenerativa","value":"Basándote en esta frase y continuando con la conversación \"Los convenios son alianzas estratégicas que realizamos para ampliar el portafolio de beneficios de la Familia OS \uD83D\uDE03\", construye una respuesta informativa, no dejes preguntas abiertas, indica al usuario que puede responder Si o No, y hazlo humano y amigable"}],"generators":[]}}]}
```

### Grupo · Atajos_Flow_Global (global)

- intent «route_EvaluacionDeDesemepeño» → 02. Mi Evaluación De Desempeño
- intent «route_LiderMision» → 03. Líder Misión
- intent «route_Referidos» → 04. Referidos
- intent «route_Reconoser» → 05. Reconoser
- intent «route_ProcesoDeSeleccion» → 06. Mi proceso De Selección
- intent «route_ActualizarDatosDatix» → 07. Actualizacion de datos Datix
- intent «route_TeEscuchamos» → 08.Te escuchamos
- intent «route_Convenios» → 09.Convenios
- intent «routeCalidad_MatrizDeCalidad» → 01. Indicadores De Calidad
- intent «routeCalidad_OportunidadesYFortalezas» → 01. Indicadores De Calidad
- intent «routeCalidad_CerrarMonitoreosPendientes» → 01. Indicadores De Calidad
- intent «routeCalidad_AtributosEmbajador» → 01. Indicadores De Calidad
- intent «routeCalidad_IndicadoresDeCalidad» → 01. Indicadores De Calidad
- intent «route_Calidad» → 01. Indicadores De Calidad
- intent «Despedida» → 10. Cierre de conversación

```json camila:group
{"name":"Atajos_Flow_Global","routes":[{"intent":"route_EvaluacionDeDesemepeño","targetFlow":"02. Mi Evaluación De Desempeño","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.desempeno","value":true}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"intent":"route_LiderMision","targetFlow":"03. Líder Misión","fulfillment":{"messages":[],"setParams":[{"parameter":"lider_mision","value":true},{"parameter":"$session.params.liderMision","value":true},{"parameter":"consultaGenerativa","value":"Basándote en esta frase y continuando con la conversación \"Aquí podrás recordar tus resultados de Líder Misión, reconocer lo que lograste y definir lo que fortalecerás para ser un líder inspirador, ganador y estratega. ¿Te gustaría revisar y recordar tus resultados? \uD83D\uDCAA\", construye una respuesta, no dejes preguntas abiertas, indica al usuario que puede responder Si o No, y hazlo humano y amigable y obviamente no menciones nada de lo que te estoy diciendo"}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"intent":"route_Referidos","targetFlow":"04. Referidos","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.referidos","value":true},{"parameter":"consultaGenerativa","value":"Basándote en esta frase y continuando con la conversación \"Recuerda que como colaborador de la Familia OS puedes referir a un familiar, amigo o conocido para hacer parte de nuestra compañía. Ten en cuenta que dos o más familiares no pueden trabajar en la misma cuenta sin importar grado de consanguinidad, por conflicto de intereses. Refiere aquí: https://frontos.outsourcing.com.co:8600/ \uD83C\uDF89\", construye una respuesta, no dejes preguntas abiertas, ofrece la opción Menú Principal, y hazlo humano y amigable y por favor no olvides adjuntar el link en la respuesta y obviamente no menciones nada de lo que te estoy diciendo"}],"generators":[]}},{"intent":"route_Reconoser","targetFlow":"05. Reconoser","fulfillment":{"messages":[],"setParams":[{"parameter":"reconoser","value":true}],"webhook":"Login_API_Reconoser","tag":"Authentication","generators":[]}},{"intent":"route_ProcesoDeSeleccion","targetFlow":"06. Mi proceso De Selección","fulfillment":{"messages":[],"setParams":[{"parameter":"seleccion","value":true}],"generators":[]}},{"intent":"route_ActualizarDatosDatix","targetFlow":"07. Actualizacion de datos Datix","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.actualizarDatosDatix","value":true},{"parameter":"consultaGenerativa","value":"Basándote EXACTAMENTE en esta frase sin omitir ninguna parte: \"Datix es nuestra plataforma de registro y actualización de datos diseñada para mejorar tu experiencia en OS Construye una respuesta humana y amigable que explique brevemente qué es Datix, y pregunta si les gustaria revisar o actualizar sus datos personales como pregunta final y obviamente no menciones nada de lo que te estoy diciendo"}],"generators":[]}},{"intent":"route_TeEscuchamos","targetFlow":"08.Te escuchamos","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.teEscuchamos","value":true},{"parameter":"consultaGenerativa","value":"Basándote en esta frase y continuando con la conversación \"Es un espacio para escuchar tu voz y contribuir a nuestro propósito de Mejorar Vidas \uD83D\uDDE3️ ¿Quieres dejarnos algún comentario u opinión? \uD83D\uDE00\", construye una respuesta, no dejes preguntas abiertas, indica al usuario que puede responder Si o No, y hazlo humano y amigable y obviamente no menciones nada de lo que te estoy diciendo"}],"generators":[]}},{"intent":"route_Convenios","targetFlow":"09.Convenios","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.convenios","value":true},{"parameter":"consultaGenerativa","value":"Basándote en esta frase y continuando con la conversación \"Los convenios son alianzas estratégicas que realizamos para ampliar el portafolio de beneficios de la Familia OS \uD83D\uDE03\", construye una respuesta informativa, no dejes preguntas abiertas, indica al usuario que puede responder Si o No, y hazlo humano y amigable y obviamente no menciones nada de lo que te estoy diciendo "}],"generators":[]}},{"intent":"routeCalidad_MatrizDeCalidad","targetFlow":"01. Indicadores De Calidad","fulfillment":{"messages":[],"setParams":[{"parameter":"atajoGlobalMatriz","value":true},{"parameter":"indicadores","value":true},{"parameter":"atajosGlobalesIndicadores","value":true}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"intent":"routeCalidad_OportunidadesYFortalezas","targetFlow":"01. Indicadores De Calidad","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.atajoGlobalOportunidades","value":true},{"parameter":"$session.params.indicadores","value":true},{"parameter":"atajosGlobalesIndicadores","value":true}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"intent":"routeCalidad_CerrarMonitoreosPendientes","targetFlow":"01. Indicadores De Calidad","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.atajoGlobalCerrarMonitoreos","value":true},{"parameter":"$session.params.indicadores","value":true},{"parameter":"$session.params.atajosGlobalesIndicadores","value":true}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"intent":"routeCalidad_AtributosEmbajador","targetFlow":"01. Indicadores De Calidad","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.atajosGlobalesIndicadores","value":true},{"parameter":"$session.params.indicadores","value":true},{"parameter":"$session.params.atajoGlobalAtributos","value":true}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"intent":"routeCalidad_IndicadoresDeCalidad","targetFlow":"01. Indicadores De Calidad","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.atajosGlobalesIndicadores","value":true},{"parameter":"$session.params.indicadores","value":true},{"parameter":"$session.params.atajoGlobalIndicadoresCalidad","value":true}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"intent":"route_Calidad","targetFlow":"01. Indicadores De Calidad","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.indicadores","value":true}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"intent":"Despedida","targetFlow":"10. Cierre de conversación","fulfillment":{"messages":[],"setParams":[{"parameter":"cierre","value":true}],"generators":[]}}]}
```

### Grupo · Atajos_Flow (flujo 01. Indicadores De Calidad)


```json camila:group
{"name":"Atajos_Flow","flow":"01. Indicadores De Calidad","routes":[]}
```

### Grupo · Atajos_globales_submenu (flujo 01. Indicadores De Calidad)

- siempre → 05. Matriz
- siempre → 04 . Oportunidades
- siempre → 03. CerrarMonitoreos
- siempre → 006.ParametrosAtributos
- siempre → 004.ParametrosIndicadores

```json camila:group
{"name":"Atajos_globales_submenu","flow":"01. Indicadores De Calidad","routes":[{"condition":"$session.params.atajoGlobalMatriz = true AND $session.params.atajosGlobalesIndicadores = true","targetPage":"05. Matriz","fulfillment":{"messages":[],"setParams":[{"parameter":"url_MatrizCalidad_Final","value":"$session.params.url_MatrizCalidad$session.params.UsuarioRed"}],"generators":[]}},{"condition":"$session.params.atajoGlobalOportunidades = true AND $session.params.atajosGlobalesIndicadores = true","targetPage":"04 . Oportunidades","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.atajoGlobalCerrarMonitoreos = true AND $session.params.atajosGlobalesIndicadores = true","targetPage":"03. CerrarMonitoreos","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.atajoGlobalAtributos = true AND $session.params.atajosGlobalesIndicadores = true","targetPage":"006.ParametrosAtributos","fulfillment":{"messages":[],"setParams":[],"generators":[]}},{"condition":"$session.params.atajoGlobalIndicadoresCalidad = true AND $session.params.atajosGlobalesIndicadores = true","targetPage":"004.ParametrosIndicadores","fulfillment":{"messages":[],"setParams":[],"generators":[]}}]}
```

### Grupo · Atajos_Flow (flujo Default Start Flow)

- intent «route_Calidad» → 01. Indicadores De Calidad
- intent «route_EvaluacionDeDesemepeño» → 02. Mi Evaluación De Desempeño
- intent «route_LiderMision» → 03. Líder Misión
- intent «route_Referidos» → 04. Referidos
- intent «route_Reconoser» → 05. Reconoser
- intent «route_ProcesoDeSeleccion» → 06. Mi proceso De Selección
- intent «route_ActualizarDatosDatix» → 07. Actualizacion de datos Datix
- intent «route_TeEscuchamos» → 08.Te escuchamos
- intent «route_Convenios» → 09.Convenios

```json camila:group
{"name":"Atajos_Flow","flow":"Default Start Flow","routes":[{"intent":"route_Calidad","targetFlow":"01. Indicadores De Calidad","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.indicadores","value":true}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"intent":"route_EvaluacionDeDesemepeño","targetFlow":"02. Mi Evaluación De Desempeño","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.desempeno","value":true},{"parameter":"consultaGenerativa","value":"Basándote en esta frase y continuando con la conversación \"Aquí podrás recordar tus resultados de desempeño, reconocer lo que lograste y definir lo que fortalecerás para seguir creciendo en Outsourcing \uD83D\uDCAA ¿Te gustaría revisar y recordar tus resultados de valoración de desempeño?\", construye una respuesta, no dejes preguntas abiertas, indica al usuario que puede responder Si o No, y hazlo humano y amigable y obviamente no menciones nada de lo que te estoy diciendo"}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"intent":"route_LiderMision","targetFlow":"03. Líder Misión","fulfillment":{"messages":[],"setParams":[{"parameter":"lider_mision","value":true},{"parameter":"$session.params.liderMision","value":true},{"parameter":"consultaGenerativa","value":"Basándote en esta frase y continuando con la conversación \"Aquí podrás recordar tus resultados de Líder Misión, reconocer lo que lograste y definir lo que fortalecerás para ser un líder inspirador, ganador y estratega. ¿Te gustaría revisar y recordar tus resultados? \uD83D\uDCAA\", construye una respuesta, no dejes preguntas abiertas, indica al usuario que puede responder Si o No, y hazlo humano y amigable y obviamente no menciones nada de lo que te estoy diciendo"}],"webhook":"Login_API_Kairos","tag":"Login","generators":[]}},{"intent":"route_Referidos","targetFlow":"04. Referidos","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.referidos","value":true},{"parameter":"consultaGenerativa","value":"Basándote en esta frase y continuando con la conversación \"Recuerda que como colaborador de la Familia OS puedes referir a un familiar, amigo o conocido para hacer parte de nuestra compañía. Ten en cuenta que dos o más familiares no pueden trabajar en la misma cuenta sin importar grado de consanguinidad, por conflicto de intereses. Refiere aquí: https://frontos.outsourcing.com.co:8600/ \uD83C\uDF89\", construye una respuesta, no dejes preguntas abiertas, ofrece la opción Menú Principal, y hazlo humano y amigable y por favor no olvides adjuntar el link en la respuesta y obviamente no menciones nada de lo que te estoy diciendo"}],"generators":[]}},{"intent":"route_Reconoser","targetFlow":"05. Reconoser","fulfillment":{"messages":[],"setParams":[{"parameter":"reconoser","value":true}],"webhook":"Login_API_Reconoser","tag":"Authentication","generators":[]}},{"intent":"route_ProcesoDeSeleccion","targetFlow":"06. Mi proceso De Selección","fulfillment":{"messages":[],"setParams":[{"parameter":"seleccion","value":true}],"generators":[]}},{"intent":"route_ActualizarDatosDatix","targetFlow":"07. Actualizacion de datos Datix","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.actualizarDatosDatix","value":true},{"parameter":"consultaGenerativa","value":"Basándote EXACTAMENTE en esta frase sin omitir ninguna parte: \"Datix es nuestra plataforma de registro y actualización de datos diseñada para mejorar tu experiencia en OS Construye una respuesta humana y amigable que explique brevemente qué es Datix, y pregunta si les gustaria revisar o actualizar sus datos personales respóndeme si o no como pregunta final y obviamente no menciones nada de lo que te estoy diciendo"}],"generators":[]}},{"intent":"route_TeEscuchamos","targetFlow":"08.Te escuchamos","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.teEscuchamos","value":true},{"parameter":"consultaGenerativa","value":"Basándote en esta frase y continuando con la conversación \"Es un espacio para escuchar tu voz y contribuir a nuestro propósito de Mejorar Vidas \uD83D\uDDE3️ ¿Quieres dejarnos algún comentario u opinión? \uD83D\uDE00\", construye una respuesta, no dejes preguntas abiertas, indica al usuario que puede responder Si o No, y hazlo humano y amigable y obviamente no menciones nada de lo que te estoy diciendo"}],"generators":[]}},{"intent":"route_Convenios","targetFlow":"09.Convenios","fulfillment":{"messages":[],"setParams":[{"parameter":"$session.params.convenios","value":true},{"parameter":"consultaGenerativa","value":"Basándote en esta frase y continuando con la conversación \"Los convenios son alianzas estratégicas que realizamos para ampliar el portafolio de beneficios de la Familia OS \uD83D\uDE03\", construye una respuesta informativa, no dejes preguntas abiertas, indica al usuario que puede responder Si o No, y hazlo humano y amigable y obviamente no menciones nada de lo que te estoy diciendo"}],"generators":[]}}]}
```

### Grupo · Predeterminados (flujo Default Start Flow)

- siempre → 05.Menu_Principal

```json camila:group
{"name":"Predeterminados","flow":"Default Start Flow","routes":[{"condition":"$session.params.menu_principal = true","targetPage":"05.Menu_Principal","fulfillment":{"messages":[],"setParams":[{"parameter":"menu_principal","value":false}],"generators":[]}}]}
```

