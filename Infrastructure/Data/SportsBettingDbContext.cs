using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace Infrastructure.Data;

public class SportsBettingDbContext : DbContext
{
    public SportsBettingDbContext(
        DbContextOptions<SportsBettingDbContext> options)
        : base(options)
    {
    }

    // =========================
    // CORE FOOTBALL ENTITIES
    // =========================

    public DbSet<Match> Matches
        => Set<Match>();

    public DbSet<Team> Teams
        => Set<Team>();

    public DbSet<Player> Players
        => Set<Player>();
    public DbSet<PlayerInjury> PlayerInjuries
        => Set<PlayerInjury>();

    public DbSet<Competition> Competitions
        => Set<Competition>();

    // =========================
    // FORM
    // =========================

    public DbSet<TeamForm> TeamForms
        => Set<TeamForm>();

    public DbSet<PerformanceSnapshot>
        PerformanceSnapshots
        => Set<PerformanceSnapshot>();

    // =========================
    // PREDICTIONS
    // =========================

    public DbSet<Prediction> Predictions
        => Set<Prediction>();

    public DbSet<Strategy> Strategies
        => Set<Strategy>();

    public DbSet<StrategyPerformance>
        StrategyPerformances
        => Set<StrategyPerformance>();

    public DbSet<StrategyReplacement>
        StrategyReplacements
        => Set<StrategyReplacement>();

    // =========================
    // BETTING
    // =========================

    public DbSet<Bet> Bets
        => Set<Bet>();

    public DbSet<BetResult> BetResults
        => Set<BetResult>();

    public DbSet<BettingSlip> BettingSlips
        => Set<BettingSlip>();

    public DbSet<BettingSlipSelection>
        BettingSlipSelections
        => Set<BettingSlipSelection>();

    public DbSet<MatchOdds> MatchOdds
    => Set<MatchOdds>();

    public DbSet<MarketEdge> MarketEdges
        => Set<MarketEdge>();


    // =========================
    // SMART MONEY
    // =========================

    public DbSet<SmartMoneyTipster>
        SmartMoneyTipsters
            => Set<SmartMoneyTipster>();

    public DbSet<SmartMoneyPick> SmartMoneyPicks
            => Set<SmartMoneyPick>();

    // =========================
    // Tipsters
    // =========================

    public DbSet<Tipster> Tipsters
        => Set<Tipster>();

    public DbSet<TipsterPrediction>
        TipsterPredictions
            => Set<TipsterPrediction>();

    // =========================
    // Backtesting
    // =========================

    public DbSet<BacktestResult>
        BacktestResults
            => Set<BacktestResult>();

    // =========================
    // Confidence
    // =========================

    public DbSet<ConfidenceCalibration>
        ConfidenceCalibrations { get; set; }

    // =========================
    // Strategies
    // =========================

    public DbSet<StrategyParameter>
        StrategyParameters
            => Set<StrategyParameter>();

    public DbSet<StrategyParameterHistory>
        StrategyParameterHistories
            => Set<StrategyParameterHistory>();

    public DbSet<MutationOutcome>
        MutationOutcomes
            => Set<MutationOutcome>();

    public DbSet<PendingMutation>
        PendingMutations
            => Set<PendingMutation>();

    // =========================
    // MODEL CONFIGURATION
    // =========================

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BettingSlipSelection>()
            .HasOne(x => x.BettingSlip)
            .WithMany(x => x.Selections)
            .HasForeignKey(x => x.BettingSlipId);

        modelBuilder.Entity<StrategyParameter>()
            .HasIndex(x =>
            new
            {
                x.StrategyVersion,
                x.ParameterName
            })
            .IsUnique();

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SportsBettingDbContext)
                .Assembly);
    }
}