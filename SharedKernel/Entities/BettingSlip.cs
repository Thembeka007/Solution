using SharedKernel.Base;

namespace SharedKernel.Entities;

public class BettingSlip : AuditableEntity
{
    public string Category { get; set; } = "";

    public decimal Stake { get; set; }

    public decimal EstimatedOdds { get; set; }

    public decimal PotentialPayout { get; set; }

    public decimal ActualPayout { get; set; }

    public decimal Profit { get; set; }

    public bool? Won { get; set; }

    public bool Settled { get; set; }

    public bool IsCompleted { get; set; }

    public string StrategyVersion { get; set; }
        = "";

    public DateTime CreatedDateUtc { get; set; }

    public List<BettingSlipSelection>
        Selections { get; set; } = new();
}
