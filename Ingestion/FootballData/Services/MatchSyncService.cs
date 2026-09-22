using Infrastructure.Data;
using Ingestion.FootballData.DTOs;
using Ingestion.Normalizers;
using Ingestion.Teams.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Ingestion.FootballData.Services;

public class MatchSyncService
{
    private readonly SportsBettingDbContext _db;

    private readonly FootballDataService _footballData;

    private readonly TeamIdentityService
        _teamIdentityService;

    private readonly ILogger<MatchSyncService>
        _logger;

    public MatchSyncService(
        SportsBettingDbContext db,
        FootballDataService footballData,
        TeamIdentityService teamIdentityService,
        ILogger<MatchSyncService> logger)
    {
        _db = db;
        _footballData = footballData;
        _teamIdentityService = teamIdentityService;
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

            _logger.LogCritical(
                "STARTING COMPETITION SYNC {code} | {name}",
                competition.Code,
                competition.Name);

            if (string.IsNullOrWhiteSpace(
                    competition.Code))
            {
                continue;
            }

            _logger.LogInformation(
                "Loading matches for {code}",
                competition.Code);

            List<MatchDto> matches;

            try
            {
                 matches =
                    await _footballData.GetMatchesAsync(
                        competition.Code);

                _logger.LogCritical(
                    "COMPLETED FOOTBALLDATA REQUEST {code} | Matches={count}",
                    competition.Code,
                    matches.Count);
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
                // Log you are looking for
                _logger.LogInformation(
                    "Competition {code} returned {count} matches",
                    competition.Code,
                    matches.Count);


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


                    _logger.LogCritical(
                        """
                        FOOTBALL-DATA TEAM IDENTITY

                        HomeId={homeId}
                        HomeName={homeName}

                        AwayId={awayId}
                        AwayName={awayName}
                        """,
                        dto.HomeTeam.Id,
                        dto.HomeTeam.Name,
                        dto.AwayTeam.Id,
                        dto.AwayTeam.Name);


                    await _teamIdentityService
                        .RegisterFixtureMappingsAsync(
                            existing,
                            "FootballData",
                            dto.HomeTeam.Name,
                            dto.AwayTeam.Name,
                            dto.HomeTeam.Id?.ToString(),
                            dto.AwayTeam.Id?.ToString());

                    continue;
                }


                var homeTeam =
                    await GetOrCreateTeamAsync(
                        dto.HomeTeam);

                var awayTeam =
                    await GetOrCreateTeamAsync(
                        dto.AwayTeam);

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

        _logger.LogCritical(
            """
            ========================================
            MATCH SYNC CYCLE COMPLETE

            CompetitionsProcessed={count}

            ========================================
            """,
            competitions.Count);
    }

    private async Task<Team>
        GetOrCreateTeamAsync(
            TeamDto teamDto)
    {
        if (teamDto == null ||
            string.IsNullOrWhiteSpace(teamDto.Name))
        {
            throw new Exception(
                "Attempted to create team with empty identity.");
        }

        var trackedTeam =
            _db.Teams.Local
                .FirstOrDefault(x =>
                    x.Name == teamDto.Name);

        if (trackedTeam != null)
        {
            return trackedTeam;
        }


        // =========================================
        // FOOTBALL-DATA IDENTITY
        // =========================================

        if (teamDto.Id.HasValue)
        {
            var existingMapping =
                await _db.TeamExternalMappings
                    .FirstOrDefaultAsync(x =>
                        x.Source == "FootballData" &&
                        x.ExternalTeamId ==
                            teamDto.Id.Value.ToString() &&
                        x.IsActive);

            if (existingMapping != null)
            {
                var mappedTeam =
                    await _db.Teams
                        .FirstOrDefaultAsync(x =>
                            x.Id ==
                            existingMapping.TeamId);

                if (mappedTeam != null)
                {
                    existingMapping.LastSeenUtc =
                        DateTime.UtcNow;

                    existingMapping.ExternalName =
                        teamDto.Name.Trim();

                    await _db.SaveChangesAsync();

                    return mappedTeam;
                }
            }
        }


        var team =
            await _db.Teams
                .FirstOrDefaultAsync(x =>
                    x.Name == teamDto.Name);

        if (team == null)
        {
            team =
                new Team
                {
                    Id = Guid.NewGuid(),
                    Name = teamDto.Name.Trim(),
                    CreatedUtc = DateTime.UtcNow
                };

            _db.Teams.Add(team);
          
        }

        // =========================================
        // REGISTER FOOTBALL-DATA IDENTITY
        // =========================================

        if (teamDto.Id.HasValue)
        {
            var mapping =
                await _db.TeamExternalMappings
                    .FirstOrDefaultAsync(x =>
                        x.Source == "FootballData" &&
                        x.ExternalTeamId ==
                            teamDto.Id.Value.ToString());

            if (mapping == null)
            {
                _db.TeamExternalMappings.Add(
                    new TeamExternalMapping
                    {
                        TeamId = team.Id,

                        Source = "FootballData",

                        ExternalTeamId =
                            teamDto.Id.Value.ToString(),

                        ExternalName =
                            teamDto.Name.Trim(),

                        FirstSeenUtc =
                            DateTime.UtcNow,

                        LastSeenUtc =
                            DateTime.UtcNow,

                        IsActive = true
                    });
            }
            else
            {
                mapping.TeamId =
                    team.Id;

                mapping.ExternalName =
                    teamDto.Name.Trim();

                mapping.LastSeenUtc =
                    DateTime.UtcNow;

                mapping.IsActive =
                    true;
            }
        }

        return team;
    }
}
