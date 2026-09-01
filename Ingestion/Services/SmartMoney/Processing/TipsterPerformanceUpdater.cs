using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ingestion.Services.SmartMoney.Processing;

public class TipsterPerformanceUpdater
{
    private readonly SportsBettingDbContext _db;

    public TipsterPerformanceUpdater(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task UpdateAsync()
    {
        var tipsters =
            await _db.Tipsters
                .ToListAsync();

        var detector =
            new SmartMoneyDetector();

        var calculator =
            new TipsterWeightCalculator();

        foreach (var tipster in tipsters)
        {
            tipster.IsSmartMoney =
                detector.IsSmartMoney(
                    tipster);

            tipster.Weight =
                calculator.Calculate(
                    tipster);

            tipster.LastUpdatedUtc =
                DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }
}
