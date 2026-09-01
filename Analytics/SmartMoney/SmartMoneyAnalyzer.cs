using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Analytics.SmartMoney;

public class SmartMoneyAnalyzer
{
    private readonly SportsBettingDbContext _db;

    public SmartMoneyAnalyzer(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<List<Tipster>>
        GetSmartMoneyTipstersAsync()
    {
        var tipsters =
            await _db.Tipsters
                .ToListAsync();

        return tipsters
            .Where(x =>
                x.WinRate >= 70 &&
                x.ROI >= 10 &&
                x.TotalPredictions >= 50)
            .OrderByDescending(x =>
                x.Weight)
            .ToList();
    }
}
