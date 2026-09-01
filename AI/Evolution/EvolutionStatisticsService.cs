using AI.DTOs;
using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AI.Evolution;

public class EvolutionStatisticsService
{
    private readonly SportsBettingDbContext _db;

    private readonly ILogger<EvolutionStatisticsService>
        _logger;

    public EvolutionStatisticsService(
        SportsBettingDbContext db,
        ILogger<EvolutionStatisticsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<EvolutionStatisticsDto>
        GetStatisticsAsync()
    {
        var strategies =
            await _db.StrategyPerformances
                .ToListAsync();

        var mutations =
            await _db.Set<MutationOutcome>()
                .Where(x => x.EvaluatedUtc != null)
                .ToListAsync();

        _logger.LogInformation(
            """
            =========================================

            Evolution Statistics

            Total={total}

            Active={active}

            Retired={retired}

            Best={best}

            BestROI={roi:F2}

            AvgROI={avgRoi:F2}

            AvgAccuracy={avgAcc:F2}

            AvgFitness={avgFitness:F2}

            MutationSuccess={success:P}

            """,
            strategies.Count,
            strategies.Count(x => x.IsActive),
            strategies.Count(x => !x.IsActive),
            strategies.OrderByDescending(x => x.FitnessScore)
                      .Select(x => x.StrategyVersion)
                      .FirstOrDefault(),
            strategies.Any() ? strategies.Max(x => x.ROI) : 0,
            strategies.Any() ? strategies.Average(x => x.ROI) : 0,
            strategies.Any() ? strategies.Average(x => x.Accuracy) : 0,
            strategies.Any() ? strategies.Average(x => x.FitnessScore) : 0,
            mutations.Any()
                ? (decimal)mutations.Count(x => x.Successful) / mutations.Count
                : 0);

        return new EvolutionStatisticsDto
        {
            TotalStrategies =
                strategies.Count,

            ActiveStrategies =
                strategies.Count(x => x.IsActive),

            RetiredStrategies =
                strategies.Count(x => !x.IsActive),

            BestStrategy =
                strategies
                    .OrderByDescending(x => x.FitnessScore)
                    .Select(x => x.StrategyVersion)
                    .FirstOrDefault() ?? "",

            BestROI =
                strategies.Any()
                    ? strategies.Max(x => x.ROI)
                    : 0,

            AverageROI =
                strategies.Any()
                    ? strategies.Average(x => x.ROI)
                    : 0,

            AverageAccuracy =
                strategies.Any()
                    ? strategies.Average(x => x.Accuracy)
                    : 0,

            AverageFitness =
                strategies.Any()
                    ? strategies.Average(x => x.FitnessScore)
                    : 0,

            MutationCount =
                mutations.Count,

            SuccessfulMutations =
                mutations.Count(x => x.Successful),

            MutationSuccessRate =
                mutations.Any()
                    ? (decimal)mutations.Count(x => x.Successful)
                    / mutations.Count
                    : 0

        };

    }
}
