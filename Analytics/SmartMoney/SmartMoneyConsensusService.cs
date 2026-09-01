using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Analytics.SmartMoney;

public class SmartMoneyConsensusService
{
    private readonly SportsBettingDbContext _db;

    public SmartMoneyConsensusService(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<decimal>
        GetConsensusStrengthAsync(
            Guid matchId,
            string predictionType,
            string outcome)
    {
        var picks =
            await _db.TipsterPredictions
                .Include(x => x.Tipster)
                .Where(x =>
                    x.MatchId == matchId &&
                    x.PredictionType ==
                        predictionType &&
                    x.PredictedOutcome ==
                        outcome)
                .ToListAsync();

        if (!picks.Any())
            return 0;

        decimal totalWeight = 0;

        foreach (var pick in picks)
        {
            if (pick.Tipster is not null)
            {
                totalWeight +=
                    pick.Tipster.Weight;
            }
        }

        return Math.Min(totalWeight, 100);
    }
}
