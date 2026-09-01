using SharedKernel.Entities;

namespace AI.Evolution.Laboratory.Services;

public class LaboratoryConsistencyService
{
    public (
        int EvaluationWindows,
        int ProfitableEvaluationWindows)
        Evaluate(
            IReadOnlyList<Prediction> predictions)
    {
        if (predictions == null ||
            predictions.Count == 0)
        {
            return (0, 0);
        }

        var ordered =
            predictions
                .OrderBy(x =>
                    x.CreatedUtc)
                .ThenBy(x =>
                    x.Id)
                .ToList();

        // =========================================
        // BUILD SEQUENTIAL WINDOWS
        // =========================================
        //
        // Each window represents a fixed number of
        // chronological predictions.
        //
        // The final partial window is retained.
        //
        // We begin with 5 predictions per window.
        //
        // =========================================

        const int WindowSize = 5;

        var windows =
            new List<List<Prediction>>();

        for(int i = 0; i < ordered.Count; i+= WindowSize)
        {
            windows.Add(
                ordered
                    .Skip(i)
                    .Take(WindowSize)
                    .ToList());
        }

        int evaluationWindows = 
            windows.Count;

        int profitableEvaluationWindows =
            windows.Count(
                window =>
                    window.Sum(x =>
                        x.ProfitLoss) > 0m);

        return (evaluationWindows,
            profitableEvaluationWindows);

    }
}
