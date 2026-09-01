using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;
using SharedKernel.Enums;

namespace Management.Services;

public class SlipSettlementService
{
    private readonly SportsBettingDbContext
        _db;

    private readonly ILogger<SlipSettlementService>
        _logger;

    public SlipSettlementService(
        SportsBettingDbContext db,
        ILogger<SlipSettlementService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SettleAsync()
    {
        var slips =
            await _db.BettingSlips
                .Include(x => x.Selections)
                    .ThenInclude(x => x.Match)
                .Where(x => !x.Settled)
                .ToListAsync();

        _logger.LogInformation(
            "Found {count} unsettled slips",
            slips.Count);

        foreach (var slip in slips)
        {
            await SettleSlipAsync(slip);
        }

        await _db.SaveChangesAsync();
    }

    private Task SettleSlipAsync(
        BettingSlip slip)
    {
        bool allFinished = true;
        bool allWon = true;

        foreach (var selection
            in slip.Selections)
        {
            var match =
                selection.Match;


            _logger.LogInformation(
                "Slip={slipId} Match={matchId} Status={status}",
                slip.Id,
                match.Id,
                match.Status);

            if (match.Status !=
                MatchStatus.Completed)
            {
                _logger.LogInformation(
                    "Match not finished yet");

                allFinished = false;
                break;
            }

            bool won =
                EvaluateSelection(
                    selection,
                    match);

            selection.Won = won;
            selection.Settled = true;
            selection.SettledUtc =
                DateTime.UtcNow;

            selection.HomeScore =
                match.HomeScore;

            selection.AwayScore =
                match.AwayScore;

            if (!won)
            {
                allWon = false;
            }
        }

        if (!allFinished)
        {
            return Task.CompletedTask;
        }

        slip.IsCompleted = true;
        slip.Settled = true;
        slip.Won = allWon;

        if (allWon)
        {
            slip.ActualPayout =
                slip.Stake *
                slip.EstimatedOdds;

            slip.Profit =
                slip.ActualPayout -
                slip.Stake;
        }
        else
        {
            slip.ActualPayout = 0;

            slip.Profit =
                -slip.Stake;
        }

        slip.UpdatedAtUtc =
            DateTime.UtcNow;

        _logger.LogInformation(
            "Settled slip {id} Won={won}",
            slip.Id,
            allWon);

        return Task.CompletedTask;
    }

    private static bool EvaluateSelection(
        BettingSlipSelection selection,
        Match match)
    {
        return selection
            .PredictedOutcome switch
        {
            "HOME_WIN" =>
                match.HomeScore >
                match.AwayScore,

            "AWAY_WIN" =>
                match.AwayScore >
                match.HomeScore,

            "DRAW" =>
                match.HomeScore ==
                match.AwayScore,

            "OVER_2_5" =>
                (match.HomeScore +
                 match.AwayScore) > 2,

            "YES" =>
                match.HomeScore > 0 &&
                match.AwayScore > 0,

            _ => false
        };
    }
}
