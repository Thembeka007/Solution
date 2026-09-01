using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AI.Evolution;

public class FitnessEvaluationService
{
    private readonly SportsBettingDbContext _db;

    private readonly ILogger<FitnessEvaluationService>
        _logger;

    public FitnessEvaluationService(
        SportsBettingDbContext db,
        ILogger<FitnessEvaluationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<decimal> CalculateFitnessAsync(
        string strategyVersion)
    {
        var performance =
            await _db.StrategyPerformances
                .FirstOrDefaultAsync(x =>
                    x.StrategyVersion ==
                    strategyVersion);

        if (performance == null)
            return 0m;

        decimal roi =
            performance.ROI;

        decimal accuracy =
            performance.Accuracy;

        // -------------------------
        // Normalize ROI
        // -------------------------

        decimal roiScore =
            Math.Clamp(
                (roi + 100m) / 200m,
                0m,
                1m);

        // -------------------------
        // Normalize Accuracy
        // -------------------------

        decimal accuracyScore =
            Math.Clamp(
                accuracy / 100m,
                0m,
                1m);

        // -------------------------
        // Weighted Fitness
        // -------------------------

        decimal fitness =
            (roiScore * 0.60m)
            +
            (accuracyScore * 0.40m);

        // ==========================================
        // NEW
        // Persist fitness
        // ==========================================

        performance.FitnessScore =
            fitness;

        performance.LastEvaluationUtc =
            DateTime.UtcNow;

        _logger.LogInformation(
            """
            Fitness Evaluation

            Strategy={strategy}

            ROI={roi:F2}

            Accuracy={accuracy:F2}

            Fitness={fitness:F3}
            """,
            strategyVersion,
            roi,
            accuracy,
            fitness);

        return fitness;
    }
}
