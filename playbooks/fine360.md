---
nombre: DIMAR
disparadores: entrar a dimar, ingresar a dimar, login dimar, iniciar sesion en dimar, abrir dimar, entrar a fine360, ingresar a fine360
---
# DIMAR

> Pasos tomados del vídeo «Login - Brave 2026-11-10 16-02-35.mp4». Usuario y clave salen de las credenciales del sitio (solo en memoria) o de los campos de «▶ Ejecutar».
> La alerta «Es necesario aceptar las notificaciones para continuar» se acepta sola (el navegador acepta las alertas `alert`).

## Entradas

## Pasos
1. ir | https://fine360.outsourcing.com.co:9865/login
2. esperar | input[placeholder="Ingrese su usuario"]
3. escribir | input[placeholder="Ingrese su usuario"] | {{credencial.usuario}}
4. escribir | input[placeholder="Ingrese su contraseña"] | {{credencial.clave}}
5. seleccionar | select | Outsourcing | Elegir la opción «Outsourcing»
6. clic | button:has-text("Ingresar") | | Ingresar
7. esperar | text=Información Casos | | Esperar el panel principal tras el login
8. clic | button.mud-icon-button:has(svg path[d="M16.59 8.59L12 13.17 7.41 8.59 6 10l6 6 6-6z"]) | | Expandir menú
9. esperar | espera 3 segundos | esperar
10. clic | button:has-text("Logout") | Salir
