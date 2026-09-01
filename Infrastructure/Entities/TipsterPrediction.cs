namespace Infrastructure.Entities;

public class TipsterPrediction
{
    public Guid Id { get; set; }

    public Guid TipsterId { get; set; }

    public Tipster? Tipster { get; set; }

    public Guid MatchId { get; set; }

    public string PredictionType { get; set; } = "";

    public string PredictedOutcome { get; set; } = "";

    public decimal Odds { get; set; }

    public bool Won { get; set; }

    public DateTime CreatedUtc { get; set; }
}
