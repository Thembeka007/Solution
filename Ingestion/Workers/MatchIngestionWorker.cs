using Analytics.Services;
using Infrastructure.Data;
using Ingestion.DTOs;
using Ingestion.FootballData.Services;
using Ingestion.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;
using System.Diagnostics;
using System.Text.Json;

namespace Ingestion.Workers;

public class MatchIngestionWorker
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<MatchIngestionWorker>
        _logger;

    private readonly FootballDataService _footballDataService;

    public MatchIngestionWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<MatchIngestionWorker> logger,
        FootballDataService footballDataService)
    {
        _scopeFactory = scopeFactory;

        _logger = logger;

        _footballDataService = footballDataService;

    }

    [DebuggerStepThrough]
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {

        _logger.LogInformation(
            "Testing Football-Data competitions endpoint");

        var competitions =
            await _footballDataService
                .GetCompetitionsAsync();

        _logger.LogInformation(
            "Competitions returned: {count}",
            competitions.Count);

        foreach (var c in competitions.Take(20))
        {
            _logger.LogInformation(
                "{id} | {name} | {code} | {country}",
                c.Id,
                c.Name,
                c.Code,
                c.Area.Name);
        }

        using var scope =
            _scopeFactory.CreateScope();

        var competitionSyncService =
            scope.ServiceProvider
                .GetRequiredService<
                    CompetitionSyncService>();

        await competitionSyncService.SyncAsync();

        _logger.LogInformation(
            "Competition sync completed");

        var matchSyncService =
            scope.ServiceProvider
                .GetRequiredService<
                    MatchSyncService>();        
        try
        {
            await matchSyncService.SyncAsync(); ;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Match sync failed");
        }

        _logger.LogInformation(
            "Match sync completed");

        return;





        //while (!stoppingToken
        //    .IsCancellationRequested)
        //{
        //    try
        //    {
        //        using var scope =
        //            _scopeFactory.CreateScope();

        //        var footballApi =
        //            scope.ServiceProvider
        //                .GetRequiredService<
        //                    FootballApiService>();

        //        var db =
        //            scope.ServiceProvider
        //                .GetRequiredService<
        //                    SportsBettingDbContext>();

        //        _logger.LogInformation(
        //            "Running ingestion at {time}",
        //            DateTime.UtcNow);

        //        // DOWNLOAD FIXTURES

        //        _logger.LogInformation(
        //            "Requesting fixtures for {date}",
        //            DateTime.UtcNow.ToString("yyyy-MM-dd"));

        //        var json =
        //            await footballApi
        //                .GetFixturesAsync(
        //                    DateTime.UtcNow);

        //        _logger.LogInformation(
        //                "Fixtures downloaded at: {0}",
        //                DateTime.Now);

        //        // DESERIALIZE RESPONSE            

        //        var dto = JsonSerializer.
        //            Deserialize<
        //                FixtureResponseDto>(
        //            json,
        //            new JsonSerializerOptions
        //            {
        //                PropertyNameCaseInsensitive = true
        //            });

        //        Console.WriteLine(dto?.Response.Count ?? 0);

        //        _logger.LogInformation(
        //            "Events returned: {count}",
        //            dto!.Response.Count);

        //        if (dto!.Errors.ValueKind == JsonValueKind.Array)
        //        {
        //            if (dto.Errors.GetArrayLength() > 0)
        //            {
        //                throw new Exception($"API returned errors: {dto.Errors}");
        //            }
        //        }
        //        else if (dto.Errors.ValueKind == JsonValueKind.Object)
        //        {
        //            if (dto.Errors.EnumerateObject().Any())
        //            {
        //                throw new Exception($"API returned errors: {dto.Errors}");
        //            }
        //        }

        //        _logger.LogInformation(
        //            "Response count = {count}",
        //            dto?.Response?.Count ?? -1);

        //        if (dto == null ||
        //            dto.Response == null ||
        //            !dto.Response.Any())
        //        {
        //            _logger.LogWarning(
        //                "No fixtures returned");

        //            await Task.Delay(
        //                TimeSpan.FromMinutes(15),
        //                stoppingToken);

        //            continue;
        //        }

        //        foreach (var fixture
        //            in dto.Response)
        //        {
        //            // =========================
        //            // HOME TEAM UPSERT
        //            // =========================

        //            var homeTeam =
        //                await db.Teams
        //                    .FirstOrDefaultAsync(x =>
        //                        x.Name ==
        //                        fixture.Teams
        //                            .Home
        //                            .Name);

        //            if (homeTeam == null)
        //            {
        //                homeTeam =
        //                    new Team
        //                    {
        //                        Id = Guid.NewGuid(),

        //                        Name =
        //                            fixture.Teams
        //                                .Home
        //                                .Name,

        //                        CreatedUtc =
        //                            DateTime.UtcNow
        //                    };

        //                db.Teams.Add(homeTeam);

        //                _logger.LogInformation(
        //                    "Created team: {team}",
        //                    homeTeam.Name);
        //            }

        //            // =========================
        //            // AWAY TEAM UPSERT
        //            // =========================

        //            var awayTeam =
        //                await db.Teams
        //                    .FirstOrDefaultAsync(x =>
        //                        x.Name ==
        //                        fixture.Teams
        //                            .Away
        //                            .Name);

        //            if (awayTeam == null)
        //            {
        //                awayTeam =
        //                    new Team
        //                    {
        //                        Id = Guid.NewGuid(),

        //                        Name =
        //                            fixture.Teams
        //                                .Away
        //                                .Name,

        //                        CreatedUtc =
        //                            DateTime.UtcNow
        //                    };

        //                db.Teams.Add(awayTeam);

        //                _logger.LogInformation(
        //                    "Created team: {team}",
        //                    awayTeam.Name);
        //            }

        //            // =========================
        //            // PREVENT DUPLICATE MATCHES
        //            // =========================

        //            var existingMatch =
        //                await db.Matches
        //                    .FirstOrDefaultAsync(x =>
        //                        x.ExternalFixtureId ==
        //                        fixture.Fixture.Id);

        //            if (existingMatch != null)
        //            {
        //                _logger.LogInformation(
        //                    "Match already exists");

        //                existingMatch.HomeScore =
        //                    fixture.Goals.Home;

        //                existingMatch.AwayScore =
        //                    fixture.Goals.Away;

        //                continue;
        //            }

        //            _logger.LogInformation(
        //                "Fixture {id} Status={status} Goals={home}-{away}",
        //                fixture.Fixture.Id,
        //                fixture.Fixture.Status.Short,
        //                fixture.Goals.Home,
        //                fixture.Goals.Away);

        //            _logger.LogInformation(
        //                "Fixture {id} Goals: {home}-{away}",
        //                fixture.Fixture.Id,
        //                fixture.Goals.Home,
        //                fixture.Goals.Away);

        //            // =========================
        //            // CREATE MATCH
        //            // =========================

        //            var match =
        //                new Match
        //                {
        //                    Id = Guid.NewGuid(),

        //                    ExternalFixtureId =
        //                        fixture.Fixture.Id,

        //                    HomeTeamId =
        //                        homeTeam.Id,

        //                    AwayTeamId =
        //                        awayTeam.Id,

        //                    HomeTeam =
        //                        homeTeam,

        //                    AwayTeam =
        //                        awayTeam,

        //                    KickoffUtc =
        //                        fixture.Fixture.Date
        //                        .UtcDateTime,

        //                    HomeScore =
        //                        fixture.Goals.Home,

        //                    AwayScore =
        //                        fixture.Goals.Away,

        //                    LeagueName =
        //                        fixture.League.Name,

        //                    CreatedUtc =
        //                        DateTime.UtcNow
        //                };

        //            db.Matches.Add(match);

        //            _logger.LogInformation(
        //                "Created match: {home} vs {away}",
        //                homeTeam.Name,
        //                awayTeam.Name);

        //            // =========================
        //            // GENERATE TEAM FORM
        //            // ONLY FOR COMPLETED MATCHES
        //            // =========================

        //            if (match.HomeScore != null &&
        //                match.AwayScore != null)
        //            {
        //                var formGenerator =
        //                    scope.ServiceProvider
        //                        .GetRequiredService<
        //                            TeamFormGenerator>();

        //                await formGenerator
        //                    .GenerateAsync(match);

        //                _logger.LogInformation(
        //                    "Generated TeamForms");
        //            }
        //        }

        //        // SAVE DATABASE CHANGES

        //        foreach (var entry in db.ChangeTracker.Entries())
        //        {
        //            foreach (var property in entry.Properties)
        //            {
        //                if (property.CurrentValue is DateTime dt)
        //                {
        //                    Console.WriteLine(
        //                        $"{entry.Entity.GetType().Name}.{property.Metadata.Name} = {dt} | Kind={dt.Kind}");
        //                }
        //            }
        //        }

        //        await db.SaveChangesAsync();

        //        _logger.LogInformation(
        //            "Ingestion cycle completed successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(
        //            ex,
        //            "Ingestion failed");
        //    }

        //    // WAIT BEFORE NEXT INGESTION CYCLE

        //    await Task.Delay(
        //        TimeSpan.FromMinutes(15),
        //        stoppingToken);
        //}
    }
}
