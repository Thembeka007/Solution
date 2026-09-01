namespace Ingestion.DTOs;

public class FixtureInfoDto
{
    public int Id { get; set; }

    public DateTimeOffset Date { get; set; }

    public FixtureStatusDto Status { get; set; }
        = new();
}
