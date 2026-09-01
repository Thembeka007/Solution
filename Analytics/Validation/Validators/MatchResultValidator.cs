using SharedKernel.Entities;

namespace Analytics.Validation.Validators;

public class MatchResultValidator
    : IPredictionValidator
{
    public bool CanValidate(
    Prediction prediction)
    {
        return prediction.PredictionType ==
               "MATCH_RESULT";
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
                    PredictionOutcome.Pending,

                Reason =
                    "Match not finished."
            };
        }

        string actual;

        if (match.HomeScore >
            match.AwayScore)
        {
            actual = "HOME_WIN";
        }
        else if (match.HomeScore <
                 match.AwayScore)
        {
            actual = "AWAY_WIN";
        }
        else
        {
            actual = "DRAW";
        }

        bool won =
            prediction.PredictedOutcome ==
            actual;

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
