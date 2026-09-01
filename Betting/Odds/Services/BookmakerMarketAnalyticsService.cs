using Betting.Odds.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Betting.Odds.Services;

public class BookmakerMarketAnalyticsService
{
    private readonly SportsBettingDbContext _db;
    public BookmakerMarketAnalyticsService(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<List<BookmakerMarketAnalytics>>
        AnalyzeMatchAsync(Guid matchId)
    {
        var observations =
            await _db.MatchOdds
                .Where(x =>
                    x.MatchId == matchId
                    &&
                    x.HomeOdds > 1m
                    &&
                    x.DrawOdds > 1m
                    &&
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

        var results =
            new List<BookmakerMarketAnalytics>();


        foreach (var observation in latestPerBookmaker)
        {
            decimal homeImplied =
                1m / observation.HomeOdds;

            decimal drawImplied =
                1m / observation.DrawOdds;

            decimal awayImplied =
                1m / observation.AwayOdds;

            decimal impliedTotal =
                homeImplied + drawImplied + awayImplied;

            decimal overround =
                (impliedTotal - 1m) * 100m;

            results.Add(
                new BookmakerMarketAnalytics
                {
                    MatchId =
                        observation.MatchId,

                    Bookmaker =
                        observation.Bookmaker,

                    // ---------------------------------
                    // RAW ODDS
                    // ---------------------------------

                    HomeOdds =
                        observation.HomeOdds,

                    DrawOdds =
                        observation.DrawOdds,

                    AwayOdds =
                        observation.AwayOdds,

                    // ---------------------------------
                    // DERIVED PROBABILITIES
                    // ---------------------------------

                    HomeImpliedProbability =
                        homeImplied * 100m,

                    DrawImpliedProbability =
                        drawImplied * 100m,

                    AwayImpliedProbability =
                        awayImplied * 100m,

                    // ---------------------------------
                    // OVERROUND
                    // ---------------------------------

                    Overround =
                        overround,

                    // ---------------------------------
                    // TIMESTAMPS
                    // ---------------------------------

                    ProviderUpdatedUtc =
                        observation.ProviderUpdatedUtc,

                    RetrievedUtc =
                        observation.RetrievedUtc
                });
        }

        return results;

    }

    public async Task<MarketCoverage>
        GetCoverageAsync(Guid matchId)
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
            return new MarketCoverage
            {
                MatchId =
                    matchId,

                BookmakerCount =
                    0
            };
        }

        return new MarketCoverage
        {
            MatchId =
                matchId,
            
            BookmakerCount =
                latestPerBookmaker.Count,
            
            OldestProviderUpdateUtc =
                latestPerBookmaker
                    .Select(x =>
                        (DateTime?)x.ProviderUpdatedUtc)
                    .Where(x =>
                        x.HasValue &&
                        x.Value > DateTime.MinValue)
                    .OrderBy(x => x)
                    .FirstOrDefault(),

            NewestProviderUpdateUtc =
                latestPerBookmaker
                    .Select(x =>
                        (DateTime?)x.ProviderUpdatedUtc)
                    .Where(x =>
                        x.HasValue &&
                        x.Value > DateTime.MinValue)
                    .OrderByDescending(x => x)
                    .FirstOrDefault(),

            LatestRetrievedUtc =
                latestPerBookmaker
                    .Select(x =>
                        (DateTime?)x.RetrievedUtc)
                    .OrderByDescending(x => x)
                    .FirstOrDefault()
        };
    }

}
