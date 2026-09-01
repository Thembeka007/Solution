using SharedKernel.Base;

namespace SharedKernel.Entities;

public class TeamForm : Entity
{
    public Guid TeamId { get; set; }

    public string TeamName { get; set; } = "";

    public Guid MatchId { get; set; }

    public DateTime MatchDateUtc { get; set; }

    public int GoalsFor { get; set; }

    public int GoalsAgainst { get; set; }

    public bool BTTS { get; set; }

    public bool Over25 { get; set; }

    public int Points { get; set; }

    public bool IsHomeGame { get; set; }

    public double XG { get; set; }

    public double OpponentXG { get; set; }

}