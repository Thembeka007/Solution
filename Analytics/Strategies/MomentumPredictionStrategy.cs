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

    private readonly StrategySelectionService
        _selection;

    private readonly StrategyParameterService
        _parameterService;

    private readonly ActiveStrategyService
        _activeStrategy;

    private readonly MomentumModelService
        _momentumModel;

    private string _cachedName =
        "MOMENTUM_V1";

    public string Name =>
        _cachedName;

    public MomentumPredictionStrategy(
        SportsBettingDbContext db,       
        ILogger<MomentumPredictionStrategy> logger,
        StrategySelectionService selection,
        StrategyParameterService parameterService,
        ActiveStrategyService activeStrategy,
        MomentumModelService momentumModel)
    {
        _db = db;
        _logger = logger;
        _selection = selection;
        _parameterService = parameterService;
        _activeStrategy = activeStrategy;
        _momentumModel = momentumModel;
    }

    public async Task<List<Prediction>>
        GeneratePredictionsAsync(
            Match match)
    {

        // =========================================
        // RESOLVE ACTIVE MOMENTUM VERSION
        // =========================================

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

        // =========================================
        // LOAD TEAM FORM
        // =========================================

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
            """
            Momentum form loaded

            Home={home}

            HomeForms={homeForms}

            Away={away}

            AwayForms={awayForms}
            """,
            match.HomeTeam!.Name,
            homeForm.Count,
            match.AwayTeam!.Name,
            awayForm.Count);

        if (homeForm.Count < 2 ||
            awayForm.Count < 2)
        {
            _logger.LogWarning(
                """
                Momentum prediction skipped

                Home={home}

                Away={away}

                Reason=Insufficient form data
                """,
                match.HomeTeam.Name,
                match.AwayTeam.Name);

            return output;
        }

        // =========================================
        // LOAD STRATEGY PARAMETERS
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

        decimal homeAdvantageMultiplier =
            await _parameterService.GetParameterAsync(
                Name,
                "HomeAdvantageMultiplier",
                1.15m);

        decimal edgeThreshold =
            await _parameterService.GetParameterAsync(
                Name,
                "EdgeThreshold",
                1.50m);

        decimal minimumEdgePercent =
            await _parameterService.GetParameterAsync(
                Name,
                "MinimumEdgePercent",
                0.15m);

        decimal confidenceThreshold =
            await _parameterService.GetParameterAsync(
                Name,
                "ConfidenceThreshold",
                60m);

        _logger.LogInformation(
            """
            Momentum Parameters

            Strategy={strategy}

            AttackBonus={attack}

            WeakDefenceBonus={defence}

            HomeAdvantageMultiplier={homeAdvantage}

            EdgeThreshold={edge}

            MinimumEdgePercent={minimumEdgePercent:P2}

            ConfidenceThreshold={confidence}
            """,
            Name,
            attackBonus,
            weakDefenceBonus,
            homeAdvantageMultiplier,
            edgeThreshold,
            minimumEdgePercent,
            confidenceThreshold);

        // =========================================
        // RUN MOMENTUM MODEL
        // =========================================

        var result =
            _momentumModel.Calculate(
                homeForm,
                awayForm,
                attackBonus,
                weakDefenceBonus,
                homeAdvantageMultiplier,
                edgeThreshold,
                minimumEdgePercent,
                confidenceThreshold);


        if (!result.ShouldGeneratePrediction)
        {
            _logger.LogInformation(
                """
                Momentum model produced no prediction

                Strategy={strategy}

                Match={match}

                """,
                Name,
                match.Id);

            return output;
        }

        // =========================================
        // CREATE PREDICTION
        // =========================================

        _logger.LogInformation(
            """
            Momentum prediction generated

            Strategy={strategy}

            Match={match}

            Outcome={outcome}

            Confidence={confidence:F2}

            HomeScore={homeScore:F2}

            AwayScore={awayScore:F2}

            Edge={edge:F2}

            EdgePercent={edgePercent:P2}
            """,
            Name,
            match.Id,
            result.PredictedOutcome,
            result.Confidence,
            result.HomeScore,
            result.AwayScore,
            result.Edge,
            result.EdgePercent);

        output.Add(
            new Prediction
            {
                MatchId =
                    match.Id,

                PredictionType =
                    "MATCH_RESULT",

                PredictedOutcome =
                    result.PredictedOutcome,

                Confidence =
                    result.Confidence,

                StrategyVersion =
                    Name,

                MatchDateUtc =
                    match.KickoffUtc,

                Stake =
                    result.PredictedOutcome ==
                    "AWAY_WIN"
                        ? 1m
                        : 0m
            });

        return output;

    }       
}
