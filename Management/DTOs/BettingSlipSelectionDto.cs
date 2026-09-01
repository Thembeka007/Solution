namespace Management.DTOs;

public class BettingSlipSelectionDto
{
    public Guid MatchId { get; set; }

    public string HomeTeam { get; set; }
        = "";

    public string AwayTeam { get; set; }
        = "";

    public string PredictionType { get; set; }
        = "";

    public string PredictedOutcome { get; set; }
        = "";

    public decimal Confidence { get; set; }

    public decimal Odds { get; set; }

    public bool Settled { get; set; }

    public bool Won { get; set; }
}
