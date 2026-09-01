using Analytics.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Services;

public class TrueFormAnalyzer
{
    private readonly SportsBettingDbContext _db;
    public TrueFormAnalyzer(
    SportsBettingDbContext db)
    {
        _db = db;
    }
    public async Task<TrueFormResult>
    AnalyzeAsync(Guid teamId)
    {
        var forms =
            await _db.TeamForms
                .Where(x =>
                    x.TeamId == teamId)
                .OrderByDescending(x =>
                    x.MatchDateUtc)
                .Take(10)
                .ToListAsync();

        if (!forms.Any())
        {
            return new TrueFormResult();
        }

        // =========================
        // OVERALL FORM (10 MATCHES)
        // =========================

        var overallMomentum =
            forms.Average(x =>
            (decimal)x.Points);

        // =========================
        // TRUEFORM (LAST 3)
        // =========================

        var trueForm =
            forms.Take(3).ToList();

        var trueMomentum =
            trueForm.Average(x =>
            (decimal)x.Points);

        // =========================
        // OVERRIDE DETECTION
        // =========================

        var difference =
            (decimal)Math.Abs(
                trueMomentum -
                overallMomentum);

        var overrideTriggered =
            difference >= 1.0m;

        // =========================
        // HOT STREAK DETECTION
        // =========================

        var hotStreak =
            trueForm.All(x =>
                x.Points == 3);

        // =========================
        // COLLAPSE DETECTION
        // =========================

        var collapse =
            overallMomentum >= 2 &&
            trueMomentum <= 0.5m;

        // =========================
        // CONFIDENCE ADJUSTMENT
        // =========================

        decimal confidenceAdjustment = 0;

        if (hotStreak)
        {
            confidenceAdjustment += 8;
        }

        if (collapse)
        {
            confidenceAdjustment -= 12;
        }

        if (overrideTriggered)
        {
            confidenceAdjustment += 5;
        }

        return new TrueFormResult
        {
            OverallMomentum =
                overallMomentum,

            TrueMomentum =
                trueMomentum,

            OverrideTriggered =
                overrideTriggered,

            HotStreak =
                hotStreak,

            CollapseDetected =
                collapse,

            ConfidenceAdjustment =
                confidenceAdjustment
        };
    }
}