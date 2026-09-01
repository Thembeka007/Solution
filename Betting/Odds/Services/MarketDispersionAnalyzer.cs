using Betting.Odds.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Betting.Odds.Services;

public class MarketDispersionAnalyzer
{
    private readonly SportsBettingDbContext _db;
    
    public MarketDispersionAnalyzer(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<MarketDispersionMetrics?>
        AnalyzeAsync(Guid matchId)
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

        var homeOdds =
            latestPerBookmaker
                .Select(x =>
                    x.HomeOdds)
                .ToList();
        
        var drawOdds =
            latestPerBookmaker
                .Select(x =>
                    x.DrawOdds)
                .ToList();
        
        var awayOdds =
            latestPerBookmaker
                .Select(x =>
                    x.AwayOdds)
                .ToList();
        
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
        
        var overrounds =
            latestPerBookmaker
                .Select(x =>
                {
                    var total =
                        (1m / x.HomeOdds) +
                        (1m / x.DrawOdds) +
                        (1m / x.AwayOdds);
                    
                    return
                        (total - 1m) * 100m;
                })
                .ToList();
        
        var providerUpdates =
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

        return new MarketDispersionMetrics
        {
            MatchId =
                matchId,

            BookmakerCount =
                latestPerBookmaker.Count,
            
            // =========================================
            // HOME ODDS
            // =========================================

            HomeOddsMinimum =
                homeOdds.Min(),
            
            HomeOddsMaximum =
                homeOdds.Max(),
            
            HomeOddsMean =
                homeOdds.Average(),
            
            HomeOddsMedian =
                Median(homeOdds),
            
            HomeOddsRange =
                homeOdds.Max() - homeOdds.Min(),
            
            // =========================================
            // DRAW ODDS
            // =========================================
            
            DrawOddsMinimum =
                drawOdds.Min(),
            
            DrawOddsMaximum =
                drawOdds.Max(),
            
            DrawOddsMean =
                drawOdds.Average(),
            
            DrawOddsMedian =
                Median(drawOdds),
            
            DrawOddsRange =
                drawOdds.Max() - drawOdds.Min(),
            
            // =========================================
            // AWAY ODDS
            // =========================================
            
            AwayOddsMinimum =
                awayOdds.Min(),
            
            AwayOddsMaximum =
                awayOdds.Max(),
            
            AwayOddsMean =
                awayOdds.Average(),
            
            AwayOddsMedian =
                Median(awayOdds),
            
            AwayOddsRange =
                awayOdds.Max() - awayOdds.Min(),
            
            // =========================================
            // HOME PROBABILITY
            // =========================================
            
            HomeProbabilityMinimum =
                homeProbabilities.Min(),
            
            HomeProbabilityMaximum =
                homeProbabilities.Max(),
            
            HomeProbabilityMean =
                homeProbabilities.Average(),
            
            HomeProbabilityMedian =
                Median(homeProbabilities),
            
            HomeProbabilityRange =
                homeProbabilities.Max() - homeProbabilities.Min(),
            
            // =========================================
            // DRAW PROBABILITY
            // =========================================

            DrawProbabilityMinimum =
                drawProbabilities.Min(),
            
            DrawProbabilityMaximum =
                drawProbabilities.Max(),
            
            DrawProbabilityMean =
                drawProbabilities.Average(),
            
            DrawProbabilityMedian =
                Median(drawProbabilities),
            
            DrawProbabilityRange =
                drawProbabilities.Max() - drawProbabilities.Min(),
            
            // =========================================
            // AWAY PROBABILITY
            // =========================================
            
            AwayProbabilityMinimum =
                awayProbabilities.Min(),
            
            AwayProbabilityMaximum =
                awayProbabilities.Max(),
            
            AwayProbabilityMean =
                awayProbabilities.Average(),
            
            AwayProbabilityMedian =
                Median(awayProbabilities),
            
            AwayProbabilityRange =
                awayProbabilities.Max() - awayProbabilities.Min(),
            
            // =========================================
            // OVERROUND
            // =========================================
            
            OverroundMinimum =
                overrounds.Min(),
            
            OverroundMaximum =
                overrounds.Max(),
            
            OverroundMean =
                overrounds.Average(),
            
            OverroundMedian =
                Median(overrounds),
            
            OverroundRange =
                overrounds.Max() - overrounds.Min(),
            
            // =========================================
            // PROVIDER TIMING
            // =========================================
            
            OldestProviderUpdatedUtc =
                providerUpdates.Any()
                    ? providerUpdates.First()
                    : null,
            
            NewestProviderUpdatedUtc =
                providerUpdates.Any()
                    ? providerUpdates.Last()
                    : null,
            
            ProviderUpdateSpreadSeconds =
                providerUpdates.Count > 1
                    ? (decimal)
                        (providerUpdates.Last() -
                            providerUpdates.First())
                            .TotalSeconds
                    : 0m,
            
            // =========================================
            // RETRIEVAL TIMING
            // =========================================
            
            OldestRetrievedUtc =
                retrievalTimes.First(),
            
            NewestRetrievedUtc =
                retrievalTimes.Last(),
            
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
