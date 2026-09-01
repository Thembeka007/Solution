using Betting.DTOs;
using Microsoft.Extensions.Logging;
using SharedKernel.Enums;

namespace Betting.Allocation;

public class StakeAllocationService
{
    private readonly ILogger<StakeAllocationService>
        _logger;

    public StakeAllocationService(ILogger<StakeAllocationService> logger)
    {
        _logger = logger;
    }
    public void Allocate(
        List<DailyPickDto> picks,
        decimal dailyBudget)
    {


        var bread =
            picks.Where(x =>
                x.Category ==
                "BREAD")
            .ToList();

        var average =
            picks.Where(x =>
                x.Category ==
                "AVERAGE")
            .ToList();

        var lottery =
            picks.Where(x =>
                x.Category ==
                "LOTTERY")
            .ToList();

        _logger.LogInformation(
            "CATEGORY COUNTS Bread={bread} Average={average} Lottery={lottery}",
            bread.Count,
            average.Count,
            lottery.Count); 

        _logger.LogInformation(
            "DAILY BUDGET={budget}",
            dailyBudget);

        _logger.LogInformation(
            "BREAD PICKS={count}",
            bread.Count);

        _logger.LogInformation(
            "Bread={bread} Average={average} Lottery={lottery}",
            bread.Count,
            average.Count,
            lottery.Count);

        decimal breadBudget =
            dailyBudget * 0.60m;

        decimal averageBudget =
            dailyBudget * 0.30m;

        decimal lotteryBudget =
            dailyBudget * 0.10m;

        if (bread.Any())
        {

            _logger.LogInformation(
                "DailyBudget={budget}",
                dailyBudget);

            _logger.LogInformation(
                "BreadBudget={budget}",
                breadBudget);

            _logger.LogInformation(
                "BreadCount={count}",
                bread.Count);

            decimal stake =
                breadBudget /
                bread.Count;

            _logger.LogInformation(
                "Calculated stake={stake}",
                stake);

            _logger.LogInformation(
                "BREAD BUDGET={budget} STAKE={stake}",
                breadBudget,
                stake);


            foreach (var pick in bread)
            {

                _logger.LogInformation(
                    "SETTING PICK STAKE Match={match} Stake={stake}",
                    pick.MatchId,
                    pick.Stake);

                _logger.LogInformation(
                    "ALLOCATING Match={match} Category={category}",
                    pick.MatchId,
                    pick.Category);

                pick.Stake =
                    Math.Round(stake, 2);


                _logger.LogInformation(
                    "AFTER ALLOCATION Match={match} Stake={stake}",
                    pick.MatchId,
                    pick.Stake);
            }
        }

        if (average.Any())
        {
            decimal stake =
                averageBudget /
                average.Count;

            foreach (var pick in average)
            {
                pick.Stake =
                    Math.Round(stake, 2);

                _logger.LogInformation(
                    "AFTER ALLOCATION Match={match} Stake={stake}",
                    pick.MatchId,
                    pick.Stake);
            }
        }

        if (lottery.Any())
        {
            decimal stake =
                lotteryBudget /
                lottery.Count;

            foreach (var pick in lottery)
            {
                pick.Stake =
                    Math.Round(stake, 2);

                _logger.LogInformation(
                    "AFTER ALLOCATION Match={match} Stake={stake}",
                    pick.MatchId,
                    pick.Stake);
            }
        }
    }
}
