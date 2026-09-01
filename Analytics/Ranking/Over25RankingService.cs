using Analytics.Models;
using Analytics.Services;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Analytics.Ranking;

public class Over25RankingService
{
    private readonly ILogger<Over25RankingService>
    _logger;

    public Over25RankingService(
        ILogger<Over25RankingService> logger)
    {
        _logger = logger;
    }

    private const decimal
        GoalsWeight = 20m;

    private const decimal
        ConcededWeight = 15m;

    private const decimal
        XgWeight = 15m;

    private const decimal
        OverRateWeight = 20m;

    private const decimal
        BttsWeight = 10m;

    private const decimal
        H2HWeight = 20m;

    public decimal CalculateScore(
        Match match,
        GoalScoringProfile home,
        GoalScoringProfile away,
        HeadToHeadStats h2h)
    {
        decimal score = 0m;

        //-------------------------------------
        // Goals scored
        //-------------------------------------

        score +=
            (decimal)Math.Min(
                home.GoalsForAverage,
                2.5) * GoalsWeight / 2.5m;

        score +=
            (decimal)Math.Min(
                away.GoalsForAverage,
                2.5) * GoalsWeight / 2.5m;

        //-------------------------------------
        // Goals conceded
        //-------------------------------------

        score +=
            (decimal)Math.Min(
                home.GoalsAgainstAverage,
                2.5) * ConcededWeight / 2.5m;

        score +=
            (decimal)Math.Min(
                away.GoalsAgainstAverage,
                2.5) * ConcededWeight / 2.5m;

        //-------------------------------------
        // xG
        //-------------------------------------

        score +=
            (decimal)Math.Min(
                home.XGAverage,
                2.0) * XgWeight / 2.0m;

        score +=
            (decimal)Math.Min(
                away.XGAverage,
                2.0) * XgWeight / 2.0m;

        //-------------------------------------
        // Over 2.5 frequency
        //-------------------------------------

        score +=
            home.Over25Rate *
            (OverRateWeight / 2);

        score +=
            away.Over25Rate *
            (OverRateWeight / 2);

        //-------------------------------------
        // BTTS
        //-------------------------------------

        score +=
            home.BTTSRate *
            (BttsWeight / 2);

        score +=
            away.BTTSRate *
            (BttsWeight / 2);

        //-------------------------------------
        // Attack bonus
        //-------------------------------------

        if (home.ScoresOften)
            score += 2.5m;

        if (away.ScoresOften)
            score += 2.5m;

        //-------------------------------------
        // Weak defence
        //-------------------------------------

        if (home.ConcedesOften)
            score += 2.5m;

        if (away.ConcedesOften)
            score += 2.5m;

        //-------------------------------------
        // H2H
        //-------------------------------------

        if (h2h.Matches > 0)
        {
            decimal rate =
                h2h.Over25Count /
                (decimal)h2h.Matches;

            score +=
                rate *
                H2HWeight;
        }

        score =
            Math.Clamp(
                score,
                0m,
                100m);

        _logger.LogInformation(
            """
            OVER25 SCORE

            Match={match}

            HomeGoals={homeGoals:F2}
            AwayGoals={awayGoals:F2}

            HomeConceded={homeAgainst:F2}
            AwayConceded={awayAgainst:F2}

            HomexG={homeXg:F2}
            AwayxG={awayXg:F2}

            HomeOver={homeOver:P}
            AwayOver={awayOver:P}

            H2H={h2h}/{matches}

            FinalScore={score:F2}
            """,
            match.Id,
            home.GoalsForAverage,
            away.GoalsForAverage,
            home.GoalsAgainstAverage,
            away.GoalsAgainstAverage,
            home.XGAverage,
            away.XGAverage,
            home.Over25Rate,
            away.Over25Rate,
            h2h.Over25Count,
            h2h.Matches,
            score);

        return score;
    }
}
