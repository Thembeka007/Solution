using Analytics.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Analytics.Services;

public class HistoricalCandidatePredictionService
{
    private readonly SportsBettingDbContext _db;

    private readonly StrategyParameterService
        _parameterService;

    private readonly MomentumModelService
        _momentumModel;

    private readonly HistoricalCandidateMarketService
        _marketService;

    private readonly ILogger<HistoricalCandidatePredictionService>
        _logger;


    public HistoricalCandidatePredictionService(
        SportsBettingDbContext db,
        StrategyParameterService parameterService,
        MomentumModelService momentumModel,
        HistoricalCandidateMarketService marketService,
        ILogger<HistoricalCandidatePredictionService> logger)
    {
        _db = db;
        _parameterService = parameterService;
        _momentumModel = momentumModel;
        _marketService = marketService;
        _logger = logger;
        
    }

    public async Task<HistoricalCandidatePredictionResult?>
        EvaluateAsync(
            string strategyVersion,
            Match match)
    {
        //------------------------------------------
        // LOAD HISTORICAL FORM
        //------------------------------------------

        var homeForm =
            await _db.TeamForms
                .AsNoTracking()
                .Where(x =>
                    x.TeamId == match.HomeTeamId &&
                    x.MatchDateUtc < match.KickoffUtc)
                .OrderByDescending(x =>
                    x.MatchDateUtc)
                .ThenByDescending(x =>
                    x.Id)
                .Take(10)
                .ToListAsync();

        var awayForm =
            await _db.TeamForms
                .AsNoTracking()
                .Where(x =>
                    x.TeamId == match.AwayTeamId &&
                    x.MatchDateUtc < match.KickoffUtc)
                .OrderByDescending(x =>
                    x.MatchDateUtc)
                .ThenByDescending(x =>
                    x.Id)
                .Take(10)
                .ToListAsync();

        _logger.LogInformation(
            """
            Historical candidate form loaded

            Strategy={strategy}

            Match={match}

            HomeForms={homeForms}

            AwayForms={awayForms}

            Kickoff={kickoff}
            """,
            strategyVersion,
            match.Id,
            homeForm.Count,
            awayForm.Count,
            match.KickoffUtc);

        if (homeForm.Count < 2 ||
            awayForm.Count < 2)
        {
            _logger.LogInformation(
                """
                
                Historical candidate skipped
                
                Strategy={strategy}
                
                Match={match}
                
                HomeTeam={homeTeam}
                
                AwayTeam={awayTeam}
                
                HomeForms={homeForms}
                
                AwayForms={awayForms}
                
                HomeFormRequirementMet={homeRequirement}
                
                AwayFormRequirementMet={awayRequirement}
                
                Reason=Insufficient historical form
                
                """,
                strategyVersion,
                match.Id,
                match.HomeTeamId,
                match.AwayTeamId,
                homeForm.Count,
                awayForm.Count,
                homeForm.Count >= 2,
                awayForm.Count >= 2);

            return null;
        }

        //------------------------------------------
        // LOAD CANDIDATE PARAMETERS
        //------------------------------------------

        decimal attackBonus =
            await _parameterService.GetParameterAsync(
                strategyVersion,
                "AttackBonus",
                0.40m);

        decimal weakDefenceBonus =
            await _parameterService.GetParameterAsync(
                strategyVersion,
                "WeakDefenceBonus",
                0.25m);

        decimal homeAdvantageMultiplier =
            await _parameterService.GetParameterAsync(
                strategyVersion,
                "HomeAdvantageMultiplier",
                1.15m);

        decimal edgeThreshold =
            await _parameterService.GetParameterAsync(
                strategyVersion,
                "EdgeThreshold",
                1.50m);

        decimal minimumEdgePercent =
            await _parameterService.GetParameterAsync(
                strategyVersion,
                "MinimumEdgePercent",
                0.15m);

        decimal confidenceThreshold =
            await _parameterService.GetParameterAsync(
                strategyVersion,
                "ConfidenceThreshold",
                60m);

        //------------------------------------------
        // RUN CANDIDATE MODEL
        //------------------------------------------

        var model =
            _momentumModel.Calculate(
                homeForm,
                awayForm,
                attackBonus,
                weakDefenceBonus,
                homeAdvantageMultiplier,
                edgeThreshold,
                minimumEdgePercent,
                confidenceThreshold);

        if (!model.ShouldGeneratePrediction)
        {
            _logger.LogInformation(
                """
                Historical candidate produced no prediction

                Strategy={strategy}

                Match={match}
                """,
                strategyVersion,
                match.Id);

            return new HistoricalCandidatePredictionResult
            {
                StrategyVersion = strategyVersion,
                MatchId = match.Id,
                KickoffUtc = match.KickoffUtc,
                ShouldGeneratePrediction = false
            };
        }

        //------------------------------------------
        // BUILD RESULT
        //------------------------------------------

        var result =
            new HistoricalCandidatePredictionResult
            {
                StrategyVersion =
                    strategyVersion,

                MatchId =
                    match.Id,

                KickoffUtc =
                    match.KickoffUtc,

                ShouldGeneratePrediction =
                    true,

                PredictedOutcome =
                    model.PredictedOutcome,

                Confidence =
                    model.Confidence,

                HomeScore =
                    model.HomeScore,

                AwayScore =
                    model.AwayScore,

                Edge =
                    model.Edge,

                EdgePercent =
                    model.EdgePercent
            };

        await _marketService.ApplyMarketDataAsync(
            result,
            match);

        _logger.LogInformation(
            """
            Historical candidate prediction generated

            Strategy={strategy}

            Match={match}

            Outcome={outcome}

            Confidence={confidence:F2}

            HomeScore={homeScore:F2}

            AwayScore={awayScore:F2}

            Edge={edge:F2}

            EdgePercent={edgePercent:P2}
            """,
            strategyVersion,
            match.Id,
            result.PredictedOutcome,
            result.Confidence,
            result.HomeScore,
            result.AwayScore,
            result.Edge,
            result.EdgePercent);

        return result;
    }

}
