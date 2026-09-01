namespace Ingestion.Services.SmartMoney.DTOs;

public class TipsterSignalDto
{
    public Guid MatchId { get; set; }

    public string PredictionType { get; set; } = "";

    public string PredictedOutcome { get; set; } = "";

    public decimal CombinedWeight { get; set; }

    public int SmartMoneyCount { get; set; }

    public decimal AverageOdds { get; set; }
}
