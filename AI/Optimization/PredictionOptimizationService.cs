using SharedKernel.Entities;

namespace AI.Optimization;

public class PredictionOptimizationService
{
    public decimal OptimizeConfidence(
        Prediction prediction,
        Dictionary<string, decimal> features,
        Dictionary<string, decimal> weights)
    {
        decimal confidence = 0;

        // =========================
        // TRUEFORM
        // =========================

        confidence +=
            features["home_trueform"]
            * weights["trueform"]
            * 10;

        confidence +=
            features["away_trueform"]
            * weights["trueform"]
            * 10;

        // =========================
        // BTTS SIGNAL
        // =========================

        confidence +=
            features["home_btts"]
            * weights["statistics"];

        confidence +=
            features["away_btts"]
            * weights["statistics"];

        return Math.Min(confidence, 99);
    }
}
