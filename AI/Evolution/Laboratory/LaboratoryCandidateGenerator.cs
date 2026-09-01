using AI.Evolution.Laboratory.Models;
using Microsoft.Extensions.Logging;

namespace AI.Evolution.Laboratory;

public class LaboratoryCandidateGenerator
{
    private readonly ILogger<LaboratoryCandidateGenerator>
        _logger;

    private readonly StrategyFamilyService
        _familyService;

    public LaboratoryCandidateGenerator(
        ILogger<LaboratoryCandidateGenerator> logger,
        StrategyFamilyService familyService)
    {
        _logger = logger;
        _familyService = familyService;
    }

    public List<LaboratoryStrategyCandidate> GenerateFromReference(
        string referenceStrategyVersion,
        IEnumerable<MutationPlan> mutationPlans,
        int generation)
    {
        var candidates =
            new List<LaboratoryStrategyCandidate>();

        var family =
            _familyService
                .GetFamily(referenceStrategyVersion);

        foreach (var plan in mutationPlans)
        {
            // =====================================
            // SAFETY
            // =====================================
            //
            // The mutation plan must NEVER replace
            // the live strategy.
            //
            // It creates a separate laboratory
            // candidate.
            // =====================================

            if (string.IsNullOrWhiteSpace(
                plan.StrategyVersion))
            {
                continue;
            }

            var candidateId =
                Guid.NewGuid();

            var candidateVersion =
                BuildCandidateVersion(
                    family,
                    candidateId);

            var changes =
                BuildChangeDescription(plan);

            var candidate =
                new LaboratoryStrategyCandidate
                {
                    CandidateId =
                        candidateId,

                    StrategyVersion =
                        candidateVersion,

                    StrategyFamily =
                        family,

                    ReferenceStrategyVersion =
                        referenceStrategyVersion,

                    Generation =
                        generation,

                    GenerationMethod =
                        "MUTATION",

                    Changes =
                        changes,

                    Status =
                        "CREATED"
                };

            candidates.Add(candidate);

            _logger.LogInformation(
                """
                ========================================

                LABORATORY CANDIDATE CREATED

                Candidate={candidate}

                Family={family}

                Reference={reference}

                Generation={generation}

                Method={method}

                Changes={changes}

                ========================================
                """,
                candidate.StrategyVersion,
                candidate.StrategyFamily,
                candidate.ReferenceStrategyVersion,
                candidate.Generation,
                candidate.GenerationMethod,
                string.Join(
                    " | ",
                    candidate.Changes));
        }

        return candidates;
    }

    private static string BuildCandidateVersion(
        string family,
        Guid candidateId)
    {
        return
            $"{family}_LAB_{candidateId:N}";
    }

    private static List<string>
        BuildChangeDescription(
            MutationPlan plan)
    {
        var changes =
            new List<string>();

        if (plan.IncreaseFormWeight)
        {
            changes.Add(
                "IncreaseFormWeight");
        }

        if (plan.IncreaseGoalWeight)
        {
            changes.Add(
                "IncreaseGoalWeight");
        }

        if (plan.IncreaseConfidenceThreshold)
        {
            changes.Add(
                "IncreaseConfidenceThreshold");
        }

        if (plan.ReduceConfidenceThreshold)
        {
            changes.Add(
                "ReduceConfidenceThreshold");
        }

        if (plan.DecreaseHomeAdvantage)
        {
            changes.Add(
                "DecreaseHomeAdvantage");
        }

        return changes;
    }
}
