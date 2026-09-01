using Ingestion.Services.SignalAggregation.DTOs;

namespace Ingestion.Services.SignalAggregation.Abstractions;

public interface ISignalAggregator
{
    decimal CalculateConfidence(
    List<WeightedSignalDto> signals);
}
