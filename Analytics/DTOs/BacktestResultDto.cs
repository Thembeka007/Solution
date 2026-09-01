namespace Analytics.DTOs;

public class BacktestResultDto
{
    public string Strategy { get; set; }
        = "";

    public int Bets { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public decimal WinRate { get; set; }

    public decimal ROI { get; set; }

    public decimal Profit { get; set; }
}
