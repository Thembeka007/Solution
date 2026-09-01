using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AI.Evolution;

public class MutationLearningService
{
    private readonly SportsBettingDbContext _db;
    
    private readonly ILogger<MutationLearningService> _logger;

    public MutationLearningService(
        SportsBettingDbContext db,
        ILogger<MutationLearningService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<decimal> GetLearningMultiplierAsync(
        string strategyVersion,
        string parameterName)
    {
        var history =
            await _db.Set<MutationOutcome>()
                .Where(x => x.StrategyVersion ==
                    strategyVersion && x.ParameterName ==
                    parameterName && x.EvaluatedUtc != null)
                .OrderByDescending(x =>
                    x.MutationUtc)
                .Take(20)
                .ToListAsync();

        if (!history.Any())
        {
            _logger.LogInformation(
                "No evaluated mutation history for {strategy}:{parameter}. Using multiplier 1.00.",
                strategyVersion,
                parameterName);

            return 1.00m;
        }


        var successes =
            history.Count(x =>
                x.Successful);
        
        var rate =
            (decimal)successes /
            history.Count;
        
        decimal multiplier =
            rate switch
            {
                >= 0.80m => 1.50m,
                >= 0.60m => 1.25m,
                >= 0.40m => 1.00m,
                >= 0.20m => 0.75m,
                       _ => 0.50m
            };
        
        _logger.LogInformation(
            """
            Mutation learning Strategy={strategy}
            Parameter={parameter}
            SuccessRate={rate:P}
            Multiplier={multiplier}
            Samples={samples}
            
            """,
            strategyVersion,
            parameterName,
            rate,
            multiplier,
            history.Count);
        
        return multiplier;
    }
}
