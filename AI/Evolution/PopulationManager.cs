using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace AI.Evolution;

public class PopulationManager
{
    private readonly SportsBettingDbContext _db;
    
    private readonly ILogger<PopulationManager> _logger;

    private readonly FitnessEvaluationService
        _fitnessEvaluationService;

    public PopulationManager(
        SportsBettingDbContext db,
        ILogger<PopulationManager> logger,
        FitnessEvaluationService fitnessEvaluationService)
    {
        _db = db;
        _logger = logger;
        _fitnessEvaluationService =
            fitnessEvaluationService;
    }
    // ============================================
    // GET ENTIRE POPULATION
    // ============================================
    public async Task<List<StrategyPerformance>> GetPopulationAsync(
        string familyName)
    {
        var population =
            await _db.StrategyPerformances
                .Where(x =>
                    x.StrategyVersion.StartsWith(
                    familyName)) 
                .ToListAsync();

        var ranked =
            new List<(StrategyPerformance Strategy,
                decimal Fitness)>();

        foreach (var strategy in population)
        {
            decimal fitness =
                await _fitnessEvaluationService
                    .CalculateFitnessAsync(
                        strategy.StrategyVersion);

            ranked.Add((strategy, fitness));
        }

        ranked =
            ranked
                .OrderByDescending(x => x.Fitness)
                .ToList();

        _logger.LogInformation(
            """
            Population Loaded
            
            Family={family}

            Population={count}
            
            """,
            familyName,
            population.Count);

        return ranked
            .Select(x => x.Strategy)
            .ToList();
    }

    // ============================================
    // IDENTIFY STRATEGIES TO RETIRE
    // ============================================

    public async Task<List<StrategyPerformance>> GetStrategiesToRetireAsync(
        string familyName,
        int maxPopulation)
    {
        var population =
            await GetPopulationAsync(familyName);

        var matureStrategies =
            population
                .Where(x =>
                    x.IsMature &&
                    x.LaboratoryPassed &&
                    !x.IsElite)
                .ToList();

        var immatureStrategies =
            population
                .Where(x =>
                    !x.IsMature)
                .ToList();

        var laboratoryFailedStrategies =
            population
                .Where(x =>
                    x.IsMature &&
                    !x.LaboratoryPassed)
                .ToList();

        _logger.LogInformation(
            """
            Population maturity

            Family={family}

            Population={population}

            MatureLaboratoryApproved={matureApproved}

            Immature={immature}

            LaboratoryFailed={laboratoryFailed}

            """,
            familyName,
            population.Count,
            matureStrategies.Count,
            immatureStrategies.Count,
            laboratoryFailedStrategies.Count);

        // ============================================
        // FIND ELITE STRATERGY
        // ============================================

        var elite =
            population
                .FirstOrDefault(x =>
                    x.IsElite &&
                    x.IsMature &&
                    x.LaboratoryPassed);

        if (elite is not null)
        {
            _logger.LogInformation(
                """
                Elite protected
                
                Family={family}
                
                Elite={elite}
                
                ROI={roi:F2}
                
                Accuracy={accuracy:F2}
                
                """,
                familyName,
                elite.StrategyVersion,
                elite.ROI,
                elite.Accuracy);
        
        }
        else
        {
            _logger.LogInformation(
                """
                No elite strategy found
                
                Family={family}
                
                """,
                familyName);
        
        }

        // ============================================
        // CHECK POPULATION SIZE
        // ============================================

        if (matureStrategies.Count <= maxPopulation)
        {
            _logger.LogInformation(
                """
                Population within limit

                Family={family}

                Population={population}

                MaturePopulation={maturePopulation}

                Limit={limit} 

                """,
                familyName,
                population.Count,
                matureStrategies.Count,
                maxPopulation);

            return new List<StrategyPerformance>();
        }

        // ============================================
        // SELECT SURVIVORS
        // ============================================

        var survivors =
            new List<StrategyPerformance>();

        if (elite is not null)
        {
            survivors.Add(elite);
        }   

        foreach (var strategy in matureStrategies)
        {
            if (survivors.Count >= maxPopulation)
            {
                break;
            }

            if (elite is not null &&
                strategy.Id == elite.Id)
            {
                continue;
            }

            survivors.Add(strategy);
        }

        // ============================================
        // DETERMINE RETIREMENTS
        // ============================================

        var survivorIds =
            survivors
                .Select(x => x.Id)
                .ToHashSet();
        
        var retired =
            matureStrategies
                .Where(x =>
                    !survivorIds.Contains(x.Id))
                .ToList();

        _logger.LogInformation(
            """
            Population limit exceeded
            
            Family={family}
            
            Population={population}
            
            MaturePopulation={maturePopulation}
            
            Survivors={survivors}
            
            Retire={retire}

            """,
            familyName,
            population.Count,
            matureStrategies.Count,
            survivors.Count,
            retired.Count);

        // ============================================
        // LOG RETIREMENT CANDIDATES
        // ============================================

        foreach (var strategy in retired)
        {
            decimal fitness =
                await _fitnessEvaluationService
                    .CalculateFitnessAsync(strategy.StrategyVersion);

            _logger.LogInformation(
                """
                Marked for retirement

                Strategy={strategy}

                Fitness={fitness:F3}
                
                ROI={roi:F2}
                
                Accuracy={accuracy:F2}
                
                """,
                strategy.StrategyVersion,
                fitness,
                strategy.ROI,
                strategy.Accuracy);
        }

        return retired;

    }

    // ============================================
    // RETIRE STRATEGIES
    // ============================================

    public async Task RetireStrategiesAsync(
        List<StrategyPerformance> strategies)
    {
        if (!strategies.Any())
        {
            _logger.LogInformation(
                "No strategies need retirement.");

            return;
        }

        foreach (var strategy in strategies)
        {
            // =====================================
            // SAFETY CHECK 
            // =====================================
            //
            // An elite strategy must NEVER be retired, even if accidentally passed into this method.
            //
            // =====================================

            if (strategy.IsElite)
            {
                _logger.LogWarning(
                    """
                    Elite retirement blocked
                    
                    Strategy={strategy}

                    LaboratoryPassed={laboratoryPassed}
                    
                    ROI={roi:F2}
                    
                    Accuracy={accuracy:F2}                    
                    """,
                    strategy.StrategyVersion,
                    strategy.LaboratoryPassed,
                    strategy.ROI,
                    strategy.Accuracy);
                
                continue;
            
            }

            decimal fitness =
                await _fitnessEvaluationService
                    .CalculateFitnessAsync(
                        strategy.StrategyVersion);

            strategy.IsActive = false;

            _logger.LogInformation(
                """
                Strategy retired

                Strategy={strategy}

                Fitness={fitness:F3}

                ROI={roi:F2}

                Accuracy={accuracy:F2}

                """,
                strategy.StrategyVersion,
                fitness,
                strategy.ROI,
                strategy.Accuracy);
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "{count} strategies retired.",
            strategies.Count);
    }
}
