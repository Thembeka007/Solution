namespace Analytics.SmartMoney;

public class TipsterWeightCalculator
{
    public decimal CalculateWeight(
    decimal winRate,
    decimal roi,
    decimal profit,
    int totalPredictions)
    {
        if (totalPredictions < 20)
            return 0;

        decimal weight = 0;

        // =========================
        // WIN RATE
        // =========================

        weight += winRate * 0.4m;

        // =========================
        // ROI
        // =========================

        weight += roi * 0.3m;

        // =========================
        // PROFITABILITY
        // =========================

        weight +=
            Math.Min(profit / 100m, 20m);

        // =========================
        // EXPERIENCE
        // =========================

        weight +=
            Math.Min(totalPredictions / 50m, 10m);

        return Math.Min(weight, 100m);
    }
}
