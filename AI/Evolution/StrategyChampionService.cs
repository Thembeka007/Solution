using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace AI.Evolution;

public class StrategyChampionService
{
    private readonly SportsBettingDbContext _db;

    private readonly ILogger<StrategyChampionService>
        _logger;

    public StrategyChampionService(
        SportsBettingDbContext db,
        ILogger<StrategyChampionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SelectChampionsAsync()
    {
        var strategies =
            await _db.StrategyPerformances               
                .ToListAsync();

        if (!strategies.Any())
        {
            _logger.LogWarning(
                "No mature strategies available for champion selection.");

            return;
        }            

        // =======================================
        // GROUP INTO FAMILY STRATEGIES
        // =======================================

        var families =
            strategies                
                .GroupBy(x =>
                    x.StrategyFamily);

        foreach (var family in families)
        {

            foreach (var strategy in family)
            {
                strategy.IsActive = false;
                strategy.IsElite = false;
            }

            _logger.LogInformation(
                """
                Elite flags cleared
                
                Family={family}
                
                StrategiesCleared={count}
                
                """,
                family.Key,
                family.Count());

            // ===================================
            // ONLY MATURE STRATEGIES CAN
            // BECOME THE NEW CHAMPION
            // ===================================

            var matureStrategies =
                family
                    .Where(x => x.IsMature)
                    .ToList();

            if (!matureStrategies.Any())
            {
                _logger.LogInformation(
                    """
                    No mature strategy available for elite selection
                    
                    Family={family}
                    
                    """,
                    family.Key);
                
                continue;
            }

            // ===================================
            // CALCULATE FITNESS
            // ===================================

            var ranked =
                matureStrategies
                    .Select(x => new
                    {
                        Strategy = x,

                        Fitness =
                            (x.ROI * 0.55m)
                            +
                            (x.Accuracy * 0.30m)
                            +
                            ((100m -
                                Math.Abs(
                                    x.ConfidenceAccuracyGap))
                                * 0.15m)
                    })
                    .OrderByDescending(x =>
                        x.Fitness)
                    .ToList();

            if (!ranked.Any())
                continue;

            // ===================================
            // CHAMPION
            // ===================================

            var champion =
                ranked.First();

            // ===================================
            // ACTIVATE ONLY THE MATURE CHAMPION
            // ===================================

            foreach (var strategy in family)
            {
                strategy.IsActive = false;
                strategy.IsElite = false;
            }

            // ===================================
            // ACTIVATE CHAMPION
            // ===================================

            champion.Strategy.IsActive = true;
            champion.Strategy.IsElite = true;

            champion.Strategy.LastEvaluationUtc =
                DateTime.UtcNow;

            _logger.LogInformation(
                """
                =========================================
                Elite Strategy Selected

                Family:
                {family}

                Champion:
                {champion}

                ROI:
                {roi:F2}

                Accuracy:
                {accuracy:F2}

                Fitness:
                {fitness:F2}
                =========================================
                """,
                family.Key,
                champion.Strategy.StrategyVersion,
                champion.Strategy.ROI,
                champion.Strategy.Accuracy,
                champion.Fitness);
        }

        await _db.SaveChangesAsync();

        // =======================================
        // VERIFY ELITE STATE
        // =======================================

        var eliteStrategies =
            await _db.StrategyPerformances
                .Where(x => x.IsElite)
                .ToListAsync();

        foreach (var elite in eliteStrategies)
        {
            _logger.LogInformation(
                """
                Elite verification
                
                Family={family}
                
                Elite={strategy}
                
                Active={active}
                
                Mature={mature}
                
                ROI={roi:F2}
                
                Accuracy={accuracy:F2}
                
                """,
                elite.StrategyFamily,
                elite.StrategyVersion,
                elite.IsActive,
                elite.IsMature,
                elite.ROI,
                elite.Accuracy);
        }

    }

}


