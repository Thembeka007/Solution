namespace Management.DTOs;

public class PerformanceSnapshotDto
{
    public DateTime SnapshotDateUtc { get; set; }

    public decimal OpeningBalance { get; set; }

    public decimal ClosingBalance { get; set; }

    public decimal ProfitLoss { get; set; }

    public decimal ROI { get; set; }

    public decimal WinRate { get; set; }

    public int TotalSlips { get; set; }

    public int WonSlips { get; set; }

    public int LostSlips { get; set; }

    public int WinningStreak { get; set; }

    public decimal BreadROI { get; set; }

    public decimal AverageROI { get; set; }

    public decimal LotteryROI { get; set; }

    public decimal ConfidenceAccuracy { get; set; }

    public bool StrategyReplacementTriggered { get; set; }

    public string ActiveStrategyVersion { get; set; }
        = "";
}
