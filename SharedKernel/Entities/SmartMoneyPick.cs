namespace SharedKernel.Entities;

public class SmartMoneyPick
{
    public Guid Id { get; set; }

    public Guid TipsterId { get; set; }

    public SmartMoneyTipster? Tipster { get; set; }

    public string HomeTeam { get; set; } = "";

    public string AwayTeam { get; set; } = "";

    public string PredictionType { get; set; } = "";

    public string PredictionOutcome { get; set; } = "";

    public decimal Odds { get; set; }

    public DateTime MatchDateUtc { get; set; }

    public DateTime CreatedUtc { get; set; }
}
