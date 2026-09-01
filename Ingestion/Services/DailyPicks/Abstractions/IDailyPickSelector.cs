using SharedKernel.Entities;

namespace Ingestion.Services.DailyPicks.Abstractions;

public interface IDailyPickSelector
{
    Task<List<Prediction>>
    GetTopPicksAsync(DateTime date);
}
