namespace AI.Optimization;

public class ConfidenceTuningService
{
    public decimal Tune(
        decimal currentConfidence,
        decimal historicalAccuracy)
    {
        if (historicalAccuracy > 80)
        {
            return Math.Min(
                currentConfidence + 5,
                99);
        }

        if (historicalAccuracy < 50)
        {
            return Math.Max(
                currentConfidence - 10,
                40);
        }

        return currentConfidence;
    }
}
