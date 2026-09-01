using Infrastructure.Data;
using Management.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/predictions")]
public class PredictionController
    : ControllerBase
{
    private readonly SportsBettingDbContext _db;

    public PredictionController(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    // =====================================
    // TODAY'S PREDICTIONS
    // =====================================

    [HttpGet("today")]
    public async Task<
        ActionResult<
            List<PredictionResponseDto>>>
                GetTodayAsync()
    {
        var today =
            DateTime.UtcNow.Date;

        var predictions =
            await _db.Predictions
                .Include(x =>
                    x.Match)
                        .ThenInclude(x =>
                            x!.HomeTeam)
                .Include(x =>
                    x.Match)
                        .ThenInclude(x =>
                            x!.AwayTeam)
                .Where(x =>
                    x.MatchDateUtc.Date
                        == today)
                .OrderByDescending(x =>
                    x.Confidence)
                .ToListAsync();

        var output =
            predictions.Select(x =>
                new PredictionResponseDto
                {
                    Id =
                        x.Id,

                    MatchId =
                        x.MatchId,

                    HomeTeam =
                        x.Match
                            ?.HomeTeam
                            ?.Name ?? "",

                    AwayTeam =
                        x.Match
                            ?.AwayTeam
                            ?.Name ?? "",

                    KickoffUtc =
                        x.Match
                            ?.KickoffUtc
                            ?? DateTime.UtcNow,

                    PredictionType =
                        x.PredictionType,

                    PredictedOutcome =
                        x.PredictedOutcome,

                    Confidence =
                        x.Confidence,

                    StrategyVersion =
                        x.StrategyVersion,

                    IsSettled =
                        x.IsSettled,

                    IsCorrect =
                        x.IsCorrect,

                    ProfitLoss =
                        x.ProfitLoss,

                    ClosingOdds =
                        x.ClosingOdds
                })
            .ToList();

        return Ok(output);
    }
}
