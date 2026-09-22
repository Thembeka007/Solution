using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace AI.Evolution;

public class ParentSelectionService
{
    private readonly SportsBettingDbContext _db;

    private readonly StrategyFamilyService _familyService;

    private readonly ILogger<ParentSelectionService> _logger;

    private readonly FitnessEvaluationService
        _fitnessEvaluationService;

    private const int TournamentSize = 3;

    public ParentSelectionService(
        SportsBettingDbContext db,
        StrategyFamilyService familyService,
        ILogger<ParentSelectionService> logger,
        FitnessEvaluationService fitnessEvaluationService)
    {
        _db = db;
        _familyService = familyService;
        _logger = logger;
        _fitnessEvaluationService = fitnessEvaluationService;
    }

    public async Task<(string ParentA, string ParentB)>
        SelectParentsAsync(string strategyVersion)
    {
        string family =
            _familyService
                .GetFamily(strategyVersion);

        // ==========================================
        // LOAD ELIGIBLE PARENTS
        // ==========================================

        var candidates =
            await _db.StrategyPerformances
                .Where(x =>
                    x.IsActive &&
                    x.IsMature &&
                    x.LaboratoryPassed &&
                    x.StrategyFamily ==
                    family)
                .ToListAsync();

        _logger.LogInformation(
            """
            Tournament parent selection
            
            Family={family}
            
            EligibleCandidates={count}
            
            """,
            family,
            candidates.Count);

        // ==========================================
        // VALIDATE GENOMES
        // ==========================================

        var validCandidates =
            new List<StrategyPerformance>();

        foreach (var strategy in candidates)
        {
            int parameterCount =
                await _db.StrategyParameters
                    .CountAsync(x =>
                        x.StrategyVersion ==
                        strategy.StrategyVersion);

            if (parameterCount != 6)
            {
                _logger.LogWarning(
                    """
                    Strategy excluded from tournament
                    
                    Strategy={strategy}
                    
                    ParameterCount={count}
                    
                    """,
                    strategy.StrategyVersion,
                    parameterCount);
                
                continue;
            }

            validCandidates.Add(strategy);
        }

        // ==========================================
        // NO VALID PARENTS
        // ==========================================

        if (validCandidates.Count == 0)
        {
            _logger.LogWarning(
                """
                No valid tournament parents
                
                Family={family}
                
                RequestedStrategy={strategy}

                No parent fallback will be permitted                
                """,
                strategyVersion,
                strategyVersion);
            
            return (
                string.Empty,
                string.Empty);
        }

        // ==========================================
        // CALCULATE FITNESS
        // ==========================================

        var ranked =
            new List<(StrategyPerformance Strategy,
                decimal Fitness)>();

        foreach (var strategy in validCandidates)
        {
            decimal fitness =
                await _fitnessEvaluationService
                    .CalculateFitnessAsync(
                        strategy.StrategyVersion);

            ranked.Add(
                (
                    strategy,
                    fitness
                ));

            _logger.LogInformation(
                """
                Tournament candidate

                Strategy={strategy}

                LaboratoryPassed={laboratoryPassed}
                
                Fitness={fitness:F3}

                """,
                strategy.StrategyVersion,
                strategy.LaboratoryPassed,
                fitness);
        }    

        // ======================================
        // TOURNAMENT SELECTION - PARENTS
        // ======================================

        var parentA =
            SelectTournamentWinner(
                ranked,
                strategyVersion);

        var parentB =
            SelectTournamentWinner(
                ranked,
                parentA.Strategy.StrategyVersion);

        _logger.LogInformation(
            """
            ========================================
            Tournament parents selected
            
            Family={family}
            
            ParentA={parentA}
            
            FitnessA={fitnessA:F3}
            
            ParentB={parentB}
            
            FitnessB={fitnessB:F3}
            
            ========================================
            
            """,
            family,
            parentA.Strategy.StrategyVersion,
            parentA.Fitness,
            parentB.Strategy.StrategyVersion,
            parentB.Fitness);


        return (
            parentA.Strategy.StrategyVersion,
            parentB.Strategy.StrategyVersion);
    }

    private (
        StrategyPerformance Strategy,
        decimal Fitness)
        SelectTournamentWinner(
            List<(StrategyPerformance Strategy,
            decimal Fitness)> candidates,
            string excludeStrategy)
    {
        // ==========================================
        // ONLY ONE CANDIDATE
        // ==========================================

        if (candidates.Count == 1)
        {
            _logger.LogInformation(
                """
                Tournament skipped

                Reason=Only one eligible candidate

                Strategy={strategy}

                Fitness={fitness:F3}
                """,
                candidates[0].Strategy.StrategyVersion,
                candidates[0].Fitness);

            return candidates[0];
        }

        // ==========================================
        // REMOVE ALREADY SELECTED PARENT
        // ==========================================

        var available =
            candidates
                .Where(x =>
                    x.Strategy.StrategyVersion !=
                    excludeStrategy)
                .ToList();

        // ==========================================
        // SAFETY FALLBACK
        // ==========================================

        if (available.Count == 0)
        {
            available = candidates;
        }

        // ==========================================
        // BUILD TOURNAMENT
        // ==========================================

        int tournamentCount =
            Math.Min(
                TournamentSize,
                available.Count);


        var tournament =
            available
                .OrderBy(x =>
                    Random.Shared.Next())
                .Take(tournamentCount)
                .ToList();

        var winner =
            tournament
                .OrderByDescending(x =>
                    x.Fitness)
                .First();

        _logger.LogInformation(
            """
            Tournament completed
            
            Participants={participants}
            
            Winner={winner}
            
            WinnerFitness={fitness:F3}
            
            """,
            string.Join(", ",
                tournament.Select(x =>
                    x.Strategy.StrategyVersion)),
            winner.Strategy.StrategyVersion,
            winner.Fitness);

        return winner;
    }
}
