using SharedKernel.Base;

namespace SharedKernel.Entities;

public class TeamExternalMapping : Entity
{
    public Guid TeamId { get; set; }

    public string Source { get; set; } = "";
    
    public string? ExternalTeamId { get; set; } = "";
    
    public string ExternalName { get; set; } = "";
    
    public DateTime FirstSeenUtc { get; set; }

    public DateTime LastSeenUtc { get; set; }

    public bool IsActive { get; set; } = true;
}
