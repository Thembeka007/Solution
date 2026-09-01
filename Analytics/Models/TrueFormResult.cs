namespace Analytics.Models;

public class TrueFormResult
{
    public decimal OverallMomentum { get; set; }

    public decimal TrueMomentum { get; set; }

    public bool OverrideTriggered { get; set; }

    public bool HotStreak { get; set; }

    public bool CollapseDetected { get; set; }

    public decimal ConfidenceAdjustment { get; set; }

}
