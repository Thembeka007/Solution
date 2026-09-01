using SharedKernel.Entities;

namespace Betting.Selection;

public class OddsResolver
{
    public decimal ResolveOdds(
        Prediction prediction,
        MatchOdds odds)
    {
        if (prediction.PredictionType ==
            "MATCH_RESULT")
        {
            return prediction.PredictedOutcome switch
            {
                "HOME_WIN" => odds.HomeOdds,
                "DRAW" => odds.DrawOdds,
                "AWAY_WIN" => odds.AwayOdds,
                _ => 0
            };
        }

        if (prediction.PredictionType ==
            "BTTS")
        {
            return prediction.PredictedOutcome switch
            {
                "YES" => odds.BTTSYesOdds,
                "NO" => odds.BTTSNoOdds,
                _ => 0
            };
        }

        if (prediction.PredictionType ==
            "OVER25")
        {
            return prediction.PredictedOutcome switch
            {
                "OVER_2_5" => odds.Over25Odds,
                "UNDER_2_5" => odds.Under25Odds,
                _ => 0
            };
        }

        return 0;
    }
}
