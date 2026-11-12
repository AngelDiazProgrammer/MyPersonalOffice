using AI.Agent.Office.Api.Auth;
using AI.Agent.Office.Infrastructure.Camila;

namespace AI.Agent.Office.Api.Extensions;

/// <summary>Comandos de mantenimiento que se ejecutan desde la línea de comandos y terminan, sin levantar el servidor.</summary>
public static class OfficeCommands
{
    /// <summary>
    /// Ejecuta el comando pedido en los argumentos: <c>--camila-generar</c> (spec 009) o <c>--hash-contrasena &lt;clave&gt;</c> (spec 010,
    /// escribe la línea de hash para <c>configuration/usuarios.md</c>). Devuelve false si no se pidió ninguno y el programa debe arrancar normalmente.
    /// </summary>
    public static bool TryRun(string[] args, out int exitCode)
    {
        exitCode = 0;

        var hashAt = Array.FindIndex(args, a => string.Equals(a, "--hash-contrasena", StringComparison.OrdinalIgnoreCase));
        if (hashAt >= 0)
        {
            if (hashAt + 1 >= args.Length)
            {
                Console.Error.WriteLine("Uso: --hash-contrasena <contraseña>");
                exitCode = 1;
                return true;
            }
            Console.WriteLine(PasswordHasher.Hash(args[hashAt + 1]));
            return true;
        }

        if (!CamilaGeneratorCommand.IsRequested(args)) return false;
        exitCode = CamilaGeneratorCommand.Run(args, Console.Out);
        return true;
    }
}
