using SharedKernel.Entities;

namespace Analytics.Validation;

public interface IPredictionValidator
{
    bool CanValidate(
    Prediction prediction);

    ValidationResult Validate(
        Prediction prediction,
        Match match);
}
