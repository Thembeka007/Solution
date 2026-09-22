using Analytics.Models;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Analytics.Services;

public class MomentumModelService
{
    private readonly FormScoringService _formScoring;

    private readonly GoalScoringService _goalScoring;

    private readonly HomeAdvantageService _homeAdvantage;

    private readonly ILogger<MomentumModelService> _logger;

    public MomentumModelService(
        FormScoringService formScoring,
        GoalScoringService goalScoring,
        HomeAdvantageService homeAdvantage,
        ILogger<MomentumModelService> logger)
    {
        _formScoring = formScoring;
        _goalScoring = goalScoring;
        _homeAdvantage = homeAdvantage;
        _logger = logger;
    }

    public MomentumModelResult Calculate(
        List<TeamForm> homeForm,
        List<TeamForm> awayForm,
        decimal attackBonus,
        decimal weakDefenceBonus,
        decimal homeAdvantageMultiplier,
        decimal edgeThreshold,
        decimal minimumEdgePercent,
        decimal confidenceThreshold)
    {
        var result =
            new MomentumModelResult();

        // =========================================
        // VALIDATE FORM
        // =========================================

        if (homeForm.Count < 2 ||
            awayForm.Count < 2)
        {
            _logger.LogInformation(
                "Momentum calculation skipped because insufficient form data exists.");

            return result;
        }

        // =========================================
        // GOAL PROFILES
        // =========================================

        var homeProfile =
            _goalScoring.Analyse(homeForm);

        var awayProfile =
            _goalScoring.Analyse(awayForm);

        // =========================================
        // BASE FORM SCORES
        // =========================================

        var homeScore =
            _formScoring.CalculateScore(homeForm);

        var awayScore =
            _formScoring.CalculateScore(awayForm);

        // =========================================
        // HOME ADVANTAGE
        // =========================================

        homeScore *=
            _homeAdvantage.GetMultiplier(
                homeAdvantageMultiplier);

        homeScore =
            Math.Max(
                0.01m,
                homeScore);

        awayScore =
            Math.Max(
                0.01m,
                awayScore);

        // =========================================
        // GOAL PROFILE ADJUSTMENTS
        // =========================================

        if (homeProfile.GoalsForAverage >= 2.0)
        {
            homeScore +=
                attackBonus;
        }

        if (awayProfile.GoalsForAverage >= 2.0)
        {
            awayScore +=
                attackBonus;
        }

        if (homeProfile.ConcedesOften)
        {
            awayScore +=
                weakDefenceBonus;
        }

        if (awayProfile.ConcedesOften)
        {
            homeScore +=
                weakDefenceBonus;
        }

        // =========================================
        // EDGE
        // =========================================

        var edge =
            Math.Abs(
                homeScore -
                awayScore);

        if (edge < edgeThreshold)
        {
            _logger.LogInformation(
                """
                Momentum candidate rejected
                
                Reason=RawEdgeBelowThreshold
                
                HomeScore={homeScore:F3}
                
                AwayScore={awayScore:F3}
                
                Edge={edge:F3}
                
                EdgeThreshold={threshold:F3} 
                """,
                homeScore,
                awayScore,
                edge,
                edgeThreshold);

            return result;
        }

        // =========================================
        // EDGE PERCENT
        // =========================================

        var total =
            homeScore +
            awayScore;

        var edgePercent =
            total > 0
                ? edge / total
                : 0m;

        if (edgePercent < minimumEdgePercent)
        {
            _logger.LogInformation(
                """
                Momentum candidate rejected
                
                Reason=EdgePercentBelowMinimum
                
                HomeScore={homeScore:F3}
                
                AwayScore={awayScore:F3}
                
                Edge={edge:F3}
                
                EdgePercent={edgePercent:P2}
                
                MinimumEdgePercent={minimum:P2} 
                """,
                homeScore,
                awayScore,
                edge,
                edgePercent,
                minimumEdgePercent);


            return result;
        }

        // =========================================
        // DOMINANCE
        // =========================================

        var dominanceRatio =
            Math.Max(
                homeScore,
                awayScore)
            /
            Math.Min(
                homeScore,
                awayScore);

        var rawConfidence =
            50m +
            ((dominanceRatio - 1m) * 35m);

        rawConfidence =
            Math.Clamp(
                rawConfidence,
                50m,
                92m);

        // =========================================
        // SAMPLE SIZE / RELIABILITY
        // =========================================

        var sampleSize =
            Math.Min(
                homeForm.Count,
                awayForm.Count);

        var reliability =
            sampleSize switch
            {
                <= 2 => 0.60m,
                <= 4 => 0.75m,
                <= 6 => 0.90m,
                _ => 1.00m
            };

        var confidence =
            rawConfidence *
            reliability
            +
            50m *
            (1m - reliability);

        confidence =
            Math.Clamp(
                confidence,
                50m,
                92m);

        // =========================================
        // CONFIDENCE THRESHOLD
        // =========================================

        if (confidence < confidenceThreshold)
        {
            _logger.LogInformation(
                """
                Momentum candidate rejected
                
                Reason=ConfidenceBelowThreshold
                
                RawConfidence={rawConfidence:F2}
                
                SampleSize={sampleSize}
                
                Reliability={reliability:F2}
                
                Confidence={confidence:F2}
                
                ConfidenceThreshold={threshold:F2} 
                """,
                rawConfidence,
                sampleSize,
                reliability,
                confidence,
                confidenceThreshold);


            return result;
        }

        // =========================================
        // CONFIDENCE EDGE
        // =========================================

        var confidenceEdge =
            confidence - 50m;

        if (confidenceEdge < 10m)
        {
            _logger.LogInformation(
                """
                Momentum candidate rejected
                
                Reason=ConfidenceEdgeBelowMinimum
                
                Confidence={confidence:F2}
                
                ConfidenceEdge={edge:F2}
                
                MinimumConfidenceEdge={minimum:F2} 
                """,
                confidence,
                confidenceEdge,
                10m);

            return result;
        }

        // =========================================
        // PREDICTED OUTCOME
        // =========================================

        string outcome;

        if (homeScore > awayScore)
        {
            outcome = "HOME_WIN";
        }
        else if (awayScore > homeScore)
        {
            outcome = "AWAY_WIN";
        }
        else
        {
            return result;
        }

        // =========================================
        // RESULT
        // =========================================

        result.ShouldGeneratePrediction =
            true;

        result.HomeScore =
            homeScore;

        result.AwayScore =
            awayScore;

        result.Edge =
            edge;

        result.EdgePercent =
            edgePercent;

        result.Confidence =
            confidence;

        result.PredictedOutcome =
            outcome;

        return result;

    }
}
