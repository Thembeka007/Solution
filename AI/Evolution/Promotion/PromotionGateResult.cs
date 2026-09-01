namespace AI.Evolution.Promotion;

public class PromotionGateResult
{
    public string StrategyVersion { get; set; } = string.Empty;
    
    public bool Passed { get; set; }
    
    public bool MaturityPassed { get; set; }
    
    public bool ProfitabilityPassed { get; set; }
    
    public bool MinimumProfitPassed { get; set; }
    
    public bool AccuracyPassed { get; set; }
    
    public bool MaximumDrawdownPassed { get; set; }
    
    public bool OutOfSamplePassed { get; set; }
    
    public bool ConsistencyPassed { get; set; }
    
    public bool CapitalRiskPassed { get; set; }
    
    public List<string> Failures { get; set; } = new();
    
    public List<string> PassedGates { get; set; } = new();
}
