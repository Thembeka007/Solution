using Analytics.Ranking;
using Analytics.Selection;
using Analytics.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Analytics.Strategies;

public class DrawPredictionStrategy
    : IPredictionStrategy
{
    private readonly SportsBettingDbContext
        _db;

    private readonly FormScoringService
        _formScoring;

    private readonly ILogger<DrawPredictionStrategy>
        _logger;

    private readonly GoalScoringService
        _goalScoring;

    private readonly DrawRankingService
        _ranking;

    private readonly HeadToHeadService
        _h2h;

    private readonly StrategySelectionService
        _selection;

    public string Name =>
        "DRAW_V1";

    public DrawPredictionStrategy(
        SportsBettingDbContext db,
        FormScoringService formScoring,
        ILogger<DrawPredictionStrategy> logger,
        GoalScoringService goalScoring,
        DrawRankingService ranking,
        HeadToHeadService h2h,
        StrategySelectionService selection)
    {
        _db = db;
        _formScoring = formScoring;
        _logger = logger;
        _goalScoring = goalScoring;
        _ranking = ranking;
        _h2h = h2h;
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
                .Take(10)
                .ToListAsync();

        var awayForm =
            await _db.TeamForms
                .Where(x =>
                    x.TeamName ==
                    match.AwayTeam!.Name)
                .OrderByDescending(x =>
                    x.MatchDateUtc)
                .Take(10)
                .ToListAsync();

        if (homeForm.Count < 2 ||
            awayForm.Count < 2)
        {
            return output;
        }

        var homeProfile =
            _goalScoring.Analyse(homeForm);

        var awayProfile =
            _goalScoring.Analyse(awayForm);

        var h2h =
            await _h2h.GetStatsAsync(
                match.HomeTeamId,
                match.AwayTeamId);

        decimal homeScore =
            _formScoring.CalculateScore(homeForm);

        decimal awayScore =
            _formScoring.CalculateScore(awayForm);

        decimal score =
            _ranking.CalculateScore(
                match,
                homeProfile,
                awayProfile,
                h2h,
                homeForm,
                awayForm);

        if (score < 70m)
        {
            return output;
        }

        decimal confidence =
            Math.Clamp(
                score,
                70m,
                92m);

        _logger.LogInformation(
            "DRAW GENERATED | {home} vs {away} | Score={score:F2} Confidence={confidence:F2}",
            match.HomeTeam!.Name,
            match.AwayTeam!.Name,
            score,
            confidence);

        output.Add(
            new Prediction
            {
                MatchId =
                    match.Id,

                PredictionType =
                    "MATCH_RESULT",

                PredictedOutcome =
                    "DRAW",

                Confidence =
                    confidence,

                StrategyVersion =
                    Name,

                MatchDateUtc =
                    match.KickoffUtc

            });

        return output;
    }
}