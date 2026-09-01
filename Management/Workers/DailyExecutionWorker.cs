using AI.Evolution;
using AI.Optimization;
using Analytics.Services;
using Analytics.Validation;
using Betting.Selection;
using Betting.Services;
using Betting.Settlement;
using Infrastructure.Data;
using Ingestion.FootballData.Services;
using Ingestion.Odds.Services;
using Management.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Management.Workers;

public class DailyExecutionWorker : BackgroundService
{
    private readonly IServiceProvider _services;

    private readonly ILogger<DailyExecutionWorker>
        _logger;

    public DailyExecutionWorker(
        IServiceProvider services,
        ILogger<DailyExecutionWorker> logger)
    {
        _services = services;
        _logger = logger;

    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {

                _logger.LogInformation(
                    "Daily execution started at {time}",
                    DateTime.UtcNow);

                using var scope =
                    _services.CreateScope();

                // =====================================
                // RESOLVE WORKER SERVICES
                // =====================================

                var predictionSignals =
                    scope.ServiceProvider
                        .GetRequiredService<
                            PredictionSignalService>();

                var settlement =
                    scope.ServiceProvider
                        .GetRequiredService<
                            PredictionSettlementService>();

                var selector =
                    scope.ServiceProvider
                        .GetRequiredService<
                            DailyPickSelector>();

                var categoryAllocator =
                    scope.ServiceProvider
                        .GetRequiredService<
                            BetCategoryAllocator>();

                var slipBuilder =
                    scope.ServiceProvider
                        .GetRequiredService<
                            BettingSlipBuilder>();

                var persistence =
                    scope.ServiceProvider
                        .GetRequiredService<
                            DailySlipPersistenceService>();

                var performanceAnalyzer =
                    scope.ServiceProvider
                        .GetRequiredService<
                            PerformanceAnalyzer>();

                var strategyAnalyzer =
                    scope.ServiceProvider
                        .GetRequiredService<
                            StrategyPerformanceService>();

                var calibration =
                    scope.ServiceProvider
                        .GetRequiredService<
                            ConfidenceCalibrationService>();

                var mutationEngine =
                    scope.ServiceProvider
                        .GetRequiredService<
                            StrategyMutationEngine>();

                var replacementService =
                    scope.ServiceProvider
                        .GetRequiredService<
                            AutoStrategyReplacementService>();

                var teamFormSync =
                    scope.ServiceProvider
                        .GetRequiredService<
                            TeamFormSyncService>();

                var settlementService =
                    scope.ServiceProvider
                        .GetRequiredService<
                            SlipSettlementService>();

                var matchRefreshService =
                    scope.ServiceProvider
                        .GetRequiredService<
                            MatchRefreshService>();

                var predictionValidationService =
                    scope.ServiceProvider
                        .GetRequiredService<
                            PredictionValidationService>();

                var strategyWeightAnalyzer =
                    scope.ServiceProvider
                        .GetRequiredService<
                            StrategyWeightAnalyzer>();

                var strategyMutationService =
                    scope.ServiceProvider
                        .GetRequiredService<
                            StrategyMutationService>();

                var mutationExecutor =
                    scope.ServiceProvider
                        .GetRequiredService<
                            StrategyMutationExecutor>();

                var championService =
                    scope.ServiceProvider
                        .GetRequiredService<
                            StrategyChampionService>();

                var championFilter =
                    scope.ServiceProvider
                        .GetRequiredService<
                            ChampionMutationFilter>();

                var mutationRecorder =
                    scope.ServiceProvider
                        .GetRequiredService<
                            MutationOutcomeRecorder>();

                var mutationFeedbackService =
                    scope.ServiceProvider
                        .GetRequiredService<
                            MutationFeedbackService>();

                var populationManager =
                    scope.ServiceProvider
                        .GetRequiredService<
                            PopulationManager>();

                var evolutionStatisticsService =
                    scope.ServiceProvider
                        .GetRequiredService<
                            EvolutionStatisticsService>();

                // =====================================
                // REFRESH & SETTLE FINISHED MATCHES
                // =====================================

                _logger.LogInformation(
                    "Refresh matches starting..");

                await matchRefreshService
                  .RefreshAsync();

                _logger.LogInformation(
                    "Starting settlement process");

                await settlement
                    .SettleAsync();

                _logger.LogInformation(
                    "Settlement completed");

                //======================================
                // RUN MATCH COMPLETED VALIDATION
                //======================================

                _logger.LogInformation(
                    "Running validation process");

                await predictionValidationService
                    .ValidatePendingPredictionsAsync();

                _logger.LogInformation(
                    "Validation completed");

                //======================================
                // SYNCTEAMFORM
                //======================================

                _logger.LogInformation(
                    "Generating team forms...");

                await teamFormSync
                    .SyncAsync();

                _logger.LogInformation(
                    "Team forms generated.");

                // =====================================
                // CALIBRATION
                // =====================================

                _logger.LogInformation(
                    "Recalculating confidence calibration...");

                await calibration
                    .RecalculateAsync();

                _logger.LogInformation(
                    "Confidence calibration updated.");

                // =====================================
                // DAILY SNAPSHOT
                // =====================================

                _logger.LogInformation(
                    "Starting performance analysis");

                await performanceAnalyzer
                    .GenerateDailySnapshotAsync(
                        DateTime.UtcNow);

                await strategyAnalyzer
                    .AnalyzeAsync();

                _logger.LogInformation(
                    "Analysis completed");

                // =================================
                // EVALUATE PREVIOUS MUTATIONS
                // =================================

                _logger.LogInformation(
                    "Evaluating previous mutations...");
                
                await mutationFeedbackService
                    .EvaluateAsync();
                
                _logger.LogInformation(
                    "Mutation evaluation complete.");

                // ==================================
                // STRATEGY EVOLUTION
                // ==================================

                using var scope2 =
                    _services.CreateScope();

                var db =
                    scope2.ServiceProvider
                        .GetRequiredService<SportsBettingDbContext>();

                var active =
                    await db.StrategyPerformances
                        .Where(x => x.IsActive)
                        .OrderBy(x => x.StrategyVersion)
                        .ToListAsync();

                _logger.LogInformation(
                    "========== ACTIVE STRATEGIES ==========");

                _logger.LogInformation(
                    "Running AI strategy evolution...");

                // ANALYZE WEIGHTS

                _logger.LogInformation(
                    "Analyzing strategy weights...");

                var recommendations =
                    await strategyWeightAnalyzer
                        .AnalyzeAsync();

                _logger.LogInformation(
                    "Weight analysis complete.");

                // BUILD MUTATION PLANS

                _logger.LogInformation("Building mutation plans...");

                var mutationPlans =
                    await strategyMutationService
                        .BuildMutationPlans(recommendations);

                _logger.LogInformation("Mutation plans built.");

                // FILTER CHAMPION

                //_logger.LogInformation("Filtering champions...");

                //mutationPlans =
                //    await championFilter
                //        .FilterAsync(mutationPlans);

                //_logger.LogInformation("Champion filtering complete.");

                // EXECUTE MUTATION EXECUTOR

                _logger.LogInformation("Executing mutations...");

                await mutationExecutor
                    .ExecuteAsync(mutationPlans);

                // APPLY MUTATIONS

                await mutationEngine
                    .ApplyMutationsAsync(mutationPlans);

                _logger.LogInformation("Mutation execution complete.");

                // ==================================
                // SELECT CHAMPION STRATEGY
                // ==================================

                _logger.LogInformation(
                    "Selecting champion strategy...");

                await championService
                    .SelectChampionsAsync();

                _logger.LogInformation(
                    "Selection of champion strategy completed...");

                // ======================================
                // POPULATION MANAGEMENT
                // ======================================

                _logger.LogInformation(
                    "Managing strategy populations...");

                var families =
                    mutationPlans
                        .Select(x => x.StrategyVersion)
                        .Distinct()
                        .ToList();

                foreach (var family in families)
                {
                    var retired =
                        await populationManager
                            .GetStrategiesToRetireAsync(
                                family,
                                5);

                    await populationManager
                        .RetireStrategiesAsync(
                            retired);
                }

                _logger.LogInformation(
                    "Population management complete.");

                _logger.LogInformation(
                    "Calculating evolution statistics...");

                await evolutionStatisticsService
                    .GetStatisticsAsync();

                _logger.LogInformation(
                    "Evolution statistics complete.");


                foreach (var plan in mutationPlans)
                {
                    _logger.LogInformation(
                        """
                        Mutation Plan

                        Strategy={strategy}

                        IncreaseForm={formUp}
                        DecreaseForm={formDown}

                        IncreaseGoals={goalUp}
                        DecreaseGoals={goalDown}

                        IncreaseConfidence={confUp}
                        DecreaseConfidence={confDown}

                        Replace={replace}

                        ExpectedImprovement={improvement:F2}

                        Reason={reason}
                        """,
                        plan.StrategyVersion,
                        plan.IncreaseFormWeight,
                        plan.DecreaseFormWeight,
                        plan.IncreaseGoalWeight,
                        plan.DecreaseGoalWeight,
                        plan.IncreaseConfidenceThreshold,
                        plan.ReduceConfidenceThreshold,
                        plan.ReplaceStrategy,
                        plan.ExpectedImprovement,
                        plan.Reason);
                }

                // ==================================
                // STRATEGY REPLACEMENT
                // ==================================

                _logger.LogInformation(
                    "Running automatic strategy replacement...");

                await replacementService
                    .ExecuteAsync();

                _logger.LogInformation(
                    "Strategy replacement complete.");

                var activeAfterReplacement =
                    await db.StrategyPerformances
                        .CountAsync(x => x.IsActive);

                _logger.LogInformation(
                    "Active strategies after replacement = {count}",
                    activeAfterReplacement);

                active =
                    await db.StrategyPerformances
                        .Where(x => x.IsActive)
                        .OrderBy(x => x.StrategyVersion)
                        .ToListAsync();

                foreach (var s in active)
                {
                    await mutationRecorder
                        .RecordLatestStrategyAsync(
                            s.StrategyVersion);

                    _logger.LogInformation(
                        "{strategy} | ROI={roi} | Active={active}",
                        s.StrategyVersion,
                        s.ROI,
                        s.IsActive);
                }

                //===================================
                // SETTLEMENT SERVICE
                //===================================

                _logger.LogInformation(
                    "Settling historical slips...");

                await settlementService
                    .SettleAsync();

                _logger.LogInformation(
                    "Slip settlement complete.");

                // ==================================
                // GENERATE PREDICTIONS
                // ==================================

                _logger.LogInformation(
                    "Generating prediction signals...");

                await predictionSignals
                    .GenerateAsync(
                        DateTime.UtcNow);

                _logger.LogInformation(
                    "Prediction generation complete.");

                // ==================================
                // GET TOP PICKS
                // ==================================

                _logger.LogInformation(
                    "Selecting top picks...");

                var picks =
                    await selector
                        .GetTopPicksAsync(
                            DateTime.UtcNow);

                _logger.LogInformation(
                    "Total picks selected: {count}",
                    picks.Count);

                foreach (var pick in picks)
                {
                    _logger.LogInformation(
                        "BEFORE GROUPING Match={match} Category={category} Stake={stake}",
                        pick.MatchId,
                        pick.Category,
                        pick.Stake);
                }

                // ==================================
                // ALLOCATE CATEGORIES
                // ==================================

                _logger.LogInformation(
                    "Allocating betting categories...");

                var categorized =
                    categoryAllocator
                        .Allocate(picks);

                _logger.LogInformation(
                    "Category allocation complete.");


                // ==================================
                // BUILD BETTING SLIPS
                // ==================================

                _logger.LogInformation(
                    "Building betting slips...");

                var slips =
                    slipBuilder
                        .Build(categorized);

                _logger.LogInformation(
                    "Total slips created: {count}",
                    slips.Count);

                // =====================================                
                // PERSIST SLIPS
                // =====================================

                _logger.LogInformation(
                    "Persisting slips to database...");

                await persistence
                    .SaveAsync(slips);

                _logger.LogInformation(
                    "Slip persistence complete.");

                // ==================================
                // LOG OUTPUT
                // ==================================

                foreach (var slip in slips)
                {
                    _logger.LogInformation(
                        "CATEGORY: {category}",
                        slip.Category);

                    _logger.LogInformation(
                        "STAKE: R{stake}",
                        slip.Stake);

                    _logger.LogInformation(
                        "ESTIMATED ODDS: {odds}",
                        slip.EstimatedOdds);

                    _logger.LogInformation(
                        "POTENTIAL PAYOUT: {payout}",
                        slip.Stake *
                        slip.EstimatedOdds);

                    foreach (var pick in slip.Picks)
                    {
                        _logger.LogInformation(
                            "{home} vs {away}",
                            pick.HomeTeam,
                            pick.AwayTeam);

                        _logger.LogInformation(
                            "Prediction: {prediction}",
                            pick.DisplayPrediction);

                        _logger.LogInformation(
                            "Confidence: {confidence}",
                            pick.Confidence);

                        _logger.LogInformation(
                            "Odds: {odds}",
                            pick.Odds);

                        _logger.LogInformation(
                            "Kickoff: {kickoff}",
                            pick.KickoffUtc);
                    }
                }

                _logger.LogInformation(
                    "Daily execution completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Execution pipeline failed");
            }

            // Run daily at 06:00 UTC

            var nextRun =
                DateTime.UtcNow
                .Date
                .AddDays(1)
                .AddHours(6);

            var delay =
                nextRun - DateTime.UtcNow;

            if (delay.TotalMilliseconds < 0)
            {
                delay = TimeSpan.FromHours(24);
            }

            _logger.LogInformation(
                "Next execution scheduled in {hours} hours",
                delay.TotalHours);

            await Task.Delay(
                delay,
                stoppingToken);
        }
    }

}
