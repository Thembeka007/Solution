using Analytics.Ranking;
using Analytics.Selection;
using Analytics.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Analytics.Strategies;

public class Over25PredictionStrategy
    : IPredictionStrategy
{
    private readonly SportsBettingDbContext
        _db;

    private readonly HeadToHeadService
        _h2h;

    private readonly Over25RankingService
        _ranking;

    private readonly GoalScoringService
        _goalScoring;

    private readonly ILogger<Over25PredictionStrategy>
        _logger;

    private readonly StrategySelectionService
        _selection;

    public string Name =>
        "OVER25_V1";

    public Over25PredictionStrategy(
        SportsBettingDbContext db,
        HeadToHeadService h2h,
        Over25RankingService ranking,
        GoalScoringService goalScoring,
        ILogger<Over25PredictionStrategy> logger,
        StrategySelectionService selection)
    {
        _db = db;
        _h2h = h2h;
        _ranking = ranking;
        _goalScoring = goalScoring;
        _logger = logger;
        _selection = selection;
    }

    public async Task<List<Prediction>>
        GeneratePredictionsAsync(
            Match match)
    {
        var output =
            new List<Prediction>();

        if (!await _selection.ShouldRunAsync(Name))
        {
            _logger.LogInformation(
                "{strategy} disabled by StrategySelectionService",
                Name);

            return output;
        }

        var homeForm =
            await _db.TeamForms
                .Where(x =>
                    x.TeamName ==
                    match.HomeTeam!.Name)
                .OrderByDescending(x =>
                    x.MatchDateUtc)
                .Take(5)
                .ToListAsync();

        var awayForm =
            await _db.TeamForms
                .Where(x =>
                    x.TeamName ==
                    match.AwayTeam!.Name)
                .OrderByDescending(x =>
                    x.MatchDateUtc)
                .Take(5)
                .ToListAsync();

        if (homeForm.Count < 2 ||
            awayForm.Count < 2)
        {
            _logger.LogInformation(
                "Skipping OVER25 for {home} vs {away}. Insufficient form data.",
                match.HomeTeam!.Name,
                match.AwayTeam!.Name);

            return output;
        }

        var homeProfile =
            _goalScoring.Analyse(homeForm);

        var awayProfile =
            _goalScoring.Analyse(awayForm);

        _logger.LogInformation(
            "OVER25 Profiles | HomeGoals={homeGoals:F2} AwayGoals={awayGoals:F2} HomeOver={homeOver:P} AwayOver={awayOver:P}",
            homeProfile.GoalsForAverage,
            awayProfile.GoalsForAverage,
            homeProfile.Over25Rate,
            awayProfile.Over25Rate);

        var h2h =
            await _h2h.GetStatsAsync(
                match.HomeTeamId,
                match.AwayTeamId);

        var score =
            _ranking.CalculateScore(
                match,
                homeProfile,
                awayProfile,
                h2h);

        _logger.LogInformation(
            "OVER25 SCORE = {score:F2}",
            score);

        if (score < 70m)
        {
            return output;
        }

        output.Add(
                new Prediction
                {
                    MatchId =
                        match.Id,

                    PredictionType =
                        "OVER25",

                    PredictedOutcome =
                        "OVER_2_5",

                    Confidence =
                        Math.Clamp(
                            score,
                            70m,
                            92m),

                    StrategyVersion =
                        Name,

                    MatchDateUtc =
                        match.KickoffUtc,

                    Stake =
                        1m
                });

        _logger.LogInformation(
            "OVER25 GENERATED | {home} vs {away} | Score={score:F2} Confidence={confidence:F2}",
            match.HomeTeam!.Name,
            match.AwayTeam!.Name,
            score,
            Math.Clamp(score, 70m, 92m));

        return output;
    }
}




