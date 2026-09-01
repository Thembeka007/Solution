namespace Ingestion.DTOs;

public class TeamsDto
{
    public TeamDto Home { get; set; }
    = new();

    public TeamDto Away { get; set; }
        = new();
}
