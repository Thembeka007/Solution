using AI.Evolution;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace AI.Optimization;

public class AutoStrategyReplacementService
{
    private readonly SportsBettingDbContext
        _db;

    private readonly ILogger<AutoStrategyReplacementService>
        _logger;

    private readonly StrategyFamilyService _familyService;

    private readonly FitnessEvaluationService
        _fitnessEvaluationService;

    private const decimal MinimumChampionRoi = 40m;

    public AutoStrategyReplacementService(
        SportsBettingDbContext db,
        ILogger<AutoStrategyReplacementService> logger,
        StrategyFamilyService familyService,
        FitnessEvaluationService fitnessEvaluationService)
    {
        _db = db;
        _logger = logger;
        _familyService = familyService;
        _fitnessEvaluationService = fitnessEvaluationService;

    }
    public async Task ExecuteAsync()
    {
        // =====================================
        // LOAD ALL STRATEGIES
        // =====================================

        var strategies =
            await _db.StrategyPerformances
                .ToListAsync();

        if (!strategies.Any())
        {
            _logger.LogInformation(
                "No strategies available for champion health monitoring.");

            return;
        }

        // =====================================
        // GROUP BY STRATEGY FAMILY
        // =====================================

        var families =
            strategies.GroupBy(x =>
                string.IsNullOrWhiteSpace(x.StrategyFamily)
                    ? _familyService
                        .GetFamily(x.StrategyVersion)
                    : x.StrategyFamily);

        foreach (var family in families)
        {
            // =================================
            // FIND THE CURRENT CHAMPION
            // =================================

            var champion =
                family.FirstOrDefault(x => x.IsElite);

            if (champion == null)
            {
                _logger.LogInformation(
                    """
                    No champion found for family.
                    
                    Family={family}
                    
                    No replacement check performed. 

                    """,
                    family.Key);

                continue;
            }

            // =================================
            // CHAMPION HEALTH CHECK
            // =================================

            _logger.LogInformation(
                """
                ========================================
                Champion Health Check
                
                Family={family}
                
                Champion={champion}
                
                ROI={roi:F2}
                
                Accuracy={accuracy:F2}
                
                Mature={mature}
                
                Active={active}
                
                Elite={elite}
                
                MinimumRequiredROI={minimumRoi:F2}
                
                ======================================== 
                """,
                family.Key,
                champion.StrategyVersion,
                champion.ROI,
                champion.Accuracy,
                champion.IsMature,
                champion.IsActive,
                champion.IsElite,
                MinimumChampionRoi);

            // ========================================
            // HEALTH CHECK REQUIREMENTS _ ROI
            // ========================================

            if (champion.ROI >= MinimumChampionRoi)
            {
                _logger.LogInformation(
                    """
                    Champion is healthy.
                    
                    Family={family}
                    
                    Champion={champion}
                    
                    ROI={roi:F2}
                    
                    MinimumRequiredROI={minimumRoi:F2}
                    
                    Status=HEALTHY. NO REPLACEMENT IS REQUIRED AT THIS TIME.
                    
                    """,
                    family.Key,
                    champion.StrategyVersion,
                    champion.ROI,
                    MinimumChampionRoi);

                continue;
            }

            // ========================================
            // CHAMPION HAS FAILED
            // ========================================

            _logger.LogWarning(
                """
                ========================================

                CHAMPION FAILED PROFITABILITY CHECK
                
                Family={family}
                
                Champion={champion}
                
                ROI={roi:F2}
                
                MinimumRequiredROI={minimumRoi:F2}
                
                Status=FAILED Searching for profitable replacement...
                
                ======================================== 
                """,
                family.Key,
                champion.StrategyVersion,
                champion.ROI,
                MinimumChampionRoi);

            // ========================================
            // FIND MATURE PROFITABLE REPLACEMENTS
            // ========================================

            var candidates =
                new List<(StrategyPerformance Strategy,
                    decimal Fitness)>();

            var matureCandidates =
                family
                    .Where(x =>
                        x.IsActive &&
                        x.IsMature &&
                        x.LaboratoryPassed &&
                        x.StrategyVersion !=
                        champion.StrategyVersion &&
                        x.ROI >= MinimumChampionRoi)
                    .ToList();

            _logger.LogInformation(
                """
                Replacement candidates found.
                
                Family={family}
                
                MatureProfitableCandidates={count}
                
                MinimumRequiredROI={minimumRoi:F2}
                
                """,
                family.Key,
                matureCandidates.Count,
                MinimumChampionRoi);

            if (!matureCandidates.Any())
            {
                _logger.LogWarning(
                    """
                    ========================================

                    NO PROFITABLE LABORATORY-APPROVED REPLACEMENT AVAILABLE

                    Family={family}

                    FailedChampion={champion}

                    ChampionROI={roi:F2}

                    MinimumRequiredROI={minimumRoi:F2}

                    No mature active strategy that passed
                    the Evolution Laboratory currently meets
                    the minimum profitability requirement.

                    Champion will remain active, no mature replacements
                    currently exist.

                    ========================================

                    """,
                    family.Key,
                    champion.StrategyVersion,
                    champion.ROI,
                    MinimumChampionRoi);

                // ========================================
                // KEEP CURRENT CHAMPION ACTIVE
                //
                // No approved replacement exists.
                // The current champion remains operational
                // until a valid replacement is available.
                // ========================================

                champion.IsActive = true;
                champion.IsElite = true;
                champion.ReplacementTriggered = false;

                champion.LastEvaluationUtc =
                    DateTime.UtcNow;

                continue;
            }

            // ========================================
            // EVALUATE EACH CANDIDATE
            // ========================================

            foreach (var candidate in matureCandidates)
            {
                decimal fitness =
                    await _fitnessEvaluationService
                        .CalculateFitnessAsync(candidate.StrategyVersion);

                candidates.Add(
                    (
                        candidate,
                        fitness
                    ));

                _logger.LogInformation(
                    """
                    Replacement candidate
                    
                    Family={family}
                    
                    Strategy={strategy}
                    
                    ROI={roi:F2}
                    
                    Accuracy={accuracy:F2}

                    LaboratoryPassed={laboratoryPassed}
                    
                    Fitness={fitness:F3}
                    
                    Mature={mature}
                    
                    """,
                    family.Key,
                    candidate.StrategyVersion,
                    candidate.ROI,
                    candidate.Accuracy,
                    candidate.LaboratoryPassed,
                    fitness,
                    candidate.IsMature);
            }

            // ========================================
            // SELECT BEST PROFITABLE REPLACEMENT
            // ========================================

            var replacement =
                candidates
                    .OrderByDescending(x =>
                        x.Strategy.ROI)
                    .ThenByDescending(x =>
                        x.Strategy.Accuracy)
                    .ThenByDescending(x =>
                        x.Fitness)
                .First();

            // ========================================
            // DEACTIVATE FAILED CHAMPION
            // ========================================

            champion.IsActive = false;
            champion.IsElite = false;
            champion.ReplacementTriggered = true;

            champion.LastEvaluationUtc =
                DateTime.UtcNow;

            // ========================================
            // DEACTIVATE OTHER FAMILY STRATEGIES
            //
            // Only one active strategy should operate
            // within a family.
            // ========================================

            foreach (var strategy in family)
            {
                if (strategy.StrategyVersion !=
                    replacement.Strategy.StrategyVersion)
                {
                    strategy.IsActive = false;
                }
            }

            // ========================================
            // ACTIVATE REPLACEMENT
            // ========================================

            replacement.Strategy.IsActive = true;
            replacement.Strategy.IsElite = true;
            replacement.Strategy.ReplacementTriggered = false;

            replacement.Strategy.LastEvaluationUtc =
                DateTime.UtcNow;

            _logger.LogWarning(
                """
                ========================================
                CHAMPION REPLACED
                    
                Family={family}
                    
                FailedChampion={failedChampion}
                    
                FailedROI={failedRoi:F2}
                    
                Replacement={replacement}
                    
                ReplacementROI={replacementRoi:F2}
                    
                ReplacementAccuracy={replacementAccuracy:F2}
                    
                ReplacementFitness={replacementFitness:F3}
                    
                MinimumRequiredROI={minimumRoi:F2}
                    
                ========================================
                    
                """,
                family.Key,
                champion.StrategyVersion,
                champion.ROI,
                replacement.Strategy.StrategyVersion,
                replacement.Strategy.ROI,
                replacement.Strategy.Accuracy,
                replacement.Fitness,
                MinimumChampionRoi);
          

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Champion health monitoring and replacement complete.");
        }
    }
}
