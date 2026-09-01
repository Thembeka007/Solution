using AI.Evolution;
using Analytics.Models;
using Analytics.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace AI.Optimization;

public class StrategyMutationEngine
{
    private readonly SportsBettingDbContext
        _db;

    private readonly StrategyParameterService
        _parameterService;


    private readonly AdaptiveMutationEngine
        _adaptiveMutationEngine;

    public StrategyMutationEngine(
        SportsBettingDbContext db,
        StrategyParameterService parameterService,
        AdaptiveMutationEngine adaptiveMutationEngine)
    {
        _db = db;
        _parameterService = parameterService;
        _adaptiveMutationEngine = adaptiveMutationEngine;
    }

    // =====================================
    // WEIGHT MUTATION
    // =====================================

    public Dictionary<string, decimal> Mutate(
        Dictionary<string, decimal> weights)
    {
        var mutated =
            new Dictionary<string, decimal>(
                weights);

        var random =
            new Random();

        foreach (var key in weights.Keys)
        {
            decimal mutation =
                (decimal)(
                    random.NextDouble() * 0.2
                    - 0.1);

            mutated[key] += mutation;

            mutated[key] =
                Math.Max(
                    0.1m,
                    Math.Min(
                        mutated[key],
                        1m));
        }

        return mutated;
    }

    // =====================================
    // ACTIVE STRATEGIES
    // =====================================

    public async Task<List<StrategyPerformance>>
        GetActiveStrategiesAsync()
    {
        return await _db
            .StrategyPerformances
            .Where(x =>
                x.IsActive)
            .OrderByDescending(x =>
                x.ROI)
            .ToListAsync();
    }

    // =====================================
    // APPLY MUTATION PLAN
    // =====================================

    public Dictionary<string, decimal>
        ApplyMutationPlan(Dictionary<string, decimal> weights,
        MutationPlan plan)
    {
        var mutated =
            new Dictionary<string, decimal>(weights);

        if (plan.IncreaseFormWeight &&
            mutated.ContainsKey("Form"))
        {
            mutated["Form"] *= 1.10m;
        }

        if (plan.DecreaseFormWeight &&
            mutated.ContainsKey("Form"))
        {
            mutated["Form"] *= 0.90m;
        }

        if (plan.IncreaseGoalWeight &&
            mutated.ContainsKey("Goals"))
        {
            mutated["Goals"] *= 1.10m;
        }

        if (plan.DecreaseGoalWeight &&
            mutated.ContainsKey("Goals"))
        {
            mutated["Goals"] *= 0.90m;
        }

        if (plan.IncreaseHomeAdvantage &&
            mutated.ContainsKey("HomeAdvantage"))
        {
            mutated["HomeAdvantage"] *= 1.05m;
        }

        if (plan.DecreaseHomeAdvantage &&
            mutated.ContainsKey("HomeAdvantage"))
        {
            mutated["HomeAdvantage"] *= 0.95m;
        }

        if (plan.IncreaseH2HWeight &&
            mutated.ContainsKey("H2H"))
        {
            mutated["H2H"] *= 1.05m;
        }

        if (plan.DecreaseH2HWeight &&
            mutated.ContainsKey("H2H"))
        {
            mutated["H2H"] *= 0.95m;
        }

        return mutated;
    }

    // ===================================
    // APPLY MUTATIONS
    // ===================================

    public async Task ApplyMutationsAsync(
        List<MutationPlan> plans)
    {
        foreach (var plan in plans)
        {           

            if (plan.IncreaseGoalWeight)
            {

                decimal amount =
                    await _adaptiveMutationEngine
                        .GetMutationAmountAsync(
                            plan.StrategyVersion,
                            "AttackBonus",
                            0.05m);

                await _parameterService
                    .AdjustParameterAsync(
                        plan.StrategyVersion,
                        "AttackBonus",
                        amount,
                        0.40m,
                        0.10m,
                        1.00m);
            }

            if (plan.DecreaseGoalWeight)
            {
                decimal amount =
                    await _adaptiveMutationEngine
                        .GetMutationAmountAsync(
                            plan.StrategyVersion,
                            "AttackBonus",
                            0.05m);

                await _parameterService
                    .AdjustParameterAsync(
                        plan.StrategyVersion,
                        "AttackBonus",
                        -amount,
                        0.40m,
                        0.10m,
                        1.00m);
            }

            if (plan.IncreaseFormWeight)
            {
                decimal edge =
                    await _adaptiveMutationEngine
                    .GetMutationAmountAsync(
                        plan.StrategyVersion,
                        "EdgeThreshold",
                        0.10m);

                await _parameterService
                    .AdjustParameterAsync(
                        plan.StrategyVersion,
                        "EdgeThreshold",
                        -edge,
                        1.50m,
                        0.50m,
                        3.00m);
            }

            if (plan.DecreaseFormWeight)
            {
                decimal edge =
                    await _adaptiveMutationEngine
                    .GetMutationAmountAsync(
                        plan.StrategyVersion,
                        "EdgeThreshold",
                        0.10m);

                await _parameterService
                    .AdjustParameterAsync(
                        plan.StrategyVersion,
                        "EdgeThreshold",
                        edge,
                        1.50m,
                        0.50m,
                        3.00m);
            }

            if (plan.IncreaseConfidenceThreshold)
            {
                decimal confidence =
                    await _adaptiveMutationEngine
                    .GetMutationAmountAsync(
                        plan.StrategyVersion,
                        "ConfidenceThreshold",
                        2m);

                await _parameterService
                    .AdjustParameterAsync(
                        plan.StrategyVersion,
                        "ConfidenceThreshold",
                        confidence,
                        60m,
                        50m,
                        95m);
            }

            if (plan.ReduceConfidenceThreshold)
            {
                decimal confidence =
                    await _adaptiveMutationEngine
                    .GetMutationAmountAsync(
                        plan.StrategyVersion,
                        "ConfidenceThreshold",
                        2m);

                await _parameterService
                    .AdjustParameterAsync(
                        plan.StrategyVersion,
                        "ConfidenceThreshold",
                        -confidence,
                        60m,
                        50m,
                        95m);
            }
        }
    }
}
