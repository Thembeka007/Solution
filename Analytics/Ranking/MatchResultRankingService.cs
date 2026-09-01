using Analytics.Models;
using Analytics.Services;

namespace Analytics.Ranking;

public class MatchResultRankingService
{
    public MatchResultProfile Build(
        GoalScoringProfile home,
        GoalScoringProfile away,
        HeadToHeadStats h2h)
    {
        var profile =
            new MatchResultProfile();

        //------------------------------------
        // Attack
        //------------------------------------

        profile.HomeAttack =
            (decimal)home.GoalsForAverage;

        profile.AwayAttack =
            (decimal)away.GoalsForAverage;

        //------------------------------------
        // Defence
        //------------------------------------

        profile.HomeDefence =
            (decimal)away.GoalsAgainstAverage;

        profile.AwayDefence =
            (decimal)home.GoalsAgainstAverage;

        //------------------------------------
        // Recent Form
        //------------------------------------

        profile.HomeFormScore =
            home.BTTSRate * 10m;

        profile.AwayFormScore =
            away.BTTSRate * 10m;

        //------------------------------------
        // H2H
        //------------------------------------

        if (h2h.Matches > 0)
        {
            profile.H2HScore =
                (decimal)h2h.HomeWins /
                h2h.Matches;
        }

        //------------------------------------
        // Home Strength
        //------------------------------------

        profile.HomeStrength =
            profile.HomeAttack +
            profile.HomeDefence +
            profile.HomeFormScore +
            profile.H2HScore;

        //------------------------------------
        // Away Strength
        //------------------------------------

        profile.AwayStrength =
            profile.AwayAttack +
            profile.AwayDefence +
            profile.AwayFormScore;

        profile.FinalScore =
            profile.HomeStrength -
            profile.AwayStrength;

        return profile;
    }
}
