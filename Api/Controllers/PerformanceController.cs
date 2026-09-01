using Infrastructure.Data;
using Management.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/performance")]
public class PerformanceController
    : ControllerBase
{
    private readonly SportsBettingDbContext
        _db;

    public PerformanceController(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    // =====================================
    // DAILY SNAPSHOTS
    // =====================================

    [HttpGet("daily")]
    public async Task<
        ActionResult<
            List<PerformanceSnapshotDto>>>
                GetDailyAsync()
    {
        var snapshots =
            await _db
                .PerformanceSnapshots
                .OrderByDescending(x =>
                    x.SnapshotDateUtc)
                .Select(x =>
                    new PerformanceSnapshotDto
                    {
                        SnapshotDateUtc =
                            x.SnapshotDateUtc,

                        OpeningBalance =
                            x.OpeningBalance,

                        ClosingBalance =
                            x.ClosingBalance,

                        ProfitLoss =
                            x.ProfitLoss,

                        ROI =
                            x.ROI,

                        WinRate =
                            x.WinRate,

                        TotalSlips =
                            x.TotalSlips,

                        WonSlips =
                            x.WonSlips,

                        LostSlips =
                            x.LostSlips,

                        WinningStreak =
                            x.WinningStreak,

                        BreadROI =
                            x.BreadROI,

                        AverageROI =
                            x.AverageROI,

                        LotteryROI =
                            x.LotteryROI,

                        ConfidenceAccuracy =
                            x.ConfidenceAccuracy,

                        StrategyReplacementTriggered =
                            x.StrategyReplacementTriggered,

                        ActiveStrategyVersion =
                            x.ActiveStrategyVersion
                    })
                .ToListAsync();

        return Ok(snapshots);
    }
}
