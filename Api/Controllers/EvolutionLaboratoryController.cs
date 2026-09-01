using AI.Evolution.Laboratory.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/evolution-laboratory")]
public class EvolutionLaboratoryController : ControllerBase
{
    private readonly EvolutionLaboratoryService
        _laboratoryService;

    public EvolutionLaboratoryController(
        EvolutionLaboratoryService laboratoryService)
    {
        _laboratoryService =
            laboratoryService;
    }

    [HttpPost("evaluate")]
    public async Task<IActionResult>
        Evaluate(
            [FromQuery]
            string strategyVersion,
            [FromQuery]
            string strategyFamily)
    {
        if (string.IsNullOrWhiteSpace(
            strategyVersion))
        {
            return BadRequest(
                "strategyVersion is required.");
        }

        if (string.IsNullOrWhiteSpace(
            strategyFamily))
        {
            return BadRequest(
                "strategyFamily is required.");
        }

        var result =
            await _laboratoryService
                .EvaluateStrategyAsync(
                    strategyVersion,
                    strategyFamily);

        return Ok(result);

    }
}
