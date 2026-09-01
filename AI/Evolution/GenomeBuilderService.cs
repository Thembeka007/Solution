using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AI.Evolution;

public class GenomeBuilderService
{
    private readonly SportsBettingDbContext _db;
    
    private readonly ILogger<GenomeBuilderService> _logger;

    private static readonly Dictionary<string, decimal> DefaultParameters =
        new()
    {
        ["AttackBonus"] = 1.00m,
        ["EdgeThreshold"] = 1.00m,
        ["ConfidenceThreshold"] = 60m,
        ["HomeAdvantageMultiplier"] = 1.10m,
        ["WeakDefenceBonus"] = 0.25m
    };

    public GenomeBuilderService(
        SportsBettingDbContext db,
        ILogger<GenomeBuilderService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<StrategyGenome> BuildAsync(
        string strategyVersion)
    {
        var parameters =
            await _db.StrategyParameters
                .Where(x => x.StrategyVersion
                    == strategyVersion)
                .OrderBy(x => x.ParameterName)
                .ToListAsync();
        
        var genome =
            new StrategyGenome
            {
                StrategyVersion =
                    strategyVersion,

                Parameters =
                    new Dictionary<string, decimal>(DefaultParameters)
            };
        
        foreach (var parameter in parameters)
        {
            genome.Parameters[parameter.ParameterName] =
                parameter.Value;
        }

        int loadedCount = parameters.Count;

        _logger.LogInformation(
            """
            Genome built
            
            Strategy={strategy}

            LoadedParameters={loaded}

            FinalParameters={final}          
            """,
            strategyVersion,
            loadedCount,
            genome.Parameters.Count);
        
        return genome;
    }

    public async Task<List<StrategyGenome>>
        BuildAllAsync()
    {
        var strategies =
            await _db.StrategyParameters
                .Select(x => x.StrategyVersion)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        
        var genomes =
            new List<StrategyGenome>();

        foreach (var strategy in strategies)
        {
            genomes.Add(
                await BuildAsync(strategy));
        }

        _logger.LogInformation(
            "Built {count} strategy genomes.",
            genomes.Count);
        
        return genomes;
    }
}

