using AI.Evolution.Laboratory.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Evolution.Laboratory.Services;

public class HistoricalPredictionMarketAnalyzer
{
    private readonly SportsBettingDbContext _db;

    private readonly HistoricalMarketConditionAnalyzer
        _marketConditionAnalyzer;

    private readonly HistoricalTotalsMarketConditionAnalyzer
        _totalsMarketConditionAnalyzer;

    public HistoricalPredictionMarketAnalyzer(
        SportsBettingDbContext db,
        HistoricalMarketConditionAnalyzer marketConditionAnalyzer,
        HistoricalTotalsMarketConditionAnalyzer totalsMarketConditionAnalyzer)
    {
        _db = db;

        _marketConditionAnalyzer =
            marketConditionAnalyzer;

        _totalsMarketConditionAnalyzer =
            totalsMarketConditionAnalyzer;
    }

    public async Task<HistoricalPredictionMarketResult?>
        AnalyzeAsync(
        Guid predictionId)
    {
        // =========================================
        // LOAD PREDICTION
        // =========================================

        var prediction =
            await _db.Predictions
                .FirstOrDefaultAsync(x =>
                    x.Id == predictionId);

        if (prediction == null)
            return null;

        if (prediction.MatchId == Guid.Empty)
            return null;

        // =========================================
        // DECISION TIME
        // =========================================
        //
        // The market must be reconstructed using
        // information available when the prediction
        // was actually created.
        //
        // =========================================

        var asOfUtc =
            prediction.CreatedUtc;

        // =========================================
        // RECONSTRUCT MARKET
        // =========================================

        HistoricalMarketCondition?
            marketCondition = null;

        HistoricalTotalsMarketCondition?
            totalsMarketCondition = null;

        var marketDataStatus =
            HistoricalMarketDataStatus
                .InsufficientHistoricalData;

        if (prediction.PredictionType == "OVER25")
        {
            totalsMarketCondition =
                await _totalsMarketConditionAnalyzer
                    .AnalyzeAsync(
                        prediction.MatchId,
                        asOfUtc);

            if (totalsMarketCondition != null)
            {
                marketDataStatus =
                    HistoricalMarketDataStatus.Valid;
            }
        }
        else
        {
            marketCondition =
                await _marketConditionAnalyzer
                    .AnalyzeAsync(
                        prediction.MatchId,
                        asOfUtc);

            if (marketCondition != null)
            {
                marketDataStatus =
                    HistoricalMarketDataStatus.Valid;
            }
        }

        // =========================================
        // BUILD RESULT
        // =========================================

        return new HistoricalPredictionMarketResult
        {
            PredictionId =
                prediction.Id,
            
            MatchId =
                prediction.MatchId,
                
            StrategyVersion =
                prediction.StrategyVersion,
            
            PredictionType =
                prediction.PredictionType,
            
            PredictedOutcome =
                prediction.PredictedOutcome,
            
            PredictionCreatedUtc =
                prediction.CreatedUtc,
            
            MarketCondition =
                marketCondition,

            TotalsMarketCondition =
                totalsMarketCondition,

            MarketDataStatus =
                marketDataStatus

        };

    }

    public async Task<List<HistoricalPredictionMarketResult>>
        AnalyzeStrategyAsync(string strategyVersion)
    {
        // =========================================
        // LOAD HISTORICAL PREDICTIONS
        // =========================================

        var predictions =
            await _db.Predictions
                .Where(x =>
                    x.StrategyVersion == strategyVersion &&
                    x.IsSettled)
                .OrderBy(x =>
                    x.CreatedUtc)
                .ToListAsync();

        var results =
            new List<HistoricalPredictionMarketResult>();

        // =========================================
        // RECONSTRUCT EACH DECISION POINT
        // =========================================

        foreach (var prediction in predictions)
        {
            HistoricalMarketCondition?
                marketCondition = null;

            HistoricalTotalsMarketCondition?
                totalsMarketCondition = null;

            if (prediction.PredictionType ==
                "OVER25")
            {
                totalsMarketCondition =
                    await _totalsMarketConditionAnalyzer
                        .AnalyzeAsync(
                            prediction.MatchId,
                            prediction.CreatedUtc);
            }
            else
            {
                marketCondition =
                    await _marketConditionAnalyzer
                        .AnalyzeAsync(
                            prediction.MatchId,
                            prediction.CreatedUtc);
            }

            results.Add(
                new HistoricalPredictionMarketResult
                {
                    PredictionId =
                        prediction.Id,
                    
                    MatchId =
                        prediction.MatchId,
                    
                    StrategyVersion =
                        prediction.StrategyVersion,
                    
                    PredictionType =
                        prediction.PredictionType,
                    
                    PredictedOutcome =
                        prediction.PredictedOutcome,
                    
                    PredictionCreatedUtc =
                        prediction.CreatedUtc,
                    
                    MarketCondition =
                        marketCondition,

                    TotalsMarketCondition =
                        totalsMarketCondition
                });

        }

        return results;
    }
}
