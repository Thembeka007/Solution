namespace Infrastructure.Entities;

public class PendingMutation
{
    public Guid Id { get; set; }

    public string StrategyVersion { get; set; } = "";

    public string ParameterName { get; set; } = "";

    public decimal OldValue { get; set; }

    public decimal NewValue { get; set; }

    // ROI when mutation was applied
    public decimal StartingROI { get; set; }

    public DateTime AppliedUtc { get; set; }

    public bool Evaluated { get; set; }

    public DateTime? EvaluatedUtc { get; set; }
}
