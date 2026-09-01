using Ingestion.Services.SignalAggregation.Abstractions;
using Ingestion.Services.SignalAggregation.DTOs;

namespace Ingestion.Services.SignalAggregation.Processing;

public class SignalAggregator : ISignalAggregator
{
    public decimal CalculateConfidence(
    List<WeightedSignalDto> signals)
    {
        if (!signals.Any())
            return 0;

        decimal totalWeight =
            signals.Sum(x => x.Weight);

        decimal weightedScore =
            signals.Sum(x =>
                x.Weight * x.Score);

        return
            weightedScore / totalWeight;
    }
}
