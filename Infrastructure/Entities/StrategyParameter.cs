namespace Infrastructure.Entities;

public class StrategyParameter
{
    public Guid Id { get; set; }

    public string StrategyVersion { get; set; } = "";

    public string ParameterName { get; set; } = "";

    public decimal Value { get; set; }

    public DateTime LastUpdatedUtc { get; set; }
}
