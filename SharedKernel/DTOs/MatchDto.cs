namespace SharedKernel.DTOs;

public class MatchDto
{
    public Guid HomeTeamId { get; set; }

    public Guid AwayTeamId { get; set; }

    public DateTime KickoffUtc { get; set; }

    public int? HomeScore { get; set; }

    public int? AwayScore { get; set; }

    public string Status { get; set; } = string.Empty;

    // Optional enrichment fields (very useful for later phases)
    public Guid? LeagueId { get; set; }

    public string? HomeTeamName { get; set; }

    public string? AwayTeamName { get; set; }
}