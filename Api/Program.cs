using AI.Evolution;
using AI.Evolution.Laboratory;
using AI.Evolution.Laboratory.Services;
using AI.Evolution.Risk;
using AI.Optimization;
using Analytics.Portfolio;
using Analytics.Ranking;
using Analytics.Selection;
using Analytics.Services;
using Analytics.Services.Calibration;
using Analytics.SmartMoney;
using Analytics.Strategies;
using Analytics.Validation;
using Analytics.Validation.Validators;
using Backtesting.Engine;
using Betting.Allocation;
using Betting.Analysis;
using Betting.Configuration;
using Betting.Odds.Services;
using Betting.Selection;
using Betting.Services;
using Betting.Settlement;
using Infrastructure.Data;
using Ingestion.FootballData.Services;
using Ingestion.Odds.Abstractions;
using Ingestion.Odds.Configuration;
using Ingestion.Odds.Providers;
using Ingestion.Odds.Services;
using Ingestion.Services;
using Ingestion.Workers;
using Management.Services;
using Management.Workers;
using Microsoft.EntityFrameworkCore;
using Polly;
using Serilog;
using SharedKernel.Entities;

var builder =
    WebApplication.CreateBuilder(args);

Log.Logger =
    new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File(
            "logs/app-.txt",
            rollingInterval: RollingInterval.Day)
        .CreateLogger();

builder.Host.UseSerilog();

// ======================================================
// DATABASE
// ======================================================

builder.Services.AddDbContext<
    SportsBettingDbContext>(
    options =>
    {
        options.UseNpgsql(
            builder.Configuration
            .GetConnectionString(
                "DefaultConnection"));
    });

var connectionString =
    builder.Configuration
    .GetConnectionString("DefaultConnection");

Console.WriteLine(
    $"Connection String = {connectionString}");

// ======================================================
// CONFIGURATION FOR LIVE ODDS
// ======================================================

builder.Services.Configure<OddsApiOptions>(
    builder.Configuration.GetSection(
        "OddsApi"));

// =====================================================
// BANKROLL CONFIGURATION
// =====================================================

builder.Services.Configure<BankrollOptions>(
    builder.Configuration
    .GetSection(BankrollOptions.SectionName));


builder.Services.Configure<ScoreWeightsOptions>(
    builder.Configuration
    .GetSection("ScoreWeights"));


// ======================================================
// HTTP CLIENTS
// ======================================================

builder.Services.AddHttpClient();

builder.Logging.AddSimpleConsole();

// ======================================================
// INGESTION SERVICES
// ======================================================

builder.Services.AddScoped<
    FootballApiService>();

builder.Services.AddScoped<
    TeamFormGenerator>();

builder.Services.AddScoped<
    OddsSyncService>();

builder.Services.AddScoped<
    IOddsProvider,
    OddsApiProvider>();

builder.Services.AddScoped<
    CompetitionSyncService>();

builder.Services.AddScoped<
    MatchSyncService>();

builder.Services.AddScoped<
    MatchRefreshService>();

builder.Services.AddHttpClient<
    FootballDataService>()
    .AddTransientHttpErrorPolicy(policy =>
        policy.WaitAndRetryAsync(
            3,
            retry =>
                TimeSpan.FromSeconds(
                    Math.Pow(2, retry))));

// ======================================================
// ANALYTICS SERVICES
// ======================================================

builder.Services.AddScoped<
    PredictionSignalService>();

builder.Services.AddScoped<
    PredictionSettlementService>();

builder.Services.AddScoped<
    ClosingLineValueService>();

builder.Services.AddScoped<
    TrueFormAnalyzer>();

builder.Services.AddScoped<
    SmartMoneyConsensusService>();

builder.Services.AddScoped<
    ConfidenceEngine>();

builder.Services.AddScoped<
    ConfidenceBoostService>();

builder.Services.AddScoped<
    PickRankingService>();

builder.Services.AddScoped<
    ConfidenceCalibrationService>();

builder.Services.AddScoped<
    ConfidenceCalibrationBuilder>();

builder.Services.AddScoped<
    AdaptiveWeightingService>();

builder.Services.AddScoped<
    IPredictionStrategy,
    BttsPredictionStrategy>();

builder.Services.AddScoped<
    IPredictionStrategy,
    MomentumPredictionStrategy>();

builder.Services.AddScoped<
    IPredictionStrategy,
    Over25PredictionStrategy>();

builder.Services.AddScoped<
    IPredictionStrategy,
    DrawPredictionStrategy>();

builder.Services.AddScoped<
    StrategyPerformanceService>();

builder.Services.AddScoped<
    FormScoringService>();

builder.Services.AddScoped<
    HeadToHeadService>();

builder.Services.AddScoped<
    TeamFormSyncService>();

builder.Services.AddScoped<
    HomeAdvantageService>();

builder.Services.AddScoped<
    BttsRankingService>();

builder.Services.AddScoped<
    PredictionValidationService>();

builder.Services.AddScoped<
    IPredictionValidator,
    MatchResultValidator>();

builder.Services.AddScoped<
    IPredictionValidator,
    BttsValidator>();

builder.Services.AddScoped<
    IPredictionValidator,
    OverUnderValidator>();

builder.Services.AddScoped<
    PortfolioService>();

builder.Services.AddScoped<
    GoalScoringService>();

builder.Services.AddScoped<
    MatchResultRankingService>();

builder.Services.AddScoped<
    Over25RankingService>();

builder.Services.AddScoped<
    DrawRankingService>();

builder.Services.AddScoped<
    StrategySelectionService>();

builder.Services.AddScoped<
    StrategyWeightAnalyzer>();

builder.Services.AddScoped<
    StrategyMutationService>();

builder.Services.AddScoped<
    ActiveStrategyService>();


// ======================================================
// BETTING SERVICES
// ======================================================

builder.Services.AddScoped<
    DailyPickSelector>();

builder.Services.AddScoped<
    BetCategoryAllocator>();

builder.Services.AddScoped<
    BettingSlipBuilder>();

builder.Services.AddScoped<
    BankrollAllocator>();

builder.Services.AddScoped<
    AdaptiveBankrollService>();

builder.Services.AddScoped<
    ExpectedValueCalculator>();

builder.Services.AddScoped<
    OddsResolver>();

builder.Services.AddScoped<
    KellyCriterionCalculator>();

builder.Services.AddScoped<
    KellyBankrollAllocator>();

builder.Services.AddScoped<
    PickCategoryService>();

builder.Services.AddScoped<
    StakeAllocationService>();

builder.Services.AddScoped<
    ScoreWeightsOptions>();

builder.Services.AddScoped<
    BookmakerMarketAnalyticsService>();

builder.Services.AddScoped<
    MarketDispersionAnalyzer>();

builder.Services.AddScoped<
    ConsensusExperimentService>();

// =====================================================
// BACKTESTING SERVICES
// =====================================================

builder.Services.AddScoped<
    HistoricalReplayEngine>();

builder.Services.AddScoped<
    StrategySimulationEngine>();

builder.Services.AddScoped<
    ProfitCurveAnalyzer>();

builder.Services.AddScoped<
    HistoricalConfidenceAnalyzer>();

builder.Services.AddScoped<
    StrategyComparisonService>();

builder.Services.AddScoped<
    AutoStrategyReplacementService>();

// ======================================================
// AI OPTIMIZATION SERVICES
// ======================================================

builder.Services.AddScoped<
    FeatureEngineeringService>();

builder.Services.AddScoped<
    PredictionOptimizationService>();

builder.Services.AddScoped<
    ConfidenceTuningService>();

builder.Services.AddScoped<
    StrategyMutationEngine>();

builder.Services.AddScoped<
    ReinforcementLearningService>();

builder.Services.AddScoped<
    ROIOptimizationService>();

builder.Services.AddScoped<
    DynamicWeightingEngine>();

builder.Services.AddScoped<
    AnomalyDetectionService>();

builder.Services.AddScoped<
    StrategyMutationExecutor>();

builder.Services.AddScoped<
    StrategyChampionService>();

builder.Services.AddScoped<
    ChampionMutationFilter>();

builder.Services.AddScoped<
    AdaptiveMutationEngine>();

builder.Services.AddScoped<
    MutationOutcomeRecorder>();

builder.Services.AddScoped<
    MutationFeedbackService>();

builder.Services.AddScoped<
    MutationLearningService>();

builder.Services.AddScoped<
    ParameterSuccessAnalyzer>();

builder.Services.AddScoped<
    GenomeBuilderService>();

builder.Services.AddScoped<
    GenomeCloneService>();

builder.Services.AddScoped<
    PopulationManager>();

builder.Services.AddScoped<
    StrategyFamilyService>();

builder.Services.AddScoped<
    CrossoverService>();

builder.Services.AddScoped<
    ParentSelectionService>();

builder.Services.AddScoped<
    DiversityService>();

builder.Services.AddScoped<
    RandomGenomeService>();

builder.Services.AddScoped<
    FitnessEvaluationService>();

builder.Services.AddScoped<
    AdaptiveMutationService>();

builder.Services.AddScoped<
    HistoricalMarketConditionAnalyzer>();

builder.Services.AddScoped<
    HistoricalPredictionMarketAnalyzer>();

builder.Services.AddScoped<
    HistoricalTotalsMarketConditionAnalyzer>();

builder.Services.AddScoped<
    LaboratoryMarketDataEvaluator>();

builder.Services.AddScoped<
    LaboratoryDatasetEvaluator>();

builder.Services.AddScoped<
    EvolutionLaboratoryEvaluationService>();

builder.Services.AddScoped<
    EvolutionLaboratoryService>();

builder.Services.AddScoped<
    StrategyRiskMetricsService>();

builder.Services.AddScoped<
    LaboratoryWindowService>();

builder.Services.AddScoped<
    LaboratoryConsistencyService>();

builder.Services.AddScoped<
    HistoricalMarketEdgeAnalyzer>();

builder.Services.AddScoped<
    MarketEdgeOutcomeAnalyzer>();

builder.Services.AddScoped<
    ConfidenceMarketEdgeAnalyzer>();

// ======================================================
// MANAGEMENT SERVICES
// ======================================================

builder.Services.AddScoped<
    PerformanceAnalyzer>();

builder.Services.AddScoped<
    DailySlipPersistenceService>();

builder.Services.AddScoped<
    SlipSettlementService>();

builder.Services.AddScoped<
    EvolutionStatisticsService>();

// ======================================================
// INFRASTRUCTURE SERVICES
// ======================================================

builder.Services.AddScoped<
    StrategyParameterService>();

// ======================================================
// BACKGROUND WORKERS
// ======================================================

builder.Services.AddHostedService<
    MatchIngestionWorker>();

builder.Services.AddHostedService<
    DailyExecutionWorker>();

builder.Services.AddHostedService<
    OddsIngestionWorker>();

// =============================================
// Once - off Singleton service
// =============================================

builder.Services.AddSingleton<
    LaboratoryEvaluationSettings>();


// ======================================================
// API + SWAGGER  // AddOpenApi for now
// ======================================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();

//builder.Services.AddSwaggerGen();

var app = builder.Build();

// ======================================================
// MIDDLEWARE
// ======================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet(
    "/calibration/build",
    async (
        ConfidenceCalibrationBuilder builder) =>
    {
        var model =
            await builder.BuildAsync();

        return Results.Ok(model);
    });

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();

//    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// =====================================================
// APPLICATION START
// =====================================================

app.Run();