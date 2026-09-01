using Betting.DTOs;
using Betting.Selection;
using Microsoft.Extensions.Logging;

namespace Betting.Services;

public class BetCategoryAllocator
{
    private readonly ILogger<BetCategoryAllocator>
    _logger;

    public BetCategoryAllocator(ILogger<BetCategoryAllocator> logger)
    {
        _logger = logger;
    }

    public Dictionary<string, List<DailyPickDto>>
        Allocate(
            List<DailyPickDto> picks)
    {

        foreach (var pick in picks)
        {
            _logger.LogInformation(
                "GROUPING Match={match} Category={category}",
                pick.MatchId,
                pick.Category);
        }


        return picks
            .GroupBy(x => x.Category)
            .ToDictionary(
                g => g.Key,
                g => g.ToList());
    }

}
