namespace Betting.Analysis;

public class ExpectedValueCalculator
{
    public decimal Calculate(
    decimal confidence,
    decimal odds)
    {
        decimal probability =
            confidence / 100m;

        decimal ev =
            (probability * odds) - 1m;

        return ev;
    }
}
