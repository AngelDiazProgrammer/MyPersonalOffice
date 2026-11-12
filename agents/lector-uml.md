---
name: LectorUML
role: Ejecutor de flujos de bots
description: Lee el playbook maestro de un bot y ejecuta su flujo UML en el entorno de pruebas, con sus botones, textos, formularios, rutas y webhooks tal como los define el bot.
purpose: Ejecutar con fidelidad los flujos UML de los bots a partir de sus playbooks maestros (carpeta «playbooks bots»), en el entorno de pruebas de Chatbots.
kind: executor
environment: chatbot
tools: leer_playbook, consultar_servicio, escribir_servicio
color: "#34d399"
icon: 🧩
system: true
---
# Agente: LectorUML

**No usa un modelo de lenguaje.** Es un ejecutor determinista: así el UML es repetible y fiel al bot, y las escrituras reales en los sistemas nunca quedan a merced de una respuesta probabilística.

## Qué hace
1. Recibe un bot (por ejemplo `camila`) y carga su playbook maestro de `playbooks bots/<bot>/`.
2. Ejecuta el flujo página a página, **en el orden que indica el playbook**: asigna variables, llama webhooks, ejecuta generadores, dice los mensajes, ofrece los botones, pide los datos del formulario y evalúa las rutas.
3. Muestra en cada paso lo que trae el bot: textos originales, botones con su evento, formulario, webhooks (con su modo) y rutas; además de la traza de lo que hizo.
4. Atiende los comandos de navegación: `/jerarquia`, `/ir <flujo> › <página>`, `/ficha`, `/vars`, `/reiniciar`.

## Controles que aplica el código
- Los webhooks solo van a hosts de la lista permitida, con los secretos de `.env`.
- Los webhooks de **escritura** piden confirmación explícita («sí») mostrando el cuerpo exacto antes de ejecutarse.
- Los generadores de IA del bot original no se ejecutan en el UML (el parámetro queda sin valor y la traza lo indica).
- El estado de la conversación vive solo en memoria.

## Límites
No inventa pasos ni rutas: si el playbook no lo dice, no ocurre.
