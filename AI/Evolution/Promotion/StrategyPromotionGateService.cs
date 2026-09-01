using Microsoft.Extensions.Logging;

namespace AI.Evolution.Promotion;

public class StrategyPromotionGateService
{
    private readonly ILogger<StrategyPromotionGateService>
        _logger;
    
    private readonly StrategyPromotionSettings
        _settings;

    public StrategyPromotionGateService(
        ILogger<StrategyPromotionGateService> logger,
        StrategyPromotionSettings settings)
    {
        _logger = logger;
        _settings = settings;
    }

    public PromotionGateResult Evaluate(
        StrategyPromotionCandidate candidate)
    {
        var performance =
            candidate.Performance;
        
        var risk =
            candidate.RiskMetrics;
        
        var result =
            new PromotionGateResult
            {
                StrategyVersion =
                    performance.StrategyVersion
            };

        // =========================================
        // MATURITY
        // =========================================

        if (performance.IsMature &&
            performance.TotalPredictions >=
            _settings.MinimumPredictions)
        {
            result.MaturityPassed =
                true;
            
            result.PassedGates
                .Add("Maturity");
        }
        else
        {
            result.Failures.Add(
                $"Maturity failed. " +
                $"Predictions={performance.TotalPredictions}, " +
                $"Required={_settings.MinimumPredictions}");
        }

        // =========================================
        // PROFITABILITY
        // =========================================

        if (performance.ROI >= _settings.MinimumROI)
        {
            result.ProfitabilityPassed =
                true;
            
            result.PassedGates
                .Add("Profitability");
        
        }
        else
        {
            result.Failures.Add(
                $"ROI failed. " +
                $"ROI={performance.ROI:F2}, " +
                $"Required={_settings.MinimumROI:F2}");
        }

        // =========================================
        // MINIMUM PROFIT
        // =========================================

        if (performance.Profit >= _settings.MinimumProfit)
        {
            result.MinimumProfitPassed =
                true;
            
            result.PassedGates
                .Add("Minimum Profit");
        }
        else
        {
            result.Failures.Add(
                $"Minimum profit failed. " +
                $"Profit={performance.Profit:F2}, " +
                $"Required={_settings.MinimumProfit:F2}");
        }

        // =========================================
        // ACCURACY
        // =========================================

        if (performance.Accuracy >= _settings.MinimumAccuracy)
        {
            result.AccuracyPassed =
                true;
            
            result.PassedGates
                .Add("Accuracy");
        }
        else
        {
            result.Failures.Add(
                $"Accuracy failed. " +
                $"Accuracy={performance.Accuracy:F2}, " +
                $"Required={_settings.MinimumAccuracy:F2}");
        
        }

        // =========================================
        // MAXIMUM DRAWDOWN
        // =========================================

        if (risk.MaximumDrawdownPercentage <= _settings.MaximumDrawdownPercentage)
        {
            result.MaximumDrawdownPassed =
                true;
            
            result.PassedGates
                .Add("Maximum Drawdown");
        }
        else
        {
            result.Failures.Add(
                $"Maximum drawdown failed. " +
                $"Drawdown={risk.MaximumDrawdownPercentage:F2}%, " +
                $"Maximum={_settings.MaximumDrawdownPercentage:F2}%");
        }

        // =========================================
        // OUT OF SAMPLE
        // =========================================

        if (candidate.OutOfSamplePredictions >=
            _settings.MinimumPredictions &&
            candidate.OutOfSampleROI >=
            _settings.MinimumROI)
        {
            result.OutOfSamplePassed =
                true;
            
            result.PassedGates
                .Add("Out-of-Sample");
        }
        else
        {
            result.Failures.Add(
                $"Out-of-sample profitability failed. " +
                $"Predictions={candidate.OutOfSamplePredictions}, " +
                $"ROI={candidate.OutOfSampleROI:F2}");
        }

        // =========================================
        // CONSISTENCY
        // =========================================

        if (candidate.EvaluationWindows > 0 &&
            candidate.ProfitableEvaluationWindows ==
            candidate.EvaluationWindows)
        {
            result.ConsistencyPassed =
                true;
            
            result.PassedGates
                .Add("Consistency");
        }
        else
        {
            result.Failures.Add(
                $"Consistency failed. " +
                $"ProfitableWindows={candidate.ProfitableEvaluationWindows}/" +
                $"{candidate.EvaluationWindows}");
        }

        // =========================================
        // CAPITAL RISK
        // =========================================

        if (candidate.EstimatedCapitalExposure <= _settings.MaximumCapitalExposure)
        {
            result.CapitalRiskPassed =
                true;
            
            result.PassedGates
                .Add("Capital Risk");
        }
        else
        {
            result.Failures.Add(
                $"Capital exposure failed. " +
                $"Exposure={candidate.EstimatedCapitalExposure:F2}, " +
                $"Maximum={_settings.MaximumCapitalExposure:F2}");
        }

        // =========================================
        // FINAL DECISION
        // =========================================

        result.Passed =
            result.MaturityPassed &&
            result.ProfitabilityPassed &&
            result.MinimumProfitPassed &&
            result.AccuracyPassed &&
            result.MaximumDrawdownPassed &&
            result.OutOfSamplePassed &&
            result.ConsistencyPassed &&
            result.CapitalRiskPassed;
        
        LogResult(result);
        
        return result;

    }
    private void LogResult(
        PromotionGateResult result)
    {
        _logger.LogInformation(
            """
            ========================================
            
            PROMOTION GATE RESULT
            
            Strategy={strategy}
            
            Passed={passed}
            
            Maturity={maturity}
            
            Profitability={profitability}
            
            MinimumProfit={minimumProfit}
            
            Accuracy={accuracy}
            
            MaximumDrawdown={drawdown}
            
            OutOfSample={outOfSample}
            
            Consistency={consistency}
            
            CapitalRisk={capitalRisk}
            
            ======================================== 
            """,
            result.StrategyVersion,
            result.Passed,
            result.MaturityPassed,
            result.ProfitabilityPassed,
            result.MinimumProfitPassed,
            result.AccuracyPassed,
            result.MaximumDrawdownPassed,
            result.OutOfSamplePassed,
            result.ConsistencyPassed,
            result.CapitalRiskPassed);

        foreach (var failure in result.Failures)
        {
            _logger.LogWarning(
                "Promotion gate failure: {failure}",
                failure);
        }

    }
}
