using Infrastructure.Data;
using Ingestion.DTOs;
using Ingestion.Odds.Abstractions;
using Ingestion.Odds.Configuration;
using Ingestion.Odds.DTOs;
using Ingestion.Odds.DTOs.OddsApi;
using Ingestion.Odds.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Entities;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Ingestion.Odds.Providers;

public class OddsApiProvider : IOddsProvider
{
    private readonly IHttpClientFactory
        _httpFactory;

    private readonly OddsApiOptions
        _options;

    private readonly SportsBettingDbContext
        _db;

    private readonly ILogger<OddsApiProvider>
        _logger;

    public OddsApiProvider(
        IHttpClientFactory httpFactory,
        IOptions<OddsApiOptions> options,
        SportsBettingDbContext db,
        ILogger<OddsApiProvider> logger)
    {
        _httpFactory = httpFactory;
        _options = options.Value;
        _db = db;
        _logger = logger;
    }


    public async Task<List<OddsSnapshotDto>>
        GetOddsAsync(string sportKey)
    {

        _logger.LogCritical(
            "Requesting odds for sport: {sport}",
            sportKey);

        var client =
            _httpFactory.CreateClient();

        var output =
            new List<OddsSnapshotDto>();

        var oddsUrl =
            $"https://api.the-odds-api.com/v4/sports/{sportKey}/odds" +
            $"?apiKey={_options.ApiKey}" +
            $"&regions=eu" +
            $"&markets=h2h,totals" +
            $"&oddsFormat=decimal";

        _logger.LogInformation(
            "Calling Odds API for {sport}",
            sportKey);

        var response =
            await client
                .GetAsync(oddsUrl);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content
                    .ReadAsStringAsync();

            _logger.LogError(
                """
                Odds API failed.
                
                Sport={sport}
                
                Status={status}
                
                Response={response}
                """,
                sportKey,
                response.StatusCode,
                error);

            return output;
        }

        var oddsJson =
            await response.Content
                .ReadAsStringAsync();

        _logger.LogInformation(
            """
            Odds payload received.
            
            Sport={sport}
            
            Length={length}
            """,
            sportKey,
            oddsJson.Length);

        // ==========================================
        // DESERIALIZE API RESPONSE
        // ==========================================

        List<OddsApiEventDto>? events;

        try
        {
            events =
                JsonSerializer.Deserialize<
                    List<OddsApiEventDto>>(
                        oddsJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to deserialize Odds API response. Sport={sport}",
                sportKey);

            return output;
        }

        _logger.LogInformation(
            """
            Odds events received.

            Sport={sport}

            Count={count}
            """,
            sportKey,
            events?.Count ?? 0);
        
        if (events == null || events.Count == 0)
        {
            return output;
        }

        // =====================================================
        // LOAD MATCHES ONCE
        // =====================================================

        var matches =
            await _db.Matches
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .ToListAsync();

        // =====================================================
        // PROCESS EVENTS
        // =====================================================

        foreach (var ev in events)
        {
            // -------------------------------------------------
            // FIND MATCH
            // -------------------------------------------------

            var matchedFixture =
                matches
                    .FirstOrDefault(x =>
                        NormalizeTeamName(
                            x.HomeTeam!.Name)
                        ==
                        NormalizeTeamName(
                            ev.home_team)
                        &&
                        NormalizeTeamName(
                            x.AwayTeam!.Name)
                        ==
                        NormalizeTeamName(
                            ev.away_team));

            if (matchedFixture == null)
            {
                _logger.LogWarning(
                    "NO MATCH FOUND: {home} vs {away}",
                    ev.home_team,
                    ev.away_team);

                continue;
            }

            // -------------------------------------------------
            // PROCESS EVERY BOOKMAKER
            // -------------------------------------------------

            foreach (var bookmaker in ev.bookmakers)
            {
                _logger.LogInformation(
                    "Processing bookmaker {bookmaker} for {home} vs {away}",
                    bookmaker.title,
                    ev.home_team,
                    ev.away_team);

                // ==================================
                // FIND H2H MARKET
                // ==================================

                var h2hMarket =
                    bookmaker.markets
                        .FirstOrDefault(x =>
                            x.key == "h2h");

                // ==================================
                // FIND TOTALS MARKET
                // ==================================

                var totalsMarket =
                    bookmaker.markets
                        .FirstOrDefault(x =>
                            x.key == "totals");

                // ==================================
                // H2H OUTCOMES
                // ==================================

                var homeOutcome =
                    h2hMarket?
                        .outcomes
                        .FirstOrDefault(x =>
                            NormalizeTeamName(
                                x.name)
                            ==
                            NormalizeTeamName(
                                ev.home_team));

                var awayOutcome =
                    h2hMarket?
                        .outcomes
                        .FirstOrDefault(x =>
                            NormalizeTeamName(
                                x.name)
                            ==
                            NormalizeTeamName(
                                ev.away_team));

                var drawOutcome =
                    h2hMarket?
                        .outcomes
                        .FirstOrDefault(x =>
                            string.Equals(
                                x.name,
                                "Draw",
                                StringComparison.OrdinalIgnoreCase));

                // ==================================
                // OVER / UNDER 2.5
                // ==================================

                var over25Outcome =
                    totalsMarket?
                        .outcomes
                        .FirstOrDefault(x =>
                            string.Equals(
                                x.name,
                                "Over",
                                StringComparison.OrdinalIgnoreCase)
                            &&
                            x.point == 2.5m);

                var under25Outcome =
                    totalsMarket?
                        .outcomes
                        .FirstOrDefault(x =>
                            string.Equals(
                                x.name,
                                "Under",
                                StringComparison.OrdinalIgnoreCase)
                            &&
                            x.point == 2.5m);

                // ==================================
                // VALIDATE THAT SOMETHING USEFUL
                // WAS RETURNED
                // ==================================

                bool hasH2h =
                    homeOutcome != null &&
                    drawOutcome != null &&
                    awayOutcome != null;

                bool hasTotals =
                    over25Outcome != null &&
                    under25Outcome != null;


                if (!hasH2h && !hasTotals)
                {
                    _logger.LogWarning(
                        """
                        NO VALID MARKETS FOUND

                        Match={home} vs {away}

                        Bookmaker={bookmaker}

                        H2H={hasH2h}

                        Totals25={hasTotals}
                        """,
                        ev.home_team,
                        ev.away_team,
                        bookmaker.title,
                        hasH2h,
                        hasTotals);

                    continue;
                }

                // ==================================
                // TIMESTAMPS
                // ==================================

                var retrievedUtc =
                    DateTime.UtcNow;

                // Use the newest market timestamp
                // actually represented in this snapshot.
                //
                // The normal /odds endpoint provides
                // bookmaker-level last_update for these
                // featured markets.

                var providerUpdatedUtc =
                    GetProviderUpdatedUtc(
                        bookmaker,
                        h2hMarket,
                        totalsMarket);


                // ==================================
                // CREATE BOOKMAKER SNAPSHOT
                // ==================================

                var snapshot =
                    new OddsSnapshotDto
                    {
                        MatchId =
                            matchedFixture.Id,

                        Bookmaker =
                            bookmaker.title,

                        // ----------------------------------
                        // H2H
                        // ----------------------------------

                        HomeOdds =
                            homeOutcome?.price ?? 0m,

                        DrawOdds =
                            drawOutcome?.price ?? 0m,

                        AwayOdds =
                            awayOutcome?.price ?? 0m,

                        // ----------------------------------
                        // TOTALS 2.5
                        // ----------------------------------

                        Over25Odds =
                            over25Outcome?.price ?? 0m,

                        Under25Odds =
                            under25Outcome?.price ?? 0m,

                        // ----------------------------------
                        // TIMESTAMPS
                        // ----------------------------------

                        RetrievedUtc =
                            retrievedUtc,

                        ProviderUpdatedUtc =
                            providerUpdatedUtc
                    };

                output.Add(snapshot);

                _logger.LogInformation(
                    """
                    ODDS SNAPSHOT CREATED
                
                    Match={home} vs {away}
                
                    MatchId={matchId}
                
                    Sport={sport}
                
                    Bookmaker={bookmaker}
                
                    HomeOdds={homeOdds}
                
                    DrawOdds={drawOdds}
                
                    AwayOdds={awayOdds}
                        
                    Over25Odds={over25Odds}

                    Under25Odds={under25Odds}
                
                    ProviderUpdatedUtc={providerUpdated}
                
                    RetrievedUtc={retrieved} 
                    """,
                    ev.home_team,
                    ev.away_team,
                    matchedFixture.Id,
                    sportKey,
                    bookmaker.title,
                    snapshot.HomeOdds,
                    snapshot.DrawOdds,
                    snapshot.AwayOdds,
                    snapshot.Over25Odds,
                    snapshot.Under25Odds,
                    snapshot.ProviderUpdatedUtc,
                    snapshot.RetrievedUtc);

            }

        }

        // ==========================================
        // COMPLETE
        // ==========================================

        _logger.LogInformation(
            """
            Odds provider completed.
            
            Sport={sport}
            
            Events={events}
            
            BookmakerSnapshots={snapshots}
            """,
            sportKey,
            events.Count,
            output.Count);
        
        return output;


    }

    // =========================================================
    // TEAM NAME NORMALIZATION
    // =========================================================

    private static string NormalizeTeamName(
        string name)
    {
        return name
            .Trim()
            .Replace(
                "United States",
                "USA",
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "DR Congo",
                "Congo DR",
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "Korea Republic",
                "South Korea",
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "Cape Verde Islands",
                "Cape Verde",
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "Curacao",
                "Curaçao",
                StringComparison.OrdinalIgnoreCase)
            .ToUpperInvariant();
    }

    private static DateTime GetProviderUpdatedUtc(
        BookmakerDto bookmaker,
        MarketDto? h2hMarket,
        MarketDto? totalsMarket)
    {
        var timestamps =
            new List<DateTime>();

        if (bookmaker.last_update != default)
        {
            timestamps.Add(
                bookmaker.last_update);
        }

        if (h2hMarket != null &&
            h2hMarket.last_update != default)
        {
            timestamps.Add(
                h2hMarket.last_update);
        }

        if (totalsMarket != null &&
            totalsMarket.last_update != default)
        {
            timestamps.Add(
                totalsMarket.last_update);
        }

        return timestamps.Count == 0
            ? DateTime.MinValue
            : timestamps.Max();
    }
}
