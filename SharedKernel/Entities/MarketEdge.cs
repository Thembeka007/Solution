namespace SharedKernel.Entities;

public class MarketEdge
{
    public Guid Id { get; set; }

    public Guid PredictionId { get; set; }

    public Prediction Prediction { get; set; }
        = null!;

    public decimal ModelProbability { get; set; }

    public decimal MarketProbability { get; set; }

    public decimal EdgePercentage { get; set; }

    public decimal FairOdds { get; set; }

    public decimal MarketOdds { get; set; }

    public DateTime CreatedUtc { get; set; }
}
