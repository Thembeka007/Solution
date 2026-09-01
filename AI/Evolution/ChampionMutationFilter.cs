using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AI.Evolution;

public class ChampionMutationFilter
{
    private readonly SportsBettingDbContext _db;

    private readonly ILogger<ChampionMutationFilter> _logger;
    public ChampionMutationFilter(
        SportsBettingDbContext db,
        ILogger<ChampionMutationFilter> logger)
    {
       _db = db;
      _logger = logger;
    }

    public async Task<List<MutationPlan>>
        FilterAsync(List<MutationPlan> plans)
    {
        var result =
            new List<MutationPlan>();

        if (!plans.Any())
            return result;

        // ===================================
        // LOAD ACTIVE STRATEGY
        // ===================================

        var activeStrategies =
            await _db.StrategyPerformances
                .Where(x => x.IsActive)
                .ToListAsync();
        
        // ===================================
        // GROUP BY FAMILY
        // ===================================
        
        var families =
            activeStrategies
                .GroupBy(x =>
                    GetFamilyName(x.StrategyVersion));
        
        foreach (var family in families)
        {
            // ===============================
            // CHAMPION ROI
            // ===============================            
            var champion = family
                .OrderByDescending(x => x.ROI)
                .ThenByDescending(x => x.Accuracy)
                .First();

            var plan = plans
                .FirstOrDefault(x =>
                    x.StrategyVersion ==
                    champion.StrategyVersion);

            if (plan == null)
                continue;

            result.Add(plan);

            _logger.LogInformation(
                """
                Champion selected for mutation Family={family}
                Champion={strategy}
                ROI={roi:F2}
                Accuracy={accuracy:F2} 
                """,
                family.Key,
                champion.StrategyVersion,
                champion.ROI,
                champion.Accuracy);
        }

        _logger.LogInformation(
            "Mutation plans reduced from {before} to {after}",
            plans.Count,
            result.Count);

        return result;
    }
    
    private static string GetFamilyName(string version)
    {
        var index = version
            .IndexOf("_MUTATED_",
                StringComparison.OrdinalIgnoreCase);
        
        if (index < 0)
            return version;
        
        return version[..index];
    
    }
    
}
