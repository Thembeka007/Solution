namespace Analytics.Models;

public class GoalScoringProfile
{
    public double GoalsForAverage { get; set; }

    public double GoalsAgainstAverage { get; set; }

    public double XGAverage { get; set; }

    public double XGAgainstAverage { get; set; }

    public decimal BTTSRate { get; set; }

    public bool ScoresOften { get; set; }

    public bool ConcedesOften { get; set; }

    public int MatchesAnalysed { get; set; }

    // Extend to include prop for attacking strength

    public decimal Over25Rate { get; set; }

    public decimal AttackStrength { get; set; }

    public decimal DefenceWeakness { get; set; }

    public decimal GoalPotential { get; set; }
}
