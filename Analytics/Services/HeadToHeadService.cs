using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Services;

public class HeadToHeadService
{
    private readonly SportsBettingDbContext
        _db;

    public HeadToHeadService(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<HeadToHeadStats>
        GetStatsAsync(
            Guid homeTeamId,
            Guid awayTeamId)
    {
        var matches =
            await _db.Matches
                .Where(x =>

                    (
                        x.HomeTeamId ==
                        homeTeamId

                        &&

                        x.AwayTeamId ==
                        awayTeamId
                    )

                    ||

                    (
                        x.HomeTeamId ==
                        awayTeamId

                        &&

                        x.AwayTeamId ==
                        homeTeamId
                    ))
                .Where(x =>
                    x.HomeScore != null
                    &&
                    x.AwayScore != null)
                .OrderByDescending(x =>
                    x.KickoffUtc)
                .Take(5)
                .ToListAsync();

        var stats =
            new HeadToHeadStats();

        stats.Matches =
            matches.Count;

        stats.BttsCount =
            matches.Count(x =>
                x.HomeScore > 0
                &&
                x.AwayScore > 0);

        stats.Over25Count =
            matches.Count(x =>
                (x.HomeScore ?? 0)
                +
                (x.AwayScore ?? 0)
                > 2);

        stats.HomeWins =
            matches.Count(x =>
                x.HomeScore >
                x.AwayScore);

        stats.AwayWins =
            matches.Count(x =>
                x.AwayScore >
                x.HomeScore);

        stats.Draws =
            matches.Count(x =>
                x.HomeScore ==
                x.AwayScore);

        stats.AverageGoals =
            matches.Any()
                ? matches.Average(x =>
                (x.HomeScore ?? 0) +
                (x.AwayScore ?? 0))
                : 0;

        stats.AverageHomeGoals =
            matches.Any()
                ? matches.Average(x =>
                x.HomeScore ?? 0)
                : 0;

        stats.AverageAwayGoals =
            matches.Any()
                ? matches.Average(x =>
                x.AwayScore ?? 0)
                : 0;

        stats.AverageGoalDifference =
            matches.Any()
                ? matches.Average(x =>
                    Math.Abs(
                        (x.HomeScore ?? 0) -
                        (x.AwayScore ?? 0)))
                        : 0;

        return stats;
    }
}
