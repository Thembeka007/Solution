using Analytics.Models;

namespace AI.Evolution.Laboratory.Services;

public class LaboratoryCandidateDatasetSplitter
{
    public (
        List<HistoricalCandidatePredictionResult> InSample,
        List<HistoricalCandidatePredictionResult> Validation,
        List<HistoricalCandidatePredictionResult> OutOfSample)
            Split(
                IReadOnlyList<HistoricalCandidatePredictionResult> predictions)
    {
        if (predictions == null ||
            predictions.Count == 0)
        {
            return (
                new List<HistoricalCandidatePredictionResult>(),
                new List<HistoricalCandidatePredictionResult>(),
                new List<HistoricalCandidatePredictionResult>());
        }

        // =========================================
        // CHRONOLOGICAL ORDER
        // =========================================

        var ordered = 
            predictions
                .Where(x =>
                    x.ShouldGeneratePrediction &&
                    x.MarketDataAvailable)
                .OrderBy(x =>
                    x.KickoffUtc)
                .ThenBy(x =>
                    x.MatchId)
                .ToList();

        if (ordered.Count == 0)
        {
            return (
                new List<HistoricalCandidatePredictionResult>(),
                new List<HistoricalCandidatePredictionResult>(),
                new List<HistoricalCandidatePredictionResult>());
        }

        // =========================================
        // DATASET SIZES
        // =========================================
        //
        // 60% = In-Sample
        // 20% = Validation
        // 20% = Out-of-Sample
        //
        // The split is chronological.
        //
        // =========================================

        int total =
            ordered.Count;

        int inSampleCount =
            (int)Math.Floor(total * 0.60m);

        int validationCount =
            (int)Math.Floor(total * 0.20m);

        int outOfSampleCount =
            total - inSampleCount - validationCount;

        // =========================================
        // SAFETY
        // =========================================
        //
        // Do not manufacture datasets.
        //
        // If there are fewer than 3 predictions,
        // there is no meaningful three-way split.
        //
        // =========================================

        if (total < 3)
        {
            return (
                new List<HistoricalCandidatePredictionResult>(),
                new List<HistoricalCandidatePredictionResult>(),
                new List<HistoricalCandidatePredictionResult>());
        }

        var inSample =
            ordered
                .Take(inSampleCount)
                .ToList();

        var validation =
            ordered
                .Skip(inSampleCount)
                .Take(validationCount)
                .ToList();

        var outOfSample =
            ordered
                .Skip(inSampleCount + validationCount)
                .Take(outOfSampleCount)
                .ToList();

        return (
            inSample,
            validation,
            outOfSample);

    }
}
