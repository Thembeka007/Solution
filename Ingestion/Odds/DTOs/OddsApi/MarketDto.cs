namespace Ingestion.Odds.DTOs.OddsApi;

public class MarketDto
{
    public string key { get; set; } = "";

    public DateTime last_update { get; set; }

    public List<OutcomeDto> outcomes { get; set; }
        = new();
}
