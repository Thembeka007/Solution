namespace Analytics.Models;

public class OddsProfile
{
    public decimal HomeOdds { get; set; }

    public decimal DrawOdds { get; set; }

    public decimal AwayOdds { get; set; }

    public decimal HomeProbability { get; set; }

    public decimal DrawProbability { get; set; }

    public decimal AwayProbability { get; set; }

    public decimal FavouriteProbability { get; set; }

    public decimal MarketOverround { get; set; }

    public bool StrongFavourite { get; set; }

    public bool BalancedMarket { get; set; }

    public bool HighVarianceMatch { get; set; }

    public decimal FavouriteEdge { get; set; }
}
