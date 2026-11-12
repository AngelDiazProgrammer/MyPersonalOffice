# Especificación 011 · Agentes: catálogo, uso específico y gestión desde el front

**Estado:** Aprobada en chat por la persona responsable (2026-11-12): «cada agente debe tener un uso específico… una sección para crear, editar y eliminar agentes y un ítem del menú «Agentes» donde se listen todos».
**Constitución:** v1.1.0 · **Relacionadas:** 006 (núcleo), 009 (Camila), 010 (plataforma), 012 (playbooks de bots).

## Resumen
La plataforma es un **orquestador multiagente**: cada agente existe para algo concreto. Se agrega un **ítem «Agentes»** con el catálogo de todos los agentes, y la posibilidad de **crearlos, editarlos y eliminarlos desde el front**. Cada agente declara su **uso**, su **tipo** (lo ejecuta un modelo o código), su **entorno** y las **herramientas** que puede usar. CamilaIA y LectorUML pasan a ser agentes reales.

## Problema
Los agentes solo se crean editando archivos y la configuración a mano; no hay vista de conjunto; no se sabe para qué sirve cada uno ni dónde se usa; Camila no es un agente de la plataforma.

## Requisitos funcionales
- **RF-001** El menú tiene un ítem «Agentes» con todos los agentes cargados: icono, nombre, rol, **uso**, tipo, entorno, herramientas, estado, consumo y dónde se usa.
- **RF-002** Todo agente declara un **uso** (una frase, mínimo 10 caracteres); no se puede crear ni guardar sin él.
- **RF-003** Un agente tiene tipo `llm` (lo ejecuta un modelo) o `executor` (lo ejecuta código, sin modelo), y entorno `office`, `chatbot` o `conversational`.
- **RF-004** Desde el front se puede **crear** un agente (id, nombre, rol, uso, tipo, entorno, herramientas, base de conocimiento, icono, color, tope de tokens, instrucciones en Markdown).
- **RF-005** Se puede **editar** cualquiera; el id no cambia. Los campos internos del archivo que el formulario no muestra se conservan.
- **RF-006** Se puede **eliminar** un agente salvo los del sistema (Orquestador, CamilaIA, LectorUML); si está en una oficina se avisa y se bloquea hasta quitarlo de ella.
- **RF-007** Hay un único orquestador y no se puede crear otro desde el front.
- **RF-008** El id usa solo minúsculas, números y guiones; no admite rutas; el archivo no supera 60 KB; las herramientas y el entorno son de las listas válidas.
- **RF-009** Al guardar o eliminar se recarga el registro de agentes sin reiniciar y se deja constancia en el registro de actividad (sin el texto de las instrucciones).
- **RF-010** Las herramientas declaradas se **imponen**: un agente solo puede usar las que tiene (por ejemplo, sin `escribir_servicio` no puede escribir en los sistemas).
- **RF-011** Los agentes de un entorno distinto de `office` no participan en las oficinas virtuales salvo que se agreguen a una.
- **RF-012** Existen los agentes `camila-ia` (llm, conversational) y `lector-uml` (executor, chatbot).

## Requisitos no funcionales
- **RNF-001** Solo la persona autenticada administra agentes; las escrituras son atómicas; no se registran las instrucciones.
- **RNF-002** El formulario es accesible con teclado.

## Reglas de negocio
- **RN-001** Cada agente tiene un uso específico (principio del orquestador multiagente).
- **RN-002** Los controles de seguridad (confirmación de escrituras, hosts permitidos, secretos, datos personales) viven en código; editar un agente no los desactiva.

## Criterios de aceptación
- **CA-001** El menú muestra «Agentes» con todos los agentes y su uso.
- **CA-002** Crear un agente válido lo deja visible y usable sin reiniciar; sin uso o con id inválido se rechaza con un mensaje claro.
- **CA-003** Editar cambia las instrucciones y conserva los campos internos del archivo.
- **CA-004** Eliminar un agente de usuario lo quita; eliminar uno del sistema o uno en una oficina se bloquea con el motivo.
- **CA-005** Un agente sin `escribir_servicio` no puede escribir en los sistemas aunque el modelo lo pida.
- **CA-006** `camila-ia` y `lector-uml` aparecen en el catálogo con su uso.

## Alcance
**Incluido:** modelo de agente, API y pantalla de gestión, validaciones, herramientas impuestas, los dos agentes de sistema. **Excluido:** roles de usuario, versionado de agentes, asignar agentes a oficinas desde el front (se hace en la configuración).

## Supuestos y dependencias
- La carpeta `agents/` es el registro: todo `.md` válido se carga (la lista de `appsettings.json` deja de ser necesaria).
- Depende de la spec 010 (sesión) y alimenta la 012.

## Preguntas abiertas
Ninguna que bloquee.
