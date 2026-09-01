using Infrastructure.Entities;

namespace Backtesting.Engine;

public class StrategyComparisonService
{
    public BacktestResult?
        GetBestStrategy(
        List<BacktestResult> results)
    {
        return results
            .OrderByDescending(x =>
                x.ROI)
            .ThenByDescending(x =>
                x.WinRate)
            .FirstOrDefault();
    }
}
