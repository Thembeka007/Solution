using AI.Evolution.Laboratory.Models;

namespace AI.Evolution.Laboratory.Services;

public class MarketEdgeOutcomeAnalyzer
{
    private const int MinimumSampleSize = 20;
    public List<MarketEdgeBucketResult> Analyze(
        IReadOnlyList<HistoricalMarketEdgeResult> results)
    {
        var valid =
            results
                .Where(x =>
                    x.MarketDataStatus ==
                    "Valid")
                .ToList();


        if (!valid.Any())
        {
            return new List<MarketEdgeBucketResult>();
        }

        var buckets =
            valid
                .GroupBy(x =>
                    GetBucket(x.ProbabilityEdge))
                .OrderBy(x =>
                    BucketOrder(x.Key))
                .ToList();

        var output =
            new List<MarketEdgeBucketResult>();

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

            decimal totalProfit =
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
                        totalProfit /
                         totalStake
                      ) * 100m
                    : 0m;

            decimal averageEdge =
                data.Average(x =>
                    x.ProbabilityEdge);

            output.Add(
                new MarketEdgeBucketResult
                {
                    Bucket =
                        bucket.Key,

                    Predictions =
                        predictions,

                    CorrectPredictions =
                        correct,

                    Accuracy =
                        accuracy,

                    TotalStake =
                        totalStake,

                    TotalProfitLoss =
                        totalProfit,

                    ROI =
                        roi,

                    MinimumSampleSizeMet =
                        predictions >= MinimumSampleSize,

                    AverageEdge =
                        averageEdge

                });
        }

        return output;

    }

    private static string GetBucket(decimal edge)
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

    private static int BucketOrder(string bucket)
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
