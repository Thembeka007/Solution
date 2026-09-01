using AI.Evolution.Laboratory.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace AI.Evolution.Laboratory.Services;

public class HistoricalMarketConditionAnalyzer
{
    private readonly SportsBettingDbContext _db;

    private readonly LaboratoryEvaluationSettings _settings;

    public HistoricalMarketConditionAnalyzer(
        SportsBettingDbContext db,
        LaboratoryEvaluationSettings settings)
    {
        _db = db;
        _settings = settings;
    }

    public async Task<HistoricalMarketCondition?>
        AnalyzeAsync(
            Guid matchId,
            DateTime asOfUtc)
    {
        // =========================================
        // LOAD MATCH
        // =========================================

        var match =
            await _db.Matches
                .FirstOrDefaultAsync(x =>
                    x.Id == matchId);

        if (match == null)
            return null;

        // =========================================
        // LOAD OBSERVATION AVAILABLE AT THE TIME
        // =========================================

        var earliestAllowedUtc =
            asOfUtc.AddMinutes(
                -_settings.MaximumMarketObservationAgeMinutes);

        var observations =
            await _db.MatchOdds
                .Where(x =>
                    x.MatchId == matchId &&
                    x.RetrievedUtc <= asOfUtc &&
                    x.RetrievedUtc >= earliestAllowedUtc &&
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
            return null;

        // =========================================
        // RAW ODDS COLLECTIONS
        // =========================================

        var homeOdds =
            latestPerBookmaker
                .Select(x =>
                    x.HomeOdds)
                .OrderBy(x => x)
                .ToList();
        
        var drawOdds =
            latestPerBookmaker
                .Select(x =>
                    x.DrawOdds)
                .OrderBy(x => x)
                .ToList();
        
        var awayOdds =
            latestPerBookmaker
                .Select(x =>
                    x.AwayOdds)
                .OrderBy(x => x)
                .ToList();

        // =========================================
        // IMPLIED PROBABILITIES
        // =========================================

        var homeProbabilities =
            latestPerBookmaker
                .Select(GetHomeProbability)
                .OrderBy(x => x)
                .ToList();
        
        var drawProbabilities =
            latestPerBookmaker
                .Select(GetDrawProbability)
                .OrderBy(x => x)
                .ToList();
        
        var awayProbabilities =
            latestPerBookmaker
                .Select(GetAwayProbability)
                .OrderBy(x => x)
                .ToList();

        // =========================================
        // OVERROUNDS
        // =========================================

        var overrounds =
            latestPerBookmaker
                .Select(CalculateOverround)
                .OrderBy(x => x)
                .ToList();

        // =========================================
        // MARKET CONDITION
        // =========================================

        decimal homeProbabilityRange =
            homeProbabilities.Max() -
            homeProbabilities.Min();
        
        decimal drawProbabilityRange =
            drawProbabilities.Max() -
            drawProbabilities.Min();
        
        decimal awayProbabilityRange =
            awayProbabilities.Max() -
            awayProbabilities.Min();
        
        decimal averageProbabilityRange =
            (
            homeProbabilityRange +
            drawProbabilityRange +
            awayProbabilityRange
            ) / 3m;

        // =========================================
        // PROVIDER TIMESTAMPS
        // =========================================

        var providerDates =
            latestPerBookmaker
                .Where(x =>
                    x.ProviderUpdatedUtc >
                    DateTime.MinValue)
                .Select(x =>
                    x.ProviderUpdatedUtc)
                .OrderBy(x => x)
                .ToList();
        
        DateTime? earliestProviderUpdate =
            providerDates.Any()
                ? providerDates.First()
                : null;
        
        DateTime? latestProviderUpdate =
            providerDates.Any()
                ? providerDates.Last()
                : null;
        
        double providerSpreadMinutes =
            earliestProviderUpdate.HasValue &&
            latestProviderUpdate.HasValue
                ? (
                    latestProviderUpdate.Value -
                    earliestProviderUpdate.Value
                  ).TotalMinutes
                : 0d;

        // =========================================
        // BUILD RESULT
        // =========================================

        var result =
            new HistoricalMarketCondition
            {
                MatchId =
                    match.Id,

                KickoffUtc =
                    match.KickoffUtc,

                LeagueName =
                    match.LeagueName,

                BookmakerCount =
                    latestPerBookmaker.Count,

                FreshObservationCount =
                    latestPerBookmaker.Count,

                MaximumObservationAgeMinutes =
                    _settings.MaximumMarketObservationAgeMinutes,

                // -----------------------------
                // HOME ODDS
                // -----------------------------

                MinimumHomeOdds =
                    homeOdds.Min(),

                MaximumHomeOdds =
                    homeOdds.Max(),

                MedianHomeOdds =
                    Median(homeOdds),

                MeanHomeOdds =
                    homeOdds.Average(),

                HomeOddsRange =
                    homeOdds.Max() - homeOdds.Min(),

                // -----------------------------
                // DRAW ODDS
                // -----------------------------

                MinimumDrawOdds =
                    drawOdds.Min(),

                MaximumDrawOdds =
                    drawOdds.Max(),

                MedianDrawOdds =
                    Median(drawOdds),

                MeanDrawOdds =
                    drawOdds.Average(),

                DrawOddsRange =
                    drawOdds.Max() - drawOdds.Min(),

                // -----------------------------
                // AWAY ODDS
                // -----------------------------

                MinimumAwayOdds =
                    awayOdds.Min(),

                MaximumAwayOdds =
                    awayOdds.Max(),

                MedianAwayOdds =
                    Median(awayOdds),

                MeanAwayOdds =
                    awayOdds.Average(),

                AwayOddsRange =
                    awayOdds.Max() - awayOdds.Min(),

                // -----------------------------
                // PROBABILITIES
                // -----------------------------

                MedianHomeImpliedProbability =
                    Median(homeProbabilities),

                MedianDrawImpliedProbability =
                    Median(drawProbabilities),

                MedianAwayImpliedProbability =
                    Median(awayProbabilities),

                // -----------------------------
                // OVERROUND
                // -----------------------------

                MinimumOverround =
                    overrounds.Min(),

                MaximumOverround =
                    overrounds.Max(),

                MedianOverround =
                    Median(overrounds),

                MeanOverround =
                    overrounds.Average(),

                // -----------------------------
                // DISPERSION
                // -----------------------------

                HomeProbabilityRange =
                    homeProbabilityRange,

                DrawProbabilityRange =
                    drawProbabilityRange,

                AwayProbabilityRange =
                    awayProbabilityRange,

                AverageProbabilityRange =
                    averageProbabilityRange,

                // -----------------------------
                // TIMESTAMPS
                // -----------------------------

                EarliestProviderUpdateUtc =
                    earliestProviderUpdate,

                LatestProviderUpdateUtc =
                    latestProviderUpdate,

                ProviderUpdateSpreadMinutes =
                    providerSpreadMinutes,

                // -----------------------------
                // CLASSIFICATIONS
                // -----------------------------

                CoverageCondition =
                    ClassifyCoverage(
                        latestPerBookmaker.Count),

                DispersionCondition =
                    ClassifyDispersion(
                        averageProbabilityRange),

                OverroundCondition =
                    ClassifyOverround(
                        Median(overrounds)),

                MarketShape =
                    ClassifyMarketShape(
                        homeProbabilityRange,
                        drawProbabilityRange,
                        awayProbabilityRange,
                        homeProbabilities,
                        drawProbabilities,
                        awayProbabilities)
            };

        return result;

    }

    // =========================================================
    // IMPLIED PROBABILITIES
    // =========================================================

    private static decimal GetHomeProbability(
        MatchOdds observation)
    {
        return
            (1m / observation.HomeOdds) * 100m;
    }

    private static decimal GetDrawProbability(
        MatchOdds observation)
    {
        return
            (1m / observation.DrawOdds) * 100m;
    }

    private static decimal GetAwayProbability(
        MatchOdds observation)
    {
        return
            (1m / observation.AwayOdds) * 100m;
    }

    // =========================================================
    // OVERROUND
    // =========================================================
    
    private static decimal CalculateOverround(
        MatchOdds observation)
    {
        decimal home =
            1m / observation.HomeOdds;
        
        decimal draw =
            1m / observation.DrawOdds;
        
        decimal away =
            1m / observation.AwayOdds;
        
        return
            ((home + draw + away) - 1m) *
                100m;
    }

    // =========================================================
    // MEDIAN
    // =========================================================

    private static decimal Median(
        List<decimal> values)
    {
        if (!values.Any())
            return 0m; 
       
        int count =
            values.Count;
        
        int middle =
            count / 2;
        
        if (count % 2 == 1)
        {
            return values[middle];
        }
        
        return
            (
            values[middle - 1] +
            values[middle]) / 2m;
    }

    // =========================================================
    // COVERAGE
    // =========================================================

    private static string ClassifyCoverage(
        int bookmakerCount)
    {
        if (bookmakerCount< 3)
            return "INSUFFICIENT";
        
        if (bookmakerCount< 5)
            return "LOW";
        
        if (bookmakerCount< 10)
            return "MEDIUM";
        
        return "HIGH";
    }

    // =========================================================
    // DISPERSION
    // =========================================================

    private static string ClassifyDispersion(
        decimal averageProbabilityRange)
    {
        if (averageProbabilityRange >= 8m)
            return "HIGH";
        
        if (averageProbabilityRange >= 4m)
            return "MEDIUM";
        
        return "LOW";
    }

    // =========================================================
    // OVERROUND
    // =========================================================

    private static string ClassifyOverround(
        decimal medianOverround)
    {
        if (medianOverround >= 10m)
            return "HIGH";
        
        if (medianOverround >= 6m)
            return "MEDIUM";
        
        return "LOW";
    }

    // =========================================================
    // MARKET SHAPE
    // =========================================================

    private static string ClassifyMarketShape(
        decimal homeRange,
        decimal drawRange,
        decimal awayRange,
        List<decimal> homeProbabilities,
        List<decimal> drawProbabilities,
        List<decimal> awayProbabilities)
    {
        decimal home =
            Median(homeProbabilities);
        
        decimal draw =
            Median(drawProbabilities);
        
        decimal away =
            Median(awayProbabilities);
        
        decimal strongest =
            Math.Max(home,
                Math.Max(draw, away));

        if (strongest >= 65m)
            return "STRONG_FAVORITE";

        if (home >= 45m &&
            home <= 55m &&
            away >= 20m &&
            away <= 30m)
        {
            return "BALANCED_HOME";
        }

        if (away >= 45m &&
            away <= 55m &&
            home >= 20m &&
            home <= 30m)
        {
            return "BALANCED_AWAY";
        }

        if (homeRange >= 8m ||
            drawRange >= 8m ||
            awayRange >= 8m)
        {
            return "HIGH_DISAGREEMENT";
        }

        return "STANDARD";
    }

}
