using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;


namespace Analytics.Services;

public class StrategyPerformanceService
{
    private readonly SportsBettingDbContext _db;

    private readonly ILogger<StrategyPerformanceService>
        _logger;

    private const int MinPredictionsForMaturity = 25;

    public StrategyPerformanceService(
        SportsBettingDbContext db,
        ILogger<StrategyPerformanceService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task AnalyzeAsync()
    {
        // =====================================
        // LOAD SETTLED PREDICTIONS
        // =====================================

        var predictions =
            await _db.Predictions
                .Where(x =>
                    x.IsSettled &&
                    !string.IsNullOrWhiteSpace(
                        x.StrategyVersion))
                .ToListAsync();

        // =====================================
        // GROUP BY STRATEGY
        // =====================================

        var grouped =
            predictions
                .GroupBy(x =>
                    x.StrategyVersion);

        foreach (var group in grouped)
        {
            string strategy =
                group.Key;

            int total =
                group.Count();

            int correct =
                group.Count(x =>
                    x.IsCorrect == true);

            int incorrect =
                total - correct;

            // =================================
            // ACCURACY
            // =================================

            decimal accuracy =
                total > 0
                    ? ((decimal)correct / total) * 100m
                    : 0m;            

            // =================================
            // AVERAGE CONFIDENCE
            // =================================

            decimal avgConfidence =
                group.Average(x =>
                    x.Confidence);

            decimal confidenceGap =
                accuracy -
                avgConfidence;

            // =================================
            // REAL - ROI 
            // =================================

            decimal totalStake =
                group.Sum(x =>
                    x.Stake);

            decimal profit =
                group.Sum(x =>
                    x.ProfitLoss);

            decimal roi =
                totalStake > 0
                    ? (profit / totalStake) * 100m
                    : 0m;

            // =================================
            // FIND EXISTING RECORD
            // =================================

            var existing =
                await _db
                    .StrategyPerformances
                    .FirstOrDefaultAsync(x =>
                        x.StrategyVersion ==
                        strategy);

            if (existing is null)
            {
                existing =
                    new StrategyPerformance
                    {
                        StrategyVersion =
                            strategy,

                        CreatedAtUtc =
                            DateTime.UtcNow,

                        CreatedUtc =
                            DateTime.UtcNow,

                        LastEvaluationUtc =
                            DateTime.UtcNow
                    };

                _db.StrategyPerformances
                    .Add(existing);
            }

            // =================================
            // CORE METADATA
            // =================================

            existing.StrategyFamily =
                GetFamily(strategy);

            // =================================
            // MATURITY
            // =================================

            existing.IsMature =
                total >= MinPredictionsForMaturity;

            // =================================
            // UPDATE METRICS
            // =================================

            existing.TotalPredictions =
                total;

            existing.CorrectPredictions =
                correct;

            existing.IncorrectPredictions =
                incorrect;

            existing.Accuracy =
                accuracy;

            existing.AverageConfidence =
                avgConfidence;

            existing.ConfidenceAccuracyGap =
                confidenceGap;

            existing.Profit =
                profit;

            existing.ROI =
                roi;

            existing.LastEvaluationUtc =
                DateTime.UtcNow;

            // =================================
            // STRATEGY HEALTH
            // =================================

            _logger.LogInformation(
                """
                Strategy Performance evaluation

                Strategy={strategy}

                Family={family}

                TotalPredictions={predictions}

                Correct={correct}
                
                Incorrect={incorrect}

                Accuracy={accuracy:F2}

                AverageConfidence={confidence:F2}
                
                ConfidenceAccuracyGap={gap:F2}

                Profit={profit:F2}

                ROI={roi:F2}                

                Mature={mature}

                """,
                strategy,
                existing.StrategyFamily,
                total,
                correct,
                incorrect,
                accuracy,
                avgConfidence,
                confidenceGap,
                profit,
                roi,
                existing.IsMature);
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Strategy performance analysis completed.");
    }
    private static string GetFamily(string strategyVersion)
    {
        var index =
            strategyVersion.IndexOf(
                "_MUTATED_",
                StringComparison.OrdinalIgnoreCase);

        if (index < 0)
            return strategyVersion;

        return strategyVersion[..index];
    }
}
