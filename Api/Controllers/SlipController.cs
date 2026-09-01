using Infrastructure.Data;
using Management.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/slips")]
public class SlipController
    : ControllerBase
{
    private readonly SportsBettingDbContext
        _db;

    public SlipController(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    // =====================================
    // TODAY'S SLIPS
    // =====================================

    [HttpGet("today")]
    public async Task<
        ActionResult<
            List<BettingSlipResponseDto>>>
                GetTodayAsync()
    {
        var today =
            DateTime.UtcNow.Date;

        var slips =
            await _db.BettingSlips
                .Include(x =>
                    x.Selections)
                        .ThenInclude(x =>
                            x.Match)
                                .ThenInclude(x =>
                                    x.HomeTeam)
                .Include(x =>
                    x.Selections)
                        .ThenInclude(x =>
                            x.Match)
                                .ThenInclude(x =>
                                    x.AwayTeam)
                .Where(x =>
                    x.CreatedDateUtc.Date
                        == today)
                .OrderByDescending(x =>
                    x.CreatedDateUtc)
                .ToListAsync();

        var output =
            slips.Select(x =>
                new BettingSlipResponseDto
                {
                    Id =
                        x.Id,

                    Category =
                        x.Category,

                    Stake =
                        x.Stake,

                    EstimatedOdds =
                        x.EstimatedOdds,

                    PotentialPayout =
                        x.PotentialPayout,

                    Settled =
                        x.Settled,

                    Won =
                        x.Won == true,

                    CreatedDateUtc =
                        x.CreatedDateUtc,

                    Picks =
                        x.Selections
                            .Select(s =>
                                new BettingSlipSelectionDto
                                {
                                    MatchId =
                                        s.MatchId,

                                    HomeTeam =
                                        s.Match
                                            ?.HomeTeam
                                            ?.Name ?? "",

                                    AwayTeam =
                                        s.Match
                                            ?.AwayTeam
                                            ?.Name ?? "",

                                    PredictionType =
                                        s.PredictionType,

                                    PredictedOutcome =
                                        s.PredictedOutcome,

                                    Confidence =
                                        s.Confidence,

                                    Odds =
                                        s.Odds,

                                    Settled =
                                        s.Settled,

                                    Won =
                                        s.Won == true
                                })
                            .ToList()
                })
            .ToList();

        return Ok(output);
    }
}
