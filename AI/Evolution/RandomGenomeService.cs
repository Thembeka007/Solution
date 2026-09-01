using Microsoft.Extensions.Logging;

namespace AI.Evolution;

public class RandomGenomeService
{
    private readonly GenomeBuilderService _genomeBuilder;

    private readonly ILogger<RandomGenomeService> _logger;

    public RandomGenomeService(
        GenomeBuilderService genomeBuilder,
        ILogger<RandomGenomeService> logger)
    {
        _genomeBuilder = genomeBuilder;
        _logger = logger;
    }

    public async Task<Dictionary<string, decimal>> CreateAsync(
        string strategyVersion)
    {
        var genome =
            await _genomeBuilder
                .BuildAsync(strategyVersion);

        var randomGenome =
            new Dictionary<string, decimal>();

        foreach (var parameter in genome.Parameters)
        {
            decimal value = parameter.Value;

            double factor =
                Random.Shared.NextDouble() * 0.40 - 0.20;

            value *= (decimal)(1 + factor);

            randomGenome[parameter.Key] = value;
        }

        _logger.LogInformation(
            """
            Random genome generated

            Base={strategy}

            Parameters={count}
            """,
            strategyVersion,
            randomGenome.Count);

        return randomGenome;
    }
}
