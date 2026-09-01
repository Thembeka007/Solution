using Analytics.Selection;
using Analytics.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Analytics.Strategies;

public class MomentumPredictionStrategy
    : IPredictionStrategy
{
    private readonly ILogger<MomentumPredictionStrategy>
        _logger;

    private readonly SportsBettingDbContext
        _db;

    private readonly FormScoringService
        _formScoring;

    private readonly HomeAdvantageService
        _homeAdvantage;

    private readonly GoalScoringService
        _goalScoring;

    private readonly StrategySelectionService
        _selection;

    private readonly StrategyParameterService
        _parameterService;

    private readonly ActiveStrategyService
        _activeStrategy;

    private string _cachedName =
        "MOMENTUM_V1";

    public string Name =>
        _cachedName;

    public MomentumPredictionStrategy(
        SportsBettingDbContext db,
        FormScoringService formScoring,
        ILogger<MomentumPredictionStrategy> logger,
        HomeAdvantageService homeAdvantage,
        GoalScoringService goalScoring,
        StrategySelectionService selection,
        StrategyParameterService parameterService,
        ActiveStrategyService activeStrategy)
    {
        _db = db;
        _formScoring = formScoring;
        _logger = logger;
        _homeAdvantage = homeAdvantage;
        _goalScoring = goalScoring;
        _selection = selection;
        _parameterService = parameterService;
        _activeStrategy = activeStrategy;
    }

    public async Task<List<Prediction>>
        GeneratePredictionsAsync(
            Match match)
    {
        _cachedName =
            await _activeStrategy
                .GetMomentumVersionAsync();

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

        _logger.LogInformation(
            "Home={home} HomeForms={count}",
            match.HomeTeam!.Name,
            homeForm.Count);

        _logger.LogInformation(
            "Away={away} AwayForms={count}",
            match.AwayTeam!.Name,
            awayForm.Count);

        _logger.LogInformation(
            "Home={home} Forms={count}",
            match.HomeTeam.Name,
            homeForm.Count);

        _logger.LogInformation(
            "Away={away} Forms={count}",
            match.AwayTeam.Name,
            awayForm.Count);

        if (homeForm.Count < 2 ||
            awayForm.Count < 2)
        {
            _logger.LogWarning(
                "Skipping {home} vs {away} because no form data exists",
                match.HomeTeam.Name,
                match.AwayTeam.Name);

            return output;
        }

        var homeProfile =
            _goalScoring.Analyse(homeForm);

        var awayProfile =
            _goalScoring.Analyse(awayForm);

        //========================================================================
        // Gather inputs for TrueForm override - business rule:
        // Last 10 games considered first, but trueForm taken over last 3 games.
        //========================================================================

        var homeScore =
            _formScoring
                .CalculateScore(homeForm);

        var awayScore =
            _formScoring
                .CalculateScore(awayForm);

        // ===================================
        // Homeground advantage - Add boost
        // ===================================

        homeScore *=
            _homeAdvantage.GetMultiplier();

        _logger.LogInformation(
            "HOME ADVANTAGE APPLIED={multiplier}",
            _homeAdvantage.GetMultiplier());

        homeScore =
            Math.Max(0.01m, homeScore);

        awayScore =
            Math.Max(0.01m, awayScore);

        // =========================================
        // Get Tuning Values
        // =========================================

        decimal attackBonus =
            await _parameterService.GetParameterAsync(
                Name,
                "AttackBonus",
                0.40m);

        decimal weakDefenceBonus =
            await _parameterService.GetParameterAsync(
                Name,
                "WeakDefenceBonus",
                0.25m);

        //---------------------------------------
        // Goal profile adjustment
        //---------------------------------------

        if (homeProfile.GoalsForAverage >= 2.0)
        {
            homeScore += attackBonus;
        }

        if (awayProfile.GoalsForAverage >= 2.0)
        {
            awayScore += attackBonus;
        }

        if (homeProfile.ConcedesOften)
        {
            awayScore += weakDefenceBonus;
        }

        if (awayProfile.ConcedesOften)
        {
            homeScore += weakDefenceBonus;
        }


        var edge =
            Math.Abs(homeScore - awayScore);

        _logger.LogInformation(
            "EDGE CHECK {home} vs {away} | HomeScore={homeScore} AwayScore={awayScore} Edge={edge}",
            match.HomeTeam.Name,
            match.AwayTeam.Name,
            homeScore,
            awayScore,
            edge);

        // ===============================================================
        // EDGE THRESHOLD
        // ===============================================================

        decimal edgeThreshold =
            await _parameterService.GetParameterAsync(
                Name,
                "EdgeThreshold",
                1.5m);

        if (edge < edgeThreshold)
        {
            _logger.LogInformation(
                "Skipping {home} vs {away} because edge too small: {edge}",
                match.HomeTeam.Name,
                match.AwayTeam.Name,
                edge);

            return output;
        }

        _logger.LogInformation(
            """
            Momentum Parameters

            AttackBonus={attack}

            WeakDefenceBonus={defence}

            EdgeThreshold={edge}
            """,
            attackBonus,
            weakDefenceBonus,
            edgeThreshold);

        var total =
            homeScore + awayScore;

        var edgePercent =
            total > 0
                ? Math.Abs(homeScore - awayScore) / total
                : 0m;

        if (edgePercent < 0.15m)
        {
            _logger.LogInformation(
                "Skipping {home} vs {away} because edgePercent too small: {edgePercent:P}",
                match.HomeTeam.Name,
                match.AwayTeam.Name,
                edgePercent);

            return output;
        }

        var scoreGap =
            Math.Abs(homeScore - awayScore);

        _logger.LogInformation(
            "SCORE GAP={gap}",
            scoreGap);

        var dominanceRatio =
            Math.Max(homeScore, awayScore)
            /
            Math.Min(homeScore, awayScore);

        var rawConfidence =
            50m +
            ((dominanceRatio - 1m) * 35m);

        rawConfidence =
            Math.Clamp(
                rawConfidence,
                50m,
                92m);

        _logger.LogInformation(
            "RAW CONFIDENCE={confidence}",
            rawConfidence);

        var sampleSize =
            Math.Min(homeForm.Count,
            awayForm.Count);

        _logger.LogInformation(
            "Competition sample size = {sampleSize}",
            sampleSize);

        var reliability =
            sampleSize switch
            {
                <= 2 => 0.60m,
                <= 4 => 0.75m,
                <= 6 => 0.90m,
                _ => 1.00m
            };

        var confidence =
            rawConfidence * reliability +
            50m * (1m - reliability);

        confidence =
            Math.Clamp(
                confidence,
                50m,
                92m);

        _logger.LogInformation(
            "MATCH MODEL | Home={home} Away={away} HomeScore={homeScore} AwayScore={awayScore} Gap={gap} Confidence={confidence}",
            match.HomeTeam.Name,
            match.AwayTeam.Name,
            homeScore,
            awayScore,
            scoreGap,
            confidence);


        if (confidence < 60m)
        {
            _logger.LogInformation(
                "Skipping {home} vs {away} because confidence too low: {confidence}",
                match.HomeTeam.Name,
                match.AwayTeam.Name,
                confidence);

            return output;
        }

        var confidenceEdge =
            confidence - 50m;

        _logger.LogInformation(
            """
            Goal Adjustment

            HomeGoals={homeGoals:F2}
            AwayGoals={awayGoals:F2}

            HomeScoresOften={homeAttack}
            AwayScoresOften={awayAttack}

            HomeConcedesOften={homeDef}
            AwayConcedesOften={awayDef}

            HomeScore={homeScore:F2}
            AwayScore={awayScore:F2}
            """,
            homeProfile.GoalsForAverage,
            awayProfile.GoalsForAverage,
            homeProfile.ScoresOften,
            awayProfile.ScoresOften,
            homeProfile.ConcedesOften,
            awayProfile.ConcedesOften,
            homeScore,
            awayScore);

        if (confidenceEdge < 10m)
        {
            return output;
        }



        if (homeScore > awayScore)
        {
            _logger.LogInformation(
                "Generated prediction: HomeScore={home} AwayScore={away}" +
                "Confidence={confidence}",
                homeScore,
                awayScore,
                confidence);

            _logger.LogInformation(
                "MOMENTUM GENERATED -> Match={match} Confidence={confidence}",
                match.Id,
                confidence);

            _logger.LogInformation(
                "FINAL MODEL | Home={home} Away={away} HomeScore={homeScore} AwayScore={awayScore} Confidence={confidence}",
                match.HomeTeam.Name,
                match.AwayTeam.Name,
                homeScore,
                awayScore,
                confidence);

            output.Add(
                new Prediction
                {
                    MatchId =
                        match.Id,

                    PredictionType =
                        "MATCH_RESULT",

                    PredictedOutcome =
                        "HOME_WIN",

                    Confidence =
                        confidence,

                    StrategyVersion =
                        Name,

                    MatchDateUtc =
                        match.KickoffUtc,
                });
        }
        else if (awayScore > homeScore)
        {
            _logger.LogInformation(
                "FINAL MODEL | Home={home} Away={away} HomeScore={homeScore} AwayScore={awayScore} Confidence={confidence}",
                match.HomeTeam.Name,
                match.AwayTeam.Name,
                homeScore,
                awayScore,
                confidence);

            output.Add(
                new Prediction
                {
                    MatchId =
                        match.Id,

                    PredictionType =
                        "MATCH_RESULT",

                    PredictedOutcome =
                        "AWAY_WIN",

                    Confidence =
                        confidence,

                    StrategyVersion =
                        Name,

                    MatchDateUtc =
                        match.KickoffUtc,

                    Stake =
                        1m
                });


        }

        return output;
    }
}
