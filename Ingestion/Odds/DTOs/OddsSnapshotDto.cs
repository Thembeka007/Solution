namespace Ingestion.Odds.DTOs;

public class OddsSnapshotDto
{
    public Guid MatchId { get; set; }

    public string Bookmaker { get; set; }
        = "";

    // =========================================
    // MATCH RESULT
    // =========================================
    public decimal HomeOdds { get; set; }

    public decimal DrawOdds { get; set; }

    public decimal AwayOdds { get; set; }

    // =========================================
    // OVER / UNDER 2.5
    // =========================================
    public decimal Over25Odds { get; set; }

    public decimal Under25Odds { get; set; }

    // =========================================
    // TIMESTAMPS
    // =========================================
    public DateTime ProviderUpdatedUtc { get; set; }
   
    public DateTime RetrievedUtc { get; set; }
}
