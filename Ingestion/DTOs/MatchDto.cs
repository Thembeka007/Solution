namespace Ingestion.DTOs;

public class MatchDto
{
    public string HomeTeam { get; set; } = default!;
    public string AwayTeam { get; set; } = default!;

    public DateTime KickoffTime { get; set; }

    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }

    public decimal? HomeXG { get; set; }
    public decimal? AwayXG { get; set; }

    public int? HomeShots { get; set; }
    public int? AwayShots { get; set; }
}
