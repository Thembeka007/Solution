using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AI.Evolution;

public class AdaptiveMutationEngine
{
    private readonly SportsBettingDbContext _db;

    private readonly ILogger<AdaptiveMutationEngine> _logger;

    private readonly MutationLearningService _learningService;

    public AdaptiveMutationEngine(
        SportsBettingDbContext db,
        ILogger<AdaptiveMutationEngine> logger,
        MutationLearningService learningService)
    {
        _db = db;
        _logger = logger;
        _learningService = learningService;
    }

    public async Task<decimal> GetMutationAmountAsync(
        string strategyVersion,
        string parameterName,
        decimal defaultAmount)
    {

        var history =
            await _db.Set<MutationOutcome>()
                .Where(x => x.StrategyVersion ==
                strategyVersion && x.ParameterName ==
                parameterName)
                    .OrderByDescending(x => x.MutationUtc)
                    .Take(20)
                    .ToListAsync();

        if (!history.Any())
        {
            _logger.LogInformation(
                "No mutation history for {strategy}:{parameter}.Using default amount {amount}.",
                strategyVersion,
                parameterName,
                defaultAmount);
            
            return defaultAmount;

        }

        var successRate =
            history.Count(x => x.Successful) /
            (decimal)history.Count;
        
        decimal amount =
            defaultAmount;

        // ========================================
        // ADAPTAPTIVE MUTATION SIZE
        // ========================================
        
        if (successRate >= 0.80m)
        {
            amount *= 1.50m;
        }
        else if (successRate >= 0.60m)
        {
            amount *= 1.20m;
        }
        else if (successRate <= 0.20m)
        {
            amount *= 0.50m;
        }
        else if (successRate <= 0.40m)
        {
            amount *= 0.75m;
        }

        // =======================================
        // AVERAGE IMPROVEMENT
        // =======================================

        var averageImprovement =
            history.Average(x => x.Improvement);
        
        if (averageImprovement > 15m)
        {
            amount *= 1.25m;
        }
        
        if (averageImprovement < -10m)
        {
            amount *= 0.75m;
        }

        // =======================================
        // GET LEARNING MULTIPLIER
        // =======================================

        decimal learningMultiplier =
            await _learningService
                .GetLearningMultiplierAsync(
                    strategyVersion,
                    parameterName);

        amount *=
            learningMultiplier;

        // =======================================
        // CLAMP
        // =======================================

        amount =
            Math.Clamp(
                amount,
                0.005m,
                0.25m);
        
        _logger.LogInformation(
            """
            Adaptive mutation Strategy={strategy}
            Parameter={parameter}
            SuccessRate={success:P}
            AvgImprovement={improvement:F2}
            MutationAmount={amount:F3}
            LearningMultiplier={learning:F2}
            
            """,
            strategyVersion,
            parameterName,
            successRate,
            averageImprovement,
            amount,
            learningMultiplier);
        
        return amount;
    }
}
