using SharedKernel.Entities;

namespace Analytics.Validation.Validators;

public class OverUnderValidator
    : IPredictionValidator
{
    public bool CanValidate(
    Prediction prediction)
    {
        return prediction.PredictionType ==
               "OVER_UNDER";
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

        int goals =
            match.HomeScore.Value +
            match.AwayScore.Value;

        bool over =
            goals > 2;

        bool won =
            (prediction.PredictedOutcome == "OVER" && over)
            ||
            (prediction.PredictedOutcome == "UNDER" && !over);

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
