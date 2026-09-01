using SharedKernel.Base;

namespace SharedKernel.Entities;

public class PerformanceSnapshot : AuditableEntity
{
    public DateTime SnapshotDateUtc { get; set; }

    // =========================================
    // FINANCIALS
    // =========================================

    public decimal OpeningBalance { get; set; }

    public decimal ClosingBalance { get; set; }

    public decimal ProfitLoss { get; set; }

    public decimal ROI { get; set; }

    // =========================================
    // BETTING STATS
    // =========================================

    public int TotalSlips { get; set; }

    public int WonSlips { get; set; }

    public int LostSlips { get; set; }

    public decimal WinRate { get; set; }

    // =========================================
    // STREAKS
    // =========================================

    public int WinningStreak { get; set; }

    public int LosingStreak { get; set; }

    // =========================================
    // CATEGORY PERFORMANCE
    // =========================================

    public decimal BreadROI { get; set; }

    public decimal AverageROI { get; set; }

    public decimal LotteryROI { get; set; }

    // =========================================
    // AI PERFORMANCE
    // =========================================

    public decimal AverageConfidence { get; set; }

    public decimal ConfidenceAccuracy { get; set; }

    public decimal SmartMoneyAccuracy { get; set; }

    public decimal TrueFormAccuracy { get; set; }

    // =========================================
    // SYSTEM HEALTH
    // =========================================

    public bool StrategyReplacementTriggered { get; set; }

    public string ActiveStrategyVersion { get; set; }
        = "";
}
