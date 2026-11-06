# Base de conocimiento · Seguridad de la información — Nexora S.A.

Documento interno (ficticio) con el procedimiento oficial ante violaciones de las normas de seguridad.
Cada situación tiene un **código**. Los agentes deben consultar esta base ANTES de responder y no pueden
inventar pasos, plazos, contactos ni tickets que no aparezcan aquí.

Contactos generales: Mesa de Seguridad — ext. 4400 (24/7) · seguridad@nexora.example.

## SEG-001 · Préstamo o uso compartido de credenciales

**Severidad:** Alta
**Palabras clave:** presté mi usuario, presté mi contraseña, prestar mi usuario, prestar mi contraseña, compartí mi contraseña, compartí mi usuario, compartir credenciales, credenciales compartidas, usuario y contraseña, clave a un compañero, credenciales a un compañero, usó mi usuario, usar el usuario de otro, acceso de otra persona

### Cuándo aplica
Cuando un empleado entrega, presta o comparte su usuario, contraseña, token o acceso (VPN, correo, sistemas) con otra persona, aunque sea un compañero y con buena intención; o cuando alguien usa credenciales ajenas.

### Plazo de reporte
Máximo **2 horas** desde que ocurrió o se detectó.

### Acciones inmediatas (obligatorias)
1. Cambiar de inmediato la contraseña y cerrar todas las sesiones activas.
2. Revocar el acceso compartido (token de VPN o sesión abierta).
3. Reportar a la Mesa de Seguridad (ext. 4400) abriendo un ticket tipo **SEG-CRED**.
4. El jefe inmediato solicita el acceso formal para la otra persona con el formulario **FOR-ACC-02**.
5. Seguridad revisa los registros de acceso de las **últimas 48 horas**.

### Responsables y escalamiento
Mesa de Seguridad → Jefe del área → Recursos Humanos (solo si es reincidente).

### Qué NO hacer
- No volver a compartir las credenciales "solo para terminar la tarea".
- No esperar a que la otra persona termine su trabajo para reportar.
- No borrar historiales ni mensajes relacionados.

### Medidas posteriores
Primera vez: amonestación escrita y curso "Uso responsable de credenciales" dentro de **15 días**. Reincidencia: proceso disciplinario.

### Respuesta modelo al usuario
Reconocer que compartir credenciales viola la norma aunque la intención sea buena, indicar los 5 pasos con sus plazos, el ticket SEG-CRED y el formulario FOR-ACC-02, y aclarar la medida posterior.

### Solución A · Cierre inmediato y reporte estándar (recomendada)
**Cuándo conviene:** es el camino por defecto para cualquier préstamo de credenciales.
1. Cambiar la contraseña y cerrar todas las sesiones activas.
2. Revocar el token de VPN o la sesión compartida.
3. Reportar a la Mesa de Seguridad (ext. 4400) con un ticket **SEG-CRED** en máximo 2 horas.
4. El jefe inmediato pide el acceso formal de la otra persona con el formulario **FOR-ACC-02**.
5. Seguridad revisa los accesos de las últimas 48 horas.

### Solución B · Rotación completa con acompañamiento de Seguridad
**Cuándo conviene:** si la persona no está segura de qué sistemas llegó a usar el compañero.
1. Llamar a la Mesa de Seguridad (ext. 4400) y abrir el ticket **SEG-CRED**.
2. Seguridad acompaña por llamada la rotación de contraseña, token de VPN y claves de aplicaciones, en máximo 1 hora.
3. Seguridad invalida todas las sesiones abiertas del usuario.
4. Se revisan los accesos de las últimas 48 horas.

### Solución C · Acceso temporal formal para quien necesitaba el acceso
**Cuándo conviene:** si el compañero tenía una necesidad de trabajo real y urgente.
1. Hacer primero el cierre y el ticket **SEG-CRED** (pasos de la Solución A).
2. El jefe inmediato solicita un acceso temporal de 5 días hábiles con el formulario **FOR-ACC-05**.
3. El compañero trabaja solo con su propio usuario, nunca con el prestado.

### Solución D · Revisión de registros ampliada
**Cuándo conviene:** si hay sospecha de que el acceso prestado se usó fuera de lo acordado.
1. Reportar con ticket **SEG-CRED** y pedir expresamente la revisión ampliada.
2. Seguridad amplía la revisión de registros de 48 horas a 7 días.
3. Los hallazgos se documentan en el anexo **AUD-07** y se informan al jefe del área.

### Solución E · Capacitación inmediata y compromiso escrito
**Cuándo conviene:** si es la primera vez y la persona quiere cerrar el tema cuanto antes.
1. Cumplir los pasos de la Solución A.
2. Completar el curso "Uso responsable de credenciales" en 5 días, antes del límite de 15 días.
3. Firmar el compromiso **FOR-CMP-01**, que se anexa al ticket **SEG-CRED**.

## SEG-002 · Conexión de dispositivos USB o externos no autorizados

**Severidad:** Alta
**Palabras clave:** pendrive, usb, memoria usb, dispositivo externo, disco externo, conecté un usb, conecté un pendrive, encontré un pendrive, archivos raros, carpeta extraña, dispositivo desconocido

### Cuándo aplica
Cuando se conecta a un equipo corporativo una memoria USB, disco externo o dispositivo no autorizado, especialmente si fue encontrado o de origen desconocido, o si aparecen archivos o carpetas extrañas.

### Plazo de reporte
Inmediato; máximo **1 hora**.

### Acciones inmediatas (obligatorias)
1. Desconectar el equipo de la red (cable o Wi-Fi) **sin apagarlo**.
2. Retirar el dispositivo sin abrir ningún archivo y guardarlo en una bolsa sellada.
3. Llamar a la Mesa de Seguridad (ext. 4400) y abrir un ticket tipo **SEG-MALW**.
4. El equipo será aislado y analizado por el SOC (antivirus y análisis forense) en **24 horas**.
5. Entregar el dispositivo al equipo de Seguridad.

### Responsables y escalamiento
Mesa de Seguridad → SOC (Centro de Operaciones de Seguridad) → Jefe de TI.

### Qué NO hacer
- No seguir abriendo archivos del dispositivo.
- No formatear el dispositivo ni conectarlo a otro equipo.
- No apagar el computador (se pierde evidencia).

### Medidas posteriores
Se asigna un equipo temporal mientras se analiza el original. Capacitación en uso de medios extraíbles.

### Respuesta modelo al usuario
Explicar la urgencia, desconectar de la red sin apagar, no tocar más el dispositivo, ticket SEG-MALW y el análisis del SOC en 24 horas.

### Solución A · Aislamiento y análisis del SOC (recomendada)
**Cuándo conviene:** es el camino por defecto ante cualquier dispositivo externo desconocido.
1. Desconectar el equipo de la red sin apagarlo.
2. Retirar el dispositivo sin abrir archivos y guardarlo en una bolsa sellada.
3. Llamar a la Mesa de Seguridad (ext. 4400) y abrir un ticket **SEG-MALW** en máximo 1 hora.
4. El SOC analiza el equipo en 24 horas y se entrega el dispositivo a Seguridad.

### Solución B · Entrega presencial al SOC
**Cuándo conviene:** si la persona está en la oficina y puede moverse de inmediato.
1. Desconectar el equipo de la red sin apagarlo y abrir el ticket **SEG-MALW**.
2. Llevar el dispositivo en bolsa sellada a la oficina del SOC en máximo 2 horas.
3. El SOC recoge el equipo para el análisis forense.

### Solución C · Análisis remoto asistido
**Cuándo conviene:** si la persona trabaja fuera de la oficina.
1. Desconectar el equipo de la red sin apagarlo y abrir el ticket **SEG-MALW**.
2. El SOC toma el control remoto del equipo con la herramienta EDR en máximo 1 hora.
3. El dispositivo se envía luego por mensajería sellada a Seguridad.

### Solución D · Equipo temporal inmediato
**Cuándo conviene:** si la persona no puede dejar de trabajar mientras se analiza su equipo.
1. Cumplir los pasos de aislamiento de la Solución A.
2. TI entrega un equipo temporal en máximo 4 horas con el formulario **FOR-TI-03**.
3. El equipo original queda en análisis hasta que el SOC lo libere.

### Solución E · Escaneo EDR previo si no se abrió ningún archivo
**Cuándo conviene:** si la persona asegura que no abrió ningún archivo del dispositivo.
1. Desconectar de la red y abrir el ticket **SEG-MALW** con prioridad media.
2. El SOC ejecuta un escaneo EDR completo en máximo 6 horas.
3. Si el escaneo encuentra algo, se pasa al análisis completo de la Solución A.

## SEG-003 · Phishing: clic en enlace sospechoso o ingreso de credenciales en un sitio falso

**Severidad:** Crítica
**Palabras clave:** phishing, correo sospechoso, correo falso, clic en el enlace, di clic en el enlace, hice clic en un enlace, enlace sospechoso, página falsa, sitio falso, falsa, ingresé mi usuario y clave, ingresé mis credenciales, banco corporativo, suplantación

### Cuándo aplica
Cuando un empleado hace clic en un enlace de un correo o mensaje sospechoso, abre un adjunto dudoso o ingresa sus credenciales en una página que luego parece falsa.

### Plazo de reporte
Inmediato; máximo **30 minutos**.

### Acciones inmediatas (obligatorias)
1. Cambiar la contraseña **desde otro dispositivo** de confianza.
2. Activar o validar la verificación en dos pasos (MFA).
3. Reenviar el correo **como adjunto** a phishing@nexora.example y no borrarlo.
4. Abrir un ticket tipo **SEG-PHISH** en la Mesa de Seguridad.
5. Seguridad bloquea el dominio y revisa los accesos de las **últimas 24 horas**.
6. Si usó la misma clave en otros sistemas, cambiarla en todos.

### Responsables y escalamiento
Mesa de Seguridad → SOC → Jefe del área (informativo).

### Qué NO hacer
- No responder el correo ni reenviarlo a compañeros.
- No esperar "a ver si pasa algo".
- No borrar el correo original (es evidencia).

### Medidas posteriores
Política de **reporte sin culpa**: si reporta dentro de los 30 minutos no hay sanción; solo recibe una microcapacitación en phishing.

### Respuesta modelo al usuario
Tranquilizar (reportar a tiempo no se sanciona), dar los 6 pasos en orden, mencionar el ticket SEG-PHISH y el correo de reporte.

### Solución A · Contención estándar en 6 pasos (recomendada)
**Cuándo conviene:** es el camino por defecto ante cualquier clic o ingreso de credenciales en un sitio falso.
1. Cambiar la contraseña desde otro dispositivo de confianza.
2. Activar o validar la verificación en dos pasos (MFA).
3. Reenviar el correo como adjunto a phishing@nexora.example y no borrarlo.
4. Abrir un ticket **SEG-PHISH** en la Mesa de Seguridad, en máximo 30 minutos.
5. Seguridad bloquea el dominio y revisa los accesos de las últimas 24 horas.
6. Cambiar la clave en otros sistemas donde se haya usado.

### Solución B · Bloqueo temporal de la cuenta
**Cuándo conviene:** si la persona no puede cambiar su contraseña desde otro dispositivo.
1. Llamar a la Mesa de Seguridad (ext. 4400) y abrir el ticket **SEG-PHISH**.
2. Seguridad bloquea la cuenta durante 2 horas.
3. Seguridad restablece la contraseña con una verificación guiada y reactiva la cuenta.

### Solución C · Reporte desde el botón del correo
**Cuándo conviene:** si el cliente de correo tiene el botón "Reportar phishing".
1. Usar el botón "Reportar phishing" sobre el correo, sin reenviarlo manualmente.
2. Abrir el ticket **SEG-PHISH** en máximo 30 minutos.
3. Cambiar la contraseña y activar el MFA como en la Solución A.

### Solución D · Revisión forense del equipo
**Cuándo conviene:** si además de hacer clic se abrió un adjunto o se descargó un archivo.
1. Desconectar el equipo de la red y abrir el ticket **SEG-PHISH**.
2. El SOC revisa el equipo con antivirus y EDR en máximo 12 horas.
3. Se cambian las contraseñas desde otro dispositivo de confianza.

### Solución E · Aviso interno a otros posibles afectados
**Cuándo conviene:** si el correo se respondió o se reenvió a otras personas.
1. Cumplir los pasos de contención de la Solución A.
2. Informar a la Mesa de Seguridad a quién se reenvió el correo.
3. Seguridad emite un comunicado interno con el código **SEG-AVISO** a esas personas.

## SEG-004 · Envío de información confidencial a correo o nube personal (fuga de datos)

**Severidad:** Crítica
**Palabras clave:** gmail, correo personal, cuenta personal, nube personal, reenvié a mi gmail, reenvié a mi correo personal, base de datos de clientes, información confidencial, datos de clientes, trabajar desde casa, fuga de datos, dropbox personal

### Cuándo aplica
Cuando se envía, reenvía o sube información confidencial de la compañía (bases de datos, datos de clientes, documentos internos) a un correo, nube o dispositivo personal, incluso para trabajar desde casa.

### Plazo de reporte
Máximo **1 hora**.

### Acciones inmediatas (obligatorias)
1. No reenviar ni compartir más la información.
2. **No borrar** el mensaje del correo personal hasta que Seguridad lo autorice (es evidencia).
3. Reportar a la Mesa de Seguridad y a Protección de Datos (dpo@nexora.example) abriendo un ticket tipo **SEG-FUGA**.
4. Listar qué información y cuántos registros se enviaron.
5. El área Legal evalúa la notificación a la autoridad de protección de datos dentro de **72 horas**.
6. Para trabajar desde casa, usar solo la VPN corporativa y el equipo asignado.

### Responsables y escalamiento
Mesa de Seguridad → Protección de Datos (DPO) → Legal → Dirección.

### Qué NO hacer
- No borrar los envíos para "ocultarlo".
- No mover la información a otra nube.
- No justificarlo como urgencia: la norma prohíbe sacar datos de clientes de los sistemas de la compañía.

### Medidas posteriores
Proceso disciplinario según la gravedad y revisión de permisos de acceso a datos.

### Respuesta modelo al usuario
Explicar con claridad que sí es una violación grave, los 6 pasos con plazos, el ticket SEG-FUGA, el contacto del DPO y la alternativa permitida (VPN y equipo corporativo).

### Solución A · Contención y notificación estándar (recomendada)
**Cuándo conviene:** es el camino por defecto ante cualquier envío de datos a cuentas personales.
1. No reenviar ni compartir más la información y no borrar el mensaje enviado.
2. Reportar a la Mesa de Seguridad y al DPO (dpo@nexora.example) con un ticket **SEG-FUGA** en máximo 1 hora.
3. Listar qué información y cuántos registros se enviaron.
4. Legal evalúa la notificación a la autoridad en 72 horas.
5. Para trabajar desde casa, usar solo la VPN y el equipo asignado.

### Solución B · Borrado seguro supervisado
**Cuándo conviene:** si la información ya está en una cuenta personal y debe eliminarse.
1. Abrir el ticket **SEG-FUGA** sin borrar nada por cuenta propia.
2. Seguridad guarda una copia de evidencia del mensaje.
3. Con autorización del DPO, Seguridad supervisa el borrado seguro de la cuenta personal.

### Solución C · Inventario de lo enviado
**Cuándo conviene:** si no se sabe con exactitud qué y cuánto se envió.
1. Abrir el ticket **SEG-FUGA** en máximo 1 hora.
2. Seguridad llena el inventario con la plantilla **FOR-DPO-02** en máximo 4 horas.
3. El inventario sirve a Legal para decidir la notificación dentro de las 72 horas.

### Solución D · Teletrabajo formal posterior
**Cuándo conviene:** si el motivo fue trabajar desde casa y se quiere evitar que se repita.
1. Cumplir los pasos de contención de la Solución A.
2. Solicitar el teletrabajo formal con el formulario **FOR-TEL-01**.
3. Trabajar solo con VPN corporativa y el equipo asignado.

### Solución E · Escalamiento directo a Legal y Dirección
**Cuándo conviene:** si los datos eran de muchos clientes o muy sensibles.
1. Abrir el ticket **SEG-FUGA** y avisar al DPO.
2. Llamar a Legal en máximo 2 horas para evaluar la notificación.
3. Legal informa a la Dirección el mismo día.

## SEG-005 · Pérdida o robo de un dispositivo corporativo (laptop o celular)

**Severidad:** Alta
**Palabras clave:** me robaron, robo, robaron, perdí, extravié, laptop robada, celular robado, celular de la empresa, celular corporativo, laptop de la empresa, portátil, equipo robado, pérdida de dispositivo

### Cuándo aplica
Cuando se pierde, extravía o roban un dispositivo corporativo (laptop, celular, tablet) o uno personal con acceso al correo o sistemas de la compañía.

### Plazo de reporte
Máximo **1 hora** desde que se detecta.

### Acciones inmediatas (obligatorias)
1. Llamar a la Mesa de Seguridad (ext. 4400, 24/7) o al 01-800-NEXORA.
2. Seguridad ejecuta el **bloqueo y borrado remoto (MDM)** y revoca sesiones, tokens y el correo del dispositivo.
3. Cambiar las contraseñas de las cuentas usadas en el dispositivo.
4. En caso de robo, presentar la **denuncia** ante la autoridad en **24 horas** y entregar una copia a Seguridad.
5. TI entrega un equipo de reemplazo en **48 horas**.
6. Informar qué información contenía el dispositivo.

### Responsables y escalamiento
Mesa de Seguridad → TI (reemplazo) → Legal (si había datos de clientes).

### Qué NO hacer
- No intentar recuperarlo por cuenta propia.
- No esperar "a ver si aparece".
- No publicarlo en redes sociales.

### Medidas posteriores
Sin sanción si se reporta dentro de la hora. Si se tarda más de 24 horas, se evalúa negligencia.

### Respuesta modelo al usuario
Priorizar la llamada inmediata, explicar el bloqueo remoto, los 6 pasos con plazos (denuncia en 24 h, reemplazo en 48 h) y la ausencia de sanción si reportó a tiempo.

### Solución A · Bloqueo y borrado remoto inmediato (recomendada)
**Cuándo conviene:** es el camino por defecto ante cualquier pérdida o robo.
1. Llamar a la Mesa de Seguridad (ext. 4400, 24/7) o al 01-800-NEXORA en máximo 1 hora.
2. Seguridad ejecuta el bloqueo y borrado remoto (MDM) y revoca sesiones y tokens.
3. Cambiar las contraseñas de las cuentas usadas en el dispositivo.
4. En caso de robo, presentar la denuncia en 24 horas.
5. TI entrega un equipo de reemplazo en 48 horas.

### Solución B · Bloqueo primero y borrado diferido
**Cuándo conviene:** si es una pérdida y existe probabilidad real de recuperar el dispositivo.
1. Llamar a la Mesa de Seguridad (ext. 4400) y abrir el ticket **SEG-DISP**.
2. Seguridad bloquea el dispositivo y revoca sesiones, sin borrarlo aún.
3. Si no aparece en 24 horas, Seguridad ejecuta el borrado remoto.

### Solución C · Denuncia acompañada por Legal
**Cuándo conviene:** si fue un robo y la persona no sabe cómo presentar la denuncia.
1. Cumplir los pasos de bloqueo de la Solución A.
2. Legal acompaña la denuncia en máximo 24 horas con el formato **FOR-LEG-04**.
3. Se entrega una copia de la denuncia a Seguridad.

### Solución D · Equipo de contingencia
**Cuándo conviene:** si la persona necesita seguir trabajando de inmediato.
1. Cumplir los pasos de bloqueo de la Solución A.
2. TI presta un equipo de contingencia en máximo 4 horas.
3. El equipo de reemplazo definitivo llega en 48 horas.

### Solución E · Informe de datos expuestos
**Cuándo conviene:** si el dispositivo tenía datos de clientes.
1. Cumplir los pasos de bloqueo de la Solución A.
2. Informar a Legal y al DPO qué información contenía.
3. El DPO completa el informe con la plantilla **FOR-DPO-02** en máximo 24 horas.
