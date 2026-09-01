using Ingestion.Services.IngestionEngine.DTOs;

namespace Ingestion.Services.IngestionEngine.Abstractions;

public interface IMatchFetcher
{
    Task<List<ExternalMatchDto>> FetchMatchesAsync(DateTime date);
}
