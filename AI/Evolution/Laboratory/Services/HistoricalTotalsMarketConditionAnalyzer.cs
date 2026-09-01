using AI.Evolution.Laboratory.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace AI.Evolution.Laboratory.Services;

public class HistoricalTotalsMarketConditionAnalyzer
{
    private readonly SportsBettingDbContext _db;
    
    private const decimal OverUnderLine = 2.5m;
    
    private const double FreshObservationMaximumAgeMinutes = 15d;

    public HistoricalTotalsMarketConditionAnalyzer(
        SportsBettingDbContext db)
    {
        _db = db;
    }
    public async Task<HistoricalTotalsMarketCondition?> AnalyzeAsync(
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
        // LOAD HISTORICAL TOTALS OBSERVATIONS
        // =========================================
        //
        // Only observations that:
        //
        // 1. Belong to this match
        // 2. Were retrieved by our system
        // before the prediction decision
        // 3. Contain valid Over/Under 2.5 odds
        //
        // =========================================

        var observations =
            await _db.MatchOdds
                .Where(x =>
                    x.MatchId == matchId &&
                    x.RetrievedUtc <= asOfUtc &&
                    x.Over25Odds > 1m &&
                    x.Under25Odds > 1m)
                .OrderByDescending(x =>
                    x.RetrievedUtc)
                .ToListAsync();

        if (!observations.Any())
            return null;

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

        // =========================================
        // ODDS
        // =========================================

        var overOdds =
            latestPerBookmaker
                .Select(x =>
                    x.Over25Odds)
                .OrderBy(x => x)
                .ToList();

        var underOdds =
            latestPerBookmaker
                .Select(x =>
                    x.Under25Odds)
                .OrderBy(x => x)
                .ToList();

        // =========================================
        // IMPLIED PROBABILITIES
        // =========================================

        var overProbabilities =
            latestPerBookmaker
                .Select(x =>
                    1m / x.Over25Odds * 100m)
                .OrderBy(x => x)
                .ToList();

        var underProbabilities =
            latestPerBookmaker
                .Select(x =>
                    1m / x.Under25Odds * 100m)
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
        // PROBABILITY DISPERSION
        // =========================================

        decimal overProbabilityRange =
            overProbabilities.Max() -
            overProbabilities.Min();

        decimal underProbabilityRange =
            underProbabilities.Max() -
            underProbabilities.Min();

        decimal averageProbabilityRange =
            (
            overProbabilityRange +
            underProbabilityRange
            ) / 2m;

        // =========================================
        // OBSERVATION FRESHNESS
        // =========================================

        var ages =
            latestPerBookmaker
                .Select(x =>
                    (asOfUtc - x.RetrievedUtc)
                    .TotalMinutes)
                .Where(x =>
                    x >= 0d)
                .ToList();

        var freshObservationCount =
            ages.Count(x =>
                x <= FreshObservationMaximumAgeMinutes);

        var maximumObservationAgeMinutes =
            ages.Any()
                ? ages.Max()
                : 0d;

        // =========================================
        // PROVIDER TIMING
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

        DateTime? earliestProviderUpdateUtc =
            providerDates.Any()
                ? providerDates.First()
                : null;

        DateTime? latestProviderUpdateUtc =
            providerDates.Any()
                ? providerDates.Last()
                : null;

        double providerUpdateSpreadMinutes =
            earliestProviderUpdateUtc.HasValue &&
            latestProviderUpdateUtc.HasValue
                ? (
                latestProviderUpdateUtc.Value -
                earliestProviderUpdateUtc.Value
                  ).TotalMinutes
                : 0d;

        // =========================================
        // BUILD RESULT
        // =========================================

        return new HistoricalTotalsMarketCondition
        {
            MatchId =
                match.Id,

            KickoffUtc =
                match.KickoffUtc,

            LeagueName =
                match.LeagueName,

            Line =
                OverUnderLine,

            BookmakerCount =
                latestPerBookmaker.Count,

            FreshObservationCount =
                freshObservationCount,

            MaximumObservationAgeMinutes =
                maximumObservationAgeMinutes,
            
            // -------------------------------------
            // OVER ODDS
            // -------------------------------------

            MinimumOverOdds =
                overOdds.Min(),
            
            MaximumOverOdds =
                overOdds.Max(),
            
            MedianOverOdds =
                Median(overOdds),
            
            MeanOverOdds =
                overOdds.Average(),
            
            OverOddsRange =
                overOdds.Max() -
                overOdds.Min(),
            
            // -------------------------------------
            // UNDER ODDS
            // -------------------------------------
            
            MinimumUnderOdds =
                underOdds.Min(),
            
            MaximumUnderOdds =
                underOdds.Max(),
            
            MedianUnderOdds =
                Median(underOdds),
            
            MeanUnderOdds =
                underOdds.Average(),
            
            UnderOddsRange =
                underOdds.Max() -
                underOdds.Min(),
            
            // -------------------------------------
            // IMPLIED PROBABILITIES
            // -------------------------------------
            
            MedianOverImpliedProbability =
                Median(overProbabilities),
            
            MedianUnderImpliedProbability =
                Median(underProbabilities),
            
            // -------------------------------------
            // DISPERSION
            // -------------------------------------
            
            OverProbabilityRange =
                overProbabilityRange,
            
            UnderProbabilityRange =
                underProbabilityRange,
            
            AverageProbabilityRange =
                averageProbabilityRange,
            
            // -------------------------------------
            // OVERROUND
            // -------------------------------------
            
            MinimumOverround =
                overrounds.Min(),
            
            MaximumOverround =
                overrounds.Max(),
            
            MedianOverround =
                Median(overrounds),
            
            MeanOverround =
                overrounds.Average(),
            
            // -------------------------------------
            // PROVIDER TIMING
            // -------------------------------------
            
            EarliestProviderUpdateUtc =
                earliestProviderUpdateUtc,
            
            LatestProviderUpdateUtc =
                latestProviderUpdateUtc,
            
            ProviderUpdateSpreadMinutes =
                providerUpdateSpreadMinutes,
            
            // -------------------------------------
            // CLASSIFICATION
            // -------------------------------------
            
            CoverageCondition =
                ClassifyCoverage(latestPerBookmaker.Count),
            
            DispersionCondition =
                ClassifyDispersion(averageProbabilityRange),
            
            OverroundCondition =
                ClassifyOverround(
                    Median(overrounds))
        };
        
    }

    // =========================================================
    // OVERROUND
    // =========================================================
    private static decimal CalculateOverround(
        MatchOdds observation)
    {
        decimal over =
            1m / observation.Over25Odds;

        decimal under =
            1m / observation.Under25Odds;

        return
            ((over + under) - 1m) * 100m;
    }

    // =========================================================
    // MEDIAN
    // =========================================================
    private static decimal Median(
        List<decimal> values)
    {
        if (!values.Any())
            return 0m;

        int middle =
            values.Count / 2;

        if (values.Count % 2 == 1)
        {
            return values[middle];
        }

        return
            (
            values[middle - 1] +
            values[middle]
            ) / 2m;
    }

    // =========================================================
    // COVERAGE
    // =========================================================
    private static string ClassifyCoverage(
        int bookmakerCount)
    {

        if (bookmakerCount < 3)
            return "INSUFFICIENT";

        if (bookmakerCount < 5)
            return "LOW";

        if (bookmakerCount < 10)
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
}
