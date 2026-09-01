namespace Ingestion.FootballData.DTOs;

public class CompetitionItemDto
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Code { get; set; } = "";

    public AreaDto Area { get; set; }
        = new();
}
