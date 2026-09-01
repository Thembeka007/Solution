using AI.Evolution.Laboratory.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;
using SharedKernel.Enums;

namespace AI.Evolution.Laboratory.Services;

public class HistoricalMarketEdgeAnalyzer
{
    private readonly SportsBettingDbContext _db;

    public HistoricalMarketEdgeAnalyzer(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<HistoricalMarketEdgeResult?>
        AnalyzeAsync(Prediction prediction)
    {
        // =========================================
        // MATCH RESULT
        // =========================================

        if (prediction.PredictionType == "MATCH_RESULT")
        {
            return await AnalyzeMatchResultAsync(
                prediction);
        }

        // =========================================
        // OVER 2.5
        // =========================================

        if (prediction.PredictionType == "OVER25")
        {
            return await AnalyzeOver25Async(
                prediction);
        }

        // =========================================
        // UNSUPPORTED MARKET
        // =========================================

        return new HistoricalMarketEdgeResult
        {
            PredictionId =
                prediction.Id,

            PredictionType =
                prediction.PredictionType,

            PredictedOutcome =
                prediction.PredictedOutcome,

            StrategyConfidence =
                prediction.Confidence,

            MarketDataStatus =
                "UnsupportedMarket"
        };
    }

    // =========================================================
    // MATCH RESULT
    // =========================================================

    private async Task<HistoricalMarketEdgeResult?>
        AnalyzeMatchResultAsync(Prediction prediction)
    {
        var observations =
            await _db.MatchOdds
                .Where(x =>
                    x.MatchId == prediction.MatchId &&
                    x.RetrievedUtc <= prediction.CreatedUtc &&
                    x.HomeOdds > 1m &&
                    x.DrawOdds > 1m &&
                    x.AwayOdds > 1m)
                .OrderByDescending(x =>
                    x.RetrievedUtc)
                .ToListAsync();

        if (!observations.Any())
        {
            return new HistoricalMarketEdgeResult
            {
                PredictionId =
                    prediction.Id,

                PredictionType =
                    prediction.PredictionType,

                PredictedOutcome =
                    prediction.PredictedOutcome,

                StrategyConfidence =
                    prediction.Confidence,

                MarketDataStatus =
                    "InsufficientHistoricalData"

            };
        }

        // =========================================
        // LATEST OBSERVATION PER BOOKMAKER
        // =========================================

        var latestPerBookmaker =
            observations
                .GroupBy(x =>
                    x.Bookmaker)
                .Select(group =>
                    group
                        .OrderByDescending(x =>
                            x.RetrievedUtc)
                        .First())
                .ToList();

        if (!latestPerBookmaker.Any())
        {
            return new HistoricalMarketEdgeResult
            {
                PredictionId =
                    prediction.Id,

                PredictionType =
                    prediction.PredictionType,

                PredictedOutcome =
                    prediction.PredictedOutcome,

                StrategyConfidence =
                    prediction.Confidence,

                MarketDataStatus =
                    "InsufficientHistoricalData"
            };
        }

        // =========================================
        // COLLECT PREDICTED MARKET ODDS
        // =========================================

        var marketOddsList =
            new List<decimal>();

        foreach (var observation in latestPerBookmaker)
        {
            decimal odds =
                prediction.PredictedOutcome switch
               {
                   "HOME_WIN" =>
                        observation.HomeOdds,

                   "DRAW" =>
                        observation.DrawOdds,

                   "AWAY_WIN" =>
                        observation.AwayOdds,

                   _ =>
                        0m

               };

            if (odds > 1m)
            {
                marketOddsList.Add(odds);
            }

        }

        if (!marketOddsList.Any())
        {
            return new HistoricalMarketEdgeResult
            {
                PredictionId =
                    prediction.Id,

                PredictionType =
                    prediction.PredictionType,

                PredictedOutcome =
                    prediction.PredictedOutcome,

                StrategyConfidence =
                    prediction.Confidence,

                MarketDataStatus =
                    "InvalidMarketData"
            };
        }

        // =========================================
        // MEDIAN MARKET ODDS
        // =========================================

        marketOddsList =
            marketOddsList
                .OrderBy(x => x)
                .ToList();


        decimal marketOdds =
            Median(marketOddsList);

        // =========================================
        // MARKET IMPLIED PROBABILITY
        // =========================================

        decimal marketProbability =
            (1m / marketOdds) * 100m;

        // =========================================
        // RETURN RESULT
        // =========================================

        return new HistoricalMarketEdgeResult
        {
            PredictionId =
                prediction.Id,

            PredictionType =
                prediction.PredictionType,

            PredictedOutcome =
                prediction.PredictedOutcome,

            StrategyConfidence =
                prediction.Confidence,

            MarketImpliedProbability =
                marketProbability,

            MarketOdds =
                marketOdds,

            ProbabilityEdge =
                prediction.Confidence - marketProbability,

            BookmakerCount =
                marketOddsList.Count,

            PredictionCorrect =
                prediction.IsCorrect,

            RealisedProfitLoss =
                prediction.ProfitLoss,

            RealisedROI =
                prediction.Stake > 0m
                    ? (
                        prediction.ProfitLoss /
                        prediction.Stake
                      ) * 100m
                    : null,

            Stake =
                prediction.Stake,

            MarketDataStatus =
                "Valid"

        };
    }

    // =========================================================
    // OVER 2.5
    // =========================================================

    private async Task<HistoricalMarketEdgeResult?>
        AnalyzeOver25Async(Prediction prediction)
    {
        var observations =
            await _db.MatchOdds
                .Where(x =>
                    x.MatchId == prediction.MatchId &&
                    x.RetrievedUtc <= prediction.CreatedUtc &&
                    x.Over25Odds > 1m &&
                    x.Under25Odds > 1m)
                .OrderByDescending(x =>
                    x.RetrievedUtc)
                .ToListAsync();

        if (!observations.Any())
        {
            return new HistoricalMarketEdgeResult
            {
                PredictionId =
                    prediction.Id,

                PredictionType =
                    prediction.PredictionType,

                PredictedOutcome =
                    prediction.PredictedOutcome,

                StrategyConfidence =
                    prediction.Confidence,

                MarketDataStatus =
                    "InsufficientHistoricalData"

            };
        }

        // =========================================
        // LATEST OBSERVATION PER BOOKMAKER
        // =========================================

        var latestPerBookmaker =
            observations
                .GroupBy(x =>
                    x.Bookmaker)
                .Select(group =>
                    group
                        .OrderByDescending(x =>
                            x.RetrievedUtc)
                        .First())
                .ToList();

        if (!latestPerBookmaker.Any())
        {
            return new HistoricalMarketEdgeResult
            {
                PredictionId =
                    prediction.Id,

                PredictionType =
                    prediction.PredictionType,

                PredictedOutcome =
                    prediction.PredictedOutcome,

                StrategyConfidence =
                    prediction.Confidence,

                MarketDataStatus =
                    "InsufficientHistoricalData"
            };
        }

        // =========================================
        // MEDIAN MARKET ODDS
        // =========================================

        var overOdds =
            latestPerBookmaker
                .Select(x =>
                    x.Over25Odds)
                .OrderBy(x => x)
                .ToList();

        var underOdds =
            latestPerBookmaker
                .Select(x =>
                    x.Under25Odds)
                .OrderBy(x => x)
                .ToList();

        decimal medianOverOdds =
            Median(overOdds);
        
        decimal medianUnderOdds =
            Median(underOdds);

        // =========================================
        // SELECT PREDICTED OUTCOME
        // =========================================

        decimal marketOdds;
        
        decimal marketProbability;

        if (prediction.PredictedOutcome == "OVER_2_5")
        {
            marketOdds =
                medianOverOdds;

            marketProbability =
                (1m / medianOverOdds) * 100m;
        }
        else
        {
            marketOdds =
                medianUnderOdds;

            marketProbability =
                (1m / medianUnderOdds) * 100m;
        }


        // =========================================
        // MEDIAN OVERROUND
        // =========================================

        var overrounds =
            latestPerBookmaker
                .Select(x =>
                    (
                        (1m / x.Over25Odds) +
                        (1m / x.Under25Odds) -
                        1m
                     ) * 100m)
                .OrderBy(x => x)
                .ToList();

        decimal medianOverround =
            Median(overrounds);

        return new HistoricalMarketEdgeResult
        {
            PredictionId =
                prediction.Id,

            PredictionType =
                prediction.PredictionType,

            PredictedOutcome =
                prediction.PredictedOutcome,

            StrategyConfidence =
                prediction.Confidence,

            MarketImpliedProbability =
                marketProbability,

            MarketOdds =
                marketOdds,

            ProbabilityEdge =
                prediction.Confidence - marketProbability,

            BookmakerCount =
                latestPerBookmaker.Count,

            MedianOverround =
                medianOverround,

            PredictionCorrect =
                prediction.IsCorrect,

            RealisedProfitLoss =
                prediction.ProfitLoss,

            RealisedROI =
                prediction.Stake > 0m
                    ? (
                        prediction.ProfitLoss /
                        prediction.Stake
                      ) * 100m
                    : null,

            MarketDataStatus =
                "Valid"

        };
    }

    // =========================================================
    // MEDIAN
    // =========================================================

    private static decimal Median(
        List<decimal> values)
    {
        if (!values.Any())
            return 0m;

        int middle =
            values.Count / 2;

        if ((values.Count % 2) == 1)
        {
            return values[middle];
        }

        return
            (
                values[middle - 1] +
                values[middle]
            ) / 2m;
    }
}
