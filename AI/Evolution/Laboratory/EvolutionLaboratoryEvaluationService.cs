using AI.Evolution.Laboratory.Services;
using AI.Evolution.Risk;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace AI.Evolution.Laboratory;

public class EvolutionLaboratoryEvaluationService
{
    private readonly ILogger<EvolutionLaboratoryEvaluationService>
        _logger;
    
    private readonly LaboratoryDatasetEvaluator
        _datasetEvaluator;

    private readonly StrategyRiskMetricsService
        _riskMetricsService;

    private readonly LaboratoryEvaluationSettings
        _settings;

    private readonly LaboratoryMarketDataEvaluator
        _marketDataEvaluator;

    public EvolutionLaboratoryEvaluationService(
        ILogger<EvolutionLaboratoryEvaluationService> logger,
        LaboratoryDatasetEvaluator datasetEvaluator,
        StrategyRiskMetricsService riskMetricsService,
        LaboratoryEvaluationSettings settings,
        LaboratoryMarketDataEvaluator marketDataEvaluator)
    {
        _logger = logger;
        _datasetEvaluator = datasetEvaluator;
        _riskMetricsService = riskMetricsService;
        _settings = settings;
        _marketDataEvaluator = marketDataEvaluator;
    }

    public async Task<LaboratoryEvaluationResult> EvaluateAsync(
        string strategyVersion,
        string strategyFamily,
        IEnumerable<Prediction> inSamplePredictions,
        IEnumerable<Prediction> validationPredictions,
        IEnumerable<Prediction> outOfSamplePredictions,
        int evaluationWindows,
        int profitableEvaluationWindows)
    {
        // =========================================
        // DATASET EVALUATION
        // =========================================

        var inSample =
            _datasetEvaluator
                .Evaluate(inSamplePredictions);

        var validation =
            _datasetEvaluator
                .Evaluate(validationPredictions);

        var outOfSample =
            _datasetEvaluator
                .Evaluate(outOfSamplePredictions);

        // =========================================
        // MARKET COVERAGE
        // =========================================

        var inSampleMarketData =
            await _marketDataEvaluator
                .EvaluateAsync(inSamplePredictions);

        var validationMarketData =
            await _marketDataEvaluator
                .EvaluateAsync(validationPredictions);

        var outOfSampleMarketData =
            await _marketDataEvaluator
                .EvaluateAsync(outOfSamplePredictions);

        // =========================================
        // RISK EVALUATION
        // =========================================

        var risk =
            await _riskMetricsService
                .CalculateAsync(strategyVersion);


        // =========================================
        // BUILD RESULT
        // =========================================

        var result =
            new LaboratoryEvaluationResult
            {
                StrategyVersion =
                    strategyVersion,

                StrategyFamily =
                    strategyFamily,

                // ------------------------------------
                // COVERRAGE
                // ------------------------------------

                InSampleMarketData =
                    inSampleMarketData,

                ValidationMarketData =
                    validationMarketData,

                OutOfSampleMarketData =
                    outOfSampleMarketData,

                // -----------------------------------
                // EDGE
                // -----------------------------------

                InSampleAverageMarketEdge =
                    inSampleMarketData.AverageProbabilityEdge,

                ValidationAverageMarketEdge =
                    validationMarketData.AverageProbabilityEdge,

                OutOfSampleAverageMarketEdge =
                    outOfSampleMarketData.AverageProbabilityEdge,

                // ---------------------------------
                // IN SAMPLE
                // ---------------------------------

                InSamplePredictions =
                    inSample.TotalPredictions,

                InSampleProfit =
                    inSample.Profit,

                InSampleROI =
                    inSample.ROI,

                InSampleAccuracy =
                    inSample.Accuracy,

                // ---------------------------------
                // VALIDATION
                // ---------------------------------

                ValidationPredictions =
                    validation.TotalPredictions,

                ValidationProfit =
                    validation.Profit,

                ValidationROI =
                    validation.ROI,

                ValidationAccuracy =
                    validation.Accuracy,

                // ---------------------------------
                // OUT OF SAMPLE
                // ---------------------------------

                OutOfSamplePredictions =
                    outOfSample.TotalPredictions,

                OutOfSampleProfit =
                    outOfSample.Profit,

                OutOfSampleROI =
                    outOfSample.ROI,

                OutOfSampleAccuracy =
                    outOfSample.Accuracy,

                // ---------------------------------
                // CONSISTENCY
                // ---------------------------------

                EvaluationWindows =
                    evaluationWindows,

                ProfitableEvaluationWindows =
                    profitableEvaluationWindows,

                // ---------------------------------
                // RISK
                // ---------------------------------

                MaximumDrawdown =
                    risk.MaximumDrawdown,

                MaximumDrawdownPercentage =
                    risk.MaximumDrawdownPercentage

            };

        // =========================================
        // IN-SAMPLE GATE
        // =========================================

        result.InSamplePassed =
            result.InSamplePredictions >=
                _settings.MinimumInSamplePredictions;

        if (!result.InSamplePassed)
        {
            result.Failures.Add(
                $"Insufficient in-sample predictions. " +
                $"Actual={result.InSamplePredictions}, " +
                $"Required={_settings.MinimumInSamplePredictions}");
        }
        
        // =========================================
        // VALIDATION GATE
        // =========================================

        result.ValidationPassed =
            result.ValidationPredictions >=
                _settings.MinimumValidationPredictions
            &&
            result.ValidationROI >=
                _settings.MinimumValidationROI
            &&
            result.ValidationAccuracy >=
                _settings.MinimumValidationAccuracy;

        if (!result.ValidationPassed)
        {
            result.Failures.Add(
                $"Validation failed. " +
                $"Predictions={result.ValidationPredictions}, " +
                $"ROI={result.ValidationROI:F2}, " +
                $"Accuracy={result.ValidationAccuracy:F2}");
        }

        // =========================================
        // OUT-OF-SAMPLE GATE
        // =========================================

        result.OutOfSamplePassed =
            result.OutOfSamplePredictions >=
            _settings.MinimumOutOfSamplePredictions
        &&
        result.OutOfSampleROI >=
            _settings.MinimumOutOfSampleROI
        &&
        result.OutOfSampleAccuracy >=
            _settings.MinimumOutOfSampleAccuracy;

        if (!result.OutOfSamplePassed)
        {
            result.Failures.Add(
                $"Out-of-sample failed. " +
                $"Predictions={result.OutOfSamplePredictions}, " +
                $"ROI={result.OutOfSampleROI:F2}, " +
                $"Accuracy={result.OutOfSampleAccuracy:F2}");
        }

        // =========================================
        // CONSISTENCY GATE
        // =========================================

        result.ConsistencyPassed =
            evaluationWindows >=
                _settings.MinimumEvaluationWindows
            &&
            profitableEvaluationWindows >=
                _settings.MinimumProfitableWindows;

        if (!result.ConsistencyPassed)
        {
            result.Failures.Add(
                $"Consistency failed. " +
                $"ProfitableWindows=" +
                $"{profitableEvaluationWindows}/" +
                $"{evaluationWindows}");
        }

        // =========================================
        // RISK GATE
        // =========================================
        
        result.RiskPassed =
            result.MaximumDrawdownPercentage <=
                _settings.MaximumAllowedDrawdownPercentage;
        
        if (!result.RiskPassed)
        {
            result.Failures.Add(
                $"Risk gate failed. " +
                $"MaximumDrawdown=" +
                $"{result.MaximumDrawdownPercentage:F2}%, " +
                $"MaximumAllowed=" +
                $"{_settings.MaximumAllowedDrawdownPercentage:F2}%");
        }

        // =========================================
        // FINAL PROMOTION DECISION
        // =========================================

        result.Passed =
            result.InSamplePassed
            &&
            result.ValidationPassed
            &&
            result.OutOfSamplePassed
            &&
            result.ConsistencyPassed
            &&
            result.RiskPassed;

        LogResult(result);

        return result;

    }

    private void LogResult(LaboratoryEvaluationResult result)
    {
        _logger.LogInformation(
            """
            ----------------------------------------
            MARKET DATA COVERAGE

            IN-SAMPLE

            TotalPredictions={inTotal}

            ValidMarketData={inValid}

            InsufficientMarketData={inInsufficient}

            InvalidMarketData={inInvalid}

            Coverage={inCoverage:F2}%

            ----------------------------------------
            VALIDATION

            TotalPredictions={validationTotal}

            ValidMarketData={validationValid}

            InsufficientMarketData={validationInsufficient}

            InvalidMarketData={validationInvalid}

            Coverage={validationCoverage:F2}%

            ----------------------------------------
            OUT-OF-SAMPLE

            TotalPredictions={outTotal}

            ValidMarketData={outValid}

            InsufficientMarketData={outInsufficient}

            InvalidMarketData={outInvalid}

            Coverage={outCoverage:F2}%

            ----------------------------------------
            """,
            result.InSampleMarketData?.TotalPredictions ?? 0,
            result.InSampleMarketData?.ValidMarketDataPredictions ?? 0,
            result.InSampleMarketData?.InsufficientMarketDataPredictions ?? 0,
            result.InSampleMarketData?.InvalidMarketDataPredictions ?? 0,
            result.InSampleMarketData?.MarketDataCoveragePercentage ?? 0m,

            result.ValidationMarketData?.TotalPredictions ?? 0,
            result.ValidationMarketData?.ValidMarketDataPredictions ?? 0,
            result.ValidationMarketData?.InsufficientMarketDataPredictions ?? 0,
            result.ValidationMarketData?.InvalidMarketDataPredictions ?? 0,
            result.ValidationMarketData?.MarketDataCoveragePercentage ?? 0m,

            result.OutOfSampleMarketData?.TotalPredictions ?? 0,
            result.OutOfSampleMarketData?.ValidMarketDataPredictions ?? 0,
            result.OutOfSampleMarketData?.InsufficientMarketDataPredictions ?? 0,
            result.OutOfSampleMarketData?.InvalidMarketDataPredictions ?? 0,
            result.OutOfSampleMarketData?.MarketDataCoveragePercentage ?? 0m);
    }
}
