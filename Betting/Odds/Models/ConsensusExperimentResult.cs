namespace Betting.Odds.Models;

public class ConsensusExperimentResult
{
    public Guid MatchId { get; set; }

    // =========================================
    // MARKET COVERAGE
    // =========================================
    public int BookmakerCount { get; set; }

    // =========================================
    // METHOD A — MEDIAN RAW ODDS
    // =========================================
    public decimal MedianHomeOdds { get; set; }

    public decimal MedianDrawOdds { get; set; }

    public decimal MedianAwayOdds { get; set; }

    // =========================================
    // METHOD B — MEAN RAW ODDS
    // =========================================
    public decimal MeanHomeOdds { get; set; }

    public decimal MeanDrawOdds { get; set; }

    public decimal MeanAwayOdds { get; set; }

    // =========================================
    // METHOD C — RAW IMPLIED PROBABILITY
    // =========================================
    public decimal MeanHomeImpliedProbability { get; set; }

    public decimal MeanDrawImpliedProbability { get; set; }

    public decimal MeanAwayImpliedProbability { get; set; }

    public decimal MeanOverround { get; set; }

    // =========================================
    // MARKET DISPERSION
    // =========================================
    public decimal HomeOddsRange { get; set; }

    public decimal DrawOddsRange { get; set; }

    public decimal AwayOddsRange { get; set; }

    public decimal HomeProbabilityRange { get; set; }

    public decimal DrawProbabilityRange { get; set; }

    public decimal AwayProbabilityRange { get; set; }

    // =========================================
    // TIMING
    // =========================================
    public decimal ProviderUpdateSpreadSeconds { get; set; }

    public decimal RetrievalSpreadSeconds { get; set; }
}
