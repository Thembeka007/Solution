using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace Analytics.Services;

public class ConfidenceCalibrationService
{
    private readonly SportsBettingDbContext _db;

    public ConfidenceCalibrationService(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task RecalculateAsync()
    {
        var settledPredictions =
            await _db.Predictions
                .Where(x =>
                    x.IsSettled &&
                    x.Confidence > 0)
                .ToListAsync();

        var grouped =
            settledPredictions
                .GroupBy(x =>
                    NormalizeBand(
                        x.Confidence));

        foreach (var group in grouped)
        {
            decimal band =
                group.Key;

            int total =
                group.Count();

            int correct =
                group.Count(x => (bool)x.IsCorrect!);

            decimal accuracy = 0;

            if (total > 0)
            {
                accuracy =
                    ((decimal)correct /
                    total) * 100;
            }

            decimal adjustment =
                accuracy - band;

            adjustment =
                Math.Clamp(
                    adjustment,
                    -5m, 5m);

            if (total < 50)
            {
                continue;
            }

            var existing =
                await _db
                    .ConfidenceCalibrations
                    .FirstOrDefaultAsync(x =>
                        x.ConfidenceBand ==
                        band);

            if (existing is null)
            {
                existing =
                    new ConfidenceCalibration
                    {
                        ConfidenceBand =
                            band
                    };

                _db.ConfidenceCalibrations
                    .Add(existing);
            }

            existing.TotalPredictions =
                total;

            existing.CorrectPredictions =
                correct;

            existing.ActualAccuracy =
                accuracy;

            existing.CalibrationAdjustment =
                adjustment;

            existing.LastUpdatedUtc =
                DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<decimal>
        ApplyCalibrationAsync(
            decimal confidence)
    {
        decimal band =
            NormalizeBand(confidence);

        var calibration =
            await _db
                .ConfidenceCalibrations
                .FirstOrDefaultAsync(x =>
                    x.ConfidenceBand ==
                    band);

        if (calibration is null)
            return confidence;

        if (calibration.TotalPredictions < 50)
            return confidence;

        decimal adjustment =
            calibration.CalibrationAdjustment;

        adjustment =
            Math.Clamp(
                adjustment,
                -10m,
                10m);

        decimal adjusted =
            confidence +
            adjustment;

        if (adjusted > 99)
            adjusted = 99;

        if (adjusted < 1)
            adjusted = 1;

        return adjusted;
    }

    private decimal NormalizeBand(
        decimal confidence)
    {
        return Math.Floor(
            confidence / 10) * 10;
    }
}
