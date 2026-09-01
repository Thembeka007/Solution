namespace AI.Optimization;

public class AdaptiveBankrollService
{
    public decimal CalculateStake(
        decimal bankroll,
        decimal confidence)
    {
        if (confidence >= 90)
        {
            return bankroll * 0.08m;
        }

        if (confidence >= 80)
        {
            return bankroll * 0.05m;
        }

        return bankroll * 0.02m;
    }
}
