using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AI.Evolution;

public class StrategyWeightAnalyzer
{
    private readonly SportsBettingDbContext
        _db;

    private readonly ILogger<StrategyWeightAnalyzer>
        _logger;

    public StrategyWeightAnalyzer(
        SportsBettingDbContext db,
        ILogger<StrategyWeightAnalyzer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<
        StrategyWeightRecommendation>> AnalyzeAsync()
    {
        var recommendations =
            new List<StrategyWeightRecommendation>();

        var strategies =
            await _db.StrategyPerformances
                .Where(x => x.IsActive)
                .ToListAsync();

        foreach (var strategy in strategies)
        {
            if (strategy.ROI < 0)
            {
                recommendations.Add(
                    new StrategyWeightRecommendation
                    {
                        Strategy =
                            strategy.StrategyVersion,

                        Metric =
                            "ROI",

                        CurrentWeight = 1.00m,

                        RecommendedWeight = 0.80m,

                        ImprovementScore =
                            Math.Abs(strategy.ROI),

                        Reason =
                            "Negative ROI"
                    });
            }

            if (strategy.Accuracy < 60)
            {
                recommendations.Add(
                    new StrategyWeightRecommendation
                    {
                        Strategy =
                            strategy.StrategyVersion,

                        Metric =
                            "Accuracy",

                        CurrentWeight = 1.00m,

                        RecommendedWeight = 0.90m,

                        ImprovementScore =
                            60 - strategy.Accuracy,

                        Reason =
                            "Low prediction accuracy"
                    });
            }

            if (strategy.ConfidenceAccuracyGap < -10)
            {
                recommendations.Add(
                    new StrategyWeightRecommendation
                    {
                        Strategy =
                            strategy.StrategyVersion,

                        Metric =
                            "Confidence",

                        CurrentWeight = 1.00m,

                        RecommendedWeight = 0.85m,

                        ImprovementScore =
                            Math.Abs(strategy.ConfidenceAccuracyGap),

                        Reason =
                            "Over-confident predictions"
                    });
            }
        }

        _logger.LogInformation(
            "Generated {count} strategy recommendations",
            recommendations.Count);

        return recommendations;
    }

}
