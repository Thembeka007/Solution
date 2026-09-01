namespace AI.Optimization;

public class DynamicWeightingEngine
{
    public Dictionary<string, decimal>
    CalculateWeights(
        decimal currentWinRate)
    {
        var weights =
            new Dictionary<string, decimal>();

        // =========================
        // DEFAULT
        // =========================

        weights["trueform"] = 0.5m;
        weights["statistics"] = 0.5m;

        // =========================
        // ADAPTIVE LOGIC
        // =========================

        if (currentWinRate < 50)
        {
            weights["trueform"] = 0.7m;
            weights["statistics"] = 0.3m;
        }

        if (currentWinRate > 75)
        {
            weights["trueform"] = 0.4m;
            weights["statistics"] = 0.6m;
        }

        return weights;
    }
}
