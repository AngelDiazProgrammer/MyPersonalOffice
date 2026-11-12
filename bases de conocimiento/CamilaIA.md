---
confidential: true
---
# Camila IA · Base de conocimiento del bot CamilaOS

Generado automáticamente desde el export de Dialogflow CX (`Bots/`). **No lo edites a mano**: se regenera con `dotnet run --project src/AI.Agent.Office.Web -- --bot-generar` y el resultado es siempre el mismo.
Es la **narrativa** de la base de conocimiento: la lee el agente CamilaIA. El flujo ejecutable de cada bot está en sus playbooks (`playbooks bots/`).
Este archivo **no contiene credenciales**: los webhooks citan secretos externos con `{{secret:NOMBRE}}` (se definen en `.env`).

## Cómo leer este documento

Cada **página** es un paso del bot. Al llegar a una página ocurre, **siempre en este orden**:

1. 📌 **Asigna variables** (parámetros de sesión).
2. ⚡/✍️ **Llama a un webhook** (si la página lo tiene).
3. 🤖 **Ejecuta generadores de IA** (si los tiene).
4. 💬 **Dice** sus mensajes y 🔘 **ofrece botones**.
5. ⌨️ **Pide al usuario** los datos obligatorios de su formulario que aún no tenga (y espera su respuesta).
6. 🔀 **Evalúa las rutas automáticas** (solo condición): si una se cumple, sigue sin esperar al usuario.
7. ⏳ **Espera al usuario**: un botón, una frase que coincide con un intent, o nada que coincida (*sin coincidencia*).

- ⚡ **webhook de lectura**: consulta datos; no cambia nada. ✍️ **webhook de escritura**: guarda, envía o modifica datos reales; la plataforma **pide confirmación** antes de ejecutarlo.
- Un destino «END_SESSION» termina la conversación; «END_FLOW» vuelve al flujo inicial.
- Las rutas se prueban en este orden: página → grupos de rutas de la página → flujo → grupos del flujo.
- Debajo de cada elemento hay un bloque `camila:*` con su definición exacta; **es lo que lee la plataforma**. El texto de arriba se genera a partir de él.

## Resumen

- Flujos: 43 · Páginas: 216 · Intents: 38 · Webhooks: 33 (10 de escritura) · Generadores de IA: 13 · Entidades: 4
- Flujo inicial: Default Start Flow

## Mapa de flujos

| Flujo | Páginas | Pide datos | Lee | Escribe | IA | Se llega desde |
|---|---:|---|---|---|---|---|
| 01. Indicadores De Calidad | 20 | `usuarioActivo` | Mis_Atributos, Mis_Casos_Abiertos, Mis_Indicadores, Obtener Url Matriz de Calidad, Obtener_Usuario_Por_Cedula | — | — | 9 ruta(s) |
| 02. Mi Evaluación De Desempeño | 10 | `competenciaAFortalecer` | Evaluación de Desempeño, Obtener_Usuario_Por_Cedula | — | — | 3 ruta(s) |
| 02.1. Conversacional Evaluacion Desempeño | 23 | `mensajeUsuario1`, `mensajeUsuario2`, `itemSolicitudFecha2`, `mensajeUsuarioFecha` | Evaluación Desempeño 1 - IA Generativa Camila, Evaluación Desempeño 2 - IA Generativa Camila, Evaluación Desempeño 3 - IA Generativa Camila, Evaluación Desempeño 4 - Contextualización fechas - IA Generativa Camila, UrlPdfEvaluacionDesempeño | Guardar Acción Plan Evaluación De Desempeño | 🤖 | 2 ruta(s) |
| 03. Líder Misión | 8 | `competencia1`, `mayorSatisfaccion`, `atributoAFortalecer` | IA Generativa Camila - General, Lider Mision, Lider Mision - IA Generativa Camila | — | — | 3 ruta(s) |
| 03.1. Conversacional Lider Misión | 23 | `mensajeUsuario1`, `mensajeUsuario2`, `itemSolicitudFecha2`, `mensajeUsuarioFecha` | Evaluación Desempeño 1 - IA Generativa Camila, Evaluación Desempeño 2 - IA Generativa Camila, Evaluación Desempeño 3 - IA Generativa Camila, Evaluación Desempeño 4 - Contextualización fechas - IA Generativa Camila, UrlPdfEvaluacionDesempeño | Guardar Acción Plan Evaluación De Desempeño | 🤖 | 0 ruta(s) |
| 04. Referidos | 9 | `documentoReferido`, `nombreReferido`, `correoReferido`, `telefonoReferido` | Obtener_Colaborador_Referidos | Guardar_Referidos | — | 3 ruta(s) |
| 05. Reconoser | 7 | — | Login_API_Reconoser, Mis indicadores ReconoSER | — | — | 3 ruta(s) |
| 06. Mi proceso De Selección | 9 | — | Extraer información candidato de AIRA, Extraer información candidato de W | Finalizar Conversacion | — | 4 ruta(s) |
| 07. Actualizacion de datos Datix | 5 | — | IA Generativa Camila - General | Finalizar Conversacion | — | 4 ruta(s) |
| 08.Te escuchamos | 7 | `comentario` | IA Generativa Camila - General | Guardar Comentario Te Escuchamos | — | 3 ruta(s) |
| 09.Convenios | 5 | — | IA Generativa Camila - General | — | — | 3 ruta(s) |
| 10. Cierre de conversación | 1 | — | — | Finalizar Conversacion | — | 13 ruta(s) |
| 11. No Detecta | 2 | — | IA Generativa Camila - General | — | — | 2 ruta(s) |
| 12. ¿Algo Mas? | 1 | — | — | — | — | 31 ruta(s) |
| 13. Usuario inexistente/ inactivo | 4 | `Numero_Documento`, `placa_carro` | Obtener_Usuario_Por_Cedula | — | — | 1 ruta(s) |
| 14. Reportar Retiro | 3 | `retiroNombreColaborador`, `retiroNumeroTelColaborador`, `retiroNumeroDocumentoColaborador` | — | Envio HSM Encuesta de Retiro Camila, Finalizar Conversacion, Guardar Datos Colaborador Encuesta De Retiro | — | 1 ruta(s) |
| 14.1. Encuesta de Retiro | 15 | `respuesta` | — | Finalizar Conversacion, Guardar Encuesta De Retiro | — | 2 ruta(s) |
| 15. Viaje del colaborador | 5 | `nombre`, `cedula`, `correo`, `espacio` | — | — | — | 19 ruta(s) |
| 15.1 Encuesta W | 6 | `numero_documento` | Extraer información candidato de W | — | — | 1 ruta(s) |
| 15.10 Hasta pronto | 1 | — | — | Finalizar Conversacion | — | 1 ruta(s) |
| 15.2 Crece con nosotros | 1 | — | — | Finalizar Conversacion | — | 1 ruta(s) |
| 15.3 Cuidamos de ti | 1 | — | — | Finalizar Conversacion | — | 1 ruta(s) |
| 15.4 Induccion | 1 | — | — | Finalizar Conversacion | — | 1 ruta(s) |
| 15.5 Heramientas Kactus 1 | 1 | — | — | Finalizar Conversacion | — | 1 ruta(s) |
| 15.6 Herramientas Kactus 3 | 1 | — | — | Finalizar Conversacion | — | 1 ruta(s) |
| 15.7 Experiencia rol | 1 | — | — | Finalizar Conversacion | — | 1 ruta(s) |
| 15.8 Encuesta instalaciones | 1 | — | — | Finalizar Conversacion | — | 1 ruta(s) |
| 15.9 Lider mision | 1 | — | — | Finalizar Conversacion | — | 1 ruta(s) |
| 16. Administración de personal | 1 | — | — | — | — | 1 ruta(s) |
| 16.1 Pagos | 3 | — | — | — | — | 1 ruta(s) |
| 16.2 Creditos libranza | 3 | — | — | — | — | 1 ruta(s) |
| 16.3 EPS | 4 | — | — | — | — | 1 ruta(s) |
| 16.4 Caja compensacion | 1 | — | — | — | — | 1 ruta(s) |
| 16.5 Cesantias | 6 | — | — | — | — | 1 ruta(s) |
| 16.6 carne | 1 | — | — | — | — | 1 ruta(s) |
| 16.7 certificados | 3 | — | — | — | — | 1 ruta(s) |
| 16.8 Seguridad Social | 3 | `tipoDoc`, `numDoc`, `periodoPlanilla`, `correoPlanilla` | — | — | — | 1 ruta(s) |
| 17. Ex colaborador | 1 | — | — | — | — | 1 ruta(s) |
| 17.1 pagos | 3 | `numero_documento`, `numContrato` | — | — | — | 1 ruta(s) |
| 17.2 Cesantias | 2 | `numDoc`, `numContrato` | — | — | — | 1 ruta(s) |
| 17.3 Certificados | 5 | `documento`, `fechaExpedicion`, `correoElectronico`, `mesCertificado` | Login_API_Kairos | certificadosAdminPersonal-Basico, certificadosAdminPersonal-Ingresos | — | 1 ruta(s) |
| Default Start Flow | 6 | `nombreProvisional`, `Numero_Documento`, `exColaborador` | IA Generativa Camila - General, Login_API_Kairos, recuperarSessionWatsonId | Guardar Formulario Fine360  | — | 10 ruta(s) |
| Pruebas pdf | 2 | `pdf` | recuperarSessionWatsonId | — | — | 1 ruta(s) |

## ✍️ Webhooks de escritura (piden confirmación)

- **Envio HSM Encuesta de Retiro Camila** — POST `portalos.outsourcing.com.co:9609` · se usa en: 14. Reportar Retiro › 03. Ejecutar HSM Camila Envió de Encuesta (al entrar)
- **Finalizar Conversacion** — POST `fine360.outsourcing.com.co:9874` · se usa en: 06. Mi proceso De Selección › 08. Cierre flujo (ruta siempre); 07. Actualizacion de datos Datix › 05. Despedida (ruta siempre); 10. Cierre de conversación › 01. Cerrar Conversacion (ruta condición); 14. Reportar Retiro › 03. Ejecutar HSM Camila Envió de Encuesta (ruta siempre); 14.1. Encuesta de Retiro › 6. Despedida (ruta siempre); … (14 usos)
- **Guardar Acción Plan Evaluación De Desempeño** — POST `portalos.outsourcing.com.co:9454` · se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (ruta condición); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (ruta condición)
- **Guardar Comentario Te Escuchamos** — POST `fine360.outsourcing.com.co:9876` · se usa en: 08.Te escuchamos › 04.1 Responder comentario (ruta siempre)
- **Guardar Datos Colaborador Encuesta De Retiro** — POST `fine360.outsourcing.com.co:9876` · se usa en: 14. Reportar Retiro › 02. Guardar Datos Colaborador Retirado (al entrar)
- **Guardar Encuesta De Retiro** — POST `fine360.outsourcing.com.co:9876` · se usa en: 14.1. Encuesta de Retiro › Guardar Pregunta Y Respuesta (al entrar)
- **Guardar Formulario Fine360 ** — POST `fine360.outsourcing.com.co:9874` · se usa en: Default Start Flow › 02.Captura_Numero_Documento (ruta condición)
- **Guardar_Referidos** — POST `frontos.outsourcing.com.co:8600` · se usa en: 04. Referidos › 03. Guardar Referido (al entrar)
- **certificadosAdminPersonal-Basico** — POST `fine360.outsourcing.com.co:9876` · se usa en: 17.3 Certificados › 3. correo certificado basico (ruta condición); 17.3 Certificados › 5. envio datos certificado (ruta condición)
- **certificadosAdminPersonal-Ingresos** — POST `fine360.outsourcing.com.co:9876` · se usa en: 17.3 Certificados › 4. info ingresos y retenciones (ruta condición)

## ⌨️ Datos que el bot pide al usuario

- `usuarioActivo` (@sys.any) — 01. Indicadores De Calidad › 00.ValidacionUsuarioKairos
- `competenciaAFortalecer` (@sys.any) — 02. Mi Evaluación De Desempeño › 01.4. Selección libre de competencia: «¿Cuál de estas competencias te gustaría fortalecer en tu Plan de Desarrollo Individual?»
- `mensajeUsuario1` (@sys.any) — 02.1. Conversacional Evaluacion Desempeño › 01. Mensaje1
- `mensajeUsuario1` (@sys.any) — 02.1. Conversacional Evaluacion Desempeño › 01.1 Evaluar si hay cierre de conversacion
- `mensajeUsuario2` (@sys.any) — 02.1. Conversacional Evaluacion Desempeño › 02. Mensaje2
- `itemSolicitudFecha2` (@sys.any) — 02.1. Conversacional Evaluacion Desempeño › 03. Contexto sobre fechas
- `mensajeUsuarioFecha` (@sys.date) — 02.1. Conversacional Evaluacion Desempeño › 03.1 Entregar item y solicitar Fecha: «$session.params.mensajeGenerativoFechas»
- `mensajeSeleccionCompetencia` (@sys.any) — 02.1. Conversacional Evaluacion Desempeño › PDI - Seleccionar competencia
- `competencia1` (@sys.any) — 03. Líder Misión › 01. Notas Lider Mision
- `mayorSatisfaccion` (@sys.any) — 03. Líder Misión › 01.1.Atributo Mayor Satisfaccion
- `atributoAFortalecer` (@sys.any) — 03. Líder Misión › 01.2. Atributo A Fortalecer
- `mensajeUsuario1` (@sys.any) — 03.1. Conversacional Lider Misión › 01. Mensaje1
- `mensajeUsuario1` (@sys.any) — 03.1. Conversacional Lider Misión › 01.1 Evaluar si hay cierre de conversacion
- `mensajeUsuario2` (@sys.any) — 03.1. Conversacional Lider Misión › 02. Mensaje2
- `itemSolicitudFecha2` (@sys.any) — 03.1. Conversacional Lider Misión › 03. Contexto sobre fechas
- `mensajeUsuarioFecha` (@sys.date) — 03.1. Conversacional Lider Misión › 03.1 Entregar item y solicitar Fecha: «$session.params.mensajeGenerativoFechas»
- `mensajeSeleccionCompetencia` (@sys.any) — 03.1. Conversacional Lider Misión › PDI - Seleccionar competencia
- `documentoReferido` (@sys.any) — 04. Referidos › 01. Solicitar Referido: «🆔 Por favor, indícanos el número de documento del referido.»
- `nombreReferido` (@sys.any) — 04. Referidos › 01. Solicitar Referido: «👤 ¿Cuál es el nombre completo?»
- `correoReferido` (@sys.any) — 04. Referidos › 01. Solicitar Referido: «📧 ¿Cuál es el correo electrónico del referido?»
- `telefonoReferido` (@sys.any) — 04. Referidos › 01. Solicitar Referido: «📱 ¿Cuál es el número de teléfono del referido?»
- `comentario` (@sys.any) — 08.Te escuchamos › 04. Capturar comentario
- `Numero_Documento` (@Numero_Documento) — 13. Usuario inexistente/ inactivo › 00.1 Solicitar numero de documento
- `placa_carro` (@placa_carro) — 13. Usuario inexistente/ inactivo › 00.1 Solicitar numero de documento
- `retiroNombreColaborador` (@sys.any) — 14. Reportar Retiro › 01. Solicitar datos colaborador: «Ingresa el nombre del colaborador»
- `retiroNumeroTelColaborador` (@sys.any) — 14. Reportar Retiro › 01. Solicitar datos colaborador: «Ingresa el número de celular del colaborador»
- `retiroNumeroDocumentoColaborador` (@sys.any) — 14. Reportar Retiro › 01. Solicitar datos colaborador: «Ingresa el número de identificación del colaborador»
- `respuesta` (@sys.any) — 14.1. Encuesta de Retiro › 1. Respuesta 1 Pregunta 1
- `respuesta` (@sys.any) — 14.1. Encuesta de Retiro › 1. Respuesta 2 Pregunta 1
- `respuesta` (@sys.any) — 14.1. Encuesta de Retiro › 1. Respuesta 3 Pregunta 1
- `respuesta` (@sys.any) — 14.1. Encuesta de Retiro › 1. Respuesta 4 Pregunta 1
- `respuesta` (@sys.any) — 14.1. Encuesta de Retiro › Repuesta En Desacuerdo
- `respuesta` (@sys.any) — 14.1. Encuesta de Retiro › Respuesta Totalmente Desacuerdo
- `nombre` (@sys.any) — 15. Viaje del colaborador › 0.datosNombre: «Para iniciar, por favor escribe tu nombre completo:»
- `cedula` (@sys.any) — 15. Viaje del colaborador › 1.datosCedula
- `correo` (@sys.any) — 15. Viaje del colaborador › 2.datosCorreo
- `espacio` (@sys.any) — 15. Viaje del colaborador › 3.datosEspacio
- `numero_documento` (@sys.any) — 15.1 Encuesta W › 0.1 Solicitar Cedula: «Por favor digita tu número de documento (sin puntos ni espacios):»
- `tipoDoc` (@sys.any) — 16.8 Seguridad Social › 1. Formulario Planillas: «Digita el tipo de documento»
- `numDoc` (@sys.any) — 16.8 Seguridad Social › 1. Formulario Planillas: «Digita el número de documento»
- `periodoPlanilla` (@sys.any) — 16.8 Seguridad Social › 1. Formulario Planillas: «Ingresa mes y año MM / AAA»
- `correoPlanilla` (@sys.any) — 16.8 Seguridad Social › 1. Formulario Planillas: «Escribe el correo electrónico en el cual deseas recibir tu certificado de planillas de pago»
- `numero_documento` (@sys.any) — 17.1 pagos › 1.Formulario Liquidacion: «Digita el numero de documento:»
- `numContrato` (@sys.any) — 17.1 pagos › 1.Formulario Liquidacion: «Selecciona el número de contrato a consultar»
- `numDoc` (@sys.any) — 17.2 Cesantias › 0.Respuesta: «Digita el número de documento:»
- `numContrato` (@sys.any) — 17.2 Cesantias › 0.Respuesta: «Selecciona el número de contrato a consultar»
- `documento` (@sys.any) — 17.3 Certificados › 1. datos: «Digita el número de documento»
- `fechaExpedicion` (@sys.any) — 17.3 Certificados › 1. datos: «Ingresa fecha de expedición de tu documento DD-MM-AAAA»
- `correoElectronico` (@sys.any) — 17.3 Certificados › 3. correo certificado basico: «Por favor escribe el correo electrónico que registraste con nosotros para recibir tu certificado laboral»
- `mesCertificado` (@sys.any) — 17.3 Certificados › 3. correo certificado basico: «Por favor escribe el mes desde donde quieres que se genere el certificado.»
- `anoCertificado` (@sys.any) — 17.3 Certificados › 3. correo certificado basico: «Escribe el año desde donde desees solicitar el certificado. Recuerda que solo se puede solicitar a partir del …»
- `mesFinalCertificado` (@sys.any) — 17.3 Certificados › 3. correo certificado basico: «Por favor escribe el mes final para realizar la consulta.»
- `anoFinalCertificado` (@sys.any) — 17.3 Certificados › 3. correo certificado basico: «Escribe el año hasta donde desees solicitar el certificado.»
- `anioCertificado` (@sys.any) — 17.3 Certificados › 4. info ingresos y retenciones: «Por favor ingresa el año (Cuatro dígitos, Ejemplo: 2001) del cual necesitas generar tu certificado.»
- `correoIngresos` (@sys.any) — 17.3 Certificados › 4. info ingresos y retenciones: «Por favor escribe el correo electrónico en el cual deseas recibir tu certificado de ingresos y retenciones»
- `mesCertificado` (@sys.any) — 17.3 Certificados › 5. envio datos certificado: «Por favor escribe el mes desde donde quieres que se genere el certificado.»
- `anioCertificado` (@sys.any) — 17.3 Certificados › 5. envio datos certificado: «Escribe el año desde donde desees solicitar el certificado. Recuerda que solo se puede solicitar a partir del …»
- `correoElectronico` (@sys.any) — 17.3 Certificados › 5. envio datos certificado: «Por favor escribe el correo electrónico en el cual deseas recibir tu certificado de planillas de pago.»
- `finalMesCertificado` (@sys.any) — 17.3 Certificados › 5. envio datos certificado: «Por favor escribe el mes final para realizar la consulta.»
- `finalAnioCertificado` (@sys.any) — 17.3 Certificados › 5. envio datos certificado: «Escribe el año hasta donde desees solicitar el certificado.»
- `nombreProvisional` (@sys.any) — Default Start Flow › 01. Bienvenida_CamilaIA
- `Numero_Documento` (@Numero_Documento) — Default Start Flow › 02.Captura_Numero_Documento
- `exColaborador` (@sys.any) — Default Start Flow › 02.Captura_Numero_Documento
- `exColaborador` (@sys.any) — Default Start Flow › 04.Ingreso_Existoso
- `pdf` (@sys.any) — Pruebas pdf › enviar pdf

## 🤖 Generadores de IA

- **actualizar_estado_pdi** — se usa en: 02.1. Conversacional Evaluacion Desempeño › PDI - Actualizar estado (al entrar); 03.1. Conversacional Lider Misión › PDI - Actualizar estado (al entrar)
- **extractor_acciones** — se usa en: 02.1. Conversacional Evaluacion Desempeño › 02. Mensaje2 (al entrar); 02.1. Conversacional Evaluacion Desempeño › 03. Contexto sobre fechas (al entrar); 03.1. Conversacional Lider Misión › 02. Mensaje2 (al entrar); 03.1. Conversacional Lider Misión › 03. Contexto sobre fechas (al entrar)
- **extractor_materiales** — se usa en: 02.1. Conversacional Evaluacion Desempeño › 02. Mensaje2 (al entrar); 02.1. Conversacional Evaluacion Desempeño › 03. Contexto sobre fechas (al entrar); 03.1. Conversacional Lider Misión › 02. Mensaje2 (al entrar); 03.1. Conversacional Lider Misión › 03. Contexto sobre fechas (al entrar)
- **extraer_accion_fecha_actual** — se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)
- **extraer_contador_accion_fecha_actual** — se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)
- **extraer_estado_fechas** — se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)
- **extraer_fecha_compromiso_actual** — se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)
- **extraer_fecha_valida** — se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)
- **extraer_json_fechas** — se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)
- **extraer_mensaje_fechas** — se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)
- **procesar_fechas_pdi** — se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.15 Generar JSON fechas (al entrar); 03.1. Conversacional Lider Misión › 03.15 Generar JSON fechas (al entrar)
- **resolver_seleccion_competencia_pdi** — se usa en: (no se usa)
- **validar** — se usa en: 02.1. Conversacional Evaluacion Desempeño › PDI - Seleccionar competencia (ruta condición); 03.1. Conversacional Lider Misión › PDI - Seleccionar competencia (ruta condición)

## Índice por flujo

- **01. Indicadores De Calidad** — 20 página(s): 0.ValidacionTokenKairos · 00.ValidacionUsuarioKairos · 0001. Condicional_Atajos · 001. Menu_Indicadores · 002.UsuarioNoExiste · 003.UsuarioInactivo · 004.ParametrosIndicadores · 005.ParametrosIndicadoresRestantes · 006.ParametrosAtributos · 01.1. Link PowerBI · 01.IndicadoresCalidad · 02. Atributos · 03. CerrarMonitoreos · 03.1.1 Muestra imagen · 03.1.1 RevisarSiHayCasos · 03.1.InformacionCasosAbiertos · 03.2 NoHayMonitoreosPendientes · 04 . Oportunidades · 05. Matriz · 06. ¿Algo mas?
- **02. Mi Evaluación De Desempeño** — 10 página(s): 00. Mi_Evaluacion_De_Desempeño · 00.00 Creacion de aprametro restantes · 01. Resultados Evaluación De Desempeño · 01.1. Resultado mayor satisfacción · 01.2. Resultado a fortalecer · 01.3. Sugerir competencias a fortalecer · 01.4. Selección libre de competencia · 02. Mensaje2 · 02. No Resultados Evaluación De Desempeño · 03. ¿Algo mas?
- **02.1. Conversacional Evaluacion Desempeño** — 23 página(s): 01. Mensaje1 · 01.1 Evaluar si hay cierre de conversacion · 01.2 Evaluar respuesta Prompt 1 · 02. Mensaje2 · 02.1 Evaluar respuesta Prompt 2 · 03. Contexto sobre fechas · 03.1 Entregar item y solicitar Fecha · 03.14 Validar fecha · 03.15 Generar JSON fechas · 03.2 Procesar item y fecha · 05. Deseas generar otro plan? · 3.8 · 4. Consulta PDF plan de mejoramiento · 4.1 Entrega Pdf Plan de Mejoramiento · PDI - Actualizar estado · PDI - Evaluar competencia a fortalecer · PDI - Generar mensaje inicial · PDI - Guardar estado · PDI - Ofrecer competencia alterna · PDI - Seleccionar competencia · PDI - Validar segunda competencia · PDI - Validar selección competencia · PDI validar DEFINIR FECHAS
- **03. Líder Misión** — 8 página(s): 00. Líder_Misión · 01. Notas Lider Mision · 01.1.Atributo Mayor Satisfaccion · 01.2. Atributo A Fortalecer · 01.3 . Sugerencia Atributos · 01.4 CompetenciaMenor1 · 02. No Consultar Notas Lider Mision · 03. ¿Algo Mas?
- **03.1. Conversacional Lider Misión** — 23 página(s): 01. Mensaje1 · 01.1 Evaluar si hay cierre de conversacion · 01.2 Evaluar respuesta Prompt 1 · 02. Mensaje2 · 02.1 Evaluar respuesta Prompt 2 · 03. Contexto sobre fechas · 03.1 Entregar item y solicitar Fecha · 03.14 Validar fecha · 03.15 Generar JSON fechas · 03.2 Procesar item y fecha · 05. Deseas generar otro plan? · 3.8 · 4. Consulta PDF plan de mejoramiento · 4.1 Entrega Pdf Plan de Mejoramiento · PDI - Actualizar estado · PDI - Evaluar competencia a fortalecer · PDI - Generar mensaje inicial · PDI - Guardar estado · PDI - Ofrecer competencia alterna · PDI - Seleccionar competencia · PDI - Validar segunda competencia · PDI - Validar selección competencia · PDI validar DEFINIR FECHAS
- **04. Referidos** — 9 página(s): 00. Referidos · 01. Solicitar Referido · 01.1 Solicitar area de experiencia del referido · 01.2 Segunda lista de experiencias laborales · 02. ¿Otra área de experiencia? · 02.1. Otra área de experiencia · 02.2 Otra área de experiencia segunda lista · 03. Guardar Referido · 04. ¿Otro Referido?
- **05. Reconoser** — 7 página(s): 00. Mis_Indicadores_ReconoSer · 01.1. SolicitarMesIndicadores · 01.1.1 Mas meses · 01.2. Confirmar información de consulta · 01.3. Mostrar resultados Reconoser · 01.ConocerIndicadoresReconoser · 02.NoConocerIndicadoresReconoser
- **06. Mi proceso De Selección** — 9 página(s): 00. Definicion de variables para consultas · 01. Mi_Proceso_De_Seleccion_AIRA · 02. Mi_Proceso_De_Seleccion_W · 03. Definición de resultados globales · 04. Definición de resultados Aira · 05. Definición de resultados W · 06. Frases estado y fase W · 07. Frases estado AIRA · 08. Cierre flujo
- **07. Actualizacion de datos Datix** — 5 página(s): 00. Actualizar Datos · 02. Enlace Datix · 03. Recordatorio OS · 04. Algo Mas · 05. Despedida
- **08.Te escuchamos** — 7 página(s): 00. Dejar algun comentario · 01. Decision Dejar algun comentario · 02. Algo mas · 03. Capturar datos · 04. Capturar comentario · 04.1 Responder comentario · 05. Despedida
- **09.Convenios** — 5 página(s): 00. Alianzas estrategicas · 01. Decisión conocer conveniOS · 02. Enlace de conveniOS · 03. Algo mas · 04. Despedida
- **10. Cierre de conversación** — 1 página(s): 01. Cerrar Conversacion
- **11. No Detecta** — 2 página(s): 00. Irespondible · 01.Menu
- **12. ¿Algo Mas?** — 1 página(s): 01. ¿Algo mas?
- **13. Usuario inexistente/ inactivo** — 4 página(s): 0.1 condicional documento · 0.2 Usuario no registrado · 0.3 Usuario inactivo · 00.1 Solicitar numero de documento
- **14. Reportar Retiro** — 3 página(s): 01. Solicitar datos colaborador · 02. Guardar Datos Colaborador Retirado · 03. Ejecutar HSM Camila Envió de Encuesta
- **14.1. Encuesta de Retiro** — 15 página(s): 0. Definicion de Preguntas · 1. Primera Pregunta · 1. Respuesta 1 Pregunta 1 · 1. Respuesta 2 Pregunta 1 · 1. Respuesta 3 Pregunta 1 · 1. Respuesta 4 Pregunta 1 · 2. Segunda pregunta · 3. Tercera Pregunta · 4. Cuarta Pregunta · 5. Quinta Pregunta · 6. Despedida · Guardar Pregunta Y Respuesta · Procesamiento de respuestas · Repuesta En Desacuerdo · Respuesta Totalmente Desacuerdo
- **15. Viaje del colaborador** — 5 página(s): 0.datosNombre · 1.datosCedula · 2.datosCorreo · 3.datosEspacio · 4.despacho
- **15.1 Encuesta W** — 6 página(s): 0.1 Solicitar Cedula · 0.Definir Encuesta W · 1.Encuesta Proceso de Seleccion · 2. Encuesta Proceso de Capacitacion · 3. Encuesta Proceso de Contratacion · 4. Cierre
- **15.10 Hasta pronto** — 1 página(s): 0.Encuesta
- **15.2 Crece con nosotros** — 1 página(s): 0. Encuesta
- **15.3 Cuidamos de ti** — 1 página(s): 0. Encuesta
- **15.4 Induccion** — 1 página(s): 0. Encuesta
- **15.5 Heramientas Kactus 1** — 1 página(s): 0.Encuesta
- **15.6 Herramientas Kactus 3** — 1 página(s): 0. Encuesta
- **15.7 Experiencia rol** — 1 página(s): 0.Encuesta
- **15.8 Encuesta instalaciones** — 1 página(s): 0.Encuesta
- **15.9 Lider mision** — 1 página(s): 0.Encuesta
- **16. Administración de personal** — 1 página(s): 1. ¿En que te puedo ayudar?
- **16.1 Pagos** — 3 página(s): 0.En que puedo ayudarte? · 1. Dudas Pagos · 2.Desprendible Nomina
- **16.2 Creditos libranza** — 3 página(s): 0. ¿En que puedo ayudarte? · 1. Entidades con convenio · 2. Documentos solicitud de crédito
- **16.3 EPS** — 4 página(s): 0. ¿En que puedo ayudarte? · 1. agregarBeneficiario · 2. cambioTraslado · 3. afiliacion
- **16.4 Caja compensacion** — 1 página(s): 1. Información de adición familiar
- **16.5 Cesantias** — 6 página(s): 0. ¿En que puedo ayudarte? · 1. Estudio · 2. Compra vivienda · 3. Credito hipotecario · 4. Arreglos locativos · 5. Impuesto predial
- **16.6 carne** — 1 página(s): 1. carne
- **16.7 certificados** — 3 página(s): 0. ¿En que puedo ayudarte? · 1. Laboral/Ingresos · 2. Laboral/Especial
- **16.8 Seguridad Social** — 3 página(s): 0.menu  · 1. Formulario Planillas · 2.Confirmacion Planillas
- **17. Ex colaborador** — 1 página(s): 1. ¿En que puedo ayudarte?
- **17.1 pagos** — 3 página(s): 0. ¿En que puedo ayudarte? · 1.Formulario Liquidacion · 2. Estado liquidacion
- **17.2 Cesantias** — 2 página(s): 0.Respuesta · 1.Elección
- **17.3 Certificados** — 5 página(s): 1. datos · 2.seleccionar certificado · 3. correo certificado basico · 4. info ingresos y retenciones · 5. envio datos certificado
- **Default Start Flow** — 6 página(s): 01. Bienvenida_CamilaIA · 01.1.Elige rol · 02.Captura_Numero_Documento · 03.Validacion_Usuario_Kairos · 04.Ingreso_Existoso · 05.Menu_Principal
- **Pruebas pdf** — 2 página(s): Mostrar pdf · enviar pdf

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

### Página · 01. Indicadores De Calidad › 0.ValidacionTokenKairos

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «00.ValidacionUsuarioKairos»  
  ↳ ⚡ Llama al webhook **Obtener_Usuario_Por_Cedula** [ObtenerUsuarioPorCedula] — lectura

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

### Página · 01. Indicadores De Calidad › 0001. Condicional_Atajos

**Al entrar, en este orden:**

_No hace nada al entrar._

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `url_MatrizCalidad`, `url_MatrizCalidad_Final`, `url_MatrizCalidad_Final_Encriptada`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.atajosGlobalesIndicadores != true` → página «001. Menu_Indicadores»

**Grupos de rutas compartidas:** Atajos_globales_submenu

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

### Página · 01. Indicadores De Calidad › 002.UsuarioNoExiste

**Al entrar, en este orden:**

1. 💬 Dice: «Por ahora no te existes en al base de datos de Kairos, por favor comunícate con el área encargada»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true`

### Página · 01. Indicadores De Calidad › 003.UsuarioInactivo

**Al entrar, en este orden:**

1. 💬 Dice: «Por ahora estas inactivo en al base de datos de Kairos, por favor comunícate con el área encargada»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true`

### Página · 01. Indicadores De Calidad › 004.ParametrosIndicadores

**Al entrar, en este orden:**

1. 💬 Dice: «Estos resultados te ayudaran a conocer tu desempeño trimestralmente y a seguir fortaleciendo tu crecimiento dentro del equipo. 📈  Tus resultados de calidad del último trimestre son:»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `mesActualMes`, `mesActualCantidadMonitoreos`, `mesActualNotaCalidad`, `mesActualCantidadCriticos`, `mesActualCantidadNoCriticos`, `mesActualNotaCriticos`, `mesActualNotaNoCriticos`, `mesAnteriorMes`, `mesAnteriorCantidadMonitoreos`, `mesAnteriorNotaCalidad`, `mesAnteriorCantidadCriticos`, `mesAnteriorCantidadNoCriticos`, `mesAnteriorNotaCriticos`, `mesAnteriorNotaNoCriticos`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «005.ParametrosIndicadoresRestantes»

### Página · 01. Indicadores De Calidad › 005.ParametrosIndicadoresRestantes

**Al entrar, en este orden:**

_No hace nada al entrar._

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `mesPenultimoMes`, `mesPenultimoCantidadMonitoreos`, `mesPenultimoNotaCalidad`, `mesPenultimoCantidadCriticos`, `mesPenultimoCantidadNoCriticos`, `mesPenultimoNotaCriticos`, `mesPenultimoNotaNoCriticos`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «01.IndicadoresCalidad»  
  ↳ ⚡ Llama al webhook **Mis_Indicadores** [ObtenerEstadisticasEmbajador] — lectura

### Página · 01. Indicadores De Calidad › 006.ParametrosAtributos

**Al entrar, en este orden:**

1. 💬 Dice: «Conoce los atributos de embajador de marca asociados a tu rol. Estos reflejan los comportamientos y actitudes que fortalecen la experiencia que brindamos cada día. 😊»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `mesActualMes`, `mesActualPorcentanjeProactivo`, `mesActualPorcentanjeApasionado`, `mesActualPorcentanjeEmpatia`, `mesAnteriorMes`, `mesAnteriorPorcentanjeProactivo`, `mesAnteriorPorcentanjeApasionado`, `mesAnteriorPorcentanjeEmpatia`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «02. Atributos»  
  ↳ ⚡ Llama al webhook **Mis_Atributos** [ObtenerAtributosEmbajador] — lectura

### Página · 01. Indicadores De Calidad › 01.1. Link PowerBI

**Al entrar, en este orden:**

1. 💬 Dice: «Para consultar el detalle, ingresa con tu número de identificación aquí. 👇»
2. 💬 Dice: «https://bit.ly/Powerbi_Embajadores»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «06. ¿Algo mas?»

### Página · 01. Indicadores De Calidad › 01.IndicadoresCalidad

**Al entrar, en este orden:**

1. 💬 Dice: «MES: $session.params.MesActualMes  ✅Cantidad de Monitoreos:  Cantidad de monitoreos: $session.params.MesActualCantidadMonitoreos  ✅Calidad Emitida:  Nota: $session.params.MesActualNotaCalidad  ✅Errore…»
2. 💬 Dice: «MES: $session.params.MesAnteriorMes  ✅Cantidad de Monitoreos:  Cantidad de monitoreos: $session.params.MesAnteriorCantidadMonitoreos  ✅Calidad Emitida:  Nota: $session.params.MesAnteriorNotaCalidad  ✅…»
3. 💬 Dice: «MES: $session.params.MesPenultimoMes  ✅Cantidad de Monitoreos:  Cantidad de monitoreos: $session.params.MesPenultimoCantidadMonitoreos  ✅Calidad Emitida:  Nota: $session.params.MesPenultimoNotaCalidad…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «01.1. Link PowerBI»

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

### Página · 01. Indicadores De Calidad › 03. CerrarMonitoreos

**Al entrar, en este orden:**

1. 💬 Dice: «Tu cierre oportuno es clave para transformar los hallazgos en oportunidades reales de mejora. 💪»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `mesCasosAbiertos`, `cantidadCasosAbiertos`, `idCaso`, `fechaCaso`, `segmentoCaso`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «03.1.1 RevisarSiHayCasos»  
  ↳ ⚡ Llama al webhook **Mis_Casos_Abiertos** [ObtenerCasosAbiertos] — lectura

### Página · 01. Indicadores De Calidad › 03.1.1 Muestra imagen

**Al entrar, en este orden:**

1. 💬 Muestra la tarjeta «Producto destacado»
2. 🖼️ Muestra una imagen $session.params.imagenMonitoreos

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «06. ¿Algo mas?»

### Página · 01. Indicadores De Calidad › 03.1.1 RevisarSiHayCasos

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mesCasosAbiertos = null` → página «03.2 NoHayMonitoreosPendientes»
- si `$session.params.mesCasosAbiertos != null` → página «03.1.InformacionCasosAbiertos»

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

### Página · 01. Indicadores De Calidad › 03.2 NoHayMonitoreosPendientes

**Al entrar, en este orden:**

1. 💬 Dice: «No cuentas con monitoreos pendientes en esta ocasión»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «06. ¿Algo mas?»

### Página · 01. Indicadores De Calidad › 04 . Oportunidades

**Al entrar, en este orden:**

1. 💬 Dice: «Conocer tus oportunidades y fortalezas en tiempo real ingresando aquí. 👇»
2. 💬 Dice: «https://bit.ly/Powerbi_Embajadores»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «06. ¿Algo mas?»

### Página · 01. Indicadores De Calidad › 05. Matriz

**Al entrar, en este orden:**

1. 💬 Dice: «Esta información te permitirá comprender tu desempeño en cada atributo y apoyar tu proceso de mejora continua. 🔎»
2. 💬 Dice: «$session.params.url_MatrizCalidad_Final_Encriptada»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «06. ¿Algo mas?»

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

### Página · 02. Mi Evaluación De Desempeño › 00.00 Creacion de aprametro restantes

**Al entrar, en este orden:**

_No hace nada al entrar._

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `notaCompetencia8`, `competencia6`, `competencia7`, `competencia8`, `centroCostos`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «00. Mi_Evaluacion_De_Desempeño»

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

### Página · 02. Mi Evaluación De Desempeño › 01.1. Resultado mayor satisfacción

**Al entrar, en este orden:**

1. 📌 Asigna: `mayorSatisfaccion` = ``

### Página · 02. Mi Evaluación De Desempeño › 01.2. Resultado a fortalecer

**Al entrar, en este orden:**

1. 📌 Asigna: `competenciaAFortalecer` = ``

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «01.3. Sugerir competencias a fortalecer»

### Página · 02. Mi Evaluación De Desempeño › 01.3. Sugerir competencias a fortalecer

**Al entrar, en este orden:**

_No hace nada al entrar._

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `mensajeGenerativoCompetencia1`, `mensajeGenerativoCompetencia2`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **02.1. Conversacional Evaluacion Desempeño**

### Página · 02. Mi Evaluación De Desempeño › 01.4. Selección libre de competencia

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `competenciaAFortalecer` — @sys.any · pregunta: «¿Cuál de estas competencias te gustaría fortalecer en tu Plan de Desarrollo Individual?»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL" AND $session.params.competenciaAFortalecer != null AND ($sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia1) OR $sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia2) OR $sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia3) OR $sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia4) OR $sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia5) OR $sys.func.LOWER($session.params.competenciaAFortalecer) = $sys.func.LOWER($session.params.competencia6))` → flujo **02.1. Conversacional Evaluacion Desempeño**  
  ↳ 📌 asigna `seleccionLibrePDI` = `true`, `segundaCompetencia` = `false`, `mensajeUsuario1` = ``

### Página · 02. Mi Evaluación De Desempeño › 02. Mensaje2

**Al entrar, en este orden:**

_No hace nada al entrar._

### Página · 02. Mi Evaluación De Desempeño › 02. No Resultados Evaluación De Desempeño

**Al entrar, en este orden:**

1. 💬 Dice: «Esta bien, aunque ten presente que tu desempeño está en tus manos y es importante que consultes tus resultados con regularidad para comprender en qué punto estás ✅»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → flujo **12. ¿Algo Mas?**

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

## Flujo · 02.1. Conversacional Evaluacion Desempeño

**Qué dice al empezar:** «$session.params.mensajeGenerativoCompetencia1»

**Se llega desde:**
- 02. Mi Evaluación De Desempeño › 01.3. Sugerir competencias a fortalecer ← siempre
- 02. Mi Evaluación De Desempeño › 01.4. Selección libre de competencia ← condición

**Webhooks que usa:** ⚡ Evaluación Desempeño 1 - IA Generativa Camila · ⚡ Evaluación Desempeño 2 - IA Generativa Camila · ⚡ Evaluación Desempeño 3 - IA Generativa Camila · ⚡ Evaluación Desempeño 4 - Contextualización fechas - IA Generativa Camila · ✍️ Guardar Acción Plan Evaluación De Desempeño · ⚡ UrlPdfEvaluacionDesempeño

**Pide al usuario:** `mensajeUsuario1` (01. Mensaje1), `mensajeUsuario1` (01.1 Evaluar si hay cierre de conversacion), `mensajeUsuario2` (02. Mensaje2), `itemSolicitudFecha2` (03. Contexto sobre fechas), `mensajeUsuarioFecha` (03.1 Entregar item y solicitar Fecha), `mensajeSeleccionCompetencia` (PDI - Seleccionar competencia)

**Páginas:**
- 01. Mensaje1 — ⚡ Evaluación Desempeño 1 - IA Generativa Camila · ⌨️ pide datos
- 01.1 Evaluar si hay cierre de conversacion — ⚡ Evaluación Desempeño 4 - Contextualización fechas - IA Generativa Camila · ⌨️ pide datos
- 01.2 Evaluar respuesta Prompt 1 — ⚡ Evaluación Desempeño 2 - IA Generativa Camila
- 02. Mensaje2 — ⚡ Evaluación Desempeño 2 - IA Generativa Camila · ⌨️ pide datos · 🤖
- 02.1 Evaluar respuesta Prompt 2 — ⚡ Evaluación Desempeño 3 - IA Generativa Camila
- 03. Contexto sobre fechas — ⌨️ pide datos · 🤖
- 03.1 Entregar item y solicitar Fecha — ⌨️ pide datos
- 03.14 Validar fecha 
- 03.15 Generar JSON fechas — 🤖
- 03.2 Procesar item y fecha — ✍️ Guardar Acción Plan Evaluación De Desempeño · 🤖
- 05. Deseas generar otro plan? 
- 3.8 
- 4. Consulta PDF plan de mejoramiento — ⚡ UrlPdfEvaluacionDesempeño
- 4.1 Entrega Pdf Plan de Mejoramiento 
- PDI - Actualizar estado — 🤖
- PDI - Evaluar competencia a fortalecer 
- PDI - Generar mensaje inicial — ⚡ Evaluación Desempeño 1 - IA Generativa Camila
- PDI - Guardar estado — ⚡ Evaluación Desempeño 3 - IA Generativa Camila
- PDI - Ofrecer competencia alterna 
- PDI - Seleccionar competencia — ⌨️ pide datos · 🤖
- PDI - Validar segunda competencia 
- PDI - Validar selección competencia 
- PDI validar DEFINIR FECHAS 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.seleccionLibrePDI = true AND $session.params.competenciaAFortalecer != null` → página «PDI - Generar mensaje inicial»  
  ↳ 📌 asigna `seleccionLibrePDI` = `false`
- siempre → página «PDI - Evaluar competencia a fortalecer»  
  ↳ 📌 asigna `contador_acciones` = `0`, `total_acciones` = `6`
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 02.1. Conversacional Evaluacion Desempeño › 01. Mensaje1

**Al entrar, en este orden:**

1. 📌 Asigna: `controlConversacional` = `false`, `mensajeUsuario1` = ``, `estadoPDI` = `{}`
2. 💬 Dice: «$session.params.mensajeGenerativoCompetencia1»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `mensajeUsuario1` — @sys.any

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `controlConversacional`, `urlPdfPlanMejoramiento`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mensajeUsuario1 != null AND $session.params.controlConversacional = true` → página «01.2 Evaluar respuesta Prompt 1»  
  ↳ ⚡ Llama al webhook **Evaluación Desempeño 1 - IA Generativa Camila** [ConsultaWeb] — lectura

### Página · 02.1. Conversacional Evaluacion Desempeño › 01.1 Evaluar si hay cierre de conversacion

**Al entrar, en este orden:**

1. 📌 Asigna: `mensajeUsuario1` = ``
2. 💬 Dice: «$session.params.mensajeGenerativoCompetencia1»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `mensajeUsuario1` — @sys.any

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `contextoFechas`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mensajeGenerativoCompetencia1 = "DEFINIR FECHAS"` → página «03. Contexto sobre fechas»  
  ↳ ⚡ Llama al webhook **Evaluación Desempeño 4 - Contextualización fechas - IA Generativa Camila** [Consultaweb] — lectura
- si `$session.params.mensajeUsuario1 != null AND $session.params.mensajeGenerativoCompetencia1 != null AND $session.params.mensajeGenerativoCompetencia1 != "DEFINIR FECHAS"` → página «PDI - Actualizar estado»
- si `$session.params.mensajeGenerativoCompetencia1 != null AND $session.params.mensajeGenerativoCompetencia1 : "RESUMEN FINAL DEL PLAN DE DESARROLLO"` → se queda en la página  
  ↳ 📌 asigna `resumen_pdi_fijo` = `$session.params.mensajeGenerativoCompetencia1`

### Página · 02.1. Conversacional Evaluacion Desempeño › 01.2 Evaluar respuesta Prompt 1

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mensajeGenerativoCompetencia1 = "INICIAR DIAGNOSTICO"` → página «02. Mensaje2»  
  ↳ 📌 asigna `etapaPrompt2` = `INICIAL`, `mensajeUsuario2` = `` · ⚡ Llama al webhook **Evaluación Desempeño 2 - IA Generativa Camila** [consultaWeb] — lectura
- siempre → página «01. Mensaje1»

### Página · 02.1. Conversacional Evaluacion Desempeño › 02. Mensaje2

**Al entrar, en este orden:**

1. 📌 Asigna: `controlConversacional` = `false`, `mensajeUsuario2` = ``, `resumen_pdi_fijo` = `$if($session.params.mensajeGenerativoCompetencia1 CONTAINS "…`
2. 🤖 Ejecuta el generador de IA **extractor_materiales** con `resumen` → guarda en `materialComplementario`
3. 🤖 Ejecuta el generador de IA **extractor_acciones** con `estado_pdi` → guarda en `accionesAceptadas`
4. 💬 Dice: «$session.params.mensajeGenerativoCompetencia1»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `mensajeUsuario2` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mensajeUsuario2 != null AND $session.params.controlConversacional = true` → página «02.1 Evaluar respuesta Prompt 2»  
  ↳ 📌 asigna `resumen_pdi_fijo` = `$session.params.mensajeGenerativoCompetencia1`, `etapaPrompt2` = `DESAFIO` · ⚡ Llama al webhook **Evaluación Desempeño 2 - IA Generativa Camila** [ConsultaWeb] — lectura

### Página · 02.1. Conversacional Evaluacion Desempeño › 02.1 Evaluar respuesta Prompt 2

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mensajeGenerativoCompetencia1 = "INICIAR PLAN"` → página «01.1 Evaluar si hay cierre de conversacion»  
  ↳ 📌 asigna `mensajeUsuario1` = ``, `desafioPDI` = `$session.params.mensajeUsuario2`, `catalogoPDIActivo` = `$sys.func.IF("$session.params.competenciaAFortalecer = \"Cap…`, `estadoPDI` = `{"A1_70":null,"A2_70":null,"A3_70":null,"A1_20":null,"A2_20"…` (+2) · ⚡ Llama al webhook **Evaluación Desempeño 3 - IA Generativa Camila** [consultaWeb] — lectura
- siempre → página «02. Mensaje2»

### Página · 02.1. Conversacional Evaluacion Desempeño › 03. Contexto sobre fechas

**Al entrar, en este orden:**

1. 📌 Asigna: `fechaAnteriorCompromiso` = ``, `diasMinimosEntreAcciones` = `0`, `materialComplementario` = `$sys.func.TO_TEXT($sys.func.GET_FIELD($sys.func.TO_OBJECT($s…`
2. 🤖 Ejecuta el generador de IA **extractor_materiales** con `resumen` → guarda en `materialComplementario`
3. 🤖 Ejecuta el generador de IA **extractor_acciones** con `estado_pdi` → guarda en `accionesAceptadas`
4. 💬 Dice: «📅 Ahora vamos a definir las fechas de compromiso de tu plan de desarrollo. Ten en cuenta que el tiempo de ejecución del plan es de 6 meses, ya que tu próxima valoración será en enero de 2027. Revisar…»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `itemSolicitudFecha2` — @sys.any

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `itemSolicitudFecha1`, `itemPlanDeMejora`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «03.15 Generar JSON fechas»  
  ↳ 📌 asigna `pdiFechasParcial` = `[]`, `mensajeUsuarioFecha` = ``, `estadoFechas` = `INICIAL`, `mensajeGenerativoFechas` = `` (+6)

### Página · 02.1. Conversacional Evaluacion Desempeño › 03.1 Entregar item y solicitar Fecha

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `mensajeUsuarioFecha` — @sys.date · pregunta: «$session.params.mensajeGenerativoFechas»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «03.14 Validar fecha»  
  ↳ 📌 asigna `jsonFechas` = ``, `fechaUsuarioNormalizada` = `$sys.func.FORMAT_DATE($session.params.mensajeUsuarioFecha, "…`, `indiceFechaEsperado` = `$sys.func.TO_TEXT($sys.func.ADD($sys.func.TO_NUMBER($session…`

### Página · 02.1. Conversacional Evaluacion Desempeño › 03.14 Validar fecha

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.fechaUsuarioNormalizada < $session.params.fechaMinimaPDI` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeUsuarioFecha` = `` · 💬 dice «La fecha indicada es anterior al mínimo permitido. Por favor…»
- si `$session.params.fechaUsuarioNormalizada > $session.params.fechaMaximaPDI` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeUsuarioFecha` = `` · 💬 dice «La fecha indicada supera la fecha máxima permitida. Por favo…»
- si `$session.params.fechaMinimaSiguiente != null AND $session.params.fechaUsuarioNormalizada < $session.params.fechaMinimaSiguiente` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeUsuarioFecha` = ``, `fechaUsuarioNormalizada` = `` · 💬 dice «La fecha seleccionada está demasiado cerca de la fecha anter…»
- siempre → página «03.15 Generar JSON fechas»

### Página · 02.1. Conversacional Evaluacion Desempeño › 03.15 Generar JSON fechas

**Al entrar, en este orden:**

1. 🤖 Ejecuta el generador de IA **procesar_fechas_pdi** con `acciones`, `fecha_normalizada`, `indice_esperado`, `maxima`, `minima`, `parcial`, `total_esperado` → guarda en `jsonFechas`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «03.2 Procesar item y fecha»

### Página · 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha

**Al entrar, en este orden:**

1. 🤖 Ejecuta el generador de IA **extraer_json_fechas** con `jsonFechas` → guarda en `pdiFechasExtraidas`
2. 🤖 Ejecuta el generador de IA **extraer_accion_fecha_actual** con `jsonFechas` → guarda en `accionFechaActual`
3. 🤖 Ejecuta el generador de IA **extraer_contador_accion_fecha_actual** con `jsonFechas` → guarda en `contadorAccionFechaActual`
4. 🤖 Ejecuta el generador de IA **extraer_fecha_compromiso_actual** con `jsonFechas` → guarda en `fechaCompromisoActual`
5. 🤖 Ejecuta el generador de IA **extraer_fecha_valida** con `jsonFechas` → guarda en `fechaValida`
6. 🤖 Ejecuta el generador de IA **extraer_estado_fechas** con `json_fechas` → guarda en `estadoFechas`
7. 🤖 Ejecuta el generador de IA **extraer_mensaje_fechas** con `fecha_maxima`, `fecha_minima`, `json_fechas` → guarda en `mensajeGenerativoFechas`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.estadoFechas = "ERROR" OR $session.params.estadoFechas = "Lo siento pero no tengo información al respecto"` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeGenerativoFechas` = `No pude procesar la fecha en este momento. Por favor, escríb…`, `mensajeUsuarioFecha` = ``, `jsonFechas` = ``
- si `$session.params.fechaValida = "true" AND ($session.params.accionFechaActual = "VACIO" OR $session.params.fechaCompromisoActual = "VACIO" OR $session.params.contadorAccionFechaActual = "0")` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeGenerativoFechas` = `No pude validar completamente la fecha. Por favor, escríbela…`, `mensajeUsuarioFecha` = ``, `jsonFechas` = ``
- si `$session.params.estadoFechas = "REINTENTAR" AND $session.params.fechaValida = "false"` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeUsuarioFecha` = ``
- si `$session.params.estadoFechas = "PREGUNTAR" AND $session.params.fechaValida = "null"` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeUsuarioFecha` = ``, `pdiFechasParcial` = `$session.params.pdiFechasExtraidas`
- si `$session.params.estadoFechas = "PREGUNTAR" AND $session.params.fechaValida = "true" AND $session.params.accionFechaActual != "VACIO" AND $session.params.fechaCompromisoActual != "VACIO" AND $session.params.contadorAccionFechaActual != "0" AND $session.params.contadorAccionFechaActual = $session.params.indiceFechaEsperado AND $session.params.fechaCompromisoActual = $session.params.fechaUsuarioNormalizada` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeUsuarioFecha` = ``, `pdiFechasParcial` = `$session.params.pdiFechasExtraidas` · ✍️ Llama al webhook **Guardar Acción Plan Evaluación De Desempeño** [guardar] — **escritura, pide confirmación**
- si `$session.params.estadoFechas = "COMPLETO" AND $session.params.fechaValida = "true" AND $session.params.accionFechaActual != "VACIO" AND $session.params.fechaCompromisoActual != "VACIO" AND $session.params.contadorAccionFechaActual != "0" AND $session.params.contadorAccionFechaActual = $session.params.indiceFechaEsperado AND $session.params.fechaCompromisoActual = $session.params.fechaUsuarioNormalizada` → página «4. Consulta PDF plan de mejoramiento»  
  ↳ 📌 asigna `pdiFechasFinales` = `$session.params.pdiFechasExtraidas`, `pdiFechasParcial` = `$session.params.pdiFechasExtraidas`, `materialComplementario` = `$sys.func.TO_TEXT($sys.func.GET_FIELD($sys.func.TO_OBJECT($s…` · ✍️ Llama al webhook **Guardar Acción Plan Evaluación De Desempeño** [guardar] — **escritura, pide confirmación**
- si `$session.params.itemSolicitudFecha1 = CIERRE` → página «4. Consulta PDF plan de mejoramiento»

### Página · 02.1. Conversacional Evaluacion Desempeño › 05. Deseas generar otro plan?

**Al entrar, en este orden:**

1. 💬 Dice: «¡Excelente! Ya completaste tu plan de desarrollo para $session.params.competenciaAFortalecer. 🎉    También identificamos $session.params.competenciaAlternaPDI como otra competencia que puedes fortale…»
2. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → página «PDI - Generar mensaje inicial»  
  ↳ 📌 asigna `competenciaAFortalecer` = `$session.params.competenciaAlternaPDI`, `segundaCompetencia` = `true`, `contadorAccionFechaActual` = `0`, `indiceFechaEsperado` = `0` (+24)
- «No» (`no`) → flujo **12. ¿Algo Mas?**
💬 *Si el usuario escribe algo que coincide con:*
- intent «route_RespuestaAfirmativa» (ej. «Si quiero actualizar mis datos», «Si») → página «PDI - Generar mensaje inicial»  
  ↳ 📌 asigna `competenciaAFortalecer` = `$session.params.competenciaAlternaPDI`, `segundaCompetencia` = `true`, `contadorAccionFechaActual` = `0`, `indiceFechaEsperado` = `0` (+24)
- intent «route_RespuestaNegativa» (ej. «No, gracias», «No gracias») → flujo **12. ¿Algo Mas?**

### Página · 02.1. Conversacional Evaluacion Desempeño › 3.8

**Al entrar, en este orden:**

_No hace nada al entrar._

### Página · 02.1. Conversacional Evaluacion Desempeño › 4. Consulta PDF plan de mejoramiento

**Al entrar, en este orden:**

1. ⚡ Llama al webhook **UrlPdfEvaluacionDesempeño** [consultar] — lectura

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.urlPdfPlanMejoramiento != null` → página «4.1 Entrega Pdf Plan de Mejoramiento»

### Página · 02.1. Conversacional Evaluacion Desempeño › 4.1 Entrega Pdf Plan de Mejoramiento

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «PDI - Validar segunda competencia»

### Página · 02.1. Conversacional Evaluacion Desempeño › PDI - Actualizar estado

**Al entrar, en este orden:**

1. 🤖 Ejecuta el generador de IA **actualizar_estado_pdi** con `estado_anterior`, `mensaje_usuario`, `respuesta_camila` → guarda en `estadoPDIActualizado`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «PDI - Guardar estado»

### Página · 02.1. Conversacional Evaluacion Desempeño › PDI - Evaluar competencia a fortalecer

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.notaCompetenciaMenor1 < $session.params.notaCompetenciaMenor2` → página «PDI - Generar mensaje inicial»  
  ↳ 📌 asigna `competenciaAFortalecer` = `$session.params.competenciaMenor1`, `competenciaAlternaPDI` = `$session.params.competenciaMenor2`, `segundaCompetencia` = `false`
- si `$session.params.notaCompetenciaMenor1 > $session.params.notaCompetenciaMenor2` → página «PDI - Generar mensaje inicial»  
  ↳ 📌 asigna `competenciaAFortalecer` = `$session.params.competenciaMenor2`, `competenciaAlternaPDI` = `$session.params.competenciaMenor1`, `segundaCompetencia` = `false`
- si `$session.params.notaCompetenciaMenor1 = $session.params.notaCompetenciaMenor2` → página «PDI - Seleccionar competencia»  
  ↳ 📌 asigna `segundaCompetencia` = `false` · 💬 dice «De acuerdo con tus resultados, identificamos que  *$session.…»

### Página · 02.1. Conversacional Evaluacion Desempeño › PDI - Generar mensaje inicial

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «01. Mensaje1»  
  ↳ ⚡ Llama al webhook **Evaluación Desempeño 1 - IA Generativa Camila** [consultaWeb] — lectura

### Página · 02.1. Conversacional Evaluacion Desempeño › PDI - Guardar estado

**Al entrar, en este orden:**

1. 📌 Asigna: `estadoPDI` = `$session.params.estadoPDIActualizado`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «PDI validar DEFINIR FECHAS»  
  ↳ 📌 asigna `mensajeGenerativoCompetencia1` = `` · ⚡ Llama al webhook **Evaluación Desempeño 3 - IA Generativa Camila** [consultaWeb] — lectura

### Página · 02.1. Conversacional Evaluacion Desempeño › PDI - Ofrecer competencia alterna

**Al entrar, en este orden:**

_No hace nada al entrar._

### Página · 02.1. Conversacional Evaluacion Desempeño › PDI - Seleccionar competencia

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `mensajeSeleccionCompetencia` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mensajeSeleccionCompetencia != null` → página «PDI - Validar selección competencia»  
  ↳ 🤖 Ejecuta el generador de IA **validar** con `competencia1`, `competencia2`, `mensajeUsuario` → guarda en `resultadoSeleccionCompetencia`

### Página · 02.1. Conversacional Evaluacion Desempeño › PDI - Validar segunda competencia

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.segundaCompetencia = true` → flujo **12. ¿Algo Mas?**
- si `$session.params.segundaCompetencia = false` → página «05. Deseas generar otro plan?»

### Página · 02.1. Conversacional Evaluacion Desempeño › PDI - Validar selección competencia

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.resultadoSeleccionCompetencia = "SELECCION_1"` → página «PDI - Generar mensaje inicial»  
  ↳ 📌 asigna `competenciaAFortalecer` = `$session.params.competenciaMenor1`, `competenciaAlternaPDI` = `$session.params.competenciaMenor2`
- si `$session.params.resultadoSeleccionCompetencia = "SELECCION_2"` → página «PDI - Generar mensaje inicial»  
  ↳ 📌 asigna `competenciaAFortalecer` = `$session.params.competenciaMenor2`, `competenciaAlternaPDI` = `$session.params.competenciaMenor1`
- si `$session.params.resultadoSeleccionCompetencia = "INDECISO"` → página «PDI - Seleccionar competencia»  
  ↳ 📌 asigna `mensajeSeleccionCompetencia` = ``, `resultadoSeleccionCompetencia` = `` · 💬 dice «💬 No hay problema. Como ambas competencias tienen la misma …»
- si `$session.params.resultadoSeleccionCompetencia = "OTRA_COMPETENCIA"` → página «PDI - Seleccionar competencia»  
  ↳ 📌 asigna `mensajeSeleccionCompetencia` = ``, `resultadoSeleccionCompetencia` = `` · 💬 dice «Entiendo que mencionas otra competencia. Para este plan debe…»
- si `$session.params.resultadoSeleccionCompetencia = "AMBIGUA"` → página «PDI - Seleccionar competencia»  
  ↳ 📌 asigna `mensajeSeleccionCompetencia` = ``, `resultadoSeleccionCompetencia` = `` · 💬 dice «💬 Quiero asegurarme de entender cuál prefieres trabajar.   …»

### Página · 02.1. Conversacional Evaluacion Desempeño › PDI validar DEFINIR FECHAS

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mensajeGenerativoCompetencia1 = "DEFINIR FECHAS"` → página «03. Contexto sobre fechas»  
  ↳ 📌 asigna `materialComplementario` = `$sys.func.IF("$session.params.competenciaAFortalecer = \"Cap…`
- siempre → página «01.1 Evaluar si hay cierre de conversacion»

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

### Página · 03. Líder Misión › 01.1.Atributo Mayor Satisfaccion

**Al entrar, en este orden:**

1. 💬 Dice: «¿Qué atributo te genera mayor satisfacción y por qué?»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `mayorSatisfaccion` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mayorSatisfaccion != null` → página «01.2. Atributo A Fortalecer»

### Página · 03. Líder Misión › 01.2. Atributo A Fortalecer

**Al entrar, en este orden:**

1. 💬 Dice: «¡Que bien! Te felicito por tu desempeño 🙌, ahora ¿Qué atributo te gustaría fortalecer?»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `atributoAFortalecer` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.atributoAFortalecer != null` → página «01.3 . Sugerencia Atributos»

### Página · 03. Líder Misión › 01.3 . Sugerencia Atributos

**Al entrar, en este orden:**

1. 💬 Dice: «¡Muy bien! Actuar con humildad es reconocer qué debemos mejorar. Sigue adelante y crea tu plan de desarrollo por atributos para avanzar💪. De acuerdo con tus resultados las atributos a fortalecer son:»
2. 🔘 Ofrece los botones: «$session.params.competenciaMenor1LiderMision»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `mensajeGenerativoCompetencia1Lider`

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «$session.params.competenciaMenor1LiderMision» (`competenciaMenor1`) → página «01.4 CompetenciaMenor1»  
  ↳ ⚡ Llama al webhook **Lider Mision - IA Generativa Camila** [ConsultaWeb] — lectura

### Página · 03. Líder Misión › 01.4 CompetenciaMenor1

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.mensajeGenerativoCompetencia1Lider»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «03. ¿Algo Mas?»

### Página · 03. Líder Misión › 02. No Consultar Notas Lider Mision

**Al entrar, en este orden:**

1. 💬 Dice: «Esta bien, aunque ten presente que tu desempeño está en tus manos y es importante que consultes tus resultados con regularidad para comprender en qué punto estás 😊»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «03. ¿Algo Mas?»

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

## Flujo · 03.1. Conversacional Lider Misión

**Qué dice al empezar:** «$session.params.mensajeGenerativoCompetencia1»

**Webhooks que usa:** ⚡ Evaluación Desempeño 1 - IA Generativa Camila · ⚡ Evaluación Desempeño 2 - IA Generativa Camila · ⚡ Evaluación Desempeño 3 - IA Generativa Camila · ⚡ Evaluación Desempeño 4 - Contextualización fechas - IA Generativa Camila · ✍️ Guardar Acción Plan Evaluación De Desempeño · ⚡ UrlPdfEvaluacionDesempeño

**Pide al usuario:** `mensajeUsuario1` (01. Mensaje1), `mensajeUsuario1` (01.1 Evaluar si hay cierre de conversacion), `mensajeUsuario2` (02. Mensaje2), `itemSolicitudFecha2` (03. Contexto sobre fechas), `mensajeUsuarioFecha` (03.1 Entregar item y solicitar Fecha), `mensajeSeleccionCompetencia` (PDI - Seleccionar competencia)

**Páginas:**
- 01. Mensaje1 — ⚡ Evaluación Desempeño 1 - IA Generativa Camila · ⌨️ pide datos
- 01.1 Evaluar si hay cierre de conversacion — ⚡ Evaluación Desempeño 4 - Contextualización fechas - IA Generativa Camila · ⌨️ pide datos
- 01.2 Evaluar respuesta Prompt 1 — ⚡ Evaluación Desempeño 2 - IA Generativa Camila
- 02. Mensaje2 — ⚡ Evaluación Desempeño 2 - IA Generativa Camila · ⌨️ pide datos · 🤖
- 02.1 Evaluar respuesta Prompt 2 — ⚡ Evaluación Desempeño 3 - IA Generativa Camila
- 03. Contexto sobre fechas — ⌨️ pide datos · 🤖
- 03.1 Entregar item y solicitar Fecha — ⌨️ pide datos
- 03.14 Validar fecha 
- 03.15 Generar JSON fechas — 🤖
- 03.2 Procesar item y fecha — ✍️ Guardar Acción Plan Evaluación De Desempeño · 🤖
- 05. Deseas generar otro plan? 
- 3.8 
- 4. Consulta PDF plan de mejoramiento — ⚡ UrlPdfEvaluacionDesempeño
- 4.1 Entrega Pdf Plan de Mejoramiento 
- PDI - Actualizar estado — 🤖
- PDI - Evaluar competencia a fortalecer 
- PDI - Generar mensaje inicial — ⚡ Evaluación Desempeño 1 - IA Generativa Camila
- PDI - Guardar estado — ⚡ Evaluación Desempeño 3 - IA Generativa Camila
- PDI - Ofrecer competencia alterna 
- PDI - Seleccionar competencia — ⌨️ pide datos · 🤖
- PDI - Validar segunda competencia 
- PDI - Validar selección competencia 
- PDI validar DEFINIR FECHAS 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «PDI - Generar mensaje inicial»
- siempre → página «PDI - Evaluar competencia a fortalecer»  
  ↳ 📌 asigna `contador_acciones` = `0`, `total_acciones` = `6`
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 03.1. Conversacional Lider Misión › 01. Mensaje1

**Al entrar, en este orden:**

1. 📌 Asigna: `controlConversacional` = `false`, `mensajeUsuario1` = ``, `estadoPDI` = `{}`
2. 💬 Dice: «$session.params.mensajeGenerativoCompetencia1»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `mensajeUsuario1` — @sys.any

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `controlConversacional`, `urlPdfPlanMejoramiento`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mensajeUsuario1 != null AND $session.params.controlConversacional = true` → página «01.2 Evaluar respuesta Prompt 1»  
  ↳ ⚡ Llama al webhook **Evaluación Desempeño 1 - IA Generativa Camila** [ConsultaWeb] — lectura

### Página · 03.1. Conversacional Lider Misión › 01.1 Evaluar si hay cierre de conversacion

**Al entrar, en este orden:**

1. 📌 Asigna: `mensajeUsuario1` = ``
2. 💬 Dice: «$session.params.mensajeGenerativoCompetencia1»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `mensajeUsuario1` — @sys.any

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `contextoFechas`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mensajeGenerativoCompetencia1 = "DEFINIR FECHAS"` → página «03. Contexto sobre fechas»  
  ↳ ⚡ Llama al webhook **Evaluación Desempeño 4 - Contextualización fechas - IA Generativa Camila** [Consultaweb] — lectura
- si `$session.params.mensajeUsuario1 != null AND $session.params.mensajeGenerativoCompetencia1 != null AND $session.params.mensajeGenerativoCompetencia1 != "DEFINIR FECHAS"` → página «PDI - Actualizar estado»
- si `$session.params.mensajeGenerativoCompetencia1 != null AND $session.params.mensajeGenerativoCompetencia1 : "RESUMEN FINAL DEL PLAN DE DESARROLLO"` → se queda en la página  
  ↳ 📌 asigna `resumen_pdi_fijo` = `$session.params.mensajeGenerativoCompetencia1`

### Página · 03.1. Conversacional Lider Misión › 01.2 Evaluar respuesta Prompt 1

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mensajeGenerativoCompetencia1 = "INICIAR DIAGNOSTICO"` → página «02. Mensaje2»  
  ↳ 📌 asigna `etapaPrompt2` = `INICIAL`, `mensajeUsuario2` = `` · ⚡ Llama al webhook **Evaluación Desempeño 2 - IA Generativa Camila** [consultaWeb] — lectura
- siempre → página «01. Mensaje1»

### Página · 03.1. Conversacional Lider Misión › 02. Mensaje2

**Al entrar, en este orden:**

1. 📌 Asigna: `controlConversacional` = `false`, `mensajeUsuario2` = ``, `resumen_pdi_fijo` = `$if($session.params.mensajeGenerativoCompetencia1 CONTAINS "…`
2. 🤖 Ejecuta el generador de IA **extractor_materiales** con `resumen` → guarda en `materialComplementario`
3. 🤖 Ejecuta el generador de IA **extractor_acciones** con `estado_pdi` → guarda en `accionesAceptadas`
4. 💬 Dice: «$session.params.mensajeGenerativoCompetencia1»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `mensajeUsuario2` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mensajeUsuario2 != null AND $session.params.controlConversacional = true` → página «02.1 Evaluar respuesta Prompt 2»  
  ↳ 📌 asigna `resumen_pdi_fijo` = `$session.params.mensajeGenerativoCompetencia1`, `etapaPrompt2` = `DESAFIO` · ⚡ Llama al webhook **Evaluación Desempeño 2 - IA Generativa Camila** [ConsultaWeb] — lectura

### Página · 03.1. Conversacional Lider Misión › 02.1 Evaluar respuesta Prompt 2

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mensajeGenerativoCompetencia1 = "INICIAR PLAN"` → página «01.1 Evaluar si hay cierre de conversacion»  
  ↳ 📌 asigna `mensajeUsuario1` = ``, `desafioPDI` = `$session.params.mensajeUsuario2`, `catalogoPDIActivo` = `$sys.func.IF("$session.params.competenciaAFortalecer = \"Cap…`, `estadoPDI` = `{"A1_70":null,"A2_70":null,"A3_70":null,"A1_20":null,"A2_20"…` (+2) · ⚡ Llama al webhook **Evaluación Desempeño 3 - IA Generativa Camila** [consultaWeb] — lectura
- siempre → página «02. Mensaje2»

### Página · 03.1. Conversacional Lider Misión › 03. Contexto sobre fechas

**Al entrar, en este orden:**

1. 📌 Asigna: `fechaAnteriorCompromiso` = ``, `diasMinimosEntreAcciones` = `0`, `materialComplementario` = `$sys.func.TO_TEXT($sys.func.GET_FIELD($sys.func.TO_OBJECT($s…`
2. 🤖 Ejecuta el generador de IA **extractor_materiales** con `resumen` → guarda en `materialComplementario`
3. 🤖 Ejecuta el generador de IA **extractor_acciones** con `estado_pdi` → guarda en `accionesAceptadas`
4. 💬 Dice: «📅 Ahora vamos a definir las fechas de compromiso de tu plan de desarrollo. Ten en cuenta que el tiempo de ejecución del plan es de 6 meses, ya que tu próxima valoración será en enero de 2027. Revisar…»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `itemSolicitudFecha2` — @sys.any

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `itemSolicitudFecha1`, `itemPlanDeMejora`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «03.15 Generar JSON fechas»  
  ↳ 📌 asigna `pdiFechasParcial` = `[]`, `mensajeUsuarioFecha` = ``, `estadoFechas` = `INICIAL`, `mensajeGenerativoFechas` = `` (+6)

### Página · 03.1. Conversacional Lider Misión › 03.1 Entregar item y solicitar Fecha

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `mensajeUsuarioFecha` — @sys.date · pregunta: «$session.params.mensajeGenerativoFechas»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «03.14 Validar fecha»  
  ↳ 📌 asigna `jsonFechas` = ``, `fechaUsuarioNormalizada` = `$sys.func.FORMAT_DATE($session.params.mensajeUsuarioFecha, "…`, `indiceFechaEsperado` = `$sys.func.TO_TEXT($sys.func.ADD($sys.func.TO_NUMBER($session…`

### Página · 03.1. Conversacional Lider Misión › 03.14 Validar fecha

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.fechaUsuarioNormalizada < $session.params.fechaMinimaPDI` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeUsuarioFecha` = `` · 💬 dice «La fecha indicada es anterior al mínimo permitido. Por favor…»
- si `$session.params.fechaUsuarioNormalizada > $session.params.fechaMaximaPDI` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeUsuarioFecha` = `` · 💬 dice «La fecha indicada supera la fecha máxima permitida. Por favo…»
- si `$session.params.fechaMinimaSiguiente != null AND $session.params.fechaUsuarioNormalizada < $session.params.fechaMinimaSiguiente` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeUsuarioFecha` = ``, `fechaUsuarioNormalizada` = `` · 💬 dice «La fecha seleccionada está demasiado cerca de la fecha anter…»
- siempre → página «03.15 Generar JSON fechas»

### Página · 03.1. Conversacional Lider Misión › 03.15 Generar JSON fechas

**Al entrar, en este orden:**

1. 🤖 Ejecuta el generador de IA **procesar_fechas_pdi** con `acciones`, `fecha_normalizada`, `indice_esperado`, `maxima`, `minima`, `parcial`, `total_esperado` → guarda en `jsonFechas`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «03.2 Procesar item y fecha»

### Página · 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha

**Al entrar, en este orden:**

1. 🤖 Ejecuta el generador de IA **extraer_json_fechas** con `jsonFechas` → guarda en `pdiFechasExtraidas`
2. 🤖 Ejecuta el generador de IA **extraer_accion_fecha_actual** con `jsonFechas` → guarda en `accionFechaActual`
3. 🤖 Ejecuta el generador de IA **extraer_contador_accion_fecha_actual** con `jsonFechas` → guarda en `contadorAccionFechaActual`
4. 🤖 Ejecuta el generador de IA **extraer_fecha_compromiso_actual** con `jsonFechas` → guarda en `fechaCompromisoActual`
5. 🤖 Ejecuta el generador de IA **extraer_fecha_valida** con `jsonFechas` → guarda en `fechaValida`
6. 🤖 Ejecuta el generador de IA **extraer_estado_fechas** con `json_fechas` → guarda en `estadoFechas`
7. 🤖 Ejecuta el generador de IA **extraer_mensaje_fechas** con `fecha_maxima`, `fecha_minima`, `json_fechas` → guarda en `mensajeGenerativoFechas`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.estadoFechas = "ERROR" OR $session.params.estadoFechas = "Lo siento pero no tengo información al respecto"` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeGenerativoFechas` = `No pude procesar la fecha en este momento. Por favor, escríb…`, `mensajeUsuarioFecha` = ``, `jsonFechas` = ``
- si `$session.params.fechaValida = "true" AND ($session.params.accionFechaActual = "VACIO" OR $session.params.fechaCompromisoActual = "VACIO" OR $session.params.contadorAccionFechaActual = "0")` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeGenerativoFechas` = `No pude validar completamente la fecha. Por favor, escríbela…`, `mensajeUsuarioFecha` = ``, `jsonFechas` = ``
- si `$session.params.estadoFechas = "REINTENTAR" AND $session.params.fechaValida = "false"` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeUsuarioFecha` = ``
- si `$session.params.estadoFechas = "PREGUNTAR" AND $session.params.fechaValida = "null"` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeUsuarioFecha` = ``, `pdiFechasParcial` = `$session.params.pdiFechasExtraidas`
- si `$session.params.estadoFechas = "PREGUNTAR" AND $session.params.fechaValida = "true" AND $session.params.accionFechaActual != "VACIO" AND $session.params.fechaCompromisoActual != "VACIO" AND $session.params.contadorAccionFechaActual != "0" AND $session.params.contadorAccionFechaActual = $session.params.indiceFechaEsperado AND $session.params.fechaCompromisoActual = $session.params.fechaUsuarioNormalizada` → página «03.1 Entregar item y solicitar Fecha»  
  ↳ 📌 asigna `mensajeUsuarioFecha` = ``, `pdiFechasParcial` = `$session.params.pdiFechasExtraidas` · ✍️ Llama al webhook **Guardar Acción Plan Evaluación De Desempeño** [guardar] — **escritura, pide confirmación**
- si `$session.params.estadoFechas = "COMPLETO" AND $session.params.fechaValida = "true" AND $session.params.accionFechaActual != "VACIO" AND $session.params.fechaCompromisoActual != "VACIO" AND $session.params.contadorAccionFechaActual != "0" AND $session.params.contadorAccionFechaActual = $session.params.indiceFechaEsperado AND $session.params.fechaCompromisoActual = $session.params.fechaUsuarioNormalizada` → página «4. Consulta PDF plan de mejoramiento»  
  ↳ 📌 asigna `pdiFechasFinales` = `$session.params.pdiFechasExtraidas`, `pdiFechasParcial` = `$session.params.pdiFechasExtraidas`, `materialComplementario` = `$sys.func.TO_TEXT($sys.func.GET_FIELD($sys.func.TO_OBJECT($s…` · ✍️ Llama al webhook **Guardar Acción Plan Evaluación De Desempeño** [guardar] — **escritura, pide confirmación**
- si `$session.params.itemSolicitudFecha1 = CIERRE` → página «4. Consulta PDF plan de mejoramiento»

### Página · 03.1. Conversacional Lider Misión › 05. Deseas generar otro plan?

**Al entrar, en este orden:**

1. 💬 Dice: «¡Excelente! Ya completaste tu plan de desarrollo para $session.params.competenciaAFortalecer. 🎉    También identificamos $session.params.competenciaAlternaPDI como otra competencia que puedes fortale…»
2. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → página «PDI - Generar mensaje inicial»  
  ↳ 📌 asigna `competenciaAFortalecer` = `$session.params.competenciaAlternaPDI`, `segundaCompetencia` = `true`, `contadorAccionFechaActual` = `0`, `indiceFechaEsperado` = `0` (+24)
- «No» (`no`) → se queda en la página
💬 *Si el usuario escribe algo que coincide con:*
- intent «route_RespuestaAfirmativa» (ej. «Si quiero actualizar mis datos», «Si») → página «PDI - Generar mensaje inicial»  
  ↳ 📌 asigna `competenciaAFortalecer` = `$session.params.competenciaAlternaPDI`, `segundaCompetencia` = `true`, `contadorAccionFechaActual` = `0`, `indiceFechaEsperado` = `0` (+24)
- intent «route_RespuestaNegativa» (ej. «No, gracias», «No gracias») → se queda en la página

### Página · 03.1. Conversacional Lider Misión › 3.8

**Al entrar, en este orden:**

_No hace nada al entrar._

### Página · 03.1. Conversacional Lider Misión › 4. Consulta PDF plan de mejoramiento

**Al entrar, en este orden:**

1. ⚡ Llama al webhook **UrlPdfEvaluacionDesempeño** [consultar] — lectura

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.urlPdfPlanMejoramiento != null` → página «4.1 Entrega Pdf Plan de Mejoramiento»

### Página · 03.1. Conversacional Lider Misión › 4.1 Entrega Pdf Plan de Mejoramiento

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «PDI - Validar segunda competencia»

### Página · 03.1. Conversacional Lider Misión › PDI - Actualizar estado

**Al entrar, en este orden:**

1. 🤖 Ejecuta el generador de IA **actualizar_estado_pdi** con `estado_anterior`, `mensaje_usuario`, `respuesta_camila` → guarda en `estadoPDIActualizado`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «PDI - Guardar estado»

### Página · 03.1. Conversacional Lider Misión › PDI - Evaluar competencia a fortalecer

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.notaCompetenciaMenor1 < $session.params.notaCompetenciaMenor2` → página «PDI - Generar mensaje inicial»  
  ↳ 📌 asigna `competenciaAFortalecer` = `$session.params.competenciaMenor1`, `competenciaAlternaPDI` = `$session.params.competenciaMenor2`, `segundaCompetencia` = `false`
- si `$session.params.notaCompetenciaMenor1 > $session.params.notaCompetenciaMenor2` → página «PDI - Generar mensaje inicial»  
  ↳ 📌 asigna `competenciaAFortalecer` = `$session.params.competenciaMenor2`, `competenciaAlternaPDI` = `$session.params.competenciaMenor1`, `segundaCompetencia` = `false`
- si `$session.params.notaCompetenciaMenor1 = $session.params.notaCompetenciaMenor2` → página «PDI - Seleccionar competencia»  
  ↳ 📌 asigna `segundaCompetencia` = `false` · 💬 dice «De acuerdo con tus resultados, identificamos que  *$session.…»

### Página · 03.1. Conversacional Lider Misión › PDI - Generar mensaje inicial

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «01. Mensaje1»  
  ↳ ⚡ Llama al webhook **Evaluación Desempeño 1 - IA Generativa Camila** [consultaWeb] — lectura

### Página · 03.1. Conversacional Lider Misión › PDI - Guardar estado

**Al entrar, en este orden:**

1. 📌 Asigna: `estadoPDI` = `$session.params.estadoPDIActualizado`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «PDI validar DEFINIR FECHAS»  
  ↳ 📌 asigna `mensajeGenerativoCompetencia1` = `` · ⚡ Llama al webhook **Evaluación Desempeño 3 - IA Generativa Camila** [consultaWeb] — lectura

### Página · 03.1. Conversacional Lider Misión › PDI - Ofrecer competencia alterna

**Al entrar, en este orden:**

_No hace nada al entrar._

### Página · 03.1. Conversacional Lider Misión › PDI - Seleccionar competencia

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `mensajeSeleccionCompetencia` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mensajeSeleccionCompetencia != null` → página «PDI - Validar selección competencia»  
  ↳ 🤖 Ejecuta el generador de IA **validar** con `competencia1`, `competencia2`, `mensajeUsuario` → guarda en `resultadoSeleccionCompetencia`

### Página · 03.1. Conversacional Lider Misión › PDI - Validar segunda competencia

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.segundaCompetencia = true` → se queda en la página
- si `$session.params.segundaCompetencia = false` → página «05. Deseas generar otro plan?»

### Página · 03.1. Conversacional Lider Misión › PDI - Validar selección competencia

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.resultadoSeleccionCompetencia = "SELECCION_1"` → página «PDI - Generar mensaje inicial»  
  ↳ 📌 asigna `competenciaAFortalecer` = `$session.params.competenciaMenor1`, `competenciaAlternaPDI` = `$session.params.competenciaMenor2`
- si `$session.params.resultadoSeleccionCompetencia = "SELECCION_2"` → página «PDI - Generar mensaje inicial»  
  ↳ 📌 asigna `competenciaAFortalecer` = `$session.params.competenciaMenor2`, `competenciaAlternaPDI` = `$session.params.competenciaMenor1`
- si `$session.params.resultadoSeleccionCompetencia = "INDECISO"` → página «PDI - Seleccionar competencia»  
  ↳ 📌 asigna `mensajeSeleccionCompetencia` = ``, `resultadoSeleccionCompetencia` = `` · 💬 dice «💬 No hay problema. Como ambas competencias tienen la misma …»
- si `$session.params.resultadoSeleccionCompetencia = "OTRA_COMPETENCIA"` → página «PDI - Seleccionar competencia»  
  ↳ 📌 asigna `mensajeSeleccionCompetencia` = ``, `resultadoSeleccionCompetencia` = `` · 💬 dice «Entiendo que mencionas otra competencia. Para este plan debe…»
- si `$session.params.resultadoSeleccionCompetencia = "AMBIGUA"` → página «PDI - Seleccionar competencia»  
  ↳ 📌 asigna `mensajeSeleccionCompetencia` = ``, `resultadoSeleccionCompetencia` = `` · 💬 dice «💬 Quiero asegurarme de entender cuál prefieres trabajar.   …»

### Página · 03.1. Conversacional Lider Misión › PDI validar DEFINIR FECHAS

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.mensajeGenerativoCompetencia1 = "DEFINIR FECHAS"` → página «03. Contexto sobre fechas»  
  ↳ 📌 asigna `materialComplementario` = `$sys.func.IF("$session.params.competenciaAFortalecer = \"Cap…`
- siempre → página «01.1 Evaluar si hay cierre de conversacion»

## Flujo · 04. Referidos

**Qué dice al empezar:** «💼 ¿Cuál es el área de experiencia del referido?»

**Se llega desde:**
- Default Start Flow › 05.Menu_Principal ← evento `referidos`
- flujo (global) ← intent «route_Referidos»
- flujo Default Start Flow ← intent «route_Referidos»

**Webhooks que usa:** ✍️ Guardar_Referidos · ⚡ Obtener_Colaborador_Referidos

**Pide al usuario:** `documentoReferido` (01. Solicitar Referido), `nombreReferido` (01. Solicitar Referido), `correoReferido` (01. Solicitar Referido), `telefonoReferido` (01. Solicitar Referido)

**Páginas:**
- 00. Referidos 
- 01. Solicitar Referido — ⌨️ pide datos
- 01.1 Solicitar area de experiencia del referido 
- 01.2 Segunda lista de experiencias laborales 
- 02. ¿Otra área de experiencia? 
- 02.1. Otra área de experiencia 
- 02.2 Otra área de experiencia segunda lista 
- 03. Guardar Referido — ✍️ Guardar_Referidos
- 04. ¿Otro Referido? 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.referidos = true` → página «00. Referidos»  
  ↳ ⚡ Llama al webhook **Obtener_Colaborador_Referidos** [buscar-usuario] — lectura
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 04. Referidos › 00. Referidos

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.idUsuarioReferidos != null` → página «01. Solicitar Referido»  
  ↳ 💬 dice «Como colaborador de la Familia OS puedes referir a un famili…»
- si `$session.params.idUsuarioReferidos = null` → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true` · 💬 dice «Por ahora no te encuentras en la base para referir.»

### Página · 04. Referidos › 01. Solicitar Referido

**Al entrar, en este orden:**

1. 📌 Asigna: `referidos` = ``

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `documentoReferido` — @sys.any · pregunta: «🆔 Por favor, indícanos el número de documento del referido.»
- `nombreReferido` — @sys.any · pregunta: «👤 ¿Cuál es el nombre completo?»
- `correoReferido` — @sys.any · pregunta: «📧 ¿Cuál es el correo electrónico del referido?»
- `telefonoReferido` — @sys.any · pregunta: «📱 ¿Cuál es el número de teléfono del referido?»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `areaExperienciaReferido`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.documentoReferido != null AND $session.params.nombreReferido != null AND $session.params.correoReferido != null AND $session.params.telefonoReferido != null` → página «01.1 Solicitar area de experiencia del referido»

### Página · 04. Referidos › 01.1 Solicitar area de experiencia del referido

**Al entrar, en este orden:**

1. 💬 Dice: «💼 ¿Cuál es el área de experiencia del referido?»
2. 🔘 Ofrece los botones: «💼 Comercial/Ventas» · «🎧 Atención cliente» · «📋 Admin/Financiero» · «🚚 Logística/Prod.» · «💻 Tecnología/Sist.» · «🏥 Salud» · «📚 Educación»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- evento `siguiente_pagina` → página «01.2 Segunda lista de experiencias laborales»
- «💼 Comercial/Ventas» (`comercial_ventas`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Comercial / Ventas`
- «🎧 Atención cliente» (`atencion_cliente`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Atención al cliente`
- «📋 Admin/Financiero» (`administrativo`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Administrativo/Financiero`
- «🚚 Logística/Prod.» (`logistica_operaciones`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Logística/Produccion`
- «💻 Tecnología/Sist.» (`tecnologia_sistemas`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Tecnología / Sistemas`
- «🏥 Salud» (`salud`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Salud`
- «📚 Educación» (`educacion`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Salud`

### Página · 04. Referidos › 01.2 Segunda lista de experiencias laborales

**Al entrar, en este orden:**

1. 💬 Dice: «💼 ¿Cuál es el área de experiencia del referido?»
2. 🔘 Ofrece los botones: «🔧 Servicios generales» · «🏛️ Sector público» · «🚀 Independiente/Empr.» · «❌ Ninguno/Sin exp.» · «⬅️ Volver»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «🔧 Servicios generales» (`servicios_generales`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Servicios generales`
- «🏛️ Sector público» (`sector_publico`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Sector público`
- «🚀 Independiente/Empr.» (`independiente_emprendimiento`) → página «02. ¿Otra área de experiencia?»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Independiente / Emprendimiento`
- «❌ Ninguno/Sin exp.» (`ninguno_sin_experiencia`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `areaExperienciaReferido` = `Ninguno / Sin experiencia`
- «⬅️ Volver» (`volver_pagina`) → página «01.1 Solicitar area de experiencia del referido»

### Página · 04. Referidos › 02. ¿Otra área de experiencia?

**Al entrar, en este orden:**

1. 💬 Dice: «✏️ ¿$session.params.nombreReferido cuenta con otra área de experiencia?»
2. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → página «02.1. Otra área de experiencia»
- «No» (`no`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `No`

### Página · 04. Referidos › 02.1. Otra área de experiencia

**Al entrar, en este orden:**

1. 💬 Dice: «💼 ¿Cuál es el área extra de experiencia del referido?»
2. 🔘 Ofrece los botones: «💼 Comercial/Ventas» · «🎧 Atención cliente» · «📋 Admin/Financiero» · «🚚 Logística/Prod.» · «💻 Tecnología/Sist.» · «🏥 Salud» · «📚 Educación»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `otraAreaExperienciaReferido`

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «💼 Comercial/Ventas» (`comercial_ventas`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Comercial / Ventas`
- «🎧 Atención cliente» (`atencion_cliente`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Atención al cliente`
- «📋 Admin/Financiero» (`administrativo`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Administrativo/Financiero`
- «🚚 Logística/Prod.» (`logistica_operaciones`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Logística / Produccion`
- «💻 Tecnología/Sist.» (`tecnologia_sistemas`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Tecnología / Sistemas`
- «🏥 Salud» (`salud`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Salud`
- «📚 Educación» (`educacion`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Educación`
- evento `siguiente_pagina` → página «02.2 Otra área de experiencia segunda lista»

### Página · 04. Referidos › 02.2 Otra área de experiencia segunda lista

**Al entrar, en este orden:**

1. 💬 Dice: «💼 ¿Cuál es el área extra de experiencia del referido?»
2. 🔘 Ofrece los botones: «🔧 Servicios generales» · «🏛️ Sector público» · «🚀 Independiente/Empr.» · «⬅️ Volver»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «🔧 Servicios generales» (`servicios_generales`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Servicios generales`
- «🏛️ Sector público» (`sector_publico`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Sector público`
- «🚀 Independiente/Empr.» (`independiente_emprendimiento`) → página «03. Guardar Referido»  
  ↳ 📌 asigna `otraAreaExperienciaReferido` = `Independiente / Emprendimiento`
- «⬅️ Volver» (`volver_pagina`) → página «02.1. Otra área de experiencia»

### Página · 04. Referidos › 03. Guardar Referido

**Al entrar, en este orden:**

1. ✍️ Llama al webhook **Guardar_Referidos** [guardar] — **escritura, pide confirmación**
2. 💬 Dice: «Gracias por compartir la información. ✅  Hemos registrado el referido correctamente.»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «04. ¿Otro Referido?»

### Página · 04. Referidos › 04. ¿Otro Referido?

**Al entrar, en este orden:**

1. 💬 Dice: «¿Deseas referir a otra persona? 🤓»
2. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → página «01. Solicitar Referido»  
  ↳ 📌 asigna `documentoReferido` = ``, `nombreReferido` = ``, `correoReferido` = ``, `telefonoReferido` = `` (+2)
- «No» (`no`) → flujo **12. ¿Algo Mas?**  
  ↳ 📌 asigna `documentoReferido` = ``, `nombreReferido` = ``, `correoReferido` = ``, `telefonoReferido` = `` (+2)

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

### Página · 05. Reconoser › 01.3. Mostrar resultados Reconoser

**Al entrar, en este orden:**

1. 💬 Dice: «Perfecto, estos son tus resultados:»
2. 💬 Dice: «✅ $session.params.indicador1 $session.params.resultado1 % ✅ $session.params.indicador2 $session.params.resultado2 % ✅ $session.params.indicador3 $session.params.resultado3 % ✅ $session.params.indicado…»

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

### Página · 05. Reconoser › 02.NoConocerIndicadoresReconoser

**Al entrar, en este orden:**

1. 💬 Dice: «Entiendo, no te preocupes. Aunque recuerda que es importante conocer muy bien el programa y sus políticas para que puedas disfrutar de todos sus beneficios.»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → flujo **12. ¿Algo Mas?**

## Flujo · 06. Mi proceso De Selección

**Se llega desde:**
- Default Start Flow › 04.Ingreso_Existoso ← condición
- Default Start Flow › 05.Menu_Principal ← evento `seleccion`
- flujo (global) ← intent «route_ProcesoDeSeleccion»
- flujo Default Start Flow ← intent «route_ProcesoDeSeleccion»

**Webhooks que usa:** ⚡ Extraer información candidato de AIRA · ⚡ Extraer información candidato de W · ✍️ Finalizar Conversacion

**Páginas:**
- 00. Definicion de variables para consultas 
- 01. Mi_Proceso_De_Seleccion_AIRA — ⚡ Extraer información candidato de AIRA
- 02. Mi_Proceso_De_Seleccion_W — ⚡ Extraer información candidato de W
- 03. Definición de resultados globales 
- 04. Definición de resultados Aira 
- 05. Definición de resultados W 
- 06. Frases estado y fase W 
- 07. Frases estado AIRA 
- 08. Cierre flujo — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.seleccion = true` → página «00. Definicion de variables para consultas»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 06. Mi proceso De Selección › 00. Definicion de variables para consultas

**Al entrar, en este orden:**

_No hace nada al entrar._

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `estadoW`, `faseW`, `estadoAira`, `globalAira`, `globalW`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «01. Mi_Proceso_De_Seleccion_AIRA»

### Página · 06. Mi proceso De Selección › 01. Mi_Proceso_De_Seleccion_AIRA

**Al entrar, en este orden:**

1. ⚡ Llama al webhook **Extraer información candidato de AIRA** [ConsultaCandidatoAIRA] — lectura

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «02. Mi_Proceso_De_Seleccion_W»
🔘 *Si el usuario pulsa un botón:*
- evento `sys.webhook-error` → página «02. Mi_Proceso_De_Seleccion_W»

### Página · 06. Mi proceso De Selección › 02. Mi_Proceso_De_Seleccion_W

**Al entrar, en este orden:**

1. ⚡ Llama al webhook **Extraer información candidato de W** [https://default140443af8e794fceb3edde00131298.4f.environment.api.powerplatform.com/powerautomate/automations/direct/workflows/6593822f883f4cdc87c79560e93e5f22/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=-jHB9R2eCIWAjMPQ9z6dKoDv8xgfGfeKuySL1akSZXA] — lectura

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «03. Definición de resultados globales»

### Página · 06. Mi proceso De Selección › 03. Definición de resultados globales

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.estadoW != null AND $session.params.estadoW != "NULL"` → página «06. Frases estado y fase W»
- si `$session.params.estadoAira != null` → página «07. Frases estado AIRA»
- si `$session.params.estadoW = null AND $session.params.estadoAira = null` → página «08. Cierre flujo»  
  ↳ 💬 dice «No encontramos un proceso de selección activo asociado a tu …»

### Página · 06. Mi proceso De Selección › 04. Definición de resultados Aira

**Al entrar, en este orden:**

_No hace nada al entrar._

### Página · 06. Mi proceso De Selección › 05. Definición de resultados W

**Al entrar, en este orden:**

_No hace nada al entrar._

### Página · 06. Mi proceso De Selección › 06. Frases estado y fase W

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.faseW = "Capacitacion" AND $session.params.estadoW = "Agrupado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Bienvenido/a a Formación Outsourcing!   Nos alegra acompaña…»
- si `$session.params.faseW = "Decision" AND $session.params.estadoW = "Pendiente"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Hola! Actualmente nuestra área de selección se encuentra re…»
- si `$session.params.faseW = "Decision" AND $session.params.estadoW = "Rechazado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Hola! Actualmente nuestra área de selección se encuentra re…»
- si `$session.params.faseW = "Examen Medico" AND ($session.params.estadoW = "Examen Medico" OR $session.params.estadoW = "Programado")` → página «08. Cierre flujo»  
  ↳ 💬 dice «Actualmente estás en la etapa de examenes médicos (pendiente…»
- si `$session.params.faseW = "Examen Medico" AND $session.params.estadoW = "SST"` → página «08. Cierre flujo»  
  ↳ 💬 dice «Actualmente estás en la etapa de examenes médicos (en progra…»
- si `$session.params.faseW = "Examen Medico" AND $session.params.estadoW = "Rechazado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «Gracias por tu interés en formar parte de Outsourcing S.A.S.…»
- si `$session.params.faseW = "Capacitacion" AND $session.params.estadoW = "Capacitacion"` → página «08. Cierre flujo»  
  ↳ 💬 dice «Muy pronto iniciarás tu proceso de formación. En un tiempo m…»
- si `$session.params.faseW = "Capacitacion" AND $session.params.estadoW = "Evaluado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Has concluido tu etapa formativa! Nos enorgullece que hayas…»
- si `$session.params.faseW = "Capacitacion" AND $session.params.estadoW = "Rechazado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «Queremos agradecerte por el compromiso y la disposición que …»
- si `$session.params.faseW = "Contratacion" AND $session.params.estadoW = "Generado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Has sido seleccionado! Muy pronto el área de contratación t…»
- si `$session.params.faseW = "Contratacion" AND $session.params.estadoW = "Contratacion"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Has sido seleccionado! Muy pronto el área de contratación t…»
- si `$session.params.faseW = "Contratacion" AND $session.params.estadoW = "Enviado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Hola!  Hemos enviado tu contrato al correo electrónico regi…»
- si `$session.params.faseW = "Contratacion" AND $session.params.estadoW = "Contratado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Felicitaciones! Te damos la bienvenida a Outsourcing S.A.S.…»
- si `$session.params.faseW = "Contratacion" AND $session.params.estadoW = "Rechazado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «Hola, nuestro equipo de selección se pondrá en contacto cont…»
- siempre → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Hola! Actualmente nuestra área de selección se encuentra re…»

### Página · 06. Mi proceso De Selección › 07. Frases estado AIRA

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.estadoAira = "Pruebas"` → página «08. Cierre flujo»  
  ↳ 💬 dice «Has avanzado a la etapa de pruebas Psicotécnicas. Revisa tu …»
- si `$session.params.estadoAira = "Entrevista"` → página «08. Cierre flujo»  
  ↳ 💬 dice «Tus pruebas psicotécnicas tuvieron buenos resultados; avanza…»
- si `$session.params.estadoAira = "Antecedentes"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Gracias por postularte! En este momento estamos revisando s…»
- si `$session.params.estadoAira = "Contratado"` → página «08. Cierre flujo»  
  ↳ 💬 dice «¡Felicitaciones! 🎉 Tu proceso de selección ha finalizado co…»

### Página · 06. Mi proceso De Selección › 08. Cierre flujo

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

## Flujo · 07. Actualizacion de datos Datix

**Qué dice al empezar:** «Datix es nuestra plataforma de registro y actualización de datos diseñada para mejorar tu experiencia en OS»

**Se llega desde:**
- 13. Usuario inexistente/ inactivo › 0.3 Usuario inactivo ← evento `sys.no-match-default`
- Default Start Flow › 05.Menu_Principal ← evento `actualizacion_datos`
- flujo (global) ← intent «route_ActualizarDatosDatix»
- flujo Default Start Flow ← intent «route_ActualizarDatosDatix»

**Webhooks que usa:** ✍️ Finalizar Conversacion · ⚡ IA Generativa Camila - General

**Páginas:**
- 00. Actualizar Datos — ⚡ IA Generativa Camila - General
- 02. Enlace Datix 
- 03. Recordatorio OS 
- 04. Algo Mas 
- 05. Despedida — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.actualizarDatos = true` → página «00. Actualizar Datos»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 07. Actualizacion de datos Datix › 00. Actualizar Datos

**Al entrar, en este orden:**

1. 💬 Dice: «Datix es nuestra plataforma de registro y actualización de datos diseñada para mejorar tu experiencia en OS»
2. 💬 Dice: «¿Te gustaría revisar y actualizar tus datos? 📈»
3. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → página «02. Enlace Datix»
- «No» (`no`) → página «03. Recordatorio OS»
💬 *Si el usuario escribe algo que coincide con:*
- intent «route_RespuestaAfirmativa» (ej. «Si quiero actualizar mis datos», «Si») → página «02. Enlace Datix»  
  ↳ 📌 asigna `consultaGenerativa` = `Básate en esta respuesta "Puedes actualizar en la siguiente …` · ⚡ Llama al webhook **IA Generativa Camila - General** [ConsultaWeb] — lectura
- intent «route_RespuestaNegativa» (ej. «No, gracias», «No gracias») → página «03. Recordatorio OS»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 📌 asigna `consultaGenerativa` = `El usuario respondió algo que no fue claramente un Sí o un N…` · ⚡ Llama al webhook **IA Generativa Camila - General** [ConsultaWeb] — lectura · 💬 dice «$session.params.mensajeGenerativoGlobal»

### Página · 07. Actualizacion de datos Datix › 02. Enlace Datix

**Al entrar, en este orden:**

1. 💬 Dice: «Actualiza en el siguiente enlace. 👉»
2. 💬 Dice: «https://bit.ly/Datix»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «04. Algo Mas»

### Página · 07. Actualizacion de datos Datix › 03. Recordatorio OS

**Al entrar, en este orden:**

1. 💬 Dice: «Está bien👌, aunque recuerda que como colaborador es indispensable que actualices tus datos para estar conectados y hacer parte de todos los beneficios y actividades de nuestra compañía.»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «04. Algo Mas»

### Página · 07. Actualizacion de datos Datix › 04. Algo Mas

**Al entrar, en este orden:**

1. 📌 Asigna: `actualizarDatosDatix` = ``, `consultaGenerativa` = ``, `mensajeGenerativoGlobal` = ``
2. 💬 Dice: «¿Algo más en lo que pueda ayudarte?»
3. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → se queda en la página
🔘 *Si el usuario pulsa un botón:*
- «Si» (`Si`) → flujo **Default Start Flow**  
  ↳ 📌 asigna `menu` = `true`, `actualizarDatosDatix` = `false`
- «No» (`No`) → página «05. Despedida»

### Página · 07. Actualizacion de datos Datix › 05. Despedida

**Al entrar, en este orden:**

1. 💬 Dice: «¡Gracias por actualizar tus datos!  Recuerda que en Outsourcing nos mueve el propósito de Mejorar Vidas, nuestro Compromiso.❤️»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `cierre`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ 📌 asigna `cierre` = `true` · ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

## Flujo · 08.Te escuchamos

**Qué dice al empezar:** «Este es un espacio para que compartas tus opiniones, sugerencias o comentarios de forma escrita y contribuyas a nuestro propósito de Mejorar Vidas.    ✍️ Déjano…»

**Se llega desde:**
- Default Start Flow › 05.Menu_Principal ← evento `te_escuchamos`
- flujo (global) ← intent «route_TeEscuchamos»
- flujo Default Start Flow ← intent «route_TeEscuchamos»

**Webhooks que usa:** ✍️ Guardar Comentario Te Escuchamos · ⚡ IA Generativa Camila - General

**Pide al usuario:** `comentario` (04. Capturar comentario)

**Páginas:**
- 00. Dejar algun comentario 
- 01. Decision Dejar algun comentario 
- 02. Algo mas 
- 03. Capturar datos 
- 04. Capturar comentario — ⚡ IA Generativa Camila - General · ⌨️ pide datos
- 04.1 Responder comentario — ✍️ Guardar Comentario Te Escuchamos
- 05. Despedida 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.teEscuchamos = true` → página «00. Dejar algun comentario»  
  ↳ ⚡ Llama al webhook **IA Generativa Camila - General** [consultaWeb] — lectura
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 08.Te escuchamos › 00. Dejar algun comentario

**Al entrar, en este orden:**

1. 💬 Dice: «Este es un espacio para que compartas tus opiniones, sugerencias o comentarios de forma escrita y contribuyas a nuestro propósito de Mejorar Vidas.    ✍️ Déjanos tu mensaje por escrito y ayúdanos a se…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «01. Decision Dejar algun comentario»

### Página · 08.Te escuchamos › 01. Decision Dejar algun comentario

**Al entrar, en este orden:**

1. 📌 Asigna: `nombreDatix` = ``, `numeroDocumentoDatix` = ``, `comentario` = ``
2. 💬 Dice: «¿Quieres dejarnos algún comentario u opinión? 😀»
3. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «No» (`No`) → página «02. Algo mas»
- «Si» (`Si`) → página «03. Capturar datos»

### Página · 08.Te escuchamos › 02. Algo mas

**Al entrar, en este orden:**

1. 📌 Asigna: `teEscuchamos` = ``, `consultaGenerativa` = ``, `mensajeGenerativoGlobal` = ``
2. 💬 Dice: «¿Algo más en lo que pueda ayudarte?»
3. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`Si`) → flujo **Default Start Flow**  
  ↳ 📌 asigna `menu` = `true`
- «No» (`No`) → página «05. Despedida»

### Página · 08.Te escuchamos › 03. Capturar datos

**Al entrar, en este orden:**

1. 💬 Dice: «¿Qué comentario, opinión o sugerencia te gustaría compartir?✍️»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «04. Capturar comentario»

### Página · 08.Te escuchamos › 04. Capturar comentario

**Al entrar, en este orden:**

1. 📌 Asigna: `consultaGenerativa` = `Vas a basarte en la respuesta que nos genera la persona la c…`

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `comentario` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.comentario != null` → página «04.1 Responder comentario»  
  ↳ ⚡ Llama al webhook **IA Generativa Camila - General** [ConsultaWeb] — lectura

### Página · 08.Te escuchamos › 04.1 Responder comentario

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.mensajeGenerativoGlobal»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «02. Algo mas»  
  ↳ ✍️ Llama al webhook **Guardar Comentario Te Escuchamos** [GuardarTeEscuchamos] — **escritura, pide confirmación**

### Página · 08.Te escuchamos › 05. Despedida

**Al entrar, en este orden:**

_No hace nada al entrar._

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `cierre`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true`

## Flujo · 09.Convenios

**Qué dice al empezar:** «Los convenios son alianzas estratégicas que realizamos para ampliar el portafolio de beneficios de la familia OS. 😃»

**Se llega desde:**
- Default Start Flow › 05.Menu_Principal ← evento `convenios`
- flujo (global) ← intent «route_Convenios»
- flujo Default Start Flow ← intent «route_Convenios»

**Webhooks que usa:** ⚡ IA Generativa Camila - General

**Páginas:**
- 00. Alianzas estrategicas 
- 01. Decisión conocer conveniOS — ⚡ IA Generativa Camila - General
- 02. Enlace de conveniOS 
- 03. Algo mas 
- 04. Despedida 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.convenios = true` → página «00. Alianzas estrategicas»  
  ↳ 📌 asigna `intencion` = `route_Convenios` · ⚡ Llama al webhook **IA Generativa Camila - General** [consultaWeb] — lectura
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 09.Convenios › 00. Alianzas estrategicas

**Al entrar, en este orden:**

1. 💬 Dice: «Los convenios son alianzas estratégicas que realizamos para ampliar el portafolio de beneficios de la familia OS. 😃»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «01. Decisión conocer conveniOS»

### Página · 09.Convenios › 01. Decisión conocer conveniOS

**Al entrar, en este orden:**

1. 💬 Dice: «¿Quieres conocer nuestros convenios?. 🤩»
2. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`Si`) → página «02. Enlace de conveniOS»  
  ↳ 📌 asigna `consultaGenerativa` = `Debes dar el contexto de todos los convenios que manejamos s…` · ⚡ Llama al webhook **IA Generativa Camila - General** [ConsultaWeb] — lectura
- «No» (`No`) → página «03. Algo mas»

### Página · 09.Convenios › 02. Enlace de conveniOS

**Al entrar, en este orden:**

1. 💬 Dice: «¡Conoce nuestros convenios! Revísalos en la imagen que aparece a continuación 👇»
2. 💬 Muestra la tarjeta «Producto destacado»
3. 🖼️ Muestra una imagen https://fine360.outsourcing.com.co:9876/img/convenios-outsourcing-v2.jpg

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «03. Algo mas»

### Página · 09.Convenios › 03. Algo mas

**Al entrar, en este orden:**

1. 📌 Asigna: `convenios` = `false`, `consultaGenerativa` = ``, `mensajeGenerativoGlobal` = ``
2. 💬 Dice: «¿Algo más en lo que pueda ayudarte?»
3. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`Si`) → flujo **Default Start Flow**  
  ↳ 📌 asigna `menu` = `true`, `convenios` = `false`
- «No» (`No`) → página «04. Despedida»

### Página · 09.Convenios › 04. Despedida

**Al entrar, en este orden:**

_No hace nada al entrar._

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `cierre`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true`

## Flujo · 10. Cierre de conversación

**Qué dice al empezar:** «Gracias por tu tiempo. ¡Conversamos en una próxima oportunidad! Recuerda que en OUTSOURCING nos mueve el propósito de Mejorar Vidas. Nuestro Compromiso. ❤️»

**Se llega desde:**
- 01. Indicadores De Calidad › 002.UsuarioNoExiste ← condición
- 01. Indicadores De Calidad › 003.UsuarioInactivo ← condición
- 01. Indicadores De Calidad › 06. ¿Algo mas? ← evento `no`
- 02. Mi Evaluación De Desempeño › 03. ¿Algo mas? ← evento `no`
- 03. Líder Misión › 03. ¿Algo Mas? ← evento `no`
- 04. Referidos › 00. Referidos ← condición
- 08.Te escuchamos › 05. Despedida ← siempre
- 09.Convenios › 04. Despedida ← siempre
- … y 5 más

**Webhooks que usa:** ✍️ Finalizar Conversacion

**Páginas:**
- 01. Cerrar Conversacion — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.cierre = true` → página «01. Cerrar Conversacion»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 10. Cierre de conversación › 01. Cerrar Conversacion

**Al entrar, en este orden:**

1. 💬 Dice: «Gracias por tu tiempo. ¡Conversamos en una próxima oportunidad! Recuerda que en OUTSOURCING nos mueve el propósito de Mejorar Vidas. Nuestro Compromiso. ❤️»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.cierre = true` → 🔚 **termina la conversación**  
  ↳ 📌 asigna `cierre` = `false` · ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

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

### Página · 11. No Detecta › 00. Irespondible

**Al entrar, en este orden:**

1. 📌 Asigna: `consultaGenerativa` = `Basándote en esta frase y continuando con la conversación "R…`
2. ⚡ Llama al webhook **IA Generativa Camila - General** [consultaWeb] — lectura
3. 💬 Dice: «$session.params.mensajeGenerativoGlobal»

### Página · 11. No Detecta › 01.Menu

**Al entrar, en este orden:**

_No hace nada al entrar._

## Flujo · 12. ¿Algo Mas?

**Qué dice al empezar:** «¿Algo más en lo que pueda ayudarte?»

**Se llega desde:**
- 02. Mi Evaluación De Desempeño › 01. Resultados Evaluación De Desempeño ← condición
- 02. Mi Evaluación De Desempeño › 02. No Resultados Evaluación De Desempeño ← condición
- 02.1. Conversacional Evaluacion Desempeño › 05. Deseas generar otro plan? ← evento `no`
- 02.1. Conversacional Evaluacion Desempeño › 05. Deseas generar otro plan? ← intent «route_RespuestaNegativa»
- 02.1. Conversacional Evaluacion Desempeño › PDI - Validar segunda competencia ← condición
- 04. Referidos › 04. ¿Otro Referido? ← evento `no`
- 05. Reconoser › 02.NoConocerIndicadoresReconoser ← condición
- 16.1 Pagos › 1. Dudas Pagos ← siempre
- … y 23 más

**Páginas:**
- 01. ¿Algo mas? 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «01. ¿Algo mas?»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 12. ¿Algo Mas? › 01. ¿Algo mas?

**Al entrar, en este orden:**

1. 📌 Asigna: `intencion` = ``
2. 💬 Dice: «¿Algo más en lo que pueda ayudarte?»
3. 🔘 Ofrece los botones: «Si» · «No»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Si» (`si`) → flujo **Default Start Flow**  
  ↳ 📌 asigna `menu` = `true`, `desempeno` = `false`, `liderMision` = `false`, `referidos` = `false` (+1)
- «No» (`no`) → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true`
💬 *Si el usuario escribe algo que coincide con:*
- intent «route_RespuestaAfirmativa» (ej. «Si quiero actualizar mis datos», «Si») → flujo **Default Start Flow**  
  ↳ 📌 asigna `menu` = `true`, `desempeno` = `false`, `liderMision` = `false`, `referidos` = `false` (+1)
- intent «route_RespuestaNegativa» (ej. «No, gracias», «No gracias») → flujo **10. Cierre de conversación**  
  ↳ 📌 asigna `cierre` = `true`

## Flujo · 13. Usuario inexistente/ inactivo

**Qué dice al empezar:** «Lo sentimos, el número de identificación ingresado no se encuentra registrado en nuestra base de datos. Recuerda que debes ser un colaborador OS activo para pod…»

**Se llega desde:**
- 01. Indicadores De Calidad › 00.ValidacionUsuarioKairos ← condición

**Webhooks que usa:** ⚡ Obtener_Usuario_Por_Cedula

**Pide al usuario:** `Numero_Documento` (00.1 Solicitar numero de documento), `placa_carro` (00.1 Solicitar numero de documento)

**Páginas:**
- 0.1 condicional documento 
- 0.2 Usuario no registrado 
- 0.3 Usuario inactivo 
- 00.1 Solicitar numero de documento — ⚡ Obtener_Usuario_Por_Cedula · ⌨️ pide datos

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «00.1 Solicitar numero de documento»
💬 *Si el usuario escribe algo que coincide con:*
- intent «Default Welcome Intent» (ej. «oa», «ola») → página «00.1 Solicitar numero de documento»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 13. Usuario inexistente/ inactivo › 0.1 condicional documento

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.Numero_Documento != null AND $session.params.usuarioNoExiste = true` → página «0.2 Usuario no registrado»
- si `$session.params.usuarioInactivo = true AND $session.params.Numero_Documento != null` → página «0.3 Usuario inactivo»
- si `$session.params.existe = true AND $session.params.activo = true` → flujo **01. Indicadores De Calidad**

### Página · 13. Usuario inexistente/ inactivo › 0.2 Usuario no registrado

**Al entrar, en este orden:**

1. 💬 Dice: «Lo sentimos, el número de identificación ingresado no se encuentra registrado en nuestra base de datos. Recuerda que debes ser un colaborador OS activo para poder continuar con esta conversación ✅»
2. 🔘 Ofrece los botones: «Ir a DATIX» · «Intentar con otra cédula»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Intentar con otra cédula» (`reintentar_cedula`) → página «00.1 Solicitar numero de documento»  
  ↳ 📌 asigna `Numero_Documento` = ``
- evento `sys.no-match-1` → flujo **10. Cierre de conversación**  
  ↳ 💬 dice «Si necesitas una ayuda mas especifica por favor comunicate c…»

### Página · 13. Usuario inexistente/ inactivo › 0.3 Usuario inactivo

**Al entrar, en este orden:**

1. 💬 Dice: «Si eres colaborador OS activo y consideras que existe un error, ingresa a DATIX para validar y actualizar tu información en nuestra base de datos 👇»
2. 🔘 Ofrece los botones: «Ir a DATIX» · «Intentar con otra cédula»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Intentar con otra cédula» (`reintentar_cedula`) → página «00.1 Solicitar numero de documento»  
  ↳ 📌 asigna `Numero_Documento` = ``
- ❔ *Sin coincidencia* → flujo **07. Actualizacion de datos Datix**  
  ↳ 💬 dice «Recuerda que puedes actualizar tus datos en plataforma datix…»

### Página · 13. Usuario inexistente/ inactivo › 00.1 Solicitar numero de documento

**Al entrar, en este orden:**

1. 📌 Asigna: `Numero_Documento` = ``, `placa_carro` = ``
2. 💬 Dice: «Tu número de cédula no es válido o no se encuentra activo en nuestra base de datos 😓»
3. 💬 Dice: «Por favor verifícalo y escríbelo nuevamente sin puntos ni comas»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `Numero_Documento` — @Numero_Documento
- `placa_carro` — @placa_carro

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.Numero_Documento != null` → página «0.1 condicional documento»  
  ↳ ⚡ Llama al webhook **Obtener_Usuario_Por_Cedula** [ObtenerUsuarioPorCedula] — lectura

## Flujo · 14. Reportar Retiro

**Qué dice al empezar:** «¡Gracias por la información! Estaré enviando la encuesta de retiro al colaborador de tu equipo.»

**Se llega desde:**
- Default Start Flow › 05.Menu_Principal ← evento `reportarRetiro`

**Webhooks que usa:** ✍️ Envio HSM Encuesta de Retiro Camila · ✍️ Finalizar Conversacion · ✍️ Guardar Datos Colaborador Encuesta De Retiro

**Pide al usuario:** `retiroNombreColaborador` (01. Solicitar datos colaborador), `retiroNumeroTelColaborador` (01. Solicitar datos colaborador), `retiroNumeroDocumentoColaborador` (01. Solicitar datos colaborador)

**Páginas:**
- 01. Solicitar datos colaborador — ⌨️ pide datos
- 02. Guardar Datos Colaborador Retirado — ✍️ Guardar Datos Colaborador Encuesta De Retiro
- 03. Ejecutar HSM Camila Envió de Encuesta — ✍️ Envio HSM Encuesta de Retiro Camila · ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «01. Solicitar datos colaborador»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 14. Reportar Retiro › 01. Solicitar datos colaborador

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `retiroNombreColaborador` — @sys.any · pregunta: «Ingresa el nombre del colaborador»
- `retiroNumeroTelColaborador` — @sys.any · pregunta: «Ingresa el número de celular del colaborador»
- `retiroNumeroDocumentoColaborador` — @sys.any · pregunta: «Ingresa el número de identificación del colaborador»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.retiroNombreColaborador != null AND $session.params.retiroNumeroTelColaborador != null AND $session.params.retiroNumeroDocumentoColaborador != null` → página «02. Guardar Datos Colaborador Retirado»

### Página · 14. Reportar Retiro › 02. Guardar Datos Colaborador Retirado

**Al entrar, en este orden:**

1. ✍️ Llama al webhook **Guardar Datos Colaborador Encuesta De Retiro** [InsertarColaborador] — **escritura, pide confirmación**
2. 💬 Dice: «¡Gracias por la información! Estaré enviando la encuesta de retiro al colaborador de tu equipo.»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «03. Ejecutar HSM Camila Envió de Encuesta»

### Página · 14. Reportar Retiro › 03. Ejecutar HSM Camila Envió de Encuesta

**Al entrar, en este orden:**

1. ✍️ Llama al webhook **Envio HSM Encuesta de Retiro Camila** [EnvioHSMGupshup] — **escritura, pide confirmación**

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

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

### Página · 14.1. Encuesta de Retiro › 0. Definicion de Preguntas

**Al entrar, en este orden:**

1. 📌 Asigna: `intencion` = ``
2. 💬 Dice: «Cuéntame»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `controlGuardarEncuesta`, `preguntaEncuestaRetiro`, `pregunta1EncuestaRetiro`, `sub1Pregunta1EncuestaRetiro`, `sub2Pregunta1EncuestaRetiro`, `sub3Pregunta1EncuestaRetiro`, `sub4Pregunta1EncuestaRetiro`, `pregunta2EncuestaRetiro`, `pregunta3EncuestaRetiro`, `sub3Pregunta2EncuestaRetiro`, `sub4Pregunta2EncuestaRetiro`, `pregunta4EncuestaRetiro`, `pregunta5EncuestaRetiro`

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «1. Primera Pregunta»

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

### Página · 14.1. Encuesta de Retiro › 1. Respuesta 1 Pregunta 1

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.sub1Pregunta1EncuestaRetiro»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `respuesta` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.respuesta != null` → página «Guardar Pregunta Y Respuesta»  
  ↳ 📌 asigna `preguntaEncuestaRetiro` = `$session.params.pregunta1EncuestaRetiro Mejor oferta laboral…`, `controlGuardarEncuesta` = `1`

### Página · 14.1. Encuesta de Retiro › 1. Respuesta 2 Pregunta 1

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.sub2Pregunta1EncuestaRetiro»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `respuesta` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.respuesta != null` → página «Guardar Pregunta Y Respuesta»  
  ↳ 📌 asigna `preguntaEncuestaRetiro` = `$session.params.pregunta1EncuestaRetiro Horarios de estudio:…`, `controlGuardarEncuesta` = `1`

### Página · 14.1. Encuesta de Retiro › 1. Respuesta 3 Pregunta 1

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.sub3Pregunta1EncuestaRetiro»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `respuesta` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.respuesta != null` → página «Guardar Pregunta Y Respuesta»  
  ↳ 📌 asigna `preguntaEncuestaRetiro` = `$session.params.pregunta1EncuestaRetiro Inconformidad Capaci…`, `controlGuardarEncuesta` = `1`

### Página · 14.1. Encuesta de Retiro › 1. Respuesta 4 Pregunta 1

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.sub4Pregunta1EncuestaRetiro»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `respuesta` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.respuesta != null` → página «Guardar Pregunta Y Respuesta»  
  ↳ 📌 asigna `preguntaEncuestaRetiro` = `$session.params.pregunta1EncuestaRetiro Insatisfacción Jefe:…`, `controlGuardarEncuesta` = `1`

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

### Página · 14.1. Encuesta de Retiro › 6. Despedida

**Al entrar, en este orden:**

1. 💬 Dice: «¡Gracias por tu tiempo y sinceridad! Outsourcing S.A.S. BIC te desea muchos éxitos en tu camino profesional y recuerda que siempre serás parte de nuestra historia. ¡Hasta pronto!»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

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

### Página · 14.1. Encuesta de Retiro › Procesamiento de respuestas

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «Guardar Pregunta Y Respuesta»

### Página · 14.1. Encuesta de Retiro › Repuesta En Desacuerdo

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.sub3Pregunta2EncuestaRetiro»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `respuesta` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.respuesta != null` → página «Procesamiento de respuestas»

### Página · 14.1. Encuesta de Retiro › Respuesta Totalmente Desacuerdo

**Al entrar, en este orden:**

1. 💬 Dice: «$session.params.sub4Pregunta2EncuestaRetiro»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `respuesta` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.respuesta != null` → página «Procesamiento de respuestas»

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

### Página · 15. Viaje del colaborador › 0.datosNombre

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `nombre` — @sys.any · pregunta: «Para iniciar, por favor escribe tu nombre completo:»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «1.datosCedula»

### Página · 15. Viaje del colaborador › 1.datosCedula

**Al entrar, en este orden:**

1. 💬 Dice: «Gracias, $session.params.nombre. Ahora indícame tu número de cédula (sin puntos ni comas):»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `cedula` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «2.datosCorreo»

### Página · 15. Viaje del colaborador › 2.datosCorreo

**Al entrar, en este orden:**

1. 💬 Dice: «Por favor compártenos tu correo electrónico:»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `correo` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «3.datosEspacio»

### Página · 15. Viaje del colaborador › 3.datosEspacio

**Al entrar, en este orden:**

1. 💬 Dice: «¿A cuál espacio de formación o desarrollo asististe? (Ej: Inducción, Crece, Cuidamos, Evaluado, Contratado, etc.):»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `espacio` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «4.despacho»

### Página · 15. Viaje del colaborador › 4.despacho

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.responderEncuestaCrece = true` → flujo **15.2 Crece con nosotros**
- si `$session.params.responderEncuestaCuidamos = true` → flujo **15.3 Cuidamos de ti**
- si `$session.params.responderEncuestaInduccion = true` → flujo **15.4 Induccion**

## Flujo · 15.1 Encuesta W

**Qué dice al empezar:** «Para Outsourcing la experiencia de nuestros candidatos indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinió…»

**Se llega desde:**
- flujo 15. Viaje del colaborador ← condición

**Webhooks que usa:** ⚡ Extraer información candidato de W

**Pide al usuario:** `numero_documento` (0.1 Solicitar Cedula)

**Páginas:**
- 0.1 Solicitar Cedula — ⌨️ pide datos
- 0.Definir Encuesta W — ⚡ Extraer información candidato de W
- 1.Encuesta Proceso de Seleccion 
- 2. Encuesta Proceso de Capacitacion 
- 3. Encuesta Proceso de Contratacion 
- 4. Cierre 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0.1 Solicitar Cedula»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 15.1 Encuesta W › 0.1 Solicitar Cedula

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `numero_documento` — @sys.any · pregunta: «Por favor digita tu número de documento (sin puntos ni espacios):»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL" OR $session.params.numero_documento != null` → página «0.Definir Encuesta W»

### Página · 15.1 Encuesta W › 0.Definir Encuesta W

**Al entrar, en este orden:**

1. ⚡ Llama al webhook **Extraer información candidato de W** [https://default140443af8e794fceb3edde00131298.4f.environment.api.powerplatform.com/powerautomate/automations/direct/workflows/6593822f883f4cdc87c79560e93e5f22/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=-jHB9R2eCIWAjMPQ9z6dKoDv8xgfGfeKuySL1akSZXA] — lectura

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.faseW = "Decision" AND $session.params.estadoW = "Pendiente"` → página «1.Encuesta Proceso de Seleccion»
- si `$session.params.faseW = "Capacitacion" AND $session.params.estadoW = "Capacitacion"` → página «2. Encuesta Proceso de Capacitacion»
- si `$session.params.faseW = "Contratacion" AND $session.params.estadoW = "Contratacion"` → página «3. Encuesta Proceso de Contratacion»

### Página · 15.1 Encuesta W › 1.Encuesta Proceso de Seleccion

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de nuestros candidatos indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinión acerca del proceso de selección que re…»
2. 💬 Dice: «¿Qué tan satisfactoria fue tu experiencia con el proceso de selección?»
3. 🔘 Ofrece los botones: «Muy satisfecha» · «Satisfecha» · «Poco satisfecha» · «Insatisfecha»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Satisfecha» (`satisfecha`) → página «4. Cierre»
- «Poco satisfecha» (`poco_satisfecha`) → página «4. Cierre»
- «Insatisfecha» (`insatisfecha`) → página «4. Cierre»
- «Muy satisfecha» (`muy_satisfecha`) → página «4. Cierre»

### Página · 15.1 Encuesta W › 2. Encuesta Proceso de Capacitacion

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinión acerca del proceso de capacitaci…»
2. 💬 Dice: «¿Qué tan satisfactoria fue tu experiencia con el proceso de capacitación?»
3. 🔘 Ofrece los botones: «Muy satisfecha» · «Satisfecha» · «Poco satisfecha» · «Insatisfecha»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Satisfecha» (`satisfecha`) → página «4. Cierre»
- «Poco satisfecha» (`poco_satisfecha`) → página «4. Cierre»
- «Insatisfecha» (`insatisfecha`) → página «4. Cierre»
- «Muy satisfecha» (`muy_satisfecha`) → página «4. Cierre»

### Página · 15.1 Encuesta W › 3. Encuesta Proceso de Contratacion

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinión acerca de proceso de contratació…»
2. 💬 Dice: «¿Qué tan satisfactoria fue tu experiencia con el proceso de contratación?»
3. 🔘 Ofrece los botones: «Muy satisfecha» · «Satisfecha» · «Poco satisfecha» · «Insatisfecha»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- evento `sys.unknown-event` → se queda en la página  
  ↳ 💬 dice «Gracias por tu tiempo, $session.params.nombre. ¡Conversamos …»
- «Muy satisfecha» (`muy_satisfecha`) → página «4. Cierre»
- «Satisfecha» (`satisfecha`) → página «4. Cierre»
- «Poco satisfecha» (`poco_satisfecha`) → página «4. Cierre»
- «Insatisfecha» (`insatisfecha`) → página «4. Cierre»

### Página · 15.1 Encuesta W › 4. Cierre

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **10. Cierre de conversación**

## Flujo · 15.10 Hasta pronto

**Qué dice al empezar:** «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, es indispensable qu…»

**Se llega desde:**
- flujo 15. Viaje del colaborador ← condición

**Webhooks que usa:** ✍️ Finalizar Conversacion

**Páginas:**
- 0.Encuesta — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0.Encuesta»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 15.10 Hasta pronto › 0.Encuesta

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, es indispensable que como líder garantices que el colaborad…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

## Flujo · 15.2 Crece con nosotros

**Qué dice al empezar:** «Para Outsourcing la experiencia de colaboradores indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinión acer…»

**Se llega desde:**
- 15. Viaje del colaborador › 4.despacho ← condición

**Webhooks que usa:** ✍️ Finalizar Conversacion

**Páginas:**
- 0. Encuesta — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.responderEncuestaCrece = true` → página «0. Encuesta»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 15.2 Crece con nosotros › 0. Encuesta

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de colaboradores indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinión acerca de los espacios de formación y desarr…»
2. 🔘 Ofrece los botones: «Muy satisfactoria» · «Satisfactoria» · «Insatisfactoria» · «Muy Insatisfactoria»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

## Flujo · 15.3 Cuidamos de ti

**Qué dice al empezar:** «Para Outsourcing la experiencia de colaboradores indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinión acer…»

**Se llega desde:**
- 15. Viaje del colaborador › 4.despacho ← condición

**Webhooks que usa:** ✍️ Finalizar Conversacion

**Páginas:**
- 0. Encuesta — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.responderEncuestaCuidamos = true` → página «0. Encuesta»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 15.3 Cuidamos de ti › 0. Encuesta

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de colaboradores indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinión acerca del cuidado de la compañia.»
2. 🔘 Ofrece los botones: «Muy satisfactoria» · «Satisfactoria» · «Insatisfactoria» · «Muy Insatisfactoria»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

## Flujo · 15.4 Induccion

**Qué dice al empezar:** «Para Outsourcing la experiencia de colaboradores indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinión acer…»

**Se llega desde:**
- 15. Viaje del colaborador › 4.despacho ← condición

**Webhooks que usa:** ✍️ Finalizar Conversacion

**Páginas:**
- 0. Encuesta — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.responderEncuestaInduccion = true` → página «0. Encuesta»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 15.4 Induccion › 0. Encuesta

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de colaboradores indispensable para cumplir con nuestro propósito de mejorar vidas😃. Por ello, queremos conocer tu opinión acerca del proceso de inducción de la compañ…»
2. 🔘 Ofrece los botones: «Muy satisfactoria» · «Satisfactoria» · «Insatisfactoria» · «Muy Insatisfactoria»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

## Flujo · 15.5 Heramientas Kactus 1

**Qué dice al empezar:** «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, queremos conocer tu…»

**Se llega desde:**
- flujo 15. Viaje del colaborador ← condición

**Webhooks que usa:** ✍️ Finalizar Conversacion

**Páginas:**
- 0.Encuesta — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0.Encuesta»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 15.5 Heramientas Kactus 1 › 0.Encuesta

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, queremos conocer tu opinión acerca del proceso de entrega d…»
2. 💬 Dice: «Califique de acuerdo con las siguientes opciones es su experiencia frente a las herramientas y soporte tecnológico:»
3. 🔘 Ofrece los botones: «Muy satisfactoria» · «Satisfactoria» · «Insatisfactoria» · «Muy Insatisfactoria»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

## Flujo · 15.6 Herramientas Kactus 3

**Qué dice al empezar:** «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, queremos conocer tu…»

**Se llega desde:**
- flujo 15. Viaje del colaborador ← condición

**Webhooks que usa:** ✍️ Finalizar Conversacion

**Páginas:**
- 0. Encuesta — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0. Encuesta»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 15.6 Herramientas Kactus 3 › 0. Encuesta

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, queremos conocer tu opinión acerca del funcionamiento de he…»
2. 💬 Dice: «Califique de acuerdo con las siguientes opciones es su experiencia frente a las herramientas y soporte tecnológico:»
3. 🔘 Ofrece los botones: «Muy satisfactoria» · «Satisfactoria» · «Insatisfactoria» · «Muy Insatisfactoria»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

## Flujo · 15.7 Experiencia rol

**Qué dice al empezar:** «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, queremos conocer tu…»

**Se llega desde:**
- flujo 15. Viaje del colaborador ← condición

**Webhooks que usa:** ✍️ Finalizar Conversacion

**Páginas:**
- 0.Encuesta — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0.Encuesta»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 15.7 Experiencia rol › 0.Encuesta

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, queremos conocer tu opinión acerca de cómo te has sentido d…»
2. 💬 Dice: «Califique de acuerdo con las siguientes opciones cómo es tu experiencia en el día a día en la operación / cuenta:»
3. 🔘 Ofrece los botones: «Muy satisfactoria» · «Satisfactoria» · «Insatisfactoria» · «Muy Insatisfactoria»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

## Flujo · 15.8 Encuesta instalaciones

**Qué dice al empezar:** «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, queremos conocer tu…»

**Se llega desde:**
- flujo 15. Viaje del colaborador ← condición

**Webhooks que usa:** ✍️ Finalizar Conversacion

**Páginas:**
- 0.Encuesta — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0.Encuesta»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 15.8 Encuesta instalaciones › 0.Encuesta

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, queremos conocer tu opinión acerca de las instalaciones de …»
2. 💬 Dice: «Califique de acuerdo con las siguientes opciones cómo es tu experiencia frente a las instalaciones de la sede:»
3. 🔘 Ofrece los botones: «Muy satisfactoria» · «Satisfactoria» · «Insatisfactoria» · «Muy Insatisfactoria»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

## Flujo · 15.9 Lider mision

**Qué dice al empezar:** «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, queremos que valore…»

**Se llega desde:**
- flujo 15. Viaje del colaborador ← condición

**Webhooks que usa:** ✍️ Finalizar Conversacion

**Páginas:**
- 0.Encuesta — ✍️ Finalizar Conversacion

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0.Encuesta»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 15.9 Lider mision › 0.Encuesta

**Al entrar, en este orden:**

1. 💬 Dice: «Para Outsourcing la experiencia de nuestros colaboradores es indispensable para cumplir con nuestro propósito de mejorar vidas 😃. Por ello, queremos que valores a tu líder en los atributos: ganador, …»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → 🔚 **termina la conversación**  
  ↳ ✍️ Llama al webhook **Finalizar Conversacion** [EndConversationWithPersonalizedMessage] — **escritura, pide confirmación**

## Flujo · 16. Administración de personal

**Qué dice al empezar:** «Genial! ¿En que puedo ayudarte?»

**Se llega desde:**
- Default Start Flow › 05.Menu_Principal ← evento `administracionPersonal`

**Páginas:**
- 1. ¿En que te puedo ayudar? 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «1. ¿En que te puedo ayudar?»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 16. Administración de personal › 1. ¿En que te puedo ayudar?

**Al entrar, en este orden:**

1. 💬 Dice: «Genial! ¿En que puedo ayudarte?»
2. 🔘 Ofrece los botones: «💵 1. Pagos» · «💳 2. Libranza» · «🏥 3. EPS» · «🏢 4. Caja comp.» · «💰 5. Cesantías» · «🪪 6. Carné» · «📄 7. Certif.» · «🛡️ 8. Seg. Social»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «💵 1. Pagos» (`pagos`) → flujo **16.1 Pagos**  
  ↳ 📌 asigna `intencion` = `route_AP_Pagos`
- «💳 2. Libranza» (`creditoLibranza`) → flujo **16.2 Creditos libranza**  
  ↳ 📌 asigna `intencion` = `route_AP_CreditoLibranza`
- «🏢 4. Caja comp.» (`cajaCompensacion`) → flujo **16.4 Caja compensacion**  
  ↳ 📌 asigna `intencion` = `route_AP_CajaCompensacion`
- «🏥 3. EPS» (`epsTramite`) → flujo **16.3 EPS**  
  ↳ 📌 asigna `intencion` = `route_AP_EPSTramite`
- «💰 5. Cesantías» (`cesantias`) → flujo **16.5 Cesantias**  
  ↳ 📌 asigna `intencion` = `route_AP_Cesantias`
- «🪪 6. Carné» (`carne`) → flujo **16.6 carne**  
  ↳ 📌 asigna `intencion` = `route_AP_Carne`
- «📄 7. Certif.» (`certificados`) → flujo **16.7 certificados**  
  ↳ 📌 asigna `intencion` = `route_AP_Certificados`
- «🛡️ 8. Seg. Social» (`seguridadSocial`) → flujo **16.8 Seguridad Social**  
  ↳ 📌 asigna `intencion` = `route_AP_SeguridadSocial`

## Flujo · 16.1 Pagos

**Qué dice al empezar:** «¿En que puedo ayudarte?»

**Se llega desde:**
- 16. Administración de personal › 1. ¿En que te puedo ayudar? ← evento `pagos`

**Páginas:**
- 0.En que puedo ayudarte? 
- 1. Dudas Pagos 
- 2.Desprendible Nomina 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0.En que puedo ayudarte?»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 16.1 Pagos › 0.En que puedo ayudarte?

**Al entrar, en este orden:**

1. 💬 Dice: «¿En que puedo ayudarte?»
2. 🔘 Ofrece los botones: «📝 Desprendible» · «✍ Dudas o preguntas»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «✍ Dudas o preguntas» (`dudasInquietudes`) → página «1. Dudas Pagos»
- «📝 Desprendible» (`desprendibleNomina`) → página «2.Desprendible Nomina»

### Página · 16.1 Pagos › 1. Dudas Pagos

**Al entrar, en este orden:**

1. 💬 Dice: «Si tienes inquietudes sobre el valor pagado, debes dirigirte a tu líder inmediato para verificar las novedades reportadas al área de administración de personal. En caso de que estas hayan sido reporta…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

### Página · 16.1 Pagos › 2.Desprendible Nomina

**Al entrar, en este orden:**

1. 💬 Dice: «Para descargar tu desprendible de nómina accede a 👇  🔗 CONECTADOSWEB  https://conectadosweb.outsourcing.com.co/frmLogin.aspx»
2. 💬 Dice: «Si, presentas inconsistencias al ingresar por favor comunícate con auxnomina1@outsourcing.com.co para poder ayudarte 📧»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

## Flujo · 16.2 Creditos libranza

**Qué dice al empezar:** «¿En que te puedo ayudar?»

**Se llega desde:**
- 16. Administración de personal › 1. ¿En que te puedo ayudar? ← evento `creditoLibranza`

**Páginas:**
- 0. ¿En que puedo ayudarte? 
- 1. Entidades con convenio 
- 2. Documentos solicitud de crédito 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0. ¿En que puedo ayudarte?»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 16.2 Creditos libranza › 0. ¿En que puedo ayudarte?

**Al entrar, en este orden:**

1. 💬 Dice: «¿En que te puedo ayudar?»
2. 🔘 Ofrece los botones: «🏢 Ver convenios» · «📄 Solicitar crédito»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «🏢 Ver convenios» (`entidadesConvenio`) → página «1. Entidades con convenio»
- «📄 Solicitar crédito» (`solicitarCredito`) → página «2. Documentos solicitud de crédito»

### Página · 16.2 Creditos libranza › 1. Entidades con convenio

**Al entrar, en este orden:**

1. 💬 Dice: «¡Conoce nuestros convenios! Revísalos en la imagen que aparece a continuación. 👇»
2. 💬 Muestra la tarjeta «Producto destacado»
3. 🖼️ Muestra una imagen https://fine360.outsourcing.com.co:9876/img/convenios-outsourcing-v2.jpg

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

### Página · 16.2 Creditos libranza › 2. Documentos solicitud de crédito

**Al entrar, en este orden:**

1. 💬 Dice: «Los documentos que requieres para hacer este proceso con la entidad son:  ✅ 3 últimos desprendibles de nómina ✅Certificado Laboral ✅Debes validar con la entidad financiera si requieres soportes adicio…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

## Flujo · 16.3 EPS

**Qué dice al empezar:** «¿En que te puedo ayudar?»

**Se llega desde:**
- 16. Administración de personal › 1. ¿En que te puedo ayudar? ← evento `epsTramite`

**Páginas:**
- 0. ¿En que puedo ayudarte? 
- 1. agregarBeneficiario 
- 2. cambioTraslado 
- 3. afiliacion 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0. ¿En que puedo ayudarte?»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 16.3 EPS › 0. ¿En que puedo ayudarte?

**Al entrar, en este orden:**

1. 💬 Dice: «¿En que te puedo ayudar?»
2. 🔘 Ofrece los botones: «👤 Beneficiario» · «🔄 Traslado EPS» · «📋 Afiliación EPS»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «👤 Beneficiario» (`agregarBeneficiario`) → página «1. agregarBeneficiario»
- «🔄 Traslado EPS» (`cambioTraslado`) → página «2. cambioTraslado»
- «📋 Afiliación EPS» (`afiliacionEps`) → página «3. afiliacion»
💬 *Si el usuario escribe algo que coincide con:*
- intent «route_EncuestaSelecciónW» (ej. «Evaluar selección») → se queda en la página

### Página · 16.3 EPS › 1. agregarBeneficiario

**Al entrar, en este orden:**

1. 💬 Dice: «Tienes dos opciones:  ✅ Inscribirte en miseguridadsocial.gov.co y seguir los pasos indicados allí para agregar o retirar beneficiarios.  ✅Comunícate con la EPS afiliada»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

### Página · 16.3 EPS › 2. cambioTraslado

**Al entrar, en este orden:**

1. 💬 Dice: «Para hacer el cambio de EPS puedes ingresar a la página web https://miseguridadsocial.gov.co y efectuar la solicitud, o acercarte a un punto físico de la entidad a la que deseas trasladarte.  El trasl…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

### Página · 16.3 EPS › 3. afiliacion

**Al entrar, en este orden:**

1. 💬 Dice: «La afiliación se realiza el siguiente día hábil posterior a la contratación. Para atención no vital, la cobertura inicia a partir de los 30 días de haber comenzado a laborar. Si deseas solicitar copia…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

## Flujo · 16.4 Caja compensacion

**Qué dice al empezar:** «Si quieres adicionar a algún familiar debes ingresar al portal web de la caja de compensación familiar o comunicarte con nuestro asesor Hernan Andrade al correo…»

**Se llega desde:**
- 16. Administración de personal › 1. ¿En que te puedo ayudar? ← evento `cajaCompensacion`

**Páginas:**
- 1. Información de adición familiar 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «1. Información de adición familiar»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 16.4 Caja compensacion › 1. Información de adición familiar

**Al entrar, en este orden:**

1. 💬 Dice: «Si quieres adicionar a algún familiar debes ingresar al portal web de la caja de compensación familiar o comunicarte con nuestro asesor Hernan Andrade al correo handrade@outsourcing.com.co teléfono 31…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

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

### Página · 16.5 Cesantias › 1. Estudio

**Al entrar, en este orden:**

1. 💬 Dice: «Debes acercarte a tu fondo de cesantías y realizar directamente allí la solicitud de retiro por concepto de educación. Para ello, es posible que te soliciten documentos soporte, como la certificación …»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

### Página · 16.5 Cesantias › 2. Compra vivienda

**Al entrar, en este orden:**

1. 💬 Dice: «Debes realizar el siguiente proceso:  👇  ✅ Carta de solicitud dirigida a OUTSOURCING S.A.S BIC ✅ Promesa de compra y venta firmada por ambas partes ✅ Certificado del saldo de cesantías en el Fondo ✅ …»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

### Página · 16.5 Cesantias › 3. Credito hipotecario

**Al entrar, en este orden:**

1. 💬 Dice: «Debes realizar el siguiente proceso:  👇  ✅Carta de solicitud dirigida a OUTSOURCING S.A.S BIC ✅ Promesa de compra y venta firmada por ambas partes ✅ Certificado del saldo de cesantías en el Fondo. ✅ …»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

### Página · 16.5 Cesantias › 4. Arreglos locativos

**Al entrar, en este orden:**

1. 💬 Dice: «Debes realizar el siguiente proceso:  👇  ✅ Carta de solicitud dirigida a Outsourcing S.A.S. BIC ✅ Certificado de tradición y Libertad no mayor a 30 días de vigencia ✅ Certificado del saldo de cesantí…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

### Página · 16.5 Cesantias › 5. Impuesto predial

**Al entrar, en este orden:**

1. 💬 Dice: «Debes realizar el siguiente proceso:  👇  ✅Carta de solicitud dirigida a Outsourcing S.A.S. BIC ✅Certificado de tradición y Libertar no mayor 30 días de vigencia ✅ Certificado de saldo de cesantías en…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

## Flujo · 16.6 carne

**Qué dice al empezar:** «Si se te perdió el carné, debes dirigirte a la recepción de tu sede, solicitar el formato de descuento de nómina y entregarlo diligenciado. Se te hará la entreg…»

**Se llega desde:**
- 16. Administración de personal › 1. ¿En que te puedo ayudar? ← evento `carne`

**Páginas:**
- 1. carne 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «1. carne»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 16.6 carne › 1. carne

**Al entrar, en este orden:**

1. 💬 Dice: «Si se te perdió el carné, debes dirigirte a la recepción de tu sede, solicitar el formato de descuento de nómina y entregarlo diligenciado. Se te hará la entrega del nuevo carné en un tiempo de 3 a 5 …»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

## Flujo · 16.7 certificados

**Qué dice al empezar:** «¿En que puedo ayudarte?»

**Se llega desde:**
- 16. Administración de personal › 1. ¿En que te puedo ayudar? ← evento `certificados`

**Páginas:**
- 0. ¿En que puedo ayudarte? 
- 1. Laboral/Ingresos 
- 2. Laboral/Especial 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0. ¿En que puedo ayudarte?»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 16.7 certificados › 0. ¿En que puedo ayudarte?

**Al entrar, en este orden:**

1. 💬 Dice: «¿En que puedo ayudarte?»
2. 🔘 Ofrece los botones: «📄 Laboral/Ingresos» · «📄 Laboral especial»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «📄 Laboral/Ingresos» (`certificadoWeb`) → página «1. Laboral/Ingresos»
- «📄 Laboral especial» (`laboralEspecial`) → página «2. Laboral/Especial»

### Página · 16.7 certificados › 1. Laboral/Ingresos

**Al entrar, en este orden:**

1. 💬 Dice: «Para descargar tu certificado accede a 👇 🔗 CONECTADOSWEB https://conectadosweb.outsourcing.com.co/frmLogin.aspx»
2. 💬 Dice: «Si, presentas inconsistencias al ingresar por favor comunícate con auxnomina1@outsourcing.com.co para poder ayudarte. 📧»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

### Página · 16.7 certificados › 2. Laboral/Especial

**Al entrar, en este orden:**

1. 💬 Dice: «Para solicitar el certificado laboral especial con cargos u otras especificaciones (funciones, dirigido a una empresa en particular, entre otros), debes enviar tu solicitud al correo certificacionesla…»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

## Flujo · 16.8 Seguridad Social

**Qué dice al empezar:** «Por favor selecciona una opción:»

**Se llega desde:**
- 16. Administración de personal › 1. ¿En que te puedo ayudar? ← evento `seguridadSocial`

**Pide al usuario:** `tipoDoc` (1. Formulario Planillas), `numDoc` (1. Formulario Planillas), `periodoPlanilla` (1. Formulario Planillas), `correoPlanilla` (1. Formulario Planillas)

**Páginas:**
- 0.menu  
- 1. Formulario Planillas — ⌨️ pide datos
- 2.Confirmacion Planillas 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0.menu »
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 16.8 Seguridad Social › 0.menu 

**Al entrar, en este orden:**

1. 💬 Dice: «Por favor selecciona una opción:»
2. 🔘 Ofrece los botones: «📄 Planillas Pago»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$request.text = "📄 Planillas de pago" OR $request.text = "Planillas de pago"` → página «1. Formulario Planillas»
🔘 *Si el usuario pulsa un botón:*
- «📄 Planillas Pago» (`planillaspago`) → página «1. Formulario Planillas»

### Página · 16.8 Seguridad Social › 1. Formulario Planillas

**Al entrar, en este orden:**

1. 💬 Dice: «La planilla de pago solo la puedes solicitar a partir del año 2017. Si requieres certificados de años anteriores, debes enviar tu solicitud al correo auxnomina1@outsourcing.com.co📧»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `tipoDoc` — @sys.any · pregunta: «Digita el tipo de documento»
- `numDoc` — @sys.any · pregunta: «Digita el número de documento»
- `periodoPlanilla` — @sys.any · pregunta: «Ingresa mes y año MM / AAA»
- `correoPlanilla` — @sys.any · pregunta: «Escribe el correo electrónico en el cual deseas recibir tu certificado de planillas de pago»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «2.Confirmacion Planillas»

### Página · 16.8 Seguridad Social › 2.Confirmacion Planillas

**Al entrar, en este orden:**

1. 💬 Dice: «Tu solicitud ha sido registrada exitosamente y será tramitada máximo en un 1 día hábil.»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

## Flujo · 17. Ex colaborador

**Qué dice al empezar:** «¿En que puedo ayudarte?»

**Se llega desde:**
- Default Start Flow › 04.Ingreso_Existoso ← condición

**Páginas:**
- 1. ¿En que puedo ayudarte? 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «1. ¿En que puedo ayudarte?»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 17. Ex colaborador › 1. ¿En que puedo ayudarte?

**Al entrar, en este orden:**

1. 💬 Dice: «¿En que puedo ayudarte?»
2. 🔘 Ofrece los botones: «💵 1. Pagos» · «💰 2. Cesantías» · «📄 3. Certificados»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «💵 1. Pagos» (`pagos`) → flujo **17.1 pagos**
- «💰 2. Cesantías» (`cesantias`) → flujo **17.2 Cesantias**
- «📄 3. Certificados» (`certificados`) → flujo **17.3 Certificados**

## Flujo · 17.1 pagos

**Qué dice al empezar:** «¿En que puedo ayudarte?»

**Se llega desde:**
- 17. Ex colaborador › 1. ¿En que puedo ayudarte? ← evento `pagos`

**Pide al usuario:** `numero_documento` (1.Formulario Liquidacion), `numContrato` (1.Formulario Liquidacion)

**Páginas:**
- 0. ¿En que puedo ayudarte? 
- 1.Formulario Liquidacion — ⌨️ pide datos
- 2. Estado liquidacion 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0. ¿En que puedo ayudarte?»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 17.1 pagos › 0. ¿En que puedo ayudarte?

**Al entrar, en este orden:**

1. 💬 Dice: «¿En que puedo ayudarte?»
2. 🔘 Ofrece los botones: «Estado liquidación»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «Estado liquidación» (`estadoLiquidacion`) → página «1.Formulario Liquidacion»

### Página · 17.1 pagos › 1.Formulario Liquidacion

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `numero_documento` — @sys.any · pregunta: «Digita el numero de documento:»
- `numContrato` — @sys.any · pregunta: «Selecciona el número de contrato a consultar»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «2. Estado liquidacion»

### Página · 17.1 pagos › 2. Estado liquidacion

**Al entrar, en este orden:**

1. 💬 Dice: «¿En que puedo ayudarte?»
2. 🔘 Ofrece los botones: «🔴 Inactivo» · «🟠 Retirado»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «🔴 Inactivo» (`estadoInactivo`) → flujo **12. ¿Algo Mas?**  
  ↳ 💬 dice «Tu liquidación se encuentra lista, te llegarán los documento…»
- «🟠 Retirado» (`estadoRetirado`) → flujo **12. ¿Algo Mas?**  
  ↳ 💬 dice «Tu liquidación se encuentra en proceso de elaboración. El ti…»

## Flujo · 17.2 Cesantias

**Qué dice al empezar:** «¿En que puedo ayudarte?»

**Se llega desde:**
- 17. Ex colaborador › 1. ¿En que puedo ayudarte? ← evento `cesantias`

**Pide al usuario:** `numDoc` (0.Respuesta), `numContrato` (0.Respuesta)

**Páginas:**
- 0.Respuesta — ⌨️ pide datos
- 1.Elección 

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «0.Respuesta»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 17.2 Cesantias › 0.Respuesta

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `numDoc` — @sys.any · pregunta: «Digita el número de documento:»
- `numContrato` — @sys.any · pregunta: «Selecciona el número de contrato a consultar»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «1.Elección»

### Página · 17.2 Cesantias › 1.Elección

**Al entrar, en este orden:**

1. 💬 Dice: «¿En que puedo ayudarte?»
2. 🔘 Ofrece los botones: «📅 Mismo año» · «🗓️ Otro año»

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «🗓️ Otro año» (`anioDiferente`) → flujo **12. ¿Algo Mas?**  
  ↳ 💬 dice «A continuación te remitimos tu carta de retiro de cesantias …»
- «📅 Mismo año» (`mismoAnio`) → flujo **12. ¿Algo Mas?**  
  ↳ 💬 dice «Por el periodo laborado tus cesantías se encuentran cancelad…»

## Flujo · 17.3 Certificados

**Qué dice al empezar:** «Selecciona el tipo de certificado que necesites»

**Se llega desde:**
- 17. Ex colaborador › 1. ¿En que puedo ayudarte? ← evento `certificados`

**Webhooks que usa:** ⚡ Login_API_Kairos · ✍️ certificadosAdminPersonal-Basico · ✍️ certificadosAdminPersonal-Ingresos

**Pide al usuario:** `documento` (1. datos), `fechaExpedicion` (1. datos), `correoElectronico` (3. correo certificado basico), `mesCertificado` (3. correo certificado basico), `anoCertificado` (3. correo certificado basico), `mesFinalCertificado` (3. correo certificado basico), `anoFinalCertificado` (3. correo certificado basico), `anioCertificado` (4. info ingresos y retenciones), `correoIngresos` (4. info ingresos y retenciones), `mesCertificado` (5. envio datos certificado), `anioCertificado` (5. envio datos certificado), `correoElectronico` (5. envio datos certificado), `finalMesCertificado` (5. envio datos certificado), `finalAnioCertificado` (5. envio datos certificado)

**Páginas:**
- 1. datos — ⚡ Login_API_Kairos · ⌨️ pide datos
- 2.seleccionar certificado 
- 3. correo certificado basico — ✍️ certificadosAdminPersonal-Basico · ⌨️ pide datos
- 4. info ingresos y retenciones — ✍️ certificadosAdminPersonal-Ingresos · ⌨️ pide datos
- 5. envio datos certificado — ✍️ certificadosAdminPersonal-Basico · ⌨️ pide datos

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → página «1. datos»
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · 17.3 Certificados › 1. datos

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `documento` — @sys.any · pregunta: «Digita el número de documento»
- `fechaExpedicion` — @sys.any · pregunta: «Ingresa fecha de expedición de tu documento DD-MM-AAAA»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «2.seleccionar certificado»  
  ↳ ⚡ Llama al webhook **Login_API_Kairos** [Login] — lectura

### Página · 17.3 Certificados › 2.seleccionar certificado

**Al entrar, en este orden:**

1. 💬 Dice: «Selecciona el tipo de certificado que necesites»
2. 🔘 Ofrece los botones: «📄 Cert. Básico» · «📄 Laboral especial» · «💰 Ingresos y rete.»

**Variables del formulario (no obligatorias, normalmente las llena un webhook):** `tipoCertificado`

**Qué pasa después:**

🔘 *Si el usuario pulsa un botón:*
- «📄 Laboral especial» (`laboralEspecial`) → flujo **12. ¿Algo Mas?**  
  ↳ 💬 dice «Para solicitar el certificado laboral especial con cargos u …»
- «📄 Cert. Básico» (`certificadoBasico`) → página «3. correo certificado basico»
- «💰 Ingresos y rete.» (`ingresosRetenciones`) → página «4. info ingresos y retenciones»  
  ↳ 📌 asigna `tipoCertificado` = `ingresos`

### Página · 17.3 Certificados › 3. correo certificado basico

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `correoElectronico` — @sys.any · pregunta: «Por favor escribe el correo electrónico que registraste con nosotros para recibir tu certificado laboral»
- `mesCertificado` — @sys.any · pregunta: «Por favor escribe el mes desde donde quieres que se genere el certificado.»
- `anoCertificado` — @sys.any · pregunta: «Escribe el año desde donde desees solicitar el certificado. Recuerda que solo se puede solicitar a partir del año 2017, …»
- `mesFinalCertificado` — @sys.any · pregunta: «Por favor escribe el mes final para realizar la consulta.»
- `anoFinalCertificado` — @sys.any · pregunta: «Escribe el año hasta donde desees solicitar el certificado.»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «5. envio datos certificado»  
  ↳ 📌 asigna `tipoCertificado` = `planilla` · ✍️ Llama al webhook **certificadosAdminPersonal-Basico** [administracion-personal-certificados] — **escritura, pide confirmación** · 💬 dice «Tu solicitud ha sido registrada exitosamente y será tramitad…»

### Página · 17.3 Certificados › 4. info ingresos y retenciones

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `anioCertificado` — @sys.any · pregunta: «Por favor ingresa el año (Cuatro dígitos, Ejemplo: 2001) del cual necesitas generar tu certificado.»
- `correoIngresos` — @sys.any · pregunta: «Por favor escribe el correo electrónico en el cual deseas recibir tu certificado de ingresos y retenciones»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → página «5. envio datos certificado»  
  ↳ ✍️ Llama al webhook **certificadosAdminPersonal-Ingresos** [administracion-personal-certificados] — **escritura, pide confirmación** · 💬 dice «Tu solicitud ha sido registrada exitosamente y será tramitad…»

### Página · 17.3 Certificados › 5. envio datos certificado

**Al entrar, en este orden:**

_No hace nada al entrar._

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `mesCertificado` — @sys.any · pregunta: «Por favor escribe el mes desde donde quieres que se genere el certificado.»
- `anioCertificado` — @sys.any · pregunta: «Escribe el año desde donde desees solicitar el certificado. Recuerda que solo se puede solicitar a partir del año 2017, …»
- `correoElectronico` — @sys.any · pregunta: «Por favor escribe el correo electrónico en el cual deseas recibir tu certificado de planillas de pago.»
- `finalMesCertificado` — @sys.any · pregunta: «Por favor escribe el mes final para realizar la consulta.»
- `finalAnioCertificado` — @sys.any · pregunta: «Escribe el año hasta donde desees solicitar el certificado.»

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$page.params.status = "FINAL"` → flujo **12. ¿Algo Mas?**  
  ↳ ✍️ Llama al webhook **certificadosAdminPersonal-Basico** [administracion-personal-certificados] — **escritura, pide confirmación** · 💬 dice «Tu solicitud ha sido registrada exitosamente y será tramitad…»

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

### Página · Default Start Flow › 03.Validacion_Usuario_Kairos

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.existe = false` → se queda en la página
- si `$session.params.existe = true AND $session.params.activo = false` → se queda en la página
- si `$session.params.existe = true AND $session.params.activo = true` → se queda en la página

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

## Flujo · Pruebas pdf

**Qué dice al empezar:** «por favor enviame un pdf»

**Se llega desde:**
- flujo Default Start Flow ← intent «PDF pruebas»

**Webhooks que usa:** ⚡ recuperarSessionWatsonId

**Pide al usuario:** `pdf` (enviar pdf)

**Páginas:**
- Mostrar pdf 
- enviar pdf — ⌨️ pide datos

**Rutas propias del flujo (valen en todas sus páginas):**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `true = true` → página «enviar pdf»  
  ↳ ⚡ Llama al webhook **recuperarSessionWatsonId** [session-watson] — lectura
- ❔ *Sin coincidencia* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»
- ❔ *Sin respuesta* → se queda en la página  
  ↳ 💬 dice «Ups, no he entendido a que te refieres.»

### Página · Pruebas pdf › Mostrar pdf

**Al entrar, en este orden:**

_No hace nada al entrar._

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- siempre → flujo **12. ¿Algo Mas?**

### Página · Pruebas pdf › enviar pdf

**Al entrar, en este orden:**

1. 💬 Dice: «por favor enviame un pdf»

**⌨️ Pide al usuario** (si no lo tiene ya en la sesión):
- `pdf` — @sys.any

**Qué pasa después:**

🔀 *Automáticas (sin esperar al usuario, en este orden):*
- si `$session.params.pdf != null` → página «Mostrar pdf»

## Grupos de rutas (atajos)

Rutas compartidas por varias páginas o flujos (por ejemplo «volver al menú principal»).

### Grupo · Atajos (global)

- intent «Menu_Principal» → Default Start Flow

### Grupo · Atajos_Flow (global)

- intent «Menu_Principal» → Default Start Flow

### Grupo · Atajos_Flow ( (global)

- intent «Menu_Principal» → Default Start Flow

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

### Grupo · Atajos_Flow (flujo 01. Indicadores De Calidad)


### Grupo · Atajos_globales_submenu (flujo 01. Indicadores De Calidad)

- siempre → 05. Matriz
- siempre → 04 . Oportunidades
- siempre → 03. CerrarMonitoreos
- siempre → 006.ParametrosAtributos
- siempre → 004.ParametrosIndicadores

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

### Grupo · Predeterminados (flujo Default Start Flow)

- siempre → 05.Menu_Principal

## Intents

Frases del usuario que el bot reconoce.

### Intent · Default Negative Intent

Frases: 

### Intent · Default Welcome Intent

Frases: oa | ola | hoa | noches | tardes | días … (14 en total)

### Intent · Despedida

Frases: Ya terminé, muchas gracias por todo | No tengo más preguntas, adiós | Eso es todo, gracias por su tiempo | Me despido, muchas gracias por todo. | No necesito nada más, que tenga un buen día. | Eso es todo, gracias por la ayuda. … (30 en total)

### Intent · Encuesta De Retiro

Frases: Encuesta de retiro | Encuesta De Retiro | Encuesta Retiro | Encuesta retiro

### Intent · EncuestaExperienciaRol

Frases: Evaluar mi rol | Como te has sentido desarrollando tu rol | Calificar experiencia en el dia a dia de la operacion | Encuesta de experiencia en el rol | Responder encuesta experiencia de tu rol

### Intent · EncuestaHastaPronto

Frases: Garantizar encuesta de retiro | Encuesta hasta pronto | Responder encuesta hasta pronto

### Intent · EncuestaInstalaciones

Frases: Calificar sede | Calificar instalaciones de la compania | Responder encuesta sobre las instalaciones | Encuesta instalaciones de la sede | Responder encuesta valora las instalaciones

### Intent · EncuestaLiderMision

Frases: Valorar a mi lider | Encuesta lider mision | Responder encuesta lider mision

### Intent · Enero

Frases: 

### Intent · KactusHerramientasTecnologicas1

Frases: Evaluar herramientas | Responder encuesta entrega herramientas | Encuesta llegada herramientas tecnologicas | Calificar entrega de herramientas

### Intent · KactusHerramientasTecnologicas3

Frases: Evaluar soporte | Calificar soporte tecnologico | Encuesta soporte tecnico 3 meses | Responder encuesta funcionamiento herramientas

### Intent · Menu_Principal

Frases: Volver al inicio | Quiero ir al menú principal | Regresar al menú principal | Volver al menú principal | Llevar al menú principal | Ir al menú principal … (14 en total)

### Intent · Mis Indicadores De Calidad

Frases: CALIDAD | calidad | Calidad

### Intent · PDF pruebas

Frases: PdF | Pdf | PDF | pdf

### Intent · administracionPersonal

Frases: admin personal | administracion de personal | Ingreso administracion de personal | admin de personal | Quiero ingresar a administracion de personal

### Intent · exColaborador

Frases: Excolaborador | Ex colaborador | excolaborador | soy ex colaborador | ex colaborador

### Intent · qrEncuestaCrece

Frases: crece | encuesta crece | Responder encuesta crece con nosotros

### Intent · qrEncuestaCuidamos

Frases: cuidamos | encuesta cuidamos | Responder encuestas cuidamos de ti

### Intent · qrEncuestaInduccion

Frases: induccion | encuesta induccion | Responder encuesta Inducción

### Intent · routeCalidad_AtributosEmbajador

Frases: atributos embajador de marca | Mostrar mi evaluación de atributos de embajador | Necesito saber mis atributos de embajador | Quiero consultar mis atributos de embajador | Necesito ver mis evaluaciones de atributos de embajador | Muéstrame mis atributos de embajador … (14 en total)

### Intent · routeCalidad_CerrarMonitoreosPendientes

Frases: Cerrar monitoreos | Cierra los monitoreos que faltan, por favor | ¿Me puedes ayudar a cerrar los monitoreos? | Quiero finalizar los monitoreos que están pendientes | Necesito ayuda para cerrar los monitoreos pendientes | Cerrar todos los monitoreos pendientes … (16 en total)

### Intent · routeCalidad_IndicadoresDeCalidad

Frases: indicadores calidad | Indicadores Calidad | ¿Qué tal voy con mis indicadores de calidad? | Dime el estado actual de mis indicadores de calidad. | ¿Cómo puedo acceder a mis indicadores de calidad? | ¿Dónde puedo encontrar mis indicadores de calidad? … (15 en total)

### Intent · routeCalidad_MatrizDeCalidad

Frases: Quiero acceder a la matriz de calidad. | Enséñame mi matriz de calidad, por favor. | ¿Podrías mostrarme la matriz de calidad? | Mostrarme la matriz de calidad, por favor. | Quiero revisar mi matriz de calidad. | ¿Cómo accedo a mi matriz de calidad? … (13 en total)

### Intent · routeCalidad_OportunidadesYFortalezas

Frases: Dime mis oportunidades para crecer | Quiero saber mis fortalezas en el trabajo | ¿Cuáles son mis principales fortalezas y oportunidades de mejora? | ¿Cuáles son mis fortalezas? | ¿Cuáles son mis oportunidades? | Dime mis oportunidades y fortalezas … (12 en total)

### Intent · route_ActualizarDatosDatix

Frases: Actualizar datos personales | Quiero ingresar a Datix | Necesito actualizar mis datos en la plataforma | Cómo cambio mis datos en el sistema | Actualizar información personal | Quiero modificar mi información … (23 en total)

### Intent · route_Calidad

Frases: ¿Cómo accedo al menú de calidad? | Quiero ir a la sección de calidad | ¿Dónde está el apartado de calidad? | ¿Cómo puedo ver el menu de calidad? | Quiero acceder a las opciones de calidad | Quisiera saber dónde está la sección de calidad … (12 en total)

### Intent · route_Convenios

Frases: Información sobre beneficios y convenios | Quiero acceder a los convenios | Dónde veo los convenios disponibles | Beneficios para colaboradores | Alianzas estratégicas | Qué descuentos tienen … (29 en total)

### Intent · route_EncuestaCapacitaciónW

Frases: Evaluar capacitación

### Intent · route_EncuestaContrataciónW

Frases: Evaluar contratación

### Intent · route_EncuestaSelecciónW

Frases: Evaluar selección

### Intent · route_EvaluacionDeDesemepeño

Frases: Valoración De Desempeño | Valoración de desempeño | valoración de desempeño | Ver puntaje de desempeño | Quiero saber cómo me fue | Mis resultados laborales … (32 en total)

### Intent · route_LiderMision

Frases: lider mision | indicadores lider mision | Información sobre líder misión | Ver mi puntaje de líder misión | Resultados de mi evaluación | Cómo puedo ver mi desempeño … (26 en total)

### Intent · route_ProcesoDeSeleccion

Frases: ¿Pueden darme una actualización sobre mi proceso de selección? | ¿Cuál es el estado actual de mi solicitud de trabajo? | Necesito información sobre mi solicitud de trabajo | ¿Dónde puedo ver el estado de mi solicitud de empleo? | Quiero saber en qué etapa va mi proceso de selección | ¿Cómo puedo verificar el estado de mi proceso de selección? … (11 en total)

### Intent · route_Reconoser

Frases: ¿Dónde encuentro la información de Reconoser? | Quiero consultar mi desempeño en Reconoser | ¿Cómo puedo revisar mis resultados de Reconoser? | ¿Cómo accedo a mis calificaciones Reconoser? | Necesito ver mi informe Reconoser | ¿Dónde puedo encontrar mis indicadores Reconoser? … (12 en total)

### Intent · route_Referidos

Frases: Quiero referir a mi mama para un puesto de ... | Quiero referir a mi mama para un puesto de | Quiero referir a alguien | Referidos | Cómo aplica el programa de referidos | Quiero hacer una referencia … (30 en total)

### Intent · route_RespuestaAfirmativa

Frases: Si quiero actualizar mis datos | Si | Efectivamente | Si quiero .... | Vamos a hacerlo | Sí, adelante … (23 en total)

### Intent · route_RespuestaNegativa

Frases: No, gracias | No gracias | Dejémoslo así | No aplica | Paso por ahora | No por el momento … (21 en total)

### Intent · route_TeEscuchamos

Frases: Cómo puedo dejar un comentario | Quiero dejar una recomendación | Tengo una queja | Quiero hacer una sugerencia | Espacio para comentarios | Quiero dar mi opinión … (25 en total)

## Entidades

Formato que debe tener un dato que el bot pide.

### Entidad · Numero_Documento (KIND_REGEXP)

Valores válidos: `\d{5,15}`

### Entidad · placa-moto (KIND_REGEXP)

Valores válidos: `[A-Z]{3}[0-9]{2}[A-I]{1}`

### Entidad · placa_carro (KIND_REGEXP)

Valores válidos: `[A-Z]{3}[0-9]{3}`

### Entidad · tipoDespedida (KIND_MAP)

Valores válidos: 

## Generadores de IA

Prompts que el bot original enviaba a un modelo generativo. LectorUML no los ejecuta.

### Generador · actualizar_estado_pdi

Se usa en: 02.1. Conversacional Evaluacion Desempeño › PDI - Actualizar estado (al entrar); 03.1. Conversacional Lider Misión › PDI - Actualizar estado (al entrar)

Prompt:

> PDI — ACTUALIZADOR PRODUCCIÓN v17.1.1.4 — FIREWALL A2_20
> 
> ENTRADAS
> ESTADO=$estado_anterior
> CAMILA=$respuesta_camila
> USUARIO=$mensaje_usuario
> 
> SALIDA
> Devuelve SOLO JSON PURO con EXACTAMENTE estas 13 claves:
> A1_70,A2_70,A3_70,A1_20,A2_20,R1_10,enlaceR1_10,R2_10,enlaceR2_10,
> rechazadas70,rechazadas20,rechazados10,etapa
> 
> ESTADO_VACIO={"A1_70":null,"A2_70":null,"A3_70":null,"A1_20":null,"A2_20":null,"R1_10":null,"enlaceR1_10":null,"R2_10":null,"enlaceR2_10":null,"rechazadas70":[],"rechazadas20":[],"rechazados10":[],"etapa":"A1_70"}
> 
> ====================================================================
> 0. PRINCIPIO ÚNICO
> ====================================================================
> 
> ESTADO es la única fuente de verdad.
> 
> 1. Parsea ESTADO quitando únicamente fences ```json y ```.
> 2. Si no parsea, usa ESTADO_VACIO.
> 3. null en listas => [].
> 4. Define BASE=copia profunda e INMUTABLE de ESTADO.
> 5. Define OUT=copia profunda EXACTA de BASE.
> 
> REGLA ABSOLUTA:
> CADA RAMA empieza desde OUT=BASE y modifica SOLO los campos que esa rama enumera.
> Todo campo no enumerado DEBE quedar idéntico a BASE.
> 
> NUNCA:
> - reconstruyas slots desde CAMILA;
> - compactes o muevas slots;
> - borres un slot aceptado anterior;
> - borres listas históricas completas;
> - cambies más de una familia en una ejecución.
> 
> ====================================================================
> 1. NORMALIZACIÓN
> ====================================================================
> 
> C=CAMILA o "".
> u=USUARIO o "" en minúsculas, trim, espacios simples.
> C_NORM=C sin asteriscos Markdown y con espacios normalizados.
> 
> CANON(x):
> - null => null
> - minúsculas, trim, espacios simples
> - elimina asteriscos Markdown externos
> - elimina SOLO un prefijo funcional inicial si existe:
>   "ACCIÓN 1 DEL 70%:"
>   "ACCIÓN 2 DEL 70%:"
>   "ACCIÓN 3 DEL 70%:"
>   "ACCIÓN 1 DEL 20%:"
>   "ACCIÓN 2 DEL 20%:"
>   "RECURSO 1 DEL 10%:"
>   "RECURSO 2 DEL 10%:"
> - ignora . , : ; únicamente al final
> 
> LIMPIA_TITULO(x):
> - conserva texto/puntuación interna;
> - elimina asteriscos externos;
> - elimina SOLO el prefijo funcional inicial anterior;
> - trim;
> - NO elimina "Curso:" ni "Taller:".
> 
> AGREGA_UNICO(L,T):
> copia L; agrega T al final SOLO si CANON(T) no existe en CANON(L).
> 
> REMOVE_SOLO(L,T):
> copia L conservando orden;
> elimina SOLO x donde CANON(x)==CANON(T);
> conserva TODOS los demás exactamente.
> 
> ====================================================================
> 2. GUARDAS QUE TERMINAN INMEDIATAMENTE
> ====================================================================
> 
> Si trim(C)=="INICIAR PLAN":
> devuelve ESTADO_VACIO. FIN.
> 
> Si BASE.etapa=="FECHAS":
> devuelve BASE. FIN.
> 
> FECHA=true si u:
> - contiene enero/febrero/marzo/abril/mayo/junio/julio/agosto/septiembre/octubre/noviembre/diciembre;
> - o parece dd/mm, dd-mm, dd/mm/yyyy, yyyy-mm-dd;
> - o es "mañana" o "pasado mañana".
> 
> Si FECHA=true:
> devuelve BASE EXACTO. FIN.
> Una fecha JAMÁS confirma el resumen ni una acción.
> 
> NEGATIVO=true SOLO si u (ignorando puntuación final) es:
> "no","no gracias","no me gusta","esa no","ese no","no me convence",
> "otra","otro","otra opcion","otra opción","cambiala","cámbiala","cambialo","cámbialo".
> 
> AFIRMATIVO=true SOLO si NEGATIVO=false y u es:
> "si","sí","acepto","aceptar","dale","de una","perfecto","claro",
> "me gusta","si me gusta","sí me gusta","esa si","esa sí","ese si","ese sí",
> "quiero esa","quiero ese","la acepto","lo acepto".
> 
> SOLO=true SOLO si u contiene:
> "solo con la primera","solo la primera","me quedo con la primera",
> "con uno basta","con una basta","solo ese","solo el 1",
> "no quiero segunda accion","no quiero segunda acción",
> "pasemos al 10","pasemos al 10%","continuemos al 10","continuemos con el 10".
> 
> OMITIR_A3=true SOLO si u contiene:
> "con dos basta","solo dos","solo quiero dos","me quedo con dos",
> "con las dos basta","no quiero tercera","no quiero una tercera",
> "pasemos al 20","pasemos al 20%","continuemos al 20","continuemos con el 20".
> 
> Si no hay AFIRMATIVO ni NEGATIVO ni SOLO ni OMITIR_A3:
> devuelve BASE EXACTO. FIN.
> 
> ====================================================================
> 2.3 FIREWALL A1_70 — RECHAZO/ACEPTACIÓN TEMPRANA
> ====================================================================
> 
> Si BASE.A1_70==null:
> 
>   ACCION70_VISIBLE=true si C_NORM contiene cualquiera:
>   "ACCIÓN 1 DEL 70%:"
>   "ACCIÓN 2 DEL 70%:"
>   "ACCIÓN 3 DEL 70%:"
> 
>   Si ACCION70_VISIBLE=true:
>     TITULO_A1=texto de la misma línea después del marcador 70%, usando LIMPIA_TITULO.
>     OUT=BASE exacto.
> 
>     Si NEGATIVO:
>       OUT.A1_70=null
>       OUT.rechazadas70=AGREGA_UNICO(BASE.rechazadas70,TITULO_A1)
>       OUT.etapa="A1_70"
>       devuelve OUT. FIN ABSOLUTO.
> 
>     Si AFIRMATIVO:
>       OUT.A1_70=TITULO_A1
>       OUT.rechazadas70=REMOVE_SOLO(BASE.rechazadas70,TITULO_A1)
>       OUT.etapa="A2_70"
>       devuelve OUT. FIN ABSOLUTO.
> 
>     devuelve BASE EXACTO. FIN ABSOLUTO.
> 
> CONTROL:
> BASE.A1_70=null
> CAMILA="ACCIÓN 1 DEL 70%: Crea un tablero..."
> USUARIO="no"
> => A1_70=null
> => rechazadas70=["Crea un tablero..."]
> => etapa="A1_70"
> => jamás puede avanzar a A2_70.
> 
> ====================================================================
> 2.4 FIREWALL A2_70 — NO PERMITIR SALTO A 20%/10%
> ====================================================================
> 
> Si BASE.etapa=="A2_70"
> Y BASE.A1_70!=null
> Y BASE.A2_70==null:
> 
>   ACCION70_VISIBLE=true si C_NORM contiene cualquiera:
>   "ACCIÓN 1 DEL 70%:"
>   "ACCIÓN 2 DEL 70%:"
>   "ACCIÓN 3 DEL 70%:"
> 
>   ACCION20_O_10_VISIBLE=true si C_NORM contiene cualquiera:
>   "ACCIÓN 1 DEL 20%:"
>   "ACCIÓN 2 DEL 20%:"
>   "RECURSO 1 DEL 10%:"
>   "RECURSO 2 DEL 10%:"
> 
>   Si ACCION20_O_10_VISIBLE=true:
>     devuelve BASE EXACTO. FIN ABSOLUTO.
> 
>   Si ACCION70_VISIBLE=true:
>     TITULO_A2=texto de la misma línea después del marcador visible 70%, usando LIMPIA_TITULO.
>     OUT=BASE exacto.
> 
>     Si NEGATIVO:
>       OUT.rechazadas70=AGREGA_UNICO(BASE.rechazadas70,TITULO_A2)
>       OUT.etapa="A2_70"
>       devuelve OUT. FIN ABSOLUTO.
> 
>     Si AFIRMATIVO:
>       si CANON(TITULO_A2)==CANON(BASE.A1_70)
>       o CANON(TITULO_A2) existe en CANON(BASE.rechazadas70):
>         devuelve BASE EXACTO. FIN ABSOLUTO.
> 
>       OUT.A2_70=TITULO_A2
>       OUT.rechazadas70=REMOVE_SOLO(BASE.rechazadas70,TITULO_A2)
>       OUT.etapa="A3_70"
>       devuelve OUT. FIN ABSOLUTO.
> 
>   devuelve BASE EXACTO. FIN ABSOLUTO.
> 
> CONTROL CRÍTICO:
> BASE:
> A1_70="Motiva..."
> A2_70=null
> A1_20=null
> A2_20=null
> etapa="A2_70"
> 
> CAMILA="ACCIÓN 2 DEL 20%: Pide a su jefe..."
> USUARIO="si"
> 
> => devuelve BASE EXACTO.
> => A2_20 DEBE seguir null.
> => A1_20 DEBE seguir null.
> => etapa DEBE seguir "A2_70".
> 
> ====================================================================
> 2.45 FIREWALL A1_20 — OBLIGATORIO
> ====================================================================
> 
> Si BASE.A1_70!=null
> Y BASE.A2_70!=null
> Y BASE.A1_20==null:
> 
>   ACCION20_VISIBLE=true si C_NORM contiene:
>   "ACCIÓN 1 DEL 20%:" o "ACCIÓN 2 DEL 20%:".
> 
>   Si ACCION20_VISIBLE=true:
>     TITULO_A1_20=texto de la misma línea después del marcador visible 20%, usando LIMPIA_TITULO.
>     OUT=BASE exacto.
> 
>     Si NEGATIVO:
>       OUT.A1_20=null
>       OUT.A2_20=BASE.A2_20
>       OUT.rechazadas20=AGREGA_UNICO(BASE.rechazadas20,TITULO_A1_20)
>       OUT.etapa="A1_20"
>       devuelve OUT. FIN ABSOLUTO.
> 
>     Si AFIRMATIVO o SOLO:
>       OUT.A1_20=TITULO_A1_20
>       OUT.A2_20=BASE.A2_20
>       OUT.rechazadas20=REMOVE_SOLO(BASE.rechazadas20,TITULO_A1_20)
>       si SOLO: OUT.etapa="R1_10"
>       si no: OUT.etapa="A2_20"
>       devuelve OUT. FIN ABSOLUTO.
> 
>     devuelve BASE EXACTO. FIN ABSOLUTO.
> 
> CONTROL CRÍTICO:
> BASE.A1_20=null
> BASE.rechazadas20=["Pide feedback..."]
> BASE.etapa="A1_20"
> CAMILA="ACCIÓN 2 DEL 20%: Solicita lluvia de ideas..."
> USUARIO="si"
> => OUT.A1_20="Solicita lluvia de ideas..."
> => OUT.A2_20 sigue null
> => rechazadas20 conserva "Pide feedback..."
> => etapa="A2_20".
> 
> ====================================================================
> 2.46 FIREWALL A2_20 — RECHAZO/ACEPTACIÓN TEMPRANA
> ====================================================================
> 
> Si BASE.etapa=="A2_20"
> Y BASE.A1_20!=null
> Y BASE.A2_20==null
> Y C_NORM contiene "ACCIÓN 2 DEL 20%:":
> 
>   TITULO_A2_20=texto de la misma línea después de "ACCIÓN 2 DEL 20%:", usando LIMPIA_TITULO.
>   OUT=BASE exacto.
> 
>   Si NEGATIVO:
>     OUT.rechazadas20=AGREGA_UNICO(BASE.rechazadas20,TITULO_A2_20)
>     OUT.etapa="A2_20"
>     devuelve OUT. FIN ABSOLUTO.
> 
>   Si SOLO:
>     OUT.etapa="R1_10"
>     devuelve OUT. FIN ABSOLUTO.
> 
>   Si AFIRMATIVO:
>     si CANON(TITULO_A2_20)==CANON(BASE.A1_20)
>     o CANON(TITULO_A2_20) existe en CANON(BASE.rechazadas20):
>       devuelve BASE EXACTO. FIN ABSOLUTO.
>     OUT.A2_20=TITULO_A2_20
>     OUT.rechazadas20=REMOVE_SOLO(BASE.rechazadas20,TITULO_A2_20)
>     OUT.etapa="R1_10"
>     devuelve OUT. FIN ABSOLUTO.
> 
>   devuelve BASE EXACTO. FIN ABSOLUTO.
> 
> CONTROL:
> A1_20="Asesorar..."
> A2_20=null
> rechazadas20=[]
> CAMILA="ACCIÓN 2 DEL 20%: Coordinar..."
> USUARIO="no"
> => A2_20=null
> => rechazadas20=["Coordinar..."]
> => etapa=A2_20
> => Coordinar NO puede volver a presentarse.
> 
> ====================================================================
> 2.5 FIREWALL R1_10 — PRESERVACIÓN DE RECHAZADOS
> ====================================================================
> 
> R1_VISIBLE=true si C_NORM contiene:
> "RECURSO 1 DEL 10%:"
> y "¿Confirmas que quieres aceptar el RECURSO 1 DEL 10%?"
> 
> Si R1_VISIBLE=true y BASE.etapa in ["R1_10","A2_20"] y BASE.R1_10==null:
> 
>  TITULO_R1=texto de la misma línea después de "RECURSO 1 DEL 10%:", usando LIMPIA_TITULO.
>  URL_R1=primera URL http/https asociada al recurso.
> 
>  Si NEGATIVO:
>    OUT=BASE exacto.
>    OUT.rechazados10=AGREGA_UNICO(BASE.rechazados10,TITULO_R1).
>    OUT.etapa="R1_10".
>    devuelve OUT. FIN ABSOLUTO.
> 
>  Si AFIRMATIVO o SOLO:
>    OUT=BASE exacto.
>    OUT.R1_10=TITULO_R1.
>    OUT.enlaceR1_10=URL_R1.
> 
>    # REGLA CRÍTICA:
>    # aceptar un recurso distinto NO borra rechazados anteriores.
>    OUT.rechazados10=copia EXACTA BASE.rechazados10.
>    Si CANON(TITULO_R1) existe en CANON(BASE.rechazados10):
>       elimina SOLO TITULO_R1 de OUT.rechazados10.
> 
>    OUT.etapa="R2_10".
>    devuelve OUT. FIN ABSOLUTO.
> 
> CONTROL OBLIGATORIO:
> BASE.rechazados10=["Curso: Gestión de procesos y mejora continua – EdX."]
> TITULO_R1="Taller: Productividad para líderes de equipos – Udemy."
> USUARIO="si"
> => R1_10=Udemy
> => rechazados10=["Curso: Gestión de procesos y mejora continua – EdX."]
> => EdX NO puede quedar libre otra vez.
> 
> ====================================================================
> 3. RESUMEN — PRIORIDAD MÁXIMA Y CERRADA
> ====================================================================
> 
> RESUMEN=true si C_NORM contiene exactamente la pregunta:
> "¿Te parece alcanzable este plan?"
> 
> SI RESUMEN=true:
> - OUT=BASE exacto.
> - NO evalúes fechas, acciones, recursos ni otras ramas.
> 
> CONFIRMACION_RESUMEN=true SOLO si u, después de trim y quitar .,! finales, es EXACTAMENTE uno de:
> "si","sí","acepto","dale","de una","perfecto","claro","me gusta",
> "si me gusta","sí me gusta","lo acepto","la acepto".
> 
> Si CONFIRMACION_RESUMEN=true:
>  OUT.etapa="FECHAS".
>  devuelve OUT. FIN ABSOLUTO.
> 
> Para CUALQUIER otro USUARIO:
>  OUT.etapa="RESUMEN_FINAL".
>  devuelve OUT. FIN ABSOLUTO.
> 
> CONTROLES:
> USUARIO="el 15 de septiembre" => RESUMEN_FINAL.
> USUARIO="15/09/2026" => RESUMEN_FINAL.
> USUARIO="no" => RESUMEN_FINAL.
> USUARIO="averiguar cosas nuevas" => RESUMEN_FINAL.
> USUARIO="si" => FECHAS.
> USUARIO="sí" => FECHAS.
> 
> NINGÚN slot, enlace o lista puede cambiar en esta rama.
> 
> ====================================================================
> 4. EXTRAER PROPUESTA
> ====================================================================
> 
> Detecta en C_NORM el marcador visible:
> M1="ACCIÓN 1 DEL 70%:"
> M2="ACCIÓN 2 DEL 70%:"
> M3="ACCIÓN 3 DEL 70%:"
> M4="ACCIÓN 1 DEL 20%:"
> M5="ACCIÓN 2 DEL 20%:"
> M6="RECURSO 1 DEL 10%:"
> M7="RECURSO 2 DEL 10%:"
> 
> TITULO = texto de la misma línea después del marcador detectado, usando LIMPIA_TITULO.
> 
> Para M6/M7:
> URL = primera URL http/https asociada a ese recurso antes de la pregunta.
> Si está como [texto](URL), usa URL real.
> 
> Si no hay marcador de confirmación reconocido:
> devuelve BASE. FIN.
> 
> ====================================================================
> 5. SWITCH DETERMINISTA
> ====================================================================
> 
> ------------------------------------------------------------
> 5.1 A1_70
> ------------------------------------------------------------
> 
> APLICA si:
> BASE.etapa=="A1_70"
> Y BASE.A1_70==null
> Y C contiene confirmación de CUALQUIER acción del 70%.
> El rótulo visible NO puede desplazar el slot.
> 
> OUT=BASE exacto.
> 
> Si NEGATIVO:
>  OUT.rechazadas70=AGREGA_UNICO(BASE.rechazadas70,TITULO)
>  OUT.etapa="A1_70"
>  devuelve OUT. FIN.
> 
> Si AFIRMATIVO:
>  OUT.A1_70=TITULO
>  OUT.rechazadas70=REMOVE_SOLO(BASE.rechazadas70,TITULO)
>  OUT.etapa="A2_70"
>  devuelve OUT. FIN.
> 
> ------------------------------------------------------------
> 5.2 A2_70
> ------------------------------------------------------------
> 
> APLICA si:
> BASE.etapa=="A2_70"
> Y BASE.A1_70!=null
> Y BASE.A2_70==null
> Y C contiene confirmación de acción 70%.
> 
> OUT=BASE exacto.
> 
> Si NEGATIVO:
>  OUT.rechazadas70=AGREGA_UNICO(BASE.rechazadas70,TITULO)
>  OUT.etapa="A2_70"
>  devuelve OUT. FIN.
> 
> Si AFIRMATIVO:
>  si CANON(TITULO)==CANON(BASE.A1_70) o CANON(TITULO) existe en CANON(BASE.rechazadas70):
>    devuelve BASE. FIN.
>  OUT.A2_70=TITULO
>  OUT.rechazadas70=REMOVE_SOLO(BASE.rechazadas70,TITULO)
>  OUT.etapa="A3_70"
>  devuelve OUT. FIN.
> 
> ------------------------------------------------------------
> 5.3 A3_70
> ------------------------------------------------------------
> 
> APLICA si:
> BASE.etapa=="A3_70"
> Y BASE.A1_70!=null
> Y BASE.A2_70!=null
> Y BASE.A3_70==null
> Y C contiene confirmación de acción 70%.
> 
> OUT=BASE exacto.
> 
> Si OMITIR_A3:
>  OUT.etapa="A1_20"
>  devuelve OUT. FIN.
> 
> Si NEGATIVO:
>  OUT.rechazadas70=AGREGA_UNICO(BASE.rechazadas70,TITULO)
>  OUT.etapa="A3_70"
>  devuelve OUT. FIN.
> 
> Si AFIRMATIVO:
>  si CANON(TITULO) está en CANON([BASE.A1_70,BASE.A2_70]+BASE.rechazadas70):
>    OUT.etapa="A1_20"
>    devuelve OUT. FIN.
>  OUT.A3_70=TITULO
>  OUT.rechazadas70=REMOVE_SOLO(BASE.rechazadas70,TITULO)
>  OUT.etapa="A1_20"
>  devuelve OUT. FIN.
> 
> ------------------------------------------------------------
> 5.4 A1_20
> ------------------------------------------------------------
> 
> APLICA si:
> BASE.etapa en ["A1_20","A3_70"]
> Y BASE.A1_70!=null
> Y BASE.A2_70!=null
> Y BASE.A1_20==null
> Y C contiene confirmación de "ACCIÓN 1 DEL 20%".
> 
> OUT=BASE exacto.
> 
> Si NEGATIVO:
>  OUT.rechazadas20=AGREGA_UNICO(BASE.rechazadas20,TITULO)
>  OUT.etapa="A1_20"
>  devuelve OUT. FIN.
> 
> Si AFIRMATIVO o SOLO:
>  OUT.A1_20=TITULO
>  OUT.rechazadas20=REMOVE_SOLO(BASE.rechazadas20,TITULO)
>  si SOLO: OUT.etapa="R1_10"
>  si no: OUT.etapa="A2_20"
>  devuelve OUT. FIN.
> 
> ------------------------------------------------------------
> 5.5 A2_20
> ------------------------------------------------------------
> 
> APLICA si:
> BASE.etapa=="A2_20"
> Y BASE.A1_20!=null
> Y BASE.A2_20==null
> Y C contiene confirmación de "ACCIÓN 2 DEL 20%".
> 
> OUT=BASE exacto.
> IMPORTANTE: OUT.A1_20 permanece EXACTAMENTE BASE.A1_20 en TODA esta rama.
> 
> Si NEGATIVO:
>  OUT.rechazadas20=AGREGA_UNICO(BASE.rechazadas20,TITULO)
>  OUT.etapa="A2_20"
>  devuelve OUT. FIN.
> 
> Si SOLO:
>  OUT.etapa="R1_10"
>  devuelve OUT. FIN.
> 
> Si AFIRMATIVO:
>  si CANON(TITULO)==CANON(BASE.A1_20) o CANON(TITULO) existe en CANON(BASE.rechazadas20):
>    devuelve BASE. FIN.
>  OUT.A2_20=TITULO
>  OUT.rechazadas20=REMOVE_SOLO(BASE.rechazadas20,TITULO)
>  OUT.etapa="R1_10"
>  devuelve OUT. FIN.
> 
> ------------------------------------------------------------
> 5.6 R1_10
> ------------------------------------------------------------
> 
> APLICA si:
> BASE.etapa en ["R1_10","A2_20"]
> Y BASE.A1_20!=null
> Y BASE.R1_10==null
> Y C contiene confirmación de "RECURSO 1 DEL 10%".
> 
> OUT=BASE exacto.
> IMPORTANTE:
> OUT.A1_20=BASE.A1_20
> OUT.A2_20=BASE.A2_20
> OUT.rechazadas20=BASE.rechazadas20
> 
> Si NEGATIVO:
>  OUT.rechazados10=AGREGA_UNICO(BASE.rechazados10,TITULO)
>  OUT.etapa="R1_10"
>  devuelve OUT. FIN.
> 
> Si AFIRMATIVO o SOLO:
>  OUT.R1_10=TITULO
>  OUT.enlaceR1_10=URL
>  si CANON(TITULO) existe en CANON(BASE.rechazados10):
>    OUT.rechazados10=REMOVE_SOLO(BASE.rechazados10,TITULO)
>  si CANON(TITULO) NO existe en CANON(BASE.rechazados10):
>    OUT.rechazados10=copia EXACTA BASE.rechazados10
>  OUT.etapa="R2_10"
>  devuelve OUT. FIN.
> 
> CONTROL OBLIGATORIO:
> BASE.rechazados10=[EdX], TITULO=Udemy, AFIRMATIVO
> => OUT.rechazados10=[EdX].
> Udemy NO puede borrar EdX.
> 
> ------------------------------------------------------------
> 5.7 R2_10
> ------------------------------------------------------------
> 
> APLICA si:
> BASE.etapa=="R2_10"
> Y BASE.R1_10!=null
> Y BASE.R2_10==null
> Y C contiene confirmación de "RECURSO 2 DEL 10%".
> 
> OUT=BASE exacto.
> IMPORTANTE: todos los slots 70/20 y R1 permanecen BASE.
> 
> Si NEGATIVO o SOLO:
>  OUT.rechazados10=AGREGA_UNICO(BASE.rechazados10,TITULO) SOLO si NEGATIVO
>  OUT.etapa="RESUMEN_FINAL"
>  devuelve OUT. FIN.
> 
> Si AFIRMATIVO:
>  si CANON(TITULO)==CANON(BASE.R1_10):
>    OUT.etapa="RESUMEN_FINAL"
>    devuelve OUT. FIN.
>  si CANON(TITULO) existe en CANON(BASE.rechazados10):
>    # Un recurso rechazado como R1 jamás puede revivir como R2.
>    OUT.R2_10=null
>    OUT.enlaceR2_10=null
>    OUT.rechazados10=copia EXACTA BASE.rechazados10
>    OUT.etapa="RESUMEN_FINAL"
>    devuelve OUT. FIN ABSOLUTO.
>  OUT.R2_10=TITULO
>  OUT.enlaceR2_10=URL
>  OUT.rechazados10=REMOVE_SOLO(BASE.rechazados10,TITULO)
>  OUT.etapa="RESUMEN_FINAL"
>  devuelve OUT. FIN.
> 
> ====================================================================
> 6. SALIDA SIN RAMA
> ====================================================================
> 
> Devuelve BASE exacto.
> FIN.
> 
> ====================================================================
> 7. INVARIANTES FINALES OBLIGATORIAS
> ====================================================================
> 
> Antes de devolver cualquier OUT verifica:
> 
> 1. Si BASE.A1_20!=null y la rama NO es 5.4:
>    OUT.A1_20 DEBE ser exactamente BASE.A1_20.
> 
> 2. Si BASE.A1_70!=null y la rama no es 5.1:
>    OUT.A1_70=BASE.A1_70.
> 
> 3. Si BASE.A2_70!=null y la rama no es 5.2:
>    OUT.A2_70=BASE.A2_70.
> 
> 4. Si BASE.A3_70!=null y la rama no es 5.3:
>    OUT.A3_70=BASE.A3_70.
> 
> 5. Toda rama 10%:
>    OUT.A1_20=BASE.A1_20
>    OUT.A2_20=BASE.A2_20
>    OUT.rechazadas20=BASE.rechazadas20.
> 
> 6. Para CADA x en BASE.rechazados10:
>    si CANON(x)!=CANON(TITULO aceptado):
>       x DEBE seguir en OUT.rechazados10.
>    Si TITULO aceptado NO estaba en BASE.rechazados10:
>       OUT.rechazados10 DEBE ser IDÉNTICO a BASE.rechazados10.
> 
> 7. Nunca puede existir:
>    A1_20==null y A2_20!=null.
>    Si BASE ya cumple la jerarquía, ninguna rama puede romperla.
> 
> 8. Exactamente 13 claves. Sin Markdown. Sin explicaciones.
> 
> CASO DE REGRESIÓN CRÍTICO:
> BASE:
> A1_20="Sesión quincenal..."
> A2_20=null
> rechazadas20=[]
> etapa="A2_20"
> 
> CAMILA="ACCIÓN 2 DEL 20%: Asesorar..."
> USUARIO="no"
> 
> => A1_20 sigue "Sesión quincenal..."
> => A2_20=null
> => rechazadas20=["Asesorar..."]
> => etapa="A2_20"
> 
> Siguiente:
> CAMILA="ACCIÓN 2 DEL 20%: Coordinar..."
> USUARIO="si"
> 
> => A1_20 sigue "Sesión quincenal..."
> => A2_20="Coordinar..."
> => etapa="R1_10"
> 
> FIN DEL PROMPT.
> 

### Generador · extractor_acciones

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 02. Mensaje2 (al entrar); 02.1. Conversacional Evaluacion Desempeño › 03. Contexto sobre fechas (al entrar); 03.1. Conversacional Lider Misión › 02. Mensaje2 (al entrar); 03.1. Conversacional Lider Misión › 03. Contexto sobre fechas (al entrar)

Prompt:

> Eres un extractor determinista de ítems aceptados de un Plan de Desarrollo Individual 70/20/10.
> 
> Tu única tarea es analizar el siguiente estado JSON:
> 
> $estado_pdi
> 
> El objeto puede contener estos campos:
> 
> A1_70
> A2_70
> A3_70
> A1_20
> A2_20
> R1_10
> enlaceR1_10
> R2_10
> enlaceR2_10
> rechazadas70
> rechazadas20
> rechazados10
> etapa
> 
> Debes devolver exclusivamente los ítems ACEPTADOS que requieren una fecha de compromiso.
> 
> ============================================================
> ORDEN OBLIGATORIO
> ============================================================
> 
> Evalúa los campos exactamente en este orden:
> 
> 1. A1_70
> 2. A2_70
> 3. A3_70
> 4. A1_20
> 5. A2_20
> 6. R1_10
> 7. R2_10
> 
> Este orden es obligatorio.
> 
> NO cambies el orden aunque existan campos null entre ellos.
> 
> Los campos null simplemente se omiten.
> 
> ============================================================
> REGLAS ABSOLUTAS
> ============================================================
> 
> 1. Si uno de estos campos contiene un texto distinto de null, inclúyelo en el arreglo:
> 
> A1_70
> A2_70
> A3_70
> A1_20
> A2_20
> R1_10
> R2_10
> 
> 2. Si uno de esos campos es null, omítelo completamente.
> 
> 3. Nunca insertes null en el arreglo de salida.
> 
> 4. Nunca inventes un valor para un campo null.
> 
> 5. Nunca completes slots faltantes.
> 
> 6. Nunca reconstruyas una acción o recurso desde:
> - conversaciones anteriores;
> - rechazos;
> - propuestas;
> - resúmenes;
> - otros parámetros.
> 
> 7. Nunca uses etapa para decidir qué elementos incluir.
> 
> 8. Nunca incluyas en el arreglo:
> 
> - enlaceR1_10
> - enlaceR2_10
> - rechazadas70
> - rechazadas20
> - rechazados10
> - etapa
> 
> 9. Los recursos R1_10 y R2_10 SÍ requieren fecha de compromiso.
> 
> Por tanto:
> 
> R1_10 distinto de null
> → incluir R1_10.
> 
> R2_10 distinto de null
> → incluir R2_10.
> 
> 10. Conserva literalmente el texto de cada valor.
> 
> No resumas.
> No corrijas.
> No reformules.
> No traduzcas.
> No cambies mayúsculas.
> No cambies puntuación.
> No agregues ni elimines palabras.
> 
> 11. La cantidad final del arreglo debe ser EXACTAMENTE igual a la cantidad de campos no nulos entre:
> 
> A1_70
> A2_70
> A3_70
> A1_20
> A2_20
> R1_10
> R2_10
> 
> 12. La cantidad puede variar según el plan construido.
> 
> De acuerdo con la metodología actual:
> 
> 70%:
> mínimo 2
> máximo 3
> 
> 20%:
> mínimo 1
> máximo 2
> 
> 10%:
> mínimo 1
> máximo 2
> 
> Por lo tanto, el arreglo final normalmente puede contener entre 4 y 7 elementos.
> 
> NO fuerces una cantidad fija.
> 
> La fuente de verdad es únicamente la cantidad real de campos no nulos.
> 
> ============================================================
> EJEMPLO 1 — DOS RECURSOS
> ============================================================
> 
> Si recibes:
> 
> {
>   "A1_70": "Acción uno",
>   "A2_70": "Acción dos",
>   "A3_70": null,
>   "A1_20": "Acción tres",
>   "A2_20": null,
>   "R1_10": "Curso uno",
>   "enlaceR1_10": "https://ejemplo1.com",
>   "R2_10": "Curso dos",
>   "enlaceR2_10": "https://ejemplo2.com",
>   "rechazadas70": [],
>   "rechazadas20": [],
>   "rechazados10": [],
>   "etapa": "FIN_PDI"
> }
> 
> debes devolver exactamente:
> 
> ["Acción uno","Acción dos","Acción tres","Curso uno","Curso dos"]
> 
> Cantidad final:
> 
> 5
> 
> ============================================================
> EJEMPLO 2 — UN SOLO RECURSO
> ============================================================
> 
> Si recibes:
> 
> {
>   "A1_70": "Acción uno",
>   "A2_70": "Acción dos",
>   "A3_70": null,
>   "A1_20": "Acción tres",
>   "A2_20": null,
>   "R1_10": "Curso uno",
>   "enlaceR1_10": "https://ejemplo1.com",
>   "R2_10": null,
>   "enlaceR2_10": null,
>   "rechazadas70": [],
>   "rechazadas20": [],
>   "rechazados10": [],
>   "etapa": "FIN_PDI"
> }
> 
> debes devolver exactamente:
> 
> ["Acción uno","Acción dos","Acción tres","Curso uno"]
> 
> Cantidad final:
> 
> 4
> 
> ============================================================
> EJEMPLO 3 — PLAN MÁXIMO
> ============================================================
> 
> Si recibes:
> 
> {
>   "A1_70": "Acción 70 uno",
>   "A2_70": "Acción 70 dos",
>   "A3_70": "Acción 70 tres",
>   "A1_20": "Acción 20 uno",
>   "A2_20": "Acción 20 dos",
>   "R1_10": "Recurso uno",
>   "enlaceR1_10": "https://ejemplo1.com",
>   "R2_10": "Recurso dos",
>   "enlaceR2_10": "https://ejemplo2.com",
>   "rechazadas70": [],
>   "rechazadas20": [],
>   "rechazados10": [],
>   "etapa": "FIN_PDI"
> }
> 
> debes devolver exactamente:
> 
> ["Acción 70 uno","Acción 70 dos","Acción 70 tres","Acción 20 uno","Acción 20 dos","Recurso uno","Recurso dos"]
> 
> Cantidad final:
> 
> 7
> 
> ============================================================
> VALIDACIÓN FINAL OBLIGATORIA
> ============================================================
> 
> Antes de responder verifica uno por uno:
> 
> A1_70:
> - si no es null → incluido;
> - si es null → omitido.
> 
> A2_70:
> - si no es null → incluido;
> - si es null → omitido.
> 
> A3_70:
> - si no es null → incluido;
> - si es null → omitido.
> 
> A1_20:
> - si no es null → incluido;
> - si es null → omitido.
> 
> A2_20:
> - si no es null → incluido;
> - si es null → omitido.
> 
> R1_10:
> - si no es null → incluido;
> - si es null → omitido.
> 
> R2_10:
> - si no es null → incluido;
> - si es null → omitido.
> 
> Después confirma:
> 
> - no falta ningún campo no nulo;
> - no agregaste ningún campo null;
> - R2_10 fue evaluado;
> - enlaceR1_10 no fue incluido;
> - enlaceR2_10 no fue incluido;
> - rechazadas70 no fue incluido;
> - rechazadas20 no fue incluido;
> - rechazados10 no fue incluido;
> - etapa no fue incluida;
> - el orden final respeta:
>   A1_70,
>   A2_70,
>   A3_70,
>   A1_20,
>   A2_20,
>   R1_10,
>   R2_10.
> 
> ============================================================
> FORMATO DE SALIDA
> ============================================================
> 
> Devuelve exclusivamente un arreglo JSON plano de textos.
> 
> Debe ser JSON válido.
> 
> Debe estar en una sola línea.
> 
> No agregues:
> - explicaciones;
> - introducciones;
> - Markdown;
> - bloques ```json;
> - propiedades adicionales;
> - cantidad;
> - texto antes o después del arreglo.
> 
> Devuelve únicamente el arreglo JSON.

### Generador · extractor_materiales

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 02. Mensaje2 (al entrar); 02.1. Conversacional Evaluacion Desempeño › 03. Contexto sobre fechas (al entrar); 03.1. Conversacional Lider Misión › 02. Mensaje2 (al entrar); 03.1. Conversacional Lider Misión › 03. Contexto sobre fechas (al entrar)

Prompt:

> Analiza el siguiente texto:
> 
> "\$resumen"
> 
> REGLA DE SEGURIDAD ABSOLUTA: 
> Si el texto anterior NO contiene de forma explícita la sección "MATERIALES COMPLEMENTARIOS" o contiene la frase "DEFINIR FECHAS", responde únicamente un arreglo vacío: []
> 
> Si el texto SÍ contiene la sección de materiales, actúa como un extractor de datos ultra estricto. Ignora por completo las secciones de PRÁCTICA, MENTORÍA, FORMACIÓN y META. Extrae únicamente los elementos bajo el título "MATERIALES COMPLEMENTARIOS" (como la película/documental y el podcast).
> 
> Devuelve EXCLUSIVAMENTE un arreglo JSON con el formato del siguiente ejemplo, sin explicaciones, sin introducciones y sin usar marcas de código como ```json:
> 
> [
>   {
>     "titulo": "Cadena de favores",
>     "enlace": "https://primevideo.com..."
>   },
>   {
>     "titulo": "Ser influyente",
>     "enlace": "https://spotify.com..."
>   }
> ]
> 

### Generador · extraer_accion_fecha_actual

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)

Prompt:

> Analiza el siguiente objeto JSON:
> 
> $jsonFechas
> 
> Extrae exclusivamente el valor de la propiedad:
> 
> itemProcesado.accion
> 
> Si itemProcesado es null, devuelve exclusivamente:
> 
> VACIO
> 
> No agregues explicaciones.
> No agregues comillas.
> No uses Markdown.
> No modifiques el texto.

### Generador · extraer_contador_accion_fecha_actual

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)

Prompt:

> Analiza el siguiente objeto JSON:
> 
> $jsonFechas
> 
> Extrae exclusivamente el valor numérico de la propiedad:
> 
> itemProcesado.contadorAcciones
> 
> Si itemProcesado es null, devuelve exclusivamente:
> 
> 0
> 
> No agregues explicaciones.
> No agregues texto adicional.

### Generador · extraer_estado_fechas

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)

Prompt:

> Analiza el siguiente contenido:
> 
> $json_fechas
> 
> Tu función es determinar el estado REAL del proceso a partir del contenido de pdiFechas y fechaValida.
> 
> NO confíes ciegamente en la propiedad "estado" si contradice el arreglo pdiFechas.
> 
> Reglas en este orden:
> 
> 1. Si el contenido no es un objeto JSON válido, no contiene pdiFechas o contiene el texto "Lo siento pero no tengo información al respecto", devuelve exclusivamente:
> ERROR
> 
> 2. Si fechaValida es false, devuelve exclusivamente:
> REINTENTAR
> 
> 3. Si pdiFechas contiene al menos un objeto cuya propiedad fecha sea null, devuelve exclusivamente:
> PREGUNTAR
> 
> Esto aplica incluso si la propiedad "estado" dice "COMPLETO".
> 
> 4. Si pdiFechas contiene uno o más objetos y TODOS tienen fecha distinta de null, devuelve exclusivamente:
> COMPLETO
> 
> 5. Durante la inicialización, si fechaValida es null y existen objetos con fecha null, devuelve exclusivamente:
> PREGUNTAR
> 
> Valores permitidos:
> PREGUNTAR
> REINTENTAR
> COMPLETO
> ERROR
> 
> No agregues explicaciones.
> No agregues comillas.
> No uses Markdown.
> No agregues ningún otro texto.
> 

### Generador · extraer_fecha_compromiso_actual

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)

Prompt:

> Analiza el siguiente objeto JSON:
> 
> $jsonFechas
> 
> Extrae exclusivamente el valor de la propiedad:
> 
> itemProcesado.fechaCompromiso
> 
> Si itemProcesado es null, devuelve exclusivamente:
> 
> VACIO
> 
> Conserva la fecha en formato YYYY-MM-DD.
> 
> No agregues explicaciones.
> No agregues comillas.
> No uses Markdown.

### Generador · extraer_fecha_valida

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)

Prompt:

> Analiza el siguiente objeto JSON:
> 
> $jsonFechas
> 
> Extrae exclusivamente el valor de la propiedad:
> 
> fechaValida
> 
> Devuelve únicamente uno de estos valores:
> 
> true
> false
> null
> 
> No agregues explicaciones.
> No agregues comillas.
> No agregues ningún otro texto.

### Generador · extraer_json_fechas

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)

Prompt:

> Analiza el siguiente objeto JSON:
> 
> $jsonFechas
> 
> Extrae exclusivamente el arreglo completo contenido en la propiedad pdiFechas.
> 
> Devuelve todos los objetos del arreglo, desde el primero hasta el último.
> 
> La respuesta debe ser un arreglo JSON completo, válido y cerrado.
> 
> Devuelve el JSON de forma compacta, en una sola línea, sin espacios ni saltos de línea innecesarios.
> 
> No cortes ningún objeto.
> No omitas elementos.
> No resumas las acciones.
> No modifiques los textos.
> No devuelvas el objeto principal.
> No uses Markdown.
> No agregues explicaciones ni texto adicional.

### Generador · extraer_mensaje_fechas

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)

Prompt:

> Analiza el siguiente objeto JSON:
> 
> $json_fechas
> 
> FECHA MÍNIMA PERMITIDA:
> 
> $fecha_minima
> 
> FECHA MÁXIMA PERMITIDA:
> 
> $fecha_maxima
> 
> Tu única función es determinar qué mensaje debe mostrarse al usuario.
> 
> La fuente de verdad para saber si queda un ítem pendiente es el arreglo pdiFechas.
> 
> ============================================================
> PRIORIDAD 1 — REINTENTO POR FECHA INVÁLIDA
> ============================================================
> 
> Si fechaValida es false o la propiedad estado es "REINTENTAR":
> 
> Devuelve exclusivamente el contenido de la propiedad "mensaje".
> 
> ============================================================
> PRIORIDAD 2 — TODAVÍA EXISTEN FECHAS NULL
> ============================================================
> 
> Si dentro de pdiFechas existe al menos un objeto cuya propiedad fecha sea null:
> 
> IGNORA las propiedades:
> - estado
> - mensaje
> - indiceActual
> - itemActual
> 
> si contradicen el arreglo.
> 
> Busca el PRIMER objeto cuya fecha sea null.
> 
> Usa:
> - su propiedad orden como número actual;
> - su propiedad accion como texto literal;
> - la cantidad total de objetos de pdiFechas como total.
> 
> Devuelve exclusivamente:
> 
> 📅 FECHA [orden] DE [total]
> 
> Ítem: [accion]
> 
> Indícame la fecha de compromiso en formato DD/MM/AAAA. Debe estar entre [FECHA MÍNIMA PERMITIDA] y [FECHA MÁXIMA PERMITIDA].
> 
> Ejemplo:
> 
> orden 1 → fecha 2026-09-10
> orden 2 → fecha 2026-09-20
> orden 3 → fecha null
> 
> Debes devolver FECHA 3 aunque la propiedad estado diga COMPLETO o mensaje diga FECHA 2.
> 
> ============================================================
> PRIORIDAD 3 — PLAN COMPLETO
> ============================================================
> 
> Si pdiFechas contiene uno o más objetos y ninguno tiene fecha null:
> 
> Devuelve exclusivamente:
> 
> ✅ Todas las fechas del plan fueron registradas correctamente. Estoy generando tu documento final.
> 
> No muestres el JSON.
> No agregues explicaciones.
> No uses Markdown adicional.
> No modifiques el texto de la acción.
> No elimines espacios dentro del texto de la acción.
> Devuelve únicamente el mensaje visible.

### Generador · procesar_fechas_pdi

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.15 Generar JSON fechas (al entrar); 03.1. Conversacional Lider Misión › 03.15 Generar JSON fechas (al entrar)

Prompt:

> Eres un transformador determinista de estado JSON para las fechas de un Plan de Desarrollo Individual.
> 
> NO interpretes lenguaje natural.
> NO calcules fechas.
> NO decidas qué índice corresponde.
> NO reutilices itemProcesado ni fechas de turnos anteriores.
> 
> Dialogflow ya determinó:
> - la fecha exacta normalizada del turno;
> - el índice exacto que debes procesar;
> - el total exacto de ítems del plan.
> 
> Tu función es únicamente actualizar el arreglo de estado de forma consistente.
> 
> ============================================================
> ENTRADAS
> ============================================================
> 
> ACCIONES ACEPTADAS:
> 
> $acciones
> 
> ESTADO ACUMULADO:
> 
> $parcial
> 
> FECHA NORMALIZADA DEL TURNO ACTUAL:
> 
> $fecha_normalizada
> 
> ÍNDICE QUE DEBES PROCESAR:
> 
> $indice_esperado
> 
> TOTAL DE ÍTEMS DEL PLAN:
> 
> $total_esperado
> 
> FECHA MÍNIMA:
> 
> $minima
> 
> FECHA MÁXIMA:
> 
> $maxima
> 
> ============================================================
> REGLAS ABSOLUTAS
> ============================================================
> 
> 1. Conserva literalmente el texto de todas las acciones.
> 2. No elimines espacios internos de las acciones.
> 3. No resumas, reformules, traduzcas ni corrijas las acciones.
> 4. No cambies el orden de los elementos.
> 5. No borres fechas ya registradas.
> 6. Modifica como máximo un elemento por turno.
> 7. FECHA NORMALIZADA ya viene validada por Dialogflow y está en formato YYYY-MM-DD.
> 8. No interpretes ni reformatees FECHA NORMALIZADA.
> 9. ÍNDICE QUE DEBES PROCESAR es la autoridad absoluta sobre el elemento que puede modificarse.
> 10. TOTAL DE ÍTEMS DEL PLAN es la autoridad absoluta para decidir si el plan continúa o termina.
> 11. Está prohibido devolver COMPLETO si queda al menos un objeto con fecha null.
> 12. Está prohibido devolver COMPLETO si ÍNDICE QUE DEBES PROCESAR es menor que TOTAL DE ÍTEMS DEL PLAN.
> 13. pdiFechas siempre debe contener todos los elementos del plan.
> 14. totalItems debe coincidir con TOTAL DE ÍTEMS DEL PLAN y con la cantidad de ACCIONES ACEPTADAS.
> 
> ============================================================
> INICIALIZACIÓN
> ============================================================
> 
> Si ESTADO ACUMULADO es null, está vacío o es exactamente []:
> 
> Convierte cada elemento de ACCIONES ACEPTADAS en un objeto con esta estructura:
> 
> {
>   "orden": número consecutivo comenzando en 1,
>   "accion": texto literal recibido,
>   "fecha": null
> }
> 
> No proceses FECHA NORMALIZADA durante la inicialización.
> No uses ÍNDICE QUE DEBES PROCESAR para registrar una fecha durante la inicialización.
> 
> Devuelve:
> 
> estado = "PREGUNTAR"
> completo = false
> fechaValida = null
> persistirCambios = true
> motivoError = null
> indiceActual = 1
> totalItems = TOTAL DE ÍTEMS DEL PLAN
> itemActual = objeto de orden 1
> itemProcesado = null
> pdiFechas = arreglo completo inicializado
> 
> mensaje:
> 
> 📅 FECHA 1 DE [TOTAL DE ÍTEMS DEL PLAN]
> 
> Ítem: [acción del objeto 1]
> 
> Indícame la fecha de compromiso. Debe estar entre [FECHA MÍNIMA] y [FECHA MÁXIMA].
> 
> ============================================================
> VALIDACIÓN DEL ESTADO ANTES DE PROCESAR
> ============================================================
> 
> Si ESTADO ACUMULADO ya contiene objetos:
> 
> Convierte ÍNDICE QUE DEBES PROCESAR y TOTAL DE ÍTEMS DEL PLAN a números enteros.
> 
> Si ÍNDICE QUE DEBES PROCESAR es menor que 1 o mayor que TOTAL DE ÍTEMS DEL PLAN:
> 
> NO modifiques nada.
> 
> Devuelve:
> 
> estado = "REINTENTAR"
> completo = false
> fechaValida = false
> persistirCambios = false
> motivoError = "DESFASE_ESTADO"
> itemProcesado = null
> pdiFechas = ESTADO ACUMULADO sin modificaciones
> 
> Si no existe un objeto cuyo orden sea exactamente igual a ÍNDICE QUE DEBES PROCESAR:
> 
> NO modifiques nada y devuelve el mismo resultado REINTENTAR anterior.
> 
> Si el objeto cuyo orden coincide con ÍNDICE QUE DEBES PROCESAR ya tiene una fecha distinta de null:
> 
> NO lo vuelvas a procesar.
> NO modifiques nada.
> Devuelve el mismo resultado REINTENTAR anterior.
> 
> ============================================================
> PROCESAMIENTO DE LA FECHA
> ============================================================
> 
> Si las validaciones anteriores son correctas:
> 
> Localiza exclusivamente el objeto cuyo:
> 
> orden = ÍNDICE QUE DEBES PROCESAR
> 
> Ese objeto debe tener:
> 
> fecha = null
> 
> Asigna exactamente:
> 
> fecha = FECHA NORMALIZADA DEL TURNO ACTUAL
> 
> No modifiques ningún otro elemento.
> 
> ============================================================
> ITEM PROCESADO
> ============================================================
> 
> Después de actualizar el objeto, itemProcesado debe ser exactamente:
> 
> {
>   "accion": texto literal de la acción actualizada,
>   "contadorAcciones": ÍNDICE QUE DEBES PROCESAR como número,
>   "fechaCompromiso": FECHA NORMALIZADA DEL TURNO ACTUAL
> }
> 
> Validación obligatoria:
> 
> itemProcesado.contadorAcciones = ÍNDICE QUE DEBES PROCESAR
> 
> Y:
> 
> itemProcesado.fechaCompromiso = FECHA NORMALIZADA DEL TURNO ACTUAL
> 
> Nunca devuelvas contadorAcciones ni fechaCompromiso de un turno anterior.
> 
> ============================================================
> DETERMINAR SI CONTINÚA O TERMINA
> ============================================================
> 
> Después de actualizar el elemento, revisa TODO el arreglo pdiFechas.
> 
> Busca el primer objeto cuya fecha siga siendo null.
> 
> ------------------------------------------------------------
> CASO A — TODAVÍA QUEDAN FECHAS NULL
> ------------------------------------------------------------
> 
> Si existe al menos un objeto con fecha null:
> 
> Está PROHIBIDO devolver estado "COMPLETO".
> 
> Debes devolver:
> 
> estado = "PREGUNTAR"
> completo = false
> fechaValida = true
> persistirCambios = true
> motivoError = null
> 
> indiceActual = orden del PRIMER objeto cuya fecha sea null
> 
> itemActual = ese mismo objeto pendiente
> 
> itemProcesado = objeto actualizado en ESTE turno
> 
> pdiFechas = arreglo completo actualizado
> 
> totalItems = TOTAL DE ÍTEMS DEL PLAN
> 
> mensaje:
> 
> 📅 FECHA [indiceActual] DE [TOTAL DE ÍTEMS DEL PLAN]
> 
> Ítem: [acción literal de itemActual]
> 
> Indícame la fecha de compromiso. Debe estar entre [FECHA MÍNIMA] y [FECHA MÁXIMA].
> 
> REGLA DE CONTROL OBLIGATORIA:
> 
> Si ÍNDICE QUE DEBES PROCESAR < TOTAL DE ÍTEMS DEL PLAN,
> normalmente debe existir al menos un elemento pendiente.
> 
> Ejemplo obligatorio:
> 
> ÍNDICE QUE DEBES PROCESAR = 5
> TOTAL DE ÍTEMS DEL PLAN = 6
> 
> Entonces:
> 
> estado = "PREGUNTAR"
> completo = false
> indiceActual = 6
> 
> Está absolutamente PROHIBIDO devolver COMPLETO en este caso.
> 
> ------------------------------------------------------------
> CASO B — YA NO QUEDA NINGUNA FECHA NULL
> ------------------------------------------------------------
> 
> Solamente puedes devolver COMPLETO si se cumplen SIMULTÁNEAMENTE estas dos condiciones:
> 
> 1. ÍNDICE QUE DEBES PROCESAR = TOTAL DE ÍTEMS DEL PLAN.
> 2. Ningún objeto de pdiFechas conserva fecha = null.
> 
> Si ambas condiciones son verdaderas:
> 
> estado = "COMPLETO"
> completo = true
> fechaValida = true
> persistirCambios = true
> motivoError = null
> indiceActual = TOTAL DE ÍTEMS DEL PLAN
> itemActual = null
> itemProcesado = objeto actualizado en ESTE turno
> pdiFechas = arreglo completo actualizado
> totalItems = TOTAL DE ÍTEMS DEL PLAN
> 
> mensaje debe ser exactamente:
> 
> ✅ Todas las fechas del plan fueron registradas correctamente. Estoy generando tu documento final.
> 
> ============================================================
> EJEMPLOS DE CONTROL
> ============================================================
> 
> Ejemplo 1:
> 
> TOTAL DE ÍTEMS DEL PLAN = 6
> ÍNDICE QUE DEBES PROCESAR = 5
> 
> Después de actualizar el 5, el 6 sigue null.
> 
> Resultado obligatorio:
> 
> estado = "PREGUNTAR"
> completo = false
> indiceActual = 6
> 
> Ejemplo 2:
> 
> TOTAL DE ÍTEMS DEL PLAN = 6
> ÍNDICE QUE DEBES PROCESAR = 6
> 
> Después de actualizar el 6, ninguna fecha queda null.
> 
> Resultado obligatorio:
> 
> estado = "COMPLETO"
> completo = true
> 
> Ejemplo 3:
> 
> TOTAL DE ÍTEMS DEL PLAN = 5
> ÍNDICE QUE DEBES PROCESAR = 5
> 
> Después de actualizar el 5, ninguna fecha queda null.
> 
> Resultado obligatorio:
> 
> estado = "COMPLETO"
> completo = true
> 
> Ejemplo 4:
> 
> TOTAL DE ÍTEMS DEL PLAN = 6
> ÍNDICE QUE DEBES PROCESAR = 5
> 
> Aunque por error algún dato anterior sugiera que el plan terminó,
> si el elemento 6 tiene fecha null:
> 
> estado = "PREGUNTAR"
> completo = false
> indiceActual = 6
> 
> ============================================================
> GUARDA DE PRODUCCIÓN — ÍNDICE INMUTABLE
> ============================================================
> 
> Antes de devolver la salida realiza una validación final obligatoria:
> 
> - Convierte ÍNDICE QUE DEBES PROCESAR a entero K.
> - Compara ESTADO ACUMULADO contra pdiFechas de salida.
> - Debe existir EXACTAMENTE un cambio de fecha y debe corresponder al objeto orden=K.
> - itemProcesado.contadorAcciones debe ser exactamente K.
> - itemProcesado.accion debe ser exactamente la accion del objeto orden=K.
> - itemProcesado.fechaCompromiso debe ser exactamente FECHA NORMALIZADA.
> - Ningún objeto de orden distinto de K puede recibir FECHA NORMALIZADA en este turno.
> 
> Si cualquiera de estas condiciones falla:
> DESCARTA la mutación y devuelve:
> estado="REINTENTAR"
> completo=false
> fechaValida=false
> persistirCambios=false
> motivoError="DESFASE_ESTADO"
> itemProcesado=null
> pdiFechas=ESTADO ACUMULADO sin modificaciones.
> 
> Nunca intentes "corregir" el índice usando el primer null, un contador anterior
> ni itemProcesado de otro turno. K es la única autoridad del elemento procesado.
> 
> ============================================================
> ============================================================
> SALIDA OBLIGATORIA
> ============================================================
> 
> Devuelve exclusivamente un único objeto JSON válido con exactamente estas propiedades:
> 
> {
>   "estado": "PREGUNTAR",
>   "completo": false,
>   "fechaValida": null,
>   "persistirCambios": true,
>   "motivoError": null,
>   "mensaje": "",
>   "indiceActual": 1,
>   "totalItems": 0,
>   "itemActual": null,
>   "itemProcesado": null,
>   "pdiFechas": []
> }
> 
> estado solamente puede ser:
> 
> "PREGUNTAR"
> "REINTENTAR"
> "COMPLETO"
> 
> No uses Markdown.
> No uses bloques de código.
> No uses ```json.
> No agregues explicaciones.
> No agregues texto antes del objeto JSON.
> No agregues texto después del objeto JSON.
> No elimines espacios dentro de los valores de accion.
> Siempre devuelve el objeto JSON completo y cerrado.
> 

### Generador · resolver_seleccion_competencia_pdi

Se usa en: (no se usa)

Prompt:

> Eres un clasificador determinista para seleccionar cuál de dos
> competencias desea trabajar el usuario en un Plan de Desarrollo Individual.
> 
> COMPETENCIA 1:
> $session.params.competenciaMenor1
> 
> COMPETENCIA 2:
> $session.params.competenciaMenor2
> 
> RESPUESTA DEL USUARIO:
> $session.params.mensajeSeleccionCompetencia
> 
> Tu única función es determinar si el usuario seleccionó
> COMPETENCIA 1, COMPETENCIA 2 o todavía no realizó una selección clara.
> 
> Debes comprender lenguaje natural y equivalencias evidentes.
> 
> Ejemplos:
> 
> Si COMPETENCIA 1 = Curiosidad y creatividad
> y COMPETENCIA 2 = Eficiencia profesional:
> 
> Usuario:
> "curiosidad"
> → SELECCION_1
> 
> Usuario:
> "quiero trabajar curiosidad y creatividad"
> → SELECCION_1
> 
> Usuario:
> "la primera"
> → SELECCION_1
> 
> Usuario:
> "eficiencia"
> → SELECCION_2
> 
> Usuario:
> "eficiencia profesional"
> → SELECCION_2
> 
> Usuario:
> "quiero mejorar eficiencia"
> → SELECCION_2
> 
> Usuario:
> "la segunda"
> → SELECCION_2
> 
> Si el usuario dice que no sabe cuál escoger, que ambas,
> pregunta cuál recomiendas o expresa indecisión:
> → INDECISO
> 
> Si solicita trabajar una competencia diferente de las dos ofrecidas:
> → OTRA_COMPETENCIA
> 
> Si el mensaje no permite identificar con suficiente certeza
> una de las dos opciones:
> → AMBIGUA
> 
> No inventes una selección.
> 
> No decidas por el usuario cuál competencia debe trabajar.
> 
> No selecciones una competencia únicamente porque aparece primero.
> 
> Devuelve EXCLUSIVAMENTE uno de estos valores:
> 
> SELECCION_1
> SELECCION_2
> INDECISO
> OTRA_COMPETENCIA
> AMBIGUA
> 
> No agregues explicaciones.
> No agregues Markdown.
> No agregues puntuación.
> No agregues ningún otro texto.

### Generador · validar

Se usa en: 02.1. Conversacional Evaluacion Desempeño › PDI - Seleccionar competencia (ruta condición); 03.1. Conversacional Lider Misión › PDI - Seleccionar competencia (ruta condición)

Prompt:

> Eres un clasificador determinista para seleccionar cuál de dos competencias desea trabajar el usuario en un Plan de Desarrollo Individual.
> 
> COMPETENCIA 1:
> $competencia1
> 
> COMPETENCIA 2:
> $competencia2
> 
> RESPUESTA DEL USUARIO:
> $mensajeUsuario
> 
> Tu única función es determinar si el usuario seleccionó COMPETENCIA 1, COMPETENCIA 2 o todavía no realizó una selección clara.
> 
> Debes comprender lenguaje natural y equivalencias evidentes.
> 
> Ejemplos:
> 
> Si COMPETENCIA 1 = Curiosidad y creatividad
> y COMPETENCIA 2 = Eficiencia profesional:
> 
> Usuario:
> "curiosidad"
> → SELECCION_1
> 
> Usuario:
> "quiero trabajar curiosidad y creatividad"
> → SELECCION_1
> 
> Usuario:
> "la primera"
> → SELECCION_1
> 
> Usuario:
> "eficiencia"
> → SELECCION_2
> 
> Usuario:
> "eficiencia profesional"
> → SELECCION_2
> 
> Usuario:
> "quiero mejorar eficiencia"
> → SELECCION_2
> 
> Usuario:
> "la segunda"
> → SELECCION_2
> 
> Si el usuario dice que no sabe cuál escoger, que quiere ambas,
> pregunta cuál recomiendas o expresa indecisión:
> → INDECISO
> 
> Si solicita trabajar una competencia diferente de las dos ofrecidas:
> → OTRA_COMPETENCIA
> 
> Si el mensaje no permite identificar con suficiente certeza una de las dos opciones:
> → AMBIGUA
> 
> No inventes una selección.
> No decidas por el usuario cuál competencia debe trabajar.
> No selecciones una competencia únicamente porque aparece primero.
> 
> Devuelve EXCLUSIVAMENTE uno de estos valores:
> 
> SELECCION_1
> SELECCION_2
> INDECISO
> OTRA_COMPETENCIA
> AMBIGUA
> 
> No agregues explicaciones.
> No agregues Markdown.
> No agregues puntuación.
> No agregues ningún otro texto.

## Webhooks

Servicios a los que llama el bot. Los de escritura (✍️) piden confirmación antes de ejecutarse.

### Webhook · Envio HSM Encuesta de Retiro Camila

- **Tipo:** ✍️ **escritura** — modifica datos reales; la plataforma pide confirmación antes de ejecutarlo
- **Llamada:** `POST https://portalos.outsourcing.com.co:9609/EnvioHSMGupshup` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Credenciales:** cabecera(s) Authorization desde `.env` (no están en este archivo)
- **Envía:** `Id` = «82», `Celular` ← `retiroNumeroTelColaborador`, `Parametros[0]` ← `retiroNombreColaborador`
- **Se usa en:** 14. Reportar Retiro › 03. Ejecutar HSM Camila Envió de Encuesta (al entrar)

### Webhook · Evaluación Desempeño 1 - IA Generativa Camila

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://portalos.outsourcing.com.co:9413/ConsultaChat` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Credenciales:** cabecera(s) Authorization desde `.env` (no están en este archivo)
- **Envía:** `competenciaAFortalecer`, `competenciaAlternaPDI`, `segundaCompetencia`, `mensajeUsuario1`, `idSessionWatson`
- **Devuelve y guarda en la sesión:** `mensajeGenerativoCompetencia1` ← `$.Message`
- **Se usa en:** 02.1. Conversacional Evaluacion Desempeño › 01. Mensaje1 (ruta condición); 02.1. Conversacional Evaluacion Desempeño › PDI - Generar mensaje inicial (ruta siempre); 03.1. Conversacional Lider Misión › 01. Mensaje1 (ruta condición); 03.1. Conversacional Lider Misión › PDI - Generar mensaje inicial (ruta siempre)

### Webhook · Evaluación Desempeño 2 - IA Generativa Camila

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://portalos.outsourcing.com.co:9413/ConsultaChat` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Credenciales:** cabecera(s) Authorization desde `.env` (no están en este archivo)
- **Envía:** `mensajeGenerativoCompetencia1`, `mensajeUsuario1`, `mensajeUsuario2`, `etapaPrompt2`, `competenciaAFortalecer`, `competenciaAlternaPDI`, `idSessionWatson`
- **Devuelve y guarda en la sesión:** `mensajeGenerativoCompetencia1` ← `$.Message`
- **Se usa en:** 02.1. Conversacional Evaluacion Desempeño › 01.2 Evaluar respuesta Prompt 1 (ruta condición); 02.1. Conversacional Evaluacion Desempeño › 02. Mensaje2 (ruta condición); 03.1. Conversacional Lider Misión › 01.2 Evaluar respuesta Prompt 1 (ruta condición); 03.1. Conversacional Lider Misión › 02. Mensaje2 (ruta condición)

### Webhook · Evaluación Desempeño 3 - IA Generativa Camila

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://portalos.outsourcing.com.co:9413/ConsultaChat` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Credenciales:** cabecera(s) Authorization desde `.env` (no están en este archivo)
- **Envía:** `competenciaAFortalecer`, `nivelED`, `desafioPDI`, `estadoPDI`, `mensajeGenerativoCompetencia1`, `mensajeUsuario1`, `catalogoPDIActivo`, `idSessionWatson`
- **Devuelve y guarda en la sesión:** `mensajeGenerativoCompetencia1` ← `$.Message`
- **Se usa en:** 02.1. Conversacional Evaluacion Desempeño › 02.1 Evaluar respuesta Prompt 2 (ruta condición); 02.1. Conversacional Evaluacion Desempeño › PDI - Guardar estado (ruta siempre); 03.1. Conversacional Lider Misión › 02.1 Evaluar respuesta Prompt 2 (ruta condición); 03.1. Conversacional Lider Misión › PDI - Guardar estado (ruta siempre)

### Webhook · Evaluación Desempeño 4 - Contextualización fechas - IA Generativa Camila

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://portalos.outsourcing.com.co:9413/ConsultaChat` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Credenciales:** cabecera(s) Authorization desde `.env` (no están en este archivo)
- **Envía:** `Consulta` = «Eres Camila en el nodo de mens…», `Sistema` = «Devuelve única y exactamente e…», `IdCliente` = «42», `Caso` ← `idSessionWatson`, `Canal` = «Whatsapp», `Temperatura` = «0.0», `Modelo` = «1», `Indice` = «camilaia», `Scope` = «1»
- **Devuelve y guarda en la sesión:** `contextoFechas` ← `$.Message`
- **Se usa en:** 02.1. Conversacional Evaluacion Desempeño › 01.1 Evaluar si hay cierre de conversacion (ruta condición); 03.1. Conversacional Lider Misión › 01.1 Evaluar si hay cierre de conversacion (ruta condición)

### Webhook · Evaluación Desempeño 5 - Listar Item y Solicitar Fecha - Plan de mejoramiento - IA Generativa Camila 

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://portalos.outsourcing.com.co:9413/ConsultaChat` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Credenciales:** cabecera(s) Authorization desde `.env` (no están en este archivo)
- **Envía:** `accionesAceptadas`, `pdiFechasParcial`, `mensajeUsuarioFecha`, `fechaActual`, `fechaMinimaPDI`, `fechaMaximaPDI`, `idSessionWatson`
- **Devuelve y guarda en la sesión:** `jsonFechas` ← `$.Message`
- **Se usa en:** (sin usos)

### Webhook · Evaluación Desempeño 6 - Listar Item y Solicitar Fecha - Plan de mejoramiento - IA Generativa Camila 

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://portalos.outsourcing.com.co:9413/ConsultaChat` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Credenciales:** cabecera(s) Authorization desde `.env` (no están en este archivo)
- **Envía:** `competenciaAFortalecer`, `mensajeGenerativoCompetencia1`, `mensajeUsuario2`, `itemPlanDeMejora1`, `fechaItem`, `idSessionWatson`
- **Devuelve y guarda en la sesión:** `itemSolicitudFecha2` ← `$.Message`
- **Se usa en:** (sin usos)

### Webhook · Evaluación de Desempeño

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://fine360.outsourcing.com.co:9876/api/Evaluador/ObtenerEvaluacionDesempeno` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Autorización dinámica:** Authorization ← Bearer $session.params.Token_Authorizacion_Kairos
- **Envía:** `cedula` ← `numero_documento`
- **Devuelve y guarda en la sesión:** `apellidosEvaluacionDesempeno` ← `$.data.apellidos`, `cargoEvaluacionDesempeno` ← `$.data.cargo`, `centroCostos` ← `$.data.ccostos`, `competencia1` ← `$.data.competencias.competencia1.competencia`, `competencia2` ← `$.data.competencias.competencia2.competencia`, `competencia3` ← `$.data.competencias.competencia3.competencia`, `competencia4` ← `$.data.competencias.competencia4.competencia`, `competencia5` ← `$.data.competencias.competencia5.competencia`, `competencia6` ← `$.data.competencias.competencia6.competencia`, `competencia7` ← `$.data.competencias.competencia7.competencia`, `competencia8` ← `$.data.competencias.competencia8.competencia`, `competenciaMenor1` ← `$.data.competenciasMenores.competencia1.competencia`, `competenciaMenor2` ← `$.data.competenciasMenores.competencia2.competencia`, `nivelED` ← `$.data.nivel`, … (27 datos)
- **Se usa en:** 02. Mi Evaluación De Desempeño › 00. Mi_Evaluacion_De_Desempeño (evento evento `opcion_si`)

### Webhook · Extraer información candidato de AIRA

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://fine360.outsourcing.com.co:9876/api/CamilaIA/ConsultaCandidatoAIRA` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Envía:** `cedula` ← `numero_documento`
- **Devuelve y guarda en la sesión:** `estadoAira` ← `$.data.estado`
- **Se usa en:** 06. Mi proceso De Selección › 01. Mi_Proceso_De_Seleccion_AIRA (al entrar)

### Webhook · Extraer información candidato de W

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://default140443af8e794fceb3edde00131298.4f.environment.api.powerplatform.com/powerautomate/automations/direct/workflows/6593822f883f4cdc87c79560e93e5f22/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=-jHB9R2eCIWAjMPQ9z6dKoDv8xgfGfeKuySL1akSZXA` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Credenciales:** cabecera(s) x-api-password, x-api-user desde `.env` (no están en este archivo)
- **Envía:** `Documento` ← `numero_documento`
- **Devuelve y guarda en la sesión:** `estadoW` ← `$.Estado`, `faseW` ← `$.Fase`
- ⚠️ Usa autenticación de cuenta de servicio de Google (serviceAgentAuth/oauth): Camila no la reproduce; solo envía las cabeceras indicadas.
- **Se usa en:** 06. Mi proceso De Selección › 02. Mi_Proceso_De_Seleccion_W (al entrar); 15.1 Encuesta W › 0.Definir Encuesta W (al entrar)

### Webhook · Finalizar Conversacion

- **Tipo:** ✍️ **escritura** — modifica datos reales; la plataforma pide confirmación antes de ejecutarlo
- **Llamada:** `POST https://fine360.outsourcing.com.co:9874/api/Conversacion/EndConversationWithPersonalizedMessage` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Envía:** `idSessionWatson` ← `idSessionWatson`, `mensaje` = «»
- **Se usa en:** 06. Mi proceso De Selección › 08. Cierre flujo (ruta siempre); 07. Actualizacion de datos Datix › 05. Despedida (ruta siempre); 10. Cierre de conversación › 01. Cerrar Conversacion (ruta condición); 14. Reportar Retiro › 03. Ejecutar HSM Camila Envió de Encuesta (ruta siempre); 14.1. Encuesta de Retiro › 6. Despedida (ruta siempre); … (14 usos)

### Webhook · Guardar Acción Plan Evaluación De Desempeño

- **Tipo:** ✍️ **escritura** — modifica datos reales; la plataforma pide confirmación antes de ejecutarlo
- **Llamada:** `POST https://portalos.outsourcing.com.co:9454/api/plan-fechas/registrar-fecha-compromiso` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Credenciales:** cabecera(s) Authorization desde `.env` (no están en este archivo)
- **Envía:** `idSesion` ← `idSessionWatson`, `documento` ← `Numero_Documento`, `nombre` ← `nombreCompletoEvaluacionDesempeno`, `cargo` ← `cargoEvaluacionDesempeno`, `cuentaArea` ← `centroCostos`, `competencia` ← `competenciaAFortalecer`, `Accion` ← `accionFechaActual`, `estadoPDI` ← `estadoPDI`, `contadorAcciones` ← `contadorAccionFechaActual`, `fechaCompromiso` ← `fechaCompromisoActual`, `materialComplementario` ← `materialComplementario`
- **Se usa en:** 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (ruta condición); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (ruta condición)

### Webhook · Guardar Comentario Te Escuchamos

- **Tipo:** ✍️ **escritura** — modifica datos reales; la plataforma pide confirmación antes de ejecutarlo
- **Llamada:** `POST https://fine360.outsourcing.com.co:9876/api/CamilaIA/GuardarTeEscuchamos` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Envía:** `idsessionwatson` ← `idSessionWatson`, `nombreCompleto` ← `nombreProvisional`, `documento` ← `Numero_Documento`, `comentarioColaborador` ← `comentario`, `respuestaBot` ← `mensajeGenerativoGlobal`
- **Se usa en:** 08.Te escuchamos › 04.1 Responder comentario (ruta siempre)

### Webhook · Guardar Datos Colaborador Encuesta De Retiro

- **Tipo:** ✍️ **escritura** — modifica datos reales; la plataforma pide confirmación antes de ejecutarlo
- **Llamada:** `POST https://fine360.outsourcing.com.co:9876/api/CamilaIA/InsertarColaborador` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Envía:** `idSesion` ← `idSessionWatson`, `nombreColaborador` ← `retiroNombreColaborador`, `documento` ← `retiroNumeroTelColaborador`, `contacto` ← `retiroNumeroDocumentoColaborador`
- **Se usa en:** 14. Reportar Retiro › 02. Guardar Datos Colaborador Retirado (al entrar)

### Webhook · Guardar Encuesta De Retiro

- **Tipo:** ✍️ **escritura** — modifica datos reales; la plataforma pide confirmación antes de ejecutarlo
- **Llamada:** `POST https://fine360.outsourcing.com.co:9876/api/CamilaIA/InsertarEncuestaRetiro` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Envía:** `idColaborador` = «», `idSesion` ← `idSessionWatson`, `pregunta` ← `preguntaEncuestaRetiro`, `respuesta` ← `respuesta`
- **Se usa en:** 14.1. Encuesta de Retiro › Guardar Pregunta Y Respuesta (al entrar)

### Webhook · Guardar Formulario Fine360 

- **Tipo:** ✍️ **escritura** — modifica datos reales; la plataforma pide confirmación antes de ejecutarlo
- **Llamada:** `POST https://fine360.outsourcing.com.co:9874/api/Formulario/GuardarFormulario` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Envía:** `UsuarioCreacionId` = «11179», `skillName` = «SKillChatBotCamilaIA», `SessionWatson` ← `idSessionWatson`, `CamposAdicionales[0].NameCampo` = «NombreCompleto», `CamposAdicionales[0].Value` ← `nombreProvisional`, `CamposAdicionales[0].IsRequired` = «True», `CamposAdicionales[1].NameCampo` = «NumeroDocumento», `CamposAdicionales[1].Value` ← `numero_documento`, `CamposAdicionales[1].IsRequired` = «True»
- **Se usa en:** Default Start Flow › 02.Captura_Numero_Documento (ruta condición)

### Webhook · Guardar_Referidos

- **Tipo:** ✍️ **escritura** — modifica datos reales; la plataforma pide confirmación antes de ejecutarlo
- **Llamada:** `POST https://frontos.outsourcing.com.co:8600/api/referidos/guardar` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Envía:** `ResponsableId` ← `idUsuarioReferidos`, `Referidos[0].DocumentoReferido` ← `documentoReferido`, `Referidos[0].NombresReferido` ← `nombreReferido`, `Referidos[0].EmailReferido` ← `correoReferido`, `Referidos[0].Telefono` ← `telefonoReferido`, `Referidos[0].AreaExperiencia` ← `areaExperienciaReferido`, `Referidos[0].Otro` ← `otraAreaExperienciaReferido`
- **Se usa en:** 04. Referidos › 03. Guardar Referido (al entrar)

### Webhook · IA Generativa Camila - General

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://portalos.outsourcing.com.co:9413/ConsultaWeb` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Credenciales:** cabecera(s) Authorization desde `.env` (no están en este archivo)
- **Envía:** `Consulta` ← `consultaGenerativa`, `Sistema` ← `sistemaGenerativo`, `IdCliente` = «42», `Caso` ← `idSessionWatson`, `Canal` = «Whastapp», `Temperatura` = «0.3», `Modelo` = «1», `Indice` = «camilaia»
- **Devuelve y guarda en la sesión:** `mensajeGenerativoGlobal` ← `$.Message`
- **Se usa en:** 03. Líder Misión › 00. Líder_Misión (evento evento `sys.no-match-default`); 07. Actualizacion de datos Datix › 00. Actualizar Datos (evento evento `sys.no-match-default`); 07. Actualizacion de datos Datix › 00. Actualizar Datos (ruta intent «route_RespuestaAfirmativa»); 08.Te escuchamos › 04. Capturar comentario (ruta condición); 09.Convenios › 01. Decisión conocer conveniOS (evento evento `Si`); … (10 usos)

### Webhook · Lider Mision

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://fine360.outsourcing.com.co:9876/api/Evaluador/ObtenerLiderMision` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Autorización dinámica:** Authorization ← Bearer $session.params.Token_Authorizacion_Kairos
- **Envía:** `cedula` ← `numero_documento`
- **Devuelve y guarda en la sesión:** `competencia1LM` ← `$.data.competencias.competencia1.competencia`, `competencia2LM` ← `$.data.competencias.competencia2.competencia`, `competencia3LM` ← `$.data.competencias.competencia3.competencia`, `competenciaMenor1LiderMision` ← `$.data.competenciasMenores.competencia1.competencia`, `nivelLM` ← `$.data.nivel`, `notaCompetencia1LM` ← `$.data.competencias.competencia1.valor`, `notaCompetencia2LM` ← `$.data.competencias.competencia2.valor`, `notaCompetencia3LM` ← `$.data.competencias.competencia3.valor`, `notaTotalLM` ← `$.data.notaFinal`
- **Se usa en:** 03. Líder Misión › 00. Líder_Misión (evento evento `si`)

### Webhook · Lider Mision - IA Generativa Camila

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://portalos.outsourcing.com.co:9413/ConsultaWeb` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Credenciales:** cabecera(s) Authorization desde `.env` (no están en este archivo)
- **Envía:** `competenciaMenor1LiderMision`, `idSessionWatson`
- **Devuelve y guarda en la sesión:** `mensajeGenerativoCompetencia1Lider` ← `$.Message`
- **Se usa en:** 03. Líder Misión › 01.3 . Sugerencia Atributos (evento evento `competenciaMenor1`)

### Webhook · Login_API_Kairos

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://fine360.outsourcing.com.co:9876/api/Login` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Envía:** `username` = «UserKairosApl», `password` = «sY4vRQlyswlNTw»
- **Devuelve y guarda en la sesión:** `Token_Authorizacion_Kairos` ← `$.datos.token`
- **Se usa en:** 17.3 Certificados › 1. datos (ruta condición); Default Start Flow › 05.Menu_Principal (evento evento `calidad`); Default Start Flow › 05.Menu_Principal (evento evento `evaluacion_desempeno`); Default Start Flow › 05.Menu_Principal (evento evento `lider_mision`); flujo (global) (grupo Atajos_Flow (copy)); … (7 usos)

### Webhook · Login_API_Reconoser

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://test.outsourcing.com.co:8025/api/v1/Authentication/Authentication` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Envía:** `username` = «CAMILIA_WS», `password` = «C@Mil4I4!2024$#XpLz9gQ»
- **Devuelve y guarda en la sesión:** `Token_Authorizacion_Reconoser` ← `$.datos.token`
- **Se usa en:** flujo (global) (grupo Atajos_Flow (copy)); flujo (global) (grupo Atajos_Flow_Global); flujo 05. Reconoser (ruta condición); flujo Default Start Flow (grupo Atajos_Flow)

### Webhook · Mis indicadores ReconoSER

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://test.outsourcing.com.co:8025/api/v1/Data/GetIndicadoresReconocer` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Autorización dinámica:** Authorization ← Bearer $session.params.Token_Authorizacion_Reconoser
- **Envía:** `documento` ← `numero_documento`, `anio` ← `anio_elegido`, `mes` ← `mes_elegido_consulta`
- **Devuelve y guarda en la sesión:** `indicador1` ← `$.datos[0].indicador1`, `indicador2` ← `$.datos[0].indicador2`, `indicador3` ← `$.datos[0].indicador3`, `indicador4` ← `$.datos[0].indicador4`, `resultado1` ← `$.datos[0].resultado1`, `resultado2` ← `$.datos[0].resultado2`, `resultado3` ← `$.datos[0].resultado3`, `resultado4` ← `$.datos[0].resultado4`
- **Se usa en:** 05. Reconoser › 01.2. Confirmar información de consulta (evento evento `si`)

### Webhook · Mis_Atributos

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://fine360.outsourcing.com.co:9876/api/Evaluador/ObtenerAtributosEmbajador` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Autorización dinámica:** Authorization ← Bearer $session.params.Token_Authorizacion_Kairos
- **Envía:** `cedula` ← `numero_documento`
- **Devuelve y guarda en la sesión:** `mesActualMes` ← `$.data[0].mes`, `mesActualPorcentanjeApasionado` ← `$.data[0].apasionado.porcentaje`, `mesActualPorcentanjeEmpatia` ← `$.data[0].empatia.porcentaje`, `mesActualPorcentanjeProactivo` ← `$.data[0].proactivo.porcentaje`, `mesAnteriorMes` ← `$.data[1].mes`, `mesAnteriorPorcentanjeApasionado` ← `$.data[1].apasionado.porcentaje`, `mesAnteriorPorcentanjeEmpatia` ← `$.data[1].empatia.porcentaje`, `mesAnteriorPorcentanjeProactivo` ← `$.data[1].proactivo.porcentaje`
- **Se usa en:** 01. Indicadores De Calidad › 006.ParametrosAtributos (ruta condición)

### Webhook · Mis_Casos_Abiertos

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://fine360.outsourcing.com.co:9876/api/Evaluador/ObtenerCasosAbiertos` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Autorización dinámica:** Authorization ← Bearer $session.params.Token_Authorizacion_Kairos
- **Envía:** `cedula` ← `numero_documento`
- **Devuelve y guarda en la sesión:** `cantidadCasosAbiertos` ← `$.data[0].cantidadAbiertos`, `fechaCaso` ← `$.data[0].casos[0].fecha`, `idCaso` ← `$.data[0].casos[0].id`, `mesCasosAbiertos` ← `$.data[0].mes`, `segmentoCaso` ← `$.data[0].casos[0].segmento`
- **Se usa en:** 01. Indicadores De Calidad › 03. CerrarMonitoreos (ruta condición)

### Webhook · Mis_Indicadores

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://fine360.outsourcing.com.co:9876/api/Evaluador/ObtenerEstadisticasEmbajador` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Autorización dinámica:** Authorization ← Bearer $session.params.Token_Authorizacion_Kairos
- **Envía:** `cedula` ← `numero_documento`, `campana` ← `Campana`
- **Devuelve y guarda en la sesión:** `mesActualCantidadCriticos` ← `$.data[0].errorCritico.cantidadErrores`, `mesActualCantidadMonitoreos` ← `$.data[0].cantidadMonitoreos`, `mesActualCantidadNoCriticos` ← `$.data[0].errorNoCritico.cantidadErrores`, `mesActualMes` ← `$.data[0].mes`, `mesActualNotaCalidad` ← `$.data[0].notaCalidadEmitida`, `mesActualNotaCriticos` ← `$.data[0].errorCritico.notaPrecisionPromedio`, `mesActualNotaNoCriticos` ← `$.data[0].errorNoCritico.notaPrecisionPromedio`, `mesAnteriorCantidadCriticos` ← `$.data[1].errorCritico.cantidadErrores`, `mesAnteriorCantidadMonitoreos` ← `$.data[1].cantidadMonitoreos`, `mesAnteriorCantidadNoCriticos` ← `$.data[1].errorNoCritico.cantidadErrores`, `mesAnteriorMes` ← `$.data[1].mes`, `mesAnteriorNotaCalidad` ← `$.data[1].notaCalidadEmitida`, `mesAnteriorNotaCriticos` ← `$.data[1].errorCritico.notaPrecisionPromedio`, `mesAnteriorNotaNoCriticos` ← `$.data[1].errorNoCritico.notaPrecisionPromedio`, … (21 datos)
- **Se usa en:** 01. Indicadores De Calidad › 005.ParametrosIndicadoresRestantes (ruta condición)

### Webhook · Obtener Url Matriz de Calidad

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://fine360.outsourcing.com.co:9876/api/Evaluador/GenerarUrlMatrizCalidad` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Envía:** `usuarioRed` ← `UsuarioRed`
- **Devuelve y guarda en la sesión:** `url_MatrizCalidad_Final_Encriptada` ← `$.url`
- **Se usa en:** 01. Indicadores De Calidad › 001. Menu_Indicadores (evento evento `mi_matriz_calidad`); 01. Indicadores De Calidad › 001. Menu_Indicadores (ruta intent «routeCalidad_MatrizDeCalidad»)

### Webhook · Obtener_Colaborador_Referidos

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://frontos.outsourcing.com.co:8600/api/referidos/buscar-usuario` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Envía:** `Cedula` ← `numero_documento`
- **Devuelve y guarda en la sesión:** `idUsuarioReferidos` ← `$.data.Id`
- **Se usa en:** flujo 04. Referidos (ruta condición)

### Webhook · Obtener_Usuario_Por_Cedula

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://fine360.outsourcing.com.co:9876/api/v1/Usuarios/ObtenerUsuarioPorCedula` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Autorización dinámica:** Authorization ← Bearer $session.params.Token_Authorizacion_Kairos
- **Envía:** `cedula` ← `numero_documento`
- **Devuelve y guarda en la sesión:** `Apellidos` ← `$.usuario.apellidos`, `Campana` ← `$.usuario.campanaUsuarioUsuarios[0].campana.nombre`, `Nombres` ← `$.usuario.nombres`, `UsuarioRed` ← `$.usuario.nombreUsuario`, `activo` ← `$.activo`, `existe` ← `$.existe`, `id` ← `$.usuario.id`, `idCampana` ← `$.usuario.campanaUsuarioUsuarios[0].campana.id`
- ⚠️ Usa autenticación de cuenta de servicio de Google (serviceAgentAuth/oauth): Camila no la reproduce; solo envía las cabeceras indicadas.
- **Se usa en:** 01. Indicadores De Calidad › 0.ValidacionTokenKairos (ruta condición); 13. Usuario inexistente/ inactivo › 00.1 Solicitar numero de documento (ruta condición); flujo 02. Mi Evaluación De Desempeño (ruta condición)

### Webhook · UrlPdfEvaluacionDesempeño

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://portalos.outsourcing.com.co:9454/api/pdf-urls/consultar` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Credenciales:** cabecera(s) Authorization desde `.env` (no están en este archivo)
- **Envía:** `documento` ← `numero_documento`
- **Devuelve y guarda en la sesión:** `urlPdfPlanMejoramiento` ← `$.datos[0].urlPdf`
- **Se usa en:** 02.1. Conversacional Evaluacion Desempeño › 4. Consulta PDF plan de mejoramiento (al entrar); 03.1. Conversacional Lider Misión › 4. Consulta PDF plan de mejoramiento (al entrar)

### Webhook · certificadosAdminPersonal-Basico

- **Tipo:** ✍️ **escritura** — modifica datos reales; la plataforma pide confirmación antes de ejecutarlo
- **Llamada:** `POST https://fine360.outsourcing.com.co:9876/api/Evaluador/administracion-personal-certificados` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Autorización dinámica:** Authorization ← Bearer $session.params.Token_Authorizacion_Kairos
- **Envía:** `documento` ← `documento`, `nombre` ← `nombreProvisional`, `fechaIngreso` = «», `fechaTerminacion` = «», `salarioBasico` = «», `salarioVariable` = «», `cargo` = «», `centroCostos` = «», `estado` = «», `terminoContrato` = «», `tipoCertificado` ← `tipoCertificado`, `email` ← `correoElectronico`, `añoCertificadoIngresos` = «», `mesInicioPlanilla` ← `mesCertificado`, `añoInicioPlanilla` ← `anoCertificado`, `mesFinalPlanilla` ← `mesFinalCertificado`, `añoFinalPlanilla` ← `anoFinalCertificado`, `fechaNovedad` = «»
- **Se usa en:** 17.3 Certificados › 3. correo certificado basico (ruta condición); 17.3 Certificados › 5. envio datos certificado (ruta condición)

### Webhook · certificadosAdminPersonal-Ingresos

- **Tipo:** ✍️ **escritura** — modifica datos reales; la plataforma pide confirmación antes de ejecutarlo
- **Llamada:** `POST https://fine360.outsourcing.com.co:9876/api/Evaluador/administracion-personal-certificados` · formato flexible (cuerpo propio) · tiempo máximo 30 s
- **Autorización dinámica:** Authorization ← Bearer $session.params.Token_Authorizacion_Kairos
- **Envía:** `documento` ← `documento`, `nombre` ← `nombreProvisional`, `fechaIngreso` = «», `fechaTerminacion` = «», `salarioBasico` = «», `salarioVariable` = «», `cargo` = «», `centroCostos` = «», `estado` = «», `terminoContrato` = «», `tipoCertificado` ← `tipoCertificado`, `email` ← `correoIngresos`, `añoCertificadoIngresos` ← `aniocertificado`, `mesInicioPlanilla` = «», `añoInicioPlanilla` = «», `mesFinalPlanilla` = «», `añoFinalPlanilla` = «», `fechaNovedad` = «»
- **Se usa en:** 17.3 Certificados › 4. info ingresos y retenciones (ruta condición)

### Webhook · recuperarSessionWatsonId

- **Tipo:** ⚡ lectura — consulta datos
- **Llamada:** `POST https://fine360.outsourcing.com.co:9876/api/Login/session-watson` · formato estándar de Dialogflow (recibe etiqueta, página y todas las variables de sesión) · tiempo máximo 5 s
- **Devuelve:** variables de sesión en `sessionInfo.parameters`
- **Se usa en:** Default Start Flow › 01. Bienvenida_CamilaIA (ruta condición); flujo Default Start Flow (evento evento `Encuesta de retiro`); flujo Default Start Flow (ruta intent «Encuesta De Retiro»); flujo Pruebas pdf (ruta condición)

