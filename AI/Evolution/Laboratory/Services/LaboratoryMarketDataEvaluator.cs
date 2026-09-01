using AI.Evolution.Laboratory.Models;
using SharedKernel.Entities;

namespace AI.Evolution.Laboratory.Services;

public class LaboratoryMarketDataEvaluator
{
    private readonly HistoricalPredictionMarketAnalyzer
        _historicalPredictionMarketAnalyzer;

    private readonly HistoricalMarketEdgeAnalyzer
        _marketEdgeAnalyzer;

    private readonly MarketEdgeOutcomeAnalyzer
        _marketEdgeOutcomeAnalyzer;

    public LaboratoryMarketDataEvaluator(
        HistoricalPredictionMarketAnalyzer
            historicalPredictionMarketAnalyzer,
        HistoricalMarketEdgeAnalyzer marketEdgeAnalyzer,
        MarketEdgeOutcomeAnalyzer marketEdgeOutcomeAnalyzer)
    {
        _historicalPredictionMarketAnalyzer =
            historicalPredictionMarketAnalyzer;

        _marketEdgeAnalyzer =
            marketEdgeAnalyzer;

        _marketEdgeOutcomeAnalyzer =
            marketEdgeOutcomeAnalyzer;
    }

    public async Task<LaboratoryMarketDataAudit> 
        EvaluateAsync(
            IEnumerable<Prediction> predictions)
    {
        var data =
            predictions
                .ToList();

        var probabilityEdges =
            new List<decimal>();

        var marketEdgeResults =
            new List<HistoricalMarketEdgeResult>();

        var audit =
            new LaboratoryMarketDataAudit
            {
                TotalPredictions =
                    data.Count
            };

        foreach (var prediction in data)
        {
            var result =
                await _historicalPredictionMarketAnalyzer
                    .AnalyzeAsync(prediction.Id);

            if (result == null)
            {
                audit.InvalidMarketDataPredictions++;

                audit.Failures.Add(
                    $"Prediction {prediction.Id} " +
                        "could not be reconstructed.");

                continue;
            }

            var edge =
                await _marketEdgeAnalyzer
                    .AnalyzeAsync(prediction);

            if (edge != null)
            {
                marketEdgeResults.Add(
                    edge);

                if (edge.MarketDataStatus == "Valid")
                {
                    audit.ValidMarketEdgePredictions++;

                    probabilityEdges.Add(
                        edge.ProbabilityEdge);
                }
            }


            switch (result.MarketDataStatus)
            {
                case HistoricalMarketDataStatus.Valid:
                    audit.ValidMarketDataPredictions++;
                    break;

                case HistoricalMarketDataStatus.InsufficientHistoricalData:
                    audit.InsufficientMarketDataPredictions++;
                    break;

                case HistoricalMarketDataStatus.InvalidData:
                    audit.InvalidMarketDataPredictions++;
                    audit.Failures.Add(
                        $"Prediction {prediction.Id} " +
                        "contains invalid market data.");
                    break;
            }
        }

        if (audit.TotalPredictions > 0)
        {
            audit.MarketDataCoveragePercentage =
                (
                    (decimal)audit.ValidMarketDataPredictions /
                    audit.TotalPredictions
                ) * 100m;
        }

        // =========================================
        // IMPORTANT
        // =========================================
        //
        // This is currently descriptive only.
        //
        // We are NOT making it a promotion gate yet.
        //
        // =========================================

        audit.SufficientCoverage =
            audit.MarketDataCoveragePercentage >= 80m;

        if (probabilityEdges.Any())
        {
            audit.AverageProbabilityEdge =
                probabilityEdges.Average();
        }

        audit.MarketEdgeBuckets =
            _marketEdgeOutcomeAnalyzer
                .Analyze(marketEdgeResults);

        audit.MarketEdgeResults =
            marketEdgeResults;

        return audit;
    }

}
