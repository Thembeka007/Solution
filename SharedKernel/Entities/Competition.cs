namespace SharedKernel.Entities;

public class Competition : Base.Entity
{
    public int ExternalCompetitionId { get; set; }

    public string Name { get; set; } = "";

    public string Code { get; set; } = "";

    public string Country { get; set; } = "";

    public List<Match> Matches { get; set; }
        = new();
}
