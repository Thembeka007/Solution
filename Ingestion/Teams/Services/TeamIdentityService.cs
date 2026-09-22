using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Ingestion.Teams.Services;

public class TeamIdentityService
{
    private readonly SportsBettingDbContext _db;

    private readonly ILogger<TeamIdentityService> _logger;

    public TeamIdentityService(
        SportsBettingDbContext db,
        ILogger<TeamIdentityService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Team?> ResolveAsync(
        string source,
        string? externalTeamId,
        string externalName)
    {
        if (string.IsNullOrWhiteSpace(source) ||
            string.IsNullOrWhiteSpace(externalName))
        {
            return null;
        }

        // -----------------------------------------------------
        // FIRST: stable external ID
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(externalTeamId))
        {
            var mappingById =
                await _db.TeamExternalMappings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Source == source &&
                        x.ExternalTeamId == externalTeamId &&
                        x.IsActive);

            if (mappingById != null)
            {
                return await
                    _db.Teams
                        .FirstOrDefaultAsync(x =>
                            x.Id == mappingById.TeamId);

            }

        }

        // -----------------------------------------------------
        // SECOND: previously registered external name
        // -----------------------------------------------------

        var normalizedName =
            NormalizeName(externalName);

        var mappings =
            await _db.TeamExternalMappings
                .AsNoTracking()
                .Where(x =>
                    x.Source == source &&                    
                    x.IsActive)
                .ToListAsync();

        var mappingByName =
            mappings
                .FirstOrDefault(x =>
                    NormalizeName(x.ExternalName) ==
                    normalizedName);

        if (mappingByName == null)
        {
            return null;
        
        }

        return await
            _db.Teams
                .FirstOrDefaultAsync(x =>
                    x.Id == mappingByName.TeamId);

    }

    // =========================================================
    // REGISTER MAPPING
    // =========================================================

    public async Task<TeamExternalMapping?>
        RegisterAsync(
            Team team,
            string source,
            string? externalTeamId,
            string externalName)
    {
        if (team == null ||
            string.IsNullOrWhiteSpace(source) ||
            string.IsNullOrWhiteSpace(externalName))
        {
            return null;
        }

        var normalizedExternalName =
            NormalizeName(externalName);

        // -----------------------------------------------------
        // DO NOT CREATE DUPLICATES
        // -----------------------------------------------------

        var existingMappings =
            await _db.TeamExternalMappings
                .Where(x =>
                    x.TeamId == team.Id &&
                    x.Source == source)
                .ToListAsync();

        var existing =
            existingMappings.FirstOrDefault(x =>
                (
                    !string.IsNullOrWhiteSpace(externalTeamId) &&
                    x.ExternalTeamId == externalTeamId
                )
                ||
                NormalizeName(x.ExternalName) ==
                    normalizedExternalName);

        _logger.LogCritical(
            """
            FOOTBALL-DATA MAPPING CHECK

            Source={source}
            ExternalTeamId={externalId}
            ExternalName={externalName}
            TeamId={teamId}
            ExistingMapping={exists}
            ExistingMappingTeamId={existingTeamId}
            """,
            source,
            externalTeamId,
            externalName,
            team.Id,
            existing != null,
            existing?.TeamId);

        if (existing != null)
        {
            existing.LastSeenUtc =
                DateTime.UtcNow;

            existing.IsActive =
                true;

            if (!string.IsNullOrWhiteSpace(
                externalTeamId) &&
                string.IsNullOrWhiteSpace(
                    existing.ExternalTeamId))
            {
                existing.ExternalTeamId =
                    externalTeamId;
            }

            await _db.SaveChangesAsync();

            return existing;

        }

        // -----------------------------------------------------
        // CREATE NEW MAPPING
        // -----------------------------------------------------

        var mapping =
            new TeamExternalMapping
            {
                TeamId =
                    team.Id,

                Source =
                    source,

                ExternalTeamId =
                    externalTeamId,

                ExternalName =
                    externalName.Trim(),

                FirstSeenUtc =
                    DateTime.UtcNow,

                LastSeenUtc =
                    DateTime.UtcNow,

                IsActive =
                    true

            };

        _db.TeamExternalMappings.Add(
            mapping);

        await _db.SaveChangesAsync();

        return mapping;
    }

    // =========================================================
    // FIND CANONICAL TEAM BY NAME
    // =========================================================

    public async Task<Team?> FindCanonicalTeamAsync(
        string externalName)
    {
        if (string.IsNullOrWhiteSpace(externalName))
        {
            return null;
        
        }

        var normalizedExternalName =
            NormalizeName(externalName);

        var teams =
            await _db.Teams
                .AsNoTracking()
                .ToListAsync();

        return teams.FirstOrDefault(team =>
            NormalizeName(team.Name) ==
            normalizedExternalName);


    }

    // =========================================================
    // REGISTER PROVIDER FIXTURE TEAMS
    // =========================================================

    public async Task RegisterFixtureMappingsAsync(
        Match match,
        string source,
        string providerHomeName,
        string providerAwayName,
        string? providerHomeTeamId = null,
        string? providerAwayTeamId = null)
    {
        if (match == null ||
            string.IsNullOrWhiteSpace(source) ||
            string.IsNullOrWhiteSpace(providerHomeName) ||
            string.IsNullOrWhiteSpace(providerAwayName))
        {
            return;
        }

        var homeTeam =
            await _db.Teams
                .FirstOrDefaultAsync(x =>
                    x.Id == match.HomeTeamId);

        var awayTeam =
            await _db.Teams
                .FirstOrDefaultAsync(x =>
                    x.Id == match.AwayTeamId);

        if (homeTeam == null ||
            awayTeam == null)
        {
            return;
        }

        await RegisterAsync(
            homeTeam,
            source,
            providerHomeTeamId,
            providerHomeName);

        await RegisterAsync(
            awayTeam,
            source,
            providerAwayTeamId,
            providerAwayName);

    }


    // =========================================================
    // NORMALIZE ONLY FOR IDENTITY REGISTRATION / LOOKUP
    // =========================================================

    private static string NormalizeName(
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        return new string(
            name
                .Trim()
                .ToUpperInvariant()
                .Where(char.IsLetterOrDigit)
                .ToArray());
    }
}
