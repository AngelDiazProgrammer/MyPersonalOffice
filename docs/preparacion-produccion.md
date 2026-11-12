# Preparación para producción

Lista de comprobación antes de exponer MyPersonalOffice fuera de tu equipo (Art. 36, spec 005). Cada punto indica **cómo comprobarlo**.
Hoy la aplicación está pensada para uso **local y de una sola persona**; los puntos marcados «pendiente» no están implementados a propósito (ver [decisiones](decisiones.md)).

## 1. Compilación y pruebas
- [ ] `dotnet build AI.Agent.Office.sln` termina sin errores.
- [ ] `dotnet test AI.Agent.Office.sln` termina sin fallos (incluye pruebas de arquitectura, contratos, seguridad y registros).
- [ ] `python tests/security-probes/run_probes.py` contra la aplicación en ejecución devuelve 34/34 (ver [tests/security-probes](../tests/security-probes/)).

## 2. Secretos y configuración
- [ ] Las claves viven solo en `.env` o en variables de entorno; `.env` está en `.gitignore` (`git check-ignore .env`).
- [ ] `.env.example` lista todas las variables que usa la aplicación y no contiene valores reales.
- [ ] `Office:AllowedOrigins` en `configuration/appsettings.json` contiene únicamente las direcciones reales de la aplicación.
- [ ] `AllowedHosts` de `src/AI.Agent.Office.Api/appsettings.json` y `src/AI.Agent.Office.Web/appsettings.json` no es `*` (lo verifica `AllowedHostsTests`).
- [ ] `Office:ChatRequestsPerMinute` ajustado al uso esperado (30 por defecto).

## 3. Estado de la aplicación
- [ ] `GET /health` responde 200 (la aplicación está viva).
- [ ] `GET /health/ready` responde 200 con `{"status":"ready"}`: agentes válidos, base de conocimiento cargada (si `Knowledge:Enabled`) y al menos un proveedor de modelos configurado. Un 503 indica qué componente falta, sin revelar rutas ni claves.
- [ ] `POST /api/llm/test` con cada proveedor configurado responde `ok` (cabecera `X-Office-Client` y origen permitido, ver spec 003).

## 4. Registros y observabilidad
- [ ] La consola emite una línea JSON por evento, con marca de tiempo UTC y alcances `CorrelationId`, `ConversationId`, `TaskId` y `AgentId`.
- [ ] Cada llamada a un modelo registra proveedor, modelo, duración y tokens; ningún registro contiene el texto de los mensajes ni de las respuestas (`LoggingTests`).
- [ ] Los registros se envían a donde los lea el operador (recolector del entorno de despliegue). *Pendiente: no hay almacén de registros propio.*

## 5. Seguridad
- [ ] Respuestas con cabeceras de seguridad (CSP, `nosniff`, `no-referrer`): `curl -I http://localhost:5081/health`.
- [ ] Las peticiones que cambian algo sin origen permitido o sin `X-Office-Client` reciben 403 (probes).
- [ ] Servir siempre por HTTPS detrás de un proxy o servidor que termine TLS. *Pendiente: no se configura aquí.*
- [ ] Autenticación y varios usuarios. *Pendiente por decisión (se pospone; ver decisiones).*

## 6. Datos y privacidad
- [ ] Las solicitudes marcadas como sensibles solo van a Claude (`LlmRoutingTests`).
- [ ] Revisa las condiciones de las APIs gratuitas: pueden usar el contenido para mejorar sus productos (README, sección de privacidad).
- [ ] Los documentos generados quedan en `generated-documents/`; define su limpieza o copia de seguridad. *Pendiente: no hay base de datos ni persistencia (Art. 10).*

## 7. Arquitectura
- [ ] `Web` no referencia ni ve `Infrastructure` (`ArchitectureTests`).

## Conectividad a navegadores (spec 007)
- [ ] Revisar los sitios configurados en `browser-data/sites.json` y sus dominios permitidos; quitar los que no se usen.
- [ ] Revisar los playbooks de `playbooks/` y las propuestas pendientes antes de aprobarlas (ninguna acción irreversible sin aprobación).
- [ ] **Credenciales en texto plano:** `browser-data/credentials/*.md` guarda usuarios y claves sin cifrar (decisión solo para pruebas). Antes de producción: cifrarlas o moverlas a un gestor de secretos, y borrar esos archivos.
- [ ] La carpeta `browser-data/profiles/` contiene sesiones iniciadas: protégela como una credencial y no la copies a otros equipos.
- [ ] Con navegador invisible (`Browser:Headless=true`) solo tras haber comprobado el proceso en modo visible.
- [ ] La aplicación no tiene autenticación: quien alcance la API podría pedir ejecuciones en tus sesiones. No la expongas en red.
