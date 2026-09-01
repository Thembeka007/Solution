using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AI.Evolution.Promotion;

public class StrategyPromotionService
{
    private readonly SportsBettingDbContext _db;

    private readonly ILogger<StrategyPromotionService>
        _logger;

    private readonly StrategyPromotionGateService
        _gateService;
    public StrategyPromotionService(
        SportsBettingDbContext db,
        ILogger<StrategyPromotionService> logger,
        StrategyPromotionGateService gateService)
    {
        _db = db;
        _logger = logger;
        _gateService = gateService;
    }

    public async Task<bool> TryPromoteAsync(
        StrategyPromotionCandidate candidate)
    {
        var result =
            _gateService.Evaluate(candidate);

        if (!result.Passed)
        {
            _logger.LogWarning(
                """
                ========================================
                STRATEGY NOT PROMOTED
                
                Strategy={strategy}
                
                Reason=One or more promotion gates failed
                
                ======================================== 
                """,
                candidate.Performance.StrategyVersion);
            
            return false;
        
        }

        // =========================================
        // LOAD FAMILY
        // =========================================

        var family =
            candidate.Performance.StrategyFamily;
        
        var familyStrategies =
            await _db.StrategyPerformances
                .Where(x =>
                    x.StrategyFamily == family)
                .ToListAsync();

        // =========================================
        // DEFENSIVE DEACTIVATION
        // =========================================

        foreach (var strategy in familyStrategies)
        {
            strategy.IsActive =
                false;
            
            strategy.IsElite =
                false;
        }

        // =========================================
        // PROMOTE CANDIDATE
        // =========================================

        var promoted =
            familyStrategies
                .FirstOrDefault(x =>
                    x.StrategyVersion ==
                    candidate.Performance.StrategyVersion);

        if (promoted is null)
        {
            _logger.LogError(
                """
                Promotion aborted. StrategyPerformance record not found.
                
                Strategy={strategy}
                
                Family={family}
                
                """,
                candidate.Performance.StrategyVersion,
                family);
            
            return false;
        }

        promoted.IsActive =
            true;
        
        promoted.IsElite =
            true;
        
        promoted.LastEvaluationUtc =
            DateTime.UtcNow;
        
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            """
            ========================================
            
            STRATEGY PROMOTED TO LIVE
            
            Family={family}
            
            Strategy={strategy}
            
            ROI={roi:F2}
            
            Accuracy={accuracy:F2}
            
            MaximumDrawdown={drawdown:F2}%
            
            ======================================== 
            """,
            family,
            promoted.StrategyVersion,
            promoted.ROI,
            promoted.Accuracy,
            candidate.RiskMetrics.MaximumDrawdownPercentage);
        
        return true;

    }
}
