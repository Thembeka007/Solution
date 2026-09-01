namespace AI.Evolution.Laboratory.Models;

public class LaboratoryStrategyCandidate
{
    // =========================================
    // IDENTITY
    // =========================================

    public Guid CandidateId { get; set; }

    public string StrategyVersion { get; set; } = string.Empty;

    public string StrategyFamily { get; set; } = string.Empty;

    // =========================================
    // REFERENCE STRATEGY
    // =========================================
    //
    // This is the strategy from which the candidate
    // was derived.
    //
    // It is a reference only.
    //
    // The reference strategy is NEVER modified.
    // =========================================

    public string ReferenceStrategyVersion { get; set; } = string.Empty;

    // =========================================
    // GENERATION
    // =========================================

    public int Generation { get; set; }

    public string GenerationMethod { get; set; } = string.Empty;

    // Examples:
    //
    // MUTATION
    // CROSSOVER
    // PARAMETER_SEARCH
    // RANDOM_EXPLORATION
    // HYBRID

    // =========================================
    // CANDIDATE STATUS
    // =========================================

    public string Status { get; set; } =
        "CREATED";

    // CREATED
    // BACKTESTING
    // EVALUATED
    // REJECTED
    // PROMOTION_READY
    // PROMOTED

    // =========================================
    // MUTATION DESCRIPTION
    // =========================================

    public List<string> Changes { get; set; } =
        new();

    // =========================================
    // CREATION TIME
    // =========================================

    public DateTime CreatedUtc { get; set; }
        = DateTime.UtcNow;
}
