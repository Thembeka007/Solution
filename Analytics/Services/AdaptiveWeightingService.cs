using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Services;

public class AdaptiveWeightingService
{
    private readonly SportsBettingDbContext _db;

    public AdaptiveWeightingService(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<decimal>
        GetCategoryMultiplierAsync(
            string category)
    {
        var snapshots =
            await _db.PerformanceSnapshots
                .OrderByDescending(x =>
                    x.SnapshotDateUtc)
                .Take(30)
                .ToListAsync();

        if (!snapshots.Any())
            return 1.0m;

        decimal roi = category switch
        {
            "BREAD" =>
                snapshots.Average(x =>
                    x.BreadROI),

            "AVERAGE" =>
                snapshots.Average(x =>
                    x.AverageROI),

            "LOTTERY" =>
                snapshots.Average(x =>
                    x.LotteryROI),

            _ => 0
        };

        // =====================================
        // POSITIVE ROI
        // =====================================

        if (roi >= 25)
            return 1.50m;

        if (roi >= 15)
            return 1.30m;

        if (roi >= 5)
            return 1.15m;

        // =====================================
        // NEGATIVE ROI
        // =====================================

        if (roi <= -25)
            return 0.40m;

        if (roi <= -15)
            return 0.60m;

        if (roi <= -5)
            return 0.80m;

        return 1.0m;
    }
}
