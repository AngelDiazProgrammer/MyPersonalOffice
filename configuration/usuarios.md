# Usuarios de la plataforma

Acceso de administración de **ATLAS** (spec 010). La contraseña **no se guarda en claro**: solo su hash PBKDF2-SHA256 con sal.

| Usuario | Rol | Contraseña (hash) |
|---|---|---|
| addiazf | admin | pbkdf2-sha256$210000$T3vemtrX9yd0ovT09d7eGQ==$X37PbxihcUUDmuFkbpRXFt6i2XNYY1igJvyFwmhux+U= |

## Cómo cambiar una contraseña o agregar un usuario
1. Genera el hash: `dotnet run --project src/ATLAS.Web -- --hash-contrasena "<nueva contraseña>"`
2. Pega la línea que imprime en la tabla de arriba (una fila por usuario; el rol por ahora es `admin`).
3. No hace falta reiniciar: el archivo se lee en cada intento de acceso.

Cinco intentos fallidos seguidos bloquean el acceso 5 minutos. No escribas contraseñas en claro en este archivo.
