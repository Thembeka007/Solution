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

    public StrategyMutationExecutor(
        StrategyParameterService parameters,
        ILogger<StrategyMutationExecutor> logger,
        GenomeCloneService genomeCloneService,
        CrossoverService crossoverService,
        ParentSelectionService parentSelectionService,
        RandomGenomeService randomGenomeService,
        DiversityService diversityService,
        StrategyFamilyService strategyFamilyService,
        AdaptiveMutationService adaptiveMutationService)
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
                    Using crossover

                    Strategy={strategy}
                    """,
                    plan.StrategyVersion);

    

                var parents =
                    await _parentSelectionService
                        .SelectParentsAsync(
                            plan.StrategyVersion);

                genome =
                    await _crossoverService
                        .CreateGenomeAsync(
                            parents.ParentA,
                            parents.ParentB);

                _logger.LogInformation(
                    """
                    Parent selection finished

                    ParentA={parentA}

                    ParentB={parentB}
                    """,
                    parents.ParentA,
                    parents.ParentB);
            }

            string family =
                _strategyFamilyService
                    .GetFamily(plan.StrategyVersion);

            string cloneVersion =
                await _genomeCloneService
                    .CloneFromGenomeAsync(
                        family,
                        plan.StrategyVersion,
                        genome);

            _logger.LogInformation(
                """
                New offspring created

                Parent={parent}

                Child={child}
                """,
                plan.StrategyVersion,
                cloneVersion);

            decimal confidenceStep =
                mutationRate * 100m;

            decimal homeAdvantageStep =
                mutationRate / 2m;

            _logger.LogInformation(
                """
                Mutation steps

                WeightStep={weight:F3}

                ConfidenceStep={confidence:F2}

                HomeAdvantageStep={home:F3}
                """,
                mutationRate,
                confidenceStep,
                homeAdvantageStep);

            // =======================================s
            // EDGE THRESHOLD
            // =======================================

            if (plan.IncreaseFormWeight)
            {
                await _parameters
                    .AdjustParameterAsync(
                        cloneVersion,
                        "EdgeThreshold",
                        mutationRate,
                        1.00m,
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
                        1.00m,
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
                        1.10m,
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
                    1.10m,
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

            _logger.LogInformation(
                "Mutation complete for {strategy}",
                cloneVersion);

            _logger.LogInformation(
                """
                Strategy offspring created

                Parent={parent}

                Child={child}
                """,
                plan.StrategyVersion,
                cloneVersion);
        }
    }
}
    