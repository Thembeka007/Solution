namespace Analytics.Validation;

public class ValidationResult
{
    public PredictionOutcome Outcome { get; set; }

    public decimal Profit { get; set; }

    public bool IsValidated { get; set; }

    public string Reason { get; set; } = "";
}
