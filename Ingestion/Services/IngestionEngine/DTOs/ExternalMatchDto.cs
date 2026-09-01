namespace Ingestion.Services.IngestionEngine.DTOs;

public class ExternalMatchDto
{
    public string HomeTeam { get; set; } = "";

    public string AwayTeam { get; set; } = "";

    public DateTime KickoffUtc { get; set; }

    public int? HomeScore { get; set; }

    public int? AwayScore { get; set; }

    public string Status { get; set; } = "";
}
