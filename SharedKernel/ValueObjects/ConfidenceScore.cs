namespace SharedKernel.ValueObjects;

public class ConfidenceScore
{
    public decimal Value { get; }

    public ConfidenceScore(decimal value)
    {
        if (value < 0 || value > 100)
        {
            throw new ArgumentException(
                "Confidence must be between 0 and 100.");
        }

        Value = value;
    }
}