using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Selection;

public class StrategySelectionService
{
    private readonly SportsBettingDbContext
        _db;

    public StrategySelectionService(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<bool>
        ShouldRunAsync(
            string strategyVersion)
    {
        var strategy =
            await _db.StrategyPerformances
                .FirstOrDefaultAsync(x =>
                    x.StrategyVersion ==
                    strategyVersion);

        if (strategy is null)
            return true;

        return strategy.IsActive;
    }
}
