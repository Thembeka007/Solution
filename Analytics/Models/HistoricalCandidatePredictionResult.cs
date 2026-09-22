namespace Analytics.Models;

public class HistoricalCandidatePredictionResult
{
    public string StrategyVersion { get; set; } = "";

    public Guid MatchId { get; set; }

    public DateTime KickoffUtc { get; set; }

    public bool ShouldGeneratePrediction { get; set; }

    public string PredictedOutcome { get; set; } = "";

    public decimal Confidence { get; set; }

    public decimal HomeScore { get; set; }

    public decimal AwayScore { get; set; }

    public decimal Edge { get; set; }

    public decimal EdgePercent { get; set; }

    // ==========================================
    // HISTORICAL MARKET DATA
    // ==========================================
    public bool MarketDataAvailable { get; set; }

    public string MarketDataStatus { get; set; } = "";

    public decimal MarketOdds { get; set; }

    public decimal MarketImpliedProbability { get; set; }

    public DateTime? MarketRetrievedUtc { get; set; }

    public string? MarketBookmaker { get; set; }

    // ==========================================
    // HISTORICAL SETTLEMENT
    // ==========================================

    public bool PredictionCorrect { get; set; }

    public decimal RealisedProfitLoss { get; set; }

    public decimal RealisedROI { get; set; }

}
