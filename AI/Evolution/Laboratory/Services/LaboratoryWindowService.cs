using AI.Evolution.Laboratory.Models;
using SharedKernel.Entities;

namespace AI.Evolution.Laboratory.Services;

public class LaboratoryWindowService
{
    public LaboratoryEvaluationWindows Build(
        IReadOnlyList<Prediction> predictions)
    {
        if (predictions == null ||
            predictions.Count < 3)
        {
            throw new ArgumentException(
                "At least 3 predictions are required " +
                "to build laboratory evaluation windows.",
                nameof(predictions));
        }

        var ordered =
            predictions
                .OrderBy(x =>
                    x.CreatedUtc)
                .ThenBy(x =>
                    x.Id)
                .ToList();

        int total =
            ordered.Count;

        // =========================================
        // CHRONOLOGICAL CUT POINTS
        // =========================================

        int validationStartIndex =
            (int)Math.Floor(
                total * 0.60);

        int outOfSampleStartIndex =
            (int)Math.Floor(
                total * 0.80);

        // =========================================
        // GUARANTEE NON-EMPTY DATASETS
        // =========================================

        validationStartIndex =
            Math.Clamp(
                validationStartIndex,
                1,
                total - 2);

        outOfSampleStartIndex =
            Math.Clamp(
                outOfSampleStartIndex,
                validationStartIndex + 1,
                total - 1);

        return new LaboratoryEvaluationWindows
        {
            // =====================================
            // IN-SAMPLE
            // =====================================

            InSampleStartIndex =
                0,

            InSampleEndIndex =
                validationStartIndex,

            InSampleStartUtc =
                ordered
                    .First()
                    .CreatedUtc,

            InSampleEndUtc =
                ordered[validationStartIndex]
                    .CreatedUtc,

            // =====================================
            // VALIDATION
            // =====================================

            ValidationStartIndex =
                validationStartIndex,

            ValidationEndIndex =
                outOfSampleStartIndex,

            ValidationStartUtc =
                ordered[validationStartIndex]
                    .CreatedUtc,

            ValidationEndUtc =
                ordered[outOfSampleStartIndex]
                    .CreatedUtc,

            // =====================================
            // OUT-OF-SAMPLE
            // =====================================

            OutOfSampleStartIndex =
                outOfSampleStartIndex,

            OutOfSampleEndIndex =
                total,

            OutOfSampleStartUtc =
                ordered[outOfSampleStartIndex]
                    .CreatedUtc,

            OutOfSampleEndUtc =
                ordered
                    .Last()
                    .CreatedUtc
        };
    }
}
