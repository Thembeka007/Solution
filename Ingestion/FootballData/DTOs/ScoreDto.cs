namespace Ingestion.FootballData.DTOs;

public class ScoreDto
{
    public string? Winner { get; set; } = "";
    public FullTimeDto? FullTime { get; set; }
}
