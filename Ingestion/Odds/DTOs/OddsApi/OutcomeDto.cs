namespace Ingestion.Odds.DTOs.OddsApi;

public class OutcomeDto
{
    public string name { get; set; } = "";

    public decimal price { get; set; }

    public decimal? point { get; set; }
}
