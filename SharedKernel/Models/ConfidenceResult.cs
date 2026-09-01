namespace SharedKernel.Models;

public class ConfidenceResult
{
    public decimal BaseConfidence { get; set; }

    public decimal FinalConfidence { get; set; }

    public decimal SmartMoneyBoost { get; set; }

    public decimal TrueFormBoost { get; set; }

    public decimal LeagueAdjustment { get; set; }

    public decimal HistoricalAccuracyBoost { get; set; }

    public decimal InjuryPenalty { get; set; }

    public decimal MomentumBoost { get; set; }

    public decimal ConfidenceDecay { get; set; }

    public bool TrapDetected { get; set; }

    public bool IsAnomalyDetected { get; set; }

    public string RiskLevel { get; set; } = "";
}
