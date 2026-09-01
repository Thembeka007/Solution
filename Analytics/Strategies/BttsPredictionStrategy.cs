using Analytics.Ranking;
using Analytics.Selection;
using Analytics.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Analytics.Strategies;

public class BttsPredictionStrategy
    : IPredictionStrategy
{

    private readonly SportsBettingDbContext
        _db;

    private readonly HeadToHeadService
        _h2h;

    private readonly ILogger<BttsPredictionStrategy>
        _logger;

    private readonly BttsRankingService
        _ranking;

    private readonly StrategySelectionService
        _selection;

    public string Name =>
        "BTTS_STRATEGY_V1";

    public BttsPredictionStrategy(
        SportsBettingDbContext db,
        HeadToHeadService h2h,
        BttsRankingService ranking,
        ILogger<BttsPredictionStrategy> logger,
        StrategySelectionService selection)
    {
        _db = db;
        _h2h = h2h;
        _ranking = ranking;
        _logger = logger;
        _selection = selection;
    }


    public async Task<List<Prediction>>
        GeneratePredictionsAsync(
            Match match)
    {
        _logger.LogInformation(
            "===== BTTS STRATEGY START ===== Match={home} vs {away}",
            match.HomeTeam!.Name,
            match.AwayTeam!.Name);

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

        if (homeForm.Count < 3 ||
            awayForm.Count < 3)
        {
            _logger.LogInformation(
                 "BTTS REJECTED - insufficient history");

            return output;
        }

        var h2h =
            await _h2h.GetStatsAsync(
                match.HomeTeamId,
                match.AwayTeamId);

        decimal score =
            _ranking.CalculateScore(
                match,
                homeForm,
                awayForm,
                h2h);

        _logger.LogInformation(
            "BTTS SCORE={score:F2}",
            score);

        if (score < 70m)
        {
            _logger.LogInformation(
                "BTTS REJECTED Score={score:F2}",
                score);

            return output;
        }
        
        output.Add(
            new Prediction
            {
                MatchId =
                    match.Id,

                PredictionType =
                    "BTTS",

                PredictedOutcome =
                    "YES",

                Confidence =
                     score,

                StrategyVersion =
                     Name,

                 MatchDateUtc =
                     match.KickoffUtc,

                 Stake =
                     1m

                });

        _logger.LogInformation(
            "BTTS GENERATED Score={score:F2}",
            score);

        return output;
    }
}
