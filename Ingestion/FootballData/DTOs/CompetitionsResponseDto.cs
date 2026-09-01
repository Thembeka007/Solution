namespace Ingestion.FootballData.DTOs;

public class CompetitionsResponseDto
{
    public int Count { get; set; }

    public List<CompetitionItemDto> Competitions
    { get; set; } = new();
}
