using AI.Agent.Office.Application.Camila;
using Microsoft.AspNetCore.Mvc;

namespace AI.Agent.Office.Api.Controllers;

/// <summary>Bots con playbook maestro (spec 012): los que LectorUML ejecuta en el entorno de pruebas de Chatbots.</summary>
[ApiController]
[Route("api/bots")]
public sealed class BotsController : ControllerBase
{
    private readonly ICamilaBotSource _bots;

    public BotsController(ICamilaBotSource bots) => _bots = bots;

    /// <summary>Los bots cargados desde <c>playbooks bots/</c>.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BotInfo>>> List(CancellationToken cancellationToken)
    {
        if (_bots.Bots.Count == 0) await _bots.ReloadAsync(cancellationToken);
        return Ok(_bots.Bots);
    }

    /// <summary>Relee los playbooks sin reiniciar; devuelve cuántos bots cargó y los problemas de referencias.</summary>
    [HttpPost("reload")]
    public async Task<IActionResult> Reload(CancellationToken cancellationToken)
    {
        var count = await _bots.ReloadAsync(cancellationToken);
        return Ok(new { bots = count, problems = _bots.Bots.ToDictionary(b => b.Id, b => _bots.Problems(b.Id).Take(20).ToArray()) });
    }
}
