using AI.Evolution.Laboratory.Models;
using Analytics.Models;
using Analytics.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Entities;

namespace AI.Evolution.Laboratory.Services;

public class LaboratoryCandidateRunner
{
    private readonly SportsBettingDbContext _db;
    
    private readonly HistoricalCandidatePredictionService
        _candidatePredictionService;

    private readonly LaboratoryCandidateDatasetEvaluator
        _datasetEvaluator;

    private readonly LaboratoryCandidateDatasetSplitter
        _datasetSplitter;

    private readonly LaboratoryCandidateGateService
        _candidateGateService;

    private readonly ILogger<LaboratoryCandidateRunner>
        _logger;

    public LaboratoryCandidateRunner(
        SportsBettingDbContext db,
        HistoricalCandidatePredictionService candidatePredictionService,
        LaboratoryCandidateDatasetEvaluator datasetEvaluator,
        LaboratoryCandidateDatasetSplitter datasetSplitter,
        LaboratoryCandidateGateService candidateGateService,
        ILogger<LaboratoryCandidateRunner> logger)
    {
        _db = db;
        _candidatePredictionService =
            candidatePredictionService;
        _datasetEvaluator = datasetEvaluator;
        _datasetSplitter = datasetSplitter;
        _candidateGateService = candidateGateService;
        _logger = logger;
    }

    public async Task<LaboratoryCandidateRunResult>
        RunAsync(
            string strategyVersion,
            int matchLimit = 50)
    {

        // =========================================
        // VALIDATE INPUT
        // =========================================

        if (string.IsNullOrWhiteSpace(strategyVersion))
        {
            throw new ArgumentException(
                "Strategy version is required.",
                nameof(strategyVersion));
        
        }

        if (matchLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(matchLimit));
        }

        int insufficientHistoricalForm = 0;
        
        int modelRejected = 0;
        
        int predictionsGenerated = 0;
        
        int marketDataUnavailable = 0;

        // =========================================
        // VERIFY STRATEGY EXISTS
        // =========================================

        var strategy =
            await _db.StrategyPerformances
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.StrategyVersion == strategyVersion);

        if (strategy == null)
        {
            throw new InvalidOperationException(
                $"Strategy '{strategyVersion}' was not found.");
        
        }

        _logger.LogInformation(
            """
            ========================================
            
            LABORATORY CANDIDATE RUN
            
            Strategy={strategy}
            
            Family={family}
            
            Generation={generation}
            
            Active={active}
            
            Mature={mature}
            
            LaboratoryPassed={passed}
            
            MatchLimit={limit}

            ======================================== 
            """,
            strategy.StrategyVersion,
            strategy.StrategyFamily,
            strategy.Generation,
            strategy.IsActive,
            strategy.IsMature,
            strategy.LaboratoryPassed,
            matchLimit);

        // =========================================
        // LOAD HISTORICAL SETTLED MATCHES
        // =========================================
        //
        // We deliberately use settled matches only.
        //
        // No future/live match can enter the
        // historical experiment.
        //
        // =========================================

        var matches =
            await _db.Matches
                .AsNoTracking()
                .Where(x =>
                    x.HomeScore != null &&
                    x.AwayScore != null &&
                    
                    _db.MatchOdds.Any(o =>
                        o.MatchId == x.Id &&

                        o.RetrievedUtc <=
                            x.KickoffUtc &&

                        (
                            o.HomeOdds > 1m ||
                            o.DrawOdds > 1m ||
                            o.AwayOdds > 1m ||
                            o.Over25Odds > 1m ||
                            o.Under25Odds > 1m
                        )))
                .OrderByDescending(x =>
                    x.KickoffUtc)
                .Take(matchLimit)
                .ToListAsync();

        _logger.LogInformation(
            """
            Historical matches loaded
            
            Strategy={strategy}
            
            Matches={matches} 
            """,
            strategyVersion,
            matches.Count);

        // =========================================
        // RUN CANDIDATE
        // =========================================

        var results =
            new List<HistoricalCandidatePredictionResult>();

        int matchesSkipped = 0;

        var skippedMatches = 
            new List<Match>();


        foreach (var match in matches)
        {
            var candidatePrediction =
                await _candidatePredictionService
                    .EvaluateAsync(
                        strategyVersion,
                        match);

            if (candidatePrediction == null)
            {
                insufficientHistoricalForm++;

                matchesSkipped++;

                skippedMatches.Add(match);

                _logger.LogInformation(
                    """
                    Candidate match skipped
                    
                    Match={match}
                    
                    HomeTeam={homeTeam}
                    
                    AwayTeam={awayTeam}
                    
                    Reason=Historical form requirement not met 
                    """,
                    match.Id,
                    match.HomeTeamId,
                    match.AwayTeamId);

                continue;
            }

            if (!candidatePrediction.ShouldGeneratePrediction)
            {
                modelRejected++;
                
                results.Add(candidatePrediction);
                
                continue;
            }

            predictionsGenerated++;

            if (!candidatePrediction.MarketDataAvailable)
            {
                marketDataUnavailable++;
            
            }

            // =========================================
            // SKIPPED MATCH CHRONOLOGY
            // =========================================


            var skippedByPeriod =
                skippedMatches.GroupBy(x =>
                    new
                    {
                        Year = x.KickoffUtc.Year,
                        Month = x.KickoffUtc.Month
                    })
                .OrderBy(x =>
                    x.Key.Year)
                .ThenBy(x =>
                    x.Key.Month)
                .ToList();

            _logger.LogInformation(
                "========================================");
            
            _logger.LogInformation(
                "SKIPPED MATCH CHRONOLOGY");


            foreach (var period in skippedByPeriod)
            {
                _logger.LogInformation(
                    """
                    Period={year}-{month:D2}
                    
                    SkippedMatches={count}
                    
                    FirstSkipped={first}
                    
                    LastSkipped={last} 
                    """,
                    period.Key.Year,
                    period.Key.Month,
                    period.Count(),
                    period.Min(x =>
                        x.KickoffUtc),
                    period.Max(x =>
                        x.KickoffUtc));
            }

            _logger.LogInformation(
                "========================================");

            results.Add(candidatePrediction);


            _logger.LogInformation(
                """
                Candidate historical result
                
                Strategy={strategy}
                
                Match={match}
                
                Kickoff={kickoff}
                
                Generated={generated}
                
                Outcome={outcome}
                
                Confidence={confidence:F2}
                
                MarketData={marketData}
                
                Correct={correct}
                
                ProfitLoss={profit:F2}
                
                ROI={roi:F2} 
                """,
                strategyVersion,
                match.Id,
                match.KickoffUtc,
                candidatePrediction.ShouldGeneratePrediction,
                candidatePrediction.PredictedOutcome,
                candidatePrediction.Confidence,
                candidatePrediction.MarketDataStatus,
                candidatePrediction.PredictionCorrect,
                candidatePrediction.RealisedProfitLoss,
                candidatePrediction.RealisedROI);

        }

        _logger.LogInformation(
            """
            ========================================
            
            CANDIDATE OUTCOME BREAKDOWN
            
            MatchesExamined={examined}
            
            InsufficientHistoricalForm={form}
            
            ModelRejected={rejected}
            
            PredictionsGenerated={generated}
            
            MarketDataUnavailable={marketUnavailable}
            
            ======================================== 
            """,
            matches.Count,
            insufficientHistoricalForm,
            modelRejected,
            predictionsGenerated,
            marketDataUnavailable);


        // =========================================
        // CHRONOLOGICAL CANDIDATE COVERAGE
        // =========================================

        var candidateCoverage =
            results
                .GroupBy(x =>
                    new
                    {
                        Year = x.KickoffUtc.Year,
                        Month = x.KickoffUtc.Month
                    })
                .OrderBy(x =>
                    x.Key.Year)
                .ThenBy(x =>
                    x.Key.Month)
                .Select(x =>
                    new
                    {
                        x.Key.Year,
                        x.Key.Month,
                        Predictions = x.Count(),
                        FirstMatch = x.Min(y =>
                            y.KickoffUtc),
                        LastMatch = x.Max(y =>
                            y.KickoffUtc)
                    })
                .ToList();

        _logger.LogInformation(
            "========================================");
        
        
        _logger.LogInformation(
            "CANDIDATE CHRONOLOGICAL COVERAGE");


        foreach (var period in candidateCoverage)
        {
            _logger.LogInformation(
                """
                Period={year}-{month:D2}
                
                PredictionsGenerated={predictions}
                
                FirstMatch={first}
                
                LastMatch={last} 
                """,
                period.Year,
                period.Month,
                period.Predictions,
                period.FirstMatch,
                period.LastMatch);
        }


        _logger.LogInformation(
            "========================================");



        // =========================================
        // TEAM FORM COVERAGE DIAGNOSTIC
        // =========================================


        var examinedTeamIds =
            matches
                .SelectMany(x =>
                    new[]
                    {
                        x.HomeTeamId,
                        x.AwayTeamId
                    })
                .Distinct()
                .ToList();

        var teamFormCoverage =
            await _db.TeamForms
                .AsNoTracking()
                .Where(x =>
                    examinedTeamIds
                        .Contains(x.TeamId))
                .GroupBy(x =>
                    x.TeamId)
                .Select(group =>
                    new
                    {
                        TeamId = group.Key,
                        FormCount = group.Count()
                    })
                .ToListAsync();

        _logger.LogInformation(
            """
            ========================================
            
            TEAM FORM COVERAGE DIAGNOSTIC
            
            ExaminedMatches={matches}
            
            ExaminedTeams={teams}
            
            TeamsWithTeamForm={teamsWithForm}
            
            TeamsWithoutTeamForm={teamsWithoutForm}
            
            ======================================== 
            """,
            matches.Count,
            examinedTeamIds.Count,
            teamFormCoverage.Count,
            examinedTeamIds.Count - teamFormCoverage.Count);


        // =========================================
        // SPLIT LABORATORY DATASET
        // =========================================

        var (inSample,
            validation,
            outOfSample) =
                _datasetSplitter
                    .Split(results);

        // =========================================
        // EVALUATE DATASET PARTITIONS
        // =========================================

        var inSampleResult =
            _datasetEvaluator
                .Evaluate(inSample);

        var validationResult =
            _datasetEvaluator
                .Evaluate(validation);

        var outOfSampleResult =
            _datasetEvaluator
                .Evaluate(outOfSample);

        // ======================================
        // CANDIDATE GATEWAY
        // ======================================

        var gateResult =
            _candidateGateService.Evaluate(
                inSampleResult,
                validationResult,
                outOfSampleResult);


        // =========================================
        // LOG PARTITION RESULTS
        // =========================================

        _logger.LogInformation(
            """
            ========================================
            
            LABORATORY PARTITION RESULTS
            
            InSample:
            
            Predictions={inPredictions}
            
            Accuracy={inAccuracy:F2}
            
            Profit={inProfit:F2}
            
            ROI={inROI:F2}
            
            MaximumDrawdown={inDrawdown:F2}
            
            MaximumDrawdownPercentage={inDrawdownPercent:F2}
            
            RiskPassed={inRisk}
            
            Validation:
            
            Predictions={validationPredictions}
            
            Accuracy={validationAccuracy:F2}
            
            Profit={validationProfit:F2}
            
            ROI={validationROI:F2}
            
            MaximumDrawdown={validationDrawdown:F2}
            
            MaximumDrawdownPercentage={validationDrawdownPercent:F2}
            
            RiskPassed={validationRisk}
            
            OutOfSample:
            
            Predictions={outPredictions}
            
            Accuracy={outAccuracy:F2}
            
            Profit={outProfit:F2}
            
            ROI={outROI:F2}
            
            MaximumDrawdown={outDrawdown:F2}
            
            MaximumDrawdownPercentage={outDrawdownPercent:F2}
            
            RiskPassed={outRisk}
            
            ======================================== 
            """,
            inSampleResult.Predictions,
            inSampleResult.Accuracy,
            inSampleResult.TotalProfit,
            inSampleResult.ROI,
            inSampleResult.MaximumDrawdown,
            inSampleResult.MaximumDrawdownPercentage,
            inSampleResult.RiskPassed,
            validationResult.Predictions,
            validationResult.Accuracy,
            validationResult.TotalProfit,
            validationResult.ROI,
            validationResult.MaximumDrawdown,
            validationResult.MaximumDrawdownPercentage,
            validationResult.RiskPassed,
            outOfSampleResult.Predictions,
            outOfSampleResult.Accuracy,
            outOfSampleResult.TotalProfit,
            outOfSampleResult.ROI,
            outOfSampleResult.MaximumDrawdown,
            outOfSampleResult.MaximumDrawdownPercentage,
            outOfSampleResult.RiskPassed);


        _logger.LogInformation(
            """
            ========================================
    
            LABORATORY DATASET SPLIT
    
            Strategy={strategy}
    
            TotalUsablePredictions={total}
    
            InSample={inSample}
    
            Validation={validation}
    
            OutOfSample={outOfSample}
    
            ========================================
            """,
            strategyVersion,
            results.Count(x =>
                x.ShouldGeneratePrediction &&
                x.MarketDataAvailable),
            inSample.Count,
            validation.Count,
            outOfSample.Count);


        // =========================================
        // LAB CANDIDATE DATASET CALCULATOR
        // =========================================

        var datasetResult =
            _datasetEvaluator.Evaluate(
                results);


        // =========================================
        // BUILD RESULT
        // =========================================

        var runResult =
            new LaboratoryCandidateRunResult
            {
                StrategyVersion =
                    strategyVersion,

                StrategyFamily =
                    strategy.StrategyFamily,

                Generation =
                    strategy.Generation,

                MatchesExamined =
                    matches.Count,

                MatchesSkipped =
                    matchesSkipped,

                PredictionsGenerated =
                    results.Count(x =>
                        x.ShouldGeneratePrediction),

                ValidMarketDataPredictions =
                    results.Count(x =>
                        x.ShouldGeneratePrediction &&
                        x.MarketDataAvailable),

                SettledCandidatePredictions =
                    datasetResult.Predictions,

                CorrectPredictions =
                    datasetResult.CorrectPredictions,

                TotalProfit =
                    datasetResult.TotalProfit,

                ROI =
                    datasetResult.ROI,

                Accuracy =
                    datasetResult.Accuracy,

                Predictions =
                    results,

                MaximumDrawdown =
                    datasetResult.MaximumDrawdown,

                MaximumDrawdownPercentage =
                    datasetResult.MaximumDrawdownPercentage,

                RiskPassed =
                    datasetResult.RiskPassed,

                InSample =
                    inSampleResult,

                Validation =
                    validationResult,

                OutOfSample =
                    outOfSampleResult,
                
                Gate =
                    gateResult
            };

        // =========================================
        // FINAL DIAGNOSTICS
        // =========================================

        _logger.LogInformation(
            """
            ========================================
            
            LABORATORY CANDIDATE RUN COMPLETE
            
            Strategy={strategy}
            
            Family={family}
            
            Generation={generation}
            
            MatchesExamined={matches}
            
            MatchesSkipped={skipped}
            
            PredictionsGenerated={generated}
            
            ValidMarketData={marketData}
            
            SettledPredictions={settled}
            
            CorrectPredictions={correct}
            
            Accuracy={accuracy:F2}
            
            Profit={profit:F2}
            
            ROI={roi:F2}

            MaximumDrawdown={drawdown:F2}

            MaximumDrawdownPercentage={drawdownPercent:F2}

            RiskPassed={riskPassed}

            ======================================== 
            """,
            runResult.StrategyVersion,
            runResult.StrategyFamily,
            runResult.Generation,
            runResult.MatchesExamined,
            runResult.MatchesSkipped,
            runResult.PredictionsGenerated,
            runResult.ValidMarketDataPredictions,
            runResult.SettledCandidatePredictions,
            runResult.CorrectPredictions,
            runResult.Accuracy,
            runResult.TotalProfit,
            runResult.ROI,
            runResult.MaximumDrawdown,
            runResult.MaximumDrawdownPercentage,
            runResult.RiskPassed);

        return runResult;
    }


}
