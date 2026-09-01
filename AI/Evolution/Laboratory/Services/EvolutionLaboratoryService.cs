using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AI.Evolution.Laboratory.Services;

public class EvolutionLaboratoryService
{
    private readonly SportsBettingDbContext _db;

    private readonly EvolutionLaboratoryEvaluationService
        _evaluationService;

    private readonly ILogger<EvolutionLaboratoryService>
        _logger;

    private readonly LaboratoryWindowService
        _windowService;

    private readonly LaboratoryConsistencyService
        _consistencyService;

    public EvolutionLaboratoryService(
        SportsBettingDbContext db,
        EvolutionLaboratoryEvaluationService evaluationService,
        ILogger<EvolutionLaboratoryService> logger,
        LaboratoryWindowService windowService,
        LaboratoryConsistencyService consistencyService)
    {
        _db = db;
        _evaluationService = evaluationService;
        _logger = logger;
        _windowService = windowService;
        _consistencyService = consistencyService;
    }

    public async Task<LaboratoryEvaluationResult>
        EvaluateStrategyAsync(
            string strategyVersion,
            string strategyFamily)
    {
        // =========================================
        // LOAD HISTORICAL PREDICTIONS
        // =========================================

        var predictions =
            await _db.Predictions
                .Where(x =>
                    x.StrategyVersion ==
                    strategyVersion &&
                    x.IsSettled)
                .OrderBy(x =>
                    x.CreatedUtc)
                .ToListAsync();

        var orderedPredictions =
            predictions
                .OrderBy(x =>
                    x.CreatedUtc)
                .ThenBy(x =>
                    x.Id)
                .ToList();

        _logger.LogInformation(
            """
            Evolution Laboratory

            Strategy={strategy}

            Family={family}

            HistoricalPredictions={count}
            """,
            strategyVersion,
            strategyFamily,
            predictions.Count);

        if (predictions.Count == 0)
        {
            var emptyResult =
                await _evaluationService
                    .EvaluateAsync(
                        strategyVersion,
                        strategyFamily,
                        [],
                        [],
                        [],
                        0,
                        0);

            await PersistLaboratoryResultAsync(
                strategyVersion,
                emptyResult.Passed);

            return emptyResult;
        }

        // =========================================
        // BUILD CHRONOLOGICAL TIME WINDOWS
        // =========================================

        var windows =
            _windowService
                .Build(orderedPredictions);

        // =========================================
        // BUILD DATASETS FROM WINDOWS
        // =========================================

        var inSamplePredictions =
            orderedPredictions
                .Skip(
                    windows.InSampleStartIndex)
                .Take(
                    windows.InSampleEndIndex -
                    windows.InSampleStartIndex)
                .ToList();

        var validationPredictions =
            orderedPredictions
                .Skip(
                    windows.ValidationStartIndex)
                .Take(
                    windows.ValidationEndIndex -
                    windows.ValidationStartIndex)
                .ToList();

        var outOfSamplePredictions =
            orderedPredictions
                .Skip(
                    windows.OutOfSampleStartIndex)
                .Take(
                    windows.OutOfSampleEndIndex -
                    windows.OutOfSampleStartIndex)
                .ToList();

        _logger.LogInformation(
            """
            Laboratory chronological datasets

            Strategy={strategy}

            InSample={inSample}

            Validation={validation}

            OutOfSample={outOfSample}

            InSampleStart={inStart}

            InSampleEnd={inEnd}

            ValidationStart={validationStart}

            ValidationEnd={validationEnd}

            OutOfSampleStart={outStart}

            OutOfSampleEnd={outEnd}
            """,
            strategyVersion,
            inSamplePredictions.Count,
            validationPredictions.Count,
            outOfSamplePredictions.Count,
            windows.InSampleStartUtc,
            windows.InSampleEndUtc,
            windows.ValidationStartUtc,
            windows.ValidationEndUtc,
            windows.OutOfSampleStartUtc,
            windows.OutOfSampleEndUtc);

        // =========================================
        // CONSISTENCY EVALUATION
        // =========================================

        var consistency =
            _consistencyService
                .Evaluate(orderedPredictions);

        int evaluationWindows =
            consistency.EvaluationWindows;

        int profitableEvaluationWindows =
            consistency.ProfitableEvaluationWindows;

        _logger.LogInformation(
            """
            Laboratory consistency

            Strategy={strategy}

            EvaluationWindows={windows}

            ProfitableEvaluationWindows={profitable}
            """,
            strategyVersion,
            evaluationWindows,
            profitableEvaluationWindows);

        // =========================================
        // RUN LABORATORY EVALUATION
        // =========================================

        var result =
            await _evaluationService
                .EvaluateAsync(
                    strategyVersion,
                    strategyFamily,
                    inSamplePredictions,
                    validationPredictions,
                    outOfSamplePredictions,
                    evaluationWindows,
                    profitableEvaluationWindows);

        // =========================================
        // PERSIST LABORATORY RESULT
        // =========================================

        await PersistLaboratoryResultAsync(
            strategyVersion,
            result.Passed);

        _logger.LogInformation(
            """
            Evolution laboratory completed

            Strategy={strategy}

            Family={family}

            LaboratoryPassed={passed}

            """,
            strategyVersion,
            strategyFamily,
            result.Passed);

        return result;
    }

    private async Task PersistLaboratoryResultAsync(
        string strategyVersion,
        bool passed)
    {
        var performance =
            await _db.StrategyPerformances
                .FirstOrDefaultAsync(x =>
                    x.StrategyVersion ==
                    strategyVersion);

        if (performance == null)
        {
            _logger.LogWarning(
                """
                Laboratory result could not be persisted

                Strategy={strategy}

                LaboratoryPassed={passed}

                Reason=StrategyPerformance record not found
                """,
                strategyVersion,
                passed);

            return;
        }

        performance.LaboratoryPassed =
            passed;

        performance.LaboratoryEvaluatedUtc =
            DateTime.UtcNow;

        await _db
            .SaveChangesAsync();

        _logger.LogInformation(
            """
            Laboratory result persisted

            Strategy={strategy}

            LaboratoryPassed={passed}

            LaboratoryEvaluatedUtc={evaluated}
            """,
            strategyVersion,
            performance.LaboratoryPassed,
            performance.LaboratoryEvaluatedUtc);

    }
}
