namespace Management.DTOs;

public class BettingSlipResponseDto
{
    public Guid Id { get; set; }

    public string Category { get; set; }
        = "";

    public decimal Stake { get; set; }

    public decimal EstimatedOdds { get; set; }

    public decimal PotentialPayout { get; set; }

    public bool Settled { get; set; }

    public bool Won { get; set; }

    public DateTime CreatedDateUtc { get; set; }

    public List<BettingSlipSelectionDto>
        Picks { get; set; } = new();
}
