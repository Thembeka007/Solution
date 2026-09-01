using SharedKernel.Entities;

namespace Analytics.Validation.Validators;

public class BttsValidator
    : IPredictionValidator
{
    public bool CanValidate(
    Prediction prediction)
    {
        return prediction.PredictionType ==
               "BTTS";
    }

    public ValidationResult Validate(
     Prediction prediction,
     Match match)
    {
        if (match.HomeScore is null ||
            match.AwayScore is null)
        {
            return new ValidationResult
            {
                Outcome =
                    PredictionOutcome.Pending
            };
        }

        bool btts =
            match.HomeScore > 0 &&
            match.AwayScore > 0;

        bool won =
            (prediction.PredictedOutcome == "YES" && btts)
            ||
            (prediction.PredictedOutcome == "NO" && !btts);

        return new ValidationResult
        {
            Outcome =
                won
                    ? PredictionOutcome.Won
                    : PredictionOutcome.Lost,

            Profit =
                won
                    ? prediction.ClosingOdds - 1
                    : -1,

            IsValidated = true
        };
    }
}
