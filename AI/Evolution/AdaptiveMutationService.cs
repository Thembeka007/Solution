using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AI.Evolution;

public class AdaptiveMutationService
{
    private readonly SportsBettingDbContext _db;

    private readonly ILogger<AdaptiveMutationService>
        _logger;

    public AdaptiveMutationService(
        SportsBettingDbContext db,
        ILogger<AdaptiveMutationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<decimal> GetMutationRateAsync(
        string strategyVersion)
    {
        var performance =
            await _db.StrategyPerformances
                .FirstOrDefaultAsync(x =>
                    x.StrategyVersion ==
                    strategyVersion);

        if (performance == null)
        {
            _logger.LogWarning(
                """
                Strategy performance not found

                Strategy={strategy}

                Using default mutation rate
                """,
                strategyVersion);

            return 0.15m;
        }

        // =======================================
        // CALCULATE FITNESS
        // =======================================

        decimal fitness =
            (performance.ROI * 0.55m)
            +
            (performance.Accuracy * 0.30m)
            +
            (
                (100m -
                    Math.Abs(
                        performance.ConfidenceAccuracyGap))
                * 0.15m);

        // =======================================
        // BASE MUTATION RATE
        // =======================================

        decimal mutationRate;

        if (fitness < -50m)
        {
            mutationRate = 0.25m;
        }
        else if (fitness < 0m)
        {
            mutationRate = 0.18m;
        }
        else if (fitness < 20m)
        {
            mutationRate = 0.12m;
        }
        else if (fitness < 40m)
        {
            mutationRate = 0.08m;
        }
        else
        {
            mutationRate = 0.04m;
        }

        // =======================================
        // YOUNG STRATEGIES MUTATE MORE
        // =======================================

        if (performance.Generation <= 2)
        {
            mutationRate += 0.03m;
        }

        // =======================================
        // MATURE STRATEGIES MUTATE LESS
        // =======================================

        if (performance.Generation >= 8)
        {
            mutationRate -= 0.02m;
        }

        // =======================================
        // PRESERVE ELITE STRATEGIES
        // =======================================

        if (performance.IsElite)
        {
            mutationRate *= 0.50m;
        }

        // =======================================
        // EXPLORE MORE WITH IMMATURE STRATEGIES
        // =======================================

        if (!performance.IsMature)
        {
            mutationRate += 0.02m;
        }

        // =======================================
        // CLAMP LIMITS
        // =======================================

        mutationRate =
            Math.Clamp(
                mutationRate,
                0.02m,
                0.30m);

        _logger.LogInformation(
            """
            ========================================

            Adaptive Mutation

            Strategy={strategy}

            ROI={roi:F2}

            Accuracy={accuracy:F2}

            Fitness={fitness:F2}

            Generation={generation}

            Mature={mature}

            Elite={elite}

            MutationRate={rate:P0}

            ========================================
            """,
            strategyVersion,
            performance.ROI,
            performance.Accuracy,
            fitness,
            performance.Generation,
            performance.IsMature,
            performance.IsElite,
            mutationRate);

        return mutationRate;
    }
}
