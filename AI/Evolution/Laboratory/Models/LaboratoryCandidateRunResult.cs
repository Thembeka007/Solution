using Analytics.Models;

namespace AI.Evolution.Laboratory.Models;

public class LaboratoryCandidateRunResult
{
    public string StrategyVersion { get; set; } = "";
    
    public string StrategyFamily { get; set; } = "";
    
    public int Generation { get; set; }

    public int MatchesExamined { get; set; }

    public int MatchesSkipped { get; set; }

    public int PredictionsGenerated { get; set; }

    public int ValidMarketDataPredictions { get; set; }

    public int SettledCandidatePredictions { get; set; }

    public int CorrectPredictions { get; set; }

    public decimal Accuracy { get; set; }

    public decimal TotalProfit { get; set; }

    public decimal ROI { get; set; }

    public List<HistoricalCandidatePredictionResult>
        Predictions { get; set; } = new();

    public decimal MaximumDrawdown { get; set; }

    public decimal MaximumDrawdownPercentage { get; set; }

    public bool RiskPassed { get; set; }

    public LaboratoryCandidateDatasetResult InSample 
        { get; set; } = new();
    
    public LaboratoryCandidateDatasetResult Validation 
        { get; set; } = new();
    
    public LaboratoryCandidateDatasetResult OutOfSample
        { get; set; } = new();

    public LaboratoryCandidateGateResult Gate 
        { get; set; } = new();

}
