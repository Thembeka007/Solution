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
                
                HomeAdvantageDown={home}
                
                ExpectedImprovement={improvement:F2}
                
                """,
                existing.StrategyVersion,
                existing.IncreaseFormWeight,
                existing.IncreaseGoalWeight,
                existing.IncreaseConfidenceThreshold,
                existing.ReduceConfidenceThreshold,
                existing.DecreaseHomeAdvantage,
                existing.ExpectedImprovement);
        
        }

        _logger.LogInformation(
            "Generated {count} mutation plans",
            plans.Count);


        return plans;
    }

}
