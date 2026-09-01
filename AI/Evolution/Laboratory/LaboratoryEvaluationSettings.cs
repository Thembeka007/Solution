namespace AI.Evolution.Laboratory;

public class LaboratoryEvaluationSettings
{
    // =========================================
    // MINIMUM DATA
    // =========================================
    
    public int MinimumInSamplePredictions = 25;
    
    public int MinimumValidationPredictions = 25;
    
    public int MinimumOutOfSamplePredictions = 25;
    
    // =========================================
    // PROFITABILITY
    // =========================================
    
    public decimal MinimumValidationROI = 40m;
    
    public decimal MinimumOutOfSampleROI = 40m;
    
    // =========================================
    // ACCURACY
    // =========================================
    
    public decimal MinimumValidationAccuracy = 60m;
    
    public decimal MinimumOutOfSampleAccuracy = 60m;

    // =========================================
    // CONSISTENCY
    // =========================================

    public int MinimumEvaluationWindows = 3;

    public int MinimumProfitableWindows = 2;

    // =========================================
    // RISK
    // =========================================

    /// <summary>    
    /// Maximum acceptable drawdown percentage
    /// for a candidate entering the live population.
    /// </summary>
    public decimal MaximumAllowedDrawdownPercentage { get; set; } = 25m;

    // =========================================
    // HISTORICAL MARKET FRESHNESS
    // =========================================

    public int MaximumMarketObservationAgeMinutes = 15;

}
