using Infrastructure.Entities;
using SharedKernel.Entities;

namespace Backtesting.Engine;

public class StrategySimulationEngine
{
    public BacktestResult Simulate(
        List<Prediction> predictions,
        decimal stakePerBet)
    {
        decimal profit = 0m;

        decimal equity = 0m;

        decimal peakEquity = 0m;

        decimal maximumDrawdown = 0m;

        int wins = 0;

        int losses = 0;

        // ========================================
        // Check for null & invalid values
        // ========================================

        if (stakePerBet <= 0)
        {
            throw new ArgumentException(
                "Stake per bet must be greater than zero.",
                nameof(stakePerBet));
        }

        if (predictions == null)
        {
            throw new ArgumentNullException(
                nameof(predictions));
        }

        var orderedPredictions =
            predictions
                .Where(x =>
                    x.Match != null)
                .OrderBy(x =>
                    x.Match!.KickoffUtc)
                .ToList();

        foreach (var prediction in orderedPredictions)
        {
            // =========================================
            // DETERMINE HISTORICAL RESULT
            // =========================================

            bool won =
                EvaluatePrediction(prediction);

            // =========================================
            // HISTORICAL CLOSING ODDS
            // =========================================

            decimal odds =
                prediction.ClosingOdds;

            if (odds <= 1m)
            {
                // Cannot perform a valid betting
                // simulation without valid odds.
                continue;
            }

            // =========================================
            // BET PROFIT / LOSS
            // =========================================

            decimal betProfit;

            if (won)
            {
                // ====================================
                // Decimal odds formula:                
                // Profit = stake * (odds - 1)
                // ====================================

                betProfit =
                    stakePerBet *
                    (odds - 1m);

                wins++;
            }
            else
            {
                // ====================================
                // Losing the full stake.
                // =====================================

                betProfit =
                    -stakePerBet;

                losses++;
            }

            profit +=
                betProfit;

            // =========================================
            // EQUITY CURVE
            // =========================================

            equity +=
                betProfit;

            if (equity > peakEquity)
            {
                peakEquity =
                    equity;
            }

            decimal drawdown =
                peakEquity -
                equity;

            if (drawdown > maximumDrawdown)
            {
                maximumDrawdown =
                    drawdown;
            }
        }

        // =========================================
        // TOTAL BETS
        // =========================================

        int totalBets =
            wins + losses;

        // =========================================
        // WIN RATE
        // =========================================

        decimal winRate =
            totalBets > 0
                ? ((decimal)wins /
                   totalBets) * 100m
                : 0m;

        // =========================================
        // ROI
        // =========================================
        //
        // ROI is based on capital actually risked.
        // =========================================

        decimal totalStake =
            totalBets * stakePerBet;

        decimal roi =
            totalStake > 0
                ? (profit /
                   totalStake) * 100m
                : 0m;

        // =========================================
        // DATE RANGE
        // =========================================

        DateTime testedFromUtc =
            orderedPredictions.Any()
                ? orderedPredictions
                    .First()
                    .Match!
                    .KickoffUtc
                : DateTime.MinValue;

        DateTime testedToUtc =
            orderedPredictions.Any()
                ? orderedPredictions
                    .Last()
                    .Match!
                    .KickoffUtc
                : DateTime.MinValue;

        // =========================================
        // RESULT
        // =========================================

        return new BacktestResult
        {
            Id =
                Guid.NewGuid(),

            StrategyName =
                orderedPredictions
                    .FirstOrDefault()
                        ?.StrategyVersion
                        ?? "UNKNOWN",

            WinRate =
                winRate,

            ROI =
                roi,

            Profit =
                profit,

            TotalBets =
                totalBets,

            Wins =
                wins,

            Losses =
                losses,

            MaxDrawdown =
                maximumDrawdown,

            // ============================================
            // SharpeRatio deliberately left at zero
            // until we implement a proper return-series
            // calculation.
            // ============================================

            SharpeRatio =
                0m,

            TestedFromUtc =
                testedFromUtc,

            TestedToUtc =
                testedToUtc,

            CreatedUtc =
                DateTime.UtcNow
        };
    }


    private bool EvaluatePrediction(
        Prediction prediction)
    {
        var match =
            prediction.Match;

        if (match is null)
        {
            return false;
        }

        // =========================================
        // BTTS
        // =========================================

        if (prediction.PredictionType == "BTTS")
        {
            bool bothTeamsScored =
                (match.HomeScore ?? 0) > 0 &&
                (match.AwayScore ?? 0) > 0;

            return prediction.PredictedOutcome
                       .Equals(
                           "YES",
                           StringComparison
                            .OrdinalIgnoreCase)
                ? bothTeamsScored
                : !bothTeamsScored;
        }


        // =========================================
        // OVER / UNDER 2.5
        // =========================================

        if (prediction.PredictionType == "OVER25")
        {
            int totalGoals =
                (match.HomeScore ?? 0) +
                (match.AwayScore ?? 0);

            bool over =
                totalGoals >= 3;

            return prediction.PredictedOutcome
                       .Equals(
                           "OVER",
                           StringComparison
                            .OrdinalIgnoreCase)
                ? over
                : !over;
        }

        // =========================================
        // UNSUPPORTED MARKET
        // =========================================

        throw new NotSupportedException(
            $"Prediction type '{prediction.PredictionType}' " +
            "is not supported by the backtesting engine.");
    }
}
