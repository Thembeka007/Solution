using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Analytics.Services;

public class StrategyParameterService
{
    private readonly SportsBettingDbContext _db;

    private readonly ILogger<StrategyParameterService>
        _logger;

    public StrategyParameterService(
        SportsBettingDbContext db,
        ILogger<StrategyParameterService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<decimal> GetParameterAsync(
        string strategy,
        string parameter,
        decimal defaultValue)
    {
        var existing =
            await _db.StrategyParameters
                .FirstOrDefaultAsync(x =>
                    x.StrategyVersion == strategy &&
                    x.ParameterName == parameter);

        if (existing is null)
        {
            existing =
                new StrategyParameter
                {
                    StrategyVersion =
                        strategy,

                    ParameterName =
                        parameter,

                    Value =
                        defaultValue,

                    LastUpdatedUtc =
                        DateTime.UtcNow
                };

            _db.StrategyParameters
                .Add(existing);

            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Created parameter {strategy}:{parameter}={value}",
                strategy,
                parameter,
                defaultValue);

            return defaultValue;
        }

        return existing.Value;
    }

    public async Task SetParameterAsync(
        string strategy,
        string parameter,
        decimal value)
    {
        var existing =
            await _db.StrategyParameters
                .FirstOrDefaultAsync(x =>
                    x.StrategyVersion == strategy &&
                    x.ParameterName == parameter);

        decimal oldValue;

        if (existing is null)
        {
            oldValue = value;

            existing =
                new StrategyParameter
                {
                    StrategyVersion =
                        strategy,

                    ParameterName =
                        parameter
                };

            _db.StrategyParameters
                .Add(existing);
        }
        else
        {
            oldValue =
                existing.Value;
        }

        existing.Value =
            value;

        existing.LastUpdatedUtc =
            DateTime.UtcNow;

        var history =
            new StrategyParameterHistory
            {
                StrategyVersion =
                    strategy,

                ParameterName =
                    parameter,

                OldValue =
                    oldValue,

                NewValue =
                    value,

                ChangedUtc =
                    DateTime.UtcNow,

                Reason =
                    "AI Mutation",

                ExpectedImprovement =
                    0m
            };

        _db.StrategyParameterHistories
            .Add(history);

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Updated parameter {strategy}:{parameter} from {old} to {new}",
            strategy,
            parameter,
            oldValue,
            value);      
    }
    

    public async Task AdjustParameterAsync(
        string strategy,
        string parameter,
        decimal delta,
        decimal defaultValue,
        decimal minimum,
        decimal maximum)
    {
        decimal current =
            await GetParameterAsync(
                strategy,
                parameter,
                defaultValue);

        decimal updated =
            current + delta;

        updated =
            Math.Clamp(
                updated,
                minimum,
                maximum);

        await SetParameterAsync(
            strategy,
            parameter,
            updated);

        _logger.LogInformation(
            "{strategy}:{parameter} adjusted from {old} to {new}",
            strategy,
            parameter,
            current,
            updated);

        var performance =
            await _db.StrategyPerformances
                .FirstOrDefaultAsync(x =>
                x.StrategyVersion == strategy);

        if (performance != null)
        {
            _db.PendingMutations.Add(
                new PendingMutation
                {
                    StrategyVersion = strategy,
                    ParameterName = parameter,
                    OldValue = current,
                    NewValue = updated,
                    StartingROI = performance.ROI,
                    AppliedUtc = DateTime.UtcNow,
                    Evaluated = false
                });

            await _db.SaveChangesAsync();
        }

        _logger.LogInformation(
            "{strategy}:{parameter} adjusted from {old} to {new}",
            strategy,
            parameter,
            current,
            updated);
    }

    public async Task<List<StrategyParameter>>
        GetAllAsync(string strategy)
    {
        return await _db
            .StrategyParameters
            .Where(x =>
                x.StrategyVersion ==
                strategy)
            .OrderBy(x =>
                x.ParameterName)
            .ToListAsync();
    }

    public async Task<StrategyPerformance?>
    GetPerformanceAsync(
        string strategyVersion)
    {
        return await _db.StrategyPerformances
            .FirstOrDefaultAsync(x =>
                x.StrategyVersion ==
                strategyVersion);
    }
}
