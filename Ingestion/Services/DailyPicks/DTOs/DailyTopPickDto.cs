namespace Ingestion.Services.DailyPicks.DTOs;

public class DailyTopPickDto
{
    public Guid MatchId { get; set; }

    public string HomeTeam { get; set; } = "";

    public string AwayTeam { get; set; } = "";

    public string Prediction { get; set; } = "";

    public decimal Confidence { get; set; }

    public DateTime KickoffUtc { get; set; }
}
