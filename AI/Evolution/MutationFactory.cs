using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Evolution;

public class MutationFactory
{
    private readonly SportsBettingDbContext
        _db;

    public MutationFactory(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<List<MutationPlan>>
        BuildPlansAsync()
    {
        var plans =
            new List<MutationPlan>();

        var strategies =
            await _db.StrategyPerformances
                .ToListAsync();

        foreach (var strategy in strategies)
        {
            var plan =
                new MutationPlan
                {
                    StrategyVersion =
                        strategy.StrategyVersion
                };

            //------------------------------------
            // Poor accuracy
            //------------------------------------

            if (strategy.Accuracy < 55)
            {
                plan.IncreaseConfidenceThreshold = true;

                plan.Reason +=
                    "Low accuracy. ";
            }

            //------------------------------------
            // Negative ROI
            //------------------------------------

            if (strategy.ROI < 0)
            {
                plan.IncreaseFormWeight = true;

                plan.IncreaseGoalWeight = true;

                plan.Reason +=
                    "Negative ROI. ";
            }

            //------------------------------------
            // Confidence too optimistic
            //------------------------------------

            if (strategy.ConfidenceAccuracyGap < -10)
            {
                plan.IncreaseConfidenceThreshold = true;

                plan.Reason +=
                    "Confidence inflated. ";
            }

            //------------------------------------
            // Confidence too conservative
            //------------------------------------

            if (strategy.ConfidenceAccuracyGap > 10)
            {
                plan.ReduceConfidenceThreshold = true;

                plan.Reason +=
                    "Confidence conservative. ";
            }

            //------------------------------------
            // Catastrophic strategy
            //------------------------------------

            if (strategy.ROI < -40)
            {
                plan.ReplaceStrategy = true;

                plan.Reason +=
                    "Replace strategy. ";
            }

            plans.Add(plan);
        }

        return plans;
    }
}
