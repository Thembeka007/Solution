namespace AI.Optimization;

public class ReinforcementLearningService
{
    public decimal CalculateReward(
        bool won, decimal profit)
    {
        if (won)
        {
            return profit * 2;
        }

        return -Math.Abs(profit);
    }
}
