using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;
using SharedKernel.Models;

namespace Analytics.SmartMoney;

public class ConfidenceEngine
{
    private readonly SportsBettingDbContext _db;

    public ConfidenceEngine(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<ConfidenceResult>
        CalculateAsync(
            Match match,
            decimal baseConfidence,
            decimal smartMoneyStrength,
            decimal trueFormAdjustment)
    {
        decimal confidence =
            baseConfidence;

        // =========================
        // SMART MONEY BOOST
        // =========================

        decimal smartMoneyBoost =
            smartMoneyStrength * 0.15m;

        confidence += smartMoneyBoost;

        // =========================
        // TRUEFORM BOOST
        // =========================

        confidence += trueFormAdjustment;

        // =========================
        // HISTORICAL ACCURACY
        // =========================

        var historicalPredictions =
            await _db.Predictions
                .Where(x =>
                    x.PredictionType ==
                    "BTTS")
                .Take(200)
                .ToListAsync();

        decimal historicalAccuracy = 0;

        if (historicalPredictions.Any())
        {
            historicalAccuracy =
                historicalPredictions
                    .Average(x =>
                        x.Confidence);
        }

        decimal historicalBoost =
            historicalAccuracy * 0.02m;

        confidence += historicalBoost;

        // =========================
        // LEAGUE WEIGHTING
        // =========================

        decimal leagueAdjustment = 0;

        if (match.LeagueName ==
            "Premier League")
        {
            leagueAdjustment = 3;
        }

        if (match.LeagueName ==
            "UEFA Champions League")
        {
            leagueAdjustment = 5;
        }

        confidence += leagueAdjustment;

        // =========================
        // TRAP DETECTION
        // =========================

        bool trapDetected = false;

        if (smartMoneyStrength < 20 &&
            baseConfidence > 80)
        {
            trapDetected = true;

            confidence -= 15;
        }

        // =========================
        // CONFIDENCE DECAY
        // =========================

        decimal decay = 0;

        if (confidence > 92)
        {
            decay = 4;

            confidence -= decay;
        }

        // =========================
        // HARD CAPS
        // =========================

        if (confidence > 99m)
            confidence = 99m;

        if (confidence < 1m)
            confidence = 1m;

        // =========================
        // RISK CLASSIFICATION
        // =========================

        string riskLevel = "HIGH_RISK";

        if (confidence >= 85m)
        {
            riskLevel = "LOW_RISK";
        }
        else if (confidence >= 70m)
        {
            riskLevel = "MEDIUM_RISK";
        }

        return new ConfidenceResult
        {

            BaseConfidence =
                baseConfidence,

            FinalConfidence =
                confidence,

            SmartMoneyBoost =
                smartMoneyBoost,

            TrueFormBoost =
                trueFormAdjustment,

            LeagueAdjustment =
                leagueAdjustment,

            HistoricalAccuracyBoost =
                historicalBoost,

            TrapDetected =
                trapDetected,

            ConfidenceDecay =
                decay,
            
            RiskLevel =
                riskLevel,

            IsAnomalyDetected =
                confidence < 40m
        };
    }
}
