using Infrastructure.Data;
using Management.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/replacements")]
public class ReplacementController
    : ControllerBase
{
    private readonly SportsBettingDbContext _db;

    public ReplacementController(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    // =====================================
    // STRATEGY REPLACEMENTS
    // =====================================

    [HttpGet]
    public async Task<
        ActionResult<
            List<StrategyReplacementDto>>>
                GetAsync()
    {
        var replacements =
            await _db
                .StrategyReplacements
                .OrderByDescending(x =>
                    x.ReplacementDateUtc)
                .Select(x =>
                    new StrategyReplacementDto
                    {
                        OldStrategyVersion =
                            x.OldStrategyVersion,

                        NewStrategyVersion =
                            x.NewStrategyVersion,

                        OldStrategyROI =
                            x.OldStrategyROI,

                        ReplacementROI =
                            x.ReplacementROI,

                        ReplacementDateUtc =
                            x.ReplacementDateUtc,

                        Activated =
                            x.Activated
                    })
                .ToListAsync();

        return Ok(replacements);
    }
}
