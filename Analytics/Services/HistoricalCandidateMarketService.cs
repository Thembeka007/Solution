using Analytics.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Analytics.Services;

public class HistoricalCandidateMarketService
{
    private readonly SportsBettingDbContext _db;

    private readonly ILogger<HistoricalCandidateMarketService>
        _logger;

    public HistoricalCandidateMarketService(
        SportsBettingDbContext db,
        ILogger<HistoricalCandidateMarketService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ApplyMarketDataAsync(
        HistoricalCandidatePredictionResult result,
        Match match)
    {

        if (!result.ShouldGeneratePrediction)
        {
            return;
        }

        //------------------------------------------
        // MATCH MUST BE SETTLED
        //------------------------------------------

        if (match.HomeScore == null ||
            match.AwayScore == null)
        {
            result.MarketDataStatus =
                "MATCH_NOT_SETTLED";

            _logger.LogInformation(
                """
                Historical candidate settlement skipped

                Strategy={strategy}

                Match={match}

                Reason=Match has no final score
                """,
                result.StrategyVersion,
                match.Id);

            return;
        }

        // =========================================
        // FIND LATEST PRE-KICKOFF SNAPSHOT SECOND
        // =========================================
        //
        // MatchOdds rows belonging to the same bookmaker
        // collection can have slightly different
        // RetrievedUtc values.
        //
        // Therefore we group by UTC second rather than
        // requiring exact timestamp equality.
        //
        // =========================================

        var latestRetrievedUtc =
            await _db.MatchOdds
                .AsNoTracking()
                .Where(x =>
                    x.MatchId == match.Id &&
                    x.RetrievedUtc <= match.KickoffUtc)
                .OrderByDescending(x =>
                    x.RetrievedUtc)
                .Select(x =>
                    x.RetrievedUtc)
                .FirstOrDefaultAsync();

        if (latestRetrievedUtc == default)
        {
            result.MarketDataStatus =
                "NO_POINT_IN_TIME_ODDS";

            _logger.LogInformation(
                """
                Historical candidate market data unavailable

                Strategy={strategy}

                Match={match}

                Kickoff={kickoff}

                Reason=No odds snapshot existed before kickoff
                """,
                result.StrategyVersion,
                match.Id,
                match.KickoffUtc);

            return;
        }

        var snapshotStart =
            new DateTime(
                latestRetrievedUtc.Year,
                latestRetrievedUtc.Month,
                latestRetrievedUtc.Day,
                latestRetrievedUtc.Hour,
                latestRetrievedUtc.Minute,
                latestRetrievedUtc.Second,
                DateTimeKind.Utc);
        
        var snapshotEnd =
            snapshotStart.AddSeconds(1);


        // =========================================
        // LOAD ENTIRE POINT-IN-TIME SNAPSHOT
        // =========================================

        var snapshot =
            await _db.MatchOdds
                .AsNoTracking()
                .Where(x =>
                    x.MatchId == match.Id &&
                    x.RetrievedUtc >= snapshotStart &&
                    x.RetrievedUtc < snapshotEnd &&
                    x.RetrievedUtc <= match.KickoffUtc)
                .OrderBy(x =>
                    x.Bookmaker)
                .ThenByDescending(x =>
                    x.RetrievedUtc)
                .ThenByDescending(x =>
                    x.ProviderUpdatedUtc)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();

        if (!snapshot.Any())
        {
            result.MarketDataStatus =
                "NO_POINT_IN_TIME_ODDS";

            _logger.LogInformation(
                """
                Historical candidate market snapshot unavailable
                
                Strategy={strategy}
                
                Match={match}
                
                SnapshotStart={snapshotStart}
                
                SnapshotEnd={snapshotEnd} 
                """,
                result.StrategyVersion,
                match.Id,
                snapshotStart,
                snapshotEnd);

            return;
        }

        //=========================================
        // KEEP LATEST ROW PER BOOKMAKER
        // =========================================
        //
        // This protects against duplicate bookmaker
        // rows being recorded within the same second. 
        //
        // =========================================

        var bookmakers =
            snapshot.GroupBy(x =>
                x.Bookmaker)
            .Select(group =>
                group
                    .OrderByDescending(x =>
                        x.RetrievedUtc)
                    .ThenByDescending(x =>
                        x.ProviderUpdatedUtc)
                    .ThenByDescending(x =>
                        x.Id)
                    .First())
            .ToList();


        // =========================================
        // SELECT BEST EXECUTABLE PRICE
        // =========================================

        MatchOdds? bestOdds = null;
        
        decimal bestPrice = 0m;

        switch (result.PredictedOutcome)
        {
            case "HOME_WIN":

                bestOdds =
                    bookmakers
                        .Where(x =>
                            x.HomeOdds > 1m)
                        .OrderByDescending(x =>
                            x.HomeOdds)
                        .FirstOrDefault();

                if (bestOdds != null)
                {
                    bestPrice =
                        bestOdds.HomeOdds;
                }

                break;

            case "AWAY_WIN":

                bestOdds =
                    bookmakers
                        .Where(x =>
                            x.AwayOdds > 1m)
                        .OrderByDescending(x =>
                            x.AwayOdds)
                        .FirstOrDefault();

                if (bestOdds != null)
                {
                    bestPrice =
                        bestOdds.AwayOdds;
                }

                break;

            default:

                result.MarketDataStatus =
                    "UNSUPPORTED_OUTCOME";

                _logger.LogWarning(
                    """
                    Historical candidate settlement failed

                    Strategy={strategy}

                    Match={match}

                    Outcome={outcome}

                    Reason=Unsupported prediction outcome
                    """,
                    result.StrategyVersion,
                    match.Id,
                    result.PredictedOutcome);

                return;
        }

        //------------------------------------------
        // VALIDATE ODDS
        //------------------------------------------

        if (bestOdds == null ||
            bestPrice <= 1m)
        {
            result.MarketDataStatus =
                "INVALID_ODDS";

            _logger.LogWarning(
                """
                Historical candidate market data invalid

                Strategy={strategy}

                Match={match}

                Outcome={outcome}
                
                Bookmakers={bookmakers}
                
                Reason=No valid executable price
                """,
                result.StrategyVersion,
                match.Id,
                result.PredictedOutcome,
                bookmakers.Count);

            return;
        }


        // =========================================
        // MARKET DATA
        // =========================================
        //
        // The implied probability belongs to the
        // selected executable price.
        //
        // We therefore calculate it directly rather
        // than using another bookmaker's probability.
        //
        // =========================================

        decimal marketProbability =
            (1m / bestPrice) * 100m;

        result.MarketDataAvailable =
            true;

        result.MarketDataStatus =
            "VALID";

        result.MarketOdds =
            bestPrice;

        result.MarketImpliedProbability =
            marketProbability;

        result.MarketRetrievedUtc =
            bestOdds.RetrievedUtc;

        result.MarketBookmaker =
            bestOdds.Bookmaker;

        // =========================================
        // DETERMINE ACTUAL RESULT
        // =========================================

        string actualOutcome;

        if (match.HomeScore >
            match.AwayScore)
        {
            actualOutcome =
                "HOME_WIN";
        }
        else if (match.AwayScore >
                 match.HomeScore)
        {
            actualOutcome =
                "AWAY_WIN";
        }
        else
        {
            actualOutcome =
                "DRAW";
        }

        // =========================================
        // SETTLE PREDICTION
        // =========================================

        result.PredictionCorrect =
            result.PredictedOutcome == actualOutcome;

        if (result.PredictionCorrect)
        {
            // Stake = 1 unit.
            result.RealisedProfitLoss =
                bestPrice - 1m;
        }
        else
        {
            result.RealisedProfitLoss =
                -1m;
        }

        result.RealisedROI =
            result.RealisedProfitLoss * 100m;

        // =========================================
        // LOG
        // =========================================

        _logger.LogInformation(
            """
            Historical candidate settled

            Strategy={strategy}

            Match={match}

            Prediction={prediction}

            Actual={actual}

            Correct={correct}

            SnapshotStart={snapshotStart}
            
            Bookmakers={bookmakers}
            
            SelectedBookmaker={bookmaker}
            
            SelectedOdds={odds:F2}
            
            MarketProbability={probability:F2}
            
            ProfitLoss={profit:F2}
            
            ROI={roi:F2}
            """,
            result.StrategyVersion,
            match.Id,
            result.PredictedOutcome,
            actualOutcome,
            result.PredictionCorrect,
            snapshotStart,
            bookmakers.Count,
            result.MarketBookmaker,
            result.MarketOdds,
            result.MarketImpliedProbability,
            result.RealisedProfitLoss,
            result.RealisedROI);
    }
}
