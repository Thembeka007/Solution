using Betting.DTOs;

namespace Betting.Selection;

public class KellyBankrollAllocator
{
    public void Allocate(
    Dictionary<string, List<DailyPickDto>> categorized,
    decimal bankroll)
    {
        var allPicks =
            categorized
                .SelectMany(x => x.Value)
                .ToList();

        if (!allPicks.Any())
            return;

        decimal totalKelly =
            allPicks.Sum(x =>
                x.KellyPercentage);

        if (totalKelly <= 0)
            return;

        foreach (var pick in allPicks)
        {
            decimal weight =
                pick.KellyPercentage /
                totalKelly;

            pick.Stake =
                bankroll * weight;
        }
    }
}
