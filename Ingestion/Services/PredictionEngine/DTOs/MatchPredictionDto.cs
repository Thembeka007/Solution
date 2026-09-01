namespace Ingestion.Services.PredictionEngine.DTOs;

public class MatchPredictionDto
{
    public Guid MatchId { get; set; }

    public string HomeTeam { get; set; } = "";

    public string AwayTeam { get; set; } = "";

    public DateTime KickoffUtc { get; set; }

    public string PredictionType { get; set; } = "";

    public string PredictedOutcome { get; set; } = "";

    public decimal Confidence { get; set; }

    public string StrategyVersion { get; set; } = "";
}
