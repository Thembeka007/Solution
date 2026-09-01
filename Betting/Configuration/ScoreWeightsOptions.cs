namespace Betting.Configuration;

public class ScoreWeightsOptions
{
    public decimal ConfidenceWeight { get; set; }
        = 0.40m;

    public decimal EdgeWeight { get; set; }
        = 0.30m;

    public decimal ExpectedValueWeight { get; set; }
        = 0.20m;

    public decimal KellyWeight { get; set; }
        = 0.10m;
}
