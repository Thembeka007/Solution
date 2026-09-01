using SharedKernel.Base;

namespace SharedKernel.Entities;

public class MatchOdds : AuditableEntity
{
    public Guid MatchId { get; set; }

    public Match Match { get; set; }
        = null!;

    public string Bookmaker { get; set; }
        = "";
    // ======================================================
    // MATCH RESULTS ODDS
    // ======================================================
    public decimal HomeOdds { get; set; }

    public decimal DrawOdds { get; set; }

    public decimal AwayOdds { get; set; }

    public DateTime RetrievedUtc { get; set; }

    public DateTime ProviderUpdatedUtc { get; set; }

    public bool IsClosingOdds { get; set; }

    // ======================================================
    // PROBABILITIES  -- not using this section yet, why?
    // ======================================================

    public decimal HomeImpliedProbability { get; set; }

    public decimal DrawImpliedProbability { get; set; }

    public decimal AwayImpliedProbability { get; set; }


    // ======================================================
    // Over/Under Odds
    // ======================================================

    public decimal Over25Odds { get; set; }

    public decimal Under25Odds { get; set; }

    // ======================================================
    // BTTS Odds
    // ======================================================

    public decimal BTTSYesOdds { get; set; }

    public decimal BTTSNoOdds { get; set; }
}
