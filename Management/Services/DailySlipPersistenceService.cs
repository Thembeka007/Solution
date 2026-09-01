using Betting.DTOs;
using Infrastructure.Data;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Management.Services;

public class DailySlipPersistenceService
{
    private readonly ILogger<DailySlipPersistenceService>
        _logger;

    private readonly SportsBettingDbContext
        _db;

    public DailySlipPersistenceService(
        ILogger<DailySlipPersistenceService> logger,
        SportsBettingDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task SaveAsync(
        List<BettingSlipDto> slips)
    {
        foreach (var slipDto in slips)
        {
            _logger.LogInformation(
                "SLIP Stake={stake} Picks={count}",
                slipDto.Stake,
                slipDto.Picks.Count);

            var now = DateTime.UtcNow;

            var entity =
                new BettingSlip
                {
                    Category =
                        slipDto.Category,

                    Stake =
                        slipDto.Stake,

                    EstimatedOdds =
                        slipDto.EstimatedOdds,

                    PotentialPayout =
                        slipDto.Stake *
                        slipDto.EstimatedOdds,

                    ActualPayout = 0,
                    Profit = 0,

                    Won = false,
                    Settled = false,
                    IsCompleted = false,

                    CreatedDateUtc =
                        now,

                    CreatedUtc = now,
                    UpdatedAtUtc = now,

                    StrategyVersion = "v1"
                };

            foreach (var pick in slipDto.Picks)
            {
                _logger.LogInformation(
                    "Persisting Pick Match={match} Stake={stake}",
                    pick.MatchId,
                    pick.Stake);

                _logger.LogInformation(
                    "PICK Match={match} Stake={stake}",
                    pick.MatchId,
                    pick.Stake);

                entity.Selections.Add(
                    new BettingSlipSelection
                    {
                        MatchId =
                            pick.MatchId,

                        PredictionType =
                            pick.PredictionType,

                        PredictedOutcome =
                            pick.PredictedOutcome,

                        Confidence =
                            pick.Confidence,

                        Odds =
                            pick.Odds,

                        OpeningOdds =
                            pick.Odds,

                        Stake =
                            pick.Stake,

                        FinalModelConfidence =
                            pick.Confidence,

                        BettingCategory =
                            slipDto.Category,

                        StrategyVersion =
                            pick.StrategyVersion,

                        Settled = false,
                        Won = false,

                        CreatedUtc = now,
                        UpdatedAtUtc = now

                    });
            }

            _logger.LogInformation(
                "Slip {category} has {count} selections",
                entity.Category,
                entity.Selections.Count);

            _db.BettingSlips.Add(entity);

        }       

        await _db.SaveChangesAsync();
    }
}
