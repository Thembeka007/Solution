using SharedKernel.Base;

namespace SharedKernel.Entities;

public class StrategyPerformance
    : AuditableEntity
{
    // =====================================
    // STRATEGY
    // =====================================
    public string StrategyVersion { get; set; }
        = "";

    // =====================================
    // PREDICTION STATS
    // =====================================
    public int TotalPredictions { get; set; }

    public int CorrectPredictions { get; set; }

    public int IncorrectPredictions { get; set; }

    public decimal Accuracy { get; set; }

    // =====================================
    // CONFIDENCE TRACKING
    // =====================================
    public decimal AverageConfidence { get; set; }

    public decimal ConfidenceAccuracyGap { get; set; }    

    // =====================================
    // FINANCIALS
    // =====================================
    public decimal Profit { get; set; }

    public decimal ROI { get; set; }

    // =====================================
    // EVOLUTION
    // =====================================
    public decimal FitnessScore { get; set; }

    public bool IsMature { get; set; }

    public bool IsElite { get; set; }

    // =====================================
    // HEALTH
    // =====================================
    public bool IsActive { get; set; }
        = true;

    public bool ReplacementTriggered { get; set; }

    public DateTime LastEvaluationUtc { get; set; }

    // ======================================
    // STRATEGY MUTATION
    // ======================================
    public string StrategyFamily { get; set; } = "";

    public int Generation { get; set; }

    public Guid? ParentStrategyId { get; set; }

    public Guid? SecondParentStrategyId { get; set; }

    // ======================================
    // EVOLUTION LABORATORY
    // ======================================
    public bool LaboratoryPassed { get; set; }

    public DateTime? LaboratoryEvaluatedUtc { get; set; }

}
