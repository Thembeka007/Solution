namespace AI.Evolution;

public class StrategyWeightRecommendation
{
    public string Strategy { get; set; } = "";

    public string Metric { get; set; } = "";

    public decimal CurrentWeight { get; set; }

    public decimal RecommendedWeight { get; set; }

    public decimal ImprovementScore { get; set; }

    public string Reason { get; set; } = "";

}
