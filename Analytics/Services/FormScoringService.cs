using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace Analytics.Services;

public class FormScoringService
{
    private readonly ILogger<
        FormScoringService> _logger;

    public FormScoringService(ILogger< FormScoringService> logger)
    {
        _logger = logger;
    }

    public decimal CalculateScore(
        List<TeamForm> forms)
    {
        decimal score = 0;

        if (!forms.Any())
            return 0;

        var overall =
            forms.Take(10).ToList();

        var trueForm =
            forms.Take(3).ToList();

        decimal overallScore =
            overall.Sum(x => x.Points);

        decimal trueFormScore =
            trueForm.Sum(x => x.Points);

        decimal goalsFor =
            overall.Sum(x => x.GoalsFor);

        decimal goalsAgainst =
            overall.Sum(x => x.GoalsAgainst);

        decimal xg =
            (decimal)overall.Sum(x => x.XG);

        decimal opponentXg =
            (decimal)overall.Sum(x =>
                x.OpponentXG);        

        // overall form
        score += overallScore * 0.25m;

        // true form
        score += trueFormScore * 0.50m;

        // attack
        score += goalsFor * 0.10m;

        // defense
        score -= goalsAgainst * 0.05m;

        // expected goals
        score += xg * 0.10m;

        // expected goals conceded
        score -= opponentXg * 0.05m;



        _logger.LogInformation(
            "FORM SCORE RAW={raw}",
            score);

        var finalScore =
            Math.Max(0m, score);

        _logger.LogInformation(
            "FORM SCORE FINAL={final}",
            finalScore);

        return finalScore;
    }
}
