using Analytics.Models;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Analytics.Services;

public class GoalScoringService
{
    private readonly ILogger<GoalScoringService>
    _logger;

    public GoalScoringService(
        ILogger<GoalScoringService> logger)
    {
        _logger = logger;
    }

    public GoalScoringProfile Analyse(
        List<TeamForm> form)
    {
        var profile =
            new GoalScoringProfile();

        if (form.Count == 0)
            return profile;

        profile.MatchesAnalysed =
            form.Count;

        profile.GoalsForAverage =
            form.Average(x => x.GoalsFor);

        profile.GoalsAgainstAverage =
            form.Average(x => x.GoalsAgainst);

        profile.XGAverage =
            form.Average(x => x.XG);

        profile.XGAgainstAverage =
            form.Average(x => x.OpponentXG);

        profile.BTTSRate =
            form.Count(x => x.BTTS)
            / (decimal)form.Count;

        profile.ScoresOften =
            form.Count(x => x.GoalsFor > 0)
            >= 4;

        profile.ConcedesOften =
            form.Count(x => x.GoalsAgainst > 0)
            >= 3;

        profile.Over25Rate =
            form.Count(x => x.Over25)
            / (decimal)form.Count;

        profile.AttackStrength =
            (decimal)profile.GoalsForAverage;

        profile.DefenceWeakness =
            (decimal)profile.GoalsAgainstAverage;

        profile.GoalPotential =
            profile.AttackStrength +
            profile.DefenceWeakness;

        _logger.LogInformation(
            """
            Goal Profile

            Matches={matches}
            GoalsFor={gf:F2}
            GoalsAgainst={ga:F2}
            xG={xg:F2}
            xGA={xga:F2}
            BTTS={btts:P}
            ScoresOften={scores}
            ConcedesOften={concedes}
            Over25={over25:P}
            Attack={attack:F2}
            DefenceWeakness={defence:F2}
            GoalPotential={potential:F2}
            """,
            profile.MatchesAnalysed,
            profile.GoalsForAverage,
            profile.GoalsAgainstAverage,
            profile.XGAverage,
            profile.XGAgainstAverage,
            profile.BTTSRate,
            profile.ScoresOften,
            profile.ConcedesOften,
            profile.Over25Rate,
            profile.AttackStrength,
            profile.DefenceWeakness,
            profile.GoalPotential
            );

        return profile;
    }
}
