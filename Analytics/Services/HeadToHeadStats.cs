namespace Analytics.Services;

public class HeadToHeadStats
{
    public int Matches { get; set; }

    public int BttsCount { get; set; }

    public int Over25Count { get; set; }

    public int HomeWins { get; set; }

    public int AwayWins { get; set; }

    public int Draws { get; set; }

    public int DrawCount { get; set; }

    // ---------- New ----------
    public double AverageGoals { get; set; }

    public double AverageHomeGoals { get; set; }

    public double AverageAwayGoals { get; set; }

    public double AverageGoalDifference { get; set; }
}
