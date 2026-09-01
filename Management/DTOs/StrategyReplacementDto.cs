namespace Management.DTOs;

public class StrategyReplacementDto
{
    public string OldStrategyVersion { get; set; }
        = "";

    public string NewStrategyVersion { get; set; }
        = "";

    public decimal OldStrategyROI { get; set; }

    public decimal ReplacementROI { get; set; }

    public DateTime ReplacementDateUtc { get; set; }

    public bool Activated { get; set; }
}
