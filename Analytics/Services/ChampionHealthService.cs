using Analytics.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Analytics.Services;

public class ChampionHealthService
{
    private readonly SportsBettingDbContext _db;

    private readonly ILogger<ChampionHealthService>
        _logger;

    private const decimal MinimumRequiredROI = 40m;

    public ChampionHealthService(
        SportsBettingDbContext db,
        ILogger<ChampionHealthService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<ChampionHealthResult>>
        CheckAsync()
    {
        var results =
            new List<ChampionHealthResult>();

        // =====================================
        // LOAD CURRENT LIVE CHAMPIONS
        // =====================================

        var champions =
            await _db.StrategyPerformances
                .Where(x =>
                    x.IsActive &&
                    x.IsElite)
                .ToListAsync();

        foreach (var champion in champions)
        {
            bool healthy =
                champion.ROI >=
                MinimumRequiredROI;

            var result =
                new ChampionHealthResult
                {
                    StrategyVersion =
                        champion.StrategyVersion,

                    StrategyFamily =
                        champion.StrategyFamily,

                    ROI =
                        champion.ROI,

                    Accuracy =
                        champion.Accuracy,

                    IsMature =
                        champion.IsMature,

                    IsActive =
                        champion.IsActive,

                    IsElite =
                        champion.IsElite,

                    MinimumRequiredROI =
                        MinimumRequiredROI,

                    IsHealthy =
                        healthy
                };

            results.Add(result);

            _logger.LogInformation(
                """
                ========================================

                Champion Health Check

                Family={family}

                Champion={champion}

                ROI={roi:F2}

                Accuracy={accuracy:F2}

                Mature={mature}

                Active={active}

                Elite={elite}

                MinimumRequiredROI={minimum:F2}

                Healthy={healthy}

                ========================================
                """,
                champion.StrategyFamily,
                champion.StrategyVersion,
                champion.ROI,
                champion.Accuracy,
                champion.IsMature,
                champion.IsActive,
                champion.IsElite,
                MinimumRequiredROI,
                healthy);

            if (!healthy)
            {
                _logger.LogWarning(
                    """
                    ========================================

                    CHAMPION FAILED PROFITABILITY CHECK

                    Family={family}

                    Champion={champion}

                    ROI={roi:F2}

                    MinimumRequiredROI={minimum:F2}

                    Status=FAILED

                    ReplacementStatus=NO_PROFITABLE_REPLACEMENT

                    Action=RETAINING_FALLBACK_AND_REQUESTING_LAB_CANDIDATES

                    ========================================
                    """,
                    champion.StrategyFamily,
                    champion.StrategyVersion,
                    champion.ROI,
                    MinimumRequiredROI);
            }
        }

        return results;
    }
}
