using Ingestion.Odds.DTOs;

namespace Ingestion.Odds.Abstractions;

public interface IOddsProvider
{
    Task<List<OddsSnapshotDto>> GetOddsAsync(string sportKey);
}
