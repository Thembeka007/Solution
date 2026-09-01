using Analytics.Services;
using Betting.DTOs;

namespace Betting.Selection;

public class BankrollAllocator
{

    private readonly AdaptiveWeightingService
    _adaptive;

    public BankrollAllocator(
        AdaptiveWeightingService adaptive)
    {
        _adaptive = adaptive;
    }
    public async Task AllocateStakeAsync(
    List<DailyPickDto> picks,
    decimal dailyBudget)
    {
        var bread =
            picks.Where(x =>
                x.Category == "BREAD")
            .ToList();

        var average =
            picks.Where(x =>
                x.Category == "AVERAGE")
            .ToList();

        var lottery =
            picks.Where(x =>
                x.Category == "LOTTERY")
            .ToList();

        // =====================================
        // ADAPTIVE MULTIPLIERS
        // =====================================

        decimal breadMultiplier =
            await _adaptive
                .GetCategoryMultiplierAsync(
                    "BREAD");

        decimal averageMultiplier =
            await _adaptive
                .GetCategoryMultiplierAsync(
                    "AVERAGE");

        decimal lotteryMultiplier =
            await _adaptive
                .GetCategoryMultiplierAsync(
                    "LOTTERY");
        // =====================================
        // BASE CATEGORY ALLOCATION
        // =====================================

        decimal breadBudget =
            (dailyBudget * 0.6m)
            * breadMultiplier;

        decimal averageBudget =
            (dailyBudget * 0.3m)
            * averageMultiplier;

        decimal lotteryBudget =
            (dailyBudget * 0.1m)
            * lotteryMultiplier;

        // =====================================
        // HARD CAPS
        // =====================================

        decimal total =
            breadBudget +
            averageBudget +
            lotteryBudget;

        if (total > dailyBudget)
        {
            decimal ratio =
                dailyBudget / total;

            breadBudget *= ratio;
            averageBudget *= ratio;
            lotteryBudget *= ratio;
        }

        // =====================================
        // BREAD
        // =====================================

        if (bread.Any())
        {
            foreach (var pick in bread)
            {
                pick.Stake =
                    Math.Round(
                        breadBudget /
                        bread.Count,
                        2);
            }
        }

        // =====================================
        // AVERAGE
        // =====================================

        if (average.Any())
        {
            foreach (var pick in average)
            {
                pick.Stake =
                    Math.Round(
                        averageBudget /
                        average.Count,
                        2);
            }
        }

        // =====================================
        // LOTTERY
        // =====================================

        if (lottery.Any())
        {
            foreach (var pick in lottery)
            {
                pick.Stake =
                    Math.Round(
                        lotteryBudget /
                        lottery.Count,
                        2);
            }
        }
    }
}