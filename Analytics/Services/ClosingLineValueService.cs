namespace Analytics.Services;

public class ClosingLineValueService
{
    public decimal Calculate(
        decimal openingOdds,
        decimal closingOdds)
    {
        if (openingOdds <= 0 ||
            closingOdds <= 0)
        {
            return 0;
        }

        return
            ((openingOdds -
              closingOdds)
              /
              openingOdds)
              * 100m;
    }
}
