using Microsoft.Extensions.Logging;
using System.Numerics;

namespace AI.Evolution;

public class StrategyMutationService
{
    private readonly ILogger<StrategyMutationService>
        _logger;

    private readonly ParameterSuccessAnalyzer
        _parameterAnalyzer;

    public StrategyMutationService(
        ILogger<StrategyMutationService> logger,
        ParameterSuccessAnalyzer parameterAnalyzer)
    {
        _logger = logger;
        _parameterAnalyzer = parameterAnalyzer;
    }

    public async Task<List<MutationPlan>> BuildMutationPlans(
        List<StrategyWeightRecommendation> recommendations)
    {
        var plans =
            new List<MutationPlan>();

        // =========================================
        // HISTORICAL PARAMETER SCORE
        // =========================================

        decimal attackScore =
            await _parameterAnalyzer
                .GetSuccessScoreAsync("AttackBonus");
        
        decimal edgeScore =
            await _parameterAnalyzer
                .GetSuccessScoreAsync("EdgeThreshold");
        
        decimal confidenceScore =
            await _parameterAnalyzer
                .GetSuccessScoreAsync("ConfidenceThreshold");

        foreach (var recommendation in recommendations)
        {
            var existing =
                plans.FirstOrDefault(x =>
                    x.StrategyVersion ==
                    recommendation.Strategy);

            if (existing == null)
            {
                existing =
                    new MutationPlan
                    {
                        StrategyVersion =
                            recommendation.Strategy
                    };

                plans.Add(existing);
            }

            // =========================================
            // BUILD MUTATIONS
            // =========================================

            switch (recommendation.Metric)
            {
                case "ROI":

                    if (confidenceScore >= 0.60m)
                    {
                        existing.ReduceConfidenceThreshold =
                            true;
                    }
                    // ROI weakness can indicate that the strategy
                    // is being too selective.
                    //
                    // Explore lower relative-edge requirements
                    // while preserving the current 15% baseline.
                    
                    existing.DecreaseMinimumEdgePercent =
                        true;
                    

                    // No learning model yet
                    existing.DecreaseHomeAdvantage =
                        true;

                    break;

                case "Accuracy":

                    if (edgeScore >= 0.60m)
                    {
                        existing.IncreaseFormWeight =
                            true;
                    }

                    if (attackScore >= 0.60m)
                    {
                        existing.IncreaseGoalWeight =
                            true;
                    }

                    // Accuracy pressure can justify testing
                    // a more selective relative-edge requirement.
                    
                    existing.IncreaseMinimumEdgePercent =
                        true;

                    break;

                case "Confidence":

                    if (confidenceScore >= 0.60m)
                    {
                        existing.IncreaseConfidenceThreshold =
                            true;
                    }

                    break;
            }

            // =========================================
            // EXPECTED IMPROVEMENT
            // =========================================

            existing.ExpectedImprovement =
                Math.Max(
                    existing.ExpectedImprovement,
                    recommendation.ImprovementScore);


            if (string.IsNullOrWhiteSpace(existing.Reason))
            {
                existing.Reason =
                    recommendation.Reason;
            }
            else
            {
                existing.Reason +=
                    " | " +
                    recommendation.Reason;
            }
        }

        // =========================================
        // RESOLVE CONTRADICTORY EDGE MUTATIONS
        // =========================================
        //
        // A child must never receive both an
        // increase and decrease mutation for the
        // same parameter.
        //
        // =========================================

        foreach (var plan in plans)
        {
            if (plan.IncreaseMinimumEdgePercent &&
                plan.DecreaseMinimumEdgePercent)
            {
                // Preserve the decrease direction.
                // ROI expansion is the primary reason
                // we introduced this parameter.
                plan.IncreaseMinimumEdgePercent =
                    false;

                _logger.LogInformation(
                    """
                    Conflicting MinimumEdgePercent mutations resolved

                    Strategy={strategy}

                    Action=DecreaseOnly
                    """,
                    plan.StrategyVersion);
            }
        }

        _logger.LogInformation(
            """
            =========================================
            
            PARAMETER SUCCESS SCORES
            AttackBonus={attack:F2}
            EdgeThreshold={edge:F2}
            ConfidenceThreshold={confidence:F2}
            
            =========================================
            
            """,
            attackScore,
            edgeScore,
            confidenceScore);

        foreach (var existing in plans)
        {
            _logger.LogInformation(

                """
                Strategy={strategy}

                IncreaseFormWeight={form}

                IncreaseGoalWeight={goal}
                
                IncreaseConfidence={confidence}
                
                ReduceConfidence={reduce}

                IncreaseMinimumEdgePercent={increaseMinimumEdge}
                
                DecreaseMinimumEdgePercent={decreaseMinimumEdge}
                
                HomeAdvantageDown={home}
                
                ExpectedImprovement={improvement:F2}
                
                """,
                existing.StrategyVersion,
                existing.IncreaseFormWeight,
                existing.IncreaseGoalWeight,
                existing.IncreaseConfidenceThreshold,
                existing.ReduceConfidenceThreshold,
                existing.IncreaseMinimumEdgePercent,
                existing.DecreaseMinimumEdgePercent,
                existing.DecreaseHomeAdvantage,
                existing.ExpectedImprovement);
        
        }

        _logger.LogInformation(
            "Generated {count} mutation plans",
            plans.Count);


        return plans;
    }

}
