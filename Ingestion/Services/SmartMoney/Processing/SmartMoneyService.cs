using Infrastructure.Data;
using Ingestion.Services.SmartMoney.Abstractions;
using Ingestion.Services.SmartMoney.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Ingestion.Services.SmartMoney.Processing;

public class SmartMoneyService
    : ISmartMoneyService
{
    private readonly SportsBettingDbContext _db;

    public SmartMoneyService(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<
       List<SmartMoneyConsensusDto>>
       GetConsensusPredictionsAsync(
           DateTime date)
    {
        var predictions =
            await _db.TipsterPredictions
                .Include(x => x.Tipster)
                .Where(x =>
                    x.Tipster != null &&
                    x.Tipster.IsSmartMoney)
                .ToListAsync();

        var grouped =
            predictions
                .GroupBy(x => new
                {
                    x.MatchId,
                    x.PredictionType,
                    x.PredictedOutcome
                });

        var results =
            new List<SmartMoneyConsensusDto>();

        foreach (var group in grouped)
        {
            var smartMoneyCount =
                group.Count();

            if (smartMoneyCount < 2)
                continue;

            var combinedWeight =
                group.Sum(x =>
                    x.Tipster!.Weight);

            var averageWinRate =
                group.Average(x =>
                    x.Tipster!.WinRate);

            var confidenceBoost =
                (combinedWeight *
                 averageWinRate)
                 / smartMoneyCount;

            results.Add(
                new SmartMoneyConsensusDto
                {
                    MatchId =
                        group.Key.MatchId,

                    PredictionType =
                        group.Key.PredictionType,

                    Outcome =
                        group.Key.PredictedOutcome,

                    ConfidenceBoost =
                        confidenceBoost,

                    Tipsters =
                        group
                            .Select(x =>
                                x.Tipster!.Name)
                            .Distinct()
                            .ToList()
                });
        }

        return results
            .OrderByDescending(x =>
                x.ConfidenceBoost)
            .ToList();
    }
}
