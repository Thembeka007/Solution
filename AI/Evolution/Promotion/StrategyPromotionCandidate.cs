using AI.Evolution.Risk;
using SharedKernel.Entities;

namespace AI.Evolution.Promotion;

public class StrategyPromotionCandidate
{
    public StrategyPerformance Performance { get; set; } = null!;

    public StrategyRiskMetrics RiskMetrics { get; set; } = null!;
    
    // =========================================
    // OUT OF SAMPLE
    // =========================================

    public decimal OutOfSampleROI { get; set; }

    public decimal OutOfSampleProfit { get; set; }

    public int OutOfSamplePredictions { get; set; }
    
    // =========================================
    // CONSISTENCY
    // =========================================
    
    public int ProfitableEvaluationWindows { get; set; }
    
    public int EvaluationWindows { get; set; }
    
    // =========================================
    // CAPITAL
    // =========================================
    
    public decimal EstimatedCapitalExposure { get; set; }

}
