namespace Infrastructure.Entities;

public class BacktestResult
{
    public Guid Id { get; set; }

    public string StrategyName { get; set; } = "";

    public decimal WinRate { get; set; }

    public decimal ROI { get; set; }

    public decimal Profit { get; set; }

    public int TotalBets { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public decimal MaxDrawdown { get; set; }

    public decimal SharpeRatio { get; set; }

    public DateTime TestedFromUtc { get; set; }

    public DateTime TestedToUtc { get; set; }

    public DateTime CreatedUtc { get; set; }
}
