using Analytics.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;
using SharedKernel.Enums;

namespace Betting.Settlement;

public class PredictionSettlementService
{
    private readonly SportsBettingDbContext
        _db;

    private readonly ClosingLineValueService
     _clvService;

    private readonly ILogger<PredictionSettlementService>
    _logger;

    public PredictionSettlementService(
        SportsBettingDbContext db,
        ClosingLineValueService clvService,
        ILogger<PredictionSettlementService> logger
        )
    {
        _db = db;
        _clvService = clvService;
        _logger = logger;
    }

    public async Task SettleAsync()
    {
        // =========================================
        // LOAD ACTIVE SLIPS
        // =========================================

        var slips =
            await _db.BettingSlips
                .Include(x => x.Selections)
                .ThenInclude(x => x.Match)
                .Where(x => !x.Settled)
                .ToListAsync();

        foreach (var slip in slips)
        {
            bool allSettled = true;
            bool allWon = true;
            decimal totalProfit = 0;

            foreach (var selection in slip.Selections)
            {
                if (selection.Settled)
                    continue;

                var match =
                    selection.Match;

                // =====================================
                // MATCH NOT COMPLETED
                // =====================================

                if (match.Status !=
                    MatchStatus.Completed)
                {
                    allSettled = false;
                    continue;
                }

                selection.HomeScore =
                    match.HomeScore;

                selection.AwayScore =
                    match.AwayScore;

                // =====================================
                // DETERMINE RESULT
                // =====================================

                bool won =
                    EvaluatePrediction(
                        selection,
                        match);

                selection.Won = won;
                selection.Settled = true;

                if (won)
                {
                    selection.ProfitLoss =
                        selection.Stake *
                        (selection.Odds - 1m);
                }
                else
                {
                    selection.ProfitLoss =
                        -selection.Stake;
                }

                selection.SettledUtc =
                    DateTime.UtcNow;

                var latestOdds =
                    await _db.MatchOdds
                        .Where(x =>
                        x.MatchId ==
                            selection.MatchId)
                        .OrderByDescending(x =>
                            x.RetrievedUtc)
                        .FirstOrDefaultAsync();

                if (latestOdds is not null)
                {
                    selection.ClosingOdds =
                        latestOdds.HomeOdds;

                    selection.ClosingLineValue =
                        _clvService.Calculate(
                            selection.OpeningOdds,
                            latestOdds.HomeOdds);
                }

                var prediction =
                    await _db.Predictions
                        .FirstOrDefaultAsync(x =>
                            x.MatchId == selection.MatchId
                            &&
                            x.PredictionType == selection.PredictionType
                            &&
                            x.PredictedOutcome == selection.PredictedOutcome);


                if (prediction is not null)
                {
                    _logger.LogInformation(
                        "Prediction FOUND {id}",
                        prediction.Id);

                    _logger.LogInformation(
                        "Looking for prediction Match={match} Type={type} Outcome={outcome}",
                        selection.MatchId,
                        selection.PredictionType,
                        selection.PredictedOutcome);


                    prediction!.IsSettled = true;

                    prediction.IsCorrect =
                        selection.Won;

                    prediction.SettledUtc =
                        DateTime.UtcNow;

                }

                if (!won)
                    allWon = false;
            }

            // =========================================
            // SLIP NOT READY
            // =========================================

            if (!allSettled)
                continue;

            // =========================================
            // FINALIZE SLIP
            // =========================================

            slip.Won = allWon;
            slip.Settled = true;

            if (allWon)
            {
                slip.ActualPayout =
                    slip.Stake *
                    slip.EstimatedOdds;

                totalProfit =
                    slip.ActualPayout -
                    slip.Stake;
            }
            else
            {
                slip.ActualPayout = 0;
                totalProfit =
                    -slip.Stake;
            }

            foreach (var selection in slip.Selections)
            {
                var prediction =
                    await _db.Predictions
                        .FirstOrDefaultAsync(x =>
                            x.MatchId == selection.MatchId &&
                            x.PredictionType == selection.PredictionType &&
                            x.PredictedOutcome == selection.PredictedOutcome);

                _logger.LogInformation(
                    "Searching Prediction Match={match} Type={type} Outcome={outcome}",
                    selection.MatchId,
                    selection.PredictionType,
                    selection.PredictedOutcome);

                if (prediction is null)
                {
                    _logger.LogError(
                        "Prediction NOT FOUND Match={match} Type={type} Outcome={outcome}",
                        selection.MatchId,
                        selection.PredictionType,
                        selection.PredictedOutcome);
                    continue;
                }
                else
                {
                    _logger.LogInformation(
                        "Prediction FOUND {id}",
                        prediction.Id);
                }
               

                prediction.IsSettled = true;

                prediction.IsCorrect =
                    selection.Won;

                prediction.SettledUtc =
                    DateTime.UtcNow;

                prediction.ProfitLoss =
                    selection.ProfitLoss;

                prediction.Stake =
                    selection.Stake;

                prediction.ClosingOdds =
                    selection.ClosingOdds ?? 0m;


            }
        }

        int settledSlips =
            slips.Count(x =>
                x.Settled);

        int wonSlips =
            slips.Count(x =>
                x.Won == true);

        int lostSlips =
            settledSlips - wonSlips;

        await _db.SaveChangesAsync();

        Console.WriteLine(
            $"SETTLEMENT COMPLETE | " +
            $"Settled: {settledSlips} | " +
            $"Won: {wonSlips} | " +
            $"Lost: {lostSlips}");

    }

    // =============================================
    // PREDICTION EVALUATOR
    // =============================================

    private bool EvaluatePrediction(
        BettingSlipSelection selection,
        Match match)
    {
        int home =
            match.HomeScore ?? 0;

        int away =
            match.AwayScore ?? 0;

        // =========================================
        // BTTS
        // =========================================

        if (selection.PredictionType ==
            "BTTS")
        {
            bool btts =
                home > 0 &&
                away > 0;

            return selection.PredictedOutcome ==
                   "YES"
                ? btts
                : !btts;
        }

        // =========================================
        // OVER 2.5
        // =========================================

        if (selection.PredictionType ==
            "OVER25")
        {
            bool over =
                (home + away) > 2;

            return selection.PredictedOutcome ==
                   "OVER_2_5"
                ? over
                : !over;
        }

        // =========================================
        // MATCH RESULT
        // =========================================

        if (selection.PredictionType ==
            "MATCH_RESULT")
        {
            if (home > away &&
                selection.PredictedOutcome ==
                "HOME_WIN")
            {
                return true;
            }

            if (away > home &&
                selection.PredictedOutcome ==
                "AWAY_WIN")
            {
                return true;
            }

            if (home == away &&
                selection.PredictedOutcome ==
                "DRAW")
            {
                return true;
            }
        }

        return false;
    }
}
