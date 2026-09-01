using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtesting.Engine;

public class HistoricalConfidenceAnalyzer
{
    private readonly SportsBettingDbContext _db;

    public HistoricalConfidenceAnalyzer(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<List<HistoricalConfidenceResult>>
        AnalyzeAsync(
            DateTime from,
            DateTime to)
    {
        var predictions =
            await _db.Predictions
                .Where(x =>
                    x.IsSettled &&
                    x.CreatedUtc >= from &&
                    x.CreatedUtc <= to)
                .OrderBy(x =>
                    x.Confidence)
                .ToListAsync();

        var grouped =
            predictions
                .GroupBy(x =>
                    NormalizeBand(
                        x.Confidence))
                .OrderBy(x =>
                    x.Key);

        var results =
            new List<HistoricalConfidenceResult>();

        foreach (var group in grouped)
        {
            int total =
                group.Count();

            int wins =
                group.Count(x =>
                x.IsCorrect == true);

            int losses =
                total - wins;

            decimal accuracy = 0;

            if (total > 0)
            {
                accuracy =
                    ((decimal)wins /
                    total) * 100;
            }

            decimal averageConfidence =
                group.Average(x =>
                    x.Confidence);

            decimal variance =
                accuracy -
                averageConfidence;

            results.Add(
                new HistoricalConfidenceResult
                {
                    ConfidenceBand =
                        group.Key,

                    TotalPredictions =
                        total,

                    Wins =
                        wins,

                    Losses =
                        losses,

                    ActualAccuracy =
                        accuracy,

                    AverageConfidence =
                        averageConfidence,

                    CalibrationVariance =
                        variance
                });
        }

        return results;
    }

    private decimal NormalizeBand(
        decimal confidence)
    {
        return Math.Floor(
            confidence / 10) * 10;
    }
}

public class HistoricalConfidenceResult
{
    public decimal ConfidenceBand { get; set; }

    public int TotalPredictions { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public decimal ActualAccuracy { get; set; }

    public decimal AverageConfidence { get; set; }

    public decimal CalibrationVariance { get; set; }
}
