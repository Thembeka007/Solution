using SharedKernel.Base;
using SharedKernel.Enums;
using SharedKernel.ValueObjects;

namespace SharedKernel.Entities;

public class Prediction : Base.Entity
{
    public Guid MatchId { get; set; }

    public Match? Match { get; set; }

    public string PredictionType { get; set; } = "";

    public string PredictedOutcome { get; set; } = "";

    public decimal Confidence { get; set; }

    public string StrategyVersion { get; set; } = "";

    public DateTime MatchDateUtc { get; set; }

    public decimal Stake { get; set; }

    // =========================
    // SETTLEMENT
    // =========================

    public bool? IsCorrect { get; set; }

    public bool IsSettled { get; set; }

    public DateTime? SettledUtc { get; set; }

    public decimal ProfitLoss { get; set; }

    public decimal ClosingOdds { get; set; }

}
