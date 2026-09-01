using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Analytics.Validation;

public class PredictionValidationService
{
    private readonly SportsBettingDbContext
        _db;

    private readonly IEnumerable<IPredictionValidator>
        _validators;

    private readonly ILogger<PredictionValidationService>
        _logger;

    public PredictionValidationService(
        SportsBettingDbContext db,
        IEnumerable<IPredictionValidator> validators,
        ILogger<PredictionValidationService> logger)
    {
        _db = db;
        _validators = validators;
        _logger = logger;
    }

    public async Task ValidatePendingPredictionsAsync()
    {
        var predictions =
            await _db.Predictions
                .Include(x => x.Match)
                .Where(x => !x.IsSettled)
                .ToListAsync();

        _logger.LogInformation(
            "Validation found {count} unsettled predictions",
            predictions.Count);

        foreach (var prediction in predictions)
        {
            _logger.LogInformation(
                "Prediction {id} Tracking={state}",
                prediction.Id,
                _db.Entry(prediction).State);
        }

        foreach (var prediction in predictions)
        {
            var match =
                 await _db.Matches
                 .FirstOrDefaultAsync(x =>
                 x.Id == prediction.MatchId);

            if (match is null)
                continue;

            _logger.LogInformation(
                "Match Loaded | HomeScore={home} AwayScore={away}",
                match.HomeScore,
                match.AwayScore);

            var validator =
                _validators.FirstOrDefault(x =>
                    x.CanValidate(prediction));

            if (validator is null)
                continue;

            var result =
                validator.Validate(
                    prediction,
                    match);

            _logger.LogInformation(
                "Validator Returned | IsValidated={validated} Outcome={outcome} Profit={profit}",
                result.IsValidated,
                result.Outcome,
                result.Profit);

            if (!result.IsValidated)
                continue;

            _logger.LogInformation(
                "Prediction is being settled.");

            _logger.LogInformation(
                "Prediction settled Outcome={outcome} Profit={profit}",
                result.Outcome,
                result.Profit);

            prediction.IsSettled = true;

            prediction.IsCorrect =
                result.Outcome ==
                PredictionOutcome.Won;

            prediction.ProfitLoss =
                result.Profit;

            prediction.SettledUtc =
                DateTime.UtcNow;

            _logger.LogInformation(
                "After update State={state}",
                _db.Entry(prediction).State);
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Validation saved successfully");
    }
}
