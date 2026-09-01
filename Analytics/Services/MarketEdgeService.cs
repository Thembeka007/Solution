using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace Analytics.Services;

public class MarketEdgeService
{
    private readonly SportsBettingDbContext _db;

    public MarketEdgeService(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(
        Prediction prediction,
        decimal marketOdds)
    {

        bool exists =
            await _db.MarketEdges
                .AnyAsync(x =>
                x.PredictionId ==
                prediction.Id);

        if (exists)
        {
            return;
        }

        decimal modelProbability =
            prediction.Confidence;

        decimal marketProbability =
            (1m / marketOdds) * 100m;

        decimal edge =
            modelProbability -
            marketProbability;

        decimal fairOdds =
            100m /
            modelProbability;

        var entity =
            new MarketEdge
            {
                PredictionId =
                    prediction.Id,

                ModelProbability =
                    modelProbability,

                MarketProbability =
                    marketProbability,

                EdgePercentage =
                    edge,

                FairOdds =
                    fairOdds,

                CreatedUtc =
                    DateTime.UtcNow,

                MarketOdds =
                    marketOdds
            };

        _db.MarketEdges.Add(entity);

        await _db.SaveChangesAsync();
    }
}
