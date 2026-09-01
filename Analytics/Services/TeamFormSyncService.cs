using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace Analytics.Services;

public class TeamFormSyncService
{
    private readonly SportsBettingDbContext _db;
    private readonly TeamFormGenerator _generator;

    public TeamFormSyncService(
        SportsBettingDbContext db,
        TeamFormGenerator generator)
    {
        _db = db;
        _generator = generator;
    }

    public async Task SyncAsync()
    {
        var matches =
            await _db.Matches
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .Where(x =>
                    x.Status ==
                    MatchStatus.Completed)
                .ToListAsync();

        foreach (var match in matches)
        {
            await _generator
                .GenerateAsync(match);
        }
    }
}
