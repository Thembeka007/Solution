using Analytics.Models;
using Analytics.Portfolio;
using Analytics.SmartMoney;
using Analytics.Strategies;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;
using SharedKernel.Enums;
using System.Text.RegularExpressions;

namespace Analytics.Services;

public class PredictionSignalService
{

    private readonly ILogger<PredictionSignalService>
        _logger;

    private readonly SportsBettingDbContext
        _db;

    private readonly IEnumerable<
        IPredictionStrategy>
            _strategies;

    private readonly
        ConfidenceCalibrationService
            _calibration;

    private readonly PortfolioService
        _portfolio;

    public PredictionSignalService(
        SportsBettingDbContext db,
        IEnumerable<IPredictionStrategy> strategies,
        ConfidenceCalibrationService calibration,
        ILogger<PredictionSignalService> logger,
        PortfolioService portfolio)
    {
        _db = db;

        _strategies =
            strategies;

        _calibration =
            calibration;

        _logger =
            logger;

        _portfolio =
            portfolio;
    }

    public async Task GenerateAsync(
        DateTime date)
    {
        _logger.LogInformation(
            "GenerateAsync called with date {date}",
            date);

        _logger.LogInformation(
            "Date component = {dateOnly}",
            date.Date);

        _logger.LogInformation(
            "===== Prediction generation started for {date} =====",
            date);

        var start = DateTime.UtcNow;
        var end = start.AddDays(3);

        var matches =
            await _db.Matches
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .Where(x =>
                    x.Status == MatchStatus.Scheduled &&
                    x.KickoffUtc >= start &&
                    x.KickoffUtc < end)                    
                .ToListAsync();

        _logger.LogInformation(
            "Matches found for date {date}: {count}",
            date.Date,
            matches.Count);

        _logger.LogInformation(
            "GenerateAsync date = {date}",
            date);


        foreach (var match in matches.Take(10))
        {
            _logger.LogInformation(
                "Loaded match {id} kickoff={kickoff}",
                match.Id,
                match.KickoffUtc);

            _logger.LogInformation(
                "Processing {home} vs {away}",
                match.HomeTeam?.Name,
                match.AwayTeam?.Name);

            // =====================================
            // PREVENT DUPLICATES
            // =====================================

            _logger.LogInformation(
                "GenerateAsync called for match {id}",
                match.Id);

            _logger.LogInformation(
                "Processing Match: {home} vs {away}  | Kickoff={kickoff}",
                match.HomeTeam!.Name,
                match.AwayTeam!.Name,
                match.KickoffUtc);

            // =====================================
            // EXECUTE STRATEGIES
            // =====================================

            var portfolioCandidates =
                new List<Prediction>();

            foreach (var strategy in _strategies)
            {
                bool exists =
                    await _db.Predictions
                    .AnyAsync(x =>
                        x.MatchId == match.Id &&
                        x.StrategyVersion == strategy.Name);

                if (exists)
                {
                    _logger.LogInformation(
                        "Skipping match {matchId} for strategy {strategy}",
                        match.Id,
                        strategy.Name);

                    continue;
                }
                    

                _logger.LogInformation(
                    "Executing strategy {name} ({type})",
                    strategy.Name,
                    strategy.GetType().Name);

                _logger.LogInformation(
                    "Strategies loaded: {count}",
                    _strategies.Count());

                _logger.LogInformation(
                    "Current UTC = {utc}",
                    DateTime.UtcNow);

                _logger.LogInformation(
                    "Requested date = {date}",
                    date);

                _logger.LogInformation(
                    "Requested date.Date = {dateOnly}",
                    date.Date);

                _logger.LogInformation(
                    "BEFORE strategy {name}",
                    strategy.Name);
                
                var predictions =
                        await strategy
                            .GeneratePredictionsAsync(
                            match);

                _logger.LogInformation(
                    "{name} returned {count} predictions",
                    strategy.Name,
                    predictions.Count);

                _logger.LogInformation(
                    "AFTER strategy {name}",
                    strategy.Name);

                _logger.LogInformation(
                    "Running strategy {name} returned {count} predictions",
                    strategy.Name,
                    predictions.Count);

                foreach (var prediction
                    in predictions)
                {
                    // =============================
                    // APPLY CALIBRATION
                    // =============================

                    _logger.LogInformation(
                        "BEFORE CALIBRATION: {confidence}",
                        prediction.Confidence);

                    prediction.Confidence =
                        await _calibration
                            .ApplyCalibrationAsync(
                                prediction
                                    .Confidence);

                    _logger.LogInformation(
                        "AFTER CALIBRATION: {confidence}",
                        prediction.Confidence);

                    _logger.LogInformation(
                        "BEFORE SAVE Match={match} Confidence={confidence}",
                        prediction.MatchId,
                        prediction.Confidence);

                }

                // =============================
                // SAVE
                // =============================

                portfolioCandidates
                    .AddRange(predictions);


                _logger.LogInformation(
                    "{strategy} produced {count} predictions",
                    strategy.Name,
                    predictions.Count);
            }

            _logger.LogInformation(
                "Portfolio received {count} candidates",
                portfolioCandidates.Count);

            var portfolio =
                _portfolio.BuildPortfolio(
                    portfolioCandidates);

            _logger.LogInformation(
                "Portfolio selected {count} predictions",
                portfolio.Count);

            foreach (var prediction in portfolio)
            {
                _db.Predictions.Add(prediction);
            }

            _logger.LogInformation(
           "About to save predictions");

            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Prediction generation completed");
        }
    }
}
