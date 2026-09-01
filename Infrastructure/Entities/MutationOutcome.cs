namespace Infrastructure.Entities;

public class MutationOutcome
{
    public Guid Id { get; set; }

    public string StrategyVersion { get; set; } = "";

    public string ParameterName { get; set; } = "";

    public decimal OldValue { get; set; }

    public decimal NewValue { get; set; }

    public decimal PreviousROI { get; set; }

    public decimal NewROI { get; set; }

    public decimal Improvement { get; set; }

    public bool Successful { get; set; }

    public DateTime MutationUtc { get; set; }

    public DateTime? EvaluatedUtc { get; set; }

    public string Reason { get; set; } = "";
}
