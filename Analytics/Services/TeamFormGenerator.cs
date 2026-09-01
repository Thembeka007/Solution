using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Analytics.Services;

public class TeamFormGenerator
{
    private readonly SportsBettingDbContext _db;

    private readonly ILogger<TeamFormGenerator>
    _logger;

    public TeamFormGenerator(
        SportsBettingDbContext db,
        ILogger<TeamFormGenerator> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task GenerateAsync(
        Match match)
    {
        // PREVENT DUPLICATES

        var homeExists =
            await _db.TeamForms
                .AnyAsync(x =>
                    x.MatchId == match.Id &&
                    x.TeamId == match.HomeTeamId);

        var awayExists =
            await _db.TeamForms
                .AnyAsync(x =>
                    x.MatchId == match.Id &&
                    x.TeamId == match.AwayTeamId);

        // ================
        // HOME TEAM FORM
        // ================

        var homeForm =
            new TeamForm
            {
                Id = Guid.NewGuid(),

                TeamId =
                    match.HomeTeamId,

                MatchId =
                    match.Id,

                TeamName =
                    match.HomeTeam?.Name ?? "",

                MatchDateUtc =
                    match.KickoffUtc,

                GoalsFor =
                    match.HomeScore ?? 0,

                GoalsAgainst =
                    match.AwayScore ?? 0,

                BTTS =
                    match.HomeScore > 0 &&
                    match.AwayScore > 0,

                Over25 =
                    (match.HomeScore ?? 0)
                    +
                    (match.AwayScore ?? 0)
                    > 2,

                Points =
                    CalculatePoints(
                        match.HomeScore ?? 0,
                        match.AwayScore ?? 0),

                IsHomeGame = true,

                CreatedUtc =
                    DateTime.UtcNow
            };

        // =================
        // AWAY TEAM FORM
        // =================

        var awayForm =
            new TeamForm
            {
                Id = Guid.NewGuid(),

                TeamId =
                    match.AwayTeamId,

                MatchId =
                    match.Id,

                TeamName =
                    match.AwayTeam?.Name ?? "",

                MatchDateUtc =
                    match.KickoffUtc,

                GoalsFor =
                    match.AwayScore ?? 0,

                GoalsAgainst =
                    match.HomeScore ?? 0,

                BTTS =
                    match.HomeScore > 0 &&
                    match.AwayScore > 0,

                Over25 =
                    (match.HomeScore ?? 0)
                    +
                    (match.AwayScore ?? 0)
                    > 2,

                Points =
                    CalculatePoints(
                        match.AwayScore ?? 0,
                        match.HomeScore ?? 0),

                IsHomeGame = false,

                CreatedUtc =
                    DateTime.UtcNow
            };

        _logger.LogInformation(
            "GENERATING FORM | Match={match} Home={home} Away={away} Status={status}",
            match.Id,
            match.HomeTeam?.Name,
            match.AwayTeam?.Name,
            match.Status);

        if (!homeExists)
        {
            _db.TeamForms.Add(homeForm);
        }

        if (!awayExists)
        {
            _db.TeamForms.Add(awayForm);
        }

        await _db.SaveChangesAsync();
    }

    private int CalculatePoints(
        int goalsFor,
        int goalsAgainst)
    {
        if (goalsFor > goalsAgainst)
            return 3;

        if (goalsFor == goalsAgainst)
            return 1;

        return 0;
    }
}
