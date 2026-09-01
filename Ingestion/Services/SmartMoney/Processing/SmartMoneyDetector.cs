using Infrastructure.Entities;

namespace Ingestion.Services.SmartMoney.Processing;

public class SmartMoneyDetector
{
    public bool IsSmartMoney(
    Tipster tipster)
    {
        return
            tipster.WinRate >= 80 &&
            tipster.ROI >= 10 &&
            tipster.Profit > 0 &&
            tipster.TotalPredictions >= 100;
    }
}
