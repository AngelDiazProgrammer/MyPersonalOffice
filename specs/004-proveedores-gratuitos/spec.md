# Especificación 004 · Proveedores de modelos: APIs gratuitas primero, Claude de respaldo (incluye Gemini)

**Estado:** Implementada — especificación **retroactiva** (Art. 15, 28 y 30), reunida de varias iteraciones (enrutamiento gratuito, privacidad, Gemini, diagnóstico).
**Constitución:** v1.1.0 · **Maestro:** `.specify/maestros/02_SPECIFY.md` · **Fecha:** 2026-11-10

## Resumen
**Proveedores:** las solicitudes a modelos se atienden primero con APIs gratuitas intercambiables (Groq, Cloudflare, Gemini) en orden configurable, y con Claude cuando ninguna puede o cuando el contenido es sensible.

## Problema
Usar solo Claude cuesta dinero en pruebas; los proveedores gratuitos tienen límites de uso, modelos que cambian y formatos distintos; algunos fallos (errores con formato inesperado) tumbaban solicitudes completas.

## Objetivo y valor
Minimizar el costo manteniendo respuestas disponibles, con visibilidad del consumo por proveedor y control de qué contenido puede salir hacia cada uno.

## Usuarios y actores
- **Persona usuaria:** elige el modo de enrutamiento y ve el consumo.
- **Responsable de configuración:** añade proveedores y claves sin programar.
- **Proveedores externos** (Groq, Cloudflare, Gemini, Anthropic).

## Historias de usuario
- HU-001: Como persona usuaria, quiero que se usen primero las APIs gratuitas, para gastar lo menos posible.
- HU-002: Como responsable, quiero añadir un proveedor con una entrada de configuración, para no programar cada uno.
- HU-003: Como persona usuaria, quiero ver cuánto consume cada proveedor y cuál está en pausa, para entender qué responde.
- HU-004: Como responsable, quiero probar un proveedor concreto, para diagnosticar claves, modelos y conexión.

## Requisitos funcionales
- **RF-001** Existen tres modos: gratis primero (predeterminado), solo Claude y solo gratis; se pueden cambiar en tiempo de ejecución.
- **RF-002** Los proveedores gratuitos se definen por configuración (nombre, región, dirección base, variable de la clave, modelo, límites, parámetros extra) y se prueban en el orden configurado; un solo cliente genérico los atiende.
- **RF-003** El proveedor gratuito Gemini está disponible con el modelo `gemini-flash-lite-latest`, accedido mediante la interfaz compatible con OpenAI, con su clave en una variable de entorno.
- **RF-004** Ante límite de uso, clave rechazada, modelo no disponible, error del servidor, tiempo agotado o respuesta inservible, el proveedor se pausa o se omite y se prueba el siguiente; si ninguno responde, se usa Claude (salvo modo solo gratis).
- **RF-005** Cualquier respuesta rara o ilegible de un proveedor (incluidos errores como arreglos JSON) se trata como fallo de ese proveedor y no tumba la solicitud.
- **RF-006** Las solicitudes marcadas como sensibles (documentos o entradas confidenciales) nunca se envían a proveedores gratuitos; en modo solo gratis se rechazan con un mensaje claro.
- **RF-007** Se puede forzar Claude por solicitud o por agente.
- **RF-008** El consumo de tokens y llamadas se separa por proveedor (Claude y cada gratuito) y por agente.
- **RF-009** Al iniciar se verifica en segundo plano que el modelo configurado exista para cada clave (aceptando ids con prefijo `models/`) y se informa el problema sin tumbar la aplicación.
- **RF-010** Se puede probar un proveedor concreto con una petición mínima, opcionalmente con otro modelo, y se obtiene si funciona, la latencia o el error exacto.
- **RF-011** Los mensajes de error de proveedores se muestran sin datos internos (spec 003).
- **RF-012** Claude reintenta sin el parámetro de temperatura o con selección automática de herramienta cuando el modelo los rechaza, y lo recuerda por modelo.

## Requisitos no funcionales
- **RNF-001** Las claves solo se leen del entorno; no se registran ni llegan al navegador.
- **RNF-002** Cada proveedor tiene tiempo máximo configurable y su pausa no bloquea a los demás.

## Reglas de negocio
- **RN-001** Anthropic es el proveedor de pago principal; los gratuitos son alternativas intercambiables (Art. 15).
- **RN-002** Un proveedor no configurado (sin clave) no se usa ni se considera fallo.
- **RN-003** La privacidad prevalece sobre el costo: el contenido sensible va solo a Claude.

## Criterios de aceptación
- **CA-001** Dados proveedores gratuitos sanos, cuando se envía una solicitud, entonces responde el primero y no se llama a Claude (RF-001).
- **CA-002** Dado un proveedor con límite de uso, cuando se envía una solicitud, entonces se pausa y responde el siguiente (RF-004).
- **CA-003** Dados todos los gratuitos fallando, cuando se envía una solicitud, entonces responde Claude (RF-004); en modo solo gratis, se informa por qué no hubo respuesta.
- **CA-004** Dada una respuesta de error con forma de arreglo JSON, cuando ocurre, entonces el proveedor se omite y la solicitud no falla (RF-005).
- **CA-005** Dada una solicitud sensible, cuando se enruta, entonces no llega a ningún gratuito (RF-006).
- **CA-006** Dado el listado de modelos de Gemini con prefijo `models/`, cuando se verifica el arranque, entonces no se marca como no disponible un modelo existente (RF-009).
- **CA-007** Dado un proveedor concreto, cuando se prueba, entonces se devuelve éxito con respuesta y latencia, o el error real (RF-010).
- **CA-008** Dado el uso de varios proveedores, cuando se consulta el consumo, entonces se desglosa por proveedor (RF-008).
- **CA-009** Dada una configuración con Gemini y su clave, cuando Groq y Cloudflare no están disponibles, entonces las respuestas, el flujo de la base y los entregables se completan con Gemini (RF-003).
- **CA-010** Dado un modelo de Claude que rechaza `temperature`, cuando se llama, entonces se reintenta sin él y se recuerda (RF-012).

## Alcance
**Incluido:** enrutamiento, tres proveedores gratuitos, privacidad por sensibilidad, consumo, diagnóstico, resiliencia a errores.
**Excluido:** selector de proveedor por solicitud desde la interfaz, políticas de privacidad por proveedor (p. ej. excluir uno de datos sensibles), facturación y costos reales, resolución de DNS alternativa.

## Datos e interfaces
Entradas: solicitud (instrucciones, mensajes, límites, sensibilidad, preferencia). Salidas: respuesta con proveedor, consumo, estado de proveedores, resultado de prueba. Estados de proveedor: sin clave, disponible, en pausa con error.

## Supuestos y dependencias
- Las claves gratuitas tienen cupos que pueden agotarse; los nombres de modelos gratuitos cambian.
- Gemini depende de la resolución DNS del equipo (en la red corporativa de la persona falló al inicio).

## Riesgos funcionales y casos límite
- La capa gratuita de Gemini puede usar el contenido para mejorar productos de Google: no apta para datos reales (ver spec 002, Q-001).
- Modelos «razonadores» pueden devolver texto vacío con pocos tokens de salida.

## Preguntas abiertas
- **Q-001** (RESUELTA 2026-11-10) Por ahora, para pruebas, la política queda como está (sin bandera por proveedor). Se reabre antes de usar datos reales.
- **Q-002** ¿Se agrega un selector de proveedor en la interfaz?
