using Infrastructure.Data;
using Management.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/strategies")]
public class StrategyController
    : ControllerBase
{
    private readonly SportsBettingDbContext
        _db;

    public StrategyController(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<
        ActionResult<
            List<StrategyPerformanceDto>>>
                GetAsync()
    {
        var data =
            await _db
                .StrategyPerformances
                .OrderByDescending(x =>
                    x.ROI)
                .Select(x =>
                    new StrategyPerformanceDto
                    {
                        StrategyVersion =
                            x.StrategyVersion,

                        ROI =
                            x.ROI,

                        Accuracy =
                            x.Accuracy,

                        AverageConfidence =
                            x.AverageConfidence,

                        ConfidenceGap =
                            x.ConfidenceAccuracyGap,

                        IsActive =
                            x.IsActive,

                        ReplacementTriggered =
                            x.ReplacementTriggered,

                        LastEvaluationUtc =
                            x.LastEvaluationUtc
                    })
                .ToListAsync();

        return Ok(data);
    }
}
