namespace Betting.Odds.Models;

public class MarketDispersionMetrics
{
    public Guid MatchId { get; set; }
    
    // =========================================
    // MARKET COVERAGE
    // =========================================
    public int BookmakerCount { get; set; }
    
    // =========================================
    // HOME ODDS
    // =========================================    
    public decimal HomeOddsMinimum { get; set; }
    
    public decimal HomeOddsMaximum { get; set; }
    
    public decimal HomeOddsMean { get; set; }
    
    public decimal HomeOddsMedian { get; set; }
    
    public decimal HomeOddsRange { get; set; }
    
    // =========================================
    // DRAW ODDS
    // =========================================
    public decimal DrawOddsMinimum { get; set; }
    
    public decimal DrawOddsMaximum { get; set; }
    
    public decimal DrawOddsMean { get; set; }
    
    public decimal DrawOddsMedian { get; set; }
    
    public decimal DrawOddsRange { get; set; }
    
    // =========================================
    // AWAY ODDS
    // =========================================
    public decimal AwayOddsMinimum { get; set; }
    
    public decimal AwayOddsMaximum { get; set; }
    
    public decimal AwayOddsMean { get; set; }
    
    public decimal AwayOddsMedian { get; set; }
    
    public decimal AwayOddsRange { get; set; }
    
    // =========================================
    // IMPLIED PROBABILITIES
    // =========================================
    public decimal HomeProbabilityMinimum { get; set; }
    
    public decimal HomeProbabilityMaximum { get; set; }
    
    public decimal HomeProbabilityMean { get; set; }
    
    public decimal HomeProbabilityMedian { get; set; }
    
    public decimal HomeProbabilityRange { get; set; }

    public decimal DrawProbabilityMinimum { get; set; }
    
    public decimal DrawProbabilityMaximum { get; set; }
    
    public decimal DrawProbabilityMean { get; set; }
    
    public decimal DrawProbabilityMedian { get; set; }
    
    public decimal DrawProbabilityRange { get; set; }
    
    public decimal AwayProbabilityMinimum { get; set; }
    
    public decimal AwayProbabilityMaximum { get; set; }
    
    public decimal AwayProbabilityMean { get; set; }
    
    public decimal AwayProbabilityMedian { get; set; }
    
    public decimal AwayProbabilityRange { get; set; }
    
    // =========================================
    // OVERROUND
    // =========================================
    public decimal OverroundMinimum { get; set; }
    
    public decimal OverroundMaximum { get; set; }
    
    public decimal OverroundMean { get; set; }
    
    public decimal OverroundMedian { get; set; }
    
    public decimal OverroundRange { get; set; }
    
    // =========================================
    // TIMING
    // =========================================
    public DateTime? OldestProviderUpdatedUtc { get; set; }
    
    public DateTime? NewestProviderUpdatedUtc { get; set; }
    
    public decimal ProviderUpdateSpreadSeconds { get; set; }
    
    public DateTime? OldestRetrievedUtc { get; set; }
    
    public DateTime? NewestRetrievedUtc { get; set; }
    
    public decimal RetrievalSpreadSeconds { get; set; }

}
