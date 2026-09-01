namespace AI.Evolution.Laboratory.Models;

public class MarketEdgeBucketResult
{
    public string Bucket { get; set; } = "";

    public int Predictions { get; set; }

    public int CorrectPredictions { get; set; }

    public decimal Accuracy { get; set; }

    public decimal TotalStake { get; set; }

    public decimal TotalProfitLoss { get; set; }

    public decimal ROI { get; set; }

    public decimal AverageEdge { get; set; }

    public bool MinimumSampleSizeMet { get; set; }

}
