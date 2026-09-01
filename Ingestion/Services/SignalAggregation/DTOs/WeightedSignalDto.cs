namespace Ingestion.Services.SignalAggregation.DTOs;

public class WeightedSignalDto
{
    public string SignalType { get; set; } = "";

    public decimal Weight { get; set; }

    public decimal Score { get; set; }
}
