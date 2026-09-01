namespace Analytics.Models;

public class StrategyMutationCandidate
{
    public string StrategyVersion { get; set; }
        = "";

    public decimal ROI { get; set; }

    public decimal Accuracy { get; set; }

    public decimal ConfidenceGap { get; set; }

    public bool ReplaceImmediately { get; set; }

    public bool ReduceExposure { get; set; }
}
