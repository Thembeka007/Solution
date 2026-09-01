namespace AI.Features;

public class MatchFeatureVector
{
    public Guid MatchId { get; set; }

    // FORM
    public decimal HomeWinRate { get; set; }

    public decimal AwayWinRate { get; set; }

    public decimal HomeBTTSRate { get; set; }

    public decimal AwayBTTSRate { get; set; }

    public decimal HomeOver25Rate { get; set; }

    public decimal AwayOver25Rate { get; set; }

    // GOALS
    public decimal HomeGoalsScoredAvg { get; set; }

    public decimal AwayGoalsScoredAvg { get; set; }

    public decimal HomeGoalsConcededAvg { get; set; }

    public decimal AwayGoalsConcededAvg { get; set; }

    // MOMENTUM
    public decimal HomeMomentum { get; set; }

    public decimal AwayMomentum { get; set; }

    // SMART MONEY
    public decimal SmartMoneyStrength { get; set; }

    // MARKET
    public decimal HomeOdds { get; set; }

    public decimal DrawOdds { get; set; }

    public decimal AwayOdds { get; set; }

    // TARGETS
    public bool BTTSWon { get; set; }

    public bool Over25Won { get; set; }

    public bool HomeWon { get; set; }
}
