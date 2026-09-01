using Betting.Odds.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Betting.Odds.Services;

public class ConsensusExperimentService
{
    private readonly SportsBettingDbContext _db;

    public ConsensusExperimentService(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<ConsensusExperimentResult?>
        RunAsync(
            Guid matchId)
    {
        var observations =
            await _db.MatchOdds
                .Where(x =>
                    x.MatchId == matchId &&
                    x.HomeOdds > 1m &&
                    x.DrawOdds > 1m &&
                    x.AwayOdds > 1m)
                .OrderByDescending(x =>
                    x.RetrievedUtc)
                .ToListAsync();

        // =========================================
        // LATEST OBSERVATION PER BOOKMAKER
        // =========================================

        var latestPerBookmaker =
            observations
                .GroupBy(x =>
                    x.Bookmaker)
                .Select(group =>
                    group
                        .OrderByDescending(x =>
                            x.RetrievedUtc)
                        .First())
                .ToList();

        if (!latestPerBookmaker.Any())
        {
            return null;
        }

        // =========================================
        // RAW ODDS
        // =========================================

        var homeOdds =
            latestPerBookmaker
                .Select(x => x.HomeOdds)
                .ToList();

        var drawOdds =
            latestPerBookmaker
                .Select(x => x.DrawOdds)
                .ToList();

        var awayOdds =
            latestPerBookmaker
                .Select(x => x.AwayOdds)
                .ToList();

        // =========================================
        // RAW IMPLIED PROBABILITIES
        // =========================================

        var homeProbabilities =
            latestPerBookmaker
                .Select(x =>
                    1m / x.HomeOdds * 100m)
                .ToList();

        var drawProbabilities =
            latestPerBookmaker
                .Select(x =>
                    1m / x.DrawOdds * 100m)
                .ToList();

        var awayProbabilities =
            latestPerBookmaker
                .Select(x =>
                    1m / x.AwayOdds * 100m)
                .ToList();

        // =========================================
        // OVERROUNDS
        // =========================================

        var overrounds =
            latestPerBookmaker
                .Select(x =>
                {
                    decimal total =
                        (1m / x.HomeOdds) +
                        (1m / x.DrawOdds) +
                        (1m / x.AwayOdds);

                    return
                        (total - 1m) * 100m;
                })
                .ToList();

        // =========================================
        // PROVIDER TIMING
        // =========================================

        var providerTimes =
            latestPerBookmaker
                .Where(x =>
                    x.ProviderUpdatedUtc >
                        DateTime.MinValue)
                .Select(x =>
                    x.ProviderUpdatedUtc)
                .OrderBy(x => x)
                .ToList();

        var retrievalTimes =
            latestPerBookmaker
                .Select(x =>
                    x.RetrievedUtc)
                .OrderBy(x => x)
                .ToList();

        return new ConsensusExperimentResult
        {
            MatchId =
                matchId,

            BookmakerCount =
                latestPerBookmaker.Count,

            // =========================================
            // MEDIAN RAW ODDS
            // =========================================

            MedianHomeOdds =
                Median(homeOdds),

            MedianDrawOdds =
                Median(drawOdds),

            MedianAwayOdds =
                Median(awayOdds),

            // =========================================
            // MEAN RAW ODDS
            // =========================================

            MeanHomeOdds =
                homeOdds.Average(),

            MeanDrawOdds =
                drawOdds.Average(),

            MeanAwayOdds =
                awayOdds.Average(),

            // =========================================
            // RAW IMPLIED PROBABILITY
            // =========================================

            MeanHomeImpliedProbability =
                homeProbabilities.Average(),

            MeanDrawImpliedProbability =
                drawProbabilities.Average(),

            MeanAwayImpliedProbability =
                awayProbabilities.Average(),

            MeanOverround =
                overrounds.Average(),

            // =========================================
            // DISPERSION
            // =========================================

            HomeOddsRange =
                homeOdds.Max() -
                homeOdds.Min(),

            DrawOddsRange =
                drawOdds.Max() -
                drawOdds.Min(),

            AwayOddsRange =
                awayOdds.Max() -
                awayOdds.Min(),

            HomeProbabilityRange =
                homeProbabilities.Max() -
                homeProbabilities.Min(),

            DrawProbabilityRange =
                drawProbabilities.Max() -
                drawProbabilities.Min(),

            AwayProbabilityRange =
                awayProbabilities.Max() -
                awayProbabilities.Min(),

            // =========================================
            // TIMING
            // =========================================

            ProviderUpdateSpreadSeconds =
                providerTimes.Count > 1
                    ? (decimal)
                        (providerTimes.Last() -
                        providerTimes.First())
                        .TotalSeconds
                    : 0m,

            RetrievalSpreadSeconds =
                retrievalTimes.Count > 1
                    ? (decimal)
                        (retrievalTimes.Last() -
                        retrievalTimes.First())
                        .TotalSeconds
                    : 0m

        };
    }

    private static decimal Median(
        List<decimal> values)
    {
        if (values.Count == 0)
        {
            return 0m;
        }

        var ordered =
            values
                .OrderBy(x => x)
                .ToList();

        int middle =
            ordered.Count / 2;

        if (ordered.Count % 2 == 0)
        {
            return
                (ordered[middle - 1] +
                 ordered[middle]) / 2m;
        }

        return ordered[middle];
    }
}
