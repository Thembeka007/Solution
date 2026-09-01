namespace AI.Optimization;

public class AnomalyDetectionService
{
    public bool IsAnomaly(
        decimal currentOdds,
        decimal averageOdds)
    {
        var difference =
            Math.Abs(
                currentOdds -
                averageOdds);

        return difference >
               averageOdds * 0.3m;
    }
}
