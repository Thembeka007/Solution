namespace Betting.DTOs;

public class DailyPickDto
{
    public Guid MatchId { get; set; }

    public Guid PredictionId { get; set; }

    public string HomeTeam { get; set; } = "";

    public string AwayTeam { get; set; } = "";

    public DateTime KickoffUtc { get; set; }

    public string PredictionType { get; set; }
        = "";

    public string PredictedOutcome { get; set; }
        = "";

    public decimal Confidence { get; set; }

    public decimal Odds { get; set; }

    public string Category { get; set; } = "";

    public decimal Stake { get; set; }

    public string DisplayPrediction =>
        $"{PredictionType} - {PredictedOutcome}";

    public decimal ExpectedValue { get; set; }

    public decimal KellyPercentage { get; set; }

    public decimal RecommendedStake { get; set; }

    public decimal MarketProbability { get; set; }

    public decimal EdgePercentage { get; set; }

    public decimal FairOdds { get; set; }

    public string StrategyVersion { get; set; } = "";

    public decimal Score { get; set; }

}
