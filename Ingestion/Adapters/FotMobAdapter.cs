using Ingestion.DTOs;

namespace Ingestion.Adapters;

public class FotMobAdapter : ISourceAdapter
{
    public string SourceName => "FotMob";

    public async Task<IEnumerable<MatchDto>> GetMatchesAsync(
        CancellationToken cancellationToken = default)
    {
        // TEMPORARY MOCK DATA

        await Task.Delay(100, cancellationToken);

        return new List<MatchDto>
        {
            new MatchDto
            {
                HomeTeam = "Arsenal",
                AwayTeam = "Chelsea",
                KickoffTime = DateTime.UtcNow.AddHours(5),
                HomeXG = 2.1m,
                AwayXG = 1.4m,
                HomeShots = 14,
                AwayShots = 11
            }
        };
    }
}
