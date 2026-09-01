using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Analytics.Services.Calibration;

public class ConfidenceCalibrationBuilder
{
    private readonly SportsBettingDbContext
        _db;

    private readonly ILogger<ConfidenceCalibrationBuilder>
        _logger;

    public ConfidenceCalibrationBuilder(
        SportsBettingDbContext db,
        ILogger<ConfidenceCalibrationBuilder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ConfidenceCalibrationModel>
       BuildAsync()
    {
        var model =
            new ConfidenceCalibrationModel();

        var settled =
            await _db.Predictions
                .Where(x =>
                    x.IsSettled)
                .ToListAsync();

        _logger.LogInformation(
            "Calibration Builder analysing {count} settled predictions",
            settled.Count);

        for (decimal lower = 50m;
             lower < 95m;
             lower += 5m)
        {
            decimal upper = lower + 5m;

            var bucket =
                settled.Where(x =>
                    x.Confidence >= lower &&
                    x.Confidence < upper)
                .ToList();

            var band =
                new ConfidenceBand
                {
                    LowerBound =
                        lower,

                    UpperBound =
                        upper,

                    Predictions =
                        bucket.Count,

                    Correct =
                        bucket.Count(x =>
                            x.IsCorrect == true)
                };

            model.Bands.Add(band);

            _logger.LogInformation(
                "Band {lower}-{upper} | Predictions={predictions} Correct={correct} Accuracy={accuracy:F2}",
                lower,
                upper,
                band.Predictions,
                band.Correct,
                band.Accuracy);
        }

        return model;
    }
}
