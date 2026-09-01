namespace SharedKernel.Entities;

public class PlayerInjury
{
    public Guid Id { get; set; }

    public string PlayerName { get; set; } = "";

    public string TeamName { get; set; } = "";

    public decimal PlayerRating { get; set; }

    public bool IsSuspended { get; set; }

    public bool IsInjured { get; set; }

    public DateTime LastUpdatedUtc { get; set; }
}
