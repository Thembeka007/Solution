using Infrastructure.Entities;

namespace Ingestion.Services.SmartMoney.Processing;

public class TipsterWeightCalculator
{
    public decimal Calculate(
    Tipster tipster)
    {
        decimal score = 0;

        // WIN RATE

        score +=
            tipster.WinRate * 0.50m;

        // ROI

        score +=
            tipster.ROI * 0.25m;

        // PROFIT

        score +=
            tipster.Profit * 0.15m;

        // SAMPLE SIZE

        score +=
            tipster.TotalPredictions
            * 0.10m;

        return score;
    }
}
