using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace Backtesting.Engine;

public class HistoricalReplayEngine
{
    private readonly SportsBettingDbContext _db;

    public HistoricalReplayEngine(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<List<Prediction>>
        ReplayPredictionsAsync(
            DateTime from,
            DateTime to)
    {
        return await _db.Predictions
            .Include(x => x.Match)
            .Where(x =>
                x.Match != null &&
                x.Match.KickoffUtc >= from &&
                x.Match.KickoffUtc <= to)
            .OrderBy(x =>
                x.Match!.KickoffUtc)
            .ToListAsync();
    }
}
