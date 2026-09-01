using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Betting.Services;

public class HistoricalEdgeAnalyzer
{
    private readonly SportsBettingDbContext _db;

    public HistoricalEdgeAnalyzer(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task PrintSummaryAsync()
    {
        var edges =
            await _db.MarketEdges
                .ToListAsync();

        if (!edges.Any())
            return;

        Console.WriteLine(
            $"Average Edge: {edges.Average(x => x.EdgePercentage):F2}%");

        Console.WriteLine(
            $"Average Fair Odds: {edges.Average(x => x.FairOdds):F2}");
    }
}
