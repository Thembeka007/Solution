using Infrastructure.Data;
using Ingestion.Odds.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Ingestion.Odds.Services;

public class OddsSyncService
{
    private readonly SportsBettingDbContext _db;

    private readonly IOddsProvider _provider;

    private readonly ILogger<OddsSyncService>
    _logger;

    public OddsSyncService(
        SportsBettingDbContext db,
        IOddsProvider provider,
        ILogger<OddsSyncService> logger)
    {
        _db = db;
        _provider = provider;
        _logger = logger;
    }

    public async Task SyncAsync(
        string sportKey)
    {
        var odds =
            await _provider
                .GetOddsAsync(sportKey);

        _logger.LogInformation(
            "Provider returned {count} odds records",
            odds.Count);

        foreach (var item in odds)
        {
            bool exists =
                await _db.MatchOdds
                    .AnyAsync(x =>
                        x.MatchId ==
                            item.MatchId &&
                        x.Bookmaker ==
                            item.Bookmaker &&
                        x.RetrievedUtc ==
                            item.RetrievedUtc);

            if (exists)
                continue;

            _db.MatchOdds.Add(
                new MatchOdds
                {
                    MatchId =
                        item.MatchId,

                    Bookmaker =
                        item.Bookmaker,

                    HomeOdds =
                        item.HomeOdds,

                    DrawOdds =
                        item.DrawOdds,

                    AwayOdds =
                        item.AwayOdds,

                    Over25Odds =
                        item.Over25Odds,

                    Under25Odds =
                        item.Under25Odds,

                    ProviderUpdatedUtc =
                        item.ProviderUpdatedUtc,

                    RetrievedUtc =
                        item.RetrievedUtc
     
                });

            _logger.LogInformation(
                "Inserting odds for MatchId={id}",
                item.MatchId);
        }


        await _db.SaveChangesAsync();
    }
}
