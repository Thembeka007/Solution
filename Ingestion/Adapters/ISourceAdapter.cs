using Ingestion.DTOs;

namespace Ingestion.Adapters;

public interface ISourceAdapter
{
    string SourceName { get; }

    Task<IEnumerable<MatchDto>> GetMatchesAsync(
        CancellationToken cancellationToken = default);
}
