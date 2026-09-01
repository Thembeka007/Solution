namespace Management.DTOs;

public class PredictionResponseDto
{
    public Guid Id { get; set; }

    public Guid MatchId { get; set; }

    public string HomeTeam { get; set; }
        = "";

    public string AwayTeam { get; set; }
        = "";

    public DateTime KickoffUtc { get; set; }

    public string PredictionType { get; set; }
        = "";

    public string PredictedOutcome { get; set; }
        = "";

    public decimal Confidence { get; set; }

    public string StrategyVersion { get; set; }
        = "";

    public bool IsSettled { get; set; }

    public bool? IsCorrect { get; set; }

    public decimal ProfitLoss { get; set; }

    public decimal ClosingOdds { get; set; }
}
