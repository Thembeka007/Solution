namespace Betting.Odds.Models;

public class BookmakerMarketAnalytics
{
    public Guid MatchId { get; set; }

    public string Bookmaker { get; set; } = "";
    
    // =========================================
    // RAW MARKET ODDS
    // =========================================    
    public decimal HomeOdds { get; set; }
    
    public decimal DrawOdds { get; set; }
    
    public decimal AwayOdds { get; set; }
    
    // =========================================
    // DERIVED IMPLIED PROBABILITIES
    // =========================================
    public decimal HomeImpliedProbability { get; set; }
    
    public decimal DrawImpliedProbability { get; set; }
    
    public decimal AwayImpliedProbability { get; set; }
    
    // =========================================
    // MARKET OVERROUND
    // =========================================    
    public decimal Overround { get; set; }
    
    // =========================================
    // DATA TIMESTAMPS
    // =========================================    
    public DateTime ProviderUpdatedUtc { get; set; }
    
    public DateTime RetrievedUtc { get; set; }

}
