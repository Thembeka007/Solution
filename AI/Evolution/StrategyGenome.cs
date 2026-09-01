namespace AI.Evolution;

public class StrategyGenome
{
    public string StrategyVersion { get; set; } = "";

    public Dictionary<string, decimal> Parameters
        = new();
}
