using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AI.Evolution;

public class MutationOutcomeRecorder
{
    private readonly SportsBettingDbContext _db;
    
    private readonly ILogger<MutationOutcomeRecorder> _logger;

    public MutationOutcomeRecorder(
        SportsBettingDbContext db,
        ILogger<MutationOutcomeRecorder> logger)
    {
        _db = db;

        _logger = logger;
    }

    public async Task RecordAsync(
        string strategyVersion,
        string parameterName,
        decimal oldValue,
        decimal newValue,
        decimal previousROI,
        decimal currentROI)
    {
        var improvement =
            currentROI - previousROI;

        var outcome =
            new MutationOutcome
            {
                StrategyVersion =
                    strategyVersion,

                ParameterName =
                    parameterName,

                OldValue =
                    oldValue,

                NewValue =
                    newValue,

                PreviousROI =
                    previousROI,

                NewROI =
                    currentROI,

                Improvement =
                    improvement,

                Successful =
                    improvement > 0,

                MutationUtc =
                    DateTime.UtcNow
            };

        _db.Set<MutationOutcome>()
            .Add(outcome);

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            """
            Mutation outcome recorded Strategy={strategy}
            Parameter={parameter}
            PreviousROI={previous}
            CurrentROI={current}
            Improvement={improvement}
            Successful={success} 

            """,
            strategyVersion,
            parameterName,
            previousROI,
            currentROI,
            improvement,
            outcome.Successful);
    }

    public async Task RecordLatestStrategyAsync(
        string strategyVersion)
    {
        var performance =
            await _db.StrategyPerformances
                .FirstOrDefaultAsync(x =>
                    x.StrategyVersion ==
                    strategyVersion);

        if (performance == null)
            return;

        var parameters =
            await _db.StrategyParameters
                .Where(x =>
                    x.StrategyVersion ==
                    strategyVersion)
                    .ToListAsync();

        foreach (var parameter in parameters)
        {
            var previous =
                await _db.Set<MutationOutcome>()
                    .Where(x =>
                        x.StrategyVersion ==
                        strategyVersion &&
                        x.ParameterName ==
                        parameter.ParameterName)
                    .OrderByDescending(x =>
                    x.MutationUtc)
                    .FirstOrDefaultAsync();
            
            decimal previousROI =
                previous?.NewROI ?? performance.ROI;
            
            await RecordAsync(
                strategyVersion,
                parameter.ParameterName,
                parameter.Value,
                parameter.Value,
                previousROI,
                performance.ROI);
        }
    }

}

