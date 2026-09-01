namespace Analytics.SmartMoney;

public class ConfidenceBoostService
{
    public decimal ApplyBoost(
    decimal baseConfidence,
    decimal smartMoneyStrength)
    {
        if (smartMoneyStrength >= 80)
            return Math.Min(
                baseConfidence + 12,
                99);

        if (smartMoneyStrength >= 60)
            return Math.Min(
                baseConfidence + 8,
                99);

        if (smartMoneyStrength >= 40)
            return Math.Min(
                baseConfidence + 4,
                99);

        return baseConfidence;
    }
}
