using Ingestion.Odds.Abstractions;
using Ingestion.Odds.DTOs;

namespace Ingestion.Odds.Providers;

public class MockOddsProvider : IOddsProvider
{
    public Task<List<OddsSnapshotDto>>
        GetOddsAsync(
            string sportKey)
    {
        return Task.FromResult(
            new List<OddsSnapshotDto>());
    }
}
