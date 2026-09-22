using AI.Evolution.Laboratory.Models;
using Microsoft.Extensions.Logging;

namespace AI.Evolution.Laboratory.Services;

public class LaboratoryCandidateGateService
{
    private readonly LaboratoryEvaluationSettings
        _evaluationSettings;
    
    private readonly ILogger<LaboratoryCandidateGateService>
        _logger;

    public LaboratoryCandidateGateService(
        LaboratoryEvaluationSettings evaluationSettings,
        ILogger<LaboratoryCandidateGateService> logger)
    {
        _evaluationSettings =
            evaluationSettings;

        _logger = logger;
    }

    public LaboratoryCandidateGateResult Evaluate(
        LaboratoryCandidateDatasetResult inSample,
        LaboratoryCandidateDatasetResult validation,
        LaboratoryCandidateDatasetResult outOfSample)
    {
        var result =
            new LaboratoryCandidateGateResult();


        // =========================================
        // IN-SAMPLE DATA GATE
        // =========================================

        result.InSampleRiskPassed =
            inSample.Predictions >=
                _evaluationSettings
                    .MinimumInSamplePredictions;


        if (!result.InSampleDataPassed)
        {
            result.Failures.Add(
                $"In-Sample predictions below minimum. " +
                $"Required={_evaluationSettings.MinimumInSamplePredictions}, " +
                $"Actual={inSample.Predictions}");
        
        }

        // =========================================
        // VALIDATION DATA GATE
        // =========================================

        result.ValidationDataPassed =
            validation.Predictions >=
                _evaluationSettings
                    .MinimumValidationPredictions;

        if (!result.ValidationDataPassed)
        {
            result.Failures.Add(
                $"Validation predictions below minimum. " +
                $"Required={_evaluationSettings.MinimumValidationPredictions}, " +
                $"Actual={validation.Predictions}");

        }

        // =========================================
        // OUT-OF-SAMPLE DATA GATE
        // =========================================

        result.OutOfSampleDataPassed =
            outOfSample.Predictions >=
                _evaluationSettings
                    .MinimumOutOfSamplePredictions;

        if (!result.OutOfSampleDataPassed)
        {
            result.Failures.Add(
                $"Out-of-Sample predictions below minimum. " +
                $"Required={_evaluationSettings.MinimumOutOfSamplePredictions}, " +
                $"Actual={outOfSample.Predictions}");
        
        }

        result.DataPassed =
            result.InSampleDataPassed &&
            result.ValidationDataPassed &&
            result.OutOfSampleDataPassed;

        // =========================================
        // VALIDATION PROFITABILITY 
        // =========================================

        result.ValidationProfitabilityPassed =
            validation.ROI >=
                _evaluationSettings
                    .MinimumValidationROI;

        if (!result.ValidationProfitabilityPassed)
        {
            result.Failures.Add(
                $"Validation ROI below minimum. " +
                $"Required={_evaluationSettings.MinimumValidationROI:F2}, " +
                $"Actual={validation.ROI:F2}");

        }

        // =========================================
        // OUT-OF-SAMPLE PROFITABILITY
        // =========================================

        result.OutOfSampleProfitabilityPassed =
            outOfSample.ROI >=
                _evaluationSettings
                    .MinimumOutOfSampleROI;

        if (!result.OutOfSampleProfitabilityPassed)
        {
            result.Failures.Add(
                $"Out-of-Sample ROI below minimum. " +
                $"Required={_evaluationSettings.MinimumOutOfSampleROI:F2}, " +
                $"Actual={outOfSample.ROI:F2}");

        }

        result.ProfitabilityPassed =
            result.ValidationProfitabilityPassed &&
            result.OutOfSampleProfitabilityPassed;

        // =========================================
        // VALIDATION ACCURACY
        // =========================================

        result.ValidationAccuracyPassed =
            validation.Accuracy >=
                _evaluationSettings
                    .MinimumValidationAccuracy;

        if (!result.ValidationAccuracyPassed)
        {
            result.Failures.Add(
                $"Validation accuracy below minimum. " +
                $"Required={_evaluationSettings.MinimumValidationAccuracy:F2}, " +
                $"Actual={validation.Accuracy:F2}");
        
        }

        // =========================================
        // OUT-OF-SAMPLE ACCURACY
        // =========================================

        result.OutOfSampleAccuracyPassed =
            outOfSample.Accuracy >=
                _evaluationSettings
                    .MinimumOutOfSampleAccuracy;


        if (!result.OutOfSampleAccuracyPassed)
        {
            result.Failures.Add(
                $"Out-of-Sample accuracy below minimum. " +
                $"Required={_evaluationSettings.MinimumOutOfSampleAccuracy:F2}, " +
                $"Actual={outOfSample.Accuracy:F2}");
        
        }

        result.AccuracyPassed =
            result.ValidationAccuracyPassed &&
            result.OutOfSampleAccuracyPassed;

        // =========================================
        // RISK GATE
        // =========================================

        result.InSampleRiskPassed =
            inSample.RiskPassed;

        result.ValidationRiskPassed =
            validation.RiskPassed;

        result.OutOfSampleRiskPassed =
            outOfSample.RiskPassed;

        result.RiskPassed =
            result.InSampleRiskPassed &&
            result.ValidationRiskPassed &&
            result.OutOfSampleRiskPassed;

        if (!result.RiskPassed)
        {
            result.Failures.Add(
                "One or more laboratory datasets failed the risk gate.");
        
        }

        // =========================================
        // CONSISTENCY GATE
        // =========================================
        //
        // Consistency is evaluated from the validation
        // and out-of-sample datasets.
        //
        // The dataset evaluator already calculates
        // sequential evaluation windows and profitable
        // windows.
        //
        // We require the established minimums in both
        // forward-testing partitions.
        //
        // =========================================

        result.ValidationConsistencyPassed =
            validation.EvaluationWindows >=
                _evaluationSettings.MinimumEvaluationWindows &&
            validation.ProfitableEvaluationWindows >=
                _evaluationSettings.MinimumProfitableWindows;

        if (!result.ValidationConsistencyPassed)
        {
            result.Failures.Add(
                $"Validation consistency below minimum. " +
                $"RequiredWindows={_evaluationSettings.MinimumEvaluationWindows}, " +
                $"ActualWindows={validation.EvaluationWindows}, " +
                $"RequiredProfitableWindows={_evaluationSettings.MinimumProfitableWindows}, " +
                $"ActualProfitableWindows={validation.ProfitableEvaluationWindows}");
        
        }

        result.OutOfSampleConsistencyPassed =
            outOfSample.EvaluationWindows >=
                _evaluationSettings.MinimumEvaluationWindows &&
            outOfSample.ProfitableEvaluationWindows >=
                _evaluationSettings.MinimumProfitableWindows;


        if (!result.OutOfSampleConsistencyPassed)
        {
            result.Failures.Add(
                $"Out-of-Sample consistency below minimum. " +
                $"RequiredWindows={_evaluationSettings.MinimumEvaluationWindows}, " +
                $"ActualWindows={outOfSample.EvaluationWindows}, " +
                $"RequiredProfitableWindows={_evaluationSettings.MinimumProfitableWindows}, " +
                $"ActualProfitableWindows={outOfSample.ProfitableEvaluationWindows}");
        
        }

        result.ConsistencyPassed =
            result.ValidationConsistencyPassed &&
            result.OutOfSampleConsistencyPassed;

        // =========================================
        // FINAL CANDIDATE GATE
        // =========================================

        result.Passed =
            result.DataPassed &&
            result.ProfitabilityPassed &&
            result.AccuracyPassed &&
            result.RiskPassed &&
            result.ConsistencyPassed;


        // =========================================
        // DIAGNOSTICS
        // =========================================

        _logger.LogInformation(
            """            
            ========================================
            
            LABORATORY CANDIDATE GATE
            
            InSampleDataPassed={inSampleData}
            
            ValidationDataPassed={validationData}
            
            OutOfSampleDataPassed={outOfSampleData}
            
            DataPassed={dataPassed}
            
            ValidationProfitabilityPassed={validationProfitability}
            
            OutOfSampleProfitabilityPassed={outProfitability}
            
            ProfitabilityPassed={profitabilityPassed}
            
            ValidationAccuracyPassed={validationAccuracy}
            
            OutOfSampleAccuracyPassed={outAccuracy}
            
            AccuracyPassed={accuracyPassed}
            
            InSampleRiskPassed={inSampleRisk}
            
            ValidationRiskPassed={validationRisk}
            
            OutOfSampleRiskPassed={outRisk}
            
            RiskPassed={riskPassed}
            
            ValidationConsistencyPassed={validationConsistency}
            
            OutOfSampleConsistencyPassed={outConsistency}
            
            ConsistencyPassed={consistencyPassed}
            
            Passed={passed}
            
            Failures={failures}
            
            ========================================            
            """,
            result.InSampleDataPassed,
            result.ValidationDataPassed,
            result.OutOfSampleDataPassed,
            result.DataPassed,
            result.ValidationProfitabilityPassed,
            result.OutOfSampleProfitabilityPassed,
            result.ProfitabilityPassed,
            result.ValidationAccuracyPassed,
            result.OutOfSampleAccuracyPassed,
            result.AccuracyPassed,
            result.InSampleRiskPassed,
            result.ValidationRiskPassed,
            result.OutOfSampleRiskPassed,
            result.RiskPassed,
            result.ValidationConsistencyPassed,
            result.OutOfSampleConsistencyPassed,
            result.ConsistencyPassed,
            result.Passed,
            string.Join(
                " | ",
                result.Failures));

        return result;

    }
}
