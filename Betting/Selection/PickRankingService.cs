using Betting.Configuration;
using Microsoft.Extensions.Options;
using SharedKernel.Entities;

namespace Betting.Selection;

public class PickRankingService
{
    private readonly ScoreWeightsOptions
        _weights;

    public PickRankingService(
        IOptions<ScoreWeightsOptions> options)
    {
        _weights = options.Value;
    }

    public decimal CalculateScore(
        Prediction prediction,
        decimal expectedValue,
        decimal edge,
        decimal kelly)
    {
        decimal confidenceScore =
            prediction.Confidence;

        decimal edgeScore =
            Math.Min(edge * 5m, 100m);

        decimal evScore =
            Math.Min(expectedValue * 100m, 100m);

        decimal kellyScore =
            Math.Min(kelly * 1000m, 100m);

        decimal total =
            confidenceScore * _weights.ConfidenceWeight
            + edgeScore * _weights.EdgeWeight
            + evScore * _weights.ExpectedValueWeight
            + kellyScore * _weights.KellyWeight;

        return Math.Round(total, 2);
    }
}
