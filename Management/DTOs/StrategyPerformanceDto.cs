namespace Management.DTOs;

public class StrategyPerformanceDto
{
    public string StrategyVersion { get; set; }
        = "";

    public decimal ROI { get; set; }

    public decimal Accuracy { get; set; }

    public decimal AverageConfidence { get; set; }

    public decimal ConfidenceGap { get; set; }

    public bool IsActive { get; set; }

    public bool ReplacementTriggered { get; set; }

    public DateTime LastEvaluationUtc { get; set; }
}
