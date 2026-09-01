namespace Ingestion.Services.SmartMoney.DTOs;

public class SmartMoneyConsensusDto
{
    public Guid MatchId { get; set; }

    public string PredictionType { get; set; } = "";

    public string Outcome { get; set; } = "";

    public decimal ConfidenceBoost { get; set; }

    public List<string> Tipsters { get; set; } = new();
}
