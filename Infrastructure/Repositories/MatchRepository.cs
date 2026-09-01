using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;
using SharedKernel.Enums;

namespace Infrastructure.Repositories;

public class MatchRepository
    : Repository<Match>
{
    public MatchRepository(
        SportsBettingDbContext context)
        : base(context)
    {
    }

    public async Task<List<Match>>
        GetUpcomingMatchesAsync()
    {
        return await _context.Matches
            .Where(x =>
                x.KickoffUtc > DateTime.UtcNow &&
                !(x.Status == MatchStatus.Completed))
            .OrderBy(x => x.KickoffUtc)
            .ToListAsync();
    }
}