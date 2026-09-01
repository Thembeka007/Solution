namespace Ingestion.Services.SmartMoney.DTOs;

public class SmartMoneySignalDto
{
    public string HomeTeam { get; set; } = "";

    public string AwayTeam { get; set; } = "";

    public string PredictionType { get; set; } = "";

    public string PredictionOutcome { get; set; } = "";

    public decimal ConfidenceBoost { get; set; }
}
