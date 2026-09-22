namespace Analytics.Models;

public class MomentumModelResult
{
    public bool ShouldGeneratePrediction { get; set; }

    public decimal HomeScore { get; set; }

    public decimal AwayScore { get; set; }

    public decimal Edge { get; set; }

    public decimal EdgePercent { get; set; }

    public decimal Confidence { get; set; }

    public string PredictedOutcome { get; set; } = "";
}
