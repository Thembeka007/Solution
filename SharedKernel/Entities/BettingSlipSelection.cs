using SharedKernel.Base;

namespace SharedKernel.Entities;

public class BettingSlipSelection : AuditableEntity
{
    // =========================================
    // RELATIONSHIPS
    // =========================================

    public Guid BettingSlipId { get; set; }

    public BettingSlip BettingSlip { get; set; }
        = null!;

    public Guid MatchId { get; set; }

    public Match Match { get; set; }
        = null!;

    // =========================================
    // PREDICTION DATA
    // =========================================

    public string PredictionType { get; set; }
        = "";

    public string PredictedOutcome { get; set; }
        = "";

    // =========================================
    // BETTING DATA
    // =========================================

    public decimal Odds { get; set; }

    public decimal OpeningOdds { get; set; }

    public decimal? ClosingOdds { get; set; }

    public decimal? ClosingLineValue { get; set; }

    public decimal Confidence { get; set; }

    // =========================================
    // RESULT TRACKING
    // =========================================

    public bool? Won { get; set; }

    public bool Settled { get; set; }

    public decimal Stake { get; set; }

    public DateTime? SettledUtc { get; set; }

    // =========================================
    // ACTUAL MATCH RESULT
    // =========================================

    public int? HomeScore { get; set; }

    public int? AwayScore { get; set; }

    // =========================================
    // PERFORMANCE ANALYTICS
    // =========================================

    public decimal ProfitLoss { get; set; }

    public bool CashoutTriggered { get; set; }

    public decimal CashoutAmount { get; set; }

    // =========================================
    // AI TRACKING
    // =========================================

    public decimal SmartMoneyStrength { get; set; }

    public decimal TrueFormStrength { get; set; }

    public decimal FinalModelConfidence { get; set; }

    public string StrategyVersion { get; set; }
        = "";

    // =========================================
    // FAILURE ANALYSIS
    // =========================================

    public bool TrapDetected { get; set; }

    public string FailureReason { get; set; }
        = "";

    // =========================================
    // CATEGORY
    // =========================================

    public string BettingCategory { get; set; }
        = "";
}
