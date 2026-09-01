using Betting.DTOs;

namespace Betting.Analysis;

public class StakeSizingService
{
    private readonly KellyCriterionCalculator
    _kelly;

    public StakeSizingService(
        KellyCriterionCalculator kelly)
    {
        _kelly = kelly;
    }

    public void CalculateStakes(
        List<DailyPickDto> picks,
        decimal bankroll)
    {
        foreach (var pick in picks)
        {
            decimal kelly =
                _kelly.Calculate(
                    pick.Confidence,
                    pick.Odds);

            pick.KellyPercentage =
                kelly;

            pick.RecommendedStake =
                bankroll * kelly;
        }
    }
}
