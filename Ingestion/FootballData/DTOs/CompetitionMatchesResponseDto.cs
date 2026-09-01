namespace Ingestion.FootballData.DTOs;

public class CompetitionMatchesResponseDto
{
    public List<MatchDto> Matches { get; set; }
        = new();
}
