namespace AI.Evolution.Laboratory.Models;

public class LaboratoryMarketDataAudit
{
    public int TotalPredictions { get; set; }

    public int ValidMarketDataPredictions { get; set; }

    public int InsufficientMarketDataPredictions { get; set; }

    public int InvalidMarketDataPredictions { get; set; }

    public decimal MarketDataCoveragePercentage { get; set; }

    public bool SufficientCoverage { get; set; }

    public List<string> Failures { get; set; } = new();

    public int ValidMarketEdgePredictions { get; set; }

    public decimal AverageProbabilityEdge { get; set; }

    public List<MarketEdgeBucketResult> MarketEdgeBuckets { get; set; }
        = new();

    public List<HistoricalMarketEdgeResult> MarketEdgeResults { get; set; }
        = new();
}
