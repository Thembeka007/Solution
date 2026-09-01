namespace Betting.Analysis;

public class KellyCriterionCalculator
{
    public decimal Calculate(
    decimal confidence,
    decimal odds,
    decimal fraction = 0.25m)
    {
        decimal p =
            confidence / 100m;

        decimal b =
            odds - 1m;

        if (b <= 0)
            return 0;

        decimal q =
            1 - p;

        decimal kelly =
            ((b * p) - q) / b;

        if (kelly < 0)
            return 0;     

        return Math.Min(
            kelly * fraction,
            0.10m);
    }
}
