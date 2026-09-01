using Betting.Allocation;
using Betting.Analysis;
using Betting.Configuration;
using Betting.DTOs;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Entities;

namespace Betting.Selection;

public class DailyPickSelector
{
    private readonly SportsBettingDbContext
        _db;

    private readonly PickRankingService
        _ranking;

    private readonly ExpectedValueCalculator
        _expectedValueCalculator;

    private readonly OddsResolver
        _oddsResolver;

    private readonly KellyCriterionCalculator
        _kelly;

    private readonly ILogger<DailyPickSelector>
        _logger;

    private readonly PickCategoryService
        _categoryService;

    private readonly StakeAllocationService
        _allocationService;

    private readonly BankrollOptions
        _bankrollOptions;

    public DailyPickSelector(
        SportsBettingDbContext db,
        PickRankingService ranking,
        ExpectedValueCalculator expectedValueCalculator,
        OddsResolver oddsResolver,
        KellyCriterionCalculator kelly,
         ILogger<DailyPickSelector> logger,
         PickCategoryService categoryService,
         StakeAllocationService allocationService,
         IOptions<BankrollOptions> bankrollOptions)
    {
        _db = db;

        _ranking = ranking;

        _expectedValueCalculator =
            expectedValueCalculator;

        _oddsResolver =
            oddsResolver;

        _kelly = kelly;

        _logger = logger;

        _categoryService =
            categoryService;

        _allocationService =
            allocationService;

        _bankrollOptions =
            bankrollOptions.Value;
    }

    public async Task<List<DailyPickDto>>
        GetTopPicksAsync(
            DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(7);

        var predictions =
            await _db.Predictions
                .Include(x => x.Match!)
                    .ThenInclude(x => x.HomeTeam)
                .Include(x => x.Match!)
                    .ThenInclude(x => x.AwayTeam)
                .Where(x =>
                    !x.IsSettled &&
                    x.Match != null &&
                    x.Match.KickoffUtc >= start &&
                    x.Match.KickoffUtc < end)
                .ToListAsync();

        foreach (var p in predictions)
        {
            _logger.LogInformation(
                "SELECTOR LOADED | Match={match} Confidence={confidence} Outcome={outcome} Strategy={strategy}",
                p.MatchId,
                p.Confidence,
                p.PredictedOutcome,
                p.StrategyVersion);
        }

        _logger.LogInformation(
            "Prediction window: {start} -> {end}",
            start, end);

        var allOdds =
            await _db.MatchOdds
                .ToListAsync();

        _logger.LogInformation(
            "Odds records found: {0}",
            allOdds.Count);

        var picks =
            new List<DailyPickDto>();

        _logger.LogInformation(
            "Predictions loaded: {0}",
            predictions.Count);

        _logger.LogInformation(
            "Odds rows loaded: {0}",
            allOdds.Count);

        foreach (var prediction in predictions)
        {
            _logger.LogInformation(
                "SELECTOR STARTED WITH {count} PREDICTIONS",
                predictions.Count);

            _logger.LogInformation(
                "LOADED Prediction {id} Match={match} Kickoff={kickoff}",
                prediction.Id,
                prediction.MatchId,
                prediction.Match?.KickoffUtc);

            var latestOdds =
                allOdds
                    .Where(x =>
                        x.MatchId ==
                        prediction.MatchId)
                    .OrderByDescending(x =>
                        x.RetrievedUtc)
                    .FirstOrDefault();

            if (latestOdds is null)
            {
                _logger.LogWarning(
                    "EXIT PATH A - NO ODDS");

                _logger.LogInformation(
                    "Rejected {0}: no odds, no odds found for MatchId={matchId}",
                    prediction.Id,
                    prediction.MatchId);

                continue;
            }

            decimal bookmakerOdds =
                _oddsResolver
                    .ResolveOdds(
                        prediction,
                        latestOdds);

            _logger.LogInformation(
                "ODDS RESOLVER | Match={match} Outcome={outcome} HomeOdds={home} DrawOdds={draw} AwayOdds={away} Resolved={resolved}",
                prediction.MatchId,
                prediction.PredictedOutcome,
                latestOdds.HomeOdds,
                latestOdds.DrawOdds,
                latestOdds.AwayOdds,
                bookmakerOdds);

            if (bookmakerOdds <= 1)
            {
                _logger.LogWarning(
                    "EXIT PATH B - BOOKMAKERODDS");

                _logger.LogInformation(
                    "Rejected {0}: odds={1}",
                    prediction.Id,
                    bookmakerOdds);
                continue;
            }

            // ========================================
            // LONGSHOT FILTER
            // ========================================

            if (bookmakerOdds > 5m &&
                prediction.Confidence < 75m)
            {
                _logger.LogWarning(
                    "EXIT PATH C - LONGSHOT");

                _logger.LogInformation(
                    "Rejected {id}: Longshot Odds={odds} Confidence={confidence}",
                    prediction.Id,
                    bookmakerOdds,
                    prediction.Confidence);

                continue;
            }

            _logger.LogInformation(
                "SELECTOR DATE={date}",
                date.Date);

            if (prediction.Confidence < 60m &&
                bookmakerOdds < 2m)
            {
                _logger.LogWarning(
                    "EXIT PATH D - CONFIDENCE");

                _logger.LogInformation(
                    "Rejected {id}: Confidence={confidence}",
                    prediction.Id,
                    prediction.Confidence);

                continue;
            }


            decimal expectedValue =
                _expectedValueCalculator
                    .Calculate(
                        prediction.Confidence,
                        bookmakerOdds);

            _logger.LogInformation(
                "EV CHECK | Match={match} EV={ev}",
                prediction.MatchId,
                expectedValue);

            if (expectedValue <= 0)
            {
                _logger.LogWarning(
                    "EXIT PATH E - EV");

                _logger.LogInformation(
                    "Rejected {id}: EV={ev}",
                    prediction.Id,
                    expectedValue);

                continue;
            }

            decimal marketProbability =
                (1m /
                 bookmakerOdds) * 100m;

            decimal edge =
                prediction.Confidence -
                marketProbability;


            _logger.LogInformation(
                "MARKET EDGE | Match={match} Confidence={confidence} Market={market} Edge={edge}",
                prediction.MatchId,
                prediction.Confidence,
                marketProbability,
                edge);

            if (edge < 1.0m)
            {
                _logger.LogWarning(
                    "EXIT PATH F - EDGE");

                _logger.LogInformation(
                    "Rejected {id}: Edge too small {edge}",
                    prediction.Id,
                    edge);

                continue;
            }

            _logger.LogInformation(
                "MODEL={model}% MARKET={market}% EDGE={edge}",
                prediction.Confidence,
                marketProbability,
                edge);

            decimal fairOdds =
                prediction.Confidence > 0
                    ? 100m /
                      prediction.Confidence
                    : 0;

            decimal kelly =
                _kelly.Calculate(
                    prediction.Confidence,
                    bookmakerOdds);

            _logger.LogInformation(
                "KELLY CHECK | Match={match} Kelly={kelly}",
                prediction.MatchId,
                kelly);

            if (kelly < 0.01m)
            {
                _logger.LogWarning(
                    "EXIT PATH J - KELLY");

                _logger.LogInformation(
                    "Rejected {0}: Kelly={1}",
                    prediction.Id,
                    kelly);

                continue;
            }

            decimal score =
                _ranking.CalculateScore(
                    prediction,
                    expectedValue,
                    edge,
                    kelly);

            _logger.LogInformation(
                "SCORE BREAKDOWN | Match={match} Confidence={confidence} EV={ev} Edge={edge} Kelly={kelly} FinalScore={score}",
                prediction.MatchId,
                prediction.Confidence,
                expectedValue,
                edge,
                kelly,
                score);

            decimal recommendedStake =
                _bankrollOptions.TotalBankroll * kelly;

            _logger.LogWarning(
                "EXIT PATH K - PICK ACCEPTED");

            _logger.LogInformation(
                "PICK ACCEPTED | Match={match}",
                prediction.MatchId);

            picks.Add(
                new DailyPickDto
                {
                    PredictionId =
                        prediction.Id,

                    MatchId =
                        prediction.MatchId,

                    HomeTeam =
                        prediction.Match!
                            .HomeTeam?.Name ?? "",

                    AwayTeam =
                        prediction.Match!
                            .AwayTeam?.Name ?? "",

                    KickoffUtc =
                        prediction.Match!
                            .KickoffUtc,

                    PredictionType =
                        prediction
                            .PredictionType,

                    PredictedOutcome =
                        prediction
                            .PredictedOutcome,

                    Confidence =
                        prediction
                            .Confidence,

                    Odds =
                        bookmakerOdds,

                    ExpectedValue =
                        expectedValue,

                    MarketProbability =
                        marketProbability,

                    EdgePercentage =
                        edge,

                    FairOdds =
                        fairOdds,

                    StrategyVersion =
                        prediction
                            .StrategyVersion,

                    KellyPercentage =
                        kelly * 100m,

                    RecommendedStake =
                        recommendedStake,

                    Score =
                        score,
                });
        }

        foreach(var pick in picks)
        {
            _logger.LogInformation(
                "PRE ALLOCATION Match={match} Category={category}",
                pick.MatchId,
                pick.Category);

            pick.Category =
                _categoryService
                    .Determine(pick);

            _logger.LogInformation(
                "FINAL PICK Match={match} Category={category} Score={score} Confidence={confidence} Stake={stake}",
                pick.MatchId,
                pick.Category,
                pick.Score,
                pick.Confidence,
                pick.Stake);
        }



        _allocationService
            .Allocate(picks, 5m);

        _logger.LogInformation(
            "Returning picks: {count}",
            picks.Count);

        return picks
            .OrderByDescending(x =>
                x.Score)
            .ThenByDescending(x =>
                x.ExpectedValue)
            .ThenByDescending(x =>
                x.EdgePercentage)
            .ThenByDescending(x =>
                x.Confidence)
            .Take(10)
            .ToList();
    }

}
