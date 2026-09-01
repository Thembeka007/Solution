namespace Betting.Odds.Models;

public class MarketCoverage
{
    public Guid MatchId { get; set; }

    public int BookmakerCount { get; set; }

    public DateTime? OldestProviderUpdateUtc { get; set; }

    public DateTime? NewestProviderUpdateUtc { get; set; }

    public DateTime? LatestRetrievedUtc { get; set; }
}
