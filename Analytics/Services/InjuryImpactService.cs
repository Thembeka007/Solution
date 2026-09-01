namespace Analytics.Services;

public class InjuryImpactService
{
    public decimal CalculateImpact(
    decimal playerRating)
    {
        if (playerRating >= 90)
            return -15;

        if (playerRating >= 80)
            return -10;

        if (playerRating >= 70)
            return -5;

        return 0;
    }
}
