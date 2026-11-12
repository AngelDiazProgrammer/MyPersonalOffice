using ATLAS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ATLAS.Api.Middleware;

/// <summary>Manejo central de errores: la persona recibe mensajes seguros, nunca trazas de la pila.</summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta la petición y traduce las excepciones (validación, petición mal formada, error inesperado)
    /// en respuestas JSON seguras.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = 499; // client closed the request
        }
        catch (OfficeValidationException ex)
        {
            await WriteAsync(context, StatusCodes.Status400BadRequest, "Solicitud no válida", string.Join(" ", ex.Errors), ex.Errors);
        }
        catch (BadHttpRequestException ex)
        {
            await WriteAsync(context, StatusCodes.Status400BadRequest, "Solicitud no válida", ex.Message, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado en {Path}", context.Request.Path);
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "Error interno",
                "Ocurrió un error inesperado. Revisa los registros del servidor.", null);
        }
    }

    /// <summary>Escribe la respuesta de error con su código, título y lista de problemas.</summary>
    private static Task WriteAsync(HttpContext context, int status, string title, string detail, IEnumerable<string>? errors)
    {
        if (context.Response.HasStarted) return Task.CompletedTask;

        context.Response.StatusCode = status;
        var problem = new ProblemDetails { Status = status, Title = title, Detail = detail };
        if (errors is not null) problem.Extensions["errors"] = errors.ToArray();
        return context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
}
