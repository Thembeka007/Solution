using Betting.Allocation;
using Betting.DTOs;
using Microsoft.Extensions.Logging;

namespace Betting.Services;

public class BettingSlipBuilder
{
    private readonly ILogger<BettingSlipBuilder>
        _logger;

    public BettingSlipBuilder(ILogger<BettingSlipBuilder> logger)
    {
        _logger = logger;
    }

    public List<BettingSlipDto>
        Build(
            Dictionary<string,
            List<DailyPickDto>> categorized)
    {
        var slips =
            new List<BettingSlipDto>();

        foreach (var category in categorized)
        {
            if (!category.Value.Any())
                continue;

            decimal odds = 1;

            foreach (var pick in category.Value)
            {
                _logger.LogInformation(
                    "SLIP BUILDER Match={match} PickStake={stake}",
                    pick.MatchId,
                    pick.Stake);

                odds *= pick.Odds;
            }

            decimal slipStake =
                category.Value.Sum(x => x.Stake);

            _logger.LogInformation(
                "SLIP BUILDER Category={category} TotalStake={stake} Picks={count}",
                category.Key,
                slipStake,
                category.Value.Count);


            slips.Add(
                new BettingSlipDto
                {
                    Category = category.Key,

                    Picks = category.Value,

                    EstimatedOdds = odds,

                    Stake = category.Value.Sum(
                        x => x.Stake)
                });
        }

        return slips;
    }
}
