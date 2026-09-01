namespace AI.Evolution.Laboratory.Models;

public class HistoricalMarketCondition
{
    public Guid MatchId { get; set; }

    public DateTime KickoffUtc { get; set; }

    public string LeagueName { get; set; } = "";
    
    // =========================================
    // MARKET COVERAGE
    // =========================================
    public int BookmakerCount { get; set; }

    public int MaximumObservationAgeMinutes { get; set; }

    public int FreshObservationCount { get; set; }

    // =========================================
    // HOME MARKET
    // =========================================
    public decimal MinimumHomeOdds { get; set; }
    
    public decimal MaximumHomeOdds { get; set; }
    
    public decimal MedianHomeOdds { get; set; }
    
    public decimal MeanHomeOdds { get; set; }
    
    public decimal HomeOddsRange { get; set; }
    
    // =========================================
    // DRAW MARKET
    // =========================================   
    public decimal MinimumDrawOdds { get; set; }

    public decimal MaximumDrawOdds { get; set; }

    public decimal MedianDrawOdds { get; set; }

    public decimal MeanDrawOdds { get; set; }

    public decimal DrawOddsRange { get; set; }
    
    // =========================================
    // AWAY MARKET
    // =========================================
    public decimal MinimumAwayOdds { get; set; }

    public decimal MaximumAwayOdds { get; set; }
    
    public decimal MedianAwayOdds { get; set; }
    
    public decimal MeanAwayOdds { get; set; }
    
    public decimal AwayOddsRange { get; set; }
    
    // =========================================
    // IMPLIED PROBABILITIES
    // =========================================
    public decimal MedianHomeImpliedProbability { get; set; }
    
    public decimal MedianDrawImpliedProbability { get; set; }
    
    public decimal MedianAwayImpliedProbability { get; set; }
    
    // =========================================
    // BOOKMAKER MARGIN
    // =========================================
    public decimal MinimumOverround { get; set; }
    
    public decimal MaximumOverround { get; set; }
    
    public decimal MedianOverround { get; set; }
    
    public decimal MeanOverround { get; set; }
    
    // =========================================
    // MARKET DISPERSION
    // / =========================================
    public decimal HomeProbabilityRange { get; set; }

    public decimal DrawProbabilityRange { get; set; }
    
    public decimal AwayProbabilityRange { get; set; }
    
    public decimal AverageProbabilityRange { get; set; }
    
    // =========================================
    // MARKET FRESHNESS
    // =========================================
    public DateTime? EarliestProviderUpdateUtc { get; set; }
    
    public DateTime? LatestProviderUpdateUtc { get; set; }
    
    public double ProviderUpdateSpreadMinutes { get; set; }
    
    // =========================================
    // CLASSIFICATION
    // =========================================
    public string CoverageCondition { get; set; } = "";
    
    public string DispersionCondition { get; set; } = "";
    
    public string OverroundCondition { get; set; } = "";
    
    public string MarketShape { get; set; } = "";

}
