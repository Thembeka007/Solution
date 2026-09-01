namespace AI.Evolution.Risk;

public class StrategyRiskMetrics
{
    public string StrategyVersion { get; set; } = string.Empty;

    public string StrategyFamily { get; set; } = string.Empty;

    public int TotalPredictions { get; set; }

    public decimal TotalProfit { get; set; }

    public decimal ROI { get; set; }

    public decimal MaximumDrawdown { get; set; }

    public decimal MaximumDrawdownPercentage { get; set; }

    public decimal PeakEquity { get; set; }

    public decimal FinalEquity { get; set; }

    public decimal WorstEquity { get; set; }

    public bool HasDrawdownViolation { get; set; }

}
