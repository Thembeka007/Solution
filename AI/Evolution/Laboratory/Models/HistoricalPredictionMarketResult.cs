namespace AI.Evolution.Laboratory.Models;

public class HistoricalPredictionMarketResult
{
    public Guid PredictionId { get; set; }

    public Guid MatchId { get; set; }

    public string StrategyVersion { get; set; } = "";
    
    public string PredictionType { get; set; } = "";
    
    public string PredictedOutcome { get; set; } = "";
    
    public DateTime PredictionCreatedUtc { get; set; }

    public HistoricalMarketCondition? MarketCondition { get; set; }

    public HistoricalTotalsMarketCondition? TotalsMarketCondition { get; set; }

    public HistoricalMarketDataStatus MarketDataStatus { get; set; }
}
