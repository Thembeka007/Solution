using SharedKernel.Base;
using SharedKernel.Enums;

namespace SharedKernel.Entities;

public class Match : AuditableEntity
{    
    public Guid HomeTeamId { get; set; }

    public Guid AwayTeamId { get; set; }

    public DateTime KickoffUtc { get; set; }

    public int? HomeScore { get; set; }

    public int? AwayScore { get; set; }

    public MatchStatus Status { get; set; }

    public Team? HomeTeam { get; set; } = null!;

    public Team? AwayTeam { get; set; } = null!;

    public int ExternalFixtureId { get; set; }

    public string LeagueName { get; set; } = "";

    public Guid? CompetitionId { get; set; }

    public Competition? Competition { get; set; }

    public List<MatchOdds> Odds { get; set; }
        = new();
}
