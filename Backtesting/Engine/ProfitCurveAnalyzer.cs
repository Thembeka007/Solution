namespace Backtesting.Engine;

public class ProfitCurveAnalyzer
{
    public decimal CalculateMaxDrawdown(
    List<decimal> equityCurve)
    {
        decimal peak = equityCurve.First();

        decimal maxDrawdown = 0;

        foreach (var point in equityCurve)
        {
            if (point > peak)
            {
                peak = point;
            }

            var drawdown =
                (peak - point) / peak;

            if (drawdown > maxDrawdown)
            {
                maxDrawdown = drawdown;
            }
        }

        return maxDrawdown * 100m;
    }
}
