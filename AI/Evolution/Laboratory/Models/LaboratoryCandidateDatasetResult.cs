namespace AI.Evolution.Laboratory.Models;

public class LaboratoryCandidateDatasetResult
{
    public int Predictions { get; set; }

    public int CorrectPredictions { get; set; }

    public decimal Accuracy { get; set; }

    public decimal TotalProfit { get; set; }

    public decimal ROI { get; set; }

    public int EvaluationWindows { get; set; }

    public int ProfitableEvaluationWindows { get; set; }

    public decimal MaximumDrawdown { get; set; }

    public decimal MaximumDrawdownPercentage { get; set; }

    public bool RiskPassed { get; set; }

    public decimal AverageMarketEdge { get; set; }

    public LaboratoryMarketDataAudit? MarketData { get; set; }

}
