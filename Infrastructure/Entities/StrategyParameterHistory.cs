namespace Infrastructure.Entities;

public class StrategyParameterHistory
{
    public Guid Id { get; set; }

    public string StrategyVersion { get; set; } = "";

    public string ParameterName { get; set; } = "";

    public decimal OldValue { get; set; }

    public decimal NewValue { get; set; }

    public string Reason { get; set; } = "";

    public decimal ExpectedImprovement { get; set; }

    public DateTime ChangedUtc { get; set; }
}
