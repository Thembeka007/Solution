using Microsoft.EntityFrameworkCore;
using Ingestion.Services.IngestionEngine.Abstractions;
using SharedKernel.Enums;
using Infrastructure.Data;
using Services.IngestionEngine.Normalizers;

namespace Ingestion.Services.IngestionEngine.Processing;

public class IngestionService : IIngestionService
{
    private readonly IMatchFetcher _matchFetcher;
    private readonly MatchNormalizer _matchNormalizer;
    private readonly SportsBettingDbContext _db;

    public IngestionService(
        IMatchFetcher matchFetcher,
        MatchNormalizer matchNormalizer,
        SportsBettingDbContext db)
    {
        _matchFetcher = matchFetcher;
        _matchNormalizer = matchNormalizer;
        _db = db;
    }

    public async Task IngestMatchesAsync(DateTime date)
    {
        // 1. Fetch raw external data
        var externalMatches = await _matchFetcher.FetchMatchesAsync(date);

        // validate
        if (externalMatches == null || externalMatches.Count == 0)
            return;

        foreach (var dto in externalMatches)
        {
            // 2. Normalize → Domain entity
            var match = await _matchNormalizer.NormalizeAsync(dto);

            // 3. Validate required fields
            if (match.HomeTeamId == Guid.Empty ||
                match.AwayTeamId == Guid.Empty)
            {
                continue;
            }

            // 4. Deduplication check
            var exists = await _db.Matches.AnyAsync(x =>
                x.HomeTeamId == match.HomeTeamId &&
                x.AwayTeamId == match.AwayTeamId &&
                x.KickoffUtc == match.KickoffUtc);

            if (exists)
                continue;

            // 5. Set initial status safety (optional)
            if (match.Status == 0)
                match.Status = MatchStatus.Scheduled;

            // 6. Add to DbContext
            await _db.Matches.AddAsync(match);
        }

        // 7. Commit batch
        await _db.SaveChangesAsync();
    }
}
