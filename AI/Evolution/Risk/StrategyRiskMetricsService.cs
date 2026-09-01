using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace AI.Evolution.Risk;

public class StrategyRiskMetricsService
{
    private readonly SportsBettingDbContext _db;

    private readonly ILogger<StrategyRiskMetricsService>
        _logger;
    public StrategyRiskMetricsService(
        SportsBettingDbContext db,
        ILogger<StrategyRiskMetricsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<StrategyRiskMetrics> CalculateAsync(
        string strategyVersion)
    {
        var predictions =
            await _db.Predictions
                .Where(x =>
                    x.IsSettled &&
                    x.StrategyVersion ==
                    strategyVersion)
                .OrderBy(x =>
                    x.SettledUtc)
                .ToListAsync();

        var metrics =
            new StrategyRiskMetrics
            {
                StrategyVersion =
                    strategyVersion
            };

        if (!predictions.Any())
        {
            _logger.LogWarning(
                "No settled predictions available for" +
                "risk analysis." +

                "Strategy={strategy}",

                strategyVersion);

            return metrics;
        }

        // =========================================
        // BASIC METRICS
        // =========================================

        metrics.TotalPredictions =
            predictions.Count;
        
        metrics.TotalProfit =
            predictions.Sum(x =>
                x.ProfitLoss);
        
        var totalStake =
            predictions.Sum(x =>
                x.Stake);
        
        metrics.ROI =
            totalStake > 0
                ? (metrics.TotalProfit / totalStake) * 100m
                : 0m;

        // =========================================
        // EQUITY CURVE
        // =========================================

        decimal equity = 0m;
        
        decimal peakEquity = 0m;
        
        decimal maximumDrawdown = 0m;
        
        decimal worstEquity = 0m;

        foreach (var prediction in predictions)
        {
            equity +=
                prediction.ProfitLoss;

            // -------------------------------------
            // NEW EQUITY PEAK
            // -------------------------------------

            if (equity > peakEquity)
            {
                peakEquity =
                    equity;
            }

            // -------------------------------------
            // CURRENT DRAWDOWN
            // -------------------------------------

            var drawdown =
                peakEquity - equity;
            
            if (drawdown > maximumDrawdown)
            {
                maximumDrawdown =
                    drawdown;
            }

            // -------------------------------------
            // WORST EQUITY
            // -------------------------------------

            if (equity < worstEquity)
            {
                worstEquity =
                    equity;
            }
        }

        // =========================================
        // STORE EQUITY RESULTS
        // =========================================

        metrics.FinalEquity =
            equity;
        
        metrics.PeakEquity =
            peakEquity;
        
        metrics.MaximumDrawdown =
            maximumDrawdown;
        
        metrics.WorstEquity =
            worstEquity;

        // =========================================
        // DRAW DOWN PERCENTAGE
        // =========================================
        //
        // Use total capital deployed as the
        // denominator.
        //
        // This prevents a zero/negative equity
        // peak from producing a meaningless
        // percentage.
        // =========================================

        metrics.MaximumDrawdownPercentage =
            totalStake > 0
                ? (maximumDrawdown / totalStake) * 100m
                : 0m;

        _logger.LogInformation(
            """
            ========================================
            
            STRATEGY RISK METRICS
            
            Strategy={strategy}
            
            Predictions={predictions}
            
            Profit={profit:F2}
            
            ROI={roi:F2}
            
            PeakEquity={peak:F2}
            
            FinalEquity={final:F2}
            
            MaximumDrawdown={drawdown:F2}
            
            MaximumDrawdownPercentage={drawdownPercent:F2}

            WorstEquity={worst:F2}

            MaximumDrawdown={drawdown:F2}
            
            MaximumDrawdownPercentage={drawdownPercent:F2}
            
            ========================================
            
            """,
            strategyVersion,
            metrics.TotalPredictions,
            metrics.TotalProfit,
            metrics.ROI,
            metrics.PeakEquity,
            metrics.FinalEquity,
            metrics.MaximumDrawdown,
            metrics.MaximumDrawdownPercentage,
            metrics.WorstEquity,
            metrics.MaximumDrawdown,
            metrics.MaximumDrawdownPercentage);
        
        return metrics;


    }

}
