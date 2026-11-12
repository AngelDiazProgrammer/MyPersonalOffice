# ATLAS · Reglas para agentes de desarrollo

Este proyecto sigue **Spec-Driven Development** (Spec Kit). Antes de tocar código o artefactos:

1. Lee y respeta [`.specify/memory/constitution.md`](.specify/memory/constitution.md). No la modifiques en silencio (Art. 40); los agentes proponen, no aprueban (Art. 29, 40).
2. Sigue el flujo del Art. 28: **Specify → Clarify → Plan → Tasks → Analyze → Implement → Validación → Converge**. No se implementa nada significativo sin especificación y plan aprobados. Los cambios menores de mantenimiento pueden seguir un procedimiento simplificado si no afectan arquitectura, seguridad ni comportamiento.
3. Cada comando `/speckit-*` tiene un **prompt maestro** en [`.specify/maestros/`](.specify/maestros/README.md) que debe seguirse al pie de la letra (responsabilidad única, límites, estructura de salida, entrega). Los skills ya lo referencian.
4. Inspecciona el código real antes de modificarlo (Art. 41); no asumas funcionalidades inexistentes; no salgas del alcance autorizado.
5. Un cambio no está terminado porque compile (Art. 42): hay que ejecutar las pruebas y comprobar los criterios de aceptación.

## Datos del proyecto

- Solución .NET 8 por capas: `src/ATLAS.{Domain,Application,Infrastructure,Api,Web}` y `tests/ATLAS.Tests`.
- Agentes en `agents/*.md`; oficinas, proveedores de modelos y opciones en `configuration/appsettings.json`; secretos solo en `.env` (ignorado por git).
- Comprobación estándar: `dotnet build ATLAS.sln` y `dotnet test ATLAS.sln` (detén antes cualquier instancia en ejecución de la app: bloquea los DLL).
- Código y documentación en español, con `#region` por funcionalidad en las clases grandes (ver README, "Convenciones del código").
