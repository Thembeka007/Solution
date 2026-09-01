namespace Ingestion.DTOs;

public class LeagueDto
{
    public int Id { get; set; }

    public string Name { get; set; }
        = string.Empty;

    public string Country { get; set; }
        = string.Empty;

    public int Season { get; set; }

    public string Round { get; set; }
        = string.Empty;
}
