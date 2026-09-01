using Analytics.Services;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Analytics.Ranking;

public class BttsRankingService
{
    private readonly ILogger<BttsRankingService>
    _logger;

    //TO:DO NEXT IMPLEMENTATION
    private const decimal BttsRateWeight = 20m;
    private const decimal GoalsWeight = 5m;
    private const decimal ConcededWeight = 5m;
    private const decimal XgWeight = 2.5m;
    private const decimal H2HWeight = 10m;

    public BttsRankingService(
        ILogger<BttsRankingService> logger)
    {
        _logger = logger;
    }

    public decimal CalculateScore(
        Match match,
        List<TeamForm> homeForm,
        List<TeamForm> awayForm,
        HeadToHeadStats h2h)
    {
        decimal score = 0m;

        // ======================================
        // BTTS RATE (40 points)
        // ======================================

        decimal homeRate =
            homeForm.Count(x => x.BTTS)
            / (decimal)homeForm.Count;

        decimal awayRate =
            awayForm.Count(x => x.BTTS)
            / (decimal)awayForm.Count;


        decimal homeRateScore =
            homeRate * BttsRateWeight;

        decimal awayRateScore =
            awayRate * BttsRateWeight;

        score += homeRateScore;
       

        _logger.LogInformation(
            "BTTS SCORE after HomeRate = {score:F2}",
            score);

        score += awayRateScore;
        _logger.LogInformation(
            "BTTS SCORE after AwayRate = {score:F2}",
            score);

        // ======================================
        // GOALS SCORED (20 points)
        // ======================================

        double homeGoals =
            homeForm.Average(x => x.GoalsFor);

        double awayGoals =
            awayForm.Average(x => x.GoalsFor);

        decimal homeGoalsScore =
            (decimal)Math.Min(homeGoals, 2.0) * GoalsWeight;

        decimal awayGoalsScore =
            (decimal)Math.Min(awayGoals, 2.0) * GoalsWeight;

        score += homeGoalsScore;
        score += awayGoalsScore;

        _logger.LogInformation(
            "After Goals (+{home:F2} + {away:F2}) Score={score:F2}",
            homeGoalsScore,
            awayGoalsScore,
            score);

        // ======================================
        // GOALS CONCEDED (20 points)
        // ======================================

        double homeConceded =
            homeForm.Average(x => x.GoalsAgainst);

        double awayConceded =
            awayForm.Average(x => x.GoalsAgainst);

        decimal homeConcededScore =
            (decimal)Math.Min(homeConceded, 2.0) * ConcededWeight;

        decimal awayConcededScore =
            (decimal)Math.Min(awayConceded, 2.0) * ConcededWeight;

        score += homeConcededScore;
        score += awayConcededScore;

        _logger.LogInformation(
            "After Conceded (+{home:F2} + {away:F2}) Score={score:F2}",
            homeConcededScore,
            awayConcededScore,
            score);

        // ======================================
        // xG (10 points)
        // ======================================

        double homeXg =
            homeForm.Average(x => x.XG);

        double awayXg =
            awayForm.Average(x => x.XG);

        decimal homeXgScore = 0m;
        decimal awayXgScore = 0m;

        if (homeXg > 0)
        {
            homeXgScore =
                (decimal)Math.Min(homeXg, 2.0) * XgWeight;

            score += homeXgScore;
        }

        if (awayXg > 0)
        {
            awayXgScore =
                (decimal)Math.Min(awayXg, 2.0) * XgWeight;

            score += awayXgScore;
        }

        _logger.LogInformation(
            "After xG (+{home:F2} + {away:F2}) Score={score:F2}",
            homeXgScore,
            awayXgScore,
            score);

        // ======================================
        // BOTH TEAMS REGULARLY SCORE
        // ======================================

        bool homeScoresOften =
            homeForm.Count(x => x.GoalsFor > 0) >= 4;

        bool awayScoresOften =
            awayForm.Count(x => x.GoalsFor > 0) >= 4;

        if (homeScoresOften)
        {
            score += 2.5m;

            _logger.LogInformation(
                "Home scores often (+2.50) Score={score:F2}",
                score);
        }

        if (awayScoresOften)
        {
            score += 2.5m;

            _logger.LogInformation(
                "Away scores often (+2.50) Score={score:F2}",
                score);
        }

        // ======================================
        // BOTH TEAMS REGULARLY CONCEDE
        // ======================================

        bool homeConcedesOften =
            homeForm.Count(x => x.GoalsAgainst > 0) >= 3;

        bool awayConcedesOften =
            awayForm.Count(x => x.GoalsAgainst > 0) >= 3;

        if (homeConcedesOften)
        {
            score += 2.5m;

            _logger.LogInformation(
                "Home concedes often (+2.50) Score={score:F2}",
                score);
        }

        if (awayConcedesOften)
        {
            score += 2.5m;

            _logger.LogInformation(
                "Away concedes often (+2.50) Score={score:F2}",
                score);
        }

        // ======================================
        // HEAD TO HEAD
        // ======================================

        if (h2h.Matches > 0)
        {
            decimal h2hRate =
                h2h.BttsCount
                / (decimal)h2h.Matches;

            decimal h2hScore =
                h2hRate * H2HWeight;

            score += h2hRate;

            _logger.LogInformation(
                "After H2H (+{added:F2}) Score={score:F2}",
                h2hScore,
                score);

        }

        score =
            Math.Clamp(
                score,
                0m,
                100m);



        _logger.LogInformation(
            """
            BTTS SCORE BREAKDOWN
            Match={match}
            HomeRate={homeRate:P}
            AwayRate={awayRate:P}
            HomeGoals={homeGoals:F2}
            AwayGoals={awayGoals:F2}
            HomeConceded={homeConceded:F2}
            AwayConceded={awayConceded:F2}
            HomeXG={homeXg:F2}
            AwayXG={awayXg:F2}
            H2H={h2h}/{matches}
            FinalScore={score:F2}
            """,
            match.Id,
            homeRate,
            awayRate,
            homeGoals,
            awayGoals,
            homeConceded,
            awayConceded,
            homeXg,
            awayXg,
            h2h.BttsCount,
            h2h.Matches,
            score);

        _logger.LogInformation(
            "BTTS FINAL SCORE = {score:F2}",
            score);

        if (score >= 80)
        {
            _logger.LogInformation("BTTS BAND = ELITE");
        }
        else if (score >= 70)
        {
            _logger.LogInformation("BTTS BAND = STRONG");
        }
        else if (score >= 60)
        {
            _logger.LogInformation("BTTS BAND = GOOD");
        }
        else if (score >= 50)
        {
            _logger.LogInformation("BTTS BAND = FAIR");
        }
        else
        {
            _logger.LogInformation("BTTS BAND = POOR");
        }

        return score;
    }
}
