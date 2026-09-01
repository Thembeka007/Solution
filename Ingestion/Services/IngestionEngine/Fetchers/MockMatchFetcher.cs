using Ingestion.Services.IngestionEngine.Abstractions;
using Ingestion.Services.IngestionEngine.DTOs;

namespace Ingestion.Services.IngestionEngine.Fetchers;

public class MockMatchFetcher : IMatchFetcher
{
    public Task<List<ExternalMatchDto>> FetchMatchesAsync(DateTime date)
    {
        var matches = new List<ExternalMatchDto>
        {
            new ExternalMatchDto
            {
                HomeTeam = "Arsenal",
                AwayTeam = "Chelsea",
                KickoffUtc = date.AddHours(15),
                Status = "Scheduled"
            }
        };

        return Task.FromResult(matches);
    }
}
