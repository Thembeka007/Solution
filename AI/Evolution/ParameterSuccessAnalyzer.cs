using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AI.Evolution;

public class ParameterSuccessAnalyzer
{
    private readonly SportsBettingDbContext _db;

    private readonly ILogger<ParameterSuccessAnalyzer>
        _logger;

    public ParameterSuccessAnalyzer(
        SportsBettingDbContext db,
        ILogger<ParameterSuccessAnalyzer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<decimal> GetSuccessScoreAsync(
        string parameterName)
    {
        var history =
            await _db.Set<MutationOutcome>()
                .Where(x =>
                    x.ParameterName ==
                    parameterName)
                .OrderByDescending(x =>
                    x.MutationUtc)
                .Take(50)
                .ToListAsync();

        if (!history.Any())
        {
            _logger.LogInformation(
                "No mutation history found for {parameter}. Default score = 0.50",
                parameterName);

            return 0.50m;
        }

        // =========================================
        // SUCCESS RATE
        // =========================================

        decimal successRate =
            history.Count(x => x.Successful)
            /
            (decimal)history.Count;

        // =========================================
        // AVERAGE IMPROVEMENT
        // =========================================

        decimal averageImprovement =
            history.Average(x =>
                x.Improvement);

        // =========================================
        // NORMALIZE IMPROVEMENT
        //
        // -20 ROI -> 0
        // 0 ROI -> 0.50
        // +20 ROI -> 1
        // =========================================

        decimal normalizedImprovement =
            (averageImprovement + 20m) /
            40m;

        normalizedImprovement =
            Math.Clamp(
                normalizedImprovement,
                0m,
                1m);

        // =========================================
        // FINAL SCORE
        // =========================================

        decimal score =
            (successRate * 0.70m)
            +
            (normalizedImprovement * 0.30m);

        score =
            Math.Clamp(
                score,
                0m,
                1m);

        _logger.LogInformation(
            """
            Parameter Success

            Parameter={parameter}

            Samples={samples}

            SuccessRate={success:P}

            AverageImprovement={improvement:F2}

            FinalScore={score:F2}
            """,
            parameterName,
            history.Count,
            successRate,
            averageImprovement,
            score);

        return score;
    }

    public async Task<Dictionary<string, decimal>>
        GetAllScoresAsync()
    {
        var parameters =
            await _db.Set<MutationOutcome>()
                .Select(x => x.ParameterName)
                .Distinct()
                .ToListAsync();

        var scores =
            new Dictionary<string, decimal>();

        foreach (var parameter in parameters)
        {
            scores[parameter] =
                await GetSuccessScoreAsync(
                    parameter);
        }

        return scores;
    }

    public async Task<string?> GetBestParameterAsync()
    {
        var scores =
            await GetAllScoresAsync();

        if (!scores.Any())
            return null;

        return scores
            .OrderByDescending(x => x.Value)
            .First()
            .Key;
    }
}
