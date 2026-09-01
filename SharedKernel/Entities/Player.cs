using SharedKernel.Base;

namespace SharedKernel.Entities;

public class Player : Base.Entity
{
    public string Name { get; set; } = "";

    public string TeamName { get; set; } = "";

    public decimal Rating { get; set; }

    public bool IsInjured { get; set; }

    public bool IsSuspended { get; set; }

    public decimal MinutesPlayedRatio { get; set; }

    public decimal TeamDependencyScore { get; set; }
}
