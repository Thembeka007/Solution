using Infrastructure.Data;
using Ingestion.Odds.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ingestion.Workers;

public class OddsIngestionWorker : BackgroundService
{
    private readonly IServiceProvider _services;

    private readonly ILogger<OddsIngestionWorker>
        _logger;

    public OddsIngestionWorker(
        IServiceProvider services,
        ILogger<OddsIngestionWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Odds ingestion worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay =
                    await RunOddsCycleAsync(
                        stoppingToken);

                _logger.LogInformation(
                    "Next odds synchronization in {minutes} minutes.",
                    delay.TotalMinutes);

                await Task.Delay(
                    delay,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Odds ingestion cycle failed.");

                // Don't let a provider/API failure kill
                // the autonomous worker.
                await Task.Delay(
                    TimeSpan.FromMinutes(15),
                    stoppingToken);
            }
        }

        _logger.LogInformation(
            "Odds ingestion worker stopped.");
    }

    private async Task<TimeSpan> RunOddsCycleAsync(
        CancellationToken stoppingToken)
    {
        using var scope =
            _services.CreateScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<
                    SportsBettingDbContext>();

        var oddsSync =
            scope.ServiceProvider
                .GetRequiredService<
                    OddsSyncService>();

        var now =
            DateTime.UtcNow;

        // ==========================================
        // FIND UPCOMING MATCHES
        // ==========================================

        var horizon =
           now.AddHours(72);

        var upcomingMatches =
            await db.Matches
                .Where(x =>
                    x.KickoffUtc > now
                    &&
                    x.KickoffUtc <= horizon)
                .OrderBy(x =>
                    x.KickoffUtc)
                .ToListAsync(
                    stoppingToken);

        // ==========================================
        // FIND NEAREST KICKOFF
        // ==========================================

        var nearestKickoff =
            upcomingMatches
                .Select(x => x.KickoffUtc)
                .FirstOrDefault();        

        TimeSpan timeUntilKickoff =
            nearestKickoff == default
                ? TimeSpan.Zero
                : nearestKickoff - now;

        // =======================================================================
        // DETERMINE SPORTS REQUIRING ODDS SYNC
        //
        // Reduce the number of API calls, one call per league
        // =======================================================================

        var sportKeys =
            upcomingMatches
                .Select(x =>
                    GetSportKey(x.LeagueName))
                .Where(x => x != null)
                .Distinct()
                .ToList();

        // ==========================================
        // LOG CYCLE DATA
        // ==========================================

        _logger.LogInformation(
            """
            ODDS INGESTION CYCLE:

            CurrentUtc={now}

            HorizonUtc={horizon}

            UpcomingMatches={count}

            NearestKickoffUtc={kickoff}

            MinutesUntilKickoff={minutes}

            SportsRequiringSync={sports}
            """,
            now,
            horizon,
            upcomingMatches.Count,
            nearestKickoff == default
                ? null
                : nearestKickoff,
            timeUntilKickoff.TotalMinutes,
            string.Join(
                ", ",
                sportKeys));


        // ==========================================
        // SYNCHRONIZE ODDS
        // ==========================================

        foreach (var sportKey in sportKeys)
        {
            stoppingToken
                .ThrowIfCancellationRequested();

            _logger.LogInformation(
                "Synchronizing odds for sport {sport}",
                sportKey);

            await oddsSync
                .SyncAsync(sportKey!);
        }

        _logger.LogInformation(
            "Odds synchronization completed for {count} sports.",
            sportKeys.Count);

        // ==========================================
        // TEMPORARY NEXT-CYCLE INTERVAL
        // ==========================================
        //
        // We deliberately do NOT decide the final
        // polling strategy here yet.
        //
        // The worker will continue checking the
        // database regularly so newly ingested
        // fixtures cannot leave us sleeping for
        // an entire day.
        //
        // This value will later be replaced by the
        // configurable kickoff-aware polling policy.
        // ==========================================

        return TimeSpan.FromMinutes(30);
    }
    private static string? GetSportKey(
        string leagueName)
    {
        return leagueName switch
        {
            "Premier League" =>
                  "soccer_epl",

            "EPL" =>
                "soccer_epl",

            "Primera Division" =>
                "soccer_spain_la_liga",

            "La Liga" =>
                "soccer_spain_la_liga",

            "Bundesliga" =>
                "soccer_germany_bundesliga",

            "Serie A" =>
                "soccer_italy_serie_a",

            "Ligue 1" =>
                "soccer_france_ligue_one",

            "Eredivisie" =>
                "soccer_netherlands_eredivisie",

            "UEFA Champions League" =>
                "soccer_uefa_champs_league",

            "FIFA World Cup" =>
                "soccer_fifa_world_cup",

            _ => null
        };
    }
}
