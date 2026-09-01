namespace Ingestion.DTOs;

public class ExternalMatchDto
{
    public Guid HomeTeamId { get; set; }

    public Guid AwayTeamId { get; set; }

    public DateTime KickoffUtc { get; set; }

    public int? HomeScore { get; set; }

    public int? AwayScore { get; set; }

    public string Status { get; set; } = string.Empty;
}
