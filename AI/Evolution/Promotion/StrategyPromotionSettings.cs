namespace AI.Evolution.Promotion;

public class StrategyPromotionSettings
{
    // =========================================
    // MATURITY
    // =========================================
    
    public int MinimumPredictions = 25;
    
    // =========================================
    // PROFITABILITY
    // =========================================
    
    public decimal MinimumROI = 40m;
    
    public decimal MinimumProfit = 5m;
    
    // =========================================
    // ACCURACY
    // =========================================
        
    public decimal MinimumAccuracy = 60m;
    
    // =========================================
    // RISK
    // =========================================
    
    public decimal MaximumDrawdownPercentage = 20m;
    
    // =========================================
    // CAPITAL
    // =========================================
    
    public decimal MaximumCapitalExposure = 25m;
}
