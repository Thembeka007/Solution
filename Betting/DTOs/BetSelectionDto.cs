namespace Betting.DTOs;

public class BetSelectionDto
{
    public string HomeTeam { get; set; } = "";

    public string AwayTeam { get; set; } = "";

    public string Prediction { get; set; } = "";

    public decimal Odds { get; set; }

    public decimal Stake { get; set; }

    public decimal Confidence { get; set; }
}
