namespace AI.Evolution.Laboratory;

public class LaboratoryDatasetMetrics
{
    public int TotalPredictions { get; set; }

    public int CorrectPredictions { get; set; }

    public int IncorrectPredictions { get; set; }

    public decimal Accuracy { get; set; }

    public decimal Profit { get; set; }

    public decimal TotalStake { get; set; }

    public decimal ROI { get; set; }
}
