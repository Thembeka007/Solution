using Infrastructure.Data;
using Ingestion.Services.PredictionEngine.Abstractions;
using Ingestion.Services.PredictionEngine.DTOs;
using Ingestion.Services.SmartMoney.Abstractions;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace Ingestion.Services.PredictionEngine.Processing;

public class PredictionEngine : IPredictionEngine
{
    private readonly SportsBettingDbContext _db;
    private readonly ISmartMoneyService _smartMoneyService;

    public PredictionEngine(
        SportsBettingDbContext db, ISmartMoneyService smartMoneyService)
    {
        _db = db;
        _smartMoneyService = smartMoneyService;
    }

    public async Task<List<MatchPredictionDto>>
        GeneratePredictionsAsync(DateTime date)
    {
        var results =
            new List<MatchPredictionDto>();

        var matches =
            await _db.Matches
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
                .Where(x =>
                    x.KickoffUtc.Date == date.Date)
                .ToListAsync();

        var smartMoneySignals =
            await _smartMoneyService
            .GetConsensusPredictionsAsync(date);

        foreach (var match in matches)
        {
            var homeForm =
                await GetRecentForm(match.HomeTeamId);

            var awayForm =
                await GetRecentForm(match.AwayTeamId);

            // ===== BTTS =====

            var homeBtts =
                homeForm.Take(5)
                    .Count(x => x.BTTS);

            var awayBtts =
                awayForm.Take(5)
                    .Count(x => x.BTTS);

            if (homeBtts >= 3 &&
                awayBtts >= 2)
            {
                var smartMoneyBoost =
                    smartMoneySignals
                    .FirstOrDefault(x =>
                    x.MatchId == match.Id &&
                    x.PredictionType == "BTTS" &&
                    x.Outcome == "YES");

                decimal confidence = 82;

                if (smartMoneyBoost is not null)
                {
                    confidence +=
                        smartMoneyBoost
                            .ConfidenceBoost;
                }

                confidence =
                    Math.Min(confidence, 99);

                results.Add(
                    new MatchPredictionDto
                    {
                        MatchId = match.Id,

                        HomeTeam = match.HomeTeam!.Name,

                        AwayTeam = match.AwayTeam!.Name,

                        KickoffUtc = match.KickoffUtc,

                        PredictionType = "BTTS",

                        PredictedOutcome = "YES",

                        Confidence = confidence,

                        StrategyVersion = "v2-smart-money"
                    });

            }

            // ===== OVER 2.5 =====

            var homeOver =
                homeForm.Take(5)
                    .Count(x => x.Over25);

            var awayOver =
                awayForm.Take(5)
                    .Count(x => x.Over25);

            if (homeOver >= 3 &&
                awayOver >= 2)
            {
                results.Add(
                    new MatchPredictionDto
                    {
                        MatchId = match.Id,

                        PredictionType = "OVER25",

                        PredictedOutcome = "OVER_2_5",

                        Confidence = 80,

                        StrategyVersion = "v1"
                    });
            }

            // ===== MATCH OUTCOME =====

            var homePoints =
                homeForm.Take(3)
                    .Sum(x => x.Points);

            var awayPoints =
                awayForm.Take(3)
                    .Sum(x => x.Points);

            if (homePoints > awayPoints)
            {
                results.Add(
                    new MatchPredictionDto
                    {
                        MatchId = match.Id,

                        PredictionType = "MATCH_RESULT",

                        PredictedOutcome = "HOME_WIN",

                        Confidence = 76,

                        StrategyVersion = "v1"
                    });
            }
            else if (awayPoints > homePoints)
            {
                results.Add(
                    new MatchPredictionDto
                    {
                        MatchId = match.Id,

                        PredictionType = "MATCH_RESULT",

                        PredictedOutcome = "AWAY_WIN",

                        Confidence = 74,

                        StrategyVersion = "v1"
                    });
            }
        }

        return results
            .OrderByDescending(x => x.Confidence)
            .ToList();
    }

    private async Task<List<TeamForm>>
        GetRecentForm(Guid teamId)
    {
        return await _db.TeamForms
            .Where(x =>
                x.TeamName == teamId.ToString())
            .OrderByDescending(x =>
                x.MatchDateUtc)
            .Take(10)
            .ToListAsync();
    }
}
