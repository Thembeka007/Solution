using AI.Evolution.Laboratory.Models;

namespace AI.Evolution.Laboratory.Services;

public class ConfidenceMarketEdgeAnalyzer
{
    public List<ConfidenceMarketEdgeBucketResult>
        Analyze(
            IReadOnlyList<HistoricalMarketEdgeResult> results)
    {
        var valid =
            results
                .Where(x =>
                    x.MarketDataStatus == "Valid")
                .ToList();

        if (!valid.Any())
        {
            return new List<ConfidenceMarketEdgeBucketResult>();
        }

        var buckets =
            valid
                .GroupBy(x =>
                    new
                    {
                        ConfidenceBucket =
                            GetConfidenceBucket(
                                x.StrategyConfidence),
                    
                        EdgeBucket =
                            GetEdgeBucket(
                                x.ProbabilityEdge)
                    })
                .OrderBy(x =>
                    ConfidenceBucketOrder(
                        x.Key.ConfidenceBucket))
                .ThenBy(x =>
                    EdgeBucketOrder(
                        x.Key.EdgeBucket))
                .ToList();

        var output =
            new List<ConfidenceMarketEdgeBucketResult>();


        foreach (var bucket in buckets)
        {
            var data =
                bucket.ToList();

            int predictions =
                data.Count;

            int correct =
                data.Count(x =>
                    x.PredictionCorrect == true);

            decimal totalStake =
                data.Sum(x =>
                    x.Stake);

            decimal totalProfitLoss =
                data.Sum(x =>
                    x.RealisedProfitLoss ?? 0m);

            decimal accuracy =
                predictions > 0
                    ? (
                        (decimal)correct /
                        predictions
                      ) * 100m
                    : 0m;

            decimal roi =
                totalStake > 0m
                    ? (
                        totalProfitLoss /
                        totalStake
                      ) * 100m
                    : 0m;

            decimal averageEdge =
                data.Average(x =>
                    x.ProbabilityEdge);

            decimal averageConfidence =
                data.Average(x =>
                    x.StrategyConfidence);

            output.Add(
                new ConfidenceMarketEdgeBucketResult
                {
                    ConfidenceBucket =
                        bucket.Key.ConfidenceBucket,

                    EdgeBucket =
                        bucket.Key.EdgeBucket,

                    Predictions =
                        predictions,

                    CorrectPredictions =
                        correct,

                    Accuracy =
                        accuracy,

                    TotalStake =
                        totalStake,

                    TotalProfitLoss =
                        totalProfitLoss,

                    ROI =
                        roi,

                    AverageConfidence =
                        averageConfidence,

                    AverageEdge =
                        averageEdge,

                    MinimumSampleSizeMet =
                        predictions >= 20
                });

        }

        return output;
    }

    // =========================================================
    // CONFIDENCE BUCKETS
    // =========================================================

    private static string GetConfidenceBucket(
        decimal confidence)
    {
        if (confidence < 50m)
            return "< 50%";
        
        if (confidence < 60m)
            return "50–60%";
        
        if (confidence < 70m)
            return "60–70%";
        
        if (confidence < 80m)
            return "70–80%";
        
        return "80%+";
    }

    // =========================================================
    // EDGE BUCKETS
    // =========================================================
    private static string GetEdgeBucket(
        decimal edge)
    {
        if (edge < -10m)
            return "< -10pp";
        
        if (edge < 0m)
            return "-10 to 0pp";
        
        if (edge < 5m)
            return "0 to 5pp";
        
        if (edge < 10m)
            return "5 to 10pp";
        
        if (edge < 20m)
            return "10 to 20pp";
        
        if (edge < 30m)
            return "20 to 30pp";
        
        return "30pp+";

    }
    // =========================================================
    // SORT ORDER
    // =========================================================

    private static int ConfidenceBucketOrder(
        string bucket)
    {
        return bucket switch
        {
            "< 50%" => 1,
            "50–60%" => 2,
            "60–70%" => 3,
            "70–80%" => 4,
            "80%+" => 5,
            _ => 99
        };
    }

    private static int EdgeBucketOrder(
        string bucket)
    {
        return bucket switch
        {
            "< -10pp" => 1,
            "-10 to 0pp" => 2,
            "0 to 5pp" => 3,
            "5 to 10pp" => 4,
            "10 to 20pp" => 5,
            "20 to 30pp" => 6,
            "30pp+" => 7,
            _ => 99
        };
    }
}
