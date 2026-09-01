namespace AI.DTOs;

public class EvolutionStatisticsDto
{
    public int TotalStrategies { get; set; }

    public int ActiveStrategies { get; set; }

    public int RetiredStrategies { get; set; }

    public string BestStrategy { get; set; } = "";

    public decimal BestROI { get; set; }

    public decimal AverageROI { get; set; }

    public decimal AverageAccuracy { get; set; }

    public decimal AverageFitness { get; set; }

    public int MutationCount { get; set; }

    public int SuccessfulMutations { get; set; }

    public decimal MutationSuccessRate { get; set; }

}
