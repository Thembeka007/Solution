using Analytics.Services;
using Microsoft.Extensions.Logging;

namespace AI.Evolution;

public class StrategyMutationExecutor
{
    private readonly StrategyParameterService
        _parameters;

    private readonly ILogger<StrategyMutationExecutor>
        _logger;

    private readonly GenomeCloneService
        _genomeCloneService;

    private readonly CrossoverService
        _crossoverService;

    private readonly ParentSelectionService 
        _parentSelectionService;

    private readonly RandomGenomeService
        _randomGenomeService;

    private readonly DiversityService
        _diversityService;

    private readonly StrategyFamilyService
        _strategyFamilyService;

    private readonly AdaptiveMutationService
        _adaptiveMutationService;

    private readonly GenomeBuilderService 
        _genomeBuilder;

    public StrategyMutationExecutor(
        StrategyParameterService parameters,
        ILogger<StrategyMutationExecutor> logger,
        GenomeCloneService genomeCloneService,
        CrossoverService crossoverService,
        ParentSelectionService parentSelectionService,
        RandomGenomeService randomGenomeService,
        DiversityService diversityService,
        StrategyFamilyService strategyFamilyService,
        AdaptiveMutationService adaptiveMutationService,
        GenomeBuilderService genomeBuilder)
    {
        _parameters = parameters;
        _logger = logger;
        _genomeCloneService = genomeCloneService;
        _crossoverService = crossoverService;
        _parentSelectionService = parentSelectionService;
        _randomGenomeService = randomGenomeService;
        _diversityService = diversityService;
        _strategyFamilyService = strategyFamilyService;
        _adaptiveMutationService = adaptiveMutationService;
        _genomeBuilder = genomeBuilder;
    }

    public async Task ExecuteAsync(List<MutationPlan> plans)
    {
        foreach (var plan in plans)
        {
            decimal mutationRate =
                await _adaptiveMutationService
                    .GetMutationRateAsync(
                        plan.StrategyVersion);

            _logger.LogInformation(
                """
                Adaptive mutation

                Strategy={strategy}                

                MutationRate={rate:P0}

                """,
                plan.StrategyVersion,
                mutationRate);

            bool introduceRandom =
                _diversityService
                    .IntroduceRandomGenome();

            Dictionary<string, decimal> genome;
            string? parentA = null;
            string? parentB = null;

            if (introduceRandom)
            {
                _logger.LogInformation(
                    "Using random genome.");

                genome =
                    await _randomGenomeService
                        .CreateAsync(plan.StrategyVersion);

            }
            else
            {
                _logger.LogInformation(
                    """
                    Attempting evolutionary parent selection

                    Strategy={strategy}
                    """,
                    plan.StrategyVersion);

                var parents =
                    await _parentSelectionService
                        .SelectParentsAsync(
                            plan.StrategyVersion);

                // ==========================================
                // NORMAL EVOLUTIONARY CROSSOVER
                // ==========================================

                if (!string.IsNullOrWhiteSpace(parents.ParentA) &&
                    !string.IsNullOrWhiteSpace(parents.ParentB))
                {
                    parentA =
                        parents.ParentA;

                    parentB =
                        parents.ParentB;

                    _logger.LogInformation(
                        """
                        Use crossover

                        Strategy={strategy}

                        ParentA={parentA}

                        ParentB={parentB}
                        """,
                        plan.StrategyVersion,
                        parents.ParentA,
                        parents.ParentB);

                    genome =
                        await _crossoverService
                            .CreateGenomeAsync(
                                parents.ParentA,
                                parents.ParentB);

                }

                // ==========================================
                // BOOTSTRAP FROM REQUESTED STRATEGY
                // ==========================================
                //
                // No laboratory-approved evolutionary parents
                // exist yet.
                //
                // The requested strategy becomes the seed.
                //
                // This does NOT activate the seed.
                // GenomeCloneService creates the offspring as:
                //
                // IsActive = false
                // LaboratoryPassed = false
                // IsElite = false
                //
                // ==========================================
                else
                {
                    _logger.LogInformation(
                        """
                        No laboratory-approved parents available

                        Strategy={strategy}

                        Action=BootstrapFromStrategy
                        """,
                        plan.StrategyVersion);

                    parentA =
                        plan.StrategyVersion;

                    parentB =
                        null;

                    var seedGenome =
                        await _genomeBuilder
                            .BuildAsync(
                                plan.StrategyVersion);

                    genome =
                        seedGenome.Parameters;
                }

            }

            string family =
                _strategyFamilyService
                    .GetFamily(plan.StrategyVersion);

            string cloneVersion =
                await _genomeCloneService
                    .CloneFromGenomeAsync(
                        family,
                        parentA ?? plan.StrategyVersion,
                        genome,
                        parentB);

            decimal confidenceStep =
                mutationRate * 100m;

            decimal homeAdvantageStep =
                mutationRate / 2m;

            decimal edgePercentStep =
                mutationRate * 0.10m;

            _logger.LogInformation(
                """
                Mutation steps

                WeightStep={weight:F3}

                ConfidenceStep={confidence:F2}

                HomeAdvantageStep={home:F3}

                MinimumEdgePercentStep={edgePercent:F3}
                """,
                mutationRate,
                confidenceStep,
                homeAdvantageStep,
                edgePercentStep);

            // =======================================
            // EDGE THRESHOLD
            // =======================================

            if (plan.IncreaseFormWeight)
            {
                await _parameters
                    .AdjustParameterAsync(
                        cloneVersion,
                        "EdgeThreshold",
                        mutationRate,
                        1.50m,
                        0.25m,
                        3.00m);
            }
            if (plan.DecreaseFormWeight)
            {
                await _parameters
                    .AdjustParameterAsync(
                        cloneVersion,
                        "EdgeThreshold",
                        -mutationRate,
                        1.50m,
                        0.25m,
                        3.00m);
            }

            // =======================================
            // ATTACK BONUS
            // =======================================

            if (plan.IncreaseGoalWeight)
            {
                await _parameters
                    .AdjustParameterAsync(
                        cloneVersion,
                        "AttackBonus",
                        mutationRate,
                        1.00m,
                        0.25m,
                        3.00m);
            }

            if (plan.DecreaseGoalWeight)
            {
                await _parameters
                    .AdjustParameterAsync(
                        cloneVersion,
                        "AttackBonus",
                        -mutationRate,
                        1.00m,
                        0.25m,
                        3.00m);
            }

            // =======================================
            // CONFIDENCE THRESHOLD
            // =======================================
            
            if (plan.IncreaseConfidenceThreshold)
            {
                await _parameters
                    .AdjustParameterAsync(
                        cloneVersion,
                        "ConfidenceThreshold",
                        confidenceStep,
                        60m,
                        50m,
                        95m);
            }
            
            if (plan.ReduceConfidenceThreshold)
            {
                await _parameters
                    .AdjustParameterAsync(
                        cloneVersion,
                        "ConfidenceThreshold",
                        -confidenceStep,
                        60m,
                        50m,
                        95m);
            }

            // =======================================
            // MINIMUM EDGE PERCENT
            // =======================================

            if (plan.IncreaseMinimumEdgePercent)
            {
                await _parameters
                    .AdjustParameterAsync(
                        cloneVersion,
                        "MinimumEdgePercent",
                        mutationRate * 0.10m,
                        0.15m,
                        0.05m,
                        0.30m);
            }

            if (plan.DecreaseMinimumEdgePercent)
            {
                await _parameters
                    .AdjustParameterAsync(
                        cloneVersion,
                        "MinimumEdgePercent",
                        -(mutationRate * 0.10m),
                        0.15m,
                        0.05m,
                        0.30m);
            }


            // ===========================================
            // HOME ADVANTAGE
            // ===========================================

            if (plan.IncreaseHomeAdvantage)
            {
                await _parameters
                    .AdjustParameterAsync(
                        cloneVersion,
                        "HomeAdvantageMultiplier",
                        homeAdvantageStep,
                        1.15m,
                        1.00m,
                        1.50m);
            }

            if (plan.DecreaseHomeAdvantage)
            {
                await _parameters
                    .AdjustParameterAsync(
                        cloneVersion,
                        "HomeAdvantageMultiplier",
                        -homeAdvantageStep,
                        1.15m,
                        1.00m,
                        1.50m);
            }

            // ===============================================
            // H2H WEIGHT
            // ===============================================

            if (plan.IncreaseH2HWeight)
            {
                await _parameters
                    .AdjustParameterAsync(
                        cloneVersion,
                        "H2HWeight",
                        mutationRate,
                        1.00m,
                        0.25m,
                        3.00m);
            }

            if (plan.DecreaseH2HWeight)
            {
                await _parameters
                    .AdjustParameterAsync(
                        cloneVersion,
                        "H2HWeight",
                        -mutationRate,
                        1.00m,
                        0.25m,
                        3.00m);
            }

            // =======================================
            // WEAK DEFENCE BONUS
            // =======================================

            if (plan.IncreaseWeakDefenceBonus)
            {
                await _parameters
                    .AdjustParameterAsync(
                        cloneVersion,
                        "WeakDefenceBonus",
                        mutationRate,
                        0.25m,
                        0.00m,
                        3.00m);
            }

            if (plan.DecreaseWeakDefenceBonus)
            {
                await _parameters
                    .AdjustParameterAsync(
                        cloneVersion,
                        "WeakDefenceBonus",
                        -mutationRate,
                        0.25m,
                        0.00m,
                        3.00m);
            }

            // ==========================================
            // VERIFY MINIMUM EDGE PERCENT
            // ==========================================

            decimal resultingMinimumEdgePercent =
                await _parameters.GetParameterAsync(
                    cloneVersion,
                    "MinimumEdgePercent",
                    0.15m);

            _logger.LogInformation(
                """
                Offspring genome verification

                Strategy={strategy}

                MinimumEdgePercent={minimumEdge:P2}
                """,
                cloneVersion,
                resultingMinimumEdgePercent);



            _logger.LogInformation(
                "Mutation complete for {strategy}",
                cloneVersion);

            _logger.LogInformation(
                """
                Strategy offspring created

                ParentA={parentA}

                ParentB={parentB}

                Child={child}
                """,
                parentA ?? "None",
                parentB ?? "None",
                cloneVersion);
        }
    }
}
    