using System.Text.Json.Serialization;

namespace Ingestion.DTOs;

public class FixtureItemDto
{
    [JsonPropertyName("fixture")]
    public FixtureInfoDto Fixture { get; set; } = new();

    [JsonPropertyName("teams")]
    public TeamsDto Teams { get; set; } = new();

    [JsonPropertyName("goals")]
    public GoalsDto Goals { get; set; } = new();

    [JsonPropertyName("league")]
    public LeagueDto League { get; set; } = new();
}
