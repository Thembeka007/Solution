using SharedKernel.Base;

namespace SharedKernel.Entities;

public class StrategyReplacement

    : AuditableEntity
{
    public string OldStrategyVersion { get; set; }
        = "";

    public string NewStrategyVersion { get; set; }
        = "";

    public decimal OldStrategyROI { get; set; }

    public decimal ReplacementROI { get; set; }

    public DateTime ReplacementDateUtc { get; set; }

    public bool Activated { get; set; }
}
