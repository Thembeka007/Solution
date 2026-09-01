namespace Betting.DTOs;

public class BettingSlipDto
{
    public string Category { get; set; } = "";

    public List<DailyPickDto> Picks { get; set; }
        = new();

    public decimal EstimatedOdds { get; set; }

    public decimal Stake { get; set; }
}
