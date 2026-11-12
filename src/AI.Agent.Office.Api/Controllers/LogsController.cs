using AI.Agent.Office.Api.Logging;
using AI.Agent.Office.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AI.Agent.Office.Api.Controllers;

/// <summary>Registro en vivo de la plataforma (spec 010).</summary>
[ApiController]
[Route("api/logs")]
public sealed class LogsController : ControllerBase
{
    private readonly InMemoryLogStore _store;

    public LogsController(InMemoryLogStore store) => _store = store;

    /// <summary>Entradas nuevas desde el número de secuencia <c>after</c>.</summary>
    [HttpGet]
    public ActionResult<LogsDto> Get([FromQuery] long after = 0, [FromQuery] int max = 400) => Ok(_store.Read(after, Math.Min(max, 1000)));

    /// <summary>Vacía el registro en memoria.</summary>
    [HttpPost("clear")]
    public IActionResult Clear()
    {
        _store.Clear();
        return NoContent();
    }
}
