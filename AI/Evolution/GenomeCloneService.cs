using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace AI.Evolution;

public class GenomeCloneService
{
    private readonly SportsBettingDbContext _db;
    
    private readonly GenomeBuilderService _genomeBuilder;
    
    private readonly ILogger<GenomeCloneService> _logger;

    private readonly StrategyFamilyService _familyService;

    public GenomeCloneService(
        SportsBettingDbContext db,
        GenomeBuilderService genomeBuilder,
        ILogger<GenomeCloneService> logger,
        StrategyFamilyService familyService)
    {
        _db = db;
        _genomeBuilder = genomeBuilder;
        _logger = logger;
        _familyService = familyService;
    }

    // =====================================
    // CLONE EXISTING STRATEGY
    // =====================================

    public async Task<string> CloneAsync(
        string sourceStrategy)
    {
        var genome =
            await _genomeBuilder
                .BuildAsync(sourceStrategy);

        return await CloneFromGenomeAsync(
            _familyService.GetFamily(
                sourceStrategy),
             sourceStrategy,
             genome.Parameters);
    }

    // =====================================
    // CREATE CHILD FROM SUPPLIER
    // =====================================

    public async Task<string> CloneFromGenomeAsync(
        string family,
        string parentStrategyVersion,
        Dictionary<string, decimal> genome,
        string? secondParentStrategyVersion = null)
    {
        var newVersion =
            $"{family}_MUTATED_{DateTime.UtcNow.Ticks}";

        // ===================================
        // LOAD PRIMARY PARENT
        // ===================================

        var parent =
            await _db.StrategyPerformances
                .FirstOrDefaultAsync(x =>
                    x.StrategyVersion ==
                    parentStrategyVersion);

        // ===================================
        // LOAD SECOND PARENT
        // ===================================

        StrategyPerformance? secondParent = null;

        if (!string.IsNullOrWhiteSpace(
                secondParentStrategyVersion))
        { 

            secondParent =
                await _db.StrategyPerformances
                    .FirstOrDefaultAsync(x =>
                        x.StrategyVersion ==
                        secondParentStrategyVersion);

        }

        // ===================================
        // CREATE CHILD PARAMETERS
        // ===================================

        foreach (var parameter in genome)
        {
            _db.StrategyParameters.Add(
                new StrategyParameter
                {
                    StrategyVersion =
                        newVersion,

                    ParameterName =
                        parameter.Key,

                    Value =
                        parameter.Value,

                    LastUpdatedUtc =
                        DateTime.UtcNow
                });
        }

        // ===================================
        // CREATE PERFORMANCE RECORD
        // ===================================

        _db.StrategyPerformances.Add(
            new StrategyPerformance
            {
                StrategyVersion =
                    newVersion,

                StrategyFamily =
                    family,

                ParentStrategyId =
                    parent?.Id,

                SecondParentStrategyId =
                    secondParent?.Id,

                Generation =
                    parent == null
                        ? 0
                        : parent.Generation + 1,

                ROI =
                    0,

                Accuracy =
                    0,

                Profit =
                    0,

                TotalPredictions =
                    0,

                CorrectPredictions =
                    0,

                IncorrectPredictions =
                    0,

                AverageConfidence =
                    0,

                ConfidenceAccuracyGap =
                    0,

                ReplacementTriggered =
                    false,

                IsActive =
                    false,

                IsMature =
                    false,

                IsElite =
                    false,

                LaboratoryPassed =
                    false,

                LaboratoryEvaluatedUtc =
                    null,

                LastEvaluationUtc =
                    DateTime.UtcNow
            });

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            """
            Genome created

            ParentA={parentA}

            ParentB={parentB}

            Child={child}

            Family={family}

            Generation={generation}

            Parameters={count}
            """,
            parentStrategyVersion,
            secondParentStrategyVersion ?? "None",
            newVersion,
            family,
            parent?.Generation + 1 ?? 0,
            genome.Count);

        return newVersion;
    }

}
