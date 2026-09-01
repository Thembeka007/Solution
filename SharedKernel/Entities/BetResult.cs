namespace SharedKernel.Entities;

public class BetResult
{
    public Guid Id { get; set; }

    public Guid PredictionId { get; set; }

    public Prediction? Prediction { get; set; }

    public bool Won { get; set; }

    public decimal Odds { get; set; }

    public decimal Stake { get; set; }

    public decimal Profit { get; set; }

    public DateTime SettledUtc { get; set; }

    public string Category { get; set; } = "";

    public string StrategyVersion { get; set; }
        = "";
}
