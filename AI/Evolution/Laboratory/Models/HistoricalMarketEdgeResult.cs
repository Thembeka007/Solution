namespace AI.Evolution.Laboratory.Models;

public class HistoricalMarketEdgeResult
{
    public Guid PredictionId { get; set; }

    public string PredictionType { get; set; } = "";
    
    public string PredictedOutcome { get; set; } = "";

    // =========================================
    // STRATEGY
    // =========================================
    public decimal StrategyConfidence { get; set; }

    public decimal Stake { get; set; }

    // =========================================
    // MARKET
    // =========================================
    public decimal MarketImpliedProbability { get; set; }

    public decimal MarketOdds { get; set; }

    // =========================================
    // EDGE
    // =========================================
    public decimal ProbabilityEdge { get; set; }

    // =========================================
    // MARKET DATA
    // =========================================
    public int BookmakerCount { get; set; }

    public decimal MedianOverround { get; set; }

    public string MarketDataStatus { get; set; } = "";

    // =========================================
    // ACTUAL RESULTS
    // =========================================
    public bool? PredictionCorrect { get; set; }

    public decimal? RealisedProfitLoss { get; set; }

    public decimal? RealisedROI { get; set; }
}
