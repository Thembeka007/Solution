using Analytics.DTOs;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;
using SharedKernel.Enums;

namespace Analytics.Services;

public class BacktestService
{
    private readonly SportsBettingDbContext
        _db;

    public BacktestService(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task RunAsync()
    {
        var groups =
            await _db.BettingSlipSelections
                .Where(x => x.Settled)
                .GroupBy(x =>
                    x.StrategyVersion)
                .ToListAsync();

        foreach (var group in groups)
        {
            int wins =
                group.Count(x =>
                    x.Won == true);

            int losses =
                group.Count(x =>
                    x.Won == false);

            int bets =
                wins + losses;

            decimal winRate =
                bets == 0
                    ? 0
                    : wins * 100m / bets;

            Console.WriteLine(
                $"Strategy={group.Key}");

            Console.WriteLine(
                $"Bets={bets}");

            Console.WriteLine(
                $"Wins={wins}");

            Console.WriteLine(
                $"Losses={losses}");

            Console.WriteLine(
                $"WinRate={winRate:F2}%");
        }
    }
}
