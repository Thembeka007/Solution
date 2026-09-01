using AI.Features;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Optimization;

public class FeatureEngineeringService
{
    private readonly SportsBettingDbContext _db;

    public FeatureEngineeringService(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    public async Task<List<MatchFeatureVector>>
        GenerateTrainingSetAsync()
    {
        var matches =
            await _db.Matches
                .Where(x =>
                    x.HomeScore != null &&
                    x.AwayScore != null)
                .ToListAsync();

        var features =
            new List<MatchFeatureVector>();

        foreach (var match in matches)
        {
            var homeForms =
                await _db.TeamForms
                    .Where(x =>
                        x.TeamId == match.HomeTeamId)
                    .OrderByDescending(x =>
                        x.MatchDateUtc)
                    .Take(10)
                    .ToListAsync();

            var awayForms =
                await _db.TeamForms
                    .Where(x =>
                        x.TeamId == match.AwayTeamId)
                    .OrderByDescending(x =>
                        x.MatchDateUtc)
                    .Take(10)
                    .ToListAsync();

            if (!homeForms.Any() ||
                !awayForms.Any())
                continue;

            var vector =
                new MatchFeatureVector
                {
                    MatchId = match.Id,

                    HomeBTTSRate =
                        homeForms.Count(x => x.BTTS)
                        / (decimal)homeForms.Count,

                    AwayBTTSRate =
                        awayForms.Count(x => x.BTTS)
                        / (decimal)awayForms.Count,

                    HomeOver25Rate =
                        homeForms.Count(x => x.Over25)
                        / (decimal)homeForms.Count,

                    AwayOver25Rate =
                        awayForms.Count(x => x.Over25)
                        / (decimal)awayForms.Count,

                    HomeGoalsScoredAvg =
                        homeForms.Average(x =>
                            (decimal)x.GoalsFor),

                    AwayGoalsScoredAvg =
                        awayForms.Average(x =>
                            (decimal)x.GoalsFor),

                    HomeGoalsConcededAvg =
                        homeForms.Average(x =>
                            (decimal)x.GoalsAgainst),

                    AwayGoalsConcededAvg =
                        awayForms.Average(x =>
                            (decimal)x.GoalsAgainst),

                    BTTSWon =
                        match.HomeScore > 0 &&
                        match.AwayScore > 0,

                    Over25Won =
                        (match.HomeScore +
                         match.AwayScore) > 2,

                    HomeWon =
                        match.HomeScore >
                        match.AwayScore
                };

            features.Add(vector);
        }

        return features;
    }
}
