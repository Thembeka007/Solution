using AI.Evolution.Laboratory.Models;
using Analytics.Models;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace AI.Evolution.Laboratory.Services;

public class LaboratoryCandidateDatasetEvaluator
{
    private readonly LaboratoryConsistencyService
        _consistencyService;

    private readonly LaboratoryEvaluationSettings
        _evaluationSettings;

    private readonly ILogger<LaboratoryCandidateDatasetEvaluator> _logger;

    public LaboratoryCandidateDatasetEvaluator(
        LaboratoryEvaluationSettings evaluationSettings,
        LaboratoryConsistencyService consistencyService,
        ILogger<LaboratoryCandidateDatasetEvaluator> logger
        )
    {
        _evaluationSettings = evaluationSettings;
        _consistencyService = consistencyService;
        _logger = logger; 
    }

    public LaboratoryCandidateDatasetResult Evaluate(
        IReadOnlyList<HistoricalCandidatePredictionResult> predictions)
    {
        if (predictions == null ||
            predictions.Count == 0)
        {
            return new LaboratoryCandidateDatasetResult();
        }

        // =========================================
        // ONLY SETTLED CANDIDATE PREDICTIONS
        // =========================================
        //
        // A result is usable only when:
        //
        // 1. The candidate generated a prediction.
        // 2. Historical market data was available.
        //
        // This prevents skipped predictions and
        // missing-market-data matches from affecting
        // profitability or accuracy.
        //
        // =========================================

        var settled =
            predictions
                .Where(x =>
                    x.ShouldGeneratePrediction &&
                    x.MarketDataAvailable)
                .OrderBy(x =>
                    x.KickoffUtc)
                .ThenBy(x =>
                    x.MatchId)
                .ToList();

        if (!settled.Any())
        {

            return new LaboratoryCandidateDatasetResult();
        }

        // =========================================
        // BASIC PERFORMANCE
        // =========================================

        int totalPredictions =
            settled.Count;

        int correctPredictions =
            settled.Count(x =>
                x.PredictionCorrect);

        decimal totalProfit =
            settled.Sum(x =>
                x.RealisedProfitLoss);

        decimal roi =
            totalPredictions > 0
                ? (totalProfit / totalPredictions) * 100m
                : 0m;

        decimal accuracy =
            totalPredictions > 0
                ? ((decimal)correctPredictions / totalPredictions) * 100m
                : 0m;

        // ===============================================
        // RISK / EQUITY CURVE
        // ===============================================
        //
        // Laboratory performance is evaluated against
        // a normalized starting bankroll rather than
        // cumulative profit.
        //
        // This prevents a small early profit peak from
        // producing an artificially enormous drawdown %.
        //
        // ===============================================

        decimal startingBankroll =
            _evaluationSettings.StartingBankroll;

        decimal equity =
            startingBankroll;
        
        decimal peakEquity =
            startingBankroll;
        
        decimal maximumDrawdown =
            0m;

        foreach(var prediction in settled)
        {
            equity +=
                prediction.RealisedProfitLoss;

            if (equity > peakEquity)
            {
                peakEquity =
                    equity;
            
            }

            decimal drawdown =
                peakEquity - equity;

            if (drawdown > maximumDrawdown)
            {
                maximumDrawdown =
                    drawdown;
            
            }

        }

        decimal maximumDrawdownPercentage =
            peakEquity > 0m
                ? (maximumDrawdown / peakEquity) * 100m
                : 0m;


        bool riskPassed =
            maximumDrawdownPercentage <=
                _evaluationSettings
                    .MaximumAllowedDrawdownPercentage;


        _logger.LogInformation(
            """"
            ========================================
            
            LABORATORY RISK CALCULATION

            StartingBankroll={startingBankroll:F2}
            
            PeakEquity={peakEquity:F2}
            
            MaximumDrawdown={maximumDrawdown:F2}
            
            CalculatedDrawdownPercentage={drawdownPercentage:F2}
            
            MaximumAllowedDrawdownPercentage={maximumAllowed:F2}
            
            RiskPassed={riskPassed}
            
            ======================================== 
            """",
            _evaluationSettings.StartingBankroll,
            peakEquity,
            maximumDrawdown,
            maximumDrawdownPercentage,
            _evaluationSettings.MaximumAllowedDrawdownPercentage,
            riskPassed);

            


        // =========================================
        // CONSISTENCY
        // =========================================
        //
        // The existing consistency service expects
        // Prediction entities.
        //
        // Candidate historical results are deliberately
        // NOT Prediction entities, so consistency is
        // calculated directly here using the same
        // sequential-window concept.
        //
        // =========================================

        const int windowSize = 5;

        int evaluationWindows =
            (int)Math.Ceiling(
                totalPredictions /
                (decimal)windowSize);

        int profitableEvaluationWindows = 0;

        for (int i = 0; i < settled.Count; i += windowSize)
        {
            decimal windowProfit =
                settled.Skip(i)
                    .Take(windowSize)
                    .Sum(x =>
                        x.RealisedProfitLoss);

            if (windowProfit > 0m)
            {
                profitableEvaluationWindows++;
            }
        }

        // =========================================
        // MARKET EDGE
        // =========================================

        var edgeResults =
            settled
                .Where(x =>
                    x.MarketDataAvailable)
                .ToList();

        decimal averageMarketEdge =
            edgeResults.Any()
                ? edgeResults.Average(x =>
                    x.Edge)
                : 0m;

        // =========================================
        // BUILD RESULT
        // =========================================

        return new LaboratoryCandidateDatasetResult
        {
            Predictions =
                totalPredictions,

            CorrectPredictions =
                correctPredictions,

            Accuracy =
                accuracy,

            TotalProfit =
                totalProfit,

            ROI =
                roi,

            EvaluationWindows =
                evaluationWindows,

            ProfitableEvaluationWindows =
                profitableEvaluationWindows,

            MaximumDrawdown =
                maximumDrawdown,

            MaximumDrawdownPercentage =
                maximumDrawdownPercentage,

            RiskPassed =
                riskPassed,

            AverageMarketEdge =
                averageMarketEdge

        };

    }

}
