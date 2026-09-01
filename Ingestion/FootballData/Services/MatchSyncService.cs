using Infrastructure.Data;
using Ingestion.Normalizers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;
using Ingestion.FootballData.DTOs;

namespace Ingestion.FootballData.Services;

public class MatchSyncService
{
    private readonly SportsBettingDbContext _db;

    private readonly FootballDataService _footballData;

    private readonly ILogger<MatchSyncService>
        _logger;

    public MatchSyncService(
        SportsBettingDbContext db,
        FootballDataService footballData,
        ILogger<MatchSyncService> logger)
    {
        _db = db;
        _footballData = footballData;
        _logger = logger;
    }

    public async Task SyncAsync()
    {
        var allowedCodes = new[]
        {
            "PL",
            "PD",
            "BL1",
            "SA",
            "FL1",
            "DED",
            "CL",
            "WC"
        };

        var competitions =
            await _db.Competitions
                .Where(x =>
                    allowedCodes.Contains(x.Code))
                .ToListAsync();                

        foreach (var competition in competitions)
        {

            if (string.IsNullOrWhiteSpace(
                    competition.Code))
            {
                continue;
            }

            _logger.LogInformation(
                "Loading matches for {code}",
                competition.Code);



            _logger.LogInformation(
                "Requesting matches for competition {code}",
                competition.Code);

            _logger.LogInformation(
                "Calling FootballData for competition {code}",
                competition.Code);

            List<MatchDto> matches;

            try
            {
                 matches =
                    await _footballData.GetMatchesAsync(
                        competition.Code); 
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to load matches for {code}",
                    competition.Code);

                continue;
            }

            _logger.LogCritical(
                "Competition {code} returned fixture ids: {ids}",
                competition.Code,
                    string.Join(",", matches
                    .Select(x => x.Id)
                    .Take(20)));



            _logger.LogInformation(
                "{count} matches returned",
                matches.Count);

            foreach (var dto in matches)
            {

                _logger.LogInformation(
                    "PROCESSING COMPETITION {code}",
                    competition.Code);


                _logger.LogInformation(
                    "Fixture {id} | {home} vs {away} | {date}",
                    dto.Id,
                    dto.HomeTeam?.Name,
                    dto.AwayTeam?.Name,
                    dto.UtcDate);


                if (dto.HomeTeam == null ||
                    dto.AwayTeam == null)
                {
                    _logger.LogWarning(
                        "Skipping match {id}. Missing team object.",
                        dto.Id);

                    continue;
                }

                if (string.IsNullOrWhiteSpace(
                        dto.HomeTeam.Name) ||
                    string.IsNullOrWhiteSpace(
                        dto.AwayTeam.Name))
                {
                    _logger.LogWarning(
                        "Bad match returned. Id={id} Home='{home}' Away='{away}'",
                        dto.Id,
                        dto.HomeTeam?.Name,
                        dto.AwayTeam?.Name);

                    _logger.LogWarning(
                        "Skipping match {id}. Missing team name.",
                        dto.Id);

                    continue;
                }

                var existing =
                    await _db.Matches
                        .FirstOrDefaultAsync(x =>
                            x.ExternalFixtureId ==
                            dto.Id);

                _logger.LogInformation(
                    "Updating existing match {fixtureId} with competition {competition}",
                    dto.Id,
                    competition.Code);


                // update
                if (existing != null)
                {
                    _logger.LogInformation(
                        "Skipping existing fixture {id}",
                        dto.Id);

                    existing.CompetitionId =
                        competition.Id;

                    existing.LeagueName =
                        competition.Name;

                    existing.KickoffUtc =
                        DateTime.SpecifyKind(
                            dto.UtcDate,
                            DateTimeKind.Utc);

                    _logger.LogCritical(
                        "FIXTURE={id} STATUS='{status}' HOME={home} AWAY={away}",
                        dto.Id,
                        dto.Status,
                        dto.Score?.FullTime?.Home,
                        dto.Score?.FullTime?.Away);

                    existing.Status =
                        MatchStatusMapper.Map(
                            dto.Status!);

                    existing.HomeScore =
                        dto.Score?.FullTime?.Home;

                    existing.AwayScore =
                        dto.Score?.FullTime?.Away;


                    _logger.LogInformation(
                        "Updated fixture {fixtureId} Status={status} Score={home}-{away}",
                        dto.Id,
                        existing.Status,
                        existing.HomeScore,
                        existing.AwayScore);

                    continue;
                }

                var homeTeam =
                    await GetOrCreateTeamAsync(
                        dto.HomeTeam.Name);

                var awayTeam =
                    await GetOrCreateTeamAsync(
                        dto.AwayTeam.Name);

                var match = 
                    new Match
                    {
                        Id = Guid.NewGuid(),

                        ExternalFixtureId =
                            dto.Id,

                        HomeTeamId =
                            homeTeam.Id,

                        AwayTeamId =
                            awayTeam.Id,

                        HomeTeam =
                            homeTeam,

                        AwayTeam =
                            awayTeam,

                        CompetitionId =
                            competition.Id,

                        Competition =
                            competition,

                        LeagueName =
                            competition.Name,

                        KickoffUtc =
                            DateTime.SpecifyKind(
                                dto.UtcDate,
                                DateTimeKind.Utc),

                        HomeScore =
                            dto.Score?.FullTime?.Home,

                        AwayScore =
                            dto.Score?.FullTime?.Away,

                        Status =
                            MatchStatusMapper.Map(
                                dto.Status!),

                        CreatedUtc =
                            DateTime.UtcNow
                    };
                _db.Matches.Add(match);

                _logger.LogInformation(
                    "Inserted fixture {home} vs {away}",
                    homeTeam.Name,
                    awayTeam.Name);
            }

            await _db.SaveChangesAsync();

            await Task.Delay(7000);

        }
    }

    private async Task<Team>
        GetOrCreateTeamAsync(
            string teamName)
    {
        if (string.IsNullOrWhiteSpace(teamName))
        {
            throw new Exception(
                "Attempted to create team with empty name.");
        }

        var team =
            await _db.Teams
                .FirstOrDefaultAsync(x =>
                    x.Name == teamName);

        if (team != null)
            return team;

        team = new Team
        {
            Id = Guid.NewGuid(),
            Name = teamName,
            CreatedUtc = DateTime.UtcNow
        };

        _db.Teams.Add(team);

        return team;
    }
}
