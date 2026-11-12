---
nombre: Enviar correo (Gmail)
disparadores: enviar correo, mandar correo, enviar un correo, enviarlo por correo, envíalo por correo, mandar email, enviar email
---
# Enviar correo (Gmail)

> Playbook de ejemplo para el sitio con identificador `gmail` (URL `https://mail.google.com/mail/u/0/#inbox`, dominios permitidos `google.com`, `gmail.com`).
> Inicia sesión una vez con «Conexiones → Iniciar sesión». Los pasos 4-6 están en texto libre a propósito: la primera vez el modelo los resuelve
> mirando la página y el sistema propone un playbook estructurado que, al aprobarlo, se ejecuta sin modelo.
> Los selectores dependen del idioma de la interfaz de Gmail (aquí, español); si cambian, la autorreparación propone la actualización.

## Entradas
- destinatario | ¿A quién va dirigido el correo? (dirección de correo) | correo
- motivo | ¿Para qué es el correo? Cuéntame el objetivo en una frase.
- asunto | redactar | Asunto breve y claro, coherente con el motivo
- cuerpo | redactar | Cuerpo formal del correo según el motivo y el contexto de la conversación (por ejemplo, la solución del caso resuelto). Saludo, desarrollo en pocas frases y cierre. No inventes datos; si falta algo, escribe [POR DEFINIR].

## Pasos
1. ir | https://mail.google.com/mail/u/0/#inbox
2. esperar | div[gh="cm"]
3. clic | div[gh="cm"] | | Abrir la redacción de un correo nuevo
4. Escribe {{destinatario}} en el campo de destinatarios («Para») de la ventana de redacción y confirma la dirección
5. Escribe {{asunto}} en el campo «Asunto»
6. Escribe {{cuerpo}} en el cuerpo del mensaje
7. clic | div[role="button"][aria-label^="Enviar"] | | Enviar el correo | irreversible
8. Comprueba que apareció la confirmación de que el mensaje se envió
