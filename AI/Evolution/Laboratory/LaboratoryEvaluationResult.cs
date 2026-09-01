using AI.Evolution.Laboratory.Models;

namespace AI.Evolution.Laboratory;

public class LaboratoryEvaluationResult
{
    public string StrategyVersion { get; set; } = string.Empty;
    
    public string StrategyFamily { get; set; } = string.Empty;
    
    // =========================================
    // IN-SAMPLE
    // =========================================
    public int InSamplePredictions { get; set; }
    
    public decimal InSampleProfit { get; set; }
    
    public decimal InSampleROI { get; set; }
    
    public decimal InSampleAccuracy { get; set; }
    
    // =========================================
    // VALIDATION
    // =========================================
    public int ValidationPredictions { get; set; }
    
    public decimal ValidationProfit { get; set; }
    
    public decimal ValidationROI { get; set; }
    
    public decimal ValidationAccuracy { get; set; }

    // =========================================
    // OUT-OF-SAMPLE
    // =========================================    
    public int OutOfSamplePredictions { get; set; }
    
    public decimal OutOfSampleProfit { get; set; }
    
    public decimal OutOfSampleROI { get; set; }
    
    public decimal OutOfSampleAccuracy { get; set; }

    // =========================================
    // CONSISTENCY
    // =========================================    
    public int EvaluationWindows { get; set; }
    
    public int ProfitableEvaluationWindows { get; set; }

    // =========================================
    // RISK
    // =========================================
    public decimal MaximumDrawdown { get; set; }
    
    public decimal MaximumDrawdownPercentage { get; set; }
    
    public bool RiskPassed { get; set; }  

    // =========================================
    // FINAL STATUS
    // =========================================    
    public bool InSamplePassed { get; set; }
    
    public bool ValidationPassed { get; set; }
    
    public bool OutOfSamplePassed { get; set; }
    
    public bool ConsistencyPassed { get; set; }
    
    public bool Passed { get; set; }
    
    public List<string> Failures { get; set; } = new();

    // ==========================================
    // MARKET AUDIT
    // ==========================================

    public LaboratoryMarketDataAudit? InSampleMarketData { get; set; }

    public LaboratoryMarketDataAudit? ValidationMarketData { get; set; }

    public LaboratoryMarketDataAudit? OutOfSampleMarketData { get; set; }

    // ==========================================
    // MARKET EDGE
    // ==========================================

    public decimal InSampleAverageMarketEdge { get; set; }

    public decimal ValidationAverageMarketEdge { get; set; }

    public decimal OutOfSampleAverageMarketEdge { get; set; }
}
