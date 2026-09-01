using Infrastructure.Data;
using Ingestion.Normalizers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Enums;

namespace Ingestion.FootballData.Services;

public class MatchRefreshService
{
    private readonly SportsBettingDbContext _db;

    private readonly FootballDataService _footballData;

    private readonly ILogger<MatchRefreshService>
        _logger;

    public MatchRefreshService(
        SportsBettingDbContext db,
        FootballDataService footballData,
        ILogger<MatchRefreshService> logger)
    {
        _db = db;
        _footballData = footballData;
        _logger = logger;
    }

    public async Task RefreshAsync()
    {
        var matches =
            await _db.Matches
                .Where(x =>
                    x.Status != MatchStatus.Completed &&
                    x.KickoffUtc < DateTime.UtcNow)
                .ToListAsync();

        _logger.LogInformation(
            "Refreshing {count} stale matches",
            matches.Count);

        foreach (var match in matches)
        {
            try
            {
                var dto =
                    await _footballData
                        .GetMatchAsync(
                            match.ExternalFixtureId);

                if (dto == null)
                    continue;

                match.Status =
                    MatchStatusMapper.Map(
                        dto.Status!);

                match.HomeScore =
                    dto.Score?.FullTime?.Home;

                match.AwayScore =
                    dto.Score?.FullTime?.Away;

                _logger.LogInformation(
                    "Fixture {id} refreshed. Status={status} Score={home}-{away}",
                    match.ExternalFixtureId,
                    match.Status,
                    match.HomeScore,
                    match.AwayScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed refreshing fixture {id}",
                    match.ExternalFixtureId);
            }

            await Task.Delay(250);
        }

        await _db.SaveChangesAsync();
    }
}
