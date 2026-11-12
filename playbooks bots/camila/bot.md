---
id: camila
name: Camila IA
description: Asistente de recursos humanos de Outsourcing (export de Dialogflow CX: CamilaOS).
icon: 🧩
executor: lector-uml
agent: camila-ia
startFlow: Default Start Flow
knowledge: bases de conocimiento/CamilaIA.md
source: Bots/ (export de Dialogflow CX)
---
# Bot · Camila IA — playbook maestro

Asistente de recursos humanos de Outsourcing (export de Dialogflow CX: CamilaOS).

Este bot lo **ejecuta el agente `lector-uml`** (sin modelo de lenguaje) en el entorno de pruebas de *Chatbots*, y lo **conversa el agente `camila-ia`** apoyándose en la base de conocimiento `bases de conocimiento/CamilaIA.md`.
Generado desde el export del bot; **no edites estos archivos** (se regeneran). Tus notas van en [`notas.md`](notas.md): nunca se pisan.

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

## Índice por flujo (resumen)

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

## Playbooks de este bot

- [01. Indicadores De Calidad](flujos/01-01-indicadores-de-calidad.md) — 20 página(s)
- [02. Mi Evaluación De Desempeño](flujos/02-02-mi-evaluacion-de-desempeno.md) — 10 página(s)
- [02.1. Conversacional Evaluacion Desempeño](flujos/03-02-1-conversacional-evaluacion-desempeno.md) — 23 página(s)
- [03. Líder Misión](flujos/04-03-lider-mision.md) — 8 página(s)
- [03.1. Conversacional Lider Misión](flujos/05-03-1-conversacional-lider-mision.md) — 23 página(s)
- [04. Referidos](flujos/06-04-referidos.md) — 9 página(s)
- [05. Reconoser](flujos/07-05-reconoser.md) — 7 página(s)
- [06. Mi proceso De Selección](flujos/08-06-mi-proceso-de-seleccion.md) — 9 página(s)
- [07. Actualizacion de datos Datix](flujos/09-07-actualizacion-de-datos-datix.md) — 5 página(s)
- [08.Te escuchamos](flujos/10-08-te-escuchamos.md) — 7 página(s)
- [09.Convenios](flujos/11-09-convenios.md) — 5 página(s)
- [10. Cierre de conversación](flujos/12-10-cierre-de-conversacion.md) — 1 página(s)
- [11. No Detecta](flujos/13-11-no-detecta.md) — 2 página(s)
- [12. ¿Algo Mas?](flujos/14-12-algo-mas.md) — 1 página(s)
- [13. Usuario inexistente/ inactivo](flujos/15-13-usuario-inexistente-inactivo.md) — 4 página(s)
- [14. Reportar Retiro](flujos/16-14-reportar-retiro.md) — 3 página(s)
- [14.1. Encuesta de Retiro](flujos/17-14-1-encuesta-de-retiro.md) — 15 página(s)
- [15. Viaje del colaborador](flujos/18-15-viaje-del-colaborador.md) — 5 página(s)
- [15.1 Encuesta W](flujos/19-15-1-encuesta-w.md) — 6 página(s)
- [15.10 Hasta pronto](flujos/20-15-10-hasta-pronto.md) — 1 página(s)
- [15.2 Crece con nosotros](flujos/21-15-2-crece-con-nosotros.md) — 1 página(s)
- [15.3 Cuidamos de ti](flujos/22-15-3-cuidamos-de-ti.md) — 1 página(s)
- [15.4 Induccion](flujos/23-15-4-induccion.md) — 1 página(s)
- [15.5 Heramientas Kactus 1](flujos/24-15-5-heramientas-kactus-1.md) — 1 página(s)
- [15.6 Herramientas Kactus 3](flujos/25-15-6-herramientas-kactus-3.md) — 1 página(s)
- [15.7 Experiencia rol](flujos/26-15-7-experiencia-rol.md) — 1 página(s)
- [15.8 Encuesta instalaciones](flujos/27-15-8-encuesta-instalaciones.md) — 1 página(s)
- [15.9 Lider mision](flujos/28-15-9-lider-mision.md) — 1 página(s)
- [16. Administración de personal](flujos/29-16-administracion-de-personal.md) — 1 página(s)
- [16.1 Pagos](flujos/30-16-1-pagos.md) — 3 página(s)
- [16.2 Creditos libranza](flujos/31-16-2-creditos-libranza.md) — 3 página(s)
- [16.3 EPS](flujos/32-16-3-eps.md) — 4 página(s)
- [16.4 Caja compensacion](flujos/33-16-4-caja-compensacion.md) — 1 página(s)
- [16.5 Cesantias](flujos/34-16-5-cesantias.md) — 6 página(s)
- [16.6 carne](flujos/35-16-6-carne.md) — 1 página(s)
- [16.7 certificados](flujos/36-16-7-certificados.md) — 3 página(s)
- [16.8 Seguridad Social](flujos/37-16-8-seguridad-social.md) — 3 página(s)
- [17. Ex colaborador](flujos/38-17-ex-colaborador.md) — 1 página(s)
- [17.1 pagos](flujos/39-17-1-pagos.md) — 3 página(s)
- [17.2 Cesantias](flujos/40-17-2-cesantias.md) — 2 página(s)
- [17.3 Certificados](flujos/41-17-3-certificados.md) — 5 página(s)
- [Default Start Flow](flujos/42-default-start-flow.md) — 6 página(s)
- [Pruebas pdf](flujos/43-pruebas-pdf.md) — 2 página(s)

Catálogos: [grupos.md](catalogos/grupos.md) · [intents.md](catalogos/intents.md) · [entidades.md](catalogos/entidades.md) · [generadores.md](catalogos/generadores.md) · [webhooks.md](catalogos/webhooks.md)
