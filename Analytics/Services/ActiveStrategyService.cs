using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Services;

public class ActiveStrategyService
{
    private readonly SportsBettingDbContext _db;

    public ActiveStrategyService(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<string> GetMomentumVersionAsync()
    {
        var strategy =
            await _db.StrategyPerformances
                .Where(x =>
                    x.IsActive &&
                    x.StrategyVersion.StartsWith("MOMENTUM"))
                .OrderByDescending(x =>
                    x.LastEvaluationUtc)
                .FirstOrDefaultAsync();

        return strategy?.StrategyVersion
            ?? "MOMENTUM_V1";
    }
}
