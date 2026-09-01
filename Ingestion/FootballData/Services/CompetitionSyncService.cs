using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace Ingestion.FootballData.Services;

public class CompetitionSyncService
{
    private readonly SportsBettingDbContext _db;

    private readonly FootballDataService _footballData;

    public CompetitionSyncService(
        SportsBettingDbContext db,
        FootballDataService footballData)
    {
        _db = db;
        _footballData = footballData;
    }

    public async Task SyncAsync()
    {
        var competitions =
            await _footballData
                .GetCompetitionsAsync();

        foreach (var item in competitions)
        {
            bool exists =
                await _db.Set<Competition>()
                    .AnyAsync(x =>
                        x.ExternalCompetitionId ==
                        item.Id);

            if (exists)
                continue;

            _db.Add(
                new Competition
                {
                    ExternalCompetitionId = item.Id,
                    Name = item.Name,
                    Code = item.Code,
                    Country = item.Area.Name
                });
        }

        await _db.SaveChangesAsync();
    }
}
