namespace Management.DTOs;

public class DailyPerformanceReportDto
{
    public decimal OpeningBalance { get; set; }

    public decimal CurrentBalance { get; set; }

    public decimal DailyProfit { get; set; }

    public decimal ROI { get; set; }

    public int TotalBets { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public decimal WinRate { get; set; }

    public int LongestWinStreak { get; set; }

    public decimal BiggestWin { get; set; }

    public decimal BiggestLoss { get; set; }
}
