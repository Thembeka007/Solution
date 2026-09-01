namespace Analytics.Services;

public class StrategyAnalyzer
{
    public bool RequiresStrategyChange(
        decimal accuracy)
    {
        return accuracy < 40;
    }

    public bool Stable(
        decimal accuracy)
    {
        return accuracy >= 80;
    }
}
