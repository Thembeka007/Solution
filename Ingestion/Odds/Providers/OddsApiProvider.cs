using Infrastructure.Data;
using Ingestion.DTOs;
using Ingestion.Odds.Abstractions;
using Ingestion.Odds.Configuration;
using Ingestion.Odds.DTOs;
using Ingestion.Odds.DTOs.OddsApi;
using Ingestion.Odds.Services;
using Ingestion.Teams.Services;
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

    private readonly TeamIdentityService
        _teamIdentityService;

    private readonly ILogger<OddsApiProvider>
        _logger;

    public OddsApiProvider(
        IHttpClientFactory httpFactory,
        IOptions<OddsApiOptions> options,
        SportsBettingDbContext db,
        TeamIdentityService teamIdentityService,
        ILogger<OddsApiProvider> logger)
    {
        _httpFactory = httpFactory;
        _options = options.Value;
        _db = db;
        _teamIdentityService = teamIdentityService;
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

        // ==========================================
        // API USAGE / QUOTA DIAGNOSTICS
        // ==========================================

        string requestsUsed =
            response.Headers.TryGetValues(
                "x-requests-used",
                out var usedValues)
                    ? usedValues.FirstOrDefault() ?? "unknown"
                    : "unknown";

        string requestsRemaining =
            response.Headers.TryGetValues(
                "x-requests-remaining",
                out var remainingValues)
                    ? remainingValues.FirstOrDefault() ?? "unknown"
                    : "unknown";

        string requestsLast =
            response.Headers.TryGetValues(
                "x-requests-last",
                out var lastValues)
                    ? lastValues.FirstOrDefault() ?? "unknown"
                    : "unknown";

        _logger.LogInformation(
            """
            Odds API usage

            Sport={sport}

            RequestsUsed={used}

            RequestsRemaining={remaining}

            RequestsLast={last}
            """,
            sportKey,
            requestsUsed,
            requestsRemaining,
            requestsLast);

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
                        await FindMatchingFixtureAsync(
                            matches,
                            ev);

            if (matchedFixture == null)
            {
                _logger.LogWarning(
                    """
                    NO MATCH FOUND

                    ProviderHome={providerHome}

                    ProviderAway={providerAway}

                    ProviderCommenceTime={providerTime}
                    """,
                    ev.home_team,
                    ev.away_team,
                    ev.commence_time);

                continue;
            }


            _logger.LogInformation(
                """
                FIXTURE MATCHED

                ProviderHome={providerHome}

                ProviderAway={providerAway}

                DatabaseHome={databaseHome}

                DatabaseAway={databaseAway}

                ProviderCommenceTime={providerTime}

                DatabaseKickoff={databaseKickoff}

                ExternalFixtureId={externalFixtureId}
                """,
                ev.home_team,
                ev.away_team,
                matchedFixture.HomeTeam!.Name,
                matchedFixture.AwayTeam!.Name,
                ev.commence_time,
                matchedFixture.KickoffUtc,
                matchedFixture.ExternalFixtureId);


            _logger.LogInformation(
                """
                 REGISTERING TEAM MAPPINGS
                
                 Source=OddsApi
                 
                 MatchId={matchId}
                 
                 ExternalHome={home}
                 
                 ExternalAway={away} 
                """,
                matchedFixture.Id,
                ev.home_team, 
                ev.away_team);

            await _teamIdentityService
                .RegisterFixtureMappingsAsync(
                    matchedFixture,
                    "OddsApi",
                    ev.home_team,
                    ev.away_team);


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

    private async Task<Match?> FindMatchingFixtureAsync(
        List<Match> matches,
        OddsApiEventDto providerEvent)
    {
        var providerHome =
            NormalizeTeamName(
                providerEvent.home_team);

        var providerAway =
            NormalizeTeamName(
                providerEvent.away_team);

        var providerKickoff =
            providerEvent.commence_time;

        const double MaximumKickoffDifferenceMinutes = 180;

        // ==========================================
        // EXISTING NAME-BASED MATCHING
        // ==========================================

        var candidates =
            matches
                .Where(x =>
                    x.HomeTeam != null &&
                    x.AwayTeam != null)
                .Where(x =>
                    NormalizeTeamName(
                        x.HomeTeam!.Name) ==
                    providerHome)
                .Where(x =>
                    NormalizeTeamName(
                        x.AwayTeam!.Name) ==
                    providerAway)
                .Select(x =>
                    new
                    {
                        Match = x,

                        Difference =
                            Math.Abs(
                                (
                                    x.KickoffUtc -
                                    providerKickoff
                                ).TotalMinutes)
                    })
                .Where(x =>
                    x.Difference <=
                    MaximumKickoffDifferenceMinutes)
                .OrderBy(x =>
                    x.Difference)
                .ToList();

        if (candidates.Count > 0)
        {
            if (candidates.Count > 1)
            {
                _logger.LogWarning(
                    """
                    MULTIPLE FIXTURE MATCHES FOUND

                    ProviderHome={providerHome}

                    ProviderAway={providerAway}

                    ProviderKickoff={providerKickoff}

                    CandidateCount={count}

                    SelectedExternalFixtureId={selected}
                    """,
                    providerEvent.home_team,
                    providerEvent.away_team,
                    providerKickoff,
                    candidates.Count,
                    candidates[0]
                        .Match
                        .ExternalFixtureId);
            }

            return candidates[0].Match;
        }

        // ==========================================
        // IDENTITY FALLBACK
        // ==========================================

        var homeMapping =
            await _db.TeamExternalMappings
                .AsNoTracking()
                .Where(x =>
                    x.Source == "OddsApi" &&
                    x.IsActive)
                .ToListAsync();


        var canonicalHomeMapping =
            homeMapping.FirstOrDefault(x =>
                NormalizeTeamName(
                    x.ExternalName) ==
                providerHome);

        var canonicalAwayMapping =
            homeMapping.FirstOrDefault(x =>
                NormalizeTeamName(
                    x.ExternalName) ==
                providerAway);

        if (canonicalHomeMapping == null ||
            canonicalAwayMapping == null)
        {
            return null;
        }

        var identityCandidates =
            matches
               .Where(x =>
                   x.HomeTeam != null &&
                   x.AwayTeam != null)
               .Where(x =>
                   x.HomeTeamId ==
                   canonicalHomeMapping.TeamId)
               .Where(x =>
                   x.AwayTeamId ==
                   canonicalAwayMapping.TeamId)
               .Select(x =>
                   new
                   {
                       Match = x,
                       Difference =
                           Math.Abs(
                               (x.KickoffUtc -
                                providerKickoff)
                               .TotalMinutes)
                   })
               .Where(x =>
                   x.Difference <=
                   MaximumKickoffDifferenceMinutes)
               .OrderBy(x =>
                   x.Difference)
               .ToList();

        if (identityCandidates.Count == 0)
        {
            return null;
        }

        if (identityCandidates.Count > 1)
        {
            _logger.LogWarning(
                """
                MULTIPLE IDENTITY FIXTURE MATCHES FOUND

                ProviderHome={providerHome}

                ProviderAway={providerAway}

                ProviderKickoff={providerKickoff}

                CandidateCount={count}

                SelectedExternalFixtureId={selected}
                """,
                providerEvent.home_team,
                providerEvent.away_team,
                providerKickoff,
                identityCandidates.Count,
                identityCandidates[0]
                    .Match
                    .ExternalFixtureId);
        }

        _logger.LogInformation(
           """
            FIXTURE MATCHED VIA IDENTITY

            ProviderHome={providerHome}

            ProviderAway={providerAway}

            CanonicalHomeTeamId={homeTeamId}

            CanonicalAwayTeamId={awayTeamId}

            ProviderKickoff={providerKickoff}

            DatabaseKickoff={databaseKickoff}

            ExternalFixtureId={externalFixtureId}
            """,
           providerEvent.home_team,
           providerEvent.away_team,
           canonicalHomeMapping.TeamId,
           canonicalAwayMapping.TeamId,
           providerKickoff,
           identityCandidates[0]
               .Match
               .KickoffUtc,
           identityCandidates[0]
               .Match
               .ExternalFixtureId);

        return identityCandidates[0].Match;
    }


    // =========================================================
    // TEAM NAME NORMALIZATION
    // =========================================================

    private static string NormalizeTeamName(
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var normalized =
            name
                .Trim()
                .ToUpperInvariant();

        normalized =
            normalized
                .Replace(
                    "UNITED STATES",
                    "USA",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "DR CONGO",
                    "CONGO DR",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "KOREA REPUBLIC",
                    "SOUTH KOREA",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "CAPE VERDE ISLANDS",
                    "CAPE VERDE",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "CURACAO",
                    "CURACAO",
                    StringComparison.OrdinalIgnoreCase);

        normalized =
            normalized
                .Replace(
                    "RAYO VALLECANO DE MADRID",
                    "RAYO VALLECANO",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "RCD ESPANYOL DE BARCELONA",
                    "ESPANYOL",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "DEPORTIVO ALAVES",
                    "ALAVES",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "DEPORTIVO ALAVÉS",
                    "ALAVES",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "VALENCIA CF",
                    "VALENCIA",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "REAL MADRID CF",
                    "REAL MADRID",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "RC CELTA DE VIGO",
                    "CELTA VIGO",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "CA OSASUNA",
                    "OSASUNA",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "REAL BETIS BALOMPIE",
                    "REAL BETIS",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "REAL BETIS BALOMPIÉ",
                    "REAL BETIS",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "REAL SOCIEDAD DE FUTBOL",
                    "REAL SOCIEDAD",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "REAL SOCIEDAD DE FÚTBOL",
                    "REAL SOCIEDAD",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "SEVILLA FC",
                    "SEVILLA",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "ATHLETIC CLUB",
                    "ATHLETIC BILBAO",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "MALAGA CF",
                    "MALAGA",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "MÁLAGA CF",
                    "MALAGA",
                    StringComparison.OrdinalIgnoreCase);

        return normalized;
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
