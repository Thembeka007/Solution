namespace Ingestion.Odds.DTOs.OddsApi;

public class BookmakerDto
{
    public string key { get; set; } = "";

    public string title { get; set; } = "";

    public DateTime last_update { get; set; }

    public List<MarketDto> markets { get; set; }
        = new();
}
