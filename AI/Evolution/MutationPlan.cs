namespace AI.Evolution;

public class MutationPlan
{
    public string StrategyVersion { get; set; } = "";

    public bool IncreaseConfidenceThreshold { get; set; }

    public bool ReduceConfidenceThreshold { get; set; }

    public bool IncreaseFormWeight { get; set; }

    public bool DecreaseFormWeight { get; set; }

    public bool IncreaseGoalWeight { get; set; }

    public bool DecreaseGoalWeight { get; set; }

    public bool IncreaseHomeAdvantage { get; set; }

    public bool DecreaseHomeAdvantage { get; set; }

    public bool IncreaseH2HWeight { get; set; }

    public bool DecreaseH2HWeight { get; set; }

    public bool ReplaceStrategy { get; set; }

    public decimal ExpectedImprovement { get; set; }

    public string Reason { get; set; } = "";
}
