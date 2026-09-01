using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ingestion.DTOs;

public class FixtureResponseDto
{
    [JsonPropertyName("response")]
    public List<FixtureItemDto> Response { get; set; }
        = new();

    [JsonPropertyName("errors")]
    public JsonElement Errors { get; set; }

    [JsonPropertyName("results")]
    public int Results { get; set; }
}
