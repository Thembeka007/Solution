namespace Ingestion.Services.SignalAggregation.DTOs;

public class AggregatedPredictionDto
{
    public Guid MatchId { get; set; }

    public string PredictionType { get; set; } = "";

    public string PredictionOutcome { get; set; } = "";

    public decimal FinalConfidence { get; set; }

    public string StrategyVersion { get; set; } = "";
}
