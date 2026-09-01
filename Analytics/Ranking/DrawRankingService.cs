using Analytics.Models;
using Analytics.Services;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Analytics.Ranking;

public class DrawRankingService
{
    private readonly ILogger<DrawRankingService>
        _logger;

    public DrawRankingService(
        ILogger<DrawRankingService> logger)
    {
        _logger = logger;
    }

    //=========================================
    // WEIGHTS
    //=========================================

    private const decimal
        FormSimilarityWeight = 25m;

    private const decimal
        GoalSimilarityWeight = 20m;

    private const decimal
        DefenceSimilarityWeight = 15m;

    private const decimal
        DrawRateWeight = 20m;

    private const decimal
        H2HWeight = 20m;

    public decimal CalculateScore(
        Match match,
        GoalScoringProfile home,
        GoalScoringProfile away,
        HeadToHeadStats h2h,
        List<TeamForm> homeForm,
        List<TeamForm> awayForm)
    {
        decimal score = 0m;

        //======================================
        // FORM SIMILARITY
        //======================================

        double formDifference =
            (double)Math.Abs(
                home.GoalPotential -
                away.GoalPotential);

        decimal formScore =
            Math.Max(
                0m,
                FormSimilarityWeight -
                (decimal)(formDifference * 5));

        score += formScore;

        //======================================
        // ATTACK SIMILARITY
        //======================================

        double attackDifference =
            Math.Abs(
                home.GoalsForAverage -
                away.GoalsForAverage);

        decimal attackScore =
            Math.Max(
                0m,
                GoalSimilarityWeight -
                (decimal)(attackDifference * 8));

        score += attackScore;

        //======================================
        // DEFENCE SIMILARITY
        //======================================

        double defenceDifference =
            Math.Abs(
                home.GoalsAgainstAverage -
                away.GoalsAgainstAverage);

        decimal defenceScore =
            Math.Max(
                0m,
                DefenceSimilarityWeight -
                (decimal)(defenceDifference * 6));

        score += defenceScore;

        //======================================
        // LOW SCORING BALANCE
        //======================================

        decimal balanceScore = 0m;

        if (home.GoalsForAverage <= 1.5 &&
            away.GoalsForAverage <= 1.5)
        {
            balanceScore += 10m;
        }

        if (Math.Abs(
                home.GoalsForAverage -
                away.GoalsForAverage) <= 0.50)
        {
            balanceScore += 10m;
        }

        score += balanceScore;

        //======================================
        // H2H DRAWS
        //======================================

        if (h2h.Matches > 0)
        {
            decimal h2hRate =
                h2h.DrawCount /
                (decimal)h2h.Matches;

            score +=
                h2hRate *
                H2HWeight;
        }

        //======================================
        // BONUS
        //======================================

        if (home.ScoresOften &&
            away.ScoresOften)
        {
            score += 2m;
        }

        if (!home.ConcedesOften &&
            !away.ConcedesOften)
        {
            score += 3m;
        }

        //======================================
        // LIMIT
        //======================================

        score =
            Math.Clamp(
                score,
                0m,
                100m);

        //======================================
        // LOGGING
        //======================================

        _logger.LogInformation(
            """
            DRAW SCORE

            Match={match}

            FormScore={formScore:F2}
            AttackScore={attackScore:F2}

            DefenceScore={defenceScore:F2}
            BalanceScore={balanceScore:F2}
         
            H2H={draws}/{matches}

            FinalScore={score:F2}
            """,
            match.Id,
            formScore,
            attackScore,
            defenceScore,
            balanceScore,
            h2h.DrawCount,
            h2h.Matches,
            score);

        return score;
    }
}
