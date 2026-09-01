using Infrastructure.Data;
using Management.DTOs;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace Management.Services;

public class PerformanceAnalyzer
{
    private readonly SportsBettingDbContext _db;

    public PerformanceAnalyzer(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    // =====================================================
    // MAIN REPORT - DAILY
    // =====================================================

    public async Task<DailyPerformanceReportDto>
        GenerateReportAsync()
    {
        var slips =
            await _db.BettingSlips
                .Include(x => x.Selections)
                .Where(x => x.Settled)
                .OrderBy(x =>
                    x.CreatedDateUtc)
                .ToListAsync();

        decimal openingBalance = 0;

        decimal totalStake =
            slips.Sum(x => x.Stake);

        decimal totalPayout =
            slips.Sum(x => x.ActualPayout);

        decimal currentBalance =
            totalPayout - totalStake;

        int wins =
            slips.Count(x =>
                x.Won == true);

        int losses =
            slips.Count(x =>
                x.Won == false);

        int total =
            slips.Count;

        decimal roi = 0;

        if (totalStake > 0)
        {
            roi =
                (currentBalance /
                totalStake) * 100;
        }

        decimal winRate = 0;

        if (total > 0)
        {
            winRate =
                ((decimal)wins /
                total) * 100;
        }

        // =====================================================
        // WIN STREAK
        // =====================================================

        int streak = 0;

        int bestStreak = 0;

        foreach (var slip in slips)
        {
            if (slip.Won == true)
            {
                streak++;

                if (streak > bestStreak)
                {
                    bestStreak = streak;
                }
            }
            else
            {
                streak = 0;
            }
        }

        decimal biggestWin = 0;

        decimal biggestLoss = 0;

        if (slips.Any())
        {
            biggestWin =
                slips.Max(x =>
                    x.ActualPayout - x.Stake);

            biggestLoss =
                slips.Min(x =>
                    x.ActualPayout - x.Stake);
        }

        return new DailyPerformanceReportDto
        {
            OpeningBalance =
                openingBalance,

            CurrentBalance =
                currentBalance,

            DailyProfit =
                currentBalance -
                openingBalance,

            ROI =
                roi,

            TotalBets =
                total,

            Wins =
                wins,

            Losses =
                losses,

            WinRate =
                winRate,

            LongestWinStreak =
                bestStreak,

            BiggestWin =
                biggestWin,

            BiggestLoss =
                biggestLoss
        };
    }

    // =====================================================
    // SNAPSHOT GENERATION
    // =====================================================

    public async Task GenerateDailySnapshotAsync(
        DateTime date)
    {
        var report =
            await GenerateReportAsync();

        // =====================================================
        // CATEGORY ROI
        // =====================================================

        decimal breadRoi =
            await CalculateCategoryRoiAsync(
                "BREAD");

        decimal averageRoi =
            await CalculateCategoryRoiAsync(
                "AVERAGE");

        decimal lotteryRoi =
            await CalculateCategoryRoiAsync(
                "LOTTERY");

        // =====================================================
        // CONFIDENCE ANALYSIS
        // =====================================================

        var settledSelections =
            await _db.BettingSlipSelections
                .Where(x => x.Settled)
                .ToListAsync();

        decimal averageConfidence = 0;

        if (settledSelections.Any())
        {
            averageConfidence =
                settledSelections
                    .Average(x =>
                        x.Confidence);
        }

        decimal confidenceAccuracy = 0;

        var highConfidence =
            settledSelections
                .Where(x =>
                    x.Confidence >= 80)
                .ToList();

        if (highConfidence.Any())
        {
            int won =
                highConfidence
                    .Count(x => x.Won == true);

            confidenceAccuracy =
                ((decimal)won /
                highConfidence.Count) * 100;
        }

        // =====================================================
        // SNAPSHOT
        // =====================================================

        var snapshot =
            new PerformanceSnapshot
            {
                SnapshotDateUtc = date,

                // =========================================
                // FINANCIALS
                // =========================================

                OpeningBalance =
                    report.OpeningBalance,

                ClosingBalance =
                    report.CurrentBalance,

                ProfitLoss =
                    report.DailyProfit,

                ROI =
                    report.ROI,

                // =========================================
                // BETTING STATS
                // =========================================

                TotalSlips =
                    report.TotalBets,

                WonSlips =
                    report.Wins,

                LostSlips =
                    report.Losses,

                WinRate =
                    report.WinRate,

                // =========================================
                // STREAKS
                // =========================================

                WinningStreak =
                    report.LongestWinStreak,

                LosingStreak =
                    report.Losses,

                // =========================================
                // CATEGORY ROI
                // =========================================

                BreadROI =
                    breadRoi,

                AverageROI =
                    averageRoi,

                LotteryROI =
                    lotteryRoi,

                // =========================================
                // AI PERFORMANCE
                // =========================================

                AverageConfidence =
                    averageConfidence,

                ConfidenceAccuracy =
                    confidenceAccuracy,

                SmartMoneyAccuracy =
                    0,

                TrueFormAccuracy =
                    0,

                // =========================================
                // SYSTEM HEALTH
                // =========================================

                StrategyReplacementTriggered =
                    false,

                ActiveStrategyVersion =
                    "v1"
            };

        _db.PerformanceSnapshots
            .Add(snapshot);

        await _db.SaveChangesAsync();
    }

    // =====================================================
    // CATEGORY ROI
    // =====================================================

    private async Task<decimal>
        CalculateCategoryRoiAsync(
            string category)
    {
        var slips =
            await _db.BettingSlips
                .Where(x =>
                    x.Category == category &&
                    x.Settled)
                .ToListAsync();

        if (!slips.Any())
            return 0;

        decimal totalStake =
            slips.Sum(x => x.Stake);

        decimal totalPayout =
            slips.Sum(x =>
                x.ActualPayout);

        if (totalStake <= 0)
            return 0;

        decimal profit =
            totalPayout - totalStake;

        return
            (profit / totalStake) * 100;
    }

}
