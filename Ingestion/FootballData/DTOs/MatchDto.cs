namespace Ingestion.FootballData.DTOs;

public class MatchDto
{
    public int Id { get; set; }

    public DateTime UtcDate { get; set; }

    public string? Status { get; set; } = "";

    public int? Matchday { get; set; }

    public string? Stage { get; set; } = "";

    public TeamDto? HomeTeam { get; set; }

    public TeamDto? AwayTeam { get; set; }

    public ScoreDto? Score { get; set; }

    public DateTime? LastUpdated { get; set; }
}
